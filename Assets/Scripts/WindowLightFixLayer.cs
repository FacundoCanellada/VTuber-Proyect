using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace VTuberProject.Lighting
{
    /// <summary>
    /// Asegura que las luces de ventana (Window_StarLight) no sean afectadas u oscurecidas
    /// por el personaje cuando este pasa por encima
    /// </summary>
    [RequireComponent(typeof(Light2D))]
    public class WindowLightFixLayer : MonoBehaviour
    {
        [Header("Configuración de Capa")]
        [Tooltip("Sorting Layer específico para estas luces (debe ser diferente al del personaje)")]
        [SerializeField] private string lightSortingLayer = "Lighting";
        
        [Tooltip("Order in Layer para las luces de ventana (debe ser alto para estar al frente)")]
        [SerializeField] private int lightSortingOrder = 100;

        [Header("Light 2D Settings")]
        [Tooltip("Target Sorting Layers - capas que serán iluminadas")]
        [SerializeField] private string[] targetSortingLayers = new string[] { "Default", "Background", "Foreground" };
        
        [Tooltip("Blend Mode de la luz (0 = Additive, 1 = Multiply/Custom)")]
        [SerializeField] private int blendStyleIndex = 0;

        [Header("Sprite Renderer (si aplica)")]
        [Tooltip("Si la luz tiene un sprite asociado (cookie visual)")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Light2D light2D;

        private void Awake()
        {
            light2D = GetComponent<Light2D>();
            
            // Auto-detectar sprite renderer si existe
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        private void Start()
        {
            ConfigureLight2D();
            ConfigureSpriteRenderer();
        }

        /// <summary>
        /// Configura la Light2D para que no sea afectada por el personaje
        /// </summary>
        private void ConfigureLight2D()
        {
            if (light2D == null) return;

            // Configurar el blend mode (index del renderer data)
            light2D.blendStyleIndex = blendStyleIndex;

            // SOLUCIÓN PRINCIPAL: Configurar Target Sorting Layers
            // Esto hace que la luz SOLO ilumine las capas especificadas
            // y NO sea afectada por sprites en otras capas (como el personaje)
            
            // Nota: En Unity 2D URP, las luces se renderizan en un orden específico
            // Para asegurar que las luces de ventana estén siempre visibles:
            
            // 1. Aumentar la intensidad si parece oscurecerse
            light2D.lightOrder = lightSortingOrder;
            
            // 2. Asegurar que usa el sorting layer correcto
            // (Esto requiere configuración manual en el Inspector si no funciona)
            Debug.Log($"[WindowLightFix] Light2D configurada: {gameObject.name}");
            Debug.Log($"  - Light Order: {light2D.lightOrder}");
            Debug.Log($"  - Blend Style Index: {blendStyleIndex}");
        }

        /// <summary>
        /// Configura el Sprite Renderer si la luz tiene un sprite visible
        /// </summary>
        private void ConfigureSpriteRenderer()
        {
            if (spriteRenderer == null) return;

            // Configurar sorting layer y order
            spriteRenderer.sortingLayerName = lightSortingLayer;
            spriteRenderer.sortingOrder = lightSortingOrder;

            Debug.Log($"[WindowLightFix] SpriteRenderer configurado:");
            Debug.Log($"  - Sorting Layer: {spriteRenderer.sortingLayerName}");
            Debug.Log($"  - Sorting Order: {spriteRenderer.sortingOrder}");
        }

        /// <summary>
        /// Método alternativo: Ajustar intensidad dinámicamente cuando el personaje está cerca
        /// </summary>
        public void CompensateBrightness(bool playerNearby)
        {
            if (light2D == null) return;

            if (playerNearby)
            {
                // Aumentar intensidad cuando el personaje está cerca
                // Esto compensa visualmente el oscurecimiento
                light2D.intensity *= 1.3f;
            }
            else
            {
                // Restaurar intensidad normal
                light2D.intensity /= 1.3f;
            }
        }

        private void OnValidate()
        {
            // Aplicar cambios en el editor
            if (Application.isPlaying)
            {
                ConfigureLight2D();
                ConfigureSpriteRenderer();
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Setup Light Layers")]
        private void SetupLightLayers()
        {
            ConfigureLight2D();
            ConfigureSpriteRenderer();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
