using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TheFairyHills.Gameplay
{
    /// <summary>IronPulse placeholder: dark smoke, short fade, then remove the solid barrier.</summary>
    public sealed class ThornWall : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer wallRenderer;
        [SerializeField] private Collider2D blockingCollider;
        [SerializeField] private ParticleSystem smoke;
        [SerializeField] private SpriteRenderer glowRenderer;
        [SerializeField] private Light2D redLight;
        private float nextFlashTime;
        private float flashEndTime;
        public bool IsDispersed { get; private set; }

        public void Configure(SpriteRenderer renderer, Collider2D collider, ParticleSystem smokeEffect,
            SpriteRenderer glowEffect, Light2D pointLight)
        {
            wallRenderer = renderer;
            blockingCollider = collider;
            smoke = smokeEffect;
            glowRenderer = glowEffect;
            redLight = pointLight;
            nextFlashTime = Time.time + Random.Range(3f, 6f);
            flashEndTime = 0f;
        }

        private void Update()
        {
            if (IsDispersed || glowRenderer == null) return;

            if (Time.time >= nextFlashTime)
            {
                flashEndTime = Time.time + Random.Range(0.1f, 1f);
                nextFlashTime = Time.time + Random.Range(3f, 6f);
            }

            float pulse = (Mathf.Sin(Time.time * (Mathf.PI * 2f / 2.5f)) + 1f) * 0.5f;
            float alpha = Mathf.Lerp(0.35f, 0.7f, pulse);
            if (Time.time < flashEndTime) alpha = 1f;
            SetGlow(alpha);
        }

        public void Disperse()
        {
            if (IsDispersed) return;
            IsDispersed = true;
            StartCoroutine(FadeRoutine());
        }

        private IEnumerator FadeRoutine()
        {
            float initialGlowAlpha = glowRenderer != null ? glowRenderer.color.a : 0f;
            float initialLightIntensity = redLight != null ? redLight.intensity : 0f;
            const float glowDuration = 0.3f;
            float elapsed = 0f;
            while (elapsed < glowDuration)
            {
                elapsed += Time.deltaTime;
                float remaining = 1f - Mathf.Clamp01(elapsed / glowDuration);
                if (glowRenderer != null)
                {
                    Color color = glowRenderer.color;
                    color.a = initialGlowAlpha * remaining;
                    glowRenderer.color = color;
                }
                if (redLight != null) redLight.intensity = initialLightIntensity * remaining;
                yield return null;
            }

            SetGlow(0f);
            if (smoke != null) smoke.Play();

            Color original = wallRenderer != null ? wallRenderer.color : Color.white;
            const float duration = 0.6f;
            elapsed = 0f;
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

        private void SetGlow(float alpha)
        {
            if (glowRenderer != null)
            {
                Color color = glowRenderer.color;
                color.a = alpha;
                glowRenderer.color = color;
            }
            if (redLight != null) redLight.intensity = 0.45f * alpha;
        }
    }
}
