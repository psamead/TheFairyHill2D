using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

namespace TheFairyHills.Gameplay
{
    /// <summary>Loads one level additively while keeping the player, camera and HUD in Core.</summary>
    public sealed class ChapterSceneLoader : MonoBehaviour
    {
        [SerializeField] private PlayerController2D player;
        [SerializeField] private ChapterCameraFollow cameraFollow;
        [SerializeField] private ChapterHUD hud;
        [SerializeField] private Light2D globalLight;
        [SerializeField] private Light2D playerLight;
        private Scene currentLevel;
        private bool loading;
        private float playerLightFadeStart;
        private bool fadingPlayerLight;
        private const float PlayerLightIntensity = 1.2f;

        public void Configure(PlayerController2D corePlayer, ChapterCameraFollow follow, ChapterHUD coreHud,
            Light2D coreGlobalLight, Light2D corePlayerLight)
        {
            player = corePlayer;
            cameraFollow = follow;
            hud = coreHud;
            globalLight = coreGlobalLight;
            playerLight = corePlayerLight;
        }

        private void Start()
        {
            // When a level is opened directly in the Editor, the Core scene loads additively.
            // Apply that active level's lighting as soon as this persistent loader starts.
            Scene active = SceneManager.GetActiveScene();
            if (active.IsValid() && active != gameObject.scene && FindInScene<LevelInfo>(active) != null)
            {
                currentLevel = active;
                ApplyLevel(active, "Spawn_Start");
            }
        }

        private void Update()
        {
            if (playerLight == null || !playerLight.enabled) return;
            float fade = 1f;
            if (fadingPlayerLight)
            {
                fade = Mathf.Clamp01((Time.unscaledTime - playerLightFadeStart) / 1f);
                if (fade >= 1f) fadingPlayerLight = false;
            }
            float breathe = 1f + 0.04f * Mathf.Sin(Time.unscaledTime * (Mathf.PI * 2f / 3f));
            playerLight.intensity = PlayerLightIntensity * fade * breathe;
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
            if (level.IsValid() && level.name != "Core_Persistent" && level != currentLevel)
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
            ApplyLevelLighting(info);
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

        public void ApplyLevelLighting(LevelInfo info)
        {
            if (globalLight != null)
            {
                globalLight.color = info != null ? info.AmbientColor : Color.white;
                globalLight.intensity = info != null ? info.AmbientIntensity : 1f;
                globalLight.enabled = true;
            }

            if (playerLight != null)
            {
                bool enabledForLevel = info != null && info.PlayerLight;
                playerLight.enabled = enabledForLevel;
                fadingPlayerLight = enabledForLevel;
                playerLightFadeStart = Time.unscaledTime;
                playerLight.intensity = enabledForLevel ? 0f : PlayerLightIntensity;
            }
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
