using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Liga a textura pintada no Blender ao material importado de cada FBX.
    /// Convenção dos scripts de Tools/Blender: Art/Models/&lt;pasta&gt;/&lt;Nome&gt;.fbx traz o material
    /// M_&lt;Nome&gt;, e a textura fica em Art/Textures/&lt;pasta&gt;/&lt;Nome&gt;_BaseColor.png.
    /// </summary>
    public class BlenderModelPostprocessor : AssetPostprocessor
    {
        private const string ModelsFolder = "/Art/Models/";
        private const string TexturesFolder = "/Art/Textures/";
        private const string MaterialPrefix = "M_";
        private const string BaseColorSuffix = "_BaseColor.png";
        private const uint Version = 1;
        private const int RunAfterUrpMaterialSetup = 10;

        private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        public override uint GetVersion() => Version;

        public override int GetPostprocessOrder() => RunAfterUrpMaterialSetup;

        private void OnPreprocessMaterialDescription(MaterialDescription description, Material material,
            AnimationClip[] materialAnimation)
        {
            string materialName = description.materialName;
            if (!assetPath.Contains(ModelsFolder) || !materialName.StartsWith(MaterialPrefix))
            {
                return;
            }

            string texturePath = BuildTexturePath(materialName);
            context.DependsOnSourceAsset(texturePath);

            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (texture == null)
            {
                Debug.LogWarning($"Base color texture not found for {materialName}: {texturePath}");
                return;
            }

            material.SetTexture(BaseMapId, texture);
            material.SetColor(BaseColorId, Color.white); // o FBX traz um cinza que escureceria a textura
        }

        private string BuildTexturePath(string materialName)
        {
            string modelFolder = Path.GetDirectoryName(assetPath).Replace('\\', '/') + "/";
            string textureFolder = modelFolder.Replace(ModelsFolder, TexturesFolder);
            return textureFolder + materialName.Substring(MaterialPrefix.Length) + BaseColorSuffix;
        }
    }
}
