using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace TheFairyHills.Gameplay
{
    /// <summary>Menu-only story scene. The opaque overlay survives until Core is ready.</summary>
    public sealed class OpeningSequencePlayer : MonoBehaviour
    {
        public OpeningSequenceData data;
        public UnityEngine.UI.Image picture;
        public CanvasGroup captionGroup;
        public TMP_Text caption, pageLabel, chapterNumber, chapterTitle;
        public UnityEngine.UI.Image chapterDim;
        public CanvasGroup blackFade;
        public UnityEngine.UI.Button nextButton, skipButton;
        public AudioSource music;
        public Camera storyCamera;
        public AudioListener storyListener;
        public EventSystem storyEvents;
        public static bool BlocksGameplay { get; private set; }
        public int CurrentIndex { get; private set; }
        public bool IsChapterCard { get; private set; }
        public bool IsLoading { get; private set; }
        public float PageElapsed { get; private set; }
        public int CompletionCount { get; private set; }
        private float acceptAfter;
        private bool transitioning;
        private ChapterHUD hiddenHud;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession() => BlocksGameplay = false;

        private void Awake()
        {
            BlocksGameplay = true;
            blackFade.alpha = 1;
            nextButton.onClick.AddListener(RequestAdvance);
            skipButton.onClick.AddListener(RequestSkip);
        }

        private void Start()
        {
            if (data == null || data.frames == null || data.frames.Length == 0)
            {
                Debug.LogError("Opening sequence has no frames.", this);
                ReturnToMenu();
                return;
            }
            if (music != null && data.music != null)
            {
                music.clip = data.music; music.loop = true; music.volume = 0; music.Play();
            }
            StartCoroutine(ShowPage(0, true));
        }

        private void Update()
        {
            if (IsLoading || transitioning || Time.timeScale <= 0 || Time.unscaledTime < acceptAfter) return;
            Keyboard k = Keyboard.current;
            if (k != null && k.escapeKey.wasPressedThisFrame) RequestSkip();
            else if (k != null && (k.spaceKey.wasPressedThisFrame || k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame)) RequestAdvance();
            else
            {
                PageElapsed += Time.unscaledDeltaTime;
                float duration = IsChapterCard ? data.chapterDuration : data.frames[CurrentIndex].duration;
                if (PageElapsed >= duration) RequestAdvance();
            }
        }

        public void RequestAdvance()
        {
            if (!CanAccept()) return;
            if (IsChapterCard) StartCoroutine(Complete());
            else StartCoroutine(ShowPage(CurrentIndex + 1, false));
        }

        public void RequestSkip()
        {
            if (!CanAccept()) return;
            // Every skip sees the same chapter card and destination; no alternate game state.
            if (IsChapterCard) StartCoroutine(Complete());
            else StartCoroutine(ShowPage(data.frames.Length, false));
        }

        private bool CanAccept() => !transitioning && !IsLoading && Time.timeScale > 0 && Time.unscaledTime >= acceptAfter;

        private IEnumerator ShowPage(int index, bool first)
        {
            transitioning = true;
            SetButtons(false);
            // Same picture across three beats: fade the text, keep the image stable.
            if (!first) yield return FadeCaption(1, 0, data.fadeDuration * 0.5f);
            CurrentIndex = index;
            IsChapterCard = index >= data.frames.Length;
            PageElapsed = 0;
            picture.sprite = IsChapterCard ? data.chapterImage : data.frames[index].image;
            picture.preserveAspect = true;
            caption.text = IsChapterCard ? "" : data.frames[index].caption;
            pageLabel.text = IsChapterCard ? "" : (index + 1) + " / " + data.frames.Length;
            chapterNumber.text = IsChapterCard ? data.chapterNumber : "";
            chapterTitle.text = IsChapterCard ? data.chapterTitle : "";
            chapterDim.color = new Color(0, 0, 0, IsChapterCard ? 0.64f : 0);
            skipButton.gameObject.SetActive(!IsChapterCard);
            if (first)
            {
                for (float t = 0; t < data.fadeDuration; t += Time.unscaledDeltaTime)
                {
                    float u = t / data.fadeDuration;
                    blackFade.alpha = 1 - u; captionGroup.alpha = u;
                    if (music != null) music.volume = data.musicVolume * u;
                    yield return null;
                }
                blackFade.alpha = 0;
                if (music != null) music.volume = data.musicVolume;
            }
            else yield return FadeCaption(0, 1, data.fadeDuration * 0.5f);
            captionGroup.alpha = 1;
            PageElapsed = 0;
            acceptAfter = Time.unscaledTime + 0.2f;
            transitioning = false;
            SetButtons(true);
            // Keyboard is handled once here; disable uGUI navigation/submit on these buttons.
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }

        private IEnumerator Complete()
        {
            IsLoading = true; transitioning = true; CompletionCount++;
            SetButtons(false);
            float initialVolume = music != null ? music.volume : 0;
            for (float t = 0; t < data.fadeDuration; t += Time.unscaledDeltaTime)
            {
                float u = t / data.fadeDuration;
                blackFade.alpha = u;
                if (music != null) music.volume = initialVolume * (1 - u);
                yield return null;
            }
            blackFade.alpha = 1;
            if (music != null) music.Stop();
            DontDestroyOnLoad(gameObject);
            // Only the overlay remains active during Core/level setup; Core supplies the listener/events.
            if (storyEvents != null) storyEvents.enabled = false;
            if (storyListener != null) storyListener.enabled = false;
            if (storyCamera != null) storyCamera.enabled = false;
            AsyncOperation load = SceneManager.LoadSceneAsync(data.nextScene, LoadSceneMode.Single);
            while (load != null && !load.isDone) yield return null;
            PlayerController2D player = null;
            float readyDeadline = Time.realtimeSinceStartup + 15;
            while (player == null || Camera.main == null || FindFirstObjectByType<ChapterSceneLoader>() == null)
            {
                if (Time.realtimeSinceStartup > readyDeadline)
                { Debug.LogError("Opening: Core did not become ready; returning to menu.", this); ReturnToMenu(); yield break; }
                player = FindFirstObjectByType<PlayerController2D>();
                yield return null;
            }
            hiddenHud = FindFirstObjectByType<ChapterHUD>();
            if (hiddenHud != null) hiddenHud.gameObject.SetActive(false);
            // Let LevelBootstrap/ChapterSceneLoader finish spawn, light and camera configuration.
            yield return null; yield return null;
            AudioSource levelMusic = null;
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
                foreach (AudioSource source in root.GetComponentsInChildren<AudioSource>())
                    if (source.gameObject.name == "Level Music") levelMusic = source;
            float targetVolume = levelMusic != null ? levelMusic.volume : 0;
            if (levelMusic != null) levelMusic.volume = 0;
            for (float t = 0; t < data.fadeDuration; t += Time.unscaledDeltaTime)
            {
                float u = t / data.fadeDuration; blackFade.alpha = 1 - u;
                if (levelMusic != null) levelMusic.volume = targetVolume * u;
                yield return null;
            }
            if (levelMusic != null) levelMusic.volume = targetVolume;
            // Wait for the advancing press to be released, including held/repeated keys.
            while (NavigationHeld()) yield return null;
            yield return null;
            BlocksGameplay = false;
            if (hiddenHud != null) hiddenHud.gameObject.SetActive(true);
            blackFade.alpha = 0;
            Destroy(gameObject);
        }

        private static bool NavigationHeld()
        {
            Keyboard k = Keyboard.current;
            return (k != null && (k.spaceKey.isPressed || k.enterKey.isPressed || k.numpadEnterKey.isPressed ||
                k.escapeKey.isPressed || k.eKey.isPressed || k.qKey.isPressed)) ||
                (Mouse.current != null && Mouse.current.leftButton.isPressed);
        }

        private IEnumerator FadeCaption(float from, float to, float duration)
        {
            for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
            { captionGroup.alpha = Mathf.Lerp(from, to, t / duration); yield return null; }
            captionGroup.alpha = to;
        }

        private void SetButtons(bool value)
        { nextButton.interactable = value; skipButton.interactable = value; }

        public void ReturnToMenu()
        {
            StopAllCoroutines(); BlocksGameplay = false;
            if (hiddenHud != null) hiddenHud.gameObject.SetActive(true);
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            BlocksGameplay = false;
            if (hiddenHud != null) hiddenHud.gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            BlocksGameplay = false;
            if (hiddenHud != null) hiddenHud.gameObject.SetActive(true);
            if (blackFade != null) blackFade.alpha = 0;
            if (music != null) music.Stop();
        }
    }
}
