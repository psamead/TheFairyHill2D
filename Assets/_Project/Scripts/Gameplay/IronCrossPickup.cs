using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TheFairyHills.Gameplay
{
    /// <summary>Shows a hidden iron cross when Niamh approaches and stores it in persistent session state.</summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class IronCrossPickup : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        [SerializeField] private SpriteRenderer crossRenderer;
        [SerializeField] private ParticleSystem hintStars;
        [SerializeField] private ParticleSystem spinningHintStars;
        [SerializeField] private SpriteRenderer mainStar;
        [SerializeField] private SpriteRenderer starHalo;
        [SerializeField] private Light2D crossLight;
        [SerializeField] private float crossLightIntensity = 0.8f;
        [SerializeField] private float revealRadius = 2.5f;
        [SerializeField] private float hintTriggerX = float.NegativeInfinity;
        [SerializeField] private AudioSource pickupAudioSource;
        [SerializeField] private AudioClip pickupClip;
        [SerializeField] private AudioSource levelMusicSource;
        private PlayerController2D player;
        private bool revealed;
        private bool collected;
        private bool hintStarted;
        private Vector3 mainStarScale;
        private Vector3 haloScale;
        private Quaternion mainStarRotation;
        private float mainStarPulseStartTime;
        private MaterialPropertyBlock mainStarProperties;
        private MaterialPropertyBlock haloProperties;
        private Coroutine musicDuckRoutine;

        public void Configure(SpriteRenderer cross, ParticleSystem stars, float radius)
        {
            Configure(cross, stars, null, radius, float.NegativeInfinity);
        }

        public void Configure(SpriteRenderer cross, ParticleSystem stars, SpriteRenderer star, float radius, float triggerX)
        {
            Configure(cross, stars, null, star, null, null, 0.8f, radius, triggerX);
        }

        public void Configure(SpriteRenderer cross, ParticleSystem stars, SpriteRenderer star, Light2D light,
            float lightIntensity, float radius, float triggerX)
        {
            Configure(cross, stars, null, star, null, light, lightIntensity, radius, triggerX);
        }

        public void Configure(SpriteRenderer cross, ParticleSystem stars, ParticleSystem spinningStars,
            SpriteRenderer star, SpriteRenderer halo, Light2D light, float lightIntensity, float radius, float triggerX)
        {
            Configure(cross, stars, spinningStars, star, halo, light, lightIntensity, radius, triggerX,
                null, null, null);
        }

        public void Configure(SpriteRenderer cross, ParticleSystem stars, ParticleSystem spinningStars,
            SpriteRenderer star, SpriteRenderer halo, Light2D light, float lightIntensity, float radius, float triggerX,
            AudioSource pickupSource, AudioClip pickupSfx, AudioSource musicSource)
        {
            crossRenderer = cross;
            hintStars = stars;
            spinningHintStars = spinningStars;
            mainStar = star;
            starHalo = halo;
            crossLight = light;
            crossLightIntensity = lightIntensity;
            revealRadius = radius;
            hintTriggerX = triggerX;
            pickupAudioSource = pickupSource;
            pickupClip = pickupSfx;
            levelMusicSource = musicSource;
            if (pickupAudioSource != null)
            {
                pickupAudioSource.playOnAwake = false;
                pickupAudioSource.loop = false;
                pickupAudioSource.spatialBlend = 0f;
                pickupAudioSource.volume = 0.7f;
            }
            if (levelMusicSource != null) musicBaseVolume = levelMusicSource.volume;
        }

        private float musicBaseVolume = 0.5f;

        private void Start()
        {
            collected = GameState.Instance != null && GameState.Instance.HasIronCross;
            if (collected)
            {
                gameObject.SetActive(false);
                return;
            }
            if (crossLight != null)
            {
                crossLight.enabled = false;
                crossLight.intensity = crossLightIntensity;
            }
            if (crossRenderer != null)
            {
                Color color = crossRenderer.color;
                color.a = 0f;
                crossRenderer.color = color;
            }
            if (hintStars != null) hintStars.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (spinningHintStars != null) spinningHintStars.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (mainStar != null)
            {
                mainStarScale = mainStar.transform.localScale;
                mainStarRotation = mainStar.transform.localRotation;
                mainStar.gameObject.SetActive(false);
            }
            if (starHalo != null)
            {
                haloScale = starHalo.transform.localScale;
                starHalo.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (collected) return;
            if (player == null) player = FindFirstObjectByType<PlayerController2D>();
            if (player != null && !hintStarted && player.transform.position.x > hintTriggerX)
            {
                hintStarted = true;
                if (crossLight != null)
                {
                    crossLight.enabled = true;
                    crossLight.intensity = crossLightIntensity;
                }
                if (hintStars != null)
                {
                    hintStars.Play();
                }
                if (spinningHintStars != null) spinningHintStars.Play();
                mainStarPulseStartTime = Time.time;
                if (mainStar != null) mainStar.gameObject.SetActive(true);
                if (starHalo != null) starHalo.gameObject.SetActive(true);
            }

            if (hintStarted) UpdateMainStar();

            if (player == null || revealed || Vector2.Distance(player.transform.position, transform.position) > revealRadius) return;

            revealed = true;
            if (crossRenderer != null)
            {
                Color color = crossRenderer.color;
                color.a = 1f;
                crossRenderer.color = color;
            }
            if (hintStars != null)
            {
                ParticleSystem.EmissionModule emission = hintStars.emission;
                emission.rateOverTime = new ParticleSystem.MinMaxCurve(0.5f, 0.75f);
            }
        }

        private void UpdateMainStar()
        {
            if (mainStar == null) return;

            float elapsed = Mathf.Max(0f, Time.time - mainStarPulseStartTime);
            int breathIndex = Mathf.FloorToInt(elapsed / 1.2f);
            float phase = Mathf.Repeat(elapsed, 1.2f) / 1.2f;
            float inhale = Mathf.SmoothStep(0f, 1f, phase < 0.5f ? phase * 2f : 1f);
            float exhale = phase < 0.5f ? inhale : 1f - Mathf.SmoothStep(0f, 1f, (phase - 0.5f) * 2f);
            float breath = phase < 0.5f ? inhale : exhale;
            float flash = (breathIndex + 1) % 3 == 0 && phase >= 0.5f && phase < 0.625f
                ? Mathf.Sin(Mathf.PI * (phase - 0.5f) / 0.125f)
                : 0f;
            mainStar.transform.localScale = mainStarScale * Mathf.Lerp(0.85f, 1.15f, breath) * Mathf.Lerp(1f, 1.5f, flash);
            float angle = (breathIndex + inhale) * 45f;
            mainStar.transform.localRotation = mainStarRotation * Quaternion.Euler(0f, 0f, angle);
            SetAdditiveColor(mainStar, new Color(1f, 224f / 255f, 157f / 255f, Mathf.Lerp(0.6f, 1f, breath)),
                ref mainStarProperties);

            if (starHalo == null) return;
            starHalo.transform.localScale = haloScale * Mathf.Lerp(0.95f, 1.05f, breath) * Mathf.Lerp(1f, 1.5f, flash);
            SetAdditiveColor(starHalo, new Color(242f / 255f, 210f / 255f, 138f / 255f,
                Mathf.Lerp(0.2f, 0.45f, breath) + (0.8f - Mathf.Lerp(0.2f, 0.45f, breath)) * flash), ref haloProperties);
        }

        private static void SetAdditiveColor(SpriteRenderer renderer, Color color, ref MaterialPropertyBlock properties)
        {
            if (renderer == null) return;
            properties ??= new MaterialPropertyBlock();
            renderer.GetPropertyBlock(properties);
            properties.SetColor(BaseColorId, color);
            renderer.SetPropertyBlock(properties);
            renderer.color = Color.white;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (collected) return;
            PlayerController2D enteringPlayer = other.GetComponentInParent<PlayerController2D>();
            if (enteringPlayer == null) return;
            collected = true;
            enteringPlayer.CollectIronCross();
            if (pickupAudioSource != null && pickupClip != null) pickupAudioSource.PlayOneShot(pickupClip);
            if (levelMusicSource != null) musicDuckRoutine = StartCoroutine(DuckMusicForPickup());
            Collider2D trigger = GetComponent<Collider2D>();
            if (trigger != null) trigger.enabled = false;
            if (crossRenderer != null) crossRenderer.enabled = false;
            if (hintStars != null) hintStars.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (spinningHintStars != null) spinningHintStars.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (mainStar != null) mainStar.gameObject.SetActive(false);
            if (starHalo != null) starHalo.gameObject.SetActive(false);
            StartCoroutine(FadeLightAndDisable());
        }

        private System.Collections.IEnumerator DuckMusicForPickup()
        {
            levelMusicSource.volume = musicBaseVolume * 0.5f;
            yield return new WaitForSecondsRealtime(0.5f);
            if (levelMusicSource != null) levelMusicSource.volume = musicBaseVolume;
            musicDuckRoutine = null;
        }

        private System.Collections.IEnumerator FadeLightAndDisable()
        {
            const float duration = 0.5f;
            float startIntensity = crossLight != null ? crossLight.intensity : 0f;
            if (crossLight != null) crossLight.enabled = true;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                if (crossLight != null)
                    crossLight.intensity = startIntensity * (1f - Mathf.Clamp01(elapsed / duration));
                yield return null;
            }
            if (crossLight != null) crossLight.enabled = false;
            if (musicDuckRoutine != null) yield return musicDuckRoutine;
            gameObject.SetActive(false);
        }
    }
}
