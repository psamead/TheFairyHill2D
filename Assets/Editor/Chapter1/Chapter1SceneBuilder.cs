using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TheFairyHills.Gameplay;
using TheFairyHills.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheFairyHills.Editor
{
    [Serializable]
    internal sealed class Chapter1LayoutData
    {
        public string id;
        public string name;
        public float length;
        public float[][][] ground;
        public Chapter1PitData[] pits;
        public Chapter1BlockData[] blocks;
        public Chapter1InteractData[] interact;
    }

    [Serializable]
    internal sealed class Chapter1PitData
    {
        public float x;
        public float w;
        public float floor;
        public string label;
    }

    [Serializable]
    internal sealed class Chapter1BlockData
    {
        public float x;
        public float w;
        public float h;
        public float @base;
        public string label;
        public string kind;
        public string sprite;
        public float visual_offset_x;
        public float visual_offset_y;
        public bool solid;
        public float collider_x;
        public float collider_w;
        public float collider_offset_x;
        public float collider_offset_y;
        public float collider_size_x;
        public float collider_size_y;
    }

    [Serializable]
    internal sealed class Chapter1InteractData
    {
        public float x;
        public float y;
        public string type;
        public string label;
        public string sprite;
    }

    /// <summary>Builds the first two playable Chapter 1 graybox scenes from their approved JSON layouts.</summary>
    internal static class Chapter1SceneBuilder
    {
        private const string VillageLayoutPath = "Assets/_Project/Data/Layouts/S1_Village_layout_v2.json";
        private const string ForestLayoutPath = "Assets/_Project/Data/Layouts/S1b_ForestPath_layout_v3.json";
        private const string NiamhSpritePath = "Assets/_Project/Art/Characters/Niamh/Niamh_side_static_v3.png";
        private const string MenuScenePath = "Assets/Scenes/MainMenu.unity";
        private const string VillageScenePath = "Assets/Scenes/Ch1_S1_Village.unity";
        private const string ForestScenePath = "Assets/Scenes/Ch1_S1b_ForestPath.unity";
        private const string FondamentoPath = "Assets/_Project/Fonts/Fondamento SDF.asset";
        private const string FramePath = "Assets/_Project/Art/UI/Frame/UI_frame_celtic.png";
        private const string SoftParticleMaterialPath = "Assets/_Project/Art/FX/M_FX_SoftParticle.mat";

        [MenuItem("Tools/The Fairy Hills/Build Chapter 1 Scenes")]
        private static void BuildFromMenu() => BuildBothScenes();

        public static void BuildBothScenes()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before building Chapter 1 scenes.");

            Scene activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.IsValid() && activeScene.isDirty)
                throw new InvalidOperationException($"Save the dirty scene '{activeScene.path}' before building Chapter 1.");

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ConfigureSpriteImports();
            Chapter1LayoutData village = LoadLayout(VillageLayoutPath);
            Chapter1LayoutData forest = LoadLayout(ForestLayoutPath);

            ConfigureRevisionAssets();
            BuildCoreScene(village.length);
            BuildScene(village, VillageScenePath, false);
            BuildScene(forest, ForestScenePath, true);
            UpdateMainMenuDestination();
            SetBuildSettings();
            AssetDatabase.SaveAssets();
            Debug.Log("Built Ch1_S1_Village and Ch1_S1b_ForestPath from the approved layout JSON files.");
        }

        private static void ConfigureRevisionAssets()
        {
            string[] spritePaths =
            {
                "Assets/_Project/Art/Backgrounds/S1_Village/props/S1_prop_gate_post_L.png",
                "Assets/_Project/Art/Backgrounds/S1_Village/props/S1_prop_gate_boards.png",
                "Assets/_Project/Art/Backgrounds/S1_Village/props/S1_prop_gate_post_R.png",
                "Assets/_Project/Art/Backgrounds/S1b_ForestPath/props/F_prop_bramble_v2.png",
                "Assets/_Project/Art/Backgrounds/S1b_ForestPath/props/F_prop_cross_v2.png",
                "Assets/_Project/Art/Backgrounds/S1b_ForestPath/props/F_prop_thornwall_v2.png",
                "Assets/_Project/Art/FX/FX_soft_dot.png",
                "Assets/_Project/Art/FX/FX_sparkle_star.png",
                FramePath,
                "Assets/_Project/Art/UI/Frame/UI_frame_celtic_stone.png"
            };
            foreach (string path in spritePaths) ConfigureSpriteImport(path, false, true);
            ConfigureSpriteImport("Assets/_Project/Art/Backgrounds/S1_Village/S1_Village_L4_Fore_v2.png", true, true);
            ConfigureSpriteImport("Assets/_Project/Art/Backgrounds/S1b_ForestPath/S1b_ForestPath_L3b_GrassEdge.png", true, true);
            TextureImporter frameImporter = AssetImporter.GetAtPath(FramePath) as TextureImporter;
            if (frameImporter == null) throw new InvalidOperationException("Celtic frame texture importer missing.");
            frameImporter.spriteBorder = new Vector4(210f, 210f, 210f, 210f);
            frameImporter.SaveAndReimport();

            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) throw new InvalidOperationException("URP Particles/Unlit shader is unavailable.");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(SoftParticleMaterialPath);
            if (material == null)
            {
                material = new Material(shader) { name = "M_FX_SoftParticle" };
                AssetDatabase.CreateAsset(material, SoftParticleMaterialPath);
            }
            else material.shader = shader;
            material.SetTexture("_BaseMap", LoadSprite("Assets/_Project/Art/FX/FX_soft_dot.png").texture);
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetColor("_BaseColor", new Color(0.055f, 0.025f, 0.07f, 0.6f));
            EditorUtility.SetDirty(material);
        }

        private static void BuildCoreScene(float initialLength)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Camera camera = CreateCamera(initialLength);
            ChapterHUD hud = CreateHud();
            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            InputSystemUIInputModule uiModule = eventSystem.GetComponent<InputSystemUIInputModule>();
            InputActionAsset uiActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/Settings/InputSystem_Actions.inputactions");
            uiModule.actionsAsset = uiActions;
            uiModule.move = LoadInputReference("MainMenu-Navigate");
            uiModule.submit = LoadInputReference("MainMenu-Submit");
            uiModule.cancel = LoadInputReference("MainMenu-Cancel");
            uiModule.point = LoadInputReference("MainMenu-Point");
            uiModule.leftClick = LoadInputReference("MainMenu-Click");
            uiModule.scrollWheel = LoadInputReference("MainMenu-ScrollWheel");
            PlayerController2D player = CreatePlayer(3.5f, initialLength, hud);
            CameraFollow(camera, player.transform, initialLength);
            GameObject stateObject = new GameObject("GameState");
            stateObject.AddComponent<GameState>();
            ChapterSceneLoader loader = new GameObject("SceneLoader").AddComponent<ChapterSceneLoader>();
            loader.Configure(player, camera.GetComponent<ChapterCameraFollow>(), hud);
            if (!EditorSceneManager.SaveScene(scene, "Assets/Scenes/Core_Persistent.unity"))
                throw new InvalidOperationException("Could not save Core_Persistent scene.");
            AssetDatabase.ImportAsset("Assets/Scenes/Core_Persistent.unity", ImportAssetOptions.ForceSynchronousImport);
        }

        private static InputActionReference LoadInputReference(string name) =>
            AssetDatabase.LoadAssetAtPath<InputActionReference>($"Assets/_Project/Art/UI/MainMenu/Actions/{name}.asset");

        private static Chapter1LayoutData LoadLayout(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("Chapter 1 layout JSON is missing.", path);
            Chapter1LayoutData layout = JsonUtility.FromJson<Chapter1LayoutData>(File.ReadAllText(path));
            if (layout != null)
                layout.ground = ParseGroundSegments(File.ReadAllText(path));
            if (layout == null || string.IsNullOrEmpty(layout.id) || layout.length <= 0f || layout.ground == null)
                throw new InvalidDataException($"Layout JSON is incomplete: {path}");
            return layout;
        }

        private static float[][][] ParseGroundSegments(string json)
        {
            Match ground = Regex.Match(json, @"""ground""\s*:\s*\[(.*?)\]\s*,\s*""blocks""", RegexOptions.Singleline);
            if (!ground.Success) throw new InvalidDataException("Could not locate ground coordinates in Chapter 1 layout JSON.");
            MatchCollection points = Regex.Matches(ground.Groups[1].Value,
                @"\[\s*(-?\d+(?:\.\d+)?)\s*,\s*(-?\d+(?:\.\d+)?)\s*\]");
            if (points.Count == 0 || points.Count % 2 != 0)
                throw new InvalidDataException("Ground coordinates must contain start/end point pairs.");
            float[][][] segments = new float[points.Count / 2][][];
            for (int i = 0; i < segments.Length; i++)
            {
                segments[i] = new[]
                {
                    new[] { float.Parse(points[i * 2].Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture),
                        float.Parse(points[i * 2].Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture) },
                    new[] { float.Parse(points[i * 2 + 1].Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture),
                        float.Parse(points[i * 2 + 1].Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture) }
                };
            }
            return segments;
        }

        private static void ConfigureSpriteImports()
        {
            string[] layerPaths =
            {
                "Assets/_Project/Art/Backgrounds/S1_Village/S1_Village_L0_Sky_wip.png",
                "Assets/_Project/Art/Backgrounds/S1_Village/S1_Village_L1_Far_wip.png",
                "Assets/_Project/Art/Backgrounds/S1_Village/S1_Village_L2_Mid_wip.png",
                "Assets/_Project/Art/Backgrounds/S1_Village/S1_Village_L3_Walk_wip.png",
                "Assets/_Project/Art/Backgrounds/S1_Village/S1_Village_L4_Fore_wip.png",
                "Assets/_Project/Art/Backgrounds/S1b_ForestPath/S1b_ForestPath_L0_Sky_wip.png",
                "Assets/_Project/Art/Backgrounds/S1b_ForestPath/S1b_ForestPath_L1_Far_wip.png",
                "Assets/_Project/Art/Backgrounds/S1b_ForestPath/S1b_ForestPath_L3_Walk_ditch_v2.png",
                "Assets/_Project/Art/Backgrounds/S1b_ForestPath/S1b_ForestPath_L4_Fore_wip.png"
            };
            foreach (string path in layerPaths) ConfigureSpriteImport(path, true, true);

            string[] propPaths =
            {
                "Assets/_Project/Art/Backgrounds/S1_Village/props/S1_prop_wall_low.png",
                "Assets/_Project/Art/Backgrounds/S1_Village/props/S1_prop_hay.png",
                "Assets/_Project/Art/Backgrounds/S1_Village/props/S1_prop_wall_tall.png",
                "Assets/_Project/Art/Backgrounds/S1_Village/props/S1_prop_gate.png",
                "Assets/_Project/Art/Backgrounds/S1b_ForestPath/props/F_prop_bramble.png",
                "Assets/_Project/Art/Backgrounds/S1b_ForestPath/props/F_prop_cross.png",
                "Assets/_Project/Art/Backgrounds/S1b_ForestPath/props/F_prop_thornwall.png"
            };
            foreach (string path in propPaths) ConfigureSpriteImport(path, false, true);
            ConfigureSpriteImport(NiamhSpritePath, false, true);
        }

        private static void ConfigureSpriteImport(string path, bool groundLinePivot, bool compressed)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException($"Texture importer not available for {path}.");

            importer.GetSourceTextureWidthAndHeight(out int width, out int height);
            Vector2 pivot = groundLinePivot ? new Vector2(0f, 270f / height) :
                path == NiamhSpritePath ? new Vector2(0.5f, 0f) : Vector2.zero;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = pivot;
            importer.SetTextureSettings(settings);
            importer.spritePixelsPerUnit = 100f;
            importer.maxTextureSize = 8192;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = compressed ? TextureImporterCompression.Compressed : TextureImporterCompression.Uncompressed;
            importer.compressionQuality = 100;
            importer.SaveAndReimport();
            Debug.Log($"Imported {path} ({width}x{height}) at 100 PPU, pivot {pivot}, max 8192.");
        }

        private static void BuildScene(Chapter1LayoutData layout, string scenePath, bool forest)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateBackdropLayers(layout, null, forest);
            CreateGroundAndPits(layout);
            LevelInfo info = new GameObject("LevelInfo").AddComponent<LevelInfo>();
            info.Configure(forest ? "Ch1_S1b_ForestPath" : "Ch1_S1_Village", layout.length,
                forest ? "MainMenu" : "Ch1_S1b_ForestPath");
            new GameObject("LevelBootstrap").AddComponent<LevelBootstrap>();
            float startX = forest ? 3.5f : 3.5f;
            foreach (Chapter1InteractData interaction in layout.interact ?? Array.Empty<Chapter1InteractData>())
                if (interaction.type == "start") startX = interaction.x;
            GameObject spawnStart = new GameObject("Spawn_Start");
            spawnStart.transform.position = new Vector3(startX, 0f, 0f);
            spawnStart.AddComponent<SpawnPoint>().Configure("Spawn_Start");
            GameObject spawnPrevious = new GameObject("Spawn_FromPrevious");
            spawnPrevious.transform.position = new Vector3(startX, 0f, 0f);
            spawnPrevious.AddComponent<SpawnPoint>().Configure("Spawn_FromPrevious");

            foreach (Chapter1BlockData block in layout.blocks ?? Array.Empty<Chapter1BlockData>())
                CreateBlock(layout.id, block);
            foreach (Chapter1InteractData interaction in layout.interact ?? Array.Empty<Chapter1InteractData>())
            {
                if (interaction.type == "reach") CreateVillageGate(interaction);
                else if (interaction.type == "pickup_low") CreateIronCross(interaction);
                else if (interaction.type == "exit") CreateSceneExit(interaction, forest);
            }

            if (!EditorSceneManager.SaveScene(scene, scenePath))
                throw new InvalidOperationException($"Could not save {scenePath}.");
            AssetDatabase.ImportAsset(scenePath, ImportAssetOptions.ForceSynchronousImport);
        }

        private static Camera CreateCamera(float sceneLength)
        {
            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(9.6f, 2.7f, -10f);
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.4f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.018f, 0.028f, 0.04f, 1f);
            cameraObject.AddComponent<ChapterCameraFollow>();
            return camera;
        }

        private static void CameraFollow(Camera camera, Transform player, float sceneLength)
        {
            camera.GetComponent<ChapterCameraFollow>().Configure(player, camera, sceneLength);
        }

        private static void CreateBackdropLayers(Chapter1LayoutData layout, Camera camera, bool forest)
        {
            if (forest)
            {
                CreateLayer("L0_Sky", "Assets/_Project/Art/Backgrounds/S1b_ForestPath/S1b_ForestPath_L0_Sky_wip.png", -50, 0.05f, camera);
                CreateLayer("L1_Far", "Assets/_Project/Art/Backgrounds/S1b_ForestPath/S1b_ForestPath_L1_Far_wip.png", -40, 0.4f, camera);
                CreateLayer("L3_Walk", "Assets/_Project/Art/Backgrounds/S1b_ForestPath/S1b_ForestPath_L3_Walk_ditch_v2.png", -10, 1f, camera);
                CreateLayer("L3b_GrassEdge", "Assets/_Project/Art/Backgrounds/S1b_ForestPath/S1b_ForestPath_L3b_GrassEdge.png", -9, 1f, camera);
                CreateLayer("L4_Fore", "Assets/_Project/Art/Backgrounds/S1b_ForestPath/S1b_ForestPath_L4_Fore_wip.png", 20, 1.15f, camera, 0.55f);
            }
            else
            {
                CreateLayer("L0_Sky", "Assets/_Project/Art/Backgrounds/S1_Village/S1_Village_L0_Sky_wip.png", -50, 0.05f, camera);
                CreateLayer("L1_Far", "Assets/_Project/Art/Backgrounds/S1_Village/S1_Village_L1_Far_wip.png", -40, 0.2f, camera);
                CreateLayer("L2_Mid", "Assets/_Project/Art/Backgrounds/S1_Village/S1_Village_L2_Mid_wip.png", -30, 0.5f, camera);
                CreateLayer("L3_Walk", "Assets/_Project/Art/Backgrounds/S1_Village/S1_Village_L3_Walk_wip.png", -10, 1f, camera);
                CreateLayer("L4_Fore", "Assets/_Project/Art/Backgrounds/S1_Village/S1_Village_L4_Fore_v2.png", 20, 1.3f, camera);
            }
        }

        private static void CreateLayer(string name, string path, int order, float factor, Camera camera, float alpha = 1f)
        {
            Sprite sprite = LoadSprite(path);
            GameObject layer = new GameObject(name);
            layer.transform.position = Vector3.zero;
            SpriteRenderer renderer = layer.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
            Color layerColor = renderer.color;
            layerColor.a = alpha;
            renderer.color = layerColor;
            layer.AddComponent<ParallaxLayer>().Configure(camera, factor);
        }

        private static void CreateGroundAndPits(Chapter1LayoutData layout)
        {
            GameObject groundRoot = new GameObject("Graybox Ground");
            for (int i = 0; i < layout.ground.Length; i++)
            {
                float[][] endpoints = layout.ground[i];
                if (endpoints == null || endpoints.Length < 2 || endpoints[0].Length < 2 || endpoints[1].Length < 2)
                    throw new InvalidDataException($"Ground segment {i} in {layout.id} is malformed.");
                Vector2 start = new Vector2(endpoints[0][0], endpoints[0][1]);
                Vector2 end = new Vector2(endpoints[1][0], endpoints[1][1]);
                GameObject segment = new GameObject($"Ground_{start.x:0.0}_to_{end.x:0.0}");
                segment.transform.SetParent(groundRoot.transform, false);
                segment.transform.position = start;
                BoxCollider2D collider = segment.AddComponent<BoxCollider2D>();
                if (Mathf.Abs(start.y - end.y) < 0.001f)
                {
                    collider.size = new Vector2(end.x - start.x, 1f);
                    collider.offset = new Vector2((end.x - start.x) * 0.5f, -0.5f);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(collider);
                    EdgeCollider2D edge = segment.AddComponent<EdgeCollider2D>();
                    edge.points = new[] { Vector2.zero, end - start };
                    segment.transform.position = start;
                }
            }

            foreach (Chapter1PitData pit in layout.pits ?? Array.Empty<Chapter1PitData>())
            {
                GameObject bottom = new GameObject(pit.x < 31f ? "PitFloor_SmallGap_1.4u_WalkJump" : "PitFloor_LargeGap_2.6u_RunJump");
                BoxCollider2D collider = bottom.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(pit.w, 0.5f);
                collider.offset = new Vector2(pit.w * 0.5f, -0.25f);
                bottom.transform.position = new Vector3(pit.x, pit.floor, 0f);
            }
        }

        private static PlayerController2D CreatePlayer(float startX, float sceneLength, ChapterHUD hud)
        {
            Sprite niamh = LoadSprite(NiamhSpritePath);
            GameObject playerObject = new GameObject("Niamh", typeof(Rigidbody2D), typeof(CapsuleCollider2D));
            playerObject.transform.position = new Vector3(startX, 0f, 0f);
            Rigidbody2D body = playerObject.GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 0f;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            CapsuleCollider2D capsule = playerObject.GetComponent<CapsuleCollider2D>();
            capsule.direction = CapsuleDirection2D.Vertical;
            capsule.size = new Vector2(0.7f, 1.9f);
            capsule.offset = new Vector2(0f, 0.95f);

            GameObject visual = new GameObject("Niamh Static Sprite");
            visual.transform.SetParent(playerObject.transform, false);
            visual.transform.localScale = Vector3.one * 0.30f;
            SpriteRenderer spriteRenderer = visual.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = niamh;
            spriteRenderer.sortingOrder = 10;

            PlayerController2D controller = playerObject.AddComponent<PlayerController2D>();
            controller.Configure(spriteRenderer, hud, 0f, sceneLength);
            return controller;
        }

        private static ChapterHUD CreateHud()
        {
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FondamentoPath);
            if (font == null) throw new InvalidOperationException($"Fondamento TMP asset missing at {FondamentoPath}.");
            GameObject canvasObject = new GameObject("Gameplay HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            TMP_Text controls = CreateHudText("Controls", canvasObject.transform, font,
                "A / D or ← / → move     Shift run     Space jump     E interact     Q IronPulse", 24f,
                TextAlignmentOptions.TopLeft);
            SetTopLeft(controls.rectTransform, 36f, 30f, 1450f, 48f);
            controls.color = new Color(0.95f, 0.89f, 0.73f, 0.92f);
            CreateUiFramePanel(canvasObject.transform, "ControlsFrame", Vector2.up, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(730f, -52f), new Vector2(1500f, 82f), false);

            TMP_Text prompt = CreateHudText("Interaction Prompt", canvasObject.transform, font, string.Empty, 30f,
                TextAlignmentOptions.Center);
            SetAnchored(prompt.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(0f, 85f), new Vector2(900f, 55f));
            prompt.color = new Color(0.98f, 0.86f, 0.58f, 1f);
            GameObject promptPanel = CreateUiFramePanel(canvasObject.transform, "PromptFrame", new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 62f), new Vector2(970f, 88f), true);

            TMP_Text message = CreateHudText("Chapter Message", canvasObject.transform, font, string.Empty, 32f,
                TextAlignmentOptions.Center);
            SetAnchored(message.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, -360f), new Vector2(1300f, 100f));
            message.color = new Color(0.98f, 0.86f, 0.58f, 1f);
            GameObject messagePanel = CreateUiFramePanel(canvasObject.transform, "MessageFrame", new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -360f), new Vector2(1370f, 140f), true);

            ChapterHUD hud = canvasObject.AddComponent<ChapterHUD>();
            hud.Configure(prompt, message, promptPanel, messagePanel);
            return hud;
        }

        private static GameObject CreateUiFramePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 pivot, Vector2 position, Vector2 size, bool initiallyHidden)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform));
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            SetAnchored(rect, anchorMin, anchorMax, pivot, position, size);
            GameObject dark = new GameObject("Dark Backing", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            dark.transform.SetParent(panel.transform, false);
            StretchRect(dark.GetComponent<RectTransform>());
            Image darkImage = dark.GetComponent<Image>();
            darkImage.color = new Color(0.01f, 0.025f, 0.045f, 0.66f);
            darkImage.raycastTarget = false;
            GameObject frame = new GameObject("Celtic Frame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            frame.transform.SetParent(panel.transform, false);
            StretchRect(frame.GetComponent<RectTransform>());
            Image frameImage = frame.GetComponent<Image>();
            frameImage.sprite = LoadSprite(FramePath);
            frameImage.type = Image.Type.Tiled;
            frameImage.fillCenter = false;
            frameImage.pixelsPerUnitMultiplier = 3.5f;
            frameImage.raycastTarget = false;
            panel.SetActive(!initiallyHidden);
            return panel;
        }

        private static void StretchRect(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static TMP_Text CreateHudText(string name, Transform parent, TMP_FontAsset font, string text,
            float size, TextAlignmentOptions alignment)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            TextMeshProUGUI textComponent = textObject.GetComponent<TextMeshProUGUI>();
            textComponent.font = font;
            textComponent.text = text;
            textComponent.fontSize = size;
            textComponent.alignment = alignment;
            textComponent.textWrappingMode = TextWrappingModes.NoWrap;
            textComponent.raycastTarget = false;
            return textComponent;
        }

        private static void CreateBlock(string layoutId, Chapter1BlockData block)
        {
            string folder = layoutId == "S1_Village" ?
                "Assets/_Project/Art/Backgrounds/S1_Village/" : "Assets/_Project/Art/Backgrounds/S1b_ForestPath/";
            Sprite sprite = LoadSprite(folder + block.sprite);
            GameObject root = new GameObject(block.label);
            root.transform.position = new Vector3(block.x, block.@base, 0f);
            SpriteRenderer renderer = CreateScaledSprite(root.transform, sprite, block.w, block.h, 0);
            renderer.transform.localPosition = new Vector3(block.visual_offset_x, block.visual_offset_y, 0f);

            bool isThorn = block.kind == "thorn";
            bool solid = string.IsNullOrEmpty(block.kind) || block.solid;
            root.name = EnglishSceneName(block.label);
            BoxCollider2D collider = null;
            if (solid && !isThorn)
            {
                collider = root.AddComponent<BoxCollider2D>();
                if (block.collider_size_x > 0f && block.collider_size_y > 0f)
                {
                    collider.size = new Vector2(block.collider_size_x, block.collider_size_y);
                    collider.offset = new Vector2(block.collider_offset_x, block.collider_offset_y);
                }
                else
                {
                    collider.size = new Vector2(block.w, block.h);
                    collider.offset = new Vector2(block.w * 0.5f, block.h * 0.5f);
                }
            }

            if (isThorn)
            {
                BoxCollider2D barrier = root.AddComponent<BoxCollider2D>();
                float barrierWidth = block.collider_w > 0f ? block.collider_w : 0.08f;
                float barrierLocalX = block.collider_w > 0f ? block.collider_x - block.x : 0.04f;
                barrier.size = new Vector2(barrierWidth, block.h);
                barrier.offset = new Vector2(barrierLocalX + barrierWidth * 0.5f, block.h * 0.5f);
                ParticleSystem smoke = CreateSmoke(root.transform, new Vector3(block.w * 0.5f, block.h * 0.7f, -0.1f));
                root.AddComponent<ThornWall>().Configure(renderer, barrier, smoke);
            }
        }

        private static string EnglishSceneName(string label)
        {
            if (label == "HayBale_0.9u") return label;
            if (label.Contains("StoneWall_Low")) return "StoneWall_Low_1.2u";
            if (label.Contains("StoneWall_Tall")) return "StoneWall_Tall_1.8u";
            if (label.Contains("荆棘丛")) return "BrambleCluster_Decorative";
            if (label.Contains("受控荆棘墙")) return "ThornWall_IronPulseTarget";
            return label;
        }

        private static void CreateVillageGate(Chapter1InteractData interaction)
        {
            GameObject root = new GameObject("Village Gate");
            root.transform.position = new Vector3(24f, interaction.y, 0f);
            SpriteRenderer left = CreateGatePart(root.transform, "Gate Post Left", "S1_prop_gate_post_L.png", 0f, -5, 1.12f);
            SpriteRenderer boards = CreateGatePart(root.transform, "Gate Boards", "S1_prop_gate_boards.png", 1.04f, -5, 2.88f);
            CreateGatePart(root.transform, "Gate Post Right", "S1_prop_gate_post_R.png", 3.84f, 15, 0.98f);

            BoxCollider2D blocker = root.AddComponent<BoxCollider2D>();
            blocker.size = new Vector2(0.3f, 8f);
            blocker.offset = new Vector2(2.5f, 4f);
            VillageGate gate = root.AddComponent<VillageGate>();
            gate.Configure(boards, blocker);

            GameObject range = new GameObject("Gate Close Range");
            range.transform.SetParent(root.transform, false);
            range.transform.localPosition = new Vector3(2.5f, 4f, 0f);
            BoxCollider2D trigger = range.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(8.8f, 8f);
            range.AddComponent<GateRangeTrigger>().Configure(gate);
        }

        private static SpriteRenderer CreateGatePart(Transform parent, string name, string file, float x, int order, float width)
        {
            GameObject part = new GameObject(name);
            part.transform.SetParent(parent, false);
            part.transform.localPosition = new Vector3(x, 0f, 0f);
            SpriteRenderer renderer = CreateScaledSprite(part.transform,
                LoadSprite("Assets/_Project/Art/Backgrounds/S1_Village/props/" + file), width, 2.4f, order);
            return renderer;
        }

        private static void CreateIronCross(Chapter1InteractData interaction)
        {
            string path = "Assets/_Project/Art/Backgrounds/S1b_ForestPath/props/F_prop_cross_v2.png";
            GameObject pickup = new GameObject("IronCrossPickup_FamilyHeirloom");
            pickup.transform.position = new Vector3(interaction.x, interaction.y + 0.03f, -0.1f);
            SpriteRenderer cross = CreateScaledSprite(pickup.transform, LoadSprite(path), 1.2f, 1f, 2);
            cross.transform.localPosition = new Vector3(-0.6f, 0f, 0f);
            BoxCollider2D trigger = pickup.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(1.3f, 1.2f);
            trigger.offset = new Vector2(0f, 0.6f);
            ParticleSystem stars = CreateSparkleHint(pickup.transform);
            pickup.AddComponent<IronCrossPickup>().Configure(cross, stars, 2.5f);
        }

        private static ParticleSystem CreateSparkleHint(Transform parent)
        {
            GameObject starsObject = new GameObject("Iron Cross Hint Stars");
            starsObject.transform.SetParent(parent, false);
            starsObject.transform.localPosition = new Vector3(0f, 0.25f, -0.15f);
            ParticleSystem stars = starsObject.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = stars.main;
            main.loop = true;
            main.playOnAwake = false;
            main.startLifetime = 0.6f;
            main.startSpeed = 0.05f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.16f);
            main.startColor = new Color(1f, 0.86f, 0.52f, 0.95f);
            main.maxParticles = 10;
            ParticleSystem.EmissionModule emission = stars.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 2, 3, 1, 0.45f) });
            ParticleSystem.ShapeModule shape = stars.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.6f;
            shape.radiusThickness = 0f;
            ParticleSystemRenderer renderer = stars.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sharedMaterial = GetSparkleMaterial();
            return stars;
        }

        private static Material GetSparkleMaterial()
        {
            const string path = "Assets/_Project/Art/FX/M_FX_SparkleAdditive.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit")) { name = "M_FX_SparkleAdditive" };
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetTexture("_BaseMap", LoadSprite("Assets/_Project/Art/FX/FX_sparkle_star.png").texture);
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 2f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void CreateSceneExit(Chapter1InteractData interaction, bool forest)
        {
            GameObject exit = new GameObject(forest ? "Exit_To_S2" : "Exit_To_ForestPath");
            exit.transform.position = new Vector3(interaction.x, interaction.y + 1.2f, 0f);
            BoxCollider2D trigger = exit.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = forest ? new Vector2(2f, 2.4f) : new Vector2(1.4f, 2.4f);
            ChapterSceneExit sceneExit = exit.AddComponent<ChapterSceneExit>();
            sceneExit.Configure(forest ? string.Empty : "Ch1_S1b_ForestPath", forest, forest, "Spawn_FromVillage");
        }

        private static ParticleSystem CreateSmoke(Transform parent, Vector3 localPosition)
        {
            GameObject smokeObject = new GameObject("Black Smoke Placeholder");
            smokeObject.transform.SetParent(parent, false);
            smokeObject.transform.localPosition = localPosition;
            ParticleSystem particles = smokeObject.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.duration = 0.8f;
            main.startLifetime = 0.7f;
            main.startSpeed = 0.7f;
            main.startSize = 0.26f;
            main.startColor = new Color(0.055f, 0.025f, 0.07f, 0.86f);
            main.maxParticles = 32;
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 14) });
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 22f;
            shape.radius = 0.18f;
            ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.y = new ParticleSystem.MinMaxCurve(0.9f);
            particles.GetComponent<ParticleSystemRenderer>().sharedMaterial =
                AssetDatabase.LoadAssetAtPath<Material>(SoftParticleMaterialPath);
            return particles;
        }

        private static SpriteRenderer CreateScaledSprite(Transform parent, Sprite sprite, float width, float height, int order)
        {
            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(parent, false);
            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
            Vector2 size = sprite.bounds.size;
            visual.transform.localScale = new Vector3(width / size.x, height / size.y, 1f);
            return renderer;
        }

        private static void UpdateMainMenuDestination()
        {
            Scene menu = EditorSceneManager.OpenScene(MenuScenePath, OpenSceneMode.Single);
            MainMenuController controller = UnityEngine.Object.FindFirstObjectByType<MainMenuController>();
            if (controller == null) throw new InvalidOperationException("MainMenuController is missing from MainMenu.");
            SerializedObject serialized = new SerializedObject(controller);
            SerializedProperty destination = serialized.FindProperty("firstGameplaySceneName");
            if (destination == null) throw new InvalidOperationException("MainMenu Start destination field is missing.");
            destination.stringValue = "Ch1_S1_Village";
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(menu);
            if (!EditorSceneManager.SaveScene(menu)) throw new InvalidOperationException("Could not save MainMenu Start destination.");
        }

        private static void SetBuildSettings()
        {
            var ordered = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(MenuScenePath, true),
                new EditorBuildSettingsScene("Assets/Scenes/Core_Persistent.unity", true),
                new EditorBuildSettingsScene(VillageScenePath, true),
                new EditorBuildSettingsScene(ForestScenePath, true)
            };
            foreach (EditorBuildSettingsScene current in EditorBuildSettings.scenes)
            {
                if (string.Equals(current.path, MenuScenePath, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(current.path, "Assets/Scenes/Core_Persistent.unity", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(current.path, VillageScenePath, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(current.path, ForestScenePath, StringComparison.OrdinalIgnoreCase)) continue;
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(current.path) == null) continue;
                ordered.Add(current);
            }
            EditorBuildSettings.scenes = ordered.ToArray();
        }

        private static Sprite LoadSprite(string path)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) throw new InvalidOperationException($"Could not load sprite at {path}.");
            return sprite;
        }

        private static void SetTopLeft(RectTransform rect, float x, float top, float width, float height)
        {
            SetAnchored(rect, Vector2.up, Vector2.up, new Vector2(0f, 1f), new Vector2(x, -top), new Vector2(width, height));
        }

        private static void SetAnchored(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }
    }
}
