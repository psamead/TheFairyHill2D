using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using TheFairyHills.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace TheFairyHills.Editor
{
    internal static class MainMenuSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/MainMenu.unity";
        private const string BackgroundPath = "Assets/_Project/Art/Menu/Exported/BG_clean_signature.png";
        private const string LeftDarkPath = "Assets/_Project/Art/Menu/Exported/left_dark.png";
        private const string TitlePlaquePath = "Assets/_Project/Art/Menu/Exported/title_plaque.png";
        private const string SelectionIconPath = "Assets/_Project/Art/UI/MainMenu/ICON_triskele.png";
        private const string FondamentoPath = "Assets/_Project/Fonts/Fondamento-Regular.ttf";
        private const string FontAssetPath = "Assets/_Project/Fonts/Fondamento SDF.asset";
        private const string InputAssetPath = "Assets/Settings/InputSystem_Actions.inputactions";
        private const string InputReferencesPath = "Assets/_Project/Art/UI/MainMenu/Actions";
        private const string HoverChimePath = "Assets/_Project/Audio/UI/Menu_HoverChime.wav";

        [MenuItem("Tools/The Fairy Hills/Build MainMenu Scene")]
        private static void BuildMainMenuScene()
        {
            Scene activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.IsValid() && activeScene.isDirty)
                throw new InvalidOperationException($"Save the dirty scene '{activeScene.path}' before building MainMenu.");

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Sprite background = LoadSprite(BackgroundPath);
            Sprite leftDark = LoadSprite(LeftDarkPath);
            Sprite plaque = LoadSprite(TitlePlaquePath);
            Sprite iconSprite = LoadSprite(SelectionIconPath);
            AudioClip hoverChime = AssetDatabase.LoadAssetAtPath<AudioClip>(HoverChimePath);
            if (hoverChime == null)
                throw new InvalidOperationException($"Could not load the menu hover chime at {HoverChimePath}.");
            TMP_FontAsset fontAsset = LoadOrCreateFontAsset();
            InputActionAsset inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputAssetPath);
            if (inputActions == null)
                throw new InvalidOperationException($"Could not load the project's input actions at {InputAssetPath}.");

            EnsureAction(inputActions, "UI/Navigate", "Navigate");
            EnsureAction(inputActions, "UI/Submit", "Submit");
            EnsureAction(inputActions, "UI/Cancel", "Cancel");
            EnsureAction(inputActions, "UI/Point", "Point");
            EnsureAction(inputActions, "UI/Click", "Click");
            EnsureAction(inputActions, "UI/ScrollWheel", "ScrollWheel");
            AssetDatabase.SaveAssets();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera();

            GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;
            canvas.sortingOrder = 0;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            scaler.referencePixelsPerUnit = 100f;

            Image backgroundImage = CreateImage("BG_clean_signature", canvasObject.transform, background);
            StretchToParent(backgroundImage.rectTransform);
            Image darkImage = CreateImage("LeftDark", canvasObject.transform, leftDark);
            StretchToParent(darkImage.rectTransform);
            Image plaqueImage = CreateImage("TitlePlaque", canvasObject.transform, plaque);
            SetTopLeft(plaqueImage.rectTransform, 110f, 60f, 700f, 383f);

            GameObject menuRoot = new GameObject("MenuButtons", typeof(RectTransform));
            menuRoot.transform.SetParent(canvasObject.transform, false);
            StretchToParent(menuRoot.GetComponent<RectTransform>());

            GameObject controllerObject = new GameObject("MainMenuController");
            controllerObject.transform.SetParent(canvasObject.transform, false);
            MainMenuController controller = controllerObject.AddComponent<MainMenuController>();

            int[] rowTops = { 517, 591, 671, 751 };
            string[] labels = { "Start", "Continue", "Settings", "Quit" };
            Color normal = new Color(0.937f, 0.890f, 0.761f, 1f);
            Color selected = new Color(0.949f, 0.824f, 0.541f, 1f);
            var menuButtons = new Button[4];
            var menuOptions = new MainMenuOption[4];

            for (int i = 0; i < labels.Length; i++)
            {
                bool canContinue = i != 1;
                menuOptions[i] = CreateMenuOption(labels[i], menuRoot.transform, fontAsset,
                    212f, rowTops[i], 430f, 74f, canContinue, false, controller, normal,
                    i == 0 ? selected : normal);
                menuButtons[i] = menuOptions[i].Button;
            }

            ConfigureVerticalNavigation(menuButtons);

            Image selectionIcon = CreateImage("SelectionIcon", canvasObject.transform, iconSprite);
            SetTopLeft(selectionIcon.rectTransform, 212f, 517f, 40f, 40f);
            selectionIcon.raycastTarget = false;

            TMP_Text status = CreateText("StatusMessage", canvasObject.transform, fontAsset,
                "", 30f, normal, TextAlignmentOptions.TopLeft);
            SetTopLeft(status.rectTransform, 265f, 825f, 760f, 55f);
            status.gameObject.SetActive(false);

            GameObject settingsPanel = CreateSettingsPanel(canvasObject.transform, fontAsset,
                normal, selected, controller, out Button backButton, out MainMenuOption backOption);
            backOption.Configure(controller, backButton, backButton.GetComponentInChildren<TMP_Text>(), true);

            controller.Configure(menuButtons[0], menuButtons[1], menuButtons[2], menuButtons[3],
                backButton, menuOptions, selectionIcon, hoverChime, settingsPanel, status, "Scene_Forest_01");

            CreateEventSystem(inputActions);

            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new InvalidOperationException($"Could not save the MainMenu scene to {ScenePath}.");

            AssetDatabase.ImportAsset(ScenePath, ImportAssetOptions.ForceSynchronousImport);
            SetBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Built MainMenu scene at {ScenePath}. Start currently targets 'Scene_Forest_01' and will show a status notice until that scene is registered in Build Settings.");
        }

        private static Sprite LoadSprite(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                throw new InvalidOperationException($"Texture import settings are not available for {path}.");

            bool changed = importer.textureType != TextureImporterType.Sprite ||
                           importer.spriteImportMode != SpriteImportMode.Single ||
                           !importer.alphaIsTransparency || importer.mipmapEnabled ||
                           importer.wrapMode != TextureWrapMode.Clamp ||
                           importer.filterMode != FilterMode.Bilinear ||
                           importer.textureCompression != TextureImporterCompression.Uncompressed;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            if (changed)
                importer.SaveAndReimport();

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
                throw new InvalidOperationException($"Could not import {path} as a Sprite.");
            return sprite;
        }

        private static TMP_FontAsset LoadOrCreateFontAsset()
        {
            TMP_FontAsset existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            if (existing != null)
                return existing;

            if (TMP_Settings.LoadDefaultSettings() == null)
                throw new InvalidOperationException("TextMesh Pro settings could not be loaded from the project resources.");

            Font font = AssetDatabase.LoadAssetAtPath<Font>(FondamentoPath);
            if (font == null)
                throw new InvalidOperationException($"Could not load Fondamento font at {FondamentoPath}.");

            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(font, 90, 9,
                GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);
            if (fontAsset == null)
                throw new InvalidOperationException("TextMeshPro could not create the Fondamento font asset.");

            fontAsset.name = "Fondamento SDF";
            fontAsset.TryAddCharacters("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 .,!?'-");
            AssetDatabase.CreateAsset(fontAsset, FontAssetPath);
            if (fontAsset.material != null)
                AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
            foreach (Texture2D texture in fontAsset.atlasTextures)
            {
                if (texture != null)
                    AssetDatabase.AddObjectToAsset(texture, fontAsset);
            }
            EditorUtility.SetDirty(fontAsset);
            return fontAsset;
        }

        private static void EnsureAction(InputActionAsset asset, string actionPath, string fileName)
        {
            InputAction action = asset.FindAction(actionPath, false);
            if (action == null)
                throw new InvalidOperationException($"The project UI action '{actionPath}' is missing.");

            Directory.CreateDirectory(InputReferencesPath);
            string referencePath = $"{InputReferencesPath}/MainMenu-{fileName}.asset";
            if (AssetDatabase.LoadAssetAtPath<InputActionReference>(referencePath) != null)
                return;

            InputActionReference reference = InputActionReference.Create(action);
            reference.name = $"MainMenu {fileName}";
            AssetDatabase.CreateAsset(reference, referencePath);
        }

        private static InputActionReference GetActionReference(string fileName)
        {
            return AssetDatabase.LoadAssetAtPath<InputActionReference>(
                $"{InputReferencesPath}/MainMenu-{fileName}.asset");
        }

        private static void CreateEventSystem(InputActionAsset actions)
        {
            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
            EventSystem eventSystem = eventSystemObject.GetComponent<EventSystem>();
            eventSystem.sendNavigationEvents = true;

            InputSystemUIInputModule module = eventSystemObject.AddComponent<InputSystemUIInputModule>();
            module.actionsAsset = actions;
            module.move = null;
            module.submit = GetActionReference("Submit");
            module.cancel = GetActionReference("Cancel");
            module.point = GetActionReference("Point");
            module.leftClick = null;
            module.rightClick = null;
            module.middleClick = null;
            module.scrollWheel = null;
        }

        private static void CreateCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.4f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.015f, 0.035f, 0.07f, 1f);
        }

        private static Image CreateImage(string name, Transform parent, Sprite sprite)
        {
            GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            Image image = imageObject.GetComponent<Image>();
            image.sprite = sprite;
            image.color = Color.white;
            image.raycastTarget = false;
            image.preserveAspect = false;
            return image;
        }

        private static MainMenuOption CreateMenuOption(string labelText, Transform parent, TMP_FontAsset font,
            float x, float top, float width, float height, bool interactable, bool isSettingsBack,
            MainMenuController controller, Color normal, Color initialColor)
        {
            GameObject row = new GameObject(labelText, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            row.transform.SetParent(parent, false);
            SetTopLeft(row.GetComponent<RectTransform>(), x, top, width, height);

            Image hitImage = row.GetComponent<Image>();
            hitImage.color = Color.clear;
            hitImage.raycastTarget = true;

            Button button = row.GetComponent<Button>();
            button.targetGraphic = hitImage;
            button.interactable = interactable;
            button.transition = Selectable.Transition.None;

            TMP_Text label = CreateText("Label", row.transform, font, labelText, 50f,
                interactable ? initialColor : new Color(normal.r, normal.g, normal.b, 0.5f), TextAlignmentOptions.TopLeft);
            SetTopLeft(label.rectTransform, 53f, 0f, width - 53f, height);
            label.raycastTarget = false;

            MainMenuOption option = row.AddComponent<MainMenuOption>();
            option.Configure(controller, button, label, isSettingsBack);
            return option;
        }

        private static GameObject CreateSettingsPanel(Transform parent, TMP_FontAsset font, Color normal,
            Color selected, MainMenuController controller, out Button backButton, out MainMenuOption backOption)
        {
            GameObject overlay = new GameObject("SettingsPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            overlay.transform.SetParent(parent, false);
            StretchToParent(overlay.GetComponent<RectTransform>());
            Image overlayImage = overlay.GetComponent<Image>();
            overlayImage.color = new Color(0.015f, 0.03f, 0.055f, 0.84f);
            overlayImage.raycastTarget = true;

            GameObject card = new GameObject("SettingsCard", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            card.transform.SetParent(overlay.transform, false);
            RectTransform cardRect = card.GetComponent<RectTransform>();
            SetCenter(cardRect, Vector2.zero, new Vector2(900f, 480f));
            card.GetComponent<Image>().color = new Color(0.055f, 0.105f, 0.15f, 0.98f);

            TMP_Text title = CreateText("Title", card.transform, font, "SETTINGS", 54f, selected,
                TextAlignmentOptions.Center);
            SetCenter(title.rectTransform, new Vector2(0f, -58f), new Vector2(760f, 90f));

            TMP_Text message = CreateText("Placeholder", card.transform, font,
                "Settings will be available in a later build.", 30f, normal, TextAlignmentOptions.Center);
            SetCenter(message.rectTransform, new Vector2(0f, 38f), new Vector2(760f, 70f));

            MainMenuOption back = CreateMenuOption("Back", card.transform, font,
                0f, 0f, 280f, 68f, true, true, controller, normal, normal);
            backButton = back.Button;
            RectTransform backRect = backButton.GetComponent<RectTransform>();
            SetCenter(backRect, new Vector2(0f, 150f), new Vector2(280f, 68f));
            TMP_Text backLabel = backButton.GetComponentInChildren<TMP_Text>();
            backLabel.alignment = TextAlignmentOptions.Center;
            RectTransform backLabelRect = backLabel.rectTransform;
            backLabelRect.anchorMin = Vector2.zero;
            backLabelRect.anchorMax = Vector2.one;
            backLabelRect.pivot = new Vector2(0.5f, 0.5f);
            backLabelRect.offsetMin = Vector2.zero;
            backLabelRect.offsetMax = Vector2.zero;
            Navigation backNavigation = backButton.navigation;
            backNavigation.mode = Navigation.Mode.None;
            backButton.navigation = backNavigation;
            backOption = back;
            return overlay;
        }

        private static TMP_Text CreateText(string name, Transform parent, TMP_FontAsset font, string text,
            float fontSize, Color color, TextAlignmentOptions alignment)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            TextMeshProUGUI textComponent = textObject.GetComponent<TextMeshProUGUI>();
            textComponent.font = font;
            textComponent.text = text;
            textComponent.fontSize = fontSize;
            textComponent.color = color;
            textComponent.alignment = alignment;
            textComponent.textWrappingMode = TextWrappingModes.NoWrap;
            textComponent.overflowMode = TextOverflowModes.Overflow;
            textComponent.margin = Vector4.zero;
            textComponent.extraPadding = true;
            textComponent.raycastTarget = false;
            return textComponent;
        }

        private static void SetTopLeft(RectTransform rect, float x, float top, float width, float height)
        {
            rect.anchorMin = Vector2.up;
            rect.anchorMax = Vector2.up;
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, -top);
            rect.sizeDelta = new Vector2(width, height);
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        private static void SetCenter(RectTransform rect, Vector2 anchoredPosition, Vector2 size)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        private static void StretchToParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        private static void ConfigureVerticalNavigation(Button[] buttons)
        {
            // Continue is intentionally disabled, so navigation skips from Start to Settings.
            for (int i = 0; i < buttons.Length; i++)
            {
                Navigation navigation = buttons[i].navigation;
                navigation.mode = Navigation.Mode.Explicit;
                navigation.selectOnUp = null;
                navigation.selectOnDown = null;
                buttons[i].navigation = navigation;
            }

            SetNeighbors(buttons[0], buttons[3], buttons[2]);
            SetNeighbors(buttons[2], buttons[0], buttons[3]);
            SetNeighbors(buttons[3], buttons[2], buttons[0]);
            Navigation disabledNavigation = buttons[1].navigation;
            disabledNavigation.mode = Navigation.Mode.None;
            buttons[1].navigation = disabledNavigation;
        }

        private static void SetNeighbors(Button button, Selectable up, Selectable down)
        {
            Navigation navigation = button.navigation;
            navigation.mode = Navigation.Mode.Explicit;
            navigation.selectOnUp = up;
            navigation.selectOnDown = down;
            button.navigation = navigation;
        }

        private static void SetBuildSettings(string menuScenePath)
        {
            var scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(menuScenePath, true)
            };

            foreach (EditorBuildSettingsScene existing in EditorBuildSettings.scenes)
            {
                if (!string.Equals(existing.path, menuScenePath, StringComparison.OrdinalIgnoreCase))
                    scenes.Add(existing);
            }

            EditorBuildSettings.scenes = scenes.ToArray();

            string guid = AssetDatabase.AssetPathToGUID(menuScenePath);
            if (string.IsNullOrEmpty(guid))
                throw new InvalidOperationException($"Could not resolve the asset GUID for {menuScenePath}.");

            string settingsPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName,
                "ProjectSettings", "EditorBuildSettings.asset");
            string settingsText = File.ReadAllText(settingsPath);
            string sceneEntryPattern = $"(?m)(^\\s*-\\s*enabled:\\s*1\\s*\\r?\\n\\s*path:\\s*{Regex.Escape(menuScenePath)}\\s*\\r?\\n\\s*guid:\\s*)[0-9a-fA-F]{{32}}";
            string updatedSettings = new Regex(sceneEntryPattern).Replace(settingsText, "$1" + guid, 1);
            if (updatedSettings == settingsText)
                throw new InvalidOperationException($"Could not find the {menuScenePath} entry in EditorBuildSettings.asset.");

            File.WriteAllText(settingsPath, updatedSettings);
        }
    }
}
