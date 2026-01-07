using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace VTuberProject.Lighting
{
    /// <summary>
    /// Configura automáticamente Shadow Casters en sprites
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class AutoShadowCaster : MonoBehaviour
    {
        [Header("Shadow Settings")]
        [Tooltip("Activar sombras para este sprite")]
        public bool castsShadows = true;
        
        [Tooltip("Usar la forma del sprite o un rectángulo simple")]
        public bool useRendererSilhouette = true;
        
        [Tooltip("Proyectar sombras en sí mismo")]
        public bool selfShadows = false;
        
        [Header("Shape Settings")]
        [Tooltip("Offset de la sombra (útil para ajustar altura)")]
        public Vector3 shadowOffset = Vector3.zero;
        
        [Tooltip("Simplificación del path (mayor = menos detalle, mejor performance)")]
        [Range(0f, 1f)]
        public float shapeSimplification = 0.1f;

        private ShadowCaster2D shadowCaster;
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            SetupShadowCaster();
        }

        private void Start()
        {
            UpdateShadowCaster();
        }

        private void SetupShadowCaster()
        {
            // Buscar o crear el ShadowCaster2D
            shadowCaster = GetComponent<ShadowCaster2D>();
            
            if (shadowCaster == null && castsShadows)
            {
                shadowCaster = gameObject.AddComponent<ShadowCaster2D>();
            }
        }

        private void UpdateShadowCaster()
        {
            if (shadowCaster == null) return;
            
            shadowCaster.enabled = castsShadows;
            shadowCaster.selfShadows = selfShadows;
            shadowCaster.useRendererSilhouette = useRendererSilhouette;
            
            // Aplicar offset si es necesario
            if (shadowOffset != Vector3.zero)
            {
                transform.localPosition += shadowOffset;
            }
        }

        /// <summary>
        /// Activa/desactiva las sombras en runtime
        /// </summary>
        public void SetShadowsEnabled(bool enabled)
        {
            castsShadows = enabled;
            if (shadowCaster != null)
            {
                shadowCaster.enabled = enabled;
            }
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                UpdateShadowCaster();
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Add Shadow Caster")]
        private void AddShadowCasterComponent()
        {
            if (shadowCaster == null)
            {
                shadowCaster = gameObject.AddComponent<ShadowCaster2D>();
                shadowCaster.useRendererSilhouette = useRendererSilhouette;
                shadowCaster.selfShadows = selfShadows;
                Debug.Log($"Shadow Caster added to {gameObject.name}");
            }
        }

        [ContextMenu("Remove Shadow Caster")]
        private void RemoveShadowCasterComponent()
        {
            if (shadowCaster != null)
            {
                DestroyImmediate(shadowCaster);
                Debug.Log($"Shadow Caster removed from {gameObject.name}");
            }
        }
#endif
    }
}
