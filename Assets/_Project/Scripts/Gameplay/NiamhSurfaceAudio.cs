using UnityEngine;

namespace TheFairyHills.Gameplay
{
    /// <summary>Opt-out movement sounds; gameplay remains authoritative in PlayerController2D.</summary>
    [RequireComponent(typeof(PlayerController2D))]
    public sealed class NiamhSurfaceAudio : MonoBehaviour
    {
        [SerializeField] private SurfaceAudioProfile profile;
        [SerializeField, Range(0, 1)] private float walkVolume = 0.34f, runVolume = 0.43f, jumpVolume = 0.36f;
        [SerializeField, Range(0, 1)] private float lightLandVolume = 0.40f, heavyLandVolume = 0.52f, brushVolume = 0.20f;
        private PlayerController2D player;
        private CapsuleCollider2D capsule;
        private Animator animator;
        private AudioSource stepsSource, actionSource, brushSource;
        private readonly int[] lastStep = { -1, -1, -1, -1, -1 };
        private int lastJump = -1, lastLandLight = -1, lastLandHeavy = -1, lastBrush = -1;
        private int lastState, lastBucket;
        private float airborneTime, fastestFall, nextStep, nextBrush;
        private bool wasGrounded;
        private GroundMaterial lastSurface;
        public string LastCue { get; private set; }
        public int StepCount { get; private set; }
        public int JumpCount { get; private set; }
        public int LandCount { get; private set; }
        public int BrushCount { get; private set; }

        private void Awake()
        {
            player = GetComponent<PlayerController2D>(); capsule = GetComponent<CapsuleCollider2D>();
            animator = GetComponentInChildren<Animator>();
            stepsSource = CreateSource(); actionSource = CreateSource(); brushSource = CreateSource();
        }
        private AudioSource CreateSource()
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false; source.spatialBlend = 0; source.loop = false; return source;
        }
        private void OnEnable()
        {
            if (player == null) player = GetComponent<PlayerController2D>();
            player.Jumped += OnJump;
            wasGrounded = false; airborneTime = fastestFall = 0; lastState = 0;
        }
        private void OnDisable()
        {
            if (player != null) player.Jumped -= OnJump;
            StopSources();
        }
        private void StopSources()
        {
            if (stepsSource != null) stepsSource.Stop();
            if (actionSource != null) actionSource.Stop();
            if (brushSource != null) brushSource.Stop();
        }
        private GroundMaterial CurrentSurface()
        {
            if (player.GroundCollider != null)
            {
                var ground = player.GroundCollider.GetComponent<GroundSurface2D>();
                if (ground != null) return ground.At(player.GroundPoint.x);
            }
            return lastSurface;
        }
        private void LateUpdate()
        {
            if (profile == null || !player.isActiveAndEnabled || OpeningSequencePlayer.BlocksGameplay || Time.timeScale <= 0)
            { StopSources(); lastState = 0; airborneTime = fastestFall = 0; wasGrounded = false; return; }
            bool grounded = player.IsGrounded;
            float horizontalSpeed = Mathf.Abs(player.Body.linearVelocity.x);
            if (!grounded)
            {
                airborneTime += Time.deltaTime;
                fastestFall = Mathf.Min(fastestFall, player.Body.linearVelocity.y);
                stepsSource.Stop(); lastState = 0;
            }
            else
            {
                lastSurface = CurrentSurface();
                if (!wasGrounded && airborneTime >= 0.08f && fastestFall <= -1.5f)
                {
                    bool heavy = fastestFall <= -10;
                    Play(actionSource, heavy ? profile.heavyLand : profile.lightLand,
                        ref (heavy ? ref lastLandHeavy : ref lastLandLight), heavy ? heavyLandVolume : lightLandVolume, "Land " + (heavy ? "heavy " : "light ") + lastSurface);
                    LandCount++; nextStep = Time.time + 0.12f;
                }
                airborneTime = fastestFall = 0;
                if (horizontalSpeed > 0.15f && animator != null && !animator.IsInTransition(0))
                {
                    AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
                    if (state.IsName("Walk") || state.IsName("Run"))
                    {
                        // v8 extremes/foot contacts occur twice per cycle: phases .25 and .75.
                        int bucket = Mathf.FloorToInt(state.normalizedTime * 2 + 0.5f);
                        if (lastState == state.fullPathHash && bucket != lastBucket && Time.time >= nextStep)
                        {
                            PlayStep(state.IsName("Run")); nextStep = Time.time + 0.12f;
                        }
                        lastState = state.fullPathHash; lastBucket = bucket;
                    }
                    else lastState = 0;
                }
                else if (horizontalSpeed <= 0.15f) { stepsSource.Stop(); lastState = 0; }
            }
            wasGrounded = grounded;
            if (horizontalSpeed <= 0.15f) { brushSource.Stop(); return; }
            if (Time.time < nextBrush) return;
            foreach (var surface in GroundSurface2D.BrushSurfaces)
            {
                if (surface == null || surface.Collider == null || !surface.Collider.enabled) continue;
                Vector2 playerEdge = capsule.ClosestPoint(surface.Collider.bounds.center);
                Vector2 hayEdge = surface.Collider.ClosestPoint(playerEdge);
                if ((playerEdge - hayEdge).sqrMagnitude > 0.18f * 0.18f) continue;
                Play(brushSource, profile.hayBrush, ref lastBrush, brushVolume, "HayBrush");
                BrushCount++; nextBrush = Time.time + 0.55f; break;
            }
        }
        private void PlayStep(bool running)
        {
            foreach (var set in profile.steps)
                if (set.surface == lastSurface)
                {
                    Play(stepsSource, set.clips, ref lastStep[(int)lastSurface], running ? runVolume : walkVolume, "Step " + lastSurface);
                    StepCount++; break;
                }
        }
        private void OnJump()
        {
            if (profile == null || Time.timeScale <= 0 || OpeningSequencePlayer.BlocksGameplay) return;
            lastSurface = CurrentSurface(); stepsSource.Stop(); lastState = 0;
            Play(actionSource, profile.jump, ref lastJump, jumpVolume, "Jump " + lastSurface); JumpCount++;
        }
        private void Play(AudioSource source, AudioClip[] clips, ref int previous, float volume, string cue)
        {
            if (source == null || clips == null || clips.Length == 0) return;
            int index = Random.Range(0, clips.Length);
            if (clips.Length > 1 && index == previous) index = (index + Random.Range(1, clips.Length)) % clips.Length;
            previous = index; source.pitch = Random.Range(0.97f, 1.03f);
            source.PlayOneShot(clips[index], volume * Random.Range(0.96f, 1.04f));
            LastCue = cue + " " + clips[index].name;
        }
    }
}
