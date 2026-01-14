using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

namespace VTuberProject.Lighting
{
    /// <summary>
    /// Presets predefinidos para diferentes tipos de luces y sus horarios
    /// </summary>
    [CreateAssetMenu(fileName = "LightSchedulePreset", menuName = "VTuber/Light Schedule Preset")]
    public class LightSchedulePreset : ScriptableObject
    {
        [Header("Schedule Settings")]
        public string presetName = "Custom";
        
        [TextArea(2, 4)]
        public string description = "Descripción del preset";

        [Header("Active Periods")]
        public bool activeAtNight = false;
        public bool activeAtSunrise = false;
        public bool activeAtDay = false;
        public bool activeAtSunset = false;

        [Header("Custom Time Range")]
        public bool useCustomTimeRange = false;
        [Range(0f, 24f)]
        public float customStartHour = 19f;
        [Range(0f, 24f)]
        public float customEndHour = 7f;

        [Header("Intensity")]
        [Range(0f, 5f)]
        public float maxIntensity = 1f;
        [Range(0f, 1f)]
        public float minIntensity = 0f;

        [Header("Transition")]
        public bool smoothTransition = true;
        [Range(0.1f, 5f)]
        public float transitionDuration = 1f;

        /// <summary>
        /// Aplica este preset a un LightSchedule
        /// </summary>
        public void ApplyToSchedule(LightSchedule schedule)
        {
            if (schedule == null) return;

            schedule.activeAtNight = activeAtNight;
            schedule.activeAtSunrise = activeAtSunrise;
            schedule.activeAtDay = activeAtDay;
            schedule.activeAtSunset = activeAtSunset;
            
            schedule.useCustomTimeRange = useCustomTimeRange;
            schedule.customStartHour = customStartHour;
            schedule.customEndHour = customEndHour;
            
            schedule.maxIntensity = maxIntensity;
            schedule.minIntensity = minIntensity;
            
            schedule.smoothTransition = smoothTransition;
            schedule.transitionDuration = transitionDuration;

            Debug.Log($"Preset '{presetName}' aplicado a {schedule.gameObject.name}");
        }

        #region Presets Predefinidos

        public static LightSchedulePreset CreateNightOnlyPreset()
        {
            var preset = CreateInstance<LightSchedulePreset>();
            preset.presetName = "Night Only";
            preset.description = "Luz que solo se enciende de noche";
            preset.activeAtNight = true;
            preset.activeAtSunrise = false;
            preset.activeAtDay = false;
            preset.activeAtSunset = false;
            preset.maxIntensity = 1f;
            preset.minIntensity = 0f;
            preset.smoothTransition = true;
            preset.transitionDuration = 2f;
            return preset;
        }

        public static LightSchedulePreset CreateDayOnlyPreset()
        {
            var preset = CreateInstance<LightSchedulePreset>();
            preset.presetName = "Day Only";
            preset.description = "Luz que solo se enciende de día";
            preset.activeAtNight = false;
            preset.activeAtSunrise = true;
            preset.activeAtDay = true;
            preset.activeAtSunset = false;
            preset.maxIntensity = 1.5f;
            preset.minIntensity = 0f;
            preset.smoothTransition = true;
            preset.transitionDuration = 2f;
            return preset;
        }

        public static LightSchedulePreset CreateEveningToNightPreset()
        {
            var preset = CreateInstance<LightSchedulePreset>();
            preset.presetName = "Evening to Night";
            preset.description = "Luz que se enciende al atardecer y se apaga al amanecer";
            preset.activeAtNight = true;
            preset.activeAtSunrise = false;
            preset.activeAtDay = false;
            preset.activeAtSunset = true;
            preset.maxIntensity = 1f;
            preset.minIntensity = 0f;
            preset.smoothTransition = true;
            preset.transitionDuration = 1.5f;
            return preset;
        }

        public static LightSchedulePreset CreateAlwaysOnPreset()
        {
            var preset = CreateInstance<LightSchedulePreset>();
            preset.presetName = "Always On";
            preset.description = "Luz siempre encendida con variación de intensidad";
            preset.activeAtNight = true;
            preset.activeAtSunrise = true;
            preset.activeAtDay = true;
            preset.activeAtSunset = true;
            preset.maxIntensity = 1f;
            preset.minIntensity = 0.3f; // Nunca se apaga del todo
            preset.smoothTransition = true;
            preset.transitionDuration = 1f;
            return preset;
        }

        #endregion
    }
}
