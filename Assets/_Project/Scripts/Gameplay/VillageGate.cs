using System.Collections;
using UnityEngine;

namespace TheFairyHills.Gameplay
{
    /// <summary>Reversible gate: E hides the boards and clears the tall blocker until Niamh leaves the area.</summary>
    public sealed class VillageGate : PlayerInteractionTarget
    {
        [SerializeField] private SpriteRenderer boards;
        [SerializeField] private Sprite closedSprite;
        [SerializeField] private Sprite openSprite;
        [SerializeField] private Collider2D blocker;
        [SerializeField] private AudioSource creakSource;
        [SerializeField] private AudioSource latchSource;
        [SerializeField] private AudioClip[] creakClips;
        [SerializeField] private AudioClip latchClip;
        [SerializeField] private float overallSfxVolume = 0.7f;
        private bool opened;
        private int previousCreakIndex = -1;
        private float lastGateEventTime = float.NegativeInfinity;
        private Coroutine pendingLatch;
        private Coroutine replaceCreak;

        public void Configure(SpriteRenderer gateRenderer, Sprite closed, Sprite open, Collider2D invisibleWall)
        {
            boards = gateRenderer;
            closedSprite = closed;
            openSprite = open;
            blocker = invisibleWall;
            if (boards != null) boards.sprite = closedSprite;
            if (boards != null) boards.enabled = true;
            if (blocker != null) blocker.enabled = true;
            SetPrompt("Press E to push open the gate");
        }

        public void ConfigureAudio(AudioSource creak, AudioSource latch, AudioClip[] creaks, AudioClip latchCue)
        {
            creakSource = creak;
            latchSource = latch;
            creakClips = creaks;
            latchClip = latchCue;
            ConfigureAudioSource(creakSource);
            ConfigureAudioSource(latchSource);
            if (creakSource != null) creakSource.volume = overallSfxVolume;
            if (latchSource != null) latchSource.volume = overallSfxVolume;
        }

        public override bool CanInteract => base.CanInteract && !opened;

        public override void Interact(PlayerController2D player)
        {
            if (opened) return;
            opened = true;
            if (boards != null) boards.sprite = openSprite;
            if (blocker != null) blocker.enabled = false;
            PlayGateSound(true);
            if (player != null) player.ShowMessage("The gate swings open.");
        }

        public void CloseAfterPlayerLeaves()
        {
            if (!opened) return;
            opened = false;
            if (boards != null) boards.sprite = closedSprite;
            if (blocker != null) blocker.enabled = true;
            PlayGateSound(false);
        }

        private void PlayGateSound(bool opening)
        {
            if (pendingLatch != null)
            {
                StopCoroutine(pendingLatch);
                pendingLatch = null;
            }
            if (replaceCreak != null)
            {
                StopCoroutine(replaceCreak);
                replaceCreak = null;
            }

            bool playNewSound = Time.unscaledTime - lastGateEventTime >= 0.15f;
            lastGateEventTime = Time.unscaledTime;
            if (creakSource != null && creakSource.isPlaying)
                replaceCreak = StartCoroutine(FadeAndMaybePlay(opening, playNewSound));
            else if (playNewSound)
                StartNewCreak(opening);
        }

        private IEnumerator FadeAndMaybePlay(bool opening, bool playNewSound)
        {
            float startVolume = creakSource.volume;
            float elapsed = 0f;
            const float fadeDuration = 0.05f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                creakSource.volume = startVolume * (1f - Mathf.Clamp01(elapsed / fadeDuration));
                yield return null;
            }
            creakSource.Stop();
            creakSource.volume = overallSfxVolume;
            replaceCreak = null;
            if (playNewSound) StartNewCreak(opening);
        }

        private void StartNewCreak(bool opening)
        {
            if (creakSource == null || creakClips == null || creakClips.Length == 0) return;
            int clipIndex = Random.Range(0, creakClips.Length);
            if (creakClips.Length > 1 && clipIndex == previousCreakIndex)
                clipIndex = (clipIndex + Random.Range(1, creakClips.Length)) % creakClips.Length;
            previousCreakIndex = clipIndex;
            AudioClip clip = creakClips[clipIndex];
            if (clip == null) return;

            creakSource.Stop();
            creakSource.clip = clip;
            creakSource.pitch = opening ? 1f : 0.9f;
            creakSource.volume = overallSfxVolume;
            creakSource.Play();
            if (latchClip != null && latchSource != null)
                pendingLatch = StartCoroutine(PlayLatchAfterCreak(clip.length / Mathf.Max(0.01f, Mathf.Abs(creakSource.pitch))));
        }

        private IEnumerator PlayLatchAfterCreak(float creakLength)
        {
            yield return new WaitForSecondsRealtime(Mathf.Max(0f, creakLength - 0.5f));
            if (latchSource != null && latchClip != null)
                latchSource.PlayOneShot(latchClip, opened ? 0.6f : 0.9f);
            pendingLatch = null;
        }

        private static void ConfigureAudioSource(AudioSource source)
        {
            if (source == null) return;
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.dopplerLevel = 0f;
            source.volume = 0.7f;
        }
    }

}
