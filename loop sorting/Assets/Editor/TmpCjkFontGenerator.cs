using System.IO;
using System.Collections.Generic;
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
                atlasWidth: 2048,
                atlasHeight: 2048,
                atlasPopulationMode: AtlasPopulationMode.Dynamic,
                enableMultiAtlasSupport: true);

            if (fontAsset == null)
            {
                Debug.LogError("Failed to create TMP font asset.");
                return;
            }

            fontAsset.name = Path.GetFileNameWithoutExtension(OutputPath);
            fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            fontAsset.isMultiAtlasTexturesEnabled = true;

            if (!fontAsset.TryAddCharacters(characters, out string missing))
            {
                Debug.LogWarning("Some characters could not be added to the font atlas.");
            }
            if (!string.IsNullOrEmpty(missing))
            {
                Debug.LogWarning($"Missing characters: {missing}");
            }

            FixAtlasTextures(fontAsset);
            fontAsset.atlasPopulationMode = AtlasPopulationMode.Static;

            AssetDatabase.CreateAsset(fontAsset, OutputPath);
            AddFontSubAssets(fontAsset);

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

        private static void FixAtlasTextures(TMP_FontAsset fontAsset)
        {
            if (fontAsset == null) return;
            var atlas = fontAsset.atlasTexture;
            if (atlas == null) return;

            if (fontAsset.atlasTextures == null || fontAsset.atlasTextures.Length == 0)
            {
                fontAsset.atlasTextures = new[] { atlas };
            }
            fontAsset.isMultiAtlasTexturesEnabled = true;
            var so = new SerializedObject(fontAsset);
            var atlasIndex = so.FindProperty("m_AtlasTextureIndex");
            if (atlasIndex != null)
            {
                if (atlasIndex.intValue < 0) atlasIndex.intValue = 0;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            if (fontAsset.material != null)
            {
                fontAsset.material.SetTexture(ShaderUtilities.ID_MainTex, atlas);
            }
        }

        private static void AddFontSubAssets(TMP_FontAsset fontAsset)
        {
            if (fontAsset == null) return;
            var assetPath = AssetDatabase.GetAssetPath(fontAsset);
            if (string.IsNullOrEmpty(assetPath)) return;

            var textures = fontAsset.atlasTextures;
            if (textures != null)
            {
                for (int i = 0; i < textures.Length; i++)
                {
                    var tex = textures[i];
                    if (tex == null) continue;
                    if (string.IsNullOrEmpty(tex.name))
                    {
                        tex.name = $"{fontAsset.name} Atlas {i}";
                    }
                    if (AssetDatabase.GetAssetPath(tex) != assetPath)
                    {
                        AssetDatabase.AddObjectToAsset(tex, fontAsset);
                    }
                }
            }

            if (fontAsset.material != null)
            {
                if (string.IsNullOrEmpty(fontAsset.material.name))
                {
                    fontAsset.material.name = $"{fontAsset.name} Material";
                }
                if (AssetDatabase.GetAssetPath(fontAsset.material) != assetPath)
                {
                    AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
                }
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
