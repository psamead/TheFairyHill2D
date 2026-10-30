using TheFairyHills.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TheFairyHills.UI
{
    /// <summary>Owns start-menu navigation, selection feedback, and placeholder actions.</summary>
    public sealed class MainMenuController : MonoBehaviour
    {
        [Header("Menu options")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button settingsBackButton;
        [SerializeField] private MainMenuOption[] options;

        [Header("Presentation")]
        [SerializeField] private Image selectionIcon;
        [SerializeField] private Vector2 selectionIconOffset = new Vector2(0f, -14f);
        [SerializeField] private AudioClip hoverChime;
        [SerializeField, Range(0f, 1f)] private float hoverVolume = 0.35f;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private TMP_Text statusLabel;
        [SerializeField] private string firstGameplaySceneName = "Ch1_S1_Village";
        private MainMenuOption focusedOption;
        private MainMenuOption lastMenuOption;
        private RectTransform selectionRect;
        private RectTransform focusRect;
        private AudioSource uiAudioSource;
        private bool hasFocusedOption;
        private bool settingsOpen;

        /// <summary>Called by the editor builder to bind the authored scene objects.</summary>
        public void Configure(Button start, Button continueOption, Button settings, Button quit, Button settingsBack,
            MainMenuOption[] menuOptions, Image icon, AudioClip selectionChime, GameObject panel, TMP_Text status,
            string gameplaySceneName)
        {
            startButton = start;
            continueButton = continueOption;
            settingsButton = settings;
            quitButton = quit;
            settingsBackButton = settingsBack;
            options = menuOptions;
            selectionIcon = icon;
            hoverChime = selectionChime;
            settingsPanel = panel;
            statusLabel = status;
            firstGameplaySceneName = gameplaySceneName;
            selectionRect = selectionIcon != null ? selectionIcon.rectTransform : null;
        }

        private void Awake()
        {
            uiAudioSource = GetComponent<AudioSource>();
            if (uiAudioSource == null)
                uiAudioSource = gameObject.AddComponent<AudioSource>();
            uiAudioSource.playOnAwake = false;
            uiAudioSource.spatialBlend = 0f;

            if (selectionIcon != null)
                selectionRect = selectionIcon.rectTransform;
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }

        private void Start()
        {
            MainMenuOption initialOption = FindOption(startButton);
            if (initialOption != null)
                FocusOption(initialOption);

            if (EventSystem.current != null && startButton != null && startButton.interactable)
                EventSystem.current.SetSelectedGameObject(startButton.gameObject);
        }

        private void Update()
        {
            if (settingsOpen || Keyboard.current == null)
                return;

            if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
                MoveSelection(-1);
            else if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
                MoveSelection(1);
        }

        public void FocusOption(MainMenuOption option)
        {
            if (option == null || option.Button == null || !option.Button.interactable)
                return;

            bool changedOption = focusedOption != option;
            focusedOption = option;
            if (hasFocusedOption && changedOption && hoverChime != null && uiAudioSource != null)
                uiAudioSource.PlayOneShot(hoverChime, hoverVolume);
            hasFocusedOption = true;
            if (!option.IsSettingsBack && !settingsOpen)
                lastMenuOption = option;

            focusRect = option.transform as RectTransform;
            if (selectionIcon != null)
                selectionIcon.enabled = !settingsOpen && !option.IsSettingsBack;
            if (selectionRect != null && focusRect != null && selectionIcon != null && selectionIcon.enabled)
                selectionRect.anchoredPosition = focusRect.anchoredPosition + selectionIconOffset;

            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != option.Button.gameObject)
                EventSystem.current.SetSelectedGameObject(option.Button.gameObject);

            if (options == null)
                return;

            foreach (MainMenuOption menuOption in options)
            {
                if (menuOption != null)
                    menuOption.SetHighlighted(menuOption == focusedOption);
            }

            if (option.IsSettingsBack)
                option.SetHighlighted(true);
        }

        public void OpenSettings()
        {
            if (settingsPanel == null || settingsOpen)
                return;

            if (focusedOption != null && !focusedOption.IsSettingsBack)
                lastMenuOption = focusedOption;

            settingsOpen = true;
            settingsPanel.SetActive(true);
            if (selectionIcon != null)
                selectionIcon.enabled = false;

            if (EventSystem.current != null && settingsBackButton != null)
                EventSystem.current.SetSelectedGameObject(settingsBackButton.gameObject);
        }

        public void CloseSettings()
        {
            if (!settingsOpen)
                return;

            settingsOpen = false;
            if (settingsPanel != null)
                settingsPanel.SetActive(false);

            MainMenuOption restoreOption = lastMenuOption != null ? lastMenuOption : FindOption(startButton);
            if (restoreOption != null && restoreOption.Button != null && restoreOption.Button.interactable)
            {
                FocusOption(restoreOption);
                if (EventSystem.current != null)
                    EventSystem.current.SetSelectedGameObject(restoreOption.Button.gameObject);
            }
        }

        public void StartGame()
        {
            if (!Application.CanStreamedLevelBeLoaded("Core_Persistent") ||
                !Application.CanStreamedLevelBeLoaded(firstGameplaySceneName))
            {
                SetStatus("The first chapter scene is not available.");
                Debug.LogWarning($"MainMenu: Core_Persistent or '{firstGameplaySceneName}' is missing from Build Settings.", this);
                return;
            }

            // Enter the level first. Its LevelBootstrap adds Core_Persistent and
            // attaches the already-loaded level, so this route also matches standalone
            // level testing and cannot lose a menu-owned transition coroutine.
            SceneManager.LoadScene(firstGameplaySceneName, LoadSceneMode.Single);
        }

        private void MoveSelection(int direction)
        {
            if (options == null || options.Length == 0)
                return;

            int currentIndex = System.Array.IndexOf(options, focusedOption);
            if (currentIndex < 0)
                currentIndex = 0;

            for (int step = 1; step <= options.Length; step++)
            {
                int index = (currentIndex + direction * step) % options.Length;
                if (index < 0)
                    index += options.Length;

                MainMenuOption candidate = options[index];
                if (candidate != null && candidate.Button != null && candidate.Button.interactable)
                {
                    FocusOption(candidate);
                    return;
                }
            }
        }

        public void QuitGame()
        {
            Application.Quit();
#if UNITY_EDITOR
            Debug.Log("MainMenu: Quit requested. Application.Quit is active in player builds.", this);
#endif
        }

        private void SetStatus(string message)
        {
            if (statusLabel != null)
            {
                statusLabel.text = message;
                statusLabel.gameObject.SetActive(!string.IsNullOrEmpty(message));
            }
        }

        private MainMenuOption FindOption(Button button)
        {
            if (button == null || options == null)
                return null;

            foreach (MainMenuOption option in options)
            {
                if (option != null && option.Button == button)
                    return option;
            }

            return null;
        }
    }
}
