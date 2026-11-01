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
using UnityEngine.Rendering.Universal;

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
        public Chapter1ForestLayers layers;
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
        public string glow;
        public float sorting_order;
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
        public string sprite_open;
        public float image_x;
        public float image_w;
        public float image_h;
        public float sorting_order;
        public float blocker_x0;
        public float blocker_x1;
        public float blocker_h;
        public float range_x0;
        public float range_x1;
        public float hint_trigger_x;
    }

    [Serializable]
    internal sealed class Chapter1ForestLayers
    {
        public Chapter1ForestLayerData L2_undergrowth;
        public Chapter1ForestLayerData L3b_grass_edge;
        public Chapter1ForestLayerData L4_fore;
    }

    [Serializable]
    internal sealed class Chapter1ForestLayerData
    {
        public string sprite;
        public float parallax;
        public int sorting_order;
        public float alpha = 1f;
    }

    /// <summary>Builds the first two playable Chapter 1 graybox scenes from their approved JSON layouts.</summary>
    internal static class Chapter1SceneBuilder
    {
        private const string VillageLayoutPath = "Assets/_Project/Data/Layouts/S1_Village_layout_v3.json";
        private const string ForestLayoutPath = "Assets/_Project/Data/Layouts/S1b_ForestPath_layout_v4.json";
        private const string NiamhSpritePath = "Assets/_Project/Art/Characters/Niamh/Niamh_side_static_v3.png";
        private const string MenuScenePath = "Assets/Scenes/MainMenu.unity";
        private const string VillageScenePath = "Assets/Scenes/Ch1_S1_Village.unity";
        private const string ForestScenePath = "Assets/Scenes/Ch1_S1b_ForestPath.unity";
        private const string FondamentoPath = "Assets/_Project/Fonts/Fondamento SDF.asset";
        private const string FramePath = "Assets/_Project/Art/UI/Frame/UI_frame_celtic.png";
        private const string SoftParticleMaterialPath = "Assets/_Project/Art/FX/M_FX_SoftParticle.mat";
        private const string HudFramePath = "Assets/_Project/Art/UI/HUD/UI_frame_thin.png";
        private const string HudPanelPath = "Assets/_Project/Art/UI/HUD/UI_panel_dark.png";
        private const string EmberMaterialPath = "Assets/_Project/Art/FX/M_FX_EmberAdditive.mat";

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

        [MenuItem("Tools/The Fairy Hills/Build Revision v3 Batch A - Village UI")]
        private static void BuildRevisionV3BatchAVillageAndCore()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before building Revision v3 Batch A.");

            Scene activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.IsValid() && activeScene.isDirty)
                throw new InvalidOperationException($"Save the dirty scene '{activeScene.path}' before building Revision v3 Batch A.");

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ConfigureBatchAVillageAssets();
            Chapter1LayoutData village = LoadLayout(VillageLayoutPath);
            BuildCoreScene(village.length);
            BuildScene(village, VillageScenePath, false);
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(MenuScenePath, OpenSceneMode.Single);
            Debug.Log("Built Revision v3 Batch A: Core_Persistent HUD and S1 Village gate. Forest, MainMenu, and Build Settings were left unchanged.");
        }

        [MenuItem("Tools/The Fairy Hills/Build Revision v3 Batch B - Forest Visuals")]
        private static void BuildRevisionV3BatchBForest()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before building Revision v3 Batch B.");

            Scene activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.IsValid() && activeScene.isDirty)
                throw new InvalidOperationException($"Save the dirty scene '{activeScene.path}' before building Revision v3 Batch B.");

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ConfigureBatchBForestAssets();
            Chapter1LayoutData forest = LoadLayout(ForestLayoutPath);
            BuildScene(forest, ForestScenePath, true);
            AssetDatabase.SaveAssets();
            Debug.Log("Built Revision v3 Batch B: Forest visuals only. Core, Village, MainMenu, and Build Settings were left unchanged.");
        }

        private static void ConfigureBatchBForestAssets()
        {
            const string forestFolder = "Assets/_Project/Art/Backgrounds/S1b_ForestPath/";
            string[] layerPaths =
            {
                forestFolder + "S1b_ForestPath_L2_Undergrowth.png",
                forestFolder + "S1b_ForestPath_L3b_GrassEdge_v3.png",
                forestFolder + "S1b_ForestPath_L4_Fore_v2.png"
            };
            foreach (string path in layerPaths) ConfigureSpriteImport(path, true, false);

            string[] propPaths =
            {
                forestFolder + "props/F_prop_bramble_v3.png",
                forestFolder + "props/F_prop_thornwall_v3.png",
                forestFolder + "props/F_prop_thornwall_v3_glow.png",
                forestFolder + "props/F_prop_cross_v2.png",
                "Assets/_Project/Art/FX/FX_sparkle_star.png",
                "Assets/_Project/Art/FX/FX_soft_dot.png"
            };
            foreach (string path in propPaths) ConfigureSpriteImport(path, false, false);
            ConfigureForestParticleMaterial();
            ConfigureEmberMaterial();
        }

        private static void ConfigureForestParticleMaterial()
        {
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
            material.SetColor("_BaseColor", Color.white);
            EditorUtility.SetDirty(material);
        }

        private static void ConfigureEmberMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) throw new InvalidOperationException("URP Particles/Unlit shader is unavailable.");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(EmberMaterialPath);
            if (material == null)
            {
                material = new Material(shader) { name = "M_FX_EmberAdditive" };
                AssetDatabase.CreateAsset(material, EmberMaterialPath);
            }
            else material.shader = shader;
            material.SetTexture("_BaseMap", LoadSprite("Assets/_Project/Art/FX/FX_soft_dot.png").texture);
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 2f);
            material.SetColor("_BaseColor", Color.white);
            EditorUtility.SetDirty(material);
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
            ConfigureBatchAVillageAssets();
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

        private static void ConfigureBatchAVillageAssets()
        {
            string[] spritePaths =
            {
                HudFramePath,
                HudPanelPath,
                "Assets/_Project/Art/Backgrounds/S1_Village/props/S1_prop_gate_closed_v3.png",
                "Assets/_Project/Art/Backgrounds/S1_Village/props/S1_prop_gate_open_v3.png"
            };
            foreach (string path in spritePaths)
                ConfigureSpriteImport(path, false, false);

            foreach (string path in new[] { HudFramePath, HudPanelPath })
            {
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) throw new InvalidOperationException($"HUD texture importer missing at {path}.");
                importer.spriteBorder = new Vector4(24f, 24f, 24f, 24f);
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }

        private static void BuildCoreScene(float initialLength)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Camera camera = CreateCamera(initialLength);
            CreateGlobalLight2D();
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

        private static void CreateGlobalLight2D()
        {
            GameObject lightObject = new GameObject("Global Light 2D");
            Light2D light = lightObject.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.color = Color.white;
            light.intensity = 1f;
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
                Chapter1ForestLayers layers = layout.layers;
                if (layers == null || layers.L2_undergrowth == null || layers.L3b_grass_edge == null || layers.L4_fore == null)
                    throw new InvalidDataException("Forest v4 layout is missing its layer definitions.");
                string folder = "Assets/_Project/Art/Backgrounds/S1b_ForestPath/";
                CreateLayer("L2_Undergrowth", folder + layers.L2_undergrowth.sprite,
                    LayerOrder(layers.L2_undergrowth.sorting_order, -20), layers.L2_undergrowth.parallax, camera, layers.L2_undergrowth.alpha);
                CreateLayer("L3_Walk", "Assets/_Project/Art/Backgrounds/S1b_ForestPath/S1b_ForestPath_L3_Walk_ditch_v2.png", -10, 1f, camera);
                CreateLayer("L3b_GrassEdge", folder + layers.L3b_grass_edge.sprite,
                    LayerOrder(layers.L3b_grass_edge.sorting_order, -9), layers.L3b_grass_edge.parallax, camera, layers.L3b_grass_edge.alpha);
                CreateLayer("L4_Fore", folder + layers.L4_fore.sprite,
                    LayerOrder(layers.L4_fore.sorting_order, 20), layers.L4_fore.parallax, camera, layers.L4_fore.alpha);
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

        private static int LayerOrder(int requestedOrder, int fallback) => requestedOrder == 0 ? fallback : requestedOrder;

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
            // Preserve the manually tuned capsule saved in Core_Persistent.unity.
            capsule.size = new Vector2(0.23f, 2.08f);
            capsule.offset = new Vector2(0.08f, 1.04f);

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
                TextAlignmentOptions.Center);
            controls.color = new Color(0.95f, 0.89f, 0.73f, 0.92f);
            Vector2 controlsSize = controls.GetPreferredValues(controls.text);
            GameObject controlsPanel = CreateUiFramePanel(canvasObject.transform, "ControlsFrame", Vector2.up,
                Vector2.up, new Vector2(0f, 1f), new Vector2(24f, -24f), controlsSize + new Vector2(24f, 24f), false);
            controls.transform.SetParent(controlsPanel.transform, false);
            SetAnchored(controls.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), Vector2.zero, controlsSize);

            GameObject promptPanel = CreateUiFramePanel(canvasObject.transform, "PromptFrame", new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(0f, 85f), new Vector2(24f, 24f), true);
            TMP_Text prompt = CreateHudText("Interaction Prompt", promptPanel.transform, font, string.Empty, 30f,
                TextAlignmentOptions.Center);
            SetAnchored(prompt.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.one);
            prompt.color = new Color(0.98f, 0.86f, 0.58f, 1f);

            GameObject messagePanel = CreateUiFramePanel(canvasObject.transform, "MessageFrame", new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -360f), new Vector2(24f, 24f), true);
            TMP_Text message = CreateHudText("Chapter Message", messagePanel.transform, font, string.Empty, 32f,
                TextAlignmentOptions.Center);
            SetAnchored(message.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.one);
            message.color = new Color(0.98f, 0.86f, 0.58f, 1f);

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
            darkImage.sprite = LoadSprite(HudPanelPath);
            darkImage.type = Image.Type.Sliced;
            darkImage.color = new Color(1f, 1f, 1f, 0.6f);
            darkImage.raycastTarget = false;
            GameObject frame = new GameObject("Thin HUD Frame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            frame.transform.SetParent(panel.transform, false);
            StretchRect(frame.GetComponent<RectTransform>());
            Image frameImage = frame.GetComponent<Image>();
            frameImage.sprite = LoadSprite(HudFramePath);
            frameImage.type = Image.Type.Sliced;
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
            bool isThorn = block.kind == "thorn";
            int order = Mathf.RoundToInt(block.sorting_order != 0f ? block.sorting_order : isThorn ? 10f : 0f);
            SpriteRenderer renderer = CreateScaledSprite(root.transform, sprite, block.w, block.h, order);
            renderer.transform.localPosition = new Vector3(block.visual_offset_x, block.visual_offset_y, 0f);

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
                Vector3 effectCenter = new Vector3(barrierLocalX + barrierWidth * 0.5f, block.h * 0.5f, -0.1f);
                ParticleSystem smoke = CreateSmoke(root.transform, effectCenter, new Vector2(barrierWidth, block.h));
                SpriteRenderer glow = null;
                Light2D redLight = null;
                if (!string.IsNullOrEmpty(block.glow))
                {
                    Sprite glowSprite = LoadSprite(folder + block.glow);
                    glow = CreateScaledSprite(root.transform, glowSprite, block.w, block.h, order + 1);
                    glow.gameObject.name = "Red Glow Overlay";
                    glow.sharedMaterial = GetThornGlowMaterial(glowSprite.texture);
                    glow.color = new Color32(176, 32, 42, 128);
                    GameObject lightObject = new GameObject("Thorn Wall Red Point Light");
                    lightObject.transform.SetParent(root.transform, false);
                    lightObject.transform.localPosition = effectCenter;
                    redLight = lightObject.AddComponent<Light2D>();
                    redLight.lightType = Light2D.LightType.Point;
                    redLight.color = new Color32(176, 32, 42, 255);
                    redLight.pointLightOuterRadius = 6f;
                    redLight.intensity = 0.45f;
                    redLight.shadowsEnabled = false;
                }
                root.AddComponent<ThornWall>().Configure(renderer, barrier, smoke, glow, redLight);
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
            root.transform.position = new Vector3(interaction.x, interaction.y, 0f);
            string folder = "Assets/_Project/Art/Backgrounds/S1_Village/";
            Sprite closedSprite = LoadSprite(folder + interaction.sprite);
            Sprite openSprite = LoadSprite(folder + interaction.sprite_open);
            SpriteRenderer gateRenderer = CreateScaledSprite(root.transform, closedSprite,
                interaction.image_w, interaction.image_h, Mathf.RoundToInt(interaction.sorting_order));
            gateRenderer.transform.localPosition = new Vector3(interaction.image_x - interaction.x, 0f, 0f);

            BoxCollider2D blocker = root.AddComponent<BoxCollider2D>();
            blocker.size = new Vector2(interaction.blocker_x1 - interaction.blocker_x0, interaction.blocker_h);
            blocker.offset = new Vector2((interaction.blocker_x0 + interaction.blocker_x1) * 0.5f - interaction.x,
                interaction.blocker_h * 0.5f);
            VillageGate gate = root.AddComponent<VillageGate>();
            gate.Configure(gateRenderer, closedSprite, openSprite, blocker);

            GameObject range = new GameObject("Gate Close Range");
            range.transform.SetParent(root.transform, false);
            range.transform.localPosition = new Vector3((interaction.range_x0 + interaction.range_x1) * 0.5f - interaction.x,
                interaction.blocker_h * 0.5f, 0f);
            BoxCollider2D trigger = range.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(interaction.range_x1 - interaction.range_x0, interaction.blocker_h);
            range.AddComponent<GateRangeTrigger>().Configure(gate);
        }

        private static void CreateIronCross(Chapter1InteractData interaction)
        {
            string path = "Assets/_Project/Art/Backgrounds/S1b_ForestPath/props/F_prop_cross_v2.png";
            GameObject pickup = new GameObject("IronCrossPickup_FamilyHeirloom");
            pickup.transform.position = new Vector3(interaction.x, interaction.y + 0.03f, -0.1f);
            SpriteRenderer cross = CreateScaledSprite(pickup.transform, LoadSprite(path), 1.2f, 1f,
                interaction.sorting_order == 0f ? 11 : Mathf.RoundToInt(interaction.sorting_order));
            cross.transform.localPosition = new Vector3(-0.6f, 0f, 0f);
            BoxCollider2D trigger = pickup.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(1.3f, 1.2f);
            trigger.offset = new Vector2(0f, 0.6f);
            ParticleSystem stars = CreateSparkleHint(pickup.transform);
            SpriteRenderer mainStar = CreateMainStar(pickup.transform, interaction.sorting_order == 0f ? 11 :
                Mathf.RoundToInt(interaction.sorting_order));
            pickup.AddComponent<IronCrossPickup>().Configure(cross, stars, mainStar, 2.5f, interaction.hint_trigger_x);
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
            main.startLifetime = 0.5f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.02f, 0.12f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.4f, 0.7f);
            main.startColor = new Color(1f, 0.86f, 0.52f, 0.95f);
            main.maxParticles = 36;
            ParticleSystem.EmissionModule emission = stars.emission;
            emission.rateOverTime = new ParticleSystem.MinMaxCurve(4f, 6f);
            ParticleSystem.ShapeModule shape = stars.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 1.2f;
            shape.radiusThickness = 0f;
            ParticleSystem.ColorOverLifetimeModule color = stars.colorOverLifetime;
            color.enabled = true;
            color.color = new ParticleSystem.MinMaxGradient(CreateAlphaFadeGradient(new Color32(255, 220, 150, 255)));
            ParticleSystem.SizeOverLifetimeModule size = stars.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 0.65f), new Keyframe(1f, 1.5f)));
            ParticleSystemRenderer renderer = stars.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = 13;
            renderer.sharedMaterial = GetSparkleMaterial();
            return stars;
        }

        private static SpriteRenderer CreateMainStar(Transform parent, int crossOrder)
        {
            GameObject starObject = new GameObject("Iron Cross Main Star");
            starObject.transform.SetParent(parent, false);
            starObject.transform.localPosition = new Vector3(0f, 0.55f, -0.15f);
            SpriteRenderer renderer = starObject.AddComponent<SpriteRenderer>();
            renderer.sprite = LoadSprite("Assets/_Project/Art/FX/FX_sparkle_star.png");
            renderer.sortingOrder = crossOrder + 2;
            renderer.sharedMaterial = GetSparkleMaterial();
            renderer.color = new Color32(255, 224, 157, 255);
            Vector2 size = renderer.sprite.bounds.size;
            starObject.transform.localScale = new Vector3(0.8f / size.x, 0.8f / size.y, 1f);
            starObject.SetActive(false);
            return renderer;
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

        private static ParticleSystem CreateSmoke(Transform parent, Vector3 localPosition, Vector2 area)
        {
            GameObject smokeObject = new GameObject("Thorn Wall Purple Smoke");
            smokeObject.transform.SetParent(parent, false);
            smokeObject.transform.localPosition = localPosition;
            ParticleSystem particles = smokeObject.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.duration = 1.2f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.5f, 2.5f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(1f, 2.5f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color32(26, 14, 34, 220));
            main.maxParticles = 180;
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 40f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 80) });
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(area.x, area.y, 0.1f);
            ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.y = new ParticleSystem.MinMaxCurve(1.5f, 3f);
            ParticleSystem.NoiseModule noise = particles.noise;
            noise.enabled = true;
            noise.strength = new ParticleSystem.MinMaxCurve(0.5f, 1f);
            noise.frequency = 0.5f;
            ParticleSystem.ColorOverLifetimeModule color = particles.colorOverLifetime;
            color.enabled = true;
            color.color = new ParticleSystem.MinMaxGradient(CreateAlphaFadeGradient(new Color32(26, 14, 34, 230)));
            ParticleSystem.SizeOverLifetimeModule size = particles.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1.5f)));
            ParticleSystemRenderer smokeRenderer = particles.GetComponent<ParticleSystemRenderer>();
            smokeRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            smokeRenderer.sortingOrder = 14;
            smokeRenderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(SoftParticleMaterialPath);

            GameObject emberObject = new GameObject("Dark Red Embers");
            emberObject.transform.SetParent(smokeObject.transform, false);
            ParticleSystem embers = emberObject.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule emberMain = embers.main;
            emberMain.loop = false;
            emberMain.playOnAwake = false;
            emberMain.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.8f);
            emberMain.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.1f);
            emberMain.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.2f);
            emberMain.startColor = new ParticleSystem.MinMaxGradient(new Color32(150, 24, 34, 240));
            emberMain.maxParticles = 40;
            ParticleSystem.EmissionModule emberEmission = embers.emission;
            emberEmission.enabled = false;
            ParticleSystem.VelocityOverLifetimeModule emberVelocity = embers.velocityOverLifetime;
            emberVelocity.enabled = true;
            emberVelocity.y = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            ParticleSystemRenderer emberRenderer = embers.GetComponent<ParticleSystemRenderer>();
            emberRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            emberRenderer.sortingOrder = 15;
            emberRenderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(EmberMaterialPath);
            ParticleSystem.SubEmittersModule subEmitters = particles.subEmitters;
            subEmitters.AddSubEmitter(embers, ParticleSystemSubEmitterType.Birth,
                ParticleSystemSubEmitterProperties.InheritNothing, 0.12f);
            return particles;
        }

        private static Gradient CreateAlphaFadeGradient(Color color)
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
                new[] { new GradientAlphaKey(color.a / 255f, 0f), new GradientAlphaKey(0f, 1f) });
            return gradient;
        }

        private static Material GetThornGlowMaterial(Texture texture)
        {
            const string path = "Assets/_Project/Art/FX/M_FX_ThornGlowAdditive.mat";
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) throw new InvalidOperationException("URP Particles/Unlit shader is unavailable.");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader) { name = "M_FX_ThornGlowAdditive" };
                AssetDatabase.CreateAsset(material, path);
            }
            else material.shader = shader;
            material.SetTexture("_BaseMap", texture);
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 2f);
            material.SetColor("_BaseColor", Color.white);
            EditorUtility.SetDirty(material);
            return material;
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
