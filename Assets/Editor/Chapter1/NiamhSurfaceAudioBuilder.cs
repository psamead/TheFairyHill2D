using System;
using System.IO;
using System.Linq;
using TheFairyHills.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheFairyHills.EditorTools
{
    public static class NiamhSurfaceAudioBuilder
    {
        private const string Folder = "Assets/_Project/Audio/SFX/Niamh_Motion_v1/";
        private const string ProfilePath = "Assets/_Project/Resources/NiamhSurfaceAudio_v1.asset";
        [MenuItem("Tools/The Fairy Hills/Audio/Install Niamh Surface Audio Only")]
        public static void Install()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode first.");
            Directory.CreateDirectory("Assets/_Project/Resources"); AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            foreach (string file in Directory.GetFiles(Folder, "*.ogg"))
            {
                var importer = (AudioImporter)AssetImporter.GetAtPath(file.Replace('\\', '/'));
                importer.forceToMono = true;
                var settings = importer.defaultSampleSettings; settings.loadType = AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat = AudioCompressionFormat.Vorbis; settings.quality = 0.85f;
                settings.preloadAudioData = true;
                importer.defaultSampleSettings = settings; importer.SaveAndReimport();
            }
            var profile = AssetDatabase.LoadAssetAtPath<SurfaceAudioProfile>(ProfilePath);
            if (profile == null) { profile = ScriptableObject.CreateInstance<SurfaceAudioProfile>(); AssetDatabase.CreateAsset(profile, ProfilePath); }
            profile.steps = new[] { GroundMaterial.Dirt, GroundMaterial.Grass, GroundMaterial.Stone, GroundMaterial.Hay, GroundMaterial.Wood }
                .Select(s => new SurfaceAudioProfile.StepSet { surface = s, clips = Clips("Step_" + s.ToString().ToLowerInvariant(), 3) }).ToArray();
            profile.jump = Clips("Jump", 2); profile.lightLand = Clips("Land_light", 2); profile.heavyLand = Clips("Land_heavy", 2); profile.hayBrush = Clips("HayBrush", 2);
            profile.mappings = JsonUtility.FromJson<SurfaceAudioProfile.MapFile>(File.ReadAllText("Assets/_Project/Data/Layouts/Chapter1_SurfaceAudio_v1.json")).mappings;
            EditorUtility.SetDirty(profile); AssetDatabase.SaveAssetIfDirty(profile);
            Scene core = SceneManager.GetSceneByPath("Assets/Scenes/Core_Persistent.unity");
            bool opened = !core.IsValid() || !core.isLoaded;
            if (!opened && core.isDirty) throw new InvalidOperationException("Core has unsaved edits.");
            Scene active = SceneManager.GetActiveScene();
            if (opened) core = EditorSceneManager.OpenScene("Assets/Scenes/Core_Persistent.unity", OpenSceneMode.Additive);
            try
            {
                var player = core.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<PlayerController2D>()).Single();
                var audio = player.GetComponent<NiamhSurfaceAudio>();
                if (audio == null) audio = Undo.AddComponent<NiamhSurfaceAudio>(player.gameObject);
                var serialized = new SerializedObject(audio); serialized.FindProperty("profile").objectReferenceValue = profile; serialized.ApplyModifiedProperties();
                EditorSceneManager.MarkSceneDirty(core); EditorSceneManager.SaveScene(core);
            }
            finally { if (active.IsValid()) SceneManager.SetActiveScene(active); if (opened) EditorSceneManager.CloseScene(core, true); }
            Debug.Log("23 clips imported; narrow Core sound component saved. Surface map applies at LevelBootstrap; no level scene rebuilt or saved.");
        }
        private static AudioClip[] Clips(string prefix, int count)
        {
            return Enumerable.Range(1, count).Select(i =>
            {
                string path = Folder + "SFX_Niamh_" + prefix + "_" + i.ToString("00") + "_v1.ogg";
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip == null) throw new InvalidOperationException("Missing clip: " + path);
                return clip;
            }).ToArray();
        }
    }
}
