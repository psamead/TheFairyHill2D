using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TheFairyHills.Gameplay
{
    /// <summary>Shows a hidden iron cross when Niamh approaches and stores it in persistent session state.</summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class IronCrossPickup : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer crossRenderer;
        [SerializeField] private ParticleSystem hintStars;
        [SerializeField] private SpriteRenderer mainStar;
        [SerializeField] private Light2D crossLight;
        [SerializeField] private float crossLightIntensity = 0.8f;
        [SerializeField] private float revealRadius = 2.5f;
        [SerializeField] private float hintTriggerX = float.NegativeInfinity;
        private PlayerController2D player;
        private bool revealed;
        private bool collected;
        private bool hintStarted;
        private Vector3 mainStarScale;

        public void Configure(SpriteRenderer cross, ParticleSystem stars, float radius)
        {
            Configure(cross, stars, null, radius, float.NegativeInfinity);
        }

        public void Configure(SpriteRenderer cross, ParticleSystem stars, SpriteRenderer star, float radius, float triggerX)
        {
            Configure(cross, stars, star, null, 0.8f, radius, triggerX);
        }

        public void Configure(SpriteRenderer cross, ParticleSystem stars, SpriteRenderer star, Light2D light,
            float lightIntensity, float radius, float triggerX)
        {
            crossRenderer = cross;
            hintStars = stars;
            mainStar = star;
            crossLight = light;
            crossLightIntensity = lightIntensity;
            revealRadius = radius;
            hintTriggerX = triggerX;
        }

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
            if (mainStar != null)
            {
                mainStarScale = mainStar.transform.localScale;
                mainStar.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (collected) return;
            if (player == null) player = FindFirstObjectByType<PlayerController2D>();
            if (player == null) return;

            if (!hintStarted && player.transform.position.x > hintTriggerX)
            {
                hintStarted = true;
                if (crossLight != null)
                {
                    crossLight.enabled = true;
                    crossLight.intensity = crossLightIntensity;
                }
                if (hintStars != null)
                {
                    if (revealed)
                    {
                        ParticleSystem.EmissionModule emission = hintStars.emission;
                        emission.rateOverTime = new ParticleSystem.MinMaxCurve(1f, 1.5f);
                    }
                    hintStars.Play();
                }
                if (mainStar != null) mainStar.gameObject.SetActive(true);
            }

            if (hintStarted && mainStar != null)
            {
                float blink = (Mathf.Sin(Time.time * Mathf.PI * 2f / 0.3f) + 1f) * 0.5f;
                Color color = mainStar.color;
                color.a = Mathf.Lerp(0.22f, 1f, blink);
                mainStar.color = color;
                mainStar.transform.localScale = mainStarScale * Mathf.Lerp(0.8f, 1.08f, blink);
            }

            if (revealed || Vector2.Distance(player.transform.position, transform.position) > revealRadius) return;

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
                emission.rateOverTime = new ParticleSystem.MinMaxCurve(1f, 1.5f);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (collected) return;
            PlayerController2D enteringPlayer = other.GetComponentInParent<PlayerController2D>();
            if (enteringPlayer == null) return;
            collected = true;
            enteringPlayer.CollectIronCross();
            Collider2D trigger = GetComponent<Collider2D>();
            if (trigger != null) trigger.enabled = false;
            if (crossRenderer != null) crossRenderer.enabled = false;
            if (hintStars != null) hintStars.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            if (mainStar != null) mainStar.gameObject.SetActive(false);
            StartCoroutine(FadeLightAndDisable());
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
            gameObject.SetActive(false);
        }
    }
}
