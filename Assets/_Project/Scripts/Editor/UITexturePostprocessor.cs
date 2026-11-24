using UnityEditor;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Importa as imagens de Art/UI/ (fundos de tela) como Sprite: sem mipmaps, no tamanho original (até 2048 px,
    /// por causa do download no WebGL) e sem arredondar para potência de 2. Os montadores de tela só precisam
    /// carregar o asset como Sprite.
    /// </summary>
    public class UITexturePostprocessor : AssetPostprocessor
    {
        private const string UIFolder = "Assets/_Project/Art/UI/";
        private const int MaxTextureSize = 2048;
        private const uint Version = 1;

        public override uint GetVersion() => Version;

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(UIFolder))
            {
                return;
            }

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = MaxTextureSize;
            importer.textureCompression = TextureImporterCompression.Compressed;
        }
    }
}
