using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Live2D.Cubism.Rendering.URP;

public class SetupLive2DRenderer
{
    [MenuItem("Tools/Setup Live2D Universal Renderer")]
    public static void Setup()
    {
        // 1. Create UniversalRendererData asset
        var rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
        AssetDatabase.CreateAsset(rendererData, "Assets/Settings/UniversalRenderer.asset");

        // 2. Add CubismRenderPassFeature
        var feature = ScriptableObject.CreateInstance<CubismRenderPassFeature>();
        feature.name = "CubismRenderPassFeature";
        AssetDatabase.AddObjectToAsset(feature, rendererData);
        rendererData.rendererFeatures.Add(feature);
        EditorUtility.SetDirty(rendererData);

        // 3. Add to URP pipeline asset
        var urpAsset = UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
        if (urpAsset != null)
        {
            var so = new SerializedObject(urpAsset);
            var rendererList = so.FindProperty("m_RendererDataList");
            int newIndex = rendererList.arraySize;
            rendererList.arraySize++;
            rendererList.GetArrayElementAtIndex(newIndex).objectReferenceValue = rendererData;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(urpAsset);
            Debug.Log($"Universal Renderer agregado al pipeline en índice {newIndex}. Asignalo a la cámara como índice {newIndex}.");
        }
        else
        {
            Debug.LogError("No se encontró el URP Pipeline Asset activo.");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Listo! Ahora seleccioná la Main Camera y en Renderer elegí 'UniversalRenderer'.");
    }
}
