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
        private readonly Dictionary<string, TaskCompletionSource<SceneUnloadResult>> _unloadCompletions = new(StringComparer.Ordinal);
        
        private string _returnSceneName;

        public SceneLoaderSystem(IEnumerable<string> protectedScenes = null)
        {
            if (protectedScenes == null)
            {
                return;
            }

            foreach (var sceneName in protectedScenes.Where(name => !string.IsNullOrWhiteSpace(name)))
            {
                _protectedScenes.Add(sceneName);
            }
        }

        public IReadOnlyDictionary<string, Scene> LoadedScenes => _loadedScenes;

        public ISet<string> ProtectedScenes => _protectedScenes;

        public AdditiveSceneHandle LoadAdditiveScene(string sceneName, string returnSceneName = null)
        {
            _returnSceneName = returnSceneName;
            var unloadCompletion = GetOrCreateUnloadCompletion(sceneName);
            var loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            if (loadOperation == null)
            {
                Debug.LogWarning($"Failed to start additive load for scene '{sceneName}'.");

                unloadCompletion.TrySetException(new InvalidOperationException($"Additive scene '{sceneName}' failed to load."));
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
                ToggleSceneRoot(scene, true);
                DisableUnprotectedScenes(sceneName);
            };

            return new AdditiveSceneHandle(sceneName, unloadCompletion.Task, loadOperation);
        }

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

        public Task<SceneUnloadResult> UnloadAdditiveScene(string sceneName, object unloadData = null)
        {
            var unloadCompletion = GetOrCreateUnloadCompletion(sceneName);
            if (!_loadedScenes.Remove(sceneName))
            {
                Debug.LogWarning($"Attempted to unload additive scene '{sceneName}' that is not tracked.");
            }

            var unloadedSceneName = SceneManager.GetSceneByName(sceneName);
            ToggleSceneRoot(unloadedSceneName, false);
            var unloadOperation = SceneManager.UnloadSceneAsync(sceneName);

            if (unloadOperation == null)
            {
                unloadCompletion.TrySetException(new InvalidOperationException($"Failed to start additive unload for scene '{sceneName}'."));
                return unloadCompletion.Task;
            }

            unloadOperation.completed += _ => CompleteUnload(sceneName, unloadCompletion, unloadData);

            return unloadCompletion.Task;
        }

        public AsyncOperation UnloadScene(string sceneName)
        {
            _loadedScenes.Remove(sceneName);
            return SceneManager.UnloadSceneAsync(sceneName);
        }

        private void DisableUnprotectedScenes(string activeSceneName)
        {
            foreach (var pair in _loadedScenes)
            {
                if (string.Equals(pair.Key, activeSceneName, StringComparison.Ordinal))
                {
                    continue;
                }

                if (_protectedScenes.Contains(pair.Key))
                {
                    continue;
                }

                ToggleSceneRoot(pair.Value, false);
            }
        }

        private TaskCompletionSource<SceneUnloadResult> GetOrCreateUnloadCompletion(string sceneName)
        {
            if (!_unloadCompletions.TryGetValue(sceneName, out var completion))
            {
                completion = new TaskCompletionSource<SceneUnloadResult>(TaskCreationOptions.RunContinuationsAsynchronously);
                _unloadCompletions[sceneName] = completion;
            }

            return completion;
        }

        private void CompleteUnload(string sceneName, TaskCompletionSource<SceneUnloadResult> completion, object unloadData)
        {
            if (_returnSceneName != null)
            {
                var returnScene = SceneManager.GetSceneByName(_returnSceneName);
                ToggleSceneRoot(returnScene, true);
                _returnSceneName = null;
            }

            completion.TrySetResult(new SceneUnloadResult(sceneName, unloadData));
            _unloadCompletions.Remove(sceneName);
        }

        private static void ToggleSceneRoot(Scene scene, bool isActive)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return;
            }

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
        public AdditiveSceneHandle(string sceneName, Task<SceneUnloadResult> unloadTask, AsyncOperation loadOperation)
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
