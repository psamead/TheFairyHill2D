using UnityEngine;

namespace TheFairyHills.Gameplay
{
    /// <summary>Shows a hidden iron cross when Niamh approaches and stores it in persistent session state.</summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class IronCrossPickup : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer crossRenderer;
        [SerializeField] private ParticleSystem hintStars;
        [SerializeField] private float revealRadius = 2.5f;
        private PlayerController2D player;
        private bool revealed;
        private bool collected;

        public void Configure(SpriteRenderer cross, ParticleSystem stars, float radius)
        {
            crossRenderer = cross;
            hintStars = stars;
            revealRadius = radius;
        }

        private void Start()
        {
            collected = GameState.Instance != null && GameState.Instance.HasIronCross;
            if (collected)
            {
                gameObject.SetActive(false);
                return;
            }
            if (crossRenderer != null)
            {
                Color color = crossRenderer.color;
                color.a = 0f;
                crossRenderer.color = color;
            }
            if (hintStars != null && !hintStars.isPlaying) hintStars.Play();
        }

        private void Update()
        {
            if (collected) return;
            if (player == null) player = FindFirstObjectByType<PlayerController2D>();
            if (player == null || revealed) return;
            if (Vector2.Distance(player.transform.position, transform.position) > revealRadius) return;

            revealed = true;
            if (crossRenderer != null)
            {
                Color color = crossRenderer.color;
                color.a = 1f;
                crossRenderer.color = color;
            }
            if (hintStars != null) hintStars.Stop();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (collected) return;
            PlayerController2D enteringPlayer = other.GetComponentInParent<PlayerController2D>();
            if (enteringPlayer == null) return;
            collected = true;
            enteringPlayer.CollectIronCross();
            gameObject.SetActive(false);
        }
    }
}
