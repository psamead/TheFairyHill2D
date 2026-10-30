using UnityEngine;

namespace TheFairyHills.Gameplay
{
    /// <summary>Runtime settings for a loaded level.</summary>
    public sealed class LevelInfo : MonoBehaviour
    {
        [SerializeField] private string levelName;
        [SerializeField] private float levelLength = 38.4f;
        [SerializeField] private string nextLevelScene;
        public string LevelName => levelName;
        public float LevelLength => levelLength;
        public string NextLevelScene => nextLevelScene;

        public void Configure(string name, float length, string nextScene)
        {
            levelName = name;
            levelLength = length;
            nextLevelScene = nextScene;
        }
    }
}
