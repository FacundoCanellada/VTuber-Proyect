using UnityEngine;
using UnityEngine.Rendering.Universal;
using System;

namespace VTuberProject.Lighting
{
    /// <summary>
    /// Sistema de ciclo día/noche que controla el tiempo y la iluminación
    /// </summary>
    public class DayNightCycle : MonoBehaviour
    {
        [Header("Time Settings")]
        [Tooltip("Hora actual del día (0-24)")]
        [Range(0f, 24f)]
        public float currentTime = 12f;
        
        [Tooltip("Duración de un día completo en segundos (en tiempo real)")]
        [Range(10f, 3600f)]
        public float dayDurationInSeconds = 120f;
        
        [Tooltip("El tiempo avanza automáticamente")]
        public bool autoAdvanceTime = true;
        
        [Tooltip("Velocidad del tiempo (1 = normal, 2 = doble velocidad)")]
        [Range(0f, 10f)]
        public float timeScale = 1f;

        [Header("Light References")]
        [Tooltip("Luz global (sol/luna)")]
        public Light2D globalLight;
        
        [Tooltip("Manager de luces de la escena")]
        public SceneLightingManager lightingManager;

        [Header("Day/Night Colors")]
        [Tooltip("Color de la luz al amanecer")]
        public Color sunriseColor = new Color(1f, 0.7f, 0.4f);
        
        [Tooltip("Color de la luz al mediodía - Cálido y suave")]
        public Color dayColor = new Color(1f, 0.92f, 0.85f);
        
        [Tooltip("Color de la luz al atardecer - Rojizo/Anaranjado")]
        public Color sunsetColor = new Color(1f, 0.45f, 0.25f);
        
        [Tooltip("Color de la luz de noche - Fría (luz artificial exterior)")]
        public Color nightColor = new Color(0.35f, 0.45f, 0.65f);

        [Header("Intensity Settings")]
        [Tooltip("Intensidad de luz al amanecer")]
        [Range(0f, 2f)]
        public float sunriseIntensity = 0.5f;
        
        [Tooltip("Intensidad de luz al mediodía - Tenue y difusa")]
        [Range(0f, 2f)]
        public float dayIntensity = 0.7f;
        
        [Tooltip("Intensidad de luz al atardecer - Menos intensidad, más contraste")]
        [Range(0f, 2f)]
        public float sunsetIntensity = 0.5f;
        
        [Tooltip("Intensidad de luz de noche - Muy reducida")]
        [Range(0f, 2f)]
        public float nightIntensity = 0.15f;

        [Header("Time of Day Thresholds")]
        [Range(0f, 12f)]
        public float sunriseHour = 6f;
        
        [Range(0f, 18f)]
        public float dayStartHour = 8f;
        
        [Range(12f, 20f)]
        public float sunsetStartHour = 18f;
        
        [Range(18f, 24f)]
        public float nightStartHour = 20f;

        [Header("Indoor Lights Control")]
        [Tooltip("Hora en que se encienden las luces interiores automáticas")]
        [Range(12f, 24f)]
        public float lightsOnHour = 19f;
        
        [Tooltip("Hora en que se apagan las luces interiores automáticas")]
        [Range(0f, 12f)]
        public float lightsOffHour = 7f;

        [Header("Window Light Settings")]
        [Tooltip("Multiplicador para reducir intensidad de luces de ventana (cortinas)")]
        [Range(0f, 1f)]
        public float windowLightMultiplier = 0.4f; // 60% de reducción

        [Header("Debug")]
        public bool showDebugInfo = true;

        // Estado interno
        private TimeOfDay currentPeriod = TimeOfDay.Day;
        private bool indoorLightsOn = false;
        private float timeChangePerSecond;

        public enum TimeOfDay
        {
            Night,      // 0-6 y 20-24
            Sunrise,    // 6-8
            Day,        // 8-18
            Sunset      // 18-20
        }

        private void Start()
        {
            CalculateTimeChangeRate();
            UpdateLighting();
        }

        private void Update()
        {
            if (autoAdvanceTime)
            {
                AdvanceTime();
            }
            
            UpdateLighting();
            UpdateIndoorLights();
        }

        private void CalculateTimeChangeRate()
        {
            // Calcular cuánto avanza el tiempo por segundo (24 horas / duración en segundos)
            timeChangePerSecond = 24f / dayDurationInSeconds;
        }

        private void AdvanceTime()
        {
            currentTime += timeChangePerSecond * timeScale * Time.deltaTime;
            
            // Wrap alrededor de 24 horas
            if (currentTime >= 24f)
            {
                currentTime -= 24f;
            }
        }

