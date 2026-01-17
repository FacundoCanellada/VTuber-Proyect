using UnityEngine;

namespace VTuberProject
{
    /// <summary>
    /// Define configuraciones de colisión para diferentes tipos de objetos
    /// Reutilizable y escalable
    /// </summary>
    [CreateAssetMenu(fileName = "CollisionProfile", menuName = "VTuber/Collision Profile")]
    public class CollisionProfile : ScriptableObject
    {
        [Header("Identification")]
        [Tooltip("Nombre descriptivo del perfil")]
        public string profileName = "New Profile";
        
        [TextArea(2, 4)]
        public string description = "Descripción del tipo de objeto";

        [Header("Physics")]
        [Tooltip("Tipo de collider a usar")]
        public ColliderType colliderType = ColliderType.Box;
        
        [Tooltip("¿Es un trigger?")]
        public bool isTrigger = false;
        
        [Tooltip("¿Es estático? (no se moverá)")]
        public bool isStatic = true;
        
        [Tooltip("Layer de física")]
        public string physicsLayer = "Default";

        [Header("Collider Settings")]
        [Tooltip("Ajustar collider automáticamente al sprite")]
        public bool autoFitToSprite = true;
        
        [Tooltip("Offset del collider")]
        public Vector2 colliderOffset = Vector2.zero;
        
        [Tooltip("Tamaño del collider (si no es auto-fit)")]
        public Vector2 colliderSize = Vector2.one;
        
        [Tooltip("Reducir tamaño del collider (porcentaje)")]
        [Range(0f, 1f)]
        public float sizeReduction = 0f;

        [Header("Depth Sorting")]
        [Tooltip("Usar Y-sorting automático")]
        public bool useYSorting = false;
        
        [Tooltip("Sorting order base")]
        public int baseSortingOrder = 0;
        
        [Tooltip("Precisión del Y-sorting")]
        [Range(1, 100)]
        public int sortingPrecision = 10;
        
        [Tooltip("Offset Y para el punto de sorting")]
        public float sortingPivotOffset = 0f;

        [Header("Visual")]
        [Tooltip("Color del gizmo en Scene view")]
        public Color gizmoColor = Color.green;

        public enum ColliderType
        {
            None,
            Box,
            Circle,
            Polygon,
            Edge
        }

        /// <summary>
        /// Aplica este perfil a un GameObject
        /// </summary>
        public void ApplyToObject(GameObject target)
        {
            if (target == null) return;

            // Limpiar colliders existentes
            RemoveExistingColliders(target);

            // Añadir collider según tipo
            if (colliderType != ColliderType.None)
            {
                AddCollider(target);
                Debug.Log($"[CollisionProfile] Applied {colliderType} collider to '{target.name}' - IsTrigger: {isTrigger}");
            }

            // Configurar Y-sorting si es necesario
            if (useYSorting)
            {
                SetupYSorting(target);
            }

            // Configurar layer
            if (!string.IsNullOrEmpty(physicsLayer) && physicsLayer != "Default")
            {
                int layer = LayerMask.NameToLayer(physicsLayer);
                if (layer >= 0)
                {
                    target.layer = layer;
                }
                else
                {
                    Debug.LogWarning($"[CollisionProfile] Layer '{physicsLayer}' not found. Using Default.");
                }
            }

            // Marcar como estático si corresponde
            if (isStatic && Application.isPlaying)
            {
                var rb = target.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.bodyType = RigidbodyType2D.Static;
                }
            }
        }

        private void RemoveExistingColliders(GameObject target)
        {
            var colliders = target.GetComponents<Collider2D>();
            foreach (var col in colliders)
            {
                if (Application.isPlaying)
                    Destroy(col);
                else
                    DestroyImmediate(col);
            }
        }

        private void AddCollider(GameObject target)
        {
            Collider2D newCollider = null;

            switch (colliderType)
            {
                case ColliderType.Box:
                    var boxCol = target.AddComponent<BoxCollider2D>();
                    boxCol.offset = colliderOffset;
                    
                    if (autoFitToSprite)
                    {
                        var sprite = target.GetComponent<SpriteRenderer>();
                        if (sprite != null && sprite.sprite != null)
                        {
                            boxCol.size = sprite.sprite.bounds.size * (1f - sizeReduction);
                        }
                        else
                        {
                            boxCol.size = colliderSize;
                        }
                    }
                    else
                    {
                        boxCol.size = colliderSize;
                    }
                    newCollider = boxCol;
                    break;

                case ColliderType.Circle:
                    var circleCol = target.AddComponent<CircleCollider2D>();
                    circleCol.offset = colliderOffset;
                    
                    if (autoFitToSprite)
                    {
                        var sprite = target.GetComponent<SpriteRenderer>();
                        if (sprite != null && sprite.sprite != null)
                        {
                            circleCol.radius = Mathf.Max(sprite.sprite.bounds.size.x, sprite.sprite.bounds.size.y) * 0.5f * (1f - sizeReduction);
                        }
                        else
                        {
                            circleCol.radius = colliderSize.x * 0.5f;
                        }
                    }
                    else
                    {
                        circleCol.radius = colliderSize.x * 0.5f;
                    }
                    newCollider = circleCol;
                    break;

                case ColliderType.Polygon:
                    var polyCol = target.AddComponent<PolygonCollider2D>();
                    polyCol.offset = colliderOffset;
                    // Polygon collider se ajusta automáticamente al sprite
                    newCollider = polyCol;
                    break;

                case ColliderType.Edge:
                    var edgeCol = target.AddComponent<EdgeCollider2D>();
                    edgeCol.offset = colliderOffset;
                    newCollider = edgeCol;
                    break;
            }

            if (newCollider != null)
            {
                newCollider.isTrigger = isTrigger;
            }
        }

        private void SetupYSorting(GameObject target)
        {
            var sorter = target.GetComponent<YDepthSorter>();
            if (sorter == null)
            {
                sorter = target.AddComponent<YDepthSorter>();
            }

            sorter.autoUpdate = !isStatic; // Solo auto-update si no es estático
            sorter.baseSortingOrder = baseSortingOrder;
            sorter.precision = sortingPrecision;
            sorter.pivotYOffset = sortingPivotOffset;
            
            sorter.UpdateSortingOrder();
        }
    }
}
