using UnityEngine;

namespace TheFairyHills.Gameplay
{
    /// <summary>Runtime settings for a loaded level.</summary>
    public sealed class LevelInfo : MonoBehaviour
    {
        [SerializeField] private string levelName;
        [SerializeField] private float levelLength = 38.4f;
        [SerializeField] private string nextLevelScene;
        [SerializeField] private Color ambientColor = Color.white;
        [SerializeField] private float ambientIntensity = 1f;
        [SerializeField] private bool playerLight;
        public string LevelName => levelName;
        public float LevelLength => levelLength;
        public string NextLevelScene => nextLevelScene;
        public Color AmbientColor => ambientColor;
        public float AmbientIntensity => ambientIntensity;
        public bool PlayerLight => playerLight;

        private void Awake()
        {
            // Apply as soon as an additive level activates so its first rendered frame
            // cannot inherit the previous level's lighting.
            ChapterSceneLoader loader = FindFirstObjectByType<ChapterSceneLoader>();
            if (loader != null) loader.ApplyLevelLighting(this);
        }

        public void Configure(string name, float length, string nextScene, Color levelAmbientColor,
            float levelAmbientIntensity, bool enablePlayerLight)
        {
            levelName = name;
            levelLength = length;
            nextLevelScene = nextScene;
            ambientColor = levelAmbientColor;
            ambientIntensity = levelAmbientIntensity;
            playerLight = enablePlayerLight;
        }
    }
}