        private void UpdateLighting()
        {
            if (globalLight == null) return;

            // Determinar período del día
            currentPeriod = GetTimeOfDay();

            // Calcular color e intensidad basado en interpolación
            Color targetColor;
            float targetIntensity;

            if (currentTime >= nightStartHour || currentTime < sunriseHour)
            {
                // Noche
                targetColor = nightColor;
                targetIntensity = nightIntensity;
            }
            else if (currentTime >= sunriseHour && currentTime < dayStartHour)
            {
                // Amanecer (transición de noche a día)
                float t = Mathf.InverseLerp(sunriseHour, dayStartHour, currentTime);
                targetColor = Color.Lerp(sunriseColor, dayColor, t);
                targetIntensity = Mathf.Lerp(sunriseIntensity, dayIntensity, t);
            }
            else if (currentTime >= dayStartHour && currentTime < sunsetStartHour)
            {
                // Día
                targetColor = dayColor;
                targetIntensity = dayIntensity;
            }
            else if (currentTime >= sunsetStartHour && currentTime < nightStartHour)
            {
                // Atardecer (transición de día a noche)
                float t = Mathf.InverseLerp(sunsetStartHour, nightStartHour, currentTime);
                targetColor = Color.Lerp(sunsetColor, nightColor, t);
                targetIntensity = Mathf.Lerp(sunsetIntensity, nightIntensity, t);
            }
            else
            {
                targetColor = dayColor;
                targetIntensity = dayIntensity;
            }

            // Aplicar con interpolación suave
            globalLight.color = Color.Lerp(globalLight.color, targetColor, Time.deltaTime * 2f);
            globalLight.intensity = Mathf.Lerp(globalLight.intensity, targetIntensity, Time.deltaTime * 2f);
        }

        private void UpdateIndoorLights()
        {
            if (lightingManager == null) return;

            // Determinar si las luces deben estar encendidas
            bool shouldBeLightsOn;
            
            if (lightsOnHour > lightsOffHour)
            {
                // Caso normal: encienden por la tarde, apagan por la mañana
                shouldBeLightsOn = currentTime >= lightsOnHour || currentTime < lightsOffHour;
            }
            else
            {
                // Caso especial si alguien configura al revés
                shouldBeLightsOn = currentTime >= lightsOnHour && currentTime < lightsOffHour;
            }

            // Solo cambiar si el estado es diferente (evita cambios constantes)
            if (shouldBeLightsOn != indoorLightsOn)
            {
                indoorLightsOn = shouldBeLightsOn;
                lightingManager.ToggleIndoorLights(indoorLightsOn);
                
                if (showDebugInfo)
                {
                    Debug.Log($"Indoor lights {(indoorLightsOn ? "ON" : "OFF")} at {currentTime:F1}h");
                }
            }
            
            // Aplicar multiplicador a luces de ventana (cortinas)
            lightingManager.SetOutdoorLightsMultiplier(windowLightMultiplier);
        }

        private TimeOfDay GetTimeOfDay()
        {
            if (currentTime >= sunriseHour && currentTime < dayStartHour)
                return TimeOfDay.Sunrise;
            else if (currentTime >= dayStartHour && currentTime < sunsetStartHour)
                return TimeOfDay.Day;
            else if (currentTime >= sunsetStartHour && currentTime < nightStartHour)
                return TimeOfDay.Sunset;
            else
                return TimeOfDay.Night;
        }

        /// <summary>
        /// Establece la hora actual del día
        /// </summary>
        public void SetTime(float hour)
        {
            currentTime = Mathf.Clamp(hour, 0f, 24f);
        }

        /// <summary>
        /// Salta a un período específico del día
        /// </summary>
        public void SetTimeOfDay(TimeOfDay period)
        {
            switch (period)
            {
                case TimeOfDay.Night:
                    currentTime = 0f;
                    break;
                case TimeOfDay.Sunrise:
                    currentTime = sunriseHour;
                    break;
                case TimeOfDay.Day:
                    currentTime = 12f;
                    break;
                case TimeOfDay.Sunset:
                    currentTime = sunsetStartHour;
                    break;
            }
        }

        /// <summary>
        /// Obtiene el tiempo actual como string legible
        /// </summary>
        public string GetTimeString()
        {
            int hours = Mathf.FloorToInt(currentTime);
            int minutes = Mathf.FloorToInt((currentTime - hours) * 60f);
            return $"{hours:D2}:{minutes:D2}";
        }

        /// <summary>
        /// Obtiene el período del día actual
        /// </summary>
        public TimeOfDay GetCurrentPeriod()
        {
            return currentPeriod;
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                CalculateTimeChangeRate();
            }
        }

        private void OnGUI()
        {
            if (!showDebugInfo) return;

            // Mostrar info de debug en la esquina superior izquierda
            GUIStyle style = new GUIStyle();
            style.fontSize = 16;
            style.normal.textColor = Color.white;
            style.padding = new RectOffset(10, 10, 10, 10);

            string info = $"Time: {GetTimeString()}\n";
            info += $"Period: {currentPeriod}\n";
            info += $"Indoor Lights: {(indoorLightsOn ? "ON" : "OFF")}\n";
            info += $"Global Light Intensity: {(globalLight != null ? globalLight.intensity.ToString("F2") : "N/A")}";

            GUI.Label(new Rect(10, 10, 300, 100), info, style);
        }
    }
}
