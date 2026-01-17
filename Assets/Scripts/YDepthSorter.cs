using UnityEngine;

namespace VTuberProject
{
    /// <summary>
    /// Ajusta automáticamente el sorting order basado en la posición Y
    /// Permite que objetos se rendericen correctamente según profundidad
    /// </summary>
    [ExecuteAlways]
    public class YDepthSorter : MonoBehaviour
    {
        [Header("Sorting Settings")]
        [Tooltip("Actualizar sorting order automáticamente")]
        public bool autoUpdate = true;
        
        [Tooltip("Offset base para el sorting order")]
        public int baseSortingOrder = 0;
        
        [Tooltip("Multiplicador de precisión (mayor = más niveles)")]
        [Range(1, 100)]
        public int precision = 10;
        
        [Tooltip("Invertir dirección (objetos arriba = frente)")]
        public bool invertDirection = false;

        [Header("References")]
        [Tooltip("Renderer a ordenar (null = busca automáticamente)")]
        public Renderer targetRenderer;

        [Header("Optional Pivot Offset")]
        [Tooltip("Offset en Y para el punto de sorting (ej: pies del personaje)")]
        public float pivotYOffset = 0f;

        private float lastYPosition;
        private int lastSortingOrder;

        private void Awake()
        {
            if (targetRenderer == null)
            {
                targetRenderer = GetComponent<Renderer>();
            }
            
            if (targetRenderer == null)
            {
                targetRenderer = GetComponentInChildren<Renderer>();
            }
        }

        private void Start()
        {
            UpdateSortingOrder();
        }

        private void LateUpdate()
        {
            if (autoUpdate)
            {
                // Solo actualizar si la posición cambió (optimización)
                float currentY = transform.position.y + pivotYOffset;
                if (Mathf.Abs(currentY - lastYPosition) > 0.01f)
                {
                    UpdateSortingOrder();
                    lastYPosition = currentY;
                }
            }
        }

        /// <summary>
        /// Actualiza el sorting order basado en la posición Y actual
        /// </summary>
        public void UpdateSortingOrder()
        {
            if (targetRenderer == null) return;

            float yPosition = transform.position.y + pivotYOffset;
            
            // Calcular sorting order
            // Objetos más abajo en Y = mayor sorting order = render al frente
            int calculatedOrder = baseSortingOrder + Mathf.RoundToInt(-yPosition * precision);
            
            if (invertDirection)
            {
                calculatedOrder = baseSortingOrder + Mathf.RoundToInt(yPosition * precision);
            }

            // Aplicar a SpriteRenderer
            if (targetRenderer is SpriteRenderer spriteRenderer)
            {
                spriteRenderer.sortingOrder = calculatedOrder;
                lastSortingOrder = calculatedOrder;
            }
            // Aplicar a otros renderers si es necesario
            else if (targetRenderer != null)
            {
                // Para otros tipos de renderers
                targetRenderer.sortingOrder = calculatedOrder;
                lastSortingOrder = calculatedOrder;
            }
        }

        /// <summary>
        /// Fuerza actualización inmediata
        /// </summary>
        [ContextMenu("Force Update Sorting")]
        public void ForceUpdate()
        {
            UpdateSortingOrder();
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                UpdateSortingOrder();
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Visualizar punto de pivot para sorting
            Gizmos.color = Color.cyan;
            Vector3 pivotPoint = transform.position + new Vector3(0, pivotYOffset, 0);
            Gizmos.DrawWireSphere(pivotPoint, 0.1f);
            
            // Mostrar sorting order
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(pivotPoint + Vector3.up * 0.3f, 
                $"Sorting: {lastSortingOrder}");
            #endif
        }
    }
}
