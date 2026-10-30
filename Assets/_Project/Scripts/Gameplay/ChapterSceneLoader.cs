using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace TheFairyHills.Gameplay
{
    /// <summary>Loads one level additively while keeping the player, camera and HUD in Core.</summary>
    public sealed class ChapterSceneLoader : MonoBehaviour
    {
        [SerializeField] private PlayerController2D player;
        [SerializeField] private ChapterCameraFollow cameraFollow;
        [SerializeField] private ChapterHUD hud;
        private Scene currentLevel;
        private bool loading;

        public void Configure(PlayerController2D corePlayer, ChapterCameraFollow follow, ChapterHUD coreHud)
        {
            player = corePlayer;
            cameraFollow = follow;
            hud = coreHud;
        }

        public void LoadLevel(string sceneName, string spawnId = "Spawn_Start")
        {
            if (loading) return;
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogWarning($"Chapter 1: scene '{sceneName}' is not in Build Settings.", this);
                if (hud != null) hud.ShowMessage($"Scene unavailable: {sceneName}", 3f);
                return;
            }
            StartCoroutine(LoadLevelRoutine(sceneName, spawnId));
        }

        public void ReturnToMenu()
        {
            if (!loading) StartCoroutine(ReturnToMenuRoutine());
        }

        private IEnumerator ReturnToMenuRoutine()
        {
            loading = true;
            EventSystem coreEvents = FindFirstObjectByType<EventSystem>();
            AudioListener coreListener = Camera.main != null ? Camera.main.GetComponent<AudioListener>() : null;
            if (coreEvents != null) coreEvents.enabled = false;
            if (coreListener != null) coreListener.enabled = false;
            AsyncOperation loadMenu = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Additive);
            while (loadMenu != null && !loadMenu.isDone) yield return null;
            Scene menu = SceneManager.GetSceneByName("MainMenu");
            if (menu.IsValid()) SceneManager.SetActiveScene(menu);
            if (currentLevel.IsValid() && currentLevel.isLoaded)
            {
                AsyncOperation unloadLevel = SceneManager.UnloadSceneAsync(currentLevel);
                while (unloadLevel != null && !unloadLevel.isDone) yield return null;
            }
            Scene core = gameObject.scene;
            if (core.IsValid() && core.isLoaded) SceneManager.UnloadSceneAsync(core);
        }

        public void AttachCurrentLevel(Scene level)
        {
            if (level.IsValid() && level.name != "Core_Persistent")
                StartCoroutine(AttachRoutine(level, "Spawn_Start"));
        }

        private IEnumerator LoadLevelRoutine(string sceneName, string spawnId)
        {
            loading = true;
            AsyncOperation load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (load != null && !load.isDone) yield return null;
            Scene next = SceneManager.GetSceneByName(sceneName);
            if (next.IsValid() && next.isLoaded)
            {
                SceneManager.SetActiveScene(next);
                ApplyLevel(next, spawnId);
                if (currentLevel.IsValid() && currentLevel.isLoaded && currentLevel != next)
                {
                    AsyncOperation unload = SceneManager.UnloadSceneAsync(currentLevel);
                    while (unload != null && !unload.isDone) yield return null;
                }
                currentLevel = next;
            }
            loading = false;
        }

        private IEnumerator AttachRoutine(Scene level, string spawnId)
        {
            // The level may be the active scene before Core finishes loading.
            yield return null;
            if (!SceneManager.GetSceneByName("Core_Persistent").isLoaded) yield break;
            ApplyLevel(level, spawnId);
            currentLevel = level;
        }

        private void ApplyLevel(Scene level, string spawnId)
        {
            LevelInfo info = FindInScene<LevelInfo>(level);
            SpawnPoint[] spawns = FindAllInScene<SpawnPoint>(level);
            SpawnPoint spawn = null;
            foreach (SpawnPoint candidate in spawns)
                if (candidate.SpawnId == spawnId) { spawn = candidate; break; }
            if (spawn == null && spawns.Length > 0) spawn = spawns[0];

            if (player != null)
            {
                if (spawn != null)
                {
                    player.transform.position = spawn.transform.position;
                    if (player.Body != null) player.Body.position = spawn.transform.position;
                }
                player.ConfigureLevel(hud, info != null ? info.LevelLength : 38.4f);
            }
            if (cameraFollow != null && player != null)
                cameraFollow.Configure(player.transform, Camera.main, info != null ? info.LevelLength : 38.4f);
        }

        private static T FindInScene<T>(Scene scene) where T : Component
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                T component = root.GetComponentInChildren<T>(true);
                if (component != null) return component;
            }
            return null;
        }

        private static T[] FindAllInScene<T>(Scene scene) where T : Component
        {
            var found = new System.Collections.Generic.List<T>();
            foreach (GameObject root in scene.GetRootGameObjects()) found.AddRange(root.GetComponentsInChildren<T>(true));
            return found.ToArray();
        }

    }
}
