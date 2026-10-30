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
            if (SceneManager.GetSceneByName("Core_Persistent").isLoaded) yield break;
            if (!Application.CanStreamedLevelBeLoaded("Core_Persistent"))
            {
                Debug.LogError("LevelBootstrap: Core_Persistent is missing from Build Settings.", this);
                yield break;
            }

            AsyncOperation load = SceneManager.LoadSceneAsync("Core_Persistent", LoadSceneMode.Additive);
            while (load != null && !load.isDone) yield return null;
            ChapterSceneLoader loader = FindFirstObjectByType<ChapterSceneLoader>();
            if (loader != null) loader.AttachCurrentLevel(gameObject.scene);
        }
    }
}
