using UnityEngine;

namespace VTuberProject
{
    /// <summary>
    /// Componente que aplica configuración de colisión a un objeto de escena
    /// Permite configuración manual con aplicación fácil
    /// </summary>
    public class SceneObjectSetup : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Perfil de colisión a aplicar")]
        public CollisionProfile profile;
        
        [Tooltip("Aplicar automáticamente al iniciar")]
        public bool applyOnStart = false;
        
        [Tooltip("Tipo de objeto para referencia")]
        public ObjectType objectType = ObjectType.Furniture;

        [Header("Custom Overrides")]
        [Tooltip("Sobrescribir configuración del perfil")]
        public bool useCustomSettings = false;
        
        [Tooltip("Collider offset personalizado")]
        public Vector2 customOffset = Vector2.zero;
        
        [Tooltip("Tamaño personalizado")]
        public Vector2 customSize = Vector2.one;

        [Header("Info")]
        [SerializeField]
        private bool isSetup = false;

        public enum ObjectType
        {
            Wall,
            Furniture,
            Decoration,
            Interactable,
            Dynamic,
            Occluder
        }

        private void Start()
        {
            if (applyOnStart && profile != null && !isSetup)
            {
                ApplyProfile();
            }
        }

        /// <summary>
        /// Aplica el perfil de colisión configurado
        /// </summary>
        [ContextMenu("Apply Profile")]
        public void ApplyProfile()
        {
            if (profile == null)
            {
                Debug.LogWarning($"[SceneObjectSetup] No profile assigned to '{gameObject.name}'");
                return;
            }

            profile.ApplyToObject(gameObject);

            // Aplicar overrides personalizados si están activos
            if (useCustomSettings)
            {
                ApplyCustomOverrides();
            }

            // Verificar que el collider se aplicó correctamente
            var col = GetComponent<Collider2D>();
            if (col != null)
            {
                Debug.Log($"[SceneObjectSetup] '{gameObject.name}' - Collider: {col.GetType().Name}, IsTrigger: {col.isTrigger}");
            }

            isSetup = true;
        }

        /// <summary>
        /// Remueve toda configuración de colisión
        /// </summary>
        [ContextMenu("Remove Setup")]
        public void RemoveSetup()
        {
            // Remover colliders
            var colliders = GetComponents<Collider2D>();
            foreach (var col in colliders)
            {
                if (Application.isPlaying)
                    Destroy(col);
                else
                    DestroyImmediate(col);
            }

            // Remover Y-sorter
            var sorter = GetComponent<YDepthSorter>();
            if (sorter != null)
            {
                if (Application.isPlaying)
                    Destroy(sorter);
                else
                    DestroyImmediate(sorter);
            }

            isSetup = false;
            Debug.Log($"[SceneObjectSetup] Removed setup from '{gameObject.name}'");
        }

        private void ApplyCustomOverrides()
        {
            var boxCol = GetComponent<BoxCollider2D>();
            if (boxCol != null)
            {
                boxCol.offset = customOffset;
                boxCol.size = customSize;
            }

            var circleCol = GetComponent<CircleCollider2D>();
            if (circleCol != null)
            {
                circleCol.offset = customOffset;
                circleCol.radius = customSize.x * 0.5f;
            }
        }

        private void OnDrawGizmos()
        {
            if (profile != null)
            {
                Gizmos.color = profile.gizmoColor;
                Gizmos.DrawWireCube(transform.position, Vector3.one * 0.2f);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (profile == null) return;

            // Dibujar preview del collider
            Gizmos.color = profile.gizmoColor;
            
            Vector2 offset = useCustomSettings ? customOffset : profile.colliderOffset;
            Vector2 size = useCustomSettings ? customSize : profile.colliderSize;

            Vector3 worldPos = transform.position + (Vector3)offset;

            switch (profile.colliderType)
            {
                case CollisionProfile.ColliderType.Box:
                    Gizmos.DrawWireCube(worldPos, size);
                    break;

                case CollisionProfile.ColliderType.Circle:
                    DrawWireCircle(worldPos, size.x * 0.5f);
                    break;
            }
        }

        private void DrawWireCircle(Vector3 center, float radius, int segments = 32)
        {
            float angle = 0f;
            Vector3 lastPoint = center + new Vector3(radius, 0, 0);

            for (int i = 0; i < segments + 1; i++)
            {
                angle += 2f * Mathf.PI / segments;
                Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
                Gizmos.DrawLine(lastPoint, newPoint);
                lastPoint = newPoint;
            }
        }
    }
}
