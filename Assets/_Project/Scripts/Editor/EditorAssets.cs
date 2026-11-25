using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Guaxinim.EditorTools
{
    /// <summary>Criação de assets usada pelos menus de greybox: carrega o que existe ou cria.</summary>
    public static class EditorAssets
    {
        private const string LitShaderName = "Universal Render Pipeline/Lit";
        private const string UnlitShaderName = "Universal Render Pipeline/Unlit";
        private const string ParticlesUnlitShaderName = "Universal Render Pipeline/Particles/Unlit";
        private const string BaseColorProperty = "_BaseColor";
        private const int TransparentQueue = (int)RenderQueue.Transparent;

        /// <summary>Carrega o asset; se não existir, cria com os valores padrão (mantém o que foi ajustado).</summary>
        public static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        /// <summary>
        /// Carrega ou cria o asset e o regrava com os valores iniciais do código. Para dados cujo texto vem do código
        /// (dicas, por exemplo): uma mudança no código chega ao jogo ao regerar. Ajustes no Inspector se perdem.
        /// </summary>
        public static T LoadOrCreateFromCode<T>(string path) where T : ScriptableObject
        {
            T asset = LoadOrCreate<T>(path);
            string assetName = asset.name; // o CopySerialized também copiaria o nome vazio da instância nova
            T defaults = ScriptableObject.CreateInstance<T>();
            EditorUtility.CopySerialized(defaults, asset);
            Object.DestroyImmediate(defaults);
            asset.name = assetName;
            EditorUtility.SetDirty(asset);
            return asset;
        }

        public static Material LoadOrCreateLitMaterial(string path, Color color)
        {
            Material material = LoadOrCreateMaterial(path, LitShaderName);
            material.SetColor(BaseColorProperty, color);
            EditorUtility.SetDirty(material);
            return material;
        }

        /// <summary>Material opaco sem luz (ícones sobre os personagens).</summary>
        public static Material LoadOrCreateUnlitMaterial(string path, Color color)
        {
            Material material = LoadOrCreateMaterial(path, UnlitShaderName);
            material.SetColor(BaseColorProperty, color);
            EditorUtility.SetDirty(material);
            return material;
        }

        /// <summary>Material sem luz que desenha só as faces de trás: numa cópia um pouco maior, vira contorno.</summary>
        public static Material LoadOrCreateOutlineMaterial(string path, Color color)
        {
            Material material = LoadOrCreateUnlitMaterial(path, color);
            material.SetFloat("_Cull", (float)CullMode.Front);
            EditorUtility.SetDirty(material);
            return material;
        }

        /// <summary>Material sem luz, transparente e que usa a cor do vértice (anéis e linhas).</summary>
        public static Material LoadOrCreateTransparentUnlitMaterial(string path, Color color)
        {
            Material material = LoadOrCreateMaterial(path, ParticlesUnlitShaderName);
            MakeTransparent(material);
            material.SetColor(BaseColorProperty, color);
            EditorUtility.SetDirty(material);
            return material;
        }

        /// <summary>Cria a pasta (e as de cima) se não existir. Caminho com "/", a partir de "Assets".</summary>
        public static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            int lastSlash = path.LastIndexOf('/');
            string parent = path.Substring(0, lastSlash);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, path.Substring(lastSlash + 1));
        }

        /// <summary>
        /// Cópia translúcida de um material URP (para o ObstacleFader), salva como asset. Se já existe, é
        /// atualizada no lugar (mantém o GUID e as referências).
        /// </summary>
        public static Material CreateFadedCopy(Material source, string path, float alpha)
        {
            var faded = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (faded == null)
            {
                faded = new Material(source);
                AssetDatabase.CreateAsset(faded, path);
            }
            else
            {
                faded.shader = source.shader;
                faded.CopyPropertiesFromMaterial(source);
            }

            MakeTransparent(faded);
            Color color = faded.GetColor(BaseColorProperty);
            color.a = alpha;
            faded.SetColor(BaseColorProperty, color);
            EditorUtility.SetDirty(faded);
            return faded;
        }

        public static void SetLayerRecursively(GameObject target, int layer)
        {
            target.layer = layer;
            foreach (Transform child in target.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        public static void SetReference(Object component, string propertyName, Object value)
        {
            var serialized = new SerializedObject(component);
            serialized.FindProperty(propertyName).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetReferenceArray(Object component, string propertyName, Object[] values)
        {
            var serialized = new SerializedObject(component);
            SerializedProperty array = serialized.FindProperty(propertyName);
            array.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void MakeTransparent(Material material)
        {
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = TransparentQueue;
        }

        private static Material LoadOrCreateMaterial(string path, string shaderName)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                return material;
            }

            material = new Material(Shader.Find(shaderName));
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
    }
}
