using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

namespace VTuberProject.Lighting
{
    /// <summary>
    /// Gestiona todas las luces de la escena y permite controlarlas globalmente
    /// </summary>
    public class SceneLightingManager : MonoBehaviour
    {
        [Header("Global Light")]
        [Tooltip("Luz global de la escena (sol/luna)")]
        public Light2D globalLight;
        
        [Header("Indoor Lights")]
        [Tooltip("Luces artificiales de interiores (lámparas, monitores, etc.)")]
        public List<Light2D> indoorLights = new List<Light2D>();
        
        [Header("Outdoor Lights")]
        [Tooltip("Luces exteriores (ventanas, luz natural)")]
        public List<Light2D> outdoorLights = new List<Light2D>();
        
        [Header("Special Effects")]
        [Tooltip("Luces de efectos especiales (velas, screens)")]
        public List<Light2D> specialLights = new List<Light2D>();

        [Header("Global Controls")]
        [Range(0f, 2f)]
        public float globalIntensityMultiplier = 1f;
        
        public bool indoorLightsEnabled = true;
        public bool outdoorLightsEnabled = true;
        public bool specialLightsEnabled = true;

        private Dictionary<Light2D, float> originalIntensities = new Dictionary<Light2D, float>();

        private void Start()
        {
            // Guardar intensidades originales
            CacheOriginalIntensities();
        }

        private void Update()
        {
            // Aplicar controles globales
            SetLightGroupState(indoorLights, indoorLightsEnabled);
            SetLightGroupState(outdoorLights, outdoorLightsEnabled);
            SetLightGroupState(specialLights, specialLightsEnabled);
            
            ApplyGlobalIntensityMultiplier();
        }

        private void CacheOriginalIntensities()
        {
            originalIntensities.Clear();
            
            CacheLightGroup(indoorLights);
            CacheLightGroup(outdoorLights);
            CacheLightGroup(specialLights);
            
            if (globalLight != null)
            {
                originalIntensities[globalLight] = globalLight.intensity;
            }
        }

        private void CacheLightGroup(List<Light2D> lights)
        {
            foreach (var light in lights)
            {
                if (light != null && !originalIntensities.ContainsKey(light))
                {
                    originalIntensities[light] = light.intensity;
                }
            }
        }

        private void SetLightGroupState(List<Light2D> lights, bool enabled)
        {
            foreach (var light in lights)
            {
                if (light != null)
                {
                    light.enabled = enabled;
                }
            }
        }

        private void ApplyGlobalIntensityMultiplier()
        {
            foreach (var kvp in originalIntensities)
            {
                if (kvp.Key != null && kvp.Key.enabled)
                {
                    kvp.Key.intensity = kvp.Value * globalIntensityMultiplier;
                }
            }
        }

        /// <summary>
        /// Encuentra y registra automáticamente todas las luces en la escena
        /// </summary>
        [ContextMenu("Auto-Register All Lights")]
        public void AutoRegisterLights()
        {
            Light2D[] allLights = FindObjectsOfType<Light2D>();
            
            indoorLights.Clear();
            outdoorLights.Clear();
            specialLights.Clear();
            
            foreach (var light in allLights)
            {
                // Clasificar por nombre o tags
                string name = light.gameObject.name.ToLower();
                
                if (light.lightType == Light2D.LightType.Global)
                {
                    globalLight = light;
                }
                else if (name.Contains("lamp") || name.Contains("indoor") || name.Contains("ceiling"))
                {
                    indoorLights.Add(light);
                }
                else if (name.Contains("window") || name.Contains("outdoor") || name.Contains("natural"))
                {
                    outdoorLights.Add(light);
                }
                else if (name.Contains("candle") || name.Contains("screen") || name.Contains("effect"))
                {
                    specialLights.Add(light);
                }
                else
                {
                    // Por defecto, añadir a indoor
                    indoorLights.Add(light);
                }
            }
            
            Debug.Log($"Auto-registered: {indoorLights.Count} indoor, {outdoorLights.Count} outdoor, {specialLights.Count} special lights");
            
            CacheOriginalIntensities();
        }

        /// <summary>
        /// Activa/desactiva todas las luces de interior
        /// </summary>
        public void ToggleIndoorLights(bool state)
        {
            indoorLightsEnabled = state;
        }

        /// <summary>
        /// Establece el multiplicador de intensidad para las luces exteriores (ventanas/cortinas)
        /// </summary>
        public void SetOutdoorLightsMultiplier(float multiplier)
        {
            foreach (var light in outdoorLights)
            {
                if (light != null && originalIntensities.ContainsKey(light))
                {
                    light.intensity = originalIntensities[light] * multiplier;
                }
            }
        }

        /// <summary>
        /// Cambia la intensidad global de forma suave
        /// </summary>
        public void SetGlobalIntensity(float target, float duration = 1f)
        {
            StopAllCoroutines();
            StartCoroutine(LerpGlobalIntensity(target, duration));
        }

        private System.Collections.IEnumerator LerpGlobalIntensity(float target, float duration)
        {
            float start = globalIntensityMultiplier;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                globalIntensityMultiplier = Mathf.Lerp(start, target, elapsed / duration);
                yield return null;
            }
            
            globalIntensityMultiplier = target;
        }
    }
}
