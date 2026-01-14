using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace VTuberProject.Lighting
{
    /// <summary>
    /// Configura una luz para proyectar formas específicas (como estrellas de cortinas)
    /// </summary>
    [RequireComponent(typeof(Light2D))]
    public class WindowLightProjector : MonoBehaviour
    {
        [Header("Light Cookie")]
        [Tooltip("Sprite que se proyectará (ej: estrellas de las cortinas)")]
        public Sprite lightCookie;
        
        [Header("Projection Settings")]
        [Tooltip("Ángulo de la luz (para simular dirección del sol)")]
        [Range(-180f, 180f)]
        public float projectionAngle = 45f;
        
        [Tooltip("Escala del patrón proyectado")]
        [Range(0.1f, 5f)]
        public float patternScale = 1f;
        
        [Header("Light Properties")]
        [Tooltip("Color de la luz exterior (sol/cielo)")]
        public Color lightColor = new Color(1f, 0.95f, 0.85f); // Luz solar cálida
        
        [Range(0f, 30f)]
        public float intensity = 1.2f;
        
        [Header("Animation")]
        [Tooltip("Simular movimiento del sol (cambio de ángulo lento)")]
        public bool animateSunMovement = false;
        
        [Range(0f, 10f)]
        public float sunMovementSpeed = 1f;
        
        [Range(-45f, 45f)]
        public float sunMovementRange = 15f;

        private Light2D light2D;
        private float initialAngle;

        private void Awake()
        {
            light2D = GetComponent<Light2D>();
            initialAngle = projectionAngle;
        }

        private void Start()
        {
            ApplyLightCookie();
            UpdateLightProperties();
        }

        private void Update()
        {
            if (animateSunMovement)
            {
                float time = Time.time * sunMovementSpeed;
                projectionAngle = initialAngle + Mathf.Sin(time) * sunMovementRange;
                transform.rotation = Quaternion.Euler(0, 0, projectionAngle);
            }
            
            UpdateLightProperties();
        }

        private void ApplyLightCookie()
        {
            if (light2D == null || lightCookie == null) return;

            // Para Unity 2021.2+ y URP 12+, el cookie se asigna directamente
            // Si no funciona la reflexión, deberás asignarlo manualmente en el Inspector
            try
            {
                #if UNITY_2021_2_OR_NEWER
                // Método directo para versiones nuevas
                var lightType = light2D.GetType();
                var cookieField = lightType.GetField("m_LightCookieSprite", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (cookieField != null)
                {
                    cookieField.SetValue(light2D, lightCookie);
                    Debug.Log($"Light cookie sprite assigned: {lightCookie.name}");
                }
                else
                {
                    Debug.LogWarning("Could not find light cookie field. Please assign manually in Inspector under Light 2D > Cookie.");
                }
                #else
                // Método con reflexión para versiones anteriores
                var lightType = light2D.GetType();
                var prop = lightType.GetProperty("lightCookieSprite");
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(light2D, lightCookie, null);
                    Debug.Log($"Light cookie sprite assigned via property: {lightCookie.name}");
                }
                #endif
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Could not apply light cookie automatically: {e.Message}");
                Debug.LogWarning("Please assign the Light Cookie Sprite manually in the Light 2D component Inspector.");
            }
        }

        private void UpdateLightProperties()
        {
            if (light2D == null) return;
            
            light2D.color = lightColor;
            light2D.intensity = intensity;
            
            // Aplicar rotación
            transform.rotation = Quaternion.Euler(0, 0, projectionAngle);
        }

        private void OnValidate()
        {
            if (light2D != null && Application.isPlaying)
            {
                ApplyLightCookie();
                UpdateLightProperties();
            }
        }

        /// <summary>
        /// Cambia el sprite de proyección en tiempo de ejecución
        /// </summary>
        public void SetLightCookie(Sprite newCookie)
        {
            lightCookie = newCookie;
            ApplyLightCookie();
        }

        /// <summary>
        /// Ajusta la intensidad de forma suave
        /// </summary>
        public void FadeIntensity(float targetIntensity, float duration)
        {
            StopAllCoroutines();
            StartCoroutine(LerpIntensity(targetIntensity, duration));
        }

        private System.Collections.IEnumerator LerpIntensity(float target, float duration)
        {
            float start = intensity;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                intensity = Mathf.Lerp(start, target, elapsed / duration);
                yield return null;
            }
            
            intensity = target;
        }
    }
}
