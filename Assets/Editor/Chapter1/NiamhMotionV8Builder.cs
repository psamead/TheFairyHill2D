using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheFairyHills.Gameplay;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace TheFairyHills.EditorTools
{
    /// <summary>Builds only the reversible v8 visual trial; never calls the chapter scene builder.</summary>
    public static class NiamhMotionV8Builder
    {
        public const string Source = "Assets/_Project/Art/Characters/Niamh/Motion_v8_Source";
        public const string Output = "Assets/_Project/Art/Characters/Niamh/Motion_v8_Trial";
        public const string PrefabPath = Output + "/Niamh_Motion_v8_Trial.prefab";
        private const string CorePath = "Assets/Scenes/Core_Persistent.unity";
        private static readonly string[] PartNames = { "head", "hair_back", "body", "arm", "leg", "boot" };
        private static readonly string[] States = { "Idle", "Walk", "Run", "JumpUp", "Apex", "Fall", "Land" };
        private static Spec spec;
        private static Vector2 hip, knee, head, shoulder;
        private static Vector2[] bootOutline;

        [Serializable] private sealed class Spec
        {
            public int fps;
            public int[] source_canvas;
            public Pivots pivots;
            public Durations clips;
            public Jump jump_pose;
            public Landing landing;
        }
        [Serializable] private sealed class Pivots { public float[] head, arm, hip, knee; }
        [Serializable] private sealed class Durations { public float idle, walk, run, jump; }
        [Serializable] private sealed class Jump
        {
            public float far_leg_backward_degrees, near_leg_backward_degrees, arm_forward_degrees, head_up_degrees;
            public float descent_head_down_degrees, descent_arm_backward_degrees;
            public float descent_near_leg_forward_degrees, descent_far_leg_forward_degrees;
        }
        [Serializable] private sealed class Landing
        {
            public float duration_seconds, body_and_hair_down_px, head_and_arm_down_px, hair_left_px, arm_right_px;
        }
        private struct Pose
        {
            public float nearLeg, farLeg, nearBoot, farBoot, arm, head, lean;
            public float upperY, bodyY, headY, hairX, armX;
        }

        [MenuItem("Tools/The Fairy Hills/Niamh v8/1 Build Trial Assets")]
        public static void BuildAssets()
        {
            RequireEditMode();
            ReadSpec();
            Directory.CreateDirectory(Output + "/Animation");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            foreach (string part in PartNames)
            {
                string path = Source + "/parts/" + part + ".png";
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) throw new InvalidOperationException("Missing PNG: " + path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 100f;
                importer.alphaSource = TextureImporterAlphaSource.FromInput;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.maxTextureSize = 1024;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp;
                TextureImporterSettings settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                settings.spritePivot = new Vector2(0.5f, 0f);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                settings.spriteGenerateFallbackPhysicsShape = false;
                importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite == null || sprite.rect.size != new Vector2(327, 699) || sprite.pivot != new Vector2(163.5f, 0))
                    throw new InvalidOperationException("Sprite canvas/pivot mismatch: " + path);
            }

            var clips = new List<AnimationClip>();
            foreach (string state in States) clips.Add(BuildClip(state));
            string controllerPath = Output + "/Animation/Niamh_Motion_v8.controller";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            foreach (string stateName in States)
            {
                AnimatorState state = machine.states.Select(s => s.state).FirstOrDefault(s => s.name == stateName);
                if (state == null) state = machine.AddState(stateName);
                state.motion = clips.First(c => c.name == stateName);
                state.writeDefaultValues = false;
                if (stateName == "Idle") machine.defaultState = state;
            }
            // State switching belongs to the presentation component; no exit-time lockouts.
            EditorUtility.SetDirty(controller);
            GameObject root = CreateRig(controller);
            try { PrefabUtility.SaveAsPrefabAsset(root, PrefabPath); }
            finally { UnityEngine.Object.DestroyImmediate(root); }
            AssetDatabase.SaveAssetIfDirty(controller);
            foreach (AnimationClip clip in clips) AssetDatabase.SaveAssetIfDirty(clip);
            Debug.Log("Niamh v8: six imports, eight renderers (derived far leg/boot), seven clips and trial prefab built. No level scenes rebuilt.");
        }

        private static void ReadSpec()
        {
            spec = JsonUtility.FromJson<Spec>(File.ReadAllText(Source + "/motion_preview_spec.json"));
            if (spec.source_canvas[0] != 327 || spec.source_canvas[1] != 699)
                throw new InvalidOperationException("Unexpected source canvas.");
            hip = Pivot(spec.pivots.hip); knee = Pivot(spec.pivots.knee);
            head = Pivot(spec.pivots.head); shoulder = Pivot(spec.pivots.arm);
            // Only alpha-edge points are needed to keep the lowest moving boot on the floor.
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                texture.LoadImage(File.ReadAllBytes(Source + "/parts/boot.png"));
                Color32[] pixels = texture.GetPixels32();
                var outline = new List<Vector2>();
                for (int x = 0; x < texture.width; x++)
                {
                    int bottom = -1, top = -1;
                    for (int y = 0; y < texture.height; y++)
                        if (pixels[y * texture.width + x].a >= 128) { if (bottom < 0) bottom = y; top = y; }
                    if (bottom >= 0)
                    {
                        outline.Add(new Vector2((x - 163.5f) / 100f, bottom / 100f));
                        outline.Add(new Vector2((x - 163.5f) / 100f, (top + 1) / 100f));
                    }
                }
                bootOutline = outline.ToArray();
            }
            finally { UnityEngine.Object.DestroyImmediate(texture); }
        }

        private static Vector2 Pivot(float[] json) => new Vector2((json[0] - 163.5f) / 100f, (699f - json[1]) / 100f);

        private static GameObject CreateRig(RuntimeAnimatorController controller)
        {
            GameObject root = new GameObject("Niamh Motion v8 Trial", typeof(Animator), typeof(SortingGroup));
            root.transform.localScale = Vector3.one * 0.30f;
            var group = root.GetComponent<SortingGroup>();
            group.sortingOrder = 10;
            Animator animator = root.GetComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            Transform motion = Child(root.transform, "Motion", hip);
            Transform farLeg = Child(motion, "FarLeg_Derived", new Vector2(-0.09f, 0));
            AddSprite(farLeg, "leg", -hip, 0, 0.86f);
            AddSprite(Child(farLeg, "Boot", knee - hip), "boot", -knee, 1, 0.86f);
            Transform nearLeg = Child(motion, "NearLeg", new Vector2(0.04f, 0));
            AddSprite(nearLeg, "leg", -hip, 2, 1f);
            AddSprite(Child(nearLeg, "Boot", knee - hip), "boot", -knee, 3, 1f);
            AddSprite(Child(motion, "HairBack", Vector2.zero), "hair_back", -hip, 4, 1f);
            AddSprite(Child(motion, "Body", Vector2.zero), "body", -hip, 5, 1f);
            AddSprite(Child(motion, "Arm", shoulder - hip), "arm", -shoulder, 6, 1f);
            AddSprite(Child(motion, "Head", head - hip), "head", -head, 7, 1f);
            var presentation = root.AddComponent<NiamhMotionV8Presentation>();
            var serialized = new SerializedObject(presentation);
            serialized.FindProperty("animator").objectReferenceValue = animator;
            serialized.FindProperty("landingDuration").floatValue = spec.landing.duration_seconds;
            SpriteRenderer[] renderers = root.GetComponentsInChildren<SpriteRenderer>();
            var array = serialized.FindProperty("parts"); array.arraySize = renderers.Length;
            for (int i = 0; i < renderers.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = renderers[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return root;
        }

        private static Transform Child(Transform parent, string name, Vector2 position)
        {
            var child = new GameObject(name).transform;
            child.SetParent(parent, false); child.localPosition = position;
            return child;
        }

        private static void AddSprite(Transform pivot, string part, Vector2 offset, int order, float tint)
        {
            SpriteRenderer renderer = Child(pivot, "Art", offset).gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Source + "/parts/" + part + ".png");
            renderer.sortingOrder = order;
            renderer.color = new Color(tint, tint, tint, 1);
        }

        private static float Duration(string state)
        {
            switch (state)
            {
                case "Idle": return spec.clips.idle;
                case "Walk": return spec.clips.walk;
                case "Run": return spec.clips.run;
                case "JumpUp": return 0.25f;
                case "Apex": return 0.12f;
                case "Fall": return 0.32f;
                default: return spec.landing.duration_seconds;
            }
        }

        private static Pose Evaluate(string state, float t)
        {
            var p = new Pose();
            float u = t / Duration(state), phase = u * Mathf.PI * 2;
            switch (state)
            {
                case "Idle":
                    p.upperY = 0.02f * Mathf.Sin(phase);
                    p.head = 0.8f * Mathf.Sin(phase);
                    p.arm = 1.2f * Mathf.Sin(phase + 0.5f);
                    break;
                case "Walk": case "Run":
                    bool run = state == "Run";
                    p.nearLeg = (run ? 34 : 21) * Mathf.Sin(phase); p.farLeg = -p.nearLeg;
                    p.nearBoot = -(run ? 28 : 15) * Mathf.Max(0, -Mathf.Sin(phase));
                    p.farBoot = -Mathf.Max(0, p.nearLeg) * 0.7f;
                    p.arm = -p.nearLeg * 0.55f; p.lean = run ? -7 : -2;
                    p.head = 1.5f * Mathf.Sin(phase + 0.4f);
                    break;
                case "JumpUp": case "Apex":
                    p.farLeg = -spec.jump_pose.far_leg_backward_degrees;
                    p.nearLeg = -spec.jump_pose.near_leg_backward_degrees;
                    p.nearBoot = -16; p.farBoot = -12;
                    p.arm = spec.jump_pose.arm_forward_degrees;
                    p.head = spec.jump_pose.head_up_degrees; p.lean = -4;
                    if (state == "Apex") { p.nearLeg *= 0.55f; p.farLeg *= 0.55f; p.arm *= 0.5f; p.head = 0; }
                    break;
                case "Fall":
                    // Retimed from the offline two-second study to the real ~0.73-second jump.
                    float swing = Mathf.Pow(Mathf.Sin(Mathf.PI * Mathf.Clamp01(u)), 2);
                    p.nearLeg = spec.jump_pose.descent_near_leg_forward_degrees * swing;
                    p.farLeg = spec.jump_pose.descent_far_leg_forward_degrees * swing;
                    p.arm = -spec.jump_pose.descent_arm_backward_degrees * swing;
                    p.head = -spec.jump_pose.descent_head_down_degrees; p.lean = -4 * swing;
                    break;
                case "Land":
                    float settle = Mathf.Pow(Mathf.Sin(Mathf.PI * Mathf.Clamp01(u)), 2);
                    p.bodyY = -spec.landing.body_and_hair_down_px * settle / 100f;
                    p.headY = -spec.landing.head_and_arm_down_px * settle / 100f;
                    p.hairX = -spec.landing.hair_left_px * settle / 100f;
                    p.armX = spec.landing.arm_right_px * settle / 100f;
                    p.head = -6 * (1 - Mathf.SmoothStep(0, 1, u)) - 2 * settle;
                    break;
            }
            return p;
        }

        private static AnimationClip BuildClip(string state)
        {
            string path = Output + "/Animation/" + state + ".anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, path); }
            clip.ClearCurves(); clip.name = state; clip.frameRate = spec.fps;
            int frames = Mathf.Max(4, Mathf.CeilToInt(Duration(state) * 60));
            var curves = new Dictionary<string, List<Keyframe>>();
            for (int i = 0; i <= frames; i++)
            {
                float t = Duration(state) * i / frames; Pose p = Evaluate(state, t);
                float floor = state == "Walk" || state == "Run" ? FloorCorrection(p) : 0;
                Key(curves, "Motion", "m_LocalPosition.y", t, hip.y + floor);
                Key(curves, "Motion", "localEulerAnglesRaw.z", t, p.lean);
                Key(curves, "Motion/NearLeg", "localEulerAnglesRaw.z", t, p.nearLeg);
                Key(curves, "Motion/FarLeg_Derived", "localEulerAnglesRaw.z", t, p.farLeg);
                Key(curves, "Motion/NearLeg/Boot", "localEulerAnglesRaw.z", t, p.nearBoot);
                Key(curves, "Motion/FarLeg_Derived/Boot", "localEulerAnglesRaw.z", t, p.farBoot);
                Key(curves, "Motion/Arm", "localEulerAnglesRaw.z", t, p.arm);
                Key(curves, "Motion/Head", "localEulerAnglesRaw.z", t, p.head);
                Key(curves, "Motion/Body", "m_LocalPosition.y", t, p.upperY + p.bodyY);
                Key(curves, "Motion/HairBack", "m_LocalPosition.y", t, p.upperY + p.bodyY);
                Key(curves, "Motion/HairBack", "m_LocalPosition.x", t, p.hairX);
                Key(curves, "Motion/Arm", "m_LocalPosition.y", t, shoulder.y - hip.y + p.upperY + p.headY);
                Key(curves, "Motion/Arm", "m_LocalPosition.x", t, shoulder.x - hip.x + p.armX);
                Key(curves, "Motion/Head", "m_LocalPosition.y", t, head.y - hip.y + p.upperY + p.headY);
            }
            foreach (var item in curves)
            {
                string[] binding = item.Key.Split('|');
                var curve = new AnimationCurve(item.Value.ToArray());
                for (int i = 0; i < curve.length; i++)
                {
                    AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
                    AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
                }
                AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(binding[0], typeof(Transform), binding[1]), curve);
            }
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = state == "Idle" || state == "Walk" || state == "Run";
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static void Key(Dictionary<string, List<Keyframe>> curves, string path, string property, float t, float value)
        {
            string key = path + "|" + property;
            if (!curves.TryGetValue(key, out var keys)) curves[key] = keys = new List<Keyframe>();
            keys.Add(new Keyframe(t, value));
        }

        private static Vector2 Rotate(Vector2 point, float angle)
        {
            float a = angle * Mathf.Deg2Rad;
            return new Vector2(point.x * Mathf.Cos(a) - point.y * Mathf.Sin(a), point.x * Mathf.Sin(a) + point.y * Mathf.Cos(a));
        }

        private static float FloorCorrection(Pose p)
        {
            float lowest = float.PositiveInfinity;
            foreach (Vector2 point in bootOutline)
            {
                Vector2 near = new Vector2(0.04f, 0) + Rotate(knee - hip + Rotate(point - knee, p.nearBoot), p.nearLeg);
                Vector2 far = new Vector2(-0.09f, 0) + Rotate(knee - hip + Rotate(point - knee, p.farBoot), p.farLeg);
                lowest = Mathf.Min(lowest, hip.y + Rotate(near, p.lean).y, hip.y + Rotate(far, p.lean).y);
            }
            return -lowest;
        }

        [MenuItem("Tools/The Fairy Hills/Niamh v8/2 Install In Core Only")]
        public static void InstallInCore()
        {
            RequireEditMode();
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null) throw new InvalidOperationException("Build the trial assets first.");
            Scene originalActive = SceneManager.GetActiveScene();
            Scene core = SceneManager.GetSceneByPath(CorePath);
            bool opened = !core.IsValid() || !core.isLoaded;
            if (!opened && core.isDirty) throw new InvalidOperationException("Core has unsaved edits; preserve/review them before installation.");
            if (opened) core = EditorSceneManager.OpenScene(CorePath, OpenSceneMode.Additive);
            try
            {
                PlayerController2D player = core.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<PlayerController2D>(true)).Single();
                if (player.GetComponentInChildren<NiamhMotionV8Presentation>(true) != null)
                { Debug.Log("Niamh v8 already installed; no scene change."); return; }
                Transform old = player.transform.Find("Niamh Static Sprite");
                if (old == null) throw new InvalidOperationException("Expected rollback visual missing.");
                SpriteRenderer oldRenderer = old.GetComponent<SpriteRenderer>();
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, core);
                Undo.RegisterCreatedObjectUndo(instance, "Install Niamh v8 trial");
                instance.transform.SetParent(player.transform, false);
                instance.GetComponent<SortingGroup>().sortingLayerID = oldRenderer.sortingLayerID;
                instance.GetComponent<SortingGroup>().sortingOrder = oldRenderer.sortingOrder;
                var presentation = instance.GetComponent<NiamhMotionV8Presentation>();
                presentation.Configure(player, oldRenderer);
                PrefabUtility.RecordPrefabInstancePropertyModifications(presentation);
                Undo.RecordObject(oldRenderer, "Keep static Niamh for rollback");
                oldRenderer.enabled = true; // OnEnable hides it in Play Mode; disabling the trial restores it.
                // Hide the trial only in Edit Mode to keep the existing scene silhouette uncluttered.
                foreach (SpriteRenderer part in instance.GetComponentsInChildren<SpriteRenderer>())
                {
                    part.enabled = false;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(part);
                }
                EditorSceneManager.MarkSceneDirty(core);
                EditorSceneManager.SaveScene(core);
                Debug.Log("Niamh v8 installed in Core only. Disable the trial GameObject in Play Mode to restore the static visual.");
            }
            finally
            {
                if (originalActive.IsValid() && originalActive.isLoaded) SceneManager.SetActiveScene(originalActive);
                if (opened) EditorSceneManager.CloseScene(core, true);
            }
        }

        [MenuItem("Tools/The Fairy Hills/Niamh v8/3 Capture Magenta Pose Sheet")]
        public static void CapturePoseSheet()
        {
            RequireEditMode();
            string directory = "Docs/References/Niamh_Motion_v8/Unity_Trial";
            Directory.CreateDirectory(directory);
            Scene preview = EditorSceneManager.NewPreviewScene();
            GameObject root = null, cameraObject = null;
            Material unlit = null;
            RenderTexture target = null;
            Texture2D sheet = null;
            try
            {
                root = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath), preview);
                root.GetComponent<NiamhMotionV8Presentation>().enabled = false;
                root.GetComponent<Animator>().enabled = false;
                foreach (Transform child in root.GetComponentsInChildren<Transform>()) child.gameObject.layer = 31;
                unlit = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
                foreach (SpriteRenderer renderer in root.GetComponentsInChildren<SpriteRenderer>()) { renderer.enabled = true; renderer.sharedMaterial = unlit; }
                cameraObject = new GameObject("Niamh seam capture camera", typeof(Camera));
                SceneManager.MoveGameObjectToScene(cameraObject, preview);
                Camera camera = cameraObject.GetComponent<Camera>();
                camera.cameraType = CameraType.Preview;
                camera.overrideSceneCullingMask = EditorSceneManager.GetSceneCullingMask(preview);
                camera.orthographic = true; camera.orthographicSize = 1.32f;
                camera.transform.position = new Vector3(0, 1.08f, -10);
                camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.magenta;
                camera.cullingMask = 1 << 31;
                target = new RenderTexture(360, 540, 24); camera.targetTexture = target;
                sheet = new Texture2D(360 * 4, 540 * 2, TextureFormat.RGB24, false);
                string[] states = { "Idle", "Walk", "Run", "JumpUp", "Apex", "Fall", "Land", "Fall" };
                float[] times = { 0, 0.2f, 0.125f, 0.1f, 0.05f, 0.16f, 0.15f, 0.16f };
                for (int i = 0; i < states.Length; i++)
                {
                    root.transform.localScale = new Vector3(i == 7 ? -0.30f : 0.30f, 0.30f, 0.30f);
                    AssetDatabase.LoadAssetAtPath<AnimationClip>(Output + "/Animation/" + states[i] + ".anim").SampleAnimation(root, times[i]);
                    camera.Render();
                    RenderTexture.active = target;
                    var shot = new Texture2D(360, 540, TextureFormat.RGB24, false);
                    shot.ReadPixels(new Rect(0, 0, 360, 540), 0, 0); shot.Apply();
                    File.WriteAllBytes(directory + "/Seam_" + i + "_" + states[i] + (i == 7 ? "_Left" : "") + ".png", shot.EncodeToPNG());
                    sheet.SetPixels((i % 4) * 360, (1 - i / 4) * 540, 360, 540, shot.GetPixels());
                    UnityEngine.Object.DestroyImmediate(shot);
                }
                sheet.Apply(); File.WriteAllBytes(directory + "/Unity_Magenta_Poses.png", sheet.EncodeToPNG());
                Debug.Log("Niamh v8 Unity-rendered magenta pose sheet: " + directory + "/Unity_Magenta_Poses.png (top: idle/walk/run/ascent; bottom: apex/fall/land/fall-left).");
            }
            finally
            {
                RenderTexture.active = null;
                if (cameraObject != null) UnityEngine.Object.DestroyImmediate(cameraObject);
                if (root != null) UnityEngine.Object.DestroyImmediate(root);
                if (unlit != null) UnityEngine.Object.DestroyImmediate(unlit);
                if (target != null) { target.Release(); UnityEngine.Object.DestroyImmediate(target); }
                if (sheet != null) UnityEngine.Object.DestroyImmediate(sheet);
                EditorSceneManager.ClosePreviewScene(preview);
            }
        }

        private static void RequireEditMode()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode before modifying trial assets.");
        }
    }
}
