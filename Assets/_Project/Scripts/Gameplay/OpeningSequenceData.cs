using UnityEngine;

namespace TheFairyHills.Gameplay
{
    [CreateAssetMenu(menuName = "The Fairy Hills/Opening Sequence")]
    public sealed class OpeningSequenceData : ScriptableObject
    {
        [System.Serializable] public sealed class Frame
        {
            public string id;
            public Sprite image;
            [TextArea] public string caption;
            [Min(0.5f)] public float duration = 6;
        }
        public Frame[] frames;
        public Sprite chapterImage;
        public string chapterNumber = "CHAPTER I";
        public string chapterTitle = "THE THRESHOLD";
        public float chapterDuration = 2;
        public float fadeDuration = 0.4f;
        public string nextScene = "Ch1_S1_Village";
        public AudioClip music;
        [Range(0, 1)] public float musicVolume = 0.3f;
    }
}
