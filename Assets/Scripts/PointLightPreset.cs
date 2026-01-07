using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace VTuberProject.Lighting
{
    /// <summary>
    /// Presets para diferentes tipos de luces puntuales
    /// </summary>
    [CreateAssetMenu(fileName = "PointLightPreset", menuName = "VTuber/Point Light Preset")]
    public class PointLightPreset : ScriptableObject
    {
        [Header("Light Properties")]
        public Color lightColor = Color.white;
        
        [Range(0f, 5f)]
        public float intensity = 1f;
        
        [Range(0f, 20f)]
        public float innerRadius = 0f;
        
        [Range(0f, 20f)]
        public float outerRadius = 5f;
        
        [Header("Falloff")]
        [Range(0f, 1f)]
        public float falloffIntensity = 0.5f;
        
        [Header("Blend Style")]
        [Tooltip("0 = Multiply, 1 = Additive, 2 = Multiply with Mask, 3 = Additive with Mask")]
        [Range(0, 3)]
        public int blendStyleIndex = 1; // Additive por defecto
        
        [Header("Shadows")]
        public bool castsShadows = true;
        
        [Header("Volumetrics")]
        public bool enableVolumetrics = false;
        
        [Range(0f, 1f)]
        public float volumeIntensity = 0.5f;

        /// <summary>
        /// Aplica este preset a un Light2D
        /// </summary>
        public void ApplyToLight(Light2D light)
        {
            if (light == null) return;
            
            light.color = lightColor;
            light.intensity = intensity;
            light.pointLightInnerRadius = innerRadius;
            light.pointLightOuterRadius = outerRadius;
            light.falloffIntensity = falloffIntensity;
            light.blendStyleIndex = blendStyleIndex;
            
            // Configurar sombras
            if (castsShadows)
            {
                light.shadowsEnabled = true;
                light.shadowIntensity = 0.75f;
                light.shadowVolumeIntensity = 0.75f;
            }
            else
            {
                light.shadowsEnabled = false;
            }
            
            // Volumetrics (si está disponible)
            light.volumeIntensity = enableVolumetrics ? volumeIntensity : 0f;
        }

        #region Presets comunes
        
        public static PointLightPreset CreateWarmLampPreset()
        {
            var preset = CreateInstance<PointLightPreset>();
            preset.name = "Warm Lamp";
            preset.lightColor = new Color(1f, 0.8f, 0.5f); // Naranja cálido
            preset.intensity = 1.5f;
            preset.innerRadius = 0.5f;
            preset.outerRadius = 6f;
            preset.falloffIntensity = 0.6f;
            preset.blendStyleIndex = 1; // Additive
            preset.castsShadows = true;
            preset.enableVolumetrics = true;
            preset.volumeIntensity = 0.3f;
            return preset;
        }
        
        public static PointLightPreset CreateCoolWindowPreset()
        {
            var preset = CreateInstance<PointLightPreset>();
            preset.name = "Cool Window";
            preset.lightColor = new Color(0.7f, 0.85f, 1f); // Azul frío
            preset.intensity = 1.2f;
            preset.innerRadius = 1f;
            preset.outerRadius = 8f;
            preset.falloffIntensity = 0.7f;
            preset.blendStyleIndex = 1; // Additive
            preset.castsShadows = false;
            preset.enableVolumetrics = false;
            return preset;
        }
        
        public static PointLightPreset CreateCandlePreset()
        {
            var preset = CreateInstance<PointLightPreset>();
            preset.name = "Candle";
            preset.lightColor = new Color(1f, 0.6f, 0.2f); // Naranja intenso
            preset.intensity = 0.8f;
            preset.innerRadius = 0f;
            preset.outerRadius = 3f;
            preset.falloffIntensity = 0.8f;
            preset.blendStyleIndex = 1; // Additive
            preset.castsShadows = true;
            preset.enableVolumetrics = true;
            preset.volumeIntensity = 0.5f;
            return preset;
        }
        
        public static PointLightPreset CreateScreenGlowPreset()
        {
            var preset = CreateInstance<PointLightPreset>();
            preset.name = "Screen Glow";
            preset.lightColor = new Color(0.6f, 0.8f, 1f); // Azul monitor
            preset.intensity = 0.6f;
            preset.innerRadius = 0.2f;
            preset.outerRadius = 2.5f;
            preset.falloffIntensity = 0.5f;
            preset.blendStyleIndex = 1; // Additive
            preset.castsShadows = false;
            preset.enableVolumetrics = false;
            return preset;
        }
        
        #endregion
    }
}
