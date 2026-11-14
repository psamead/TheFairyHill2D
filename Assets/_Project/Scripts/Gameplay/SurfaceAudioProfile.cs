using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheFairyHills.Gameplay
{
    public enum GroundMaterial { Dirt, Grass, Stone, Hay, Wood }

    [CreateAssetMenu(menuName = "The Fairy Hills/Surface Audio Profile")]
    public sealed class SurfaceAudioProfile : ScriptableObject
    {
        [Serializable] public sealed class StepSet { public GroundMaterial surface; public AudioClip[] clips; }
        [Serializable] public sealed class Region { public float from, to; public GroundMaterial surface; }
        [Serializable] public sealed class Mapping
        {
            public string scene, path, description;
            public GroundMaterial surface;
            public bool brush;
            public Region[] regions;
        }
        [Serializable] public sealed class MapFile { public Mapping[] mappings; }
        public StepSet[] steps;
        public AudioClip[] jump, lightLand, heavyLand, hayBrush;
        public Mapping[] mappings;
        private static SurfaceAudioProfile cached;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCache() => cached = null;

        public static void ApplyLevel(Scene scene)
        {
            if (cached == null) cached = Resources.Load<SurfaceAudioProfile>("NiamhSurfaceAudio_v1");
            if (cached == null) return;
            foreach (Mapping mapping in cached.mappings)
            {
                if (mapping.scene != scene.name) continue;
                string[] path = mapping.path.Split(new[] { '/' }, 2);
                Transform target = null;
                foreach (GameObject root in scene.GetRootGameObjects())
                    if (root.name == path[0]) { target = path.Length == 1 ? root.transform : root.transform.Find(path[1]); break; }
                if (target == null || target.GetComponent<Collider2D>() == null)
                { Debug.LogWarning("Surface audio mapping has no collider: " + scene.name + "/" + mapping.path); continue; }
                var marker = target.GetComponent<GroundSurface2D>();
                if (marker == null) marker = target.gameObject.AddComponent<GroundSurface2D>();
                marker.Configure(mapping);
            }
        }
    }
}
