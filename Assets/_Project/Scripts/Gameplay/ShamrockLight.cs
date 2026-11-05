using System.Collections;
using UnityEngine;

namespace TheFairyHills.Gameplay
{
    /// <summary>A floating, self-emissive Chapter 1 collectible with a brief fly-in pickup.</summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public sealed class ShamrockLight : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly Color GlowColor = new Color32(207, 240, 160, 255);

        [SerializeField] private SpriteRenderer icon;
        [SerializeField] private SpriteRenderer glow;
        [SerializeField] private CircleCollider2D trigger;
        [SerializeField] private ParticleSystem collectBurst;

        private MaterialPropertyBlock iconProperties;
        private MaterialPropertyBlock glowProperties;
        private Vector3 basePosition;
        private float phase;
        private bool collecting;

        public void Configure(SpriteRenderer iconRenderer, SpriteRenderer glowRenderer, ParticleSystem burst)
        {
            icon = iconRenderer;
            glow = glowRenderer;
            collectBurst = burst;
            trigger = GetComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = 0.3f;
        }

        private void Awake()
        {
            if (trigger == null) trigger = GetComponent<CircleCollider2D>();
            iconProperties = new MaterialPropertyBlock();
            glowProperties = new MaterialPropertyBlock();
            basePosition = transform.position;
            phase = Random.Range(0f, Mathf.PI * 2f);
            ApplyColor(icon, iconProperties, Color.white);
            ApplyGlowAlpha(0.725f);
        }

        private void Update()
        {
            if (collecting) return;

            float cycle = Time.time * Mathf.PI + phase;
            transform.position = basePosition + Vector3.up * (Mathf.Sin(cycle) * 0.08f);
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(cycle) * 8f);
            ApplyGlowAlpha(0.725f + Mathf.Sin(cycle) * 0.125f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (collecting) return;
            PlayerController2D player = other.GetComponentInParent<PlayerController2D>();
            if (player == null) return;

            if (GameState.Instance == null)
            {
                Debug.LogWarning("ShamrockLight touched Niamh before the persistent GameState was available.", this);
                return;
            }

            collecting = true;
            if (trigger != null) trigger.enabled = false;
            StartCoroutine(FlyToPlayer(player));
        }

        private IEnumerator FlyToPlayer(PlayerController2D player)
        {
            Vector3 start = transform.position;
            const float duration = 0.25f;
            float elapsed = 0f;
            while (elapsed < duration && player != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                Vector3 chest = player.transform.TransformPoint(new Vector3(0f, 1.2f, 0f));
                transform.position = Vector3.Lerp(start, chest, t * t * (3f - 2f * t));
                transform.localScale = Vector3.one * (1f - t);
                ApplyGlowAlpha((0.725f + Mathf.Sin(Time.time * Mathf.PI + phase) * 0.125f) * (1f - t));
                yield return null;
            }

            Vector3 burstPosition = player != null
                ? player.transform.TransformPoint(new Vector3(0f, 1.2f, 0f))
                : transform.position;
            GameState.Instance.CollectShamrock();

            if (collectBurst != null)
            {
                collectBurst.transform.SetParent(null, true);
                collectBurst.transform.position = burstPosition;
                collectBurst.transform.localScale = Vector3.one;
                collectBurst.Play(true);
                Destroy(collectBurst.gameObject, 0.9f);
            }

            Destroy(gameObject);
        }

        private void ApplyGlowAlpha(float alpha)
        {
            Color color = GlowColor;
            color.a = alpha;
            ApplyColor(glow, glowProperties, color);
        }

        private static void ApplyColor(SpriteRenderer renderer, MaterialPropertyBlock properties, Color color)
        {
            if (renderer == null) return;
            renderer.GetPropertyBlock(properties);
            properties.SetColor(BaseColorId, color);
            renderer.SetPropertyBlock(properties);
        }
    }
}
