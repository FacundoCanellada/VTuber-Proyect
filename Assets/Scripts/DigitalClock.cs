using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace VTuberProject.Lighting
{
    /// <summary>
    /// Reloj digital que muestra la hora del ciclo día/noche
    /// </summary>
    public class DigitalClock : MonoBehaviour
    {
        [Header("References")]
        public DayNightCycle dayNightCycle;
        
        [Header("UI Elements")]
        [Tooltip("Text component para mostrar la hora (Legacy UI)")]
        public Text timeText;
        
        [Tooltip("TextMeshPro component para mostrar la hora (recomendado)")]
        public TextMeshProUGUI timeTextTMP;
        
        [Header("Display Settings")]
        [Tooltip("Formato de 12 horas (AM/PM) o 24 horas")]
        public bool use12HourFormat = false;
        
        [Tooltip("Mostrar segundos")]
        public bool showSeconds = false;
        
        [Tooltip("Mostrar el período del día (Día, Noche, etc.)")]
        public bool showPeriodOfDay = true;

        [Header("Styling")]
        public Color dayTimeColor = Color.white;
        public Color nightTimeColor = new Color(0.7f, 0.8f, 1f);
        public bool animateColor = true;

        private void Start()
        {
            if (dayNightCycle == null)
            {
                dayNightCycle = FindObjectOfType<DayNightCycle>();
            }
        }

        private void Update()
        {
            UpdateClock();
        }

        private void UpdateClock()
        {
            if (dayNightCycle == null) return;

            string displayTime = FormatTime(dayNightCycle.currentTime);
            
            // Actualizar el texto
            if (timeText != null)
            {
                timeText.text = displayTime;
                
                if (animateColor)
                {
                    timeText.color = GetColorForTime();
                }
            }
            
            if (timeTextTMP != null)
            {
                timeTextTMP.text = displayTime;
                
                if (animateColor)
                {
                    timeTextTMP.color = GetColorForTime();
                }
            }
        }

        private string FormatTime(float time)
        {
            int hours = Mathf.FloorToInt(time);
            int minutes = Mathf.FloorToInt((time - hours) * 60f);
            int seconds = Mathf.FloorToInt(((time - hours) * 60f - minutes) * 60f);

            string result = "";

            if (use12HourFormat)
            {
                string period = hours >= 12 ? "PM" : "AM";
                int displayHours = hours > 12 ? hours - 12 : (hours == 0 ? 12 : hours);
                
                if (showSeconds)
                {
                    result = $"{displayHours:D2}:{minutes:D2}:{seconds:D2} {period}";
                }
                else
                {
                    result = $"{displayHours:D2}:{minutes:D2} {period}";
                }
            }
            else
            {
                if (showSeconds)
                {
                    result = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
                }
                else
                {
                    result = $"{hours:D2}:{minutes:D2}";
                }
            }

            // Añadir período del día si está habilitado
            if (showPeriodOfDay && dayNightCycle != null)
            {
                string periodName = GetPeriodName(dayNightCycle.GetCurrentPeriod());
                result += $"\n{periodName}";
            }

            return result;
        }

        private string GetPeriodName(DayNightCycle.TimeOfDay period)
        {
            switch (period)
            {
                case DayNightCycle.TimeOfDay.Night:
                    return "Noche";
                case DayNightCycle.TimeOfDay.Sunrise:
                    return "Amanecer";
                case DayNightCycle.TimeOfDay.Day:
                    return "Día";
                case DayNightCycle.TimeOfDay.Sunset:
                    return "Atardecer";
                default:
                    return "";
            }
        }

        private Color GetColorForTime()
        {
            if (dayNightCycle == null) return Color.white;

            // Interpolar color basado en si es día o noche
            float t = 0f;
            
            if (dayNightCycle.currentTime >= 8f && dayNightCycle.currentTime < 18f)
            {
                // Es de día
                t = 1f;
            }
            else if (dayNightCycle.currentTime >= 6f && dayNightCycle.currentTime < 8f)
            {
                // Transición amanecer
                t = Mathf.InverseLerp(6f, 8f, dayNightCycle.currentTime);
            }
            else if (dayNightCycle.currentTime >= 18f && dayNightCycle.currentTime < 20f)
            {
                // Transición atardecer
                t = 1f - Mathf.InverseLerp(18f, 20f, dayNightCycle.currentTime);
            }
            
            return Color.Lerp(nightTimeColor, dayTimeColor, t);
        }
    }
}
