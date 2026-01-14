using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace VTuberProject.Lighting
{
    /// <summary>
    /// Controlador de horario para una luz individual
    /// Permite configurar cuándo se enciende/apaga según el período del día
    /// </summary>
    [RequireComponent(typeof(Light2D))]
    public class LightSchedule : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Ciclo día/noche (se busca automáticamente si está vacío)")]
        public DayNightCycle dayNightCycle;
        
        [Header("Active Periods")]
        [Tooltip("La luz está activa durante la noche (20:00 - 6:00)")]
        public bool activeAtNight = false;
        
        [Tooltip("La luz está activa durante el amanecer (6:00 - 8:00)")]
        public bool activeAtSunrise = false;
        
        [Tooltip("La luz está activa durante el día (8:00 - 18:00)")]
        public bool activeAtDay = false;
        
        [Tooltip("La luz está activa durante el atardecer (18:00 - 20:00)")]
        public bool activeAtSunset = false;

        [Header("Transition Settings")]
        [Tooltip("Transición suave al encender/apagar")]
        public bool smoothTransition = true;
        
        [Tooltip("Duración de la transición en segundos")]
        [Range(0.1f, 5f)]
        public float transitionDuration = 1f;

        [Header("Intensity Control")]
        [Tooltip("Intensidad máxima cuando está activa")]
        [Range(0f, 5f)]
        public float maxIntensity = 1f;
        
        [Tooltip("Intensidad mínima cuando está inactiva (0 = apagada)")]
        [Range(0f, 1f)]
        public float minIntensity = 0f;

        [Header("Custom Time Range (Optional)")]
        [Tooltip("Usar rango de horas personalizado en lugar de períodos")]
        public bool useCustomTimeRange = false;
        
        [Tooltip("Hora de encendido (0-24)")]
        [Range(0f, 24f)]
        public float customStartHour = 19f;
        
        [Tooltip("Hora de apagado (0-24)")]
        [Range(0f, 24f)]
        public float customEndHour = 7f;

        private Light2D light2D;
        private float targetIntensity;
        private float currentIntensity;
        private bool wasActive = false;

        private void Awake()
        {
            light2D = GetComponent<Light2D>();
            currentIntensity = light2D.intensity;
            maxIntensity = light2D.intensity; // Guardar intensidad inicial
        }

        private void Start()
        {
            if (dayNightCycle == null)
            {
                dayNightCycle = FindObjectOfType<DayNightCycle>();
            }

            if (dayNightCycle == null)
            {
                Debug.LogWarning($"LightSchedule en {gameObject.name}: No se encontró DayNightCycle en la escena!");
            }
        }

        private void Update()
        {
            if (dayNightCycle == null || light2D == null) return;

            bool shouldBeActive = ShouldBeActive();
            
            // Determinar intensidad objetivo
            targetIntensity = shouldBeActive ? maxIntensity : minIntensity;

            // Aplicar transición
            if (smoothTransition)
            {
                currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, 
                    Time.deltaTime / transitionDuration);
            }
            else
            {
                currentIntensity = targetIntensity;
            }

            // Aplicar intensidad
            light2D.intensity = currentIntensity;

            // Habilitar/deshabilitar completamente si minIntensity es 0
            if (minIntensity == 0f)
            {
                light2D.enabled = currentIntensity > 0.01f;
            }

            // Log cuando cambia el estado (solo una vez)
            if (shouldBeActive != wasActive)
            {
                wasActive = shouldBeActive;
                Debug.Log($"{gameObject.name}: {(shouldBeActive ? "Encendida" : "Apagada")} a las {dayNightCycle.GetTimeString()}");
            }
        }

        private bool ShouldBeActive()
        {
            if (useCustomTimeRange)
            {
                return IsInCustomTimeRange();
            }
            else
            {
                return IsInActivePeriod();
            }
        }

        private bool IsInActivePeriod()
        {
            DayNightCycle.TimeOfDay currentPeriod = dayNightCycle.GetCurrentPeriod();

            switch (currentPeriod)
            {
                case DayNightCycle.TimeOfDay.Night:
                    return activeAtNight;
                case DayNightCycle.TimeOfDay.Sunrise:
                    return activeAtSunrise;
                case DayNightCycle.TimeOfDay.Day:
                    return activeAtDay;
                case DayNightCycle.TimeOfDay.Sunset:
                    return activeAtSunset;
                default:
                    return false;
            }
        }

        private bool IsInCustomTimeRange()
        {
            float currentTime = dayNightCycle.currentTime;

            if (customStartHour < customEndHour)
            {
                // Rango normal (ej: 8:00 - 18:00)
                return currentTime >= customStartHour && currentTime < customEndHour;
            }
            else
            {
                // Rango que cruza medianoche (ej: 19:00 - 7:00)
                return currentTime >= customStartHour || currentTime < customEndHour;
            }
        }

        /// <summary>
        /// Fuerza el encendido de la luz
        /// </summary>
        public void TurnOn()
        {
            targetIntensity = maxIntensity;
            if (!smoothTransition)
            {
                currentIntensity = maxIntensity;
                light2D.intensity = maxIntensity;
            }
        }

        /// <summary>
        /// Fuerza el apagado de la luz
        /// </summary>
        public void TurnOff()
        {
            targetIntensity = minIntensity;
            if (!smoothTransition)
            {
                currentIntensity = minIntensity;
                light2D.intensity = minIntensity;
            }
        }

        /// <summary>
        /// Establece la intensidad máxima
        /// </summary>
        public void SetMaxIntensity(float intensity)
        {
            maxIntensity = intensity;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (light2D == null)
            {
                light2D = GetComponent<Light2D>();
            }

            // Validar que al menos un período esté activo si no usa custom
            if (!useCustomTimeRange && 
                !activeAtNight && !activeAtSunrise && !activeAtDay && !activeAtSunset)
            {
                Debug.LogWarning($"{gameObject.name}: La luz no tiene ningún período activo!");
            }
        }
#endif
    }
}
