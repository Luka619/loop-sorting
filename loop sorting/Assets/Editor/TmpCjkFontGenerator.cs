using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace LoopSorting.Editor
{
    public static class TmpCjkFontGenerator
    {
        private const string FontPath = "Assets/Fonts/LoopSortingCJK.ttf";
        private const string CharactersPath = "Assets/Fonts/LoopSortingCjkCharacters.txt";
        private const string OutputPath = "Assets/Fonts/TMP/LoopSortingCJK SDF.asset";
        private const string TmpSettingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";

        [MenuItem("LoopSorting/Fonts/Generate TMP CJK Font")]
        public static void GenerateFontAsset()
        {
            EnsureDirectories();

            var font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
            if (font == null)
            {
                Debug.LogError($"Missing font at '{FontPath}'. Place a CJK font there and re-run.");
                return;
            }

            EnsureFontImportSettings(FontPath);

            if (!File.Exists(CharactersPath))
            {
                Debug.LogError($"Missing character list at '{CharactersPath}'.");
                return;
            }

            string characters = File.ReadAllText(CharactersPath);
            characters = new string(characters.Where(c => !char.IsWhiteSpace(c)).Distinct().ToArray());
            if (string.IsNullOrEmpty(characters))
            {
                Debug.LogError("Character list is empty.");
                return;
            }

            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(OutputPath);
            if (existing != null)
            {
                AssetDatabase.DeleteAsset(OutputPath);
            }

            var fontAsset = TMP_FontAsset.CreateFontAsset(
                font,
                samplingPointSize: 90,
                atlasPadding: 9,
                renderMode: GlyphRenderMode.SDFAA,
                atlasWidth: 1024,
                atlasHeight: 1024,
                atlasPopulationMode: AtlasPopulationMode.Dynamic,
                enableMultiAtlasSupport: true);

            if (fontAsset == null)
            {
                Debug.LogError("Failed to create TMP font asset.");
                return;
            }

            fontAsset.name = Path.GetFileNameWithoutExtension(OutputPath);
            fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;

            if (!fontAsset.TryAddCharacters(characters, out string missing))
            {
                Debug.LogWarning("Some characters could not be added to the font atlas.");
            }
            if (!string.IsNullOrEmpty(missing))
            {
                Debug.LogWarning($"Missing characters: {missing}");
            }

            fontAsset.atlasPopulationMode = AtlasPopulationMode.Static;

            AssetDatabase.CreateAsset(fontAsset, OutputPath);
            if (fontAsset.atlasTexture != null)
            {
                AssetDatabase.AddObjectToAsset(fontAsset.atlasTexture, fontAsset);
            }
            if (fontAsset.material != null)
            {
                AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
            }

            AddToTmpFallback(fontAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Generated TMP font asset at '{OutputPath}'.");
        }

        private static void EnsureDirectories()
        {
            EnsureFolder("Assets/Fonts");
            EnsureFolder("Assets/Fonts/TMP");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path);
            string name = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static void EnsureFontImportSettings(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TrueTypeFontImporter;
            if (importer == null) return;
            if (!importer.includeFontData)
            {
                importer.includeFontData = true;
                importer.SaveAndReimport();
            }
        }

        private static void AddToTmpFallback(TMP_FontAsset fontAsset)
        {
            var settings = AssetDatabase.LoadAssetAtPath<TMP_Settings>(TmpSettingsPath);
            if (settings == null)
            {
                Debug.LogWarning($"TMP Settings not found at '{TmpSettingsPath}'.");
                return;
            }

            var fallback = TMP_Settings.fallbackFontAssets;
            if (fallback == null)
            {
                Debug.LogWarning("TMP Settings fallback font list is null.");
                return;
            }

            if (!fallback.Contains(fontAsset))
            {
                fallback.Add(fontAsset);
                EditorUtility.SetDirty(settings);
            }
        }
    }
}
