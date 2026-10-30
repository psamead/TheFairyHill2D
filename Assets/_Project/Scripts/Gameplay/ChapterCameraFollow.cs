using UnityEngine;

namespace TheFairyHills.Gameplay
{
    /// <summary>Horizontal camera follow with the chapter's fixed ground framing.</summary>
    public sealed class ChapterCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private float sceneLength;
        [SerializeField] private float groundCameraY = 2.7f;

        public void Configure(Transform followTarget, Camera camera, float length)
        {
            target = followTarget;
            sceneCamera = camera;
            sceneLength = length;
            groundCameraY = 2.7f;
        }

        public void SetLevelLength(float length) => sceneLength = length;

        private void LateUpdate()
        {
            if (target == null || sceneCamera == null) return;
            float halfWidth = sceneCamera.orthographicSize * sceneCamera.aspect;
            float x = Mathf.Clamp(target.position.x, halfWidth, Mathf.Max(halfWidth, sceneLength - halfWidth));
            transform.position = new Vector3(x, groundCameraY, transform.position.z);
        }
    }
}
