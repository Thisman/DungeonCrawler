// Manages scene loading/unloading with additive support, protected-scene handling, and unload notifications.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DungeonCrawler.Systems.SceneManagement
{
    public class SceneLoaderSystem
    {
        private readonly Dictionary<string, Scene> _loadedScenes = new(StringComparer.Ordinal);
        private readonly HashSet<string> _protectedScenes = new(StringComparer.Ordinal);

        // Для асинхронного ожидания выгрузки сцен.
        private readonly Dictionary<string, TaskCompletionSource<SceneUnloadResult>> _unloadCompletions =
            new(StringComparer.Ordinal);

        // Для каждой загруженной аддитивно сцены запоминаем, какую сцену нужно "вернуть" (включить) после её выгрузки.
        private readonly Dictionary<string, string> _returnScenesByScene = new(StringComparer.Ordinal);

        public SceneLoaderSystem(IEnumerable<string> protectedScenes = null)
        {
            _protectedScenes.Add("Root");
            if (protectedScenes == null)
                return;

            foreach (var sceneName in protectedScenes.Where(name => !string.IsNullOrWhiteSpace(name)))
            {
                _protectedScenes.Add(sceneName);
            }
        }

        /// <summary>
        /// Поддерживаемые сценой состояния из кода.
        /// </summary>
        public IReadOnlyDictionary<string, Scene> LoadedScenes => _loadedScenes;

        /// <summary>
        /// Сцены, которые не должны отключаться при загрузке других (root/persistent).
        /// ВАЖНО: сюда нужно добавить имя корневой сцены, где живут системные сервисы (инпут и т.п.).
        /// </summary>
        public ISet<string> ProtectedScenes => _protectedScenes;

        /// <summary>
        /// Загружает сцену аддитивно. При этом:
        /// - Включает Root-объект новой сцены (если он есть).
        /// - Отключает Root-объекты всех незашищённых уже загруженных сцен.
        /// - Запоминает сцену, к которой нужно вернуться (returnSceneName) после выгрузки этой аддитивной.
        /// </summary>
        public AdditiveSceneHandle LoadAdditiveScene(string sceneName, string returnSceneName = null)
        {
            if (!string.IsNullOrWhiteSpace(returnSceneName))
            {
                _returnScenesByScene[sceneName] = returnSceneName;
            }

            var unloadCompletion = GetOrCreateUnloadCompletion(sceneName);

            var loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (loadOperation == null)
            {
                Debug.LogWarning($"Failed to start additive load for scene '{sceneName}'.");
                unloadCompletion.TrySetException(
                    new InvalidOperationException($"Additive scene '{sceneName}' failed to load."));

                return new AdditiveSceneHandle(sceneName, unloadCompletion.Task, null);
            }

            loadOperation.completed += _ =>
            {
                var scene = SceneManager.GetSceneByName(sceneName);
                if (!scene.IsValid())
                {
                    Debug.LogWarning($"Scene '{sceneName}' is not valid after loading.");
                    return;
                }

                _loadedScenes[sceneName] = scene;

                // Включаем Root новой сцены.
                ToggleSceneRoot(scene, true);

                // Отключаем все незашищённые сцены (например, Dungeon при входе в Battle).
                DisableUnprotectedScenes(sceneName);
            };

            return new AdditiveSceneHandle(sceneName, unloadCompletion.Task, loadOperation);
        }

        /// <summary>
        /// Загружает сцену в режиме Single. Полностью очищает трекинг загруженных сцен.
        /// </summary>
        public AsyncOperation LoadScene(string sceneName)
        {
            _loadedScenes.Clear();

            var loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (loadOperation == null)
            {
                Debug.LogWarning($"Failed to start load for scene '{sceneName}'.");
                return null;
            }

            loadOperation.completed += _ =>
            {
                var scene = SceneManager.GetSceneByName(sceneName);
                if (!scene.IsValid())
                {
                    Debug.LogWarning($"Scene '{sceneName}' is not valid after loading.");
                    return;
                }

                _loadedScenes[sceneName] = scene;
            };

            return loadOperation;
        }

        /// <summary>
        /// Выгружает аддитивно загруженную сцену и возвращает Task, который завершится после выгрузки.
        /// </summary>
        public Task<SceneUnloadResult> UnloadAdditiveScene(string sceneName, object unloadData = null)
        {
            var unloadCompletion = GetOrCreateUnloadCompletion(sceneName);

            if (!_loadedScenes.Remove(sceneName))
            {
                Debug.LogWarning($"Attempted to unload additive scene '{sceneName}' that is not tracked.");
            }

            var scene = SceneManager.GetSceneByName(sceneName);
            // Перед выгрузкой выключаем Root (чтобы не светились объекты во время UnloadSceneAsync).
            ToggleSceneRoot(scene, false);

            var unloadOperation = SceneManager.UnloadSceneAsync(sceneName);
            if (unloadOperation == null)
            {
                unloadCompletion.TrySetException(
                    new InvalidOperationException($"Failed to start additive unload for scene '{sceneName}'."));
                return unloadCompletion.Task;
            }

            unloadOperation.completed += _ => CompleteUnload(sceneName, unloadCompletion, unloadData);

            return unloadCompletion.Task;
        }

        /// <summary>
        /// Вспомогательный метод для выгрузки неаддитивных сцен (если нужен).
        /// </summary>
        public AsyncOperation UnloadScene(string sceneName)
        {
            _loadedScenes.Remove(sceneName);
            return SceneManager.UnloadSceneAsync(sceneName);
        }

        /// <summary>
        /// Отключает Root-объекты всех незашищённых сцен, кроме текущей activeSceneName.
        /// </summary>
        private void DisableUnprotectedScenes(string activeSceneName)
        {
            foreach (var pair in _loadedScenes)
            {
                if (string.Equals(pair.Key, activeSceneName, StringComparison.Ordinal))
                    continue;

                // Защищённые сцены (например, root с инпутом) не трогаем.
                if (_protectedScenes.Contains(pair.Key))
                    continue;

                ToggleSceneRoot(pair.Value, false);
            }
        }

        /// <summary>
        /// Создаёт или возвращает уже существующий TaskCompletionSource для сцены.
        /// БЕЗ RunContinuationsAsynchronously, чтобы продолжения по умолчанию шли в контексте Unity main thread.
        /// </summary>
        private TaskCompletionSource<SceneUnloadResult> GetOrCreateUnloadCompletion(string sceneName)
        {
            if (!_unloadCompletions.TryGetValue(sceneName, out var completion))
            {
                completion = new TaskCompletionSource<SceneUnloadResult>();
                _unloadCompletions[sceneName] = completion;
            }

            return completion;
        }

        private void CompleteUnload(
            string sceneName,
            TaskCompletionSource<SceneUnloadResult> completion,
            object unloadData)
        {
            // Возвращаемся в сцену, которую запомнили при загрузке этой аддитивной.
            if (_returnScenesByScene.TryGetValue(sceneName, out var returnSceneName))
            {
                var returnScene = SceneManager.GetSceneByName(returnSceneName);
                ToggleSceneRoot(returnScene, true);

                _returnScenesByScene.Remove(sceneName);
            }

            completion.TrySetResult(new SceneUnloadResult(sceneName, unloadData));
            _unloadCompletions.Remove(sceneName);
        }

        /// <summary>
        /// Включает/выключает Root-объект сцены (объект с именем "Root", если он есть).
        /// </summary>
        private static void ToggleSceneRoot(Scene scene, bool isActive)
        {
            if (!scene.IsValid() || !scene.isLoaded)
                return;

            foreach (var rootObject in scene.GetRootGameObjects())
            {
                if (string.Equals(rootObject.name, "Root", StringComparison.Ordinal))
                {
                    rootObject.SetActive(isActive);
                    break;
                }
            }
        }
    }

    public readonly struct SceneUnloadResult
    {
        public SceneUnloadResult(string sceneName, object data)
        {
            SceneName = sceneName;
            Data = data;
        }

        public string SceneName { get; }
        public object Data { get; }
    }

    public class AdditiveSceneHandle
    {
        public AdditiveSceneHandle(
            string sceneName,
            Task<SceneUnloadResult> unloadTask,
            AsyncOperation loadOperation)
        {
            SceneName = sceneName;
            WhenUnloaded = unloadTask;
            LoadOperation = loadOperation;
        }

        public string SceneName { get; }
        public AsyncOperation LoadOperation { get; }
        public Task<SceneUnloadResult> WhenUnloaded { get; }
    }
}
