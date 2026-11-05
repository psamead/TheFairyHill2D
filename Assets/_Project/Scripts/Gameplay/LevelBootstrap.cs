using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheFairyHills.Gameplay
{
    /// <summary>Lets a chapter scene opened by itself in the Editor load its shared Core scene.</summary>
    public sealed class LevelBootstrap : MonoBehaviour
    {
        private IEnumerator Start()
        {
            Scene level = gameObject.scene;
            bool loadedCoreHere = false;
            Scene core = SceneManager.GetSceneByName("Core_Persistent");
            if (!core.isLoaded)
            {
                if (!Application.CanStreamedLevelBeLoaded("Core_Persistent"))
                {
                    Debug.LogError("LevelBootstrap: Core_Persistent is missing from Build Settings.", this);
                    yield break;
                }

                AsyncOperation load = SceneManager.LoadSceneAsync("Core_Persistent", LoadSceneMode.Additive);
                while (load != null && !load.isDone) yield return null;
                core = SceneManager.GetSceneByName("Core_Persistent");
                loadedCoreHere = true;
            }

            if (!core.isLoaded || !EnableCoreAudioListener(core)) yield break;
            ChapterSceneLoader loader = FindFirstObjectByType<ChapterSceneLoader>();
            if (loadedCoreHere && loader != null) loader.AttachCurrentLevel(level);

            AudioSource levelMusic = FindLevelMusic(level);
            if (levelMusic != null && !levelMusic.isPlaying) levelMusic.Play();
        }

        private bool EnableCoreAudioListener(Scene core)
        {
            AudioListener coreListener = null;
            int count = 0;
            foreach (GameObject root in core.GetRootGameObjects())
            {
                foreach (AudioListener listener in root.GetComponentsInChildren<AudioListener>(true))
                {
                    coreListener = listener;
                    count++;
                }
            }

            if (count != 1)
            {
                Debug.LogError($"LevelBootstrap: expected one Core AudioListener, found {count}.", this);
                return false;
            }
            if (!coreListener.gameObject.activeInHierarchy)
            {
                Debug.LogError("LevelBootstrap: Core AudioListener GameObject is inactive.", coreListener);
                return false;
            }
            if (!coreListener.enabled) coreListener.enabled = true;
            return true;
        }

        private static AudioSource FindLevelMusic(Scene level)
        {
            foreach (GameObject root in level.GetRootGameObjects())
            {
                foreach (AudioSource source in root.GetComponentsInChildren<AudioSource>(true))
                    if (source.gameObject.name == "Level Music") return source;
            }
            return null;
        }
    }
}
