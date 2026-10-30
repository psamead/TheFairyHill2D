using UnityEngine;
using UnityEngine.InputSystem;

namespace TheFairyHills.Gameplay
{
    /// <summary>Small keyboard-first platform controller for the Chapter 1 classroom graybox.</summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
    public sealed class PlayerController2D : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private CapsuleCollider2D capsule;
        [SerializeField] private SpriteRenderer playerVisual;
        [SerializeField] private ChapterHUD hud;
        [SerializeField] private float walkSpeed = 3.2f;
        [SerializeField] private float runSpeed = 5.6f;
        [SerializeField] private float jumpVelocity = 12f;
        [SerializeField] private float risingGravity = 30f;
        [SerializeField] private float fallingGravity = 45f;
        [SerializeField] private float coyoteDuration = 0.10f;
        [SerializeField] private float jumpBufferDuration = 0.12f;
        [SerializeField] private float groundCastDistance = 0.035f;
        [SerializeField] private float interactionRadius = 1.65f;
        [SerializeField] private float ironPulseRadius = 4.5f;
        [SerializeField] private float leftBoundary;
        [SerializeField] private float rightBoundary = 100f;

        private readonly Collider2D[] overlapBuffer = new Collider2D[24];
        private readonly RaycastHit2D[] groundHitBuffer = new RaycastHit2D[8];
        private ContactFilter2D overlapFilter;
        private ContactFilter2D groundFilter;
        private float horizontalInput;
        private float coyoteTimer;
        private float jumpBufferTimer;
        private bool running;
        private bool jumpCutQueued;
        private Vector2 lastSafePosition;
        private PlayerInteractionTarget nearestTarget;

        private bool hasIronCrossFallback;
        public bool HasIronCross => GameState.Instance != null ? GameState.Instance.HasIronCross : hasIronCrossFallback;
        public Rigidbody2D Body => body;

        public void Configure(SpriteRenderer visual, ChapterHUD sceneHud, float minX, float maxX)
        {
            body = GetComponent<Rigidbody2D>();
            capsule = GetComponent<CapsuleCollider2D>();
            playerVisual = visual;
            hud = sceneHud;
            leftBoundary = minX;
            rightBoundary = maxX;
        }

        public void ConfigureLevel(ChapterHUD sceneHud, float sceneLength)
        {
            hud = sceneHud;
            leftBoundary = 0f;
            rightBoundary = sceneLength;
        }

        private void Awake()
        {
            if (body == null) body = GetComponent<Rigidbody2D>();
            if (capsule == null) capsule = GetComponent<CapsuleCollider2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            overlapFilter = ContactFilter2D.noFilter;
            overlapFilter.useTriggers = true;
            groundFilter = ContactFilter2D.noFilter;
            groundFilter.useTriggers = false;
            lastSafePosition = body.position;
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                horizontalInput = 0f;
                return;
            }

            bool left = keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed;
            bool right = keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed;
            horizontalInput = (right ? 1f : 0f) - (left ? 1f : 0f);
            running = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
            if (playerVisual != null && horizontalInput != 0f)
                playerVisual.flipX = horizontalInput < 0f;

            if (keyboard.spaceKey.wasPressedThisFrame)
                jumpBufferTimer = jumpBufferDuration;
            else
                jumpBufferTimer = Mathf.Max(0f, jumpBufferTimer - Time.deltaTime);

            if (keyboard.spaceKey.wasReleasedThisFrame)
                jumpCutQueued = true;

            if (keyboard.eKey.wasPressedThisFrame)
                Interact();
            if (keyboard.qKey.wasPressedThisFrame)
                ReleaseIronPulse();

            RefreshInteractionPrompt();

