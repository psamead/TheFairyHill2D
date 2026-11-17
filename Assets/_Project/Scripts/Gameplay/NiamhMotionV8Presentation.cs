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
        private SpriteRenderer raisedVisual;
        private Sprite raisedSprite;
        private float raisedRemaining;
        private bool[] partVisibilityBeforeRaise;

        public bool TryShowRaisedCross(float duration)
        {
            if (!isActiveAndEnabled || parts == null) return false;
            if (raisedSprite == null) raisedSprite = Resources.Load<Sprite>("Niamh/CrossRaisedStory");
            if (raisedSprite == null)
            {
                Debug.LogWarning("Raised-cross sprite is missing; keeping the normal v8 visual.", this);
                return false;
            }
            if (raisedVisual == null)
            {
                var child = new GameObject("Raised Cross Story Pose");
                child.transform.SetParent(transform, false);
                raisedVisual = child.AddComponent<SpriteRenderer>();
                raisedVisual.sprite = raisedSprite;
                raisedVisual.sortingOrder = 8;
                foreach (var part in parts)
                    if (part != null)
                    {
                        raisedVisual.sharedMaterial = part.sharedMaterial;
                        raisedVisual.sortingLayerID = part.sortingLayerID;
                        break;
                    }
            }
            if (raisedRemaining <= 0)
            {
                partVisibilityBeforeRaise = new bool[parts.Length];
                for (int i = 0; i < parts.Length; i++)
                    partVisibilityBeforeRaise[i] = parts[i] != null && parts[i].enabled;
            }
            raisedRemaining = Mathf.Max(raisedRemaining, duration);
            SetPartsVisible(false);
            raisedVisual.enabled = true;
            ApplyFacing();
            return true;
        }

        private void EndRaisedCross()
        {
            raisedRemaining = 0;
            if (raisedVisual != null) raisedVisual.enabled = false;
            if (partVisibilityBeforeRaise != null)
                for (int i = 0; i < parts.Length && i < partVisibilityBeforeRaise.Length; i++)
                    if (parts[i] != null) parts[i].enabled = partVisibilityBeforeRaise[i];
            partVisibilityBeforeRaise = null;
            currentState = 0;
        }

        private void ApplyFacing()
        {
            Vector3 scale = authoredScale;
            scale.x = Mathf.Abs(scale.x) * (player != null && player.FacingLeft ? -1f : 1f);
            transform.localScale = scale;
        }

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
            ApplyFacing();
            if (raisedRemaining > 0)
            {
                raisedRemaining -= Time.deltaTime;
                if (raisedRemaining > 0) return;
                EndRaisedCross();
            }

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
            EndRaisedCross();
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
