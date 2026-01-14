using UnityEngine;
using UnityEngine.UI;

namespace VTuberProject.Lighting
{
    /// <summary>
    /// Reloj analógico visual con manecillas
    /// </summary>
    public class AnalogClock : MonoBehaviour
    {
        [Header("References")]
        public DayNightCycle dayNightCycle;
        
        [Header("Clock Hands")]
        [Tooltip("Transform de la manecilla de las horas")]
        public Transform hourHand;
        
        [Tooltip("Transform de la manecilla de los minutos")]
        public Transform minuteHand;
        
        [Tooltip("Transform de la manecilla de los segundos (opcional)")]
        public Transform secondHand;

        [Header("Rotation Settings")]
        [Tooltip("Rotar en sentido horario")]
        public bool clockwise = true;
        
        [Tooltip("Suavizado de la rotación")]
        [Range(0f, 1f)]
        public float smoothing = 0.1f;

        [Header("Visual Settings")]
        [Tooltip("Imagen de fondo del reloj")]
        public Image clockFace;
        
        [Tooltip("Color del reloj de día")]
        public Color dayColor = Color.white;
        
        [Tooltip("Color del reloj de noche")]
        public Color nightColor = new Color(0.7f, 0.8f, 1f);
        
        [Tooltip("Animar el color del reloj")]
        public bool animateClockColor = true;

        private float targetHourRotation;
        private float targetMinuteRotation;
        private float targetSecondRotation;

        private void Start()
        {
            if (dayNightCycle == null)
            {
                dayNightCycle = FindObjectOfType<DayNightCycle>();
            }
        }

        private void Update()
        {
            UpdateClockHands();
            UpdateClockVisuals();
        }

        private void UpdateClockHands()
        {
            if (dayNightCycle == null) return;

            float time = dayNightCycle.currentTime;
            int hours = Mathf.FloorToInt(time);
            float minutes = (time - hours) * 60f;
            float seconds = (minutes - Mathf.Floor(minutes)) * 60f;

            // Calcular rotaciones (360 grados = ciclo completo)
            // Hora: 360° / 12 horas = 30° por hora
            float hourRotation = (hours % 12) * 30f + (minutes / 60f) * 30f;
            
            // Minutos: 360° / 60 minutos = 6° por minuto
            float minuteRotation = minutes * 6f;
            
            // Segundos: 360° / 60 segundos = 6° por segundo
            float secondRotation = seconds * 6f;

            // Invertir si no es clockwise
            if (!clockwise)
            {
                hourRotation = -hourRotation;
                minuteRotation = -minuteRotation;
                secondRotation = -secondRotation;
            }

            // Aplicar rotaciones con suavizado
            if (hourHand != null)
            {
                targetHourRotation = hourRotation;
                float currentRotation = hourHand.localEulerAngles.z;
                float newRotation = Mathf.LerpAngle(currentRotation, targetHourRotation, smoothing > 0 ? Time.deltaTime / smoothing : 1f);
                hourHand.localEulerAngles = new Vector3(0, 0, newRotation);
            }

            if (minuteHand != null)
            {
                targetMinuteRotation = minuteRotation;
                float currentRotation = minuteHand.localEulerAngles.z;
                float newRotation = Mathf.LerpAngle(currentRotation, targetMinuteRotation, smoothing > 0 ? Time.deltaTime / smoothing : 1f);
                minuteHand.localEulerAngles = new Vector3(0, 0, newRotation);
            }

            if (secondHand != null)
            {
                targetSecondRotation = secondRotation;
                float currentRotation = secondHand.localEulerAngles.z;
                float newRotation = Mathf.LerpAngle(currentRotation, targetSecondRotation, smoothing > 0 ? Time.deltaTime / smoothing : 1f);
                secondHand.localEulerAngles = new Vector3(0, 0, newRotation);
            }
        }

        private void UpdateClockVisuals()
        {
            if (!animateClockColor || clockFace == null || dayNightCycle == null) return;

            // Cambiar color del reloj según hora del día
            float t = 0f;
            
            if (dayNightCycle.currentTime >= 8f && dayNightCycle.currentTime < 18f)
            {
                t = 1f; // Día
            }
            else if (dayNightCycle.currentTime >= 6f && dayNightCycle.currentTime < 8f)
            {
                t = Mathf.InverseLerp(6f, 8f, dayNightCycle.currentTime); // Amanecer
            }
            else if (dayNightCycle.currentTime >= 18f && dayNightCycle.currentTime < 20f)
            {
                t = 1f - Mathf.InverseLerp(18f, 20f, dayNightCycle.currentTime); // Atardecer
            }
            
            clockFace.color = Color.Lerp(nightColor, dayColor, t);
        }

        /// <summary>
        /// Establece el tiempo manualmente (útil para testing)
        /// </summary>
        public void SetTime(float hours)
        {
            if (dayNightCycle != null)
            {
                dayNightCycle.SetTime(hours);
            }
        }
    }
}