            if (body.position.y < -6f)
            {
                body.position = lastSafePosition;
                body.linearVelocity = Vector2.zero;
            }
        }

        private void FixedUpdate()
        {
            bool grounded = CheckGrounded();
            if (grounded)
                coyoteTimer = coyoteDuration;
            else
                coyoteTimer = Mathf.Max(0f, coyoteTimer - Time.fixedDeltaTime);

            Vector2 velocity = body.linearVelocity;
            float speed = running ? runSpeed : walkSpeed;
            velocity.x = horizontalInput * speed;

            if (jumpBufferTimer > 0f && (grounded || coyoteTimer > 0f))
            {
                velocity.y = jumpVelocity;
                jumpBufferTimer = 0f;
                coyoteTimer = 0f;
                grounded = false;
            }

            if (jumpCutQueued && velocity.y > 0f)
                velocity.y *= 0.5f;
            jumpCutQueued = false;

            if (grounded && velocity.y <= 0f)
                velocity.y = 0f;
            else if (!grounded)
            {
                float gravity = velocity.y > 0f ? risingGravity : fallingGravity;
                velocity.y = Mathf.Max(-22f, velocity.y - gravity * Time.fixedDeltaTime);
            }

            float halfWidth = capsule.bounds.extents.x;
            if ((body.position.x <= leftBoundary + halfWidth && velocity.x < 0f) ||
                (body.position.x >= rightBoundary - halfWidth && velocity.x > 0f))
                velocity.x = 0f;

            body.linearVelocity = velocity;
            if (grounded && body.position.y > -0.15f)
                lastSafePosition = body.position;
        }

        private bool CheckGrounded()
        {
            if (capsule == null) return false;
            int count = capsule.Cast(Vector2.down, groundFilter, groundHitBuffer, groundCastDistance);
            for (int i = 0; i < count; i++)
            {
                RaycastHit2D hit = groundHitBuffer[i];
                if (hit.collider != null && hit.normal.y > 0.5f)
                    return true;
            }
            return false;
        }

        private void RefreshInteractionPrompt()
        {
            nearestTarget = null;
            if (hud == null) return;

            int count = Physics2D.OverlapCircle(transform.position + Vector3.up * 0.8f,
                interactionRadius, overlapFilter, overlapBuffer);
            float nearestDistance = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                Collider2D hit = overlapBuffer[i];
                if (hit == null || hit.transform.IsChildOf(transform)) continue;
                PlayerInteractionTarget target = hit.GetComponentInParent<PlayerInteractionTarget>();
                if (target == null || !target.CanInteract) continue;
                Vector2 closest = hit.ClosestPoint(transform.position + Vector3.up * 0.8f);
                float distance = ((Vector2)transform.position + Vector2.up * 0.8f - closest).sqrMagnitude;
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestTarget = target;
                }
            }

            hud.SetPrompt(nearestTarget != null ? nearestTarget.Prompt : string.Empty);
        }

        private void Interact()
        {
            RefreshInteractionPrompt();
            if (nearestTarget != null)
                nearestTarget.Interact(this);
        }

        private void ReleaseIronPulse()
        {
            if (!HasIronCross)
            {
                if (hud != null) hud.ShowMessage("The iron cross is still missing.", 2f);
                return;
            }

            int count = Physics2D.OverlapCircle(transform.position + Vector3.up,
                ironPulseRadius, overlapFilter, overlapBuffer);
            ThornWall nearestWall = null;
            float nearestDistance = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                if (overlapBuffer[i] == null) continue;
                ThornWall wall = overlapBuffer[i].GetComponentInParent<ThornWall>();
                if (wall == null || wall.IsDispersed) continue;
                float distance = (wall.transform.position - transform.position).sqrMagnitude;
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestWall = wall;
                }
            }

            if (nearestWall != null)
                nearestWall.Disperse();
            else if (hud != null)
                hud.ShowMessage("The IronPulse needs a controlled thorn wall nearby.", 2f);
        }

        public void CollectIronCross()
        {
            if (HasIronCross) return;
            hasIronCrossFallback = true;
            if (GameState.Instance != null) GameState.Instance.CollectIronCross();
            if (hud != null)
                hud.ShowMessage("Iron Cross acquired. Press Q near the thorn wall to release IronPulse.", 4f);
        }

        public void ShowMessage(string message, float seconds = 2f)
        {
            if (hud != null) hud.ShowMessage(message, seconds);
        }

        private void OnDrawGizmosSelected()
        {
            if (capsule != null)
            {
                Gizmos.color = Color.green;
                Bounds bounds = capsule.bounds;
                Gizmos.DrawLine(new Vector3(bounds.min.x, bounds.min.y, bounds.center.z),
                    new Vector3(bounds.min.x, bounds.min.y - groundCastDistance, bounds.center.z));
                Gizmos.DrawLine(new Vector3(bounds.max.x, bounds.min.y, bounds.center.z),
                    new Vector3(bounds.max.x, bounds.min.y - groundCastDistance, bounds.center.z));
            }
            Gizmos.color = new Color(0.9f, 0.75f, 0.25f, 0.8f);
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.8f, interactionRadius);
        }
    }
}
