using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VTuberProject.Lighting
{
    /// <summary>
    /// Configuración y gestión del sistema de iluminación 2D
    /// </summary>
    [CreateAssetMenu(fileName = "LightingConfig", menuName = "VTuber/Lighting Configuration")]
    public class LightingConfiguration : ScriptableObject
    {
        [Header("Ambient Light Settings")]
        [Tooltip("Intensidad de la luz ambiental")]
        [Range(0f, 2f)]
        public float ambientIntensity = 1f;
        
        [Tooltip("Color de la luz ambiental")]
        public Color ambientColor = Color.white;

        [Header("Shadow Settings")]
        [Tooltip("Habilitar sombras en el proyecto")]
        public bool enableShadows = true;
        
        [Tooltip("Resolución de las sombras (mayor = mejor calidad)")]
        public ShadowResolution shadowResolution = ShadowResolution.Medium;
        
        [Tooltip("Suavizado de sombras")]
        [Range(0f, 1f)]
        public float shadowSoftness = 0.5f;

        [Header("Quality Settings")]
        [Tooltip("Escala de render de las texturas de luz (menor = mejor performance)")]
        [Range(0.25f, 1f)]
        public float lightTextureScale = 0.5f;
        
        [Tooltip("Número máximo de luces renderizadas")]
        [Range(1, 16)]
        public int maxLights = 8;

        [Header("HDR Settings")]
        [Tooltip("Habilitar High Dynamic Range")]
        public bool enableHDR = true;
        
        [Tooltip("Intensidad del bloom para luces brillantes")]
        [Range(0f, 1f)]
        public float bloomIntensity = 0.3f;

        public enum ShadowResolution
        {
            Low = 512,
            Medium = 1024,
            High = 2048,
            VeryHigh = 4096
        }

#if UNITY_EDITOR
        [ContextMenu("Apply Configuration")]
        public void ApplyConfiguration()
        {
            Debug.Log("Lighting Configuration Applied!");
            Debug.Log($"Ambient Intensity: {ambientIntensity}");
            Debug.Log($"Shadows: {(enableShadows ? "Enabled" : "Disabled")}");
            Debug.Log($"HDR: {(enableHDR ? "Enabled" : "Disabled")}");
        }

        [ContextMenu("Create Test Light Setup")]
        public void CreateTestLightSetup()
        {
            Debug.Log("Use the Scene Lighting Manager to create lights in your scene.");
        }
#endif
    }
}
