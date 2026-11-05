using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TheFairyHills.Gameplay
{
    /// <summary>Runs the IronPulse burn sequence, effects, and barrier removal.</summary>
    public sealed class ThornWall : MonoBehaviour
    {
        private static readonly int GlowBaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int BurnId = Shader.PropertyToID("_Burn");
        private static readonly Color GlowTint = new Color(176f / 255f, 32f / 255f, 42f / 255f, 1f);
        [SerializeField] private SpriteRenderer wallRenderer;
        [SerializeField] private Collider2D blockingCollider;
        [SerializeField] private ParticleSystem smoke;
        [SerializeField] private SpriteRenderer glowRenderer;
        [SerializeField] private Light2D redLight;
        [SerializeField] private Light2D flashLight;
        [SerializeField] private ParticleSystem sparks;
        [SerializeField] private ParticleSystem glowSmoke;
        [SerializeField] private float redLightIntensityMultiplier = 1.2f;
        [SerializeField] private Light2D exitLight;
        [SerializeField] private float exitLightTargetIntensity = 0.7f;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioClip banishClip;
        private MaterialPropertyBlock glowProperties;
        private MaterialPropertyBlock burnProperties;
        private float currentGlowAlpha = 0.5f;
        private float nextFlashTime;
        private float flashEndTime;
        public bool IsDispersed { get; private set; }

        public void Configure(SpriteRenderer renderer, Collider2D collider, ParticleSystem smokeEffect,
            SpriteRenderer glowEffect, Light2D pointLight, float lightIntensityMultiplier = 1.2f)
        {
            Configure(renderer, collider, smokeEffect, glowEffect, pointLight, null, null, null,
                lightIntensityMultiplier);
        }

        public void Configure(SpriteRenderer renderer, Collider2D collider, ParticleSystem smokeEffect,
            SpriteRenderer glowEffect, Light2D pointLight, Light2D orangeFlashLight, ParticleSystem sparkEffect,
            ParticleSystem glowSmokeEffect, float lightIntensityMultiplier = 1.2f,
            Material burnMaterial = null, AudioSource audioSource = null, AudioClip banishSfx = null)
        {
            wallRenderer = renderer;
            blockingCollider = collider;
            smoke = smokeEffect;
            glowRenderer = glowEffect;
            redLight = pointLight;
            flashLight = orangeFlashLight;
            sparks = sparkEffect;
            glowSmoke = glowSmokeEffect;
            redLightIntensityMultiplier = lightIntensityMultiplier;
            sfxSource = audioSource;
            banishClip = banishSfx;
            if (wallRenderer != null && burnMaterial != null)
                wallRenderer.sharedMaterial = burnMaterial;
            burnProperties ??= new MaterialPropertyBlock();
            SetBurn(0f);
            if (glowRenderer != null)
            {
                glowRenderer.color = Color.white;
                currentGlowAlpha = glowRenderer.sharedMaterial != null && glowRenderer.sharedMaterial.HasProperty(GlowBaseColorId)
                    ? glowRenderer.sharedMaterial.GetColor(GlowBaseColorId).a
                    : 0.5f;
            }
            nextFlashTime = Time.time + Random.Range(3f, 6f);
            flashEndTime = 0f;
        }

        public void ConfigureExitLight(Light2D light, float targetIntensity)
        {
            exitLight = light;
            exitLightTargetIntensity = targetIntensity;
            if (exitLight != null)
            {
                exitLight.enabled = false;
                exitLight.intensity = targetIntensity;
            }
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
            float initialGlowAlpha = glowRenderer != null ? currentGlowAlpha : 0f;
            float initialLightIntensity = redLight != null ? redLight.intensity : 0f;
            const float glowDuration = 0.3f;
            float elapsed = 0f;
            while (elapsed < glowDuration)
            {
                elapsed += Time.deltaTime;
                float remaining = 1f - Mathf.Clamp01(elapsed / glowDuration);
                SetGlowColor(initialGlowAlpha * remaining);
                if (redLight != null) redLight.intensity = initialLightIntensity * remaining;
                yield return null;
            }

            SetGlow(0f);
            if (smoke != null) smoke.Play(true);
            if (sfxSource != null && banishClip != null) sfxSource.PlayOneShot(banishClip);
            Coroutine flash = flashLight != null ? StartCoroutine(FadePulseLight()) : null;

            const float dissolveDuration = 1.6f;
            elapsed = 0f;
            while (elapsed < dissolveDuration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / dissolveDuration);
                SetBurn(1f - (1f - progress) * (1f - progress));
                yield return null;
            }
            SetBurn(1f);
            if (wallRenderer != null) wallRenderer.enabled = false;
            if (blockingCollider != null) blockingCollider.enabled = false;
            if (flash != null) yield return flash;
            if (exitLight != null) StartCoroutine(FadeInExitLight());
        }

        private IEnumerator FadePulseLight()
        {
            const float peakIntensity = 3f;
            const float riseDuration = 0.1f;
            const float fadeDuration = 1.2f;
            flashLight.enabled = true;
            flashLight.intensity = 0f;
            float elapsed = 0f;
            while (elapsed < riseDuration)
            {
                elapsed += Time.deltaTime;
                flashLight.intensity = peakIntensity * Mathf.Clamp01(elapsed / riseDuration);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                flashLight.intensity = peakIntensity * (1f - Mathf.Clamp01(elapsed / fadeDuration));
                yield return null;
            }
            flashLight.intensity = 0f;
            flashLight.enabled = false;
        }

        private void SetGlow(float alpha)
        {
            SetGlowColor(alpha);
            if (redLight != null) redLight.intensity = redLightIntensityMultiplier * alpha;
        }

        private IEnumerator FadeInExitLight()
        {
            const float duration = 1f;
            exitLight.enabled = true;
            exitLight.intensity = 0f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                exitLight.intensity = exitLightTargetIntensity * Mathf.Clamp01(elapsed / duration);
                yield return null;
            }
            exitLight.intensity = exitLightTargetIntensity;
        }

        private void SetGlowColor(float alpha)
        {
            if (glowRenderer == null) return;
            if (glowProperties == null) glowProperties = new MaterialPropertyBlock();
            glowRenderer.GetPropertyBlock(glowProperties);
            Color color = GlowTint;
            color.a = alpha;
            glowProperties.SetColor(GlowBaseColorId, color);
            glowRenderer.SetPropertyBlock(glowProperties);
            glowRenderer.color = Color.white;
            currentGlowAlpha = alpha;
        }

        private void SetBurn(float amount)
        {
            if (wallRenderer == null) return;
            burnProperties ??= new MaterialPropertyBlock();
            wallRenderer.GetPropertyBlock(burnProperties);
            burnProperties.SetFloat(BurnId, amount);
            wallRenderer.SetPropertyBlock(burnProperties);
            wallRenderer.color = Color.white;
        }
    }
}
