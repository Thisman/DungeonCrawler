// Manages scene loading/unloading with additive support and protected-scene handling.
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DungeonCrawler.Systems.SceneManagement
{
    public class SceneLoaderSystem
    {
        private readonly Dictionary<string, Scene> _loadedScenes = new(StringComparer.Ordinal);
        private readonly HashSet<string> _protectedScenes = new(StringComparer.Ordinal);

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

        public AsyncOperation LoadAdditiveScene(string sceneName)
        {
            var loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            if (loadOperation == null)
            {
                Debug.LogWarning($"Failed to start additive load for scene '{sceneName}'.");
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
                ToggleSceneRoot(scene, true);
                DisableUnprotectedScenes(sceneName);
            };

            return loadOperation;
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

        public AsyncOperation UnloadAdditiveScene(string sceneName)
        {
            if (!_loadedScenes.Remove(sceneName))
            {
                Debug.LogWarning($"Attempted to unload additive scene '{sceneName}' that is not tracked.");
            }

            return SceneManager.UnloadSceneAsync(sceneName);
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
}
