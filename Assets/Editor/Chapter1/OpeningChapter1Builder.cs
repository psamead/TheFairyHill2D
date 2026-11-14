using System;
using System.IO;
using System.Linq;
using TheFairyHills.Gameplay;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;

namespace TheFairyHills.EditorTools
{
    public static class OpeningChapter1Builder
    {
        public const string ScenePath = "Assets/Scenes/Opening_Chapter1.unity";
        public const string DataFolder = "Assets/_Project/Data/Storyboards";
        private const string ImagePath = "Assets/_Project/Art/Storyboards/Opening_v1/placeholder/OP01_friends_dusk_placeholder_v1.png";
        [Serializable] private sealed class Brief { public string image, next_scene; public float transition_seconds; public Frame[] frames; public Card chapter_card; }
        [Serializable] private sealed class Frame { public string id, caption; public float duration_seconds; }
        [Serializable] private sealed class Card { public string number, title; public float duration_seconds; }

        [MenuItem("Tools/The Fairy Hills/Opening/Build Opening Scene Only")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode first.");
            Scene existing = SceneManager.GetSceneByPath(ScenePath);
            if (existing.IsValid() && existing.isLoaded) throw new InvalidOperationException("Close the existing Opening scene before regenerating it.");
            Scene original = SceneManager.GetActiveScene();
            var brief = JsonUtility.FromJson<Brief>(File.ReadAllText("Docs/References/Opening_Chapter1_Placeholder_v1/opening_sequence_v1.json"));
            Directory.CreateDirectory(DataFolder);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(ImagePath);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.maxTextureSize = 4096; importer.npotScale = TextureImporterNPOTScale.None;
            importer.mipmapEnabled = false; importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            Sprite image = AssetDatabase.LoadAssetAtPath<Sprite>(ImagePath);
            var data = AssetDatabase.LoadAssetAtPath<OpeningSequenceData>(DataFolder + "/Opening_Chapter1.asset");
            if (data == null) { data = ScriptableObject.CreateInstance<OpeningSequenceData>(); AssetDatabase.CreateAsset(data, DataFolder + "/Opening_Chapter1.asset"); }
            data.frames = brief.frames.Select(f => new OpeningSequenceData.Frame { id = f.id, image = image, caption = f.caption, duration = f.duration_seconds }).ToArray();
            data.chapterImage = image; data.chapterNumber = brief.chapter_card.number; data.chapterTitle = brief.chapter_card.title;
            data.chapterDuration = brief.chapter_card.duration_seconds; data.fadeDuration = brief.transition_seconds; data.nextScene = brief.next_scene;
            data.music = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/Music/MENU_BGM_v1_loop.ogg");
            EditorUtility.SetDirty(data); AssetDatabase.SaveAssetIfDirty(data);

            string fontPath = DataFolder + "/Opening_Fondamento.asset";
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
            if (font == null)
            {
                font = TMP_FontAsset.CreateFontAsset(AssetDatabase.LoadAssetAtPath<Font>("Assets/_Project/Fonts/Fondamento-Regular.ttf"), 90, 9, GlyphRenderMode.SDFAA, 1024, 1024);
                font.name = "Opening Fondamento";
                string text = string.Join(" ", data.frames.Select(f => f.caption)) + " CHAPTER I THE THRESHOLD Next Skip Space Enter 1 / 2 3";
                if (!font.TryAddCharacters(text, out string missing)) throw new InvalidOperationException("Missing caption glyphs: " + missing);
                font.atlasPopulationMode = AtlasPopulationMode.Static;
                AssetDatabase.CreateAsset(font, fontPath);
                foreach (Texture2D atlas in font.atlasTextures) { atlas.name = "Opening Fondamento Atlas"; AssetDatabase.AddObjectToAsset(atlas, font); }
                font.material.name = "Opening Fondamento Material"; AssetDatabase.AddObjectToAsset(font.material, font);
                EditorUtility.SetDirty(font); AssetDatabase.SaveAssetIfDirty(font);
            }
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            try
            {
                var root = new GameObject("Opening Sequence", typeof(OpeningSequencePlayer), typeof(AudioSource));
                SceneManager.MoveGameObjectToScene(root, scene);
                OpeningSequencePlayer player = root.GetComponent<OpeningSequencePlayer>(); player.data = data;
                player.music = root.GetComponent<AudioSource>(); player.music.playOnAwake = false; player.music.spatialBlend = 0;
                var cameraObject = new GameObject("Opening Camera", typeof(Camera), typeof(AudioListener)); cameraObject.transform.SetParent(root.transform);
                player.storyCamera = cameraObject.GetComponent<Camera>(); player.storyCamera.clearFlags = CameraClearFlags.SolidColor;
                player.storyCamera.backgroundColor = Color.black; player.storyCamera.cullingMask = 0;
                player.storyListener = cameraObject.GetComponent<AudioListener>();
                var eventObject = new GameObject("Opening Events", typeof(EventSystem), typeof(InputSystemUIInputModule)); eventObject.transform.SetParent(root.transform);
                player.storyEvents = eventObject.GetComponent<EventSystem>(); player.storyEvents.sendNavigationEvents = false;
                var canvasObject = new GameObject("Opening Canvas", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
                canvasObject.transform.SetParent(root.transform, false);
                Canvas canvas = canvasObject.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 1000;
                var scaler = canvasObject.GetComponent<UnityEngine.UI.CanvasScaler>(); scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = 0.5f;
                var background = Image("Backdrop", canvas.transform, Vector2.zero, Vector2.one, new Color(0.035f, 0.06f, 0.07f, 1)); background.raycastTarget = true;
                player.picture = Image("Story Picture - Fit", canvas.transform, new Vector2(0.03f, 0.21f), new Vector2(0.97f, 0.94f), Color.white);
                player.picture.sprite = image; player.picture.preserveAspect = true;
                player.chapterDim = Image("Chapter Dim", canvas.transform, Vector2.zero, Vector2.one, Color.clear);
                var group = Rect("Caption Group", canvas.transform, Vector2.zero, Vector2.one); player.captionGroup = group.gameObject.AddComponent<CanvasGroup>();
                Image("Caption Divider", group, new Vector2(0.10f, 0.185f), new Vector2(0.90f, 0.187f), new Color(0.78f, 0.70f, 0.48f, 0.6f));
                player.caption = Text("Caption", group, new Vector2(0.13f, 0.045f), new Vector2(0.84f, 0.165f), font, 36);
                player.pageLabel = Text("Page", group, new Vector2(0.03f, 0.015f), new Vector2(0.11f, 0.08f), font, 22);
                player.chapterNumber = Text("Chapter Number", group, new Vector2(0.15f, 0.56f), new Vector2(0.85f, 0.66f), font, 46);
                player.chapterTitle = Text("Chapter Title", group, new Vector2(0.10f, 0.40f), new Vector2(0.90f, 0.55f), font, 80);
                player.nextButton = Button("Next", canvas.transform, new Vector2(0.87f, 0.045f), new Vector2(0.97f, 0.11f), font);
                player.skipButton = Button("Skip", canvas.transform, new Vector2(0.87f, 0.945f), new Vector2(0.97f, 0.995f), font);
                var fade = Image("Opaque Loading Fade", canvas.transform, Vector2.zero, Vector2.one, Color.black);
                player.blackFade = fade.gameObject.AddComponent<CanvasGroup>(); player.blackFade.blocksRaycasts = false;
                EditorSceneManager.SaveScene(scene, ScenePath);
            }
            finally { EditorSceneManager.CloseScene(scene, true); if (original.IsValid()) SceneManager.SetActiveScene(original); }
            var scenes = EditorBuildSettings.scenes.ToList();
            if (!scenes.Any(s => s.path == ScenePath)) { scenes.Add(new EditorBuildSettingsScene(ScenePath, true)); EditorBuildSettings.scenes = scenes.ToArray(); }
            Debug.Log("Opening scene/data built; menu routes to Opening by default. No MainMenu, Core or level scene rebuilt/saved.");
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max)
        {
            var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent, false);
            r.anchorMin = min; r.anchorMax = max; r.offsetMin = r.offsetMax = Vector2.zero; return r;
        }
        private static UnityEngine.UI.Image Image(string name, Transform parent, Vector2 min, Vector2 max, Color color)
        {
            var image = Rect(name, parent, min, max).gameObject.AddComponent<UnityEngine.UI.Image>(); image.color = color; image.raycastTarget = false; return image;
        }
        private static TMP_Text Text(string name, Transform parent, Vector2 min, Vector2 max, TMP_FontAsset font, float size)
        {
            var t = Rect(name, parent, min, max).gameObject.AddComponent<TextMeshProUGUI>(); t.font = font; t.fontSize = size;
            t.color = new Color(0.97f, 0.93f, 0.80f); t.alignment = TextAlignmentOptions.Center; t.raycastTarget = false;
            t.textWrappingMode = TextWrappingModes.Normal; t.text = ""; return t;
        }
        private static UnityEngine.UI.Button Button(string name, Transform parent, Vector2 min, Vector2 max, TMP_FontAsset font)
        {
            var image = Image(name, parent, min, max, new Color(0.15f, 0.22f, 0.22f, 1)); image.raycastTarget = true;
            var button = image.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic = image;
            var navigation = button.navigation; navigation.mode = UnityEngine.UI.Navigation.Mode.None; button.navigation = navigation;
            Text("Label", button.transform, Vector2.zero, Vector2.one, font, 28).text = name; return button;
        }
    }
}
