using System.Collections.Generic;
using UnityEngine;

namespace TheFairyHills.Gameplay
{
    /// <summary>Explicit surface metadata on the actual collider, not inferred from its name.</summary>
    public sealed class GroundSurface2D : MonoBehaviour
    {
        public GroundMaterial surface;
        public SurfaceAudioProfile.Region[] regions;
        public bool hayBrush;
        public Collider2D Collider { get; private set; }
        public static readonly List<GroundSurface2D> BrushSurfaces = new List<GroundSurface2D>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSurfaces() => BrushSurfaces.Clear();
        private void Awake() => Collider = GetComponent<Collider2D>();
        public void Configure(SurfaceAudioProfile.Mapping mapping)
        {
            surface = mapping.surface; regions = mapping.regions; hayBrush = mapping.brush;
            if (hayBrush && !BrushSurfaces.Contains(this)) BrushSurfaces.Add(this);
        }
        private void OnEnable() { if (hayBrush && !BrushSurfaces.Contains(this)) BrushSurfaces.Add(this); }
        private void OnDisable() => BrushSurfaces.Remove(this);
        public GroundMaterial At(float worldX)
        {
            if (regions != null) foreach (var region in regions)
                if (worldX >= region.from && worldX < region.to) return region.surface;
            return surface;
        }
    }
}
