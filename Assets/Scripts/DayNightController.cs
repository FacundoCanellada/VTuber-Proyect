using UnityEngine;
using UnityEngine.UI;

namespace VTuberProject.Lighting
{
    /// <summary>
    /// Controlador UI para el sistema de día/noche (opcional)
    /// </summary>
    public class DayNightController : MonoBehaviour
    {
        [Header("References")]
        public DayNightCycle dayNightCycle;

        [Header("UI Elements (Optional)")]
        public Slider timeSlider;
        public Text timeText;
        public Button sunriseButton;
        public Button noonButton;
        public Button sunsetButton;
        public Button nightButton;
        public Toggle autoTimeToggle;

        private void Start()
        {
            if (dayNightCycle == null)
            {
                dayNightCycle = FindObjectOfType<DayNightCycle>();
            }

            SetupUI();
        }

        private void Update()
        {
            UpdateUI();
        }

        private void SetupUI()
        {
            if (timeSlider != null)
            {
                timeSlider.minValue = 0f;
                timeSlider.maxValue = 24f;
                timeSlider.onValueChanged.AddListener(OnTimeSliderChanged);
            }

            if (sunriseButton != null)
                sunriseButton.onClick.AddListener(() => SetTime(DayNightCycle.TimeOfDay.Sunrise));

            if (noonButton != null)
                noonButton.onClick.AddListener(() => SetTime(DayNightCycle.TimeOfDay.Day));

            if (sunsetButton != null)
                sunsetButton.onClick.AddListener(() => SetTime(DayNightCycle.TimeOfDay.Sunset));

            if (nightButton != null)
                nightButton.onClick.AddListener(() => SetTime(DayNightCycle.TimeOfDay.Night));

            if (autoTimeToggle != null)
            {
                autoTimeToggle.isOn = dayNightCycle.autoAdvanceTime;
                autoTimeToggle.onValueChanged.AddListener(OnAutoTimeToggled);
            }
        }

        private void UpdateUI()
        {
            if (dayNightCycle == null) return;

            if (timeSlider != null && !timeSlider.IsActive())
            {
                timeSlider.value = dayNightCycle.currentTime;
            }

            if (timeText != null)
            {
                timeText.text = dayNightCycle.GetTimeString();
            }
        }

        private void OnTimeSliderChanged(float value)
        {
            if (dayNightCycle != null)
            {
                dayNightCycle.autoAdvanceTime = false;
                dayNightCycle.SetTime(value);
                
                if (autoTimeToggle != null)
                {
                    autoTimeToggle.isOn = false;
                }
            }
        }

        private void OnAutoTimeToggled(bool value)
        {
            if (dayNightCycle != null)
            {
                dayNightCycle.autoAdvanceTime = value;
            }
        }

        private void SetTime(DayNightCycle.TimeOfDay period)
        {
            if (dayNightCycle != null)
            {
                dayNightCycle.SetTimeOfDay(period);
            }
        }

        // Métodos públicos para llamar desde otros scripts o eventos
        public void PauseTime()
        {
            if (dayNightCycle != null)
                dayNightCycle.autoAdvanceTime = false;
        }

        public void ResumeTime()
        {
            if (dayNightCycle != null)
                dayNightCycle.autoAdvanceTime = true;
        }

        public void SetTimeScale(float scale)
        {
            if (dayNightCycle != null)
                dayNightCycle.timeScale = scale;
        }
    }
}
