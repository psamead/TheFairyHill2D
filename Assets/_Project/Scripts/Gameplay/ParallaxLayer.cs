using UnityEngine;

namespace TheFairyHills.Gameplay
{
    /// <summary>Offsets a background layer according to its documented parallax factor.</summary>
    public sealed class ParallaxLayer : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField, Range(0f, 2f)] private float factor = 1f;
        private float initialX;

        public void Configure(Camera camera, float parallaxFactor)
        {
            targetCamera = camera;
            factor = parallaxFactor;
            initialX = transform.position.x;
        }

        private void LateUpdate()
        {
            if (targetCamera == null) targetCamera = Camera.main;
            if (targetCamera == null) return;
            float leftEdge = targetCamera.transform.position.x - targetCamera.orthographicSize * targetCamera.aspect;
            Vector3 position = transform.position;
            position.x = initialX + leftEdge * (1f - factor);
            transform.position = position;
        }
    }
}
