using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace VTuberProject.Lighting
{
    /// <summary>
    /// Añade efecto de parpadeo/flicker a luces (ideal para velas, monitores, etc.)
    /// </summary>
    [RequireComponent(typeof(Light2D))]
    public class LightFlicker : MonoBehaviour
    {
        [Header("Flicker Settings")]
        [Tooltip("Intensidad base de la luz")]
        [Range(0f, 5f)]
        public float baseIntensity = 1f;
        
        [Tooltip("Cantidad de variación en la intensidad")]
        [Range(0f, 1f)]
        public float flickerAmount = 0.1f;
        
        [Tooltip("Velocidad del parpadeo")]
        [Range(0f, 10f)]
        public float flickerSpeed = 5f;
        
        [Header("Advanced")]
        [Tooltip("Usar Perlin noise para parpadeo suave")]
        public bool usePerlinNoise = true;
        
        [Tooltip("Variación aleatoria ocasional")]
        public bool randomSpikes = false;
        
        [Range(0f, 1f)]
        public float spikeChance = 0.01f;

        private Light2D light2D;
        private float timeOffset;

        private void Awake()
        {
            light2D = GetComponent<Light2D>();
            timeOffset = Random.Range(0f, 1000f); // Offset aleatorio para cada luz
        }

        private void OnEnable()
        {
            if (light2D == null)
                light2D = GetComponent<Light2D>();
            // no fijamos baseIntensity aquí: tomaremos la intensidad actual cada frame
            timeOffset = Random.Range(0f, 1000f);
        }

        /// <summary>
        /// Usamos LateUpdate para aplicar el parpadeo DESPUÉS de que otros managers (ej. SceneLightingManager)
        /// hayan establecido la intensidad base en Update(). Así evitamos que otros scripts sobrescriban el efecto.
        /// </summary>
        private void LateUpdate()
        {
            if (light2D == null) return;

            float flickerValue;

            float t = (Time.time + timeOffset) * flickerSpeed;
            if (usePerlinNoise)
            {
                flickerValue = Mathf.PerlinNoise(t, 0f);
            }
            else
            {
                flickerValue = (Mathf.Sin(t) + 1f) * 0.5f;
            }

            // Normalizar a [-1,1]
            float normalized = (flickerValue - 0.5f) * 2f;

            // Calcular la variación absoluta basada en la intensidad actual (la que dejó el manager)
            float currentBase = light2D.intensity;
            float variation = currentBase * (normalized * flickerAmount);

            // Spikes aleatorios ocasionales (relativo a currentBase)
            if (randomSpikes && Random.value < spikeChance)
            {
                variation += Random.Range(-1f, 1f) * currentBase * flickerAmount * 2f;
            }

            // Aplicar la variación sobre la intensidad que dejó el manager
            light2D.intensity = Mathf.Max(0f, currentBase + variation);
        }

        /// <summary>
        /// Establece la intensidad base sin modificar el parpadeo
        /// </summary>
        public void SetBaseIntensity(float intensity)
        {
            baseIntensity = intensity;
        }
    }
}
