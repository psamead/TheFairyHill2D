using System.Collections;
using UnityEngine;

namespace TheFairyHills.Gameplay
{
    /// <summary>IronPulse placeholder: dark smoke, short fade, then remove the solid barrier.</summary>
    public sealed class ThornWall : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer wallRenderer;
        [SerializeField] private Collider2D blockingCollider;
        [SerializeField] private ParticleSystem smoke;
        public bool IsDispersed { get; private set; }

        public void Configure(SpriteRenderer renderer, Collider2D collider, ParticleSystem smokeEffect)
        {
            wallRenderer = renderer;
            blockingCollider = collider;
            smoke = smokeEffect;
        }

        public void Disperse()
        {
            if (IsDispersed) return;
            IsDispersed = true;
            if (smoke != null) smoke.Play();
            StartCoroutine(FadeRoutine());
        }

        private IEnumerator FadeRoutine()
        {
            Color original = wallRenderer != null ? wallRenderer.color : Color.white;
            const float duration = 0.75f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                if (wallRenderer != null)
                {
                    Color color = original;
                    color.a = 1f - Mathf.Clamp01(elapsed / duration);
                    wallRenderer.color = color;
                }
                yield return null;
            }

            if (wallRenderer != null)
            {
                Color color = original;
                color.a = 0f;
                wallRenderer.color = color;
            }
            if (blockingCollider != null) blockingCollider.enabled = false;
        }
    }
}
