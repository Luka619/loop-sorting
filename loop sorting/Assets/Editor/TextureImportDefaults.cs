using UnityEditor;

namespace LoopSorting.Editor
{
    public sealed class TextureImportDefaults : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            var importer = assetImporter as TextureImporter;
            if (importer == null)
            {
                return;
            }

            if (importer.npotScale != TextureImporterNPOTScale.None)
            {
                importer.npotScale = TextureImporterNPOTScale.None;
            }
        }
    }
}
