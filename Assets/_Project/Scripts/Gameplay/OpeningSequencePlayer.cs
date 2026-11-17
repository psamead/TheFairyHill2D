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
        [SerializeField] private bool storybookLayout = true;
        private UnityEngine.UI.Image captionShade;
        private Texture2D shadeTexture;
        private Sprite shadeSprite;
        private Material captionMaterial;
        private TMP_Text navigationHint;
        private Vector2 lastStoryScreenSize;
        private Sprite lastStorySprite;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession() => BlocksGameplay = false;

        private void Awake()
        {
            BlocksGameplay = true;
            if (storybookLayout) ApplyStorybookLayout();
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
            if (storybookLayout)
            {
                if (captionShade != null) captionShade.gameObject.SetActive(!IsChapterCard);
                if (navigationHint != null) navigationHint.gameObject.SetActive(!IsChapterCard);
                if (!IsChapterCard)
                {
                    var frame = data.frames[index];
                    Vector2 min = frame.captionAnchorMin, max = frame.captionAnchorMax;
                    if (max.x <= min.x || max.y <= min.y)
                    { min = new Vector2(0.12f, 0.12f); max = new Vector2(0.88f, 0.28f); }
                    SetRect(caption.rectTransform, min, max);
                }
                FitStoryPicture();
            }
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
            if (captionMaterial != null) Destroy(captionMaterial);
            if (shadeSprite != null) Destroy(shadeSprite);
            if (shadeTexture != null) Destroy(shadeTexture);
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

        private static void SetRect(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min; rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        // Reuse the authored Canvas and controls; this trial is reversible in the Inspector.
        private void ApplyStorybookLayout()
        {
            picture.preserveAspect = false;
            picture.raycastTarget = false;
            Transform divider = captionGroup.transform.Find("Caption Divider");
            if (divider != null) divider.gameObject.SetActive(false);
            pageLabel.gameObject.SetActive(false);
            caption.fontSize = 36;
            caption.color = new Color(1f, 0.97f, 0.88f);
            caption.alignment = TextAlignmentOptions.Center;
            caption.enableAutoSizing = true;
            caption.fontSizeMin = 28; caption.fontSizeMax = 36;
            captionMaterial = new Material(caption.fontSharedMaterial) { name = "Opening Caption Readability" };
            captionMaterial.EnableKeyword("UNDERLAY_ON");
            captionMaterial.SetColor("_UnderlayColor", new Color(0.02f, 0.04f, 0.06f, 0.9f));
            captionMaterial.SetFloat("_UnderlayOffsetY", -0.5f);
            captionMaterial.SetFloat("_UnderlaySoftness", 0.65f);
            caption.fontSharedMaterial = captionMaterial;

            shadeTexture = new Texture2D(1, 64, TextureFormat.RGBA32, false) { name = "Opening Soft Caption Shade" };
            shadeTexture.wrapMode = TextureWrapMode.Clamp;
            for (int y = 0; y < 64; y++)
                shadeTexture.SetPixel(0, y, new Color(0, 0, 0, 0.68f * Mathf.Pow(1f - y / 63f, 1.5f)));
            shadeTexture.Apply();
            shadeSprite = Sprite.Create(shadeTexture, new Rect(0, 0, 1, 64), new Vector2(0.5f, 0.5f));
            var shadeObject = new GameObject("Soft Caption Shade", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            shadeObject.transform.SetParent(picture.transform.parent, false);
            shadeObject.transform.SetSiblingIndex(picture.transform.GetSiblingIndex() + 1);
            captionShade = shadeObject.GetComponent<UnityEngine.UI.Image>();
            captionShade.sprite = shadeSprite; captionShade.raycastTarget = false;
            SetRect(captionShade.rectTransform, Vector2.zero, new Vector2(1f, 0.36f));

            SetRect(nextButton.GetComponent<RectTransform>(), new Vector2(0.80f, 0.92f), new Vector2(0.89f, 0.985f));
            SetRect(skipButton.GetComponent<RectTransform>(), new Vector2(0.90f, 0.92f), new Vector2(0.985f, 0.985f));
            foreach (var button in new[] { nextButton, skipButton })
            {
                var background = button.GetComponent<UnityEngine.UI.Image>();
                if (background != null) background.color = new Color(0.02f, 0.04f, 0.04f, 0.18f);
                TMP_Text label = button.GetComponentInChildren<TMP_Text>();
                if (label != null) { label.fontSize = 24; label.fontSharedMaterial = captionMaterial; }
            }
            var hintObject = new GameObject("Story Navigation Hint", typeof(RectTransform), typeof(TextMeshProUGUI));
            hintObject.transform.SetParent(captionGroup.transform, false);
            navigationHint = hintObject.GetComponent<TextMeshProUGUI>();
            navigationHint.font = caption.font; navigationHint.fontSharedMaterial = captionMaterial;
            navigationHint.text = KeyHintUnderlines.Format("Space / Enter   Next     Esc   Skip");
            navigationHint.fontSize = 24; navigationHint.color = new Color(1f, 0.97f, 0.88f, 0.95f);
            navigationHint.alignment = TextAlignmentOptions.Right; navigationHint.raycastTarget = false;
            SetRect(navigationHint.rectTransform, new Vector2(0.52f, 0.025f), new Vector2(0.97f, 0.065f));
            KeyHintUnderlines.Attach(navigationHint);
        }

        private void LateUpdate()
        {
            if (storybookLayout && !IsLoading) FitStoryPicture();
        }

        private void FitStoryPicture()
        {
            if (picture == null || picture.sprite == null) return;
            var parent = picture.rectTransform.parent as RectTransform;
            if (parent == null || parent.rect.height <= 0 || parent.rect.width <= 0) return;
            if (lastStoryScreenSize == parent.rect.size && lastStorySprite == picture.sprite) return;
            lastStoryScreenSize = parent.rect.size;
            lastStorySprite = picture.sprite;
            float aspect = picture.sprite.rect.width / picture.sprite.rect.height;
            float width = Mathf.Max(parent.rect.width, parent.rect.height * aspect);
            RectTransform rect = picture.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(width, width / aspect);
        }
    }
}
