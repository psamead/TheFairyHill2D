using UnityEngine;

namespace TheFairyHills.Gameplay
{
    /// <summary>Six-part art trial. Reads the existing player; never drives gameplay or physics.</summary>
    public sealed class NiamhMotionV8Presentation : MonoBehaviour
    {
        [SerializeField] private PlayerController2D player;
        [SerializeField] private SpriteRenderer staticVisual;
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer[] parts;
        [SerializeField] private float landingDuration = 0.30f;

        private static readonly int Idle = Animator.StringToHash("Base Layer.Idle");
        private static readonly int Walk = Animator.StringToHash("Base Layer.Walk");
        private static readonly int Run = Animator.StringToHash("Base Layer.Run");
        private static readonly int JumpUp = Animator.StringToHash("Base Layer.JumpUp");
        private static readonly int Apex = Animator.StringToHash("Base Layer.Apex");
        private static readonly int Fall = Animator.StringToHash("Base Layer.Fall");
        private static readonly int Land = Animator.StringToHash("Base Layer.Land");
        private int currentState;
        private bool wasGrounded;
        private bool observedAirborne;
        private float landingRemaining;
        private Vector3 authoredScale;
        private bool staticWasEnabled;

        public void Configure(PlayerController2D controller, SpriteRenderer rollbackVisual)
        {
            player = controller;
            staticVisual = rollbackVisual;
        }

        private void Awake()
        {
            authoredScale = transform.localScale;
        }

        private void OnEnable()
        {
            if (staticVisual != null)
            {
                staticWasEnabled = staticVisual.enabled;
                staticVisual.enabled = false;
            }
            SetPartsVisible(true);
            currentState = 0;
            wasGrounded = false;
            observedAirborne = false;
            landingRemaining = 0f;
        }

        private void Update()
        {
            if (player == null || player.Body == null || animator == null) return;
            Vector3 scale = authoredScale;
            scale.x = Mathf.Abs(scale.x) * (player.FacingLeft ? -1f : 1f);
            transform.localScale = scale;

            Vector2 velocity = player.Body.linearVelocity;
            bool grounded = player.IsGrounded;
            if (!grounded && velocity.y < -0.2f) observedAirborne = true;
            if (grounded && !wasGrounded && observedAirborne)
            {
                landingRemaining = landingDuration;
                observedAirborne = false;
            }
            wasGrounded = grounded;
            landingRemaining = Mathf.Max(0f, landingRemaining - Time.deltaTime);

            float speed = Mathf.Abs(velocity.x);
            int next;
            if (!grounded)
            {
                // Jump or step-off always interrupts the landing; input is never locked.
                landingRemaining = 0f;
                next = velocity.y > 1.2f ? JumpUp : velocity.y < -1.2f ? Fall : Apex;
            }
            else if (landingRemaining > 0f &&
                     (speed < 0.1f || landingRemaining > landingDuration - 0.10f)) next = Land;
            else if (speed < 0.1f) next = Idle;
            else next = speed > (player.WalkSpeed + player.RunSpeed) * 0.5f ? Run : Walk;

            if (next != currentState)
            {
                animator.CrossFadeInFixedTime(next, currentState == 0 ? 0f : 0.06f);
                currentState = next;
            }
        }

        private void OnDisable()
        {
            SetPartsVisible(false);
            if (staticVisual != null) staticVisual.enabled = staticWasEnabled;
        }

        private void SetPartsVisible(bool visible)
        {
            if (parts == null) return;
            foreach (SpriteRenderer part in parts)
                if (part != null) part.enabled = visible;
        }
    }
}
