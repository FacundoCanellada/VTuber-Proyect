using UnityEngine;
using UnityEngine.Rendering.Universal;
using System;

namespace VTuberProject.Lighting
{
    /// <summary>
    /// Luz que puede ser encendida/apagada independientemente del ciclo día/noche
    /// Componente reutilizable y escalable
    /// </summary>
    [RequireComponent(typeof(Light2D))]
    public class InteractableLight : MonoBehaviour
    {
        [Header("Light Settings")]
        [Tooltip("La luz está encendida al inicio")]
        public bool startOn = false;
        
        [Tooltip("Intensidad cuando está encendida")]
        [Range(0f, 5f)]
        public float onIntensity = 1f;
        
        [Tooltip("Intensidad cuando está apagada")]
        [Range(0f, 1f)]
        public float offIntensity = 0f;
        
        [Tooltip("Velocidad de transición (segundos)")]
        [Range(0f, 2f)]
        public float transitionSpeed = 0.3f;

        [Header("Optional Effects")]
        [Tooltip("GameObject adicional a activar/desactivar (ej: sprites de luz)")]
        public GameObject lightEffectObject;
        
        [Tooltip("Componente SpriteRenderer para cambiar color/alpha")]
        public SpriteRenderer lightSprite;
        
        [Tooltip("Partículas al encender/apagar")]
        public ParticleSystem particles;

        [Header("Sound")]
        [Tooltip("Sonido al encender")]
        public AudioClip turnOnSound;
        
        [Tooltip("Sonido al apagar")]
        public AudioClip turnOffSound;
        
        [Tooltip("Volumen de sonidos")]
        [Range(0f, 1f)]
        public float soundVolume = 0.5f;

        [Header("Restrictions")]
        [Tooltip("Si es true, la luz solo se podrá encender durante la noche")]
        public bool onlyActiveAtNight = false;
        
        [Tooltip("Referencia al ciclo día/noche (necesaria si onlyActiveAtNight es true)")]
        public DayNightCycle dayNightCycle;

        [Header("State")]
        [SerializeField]
        private bool isOn = false;

        // Referencias
        private Light2D light2D;
        private AudioSource audioSource;
        private float targetIntensity;
        private float currentIntensity;

        // Eventos para otros sistemas
        public event Action<bool> OnLightToggled;

        public bool IsOn => isOn;

        private void Awake()
        {
            light2D = GetComponent<Light2D>();
            
            // Crear AudioSource si hay sonidos configurados
            if (turnOnSound != null || turnOffSound != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.volume = soundVolume;
            }
        }

        private void Start()
        {
            // Establecer estado inicial sin transición
            isOn = startOn;
            targetIntensity = isOn ? onIntensity : offIntensity;
            currentIntensity = targetIntensity;
            light2D.intensity = currentIntensity;
            
            UpdateVisualEffects(isOn);
        }

        private void Update()
        {
            // Transición suave de intensidad
            if (Mathf.Abs(currentIntensity - targetIntensity) > 0.01f)
            {
                float speed = transitionSpeed > 0 ? 1f / transitionSpeed : 10f;
                currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime * speed);
                light2D.intensity = currentIntensity;
                
                // Actualizar sprite si existe
                if (lightSprite != null)
                {
                    Color color = lightSprite.color;
                    color.a = Mathf.Lerp(0f, 1f, currentIntensity / onIntensity);
                    lightSprite.color = color;
                }
            }
        }

        /// <summary>
        /// Enciende la luz
        /// </summary>
        public void TurnOn()
        {
            if (isOn) return;

            // Verificar restricción de noche
            if (onlyActiveAtNight)
            {
                if (dayNightCycle == null) dayNightCycle = FindObjectOfType<DayNightCycle>();
                
                if (dayNightCycle != null)
                {
                    // Si no es de noche, no permitimos encender
                    if (dayNightCycle.GetCurrentPeriod() != DayNightCycle.TimeOfDay.Night)
                    {
                        // Opcional: Podríamos reproducir un sonido de "error" o "click fallido" aquí
                        return;
                    }
                }
            }
            
            isOn = true;
            targetIntensity = onIntensity;
            
            UpdateVisualEffects(true);
            PlaySound(turnOnSound);
            
            OnLightToggled?.Invoke(true);
        }

        /// <summary>
        /// Apaga la luz
        /// </summary>
        public void TurnOff()
        {
            if (!isOn) return;
            
            isOn = false;
            targetIntensity = offIntensity;
            
            UpdateVisualEffects(false);
            PlaySound(turnOffSound);
            
            OnLightToggled?.Invoke(false);
        }

        /// <summary>
        /// Alterna el estado de la luz
        /// </summary>
        public void Toggle()
        {
            if (isOn)
                TurnOff();
            else
                TurnOn();
        }

        /// <summary>
        /// Establece el estado de la luz sin transición
        /// </summary>
        public void SetState(bool state, bool instant = false)
        {
            isOn = state;
            targetIntensity = isOn ? onIntensity : offIntensity;
            
            if (instant)
            {
                currentIntensity = targetIntensity;
                light2D.intensity = currentIntensity;
            }
            
            UpdateVisualEffects(isOn);
        }

        private void UpdateVisualEffects(bool state)
        {
            if (lightEffectObject != null)
            {
                lightEffectObject.SetActive(state);
            }
            
            if (particles != null)
            {
                if (state && !particles.isPlaying)
                {
                    particles.Play();
                }
                else if (!state && particles.isPlaying)
                {
                    particles.Stop();
                }
            }
        }

        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip, soundVolume);
            }
        }

        // Editor helpers
        private void OnValidate()
        {
            if (Application.isPlaying && light2D != null)
            {
                targetIntensity = isOn ? onIntensity : offIntensity;
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Visualizar el rango de la luz en el editor
            if (light2D == null) light2D = GetComponent<Light2D>();
            
            Gizmos.color = isOn ? Color.yellow : Color.gray;
            
            if (light2D != null)
            {
                if (light2D.lightType == Light2D.LightType.Point)
                {
                    Gizmos.DrawWireSphere(transform.position, light2D.pointLightOuterRadius);
                }
                else if (light2D.lightType == Light2D.LightType.Freeform)
                {
                    Gizmos.DrawWireSphere(transform.position, light2D.shapeLightFalloffSize);
                }
            }
        }
    }
}
