using UnityEngine;

namespace VTuberProject
{
    /// <summary>
    /// Script de emergencia para configurar colisiones directamente
    /// Añade esto a cada objeto que necesite colisión
    /// </summary>
    public class QuickCollisionSetup : MonoBehaviour
    {
        [Header("Configuración Simple")]
        [Tooltip("Tipo de objeto")]
        public ObjectType type = ObjectType.Furniture;
        
        [Header("Collider Settings")]
        [Tooltip("Ajustar automáticamente al sprite")]
        public bool autoSize = true;
        
        [Tooltip("Reducir tamaño (0 = sin reducción, 0.5 = 50% más pequeño)")]
        [Range(0f, 0.9f)]
        public float shrinkAmount = 0.2f;

        public enum ObjectType
        {
            Wall,           // Paredes - Colisión completa
            Furniture,      // Muebles - Colisión completa
            Decoration,     // Sin colisión
            Trigger         // Trigger (puertas, switches)
        }

        private void Start()
        {
            Setup();
        }

        [ContextMenu("Setup Now")]
        public void Setup()
        {
            // Limpiar colliders existentes
            var existingColliders = GetComponents<Collider2D>();
            foreach (var col in existingColliders)
            {
                if (Application.isPlaying)
                    Destroy(col);
                else
                    DestroyImmediate(col);
            }

            // Aplicar según tipo
            switch (type)
            {
                case ObjectType.Wall:
                case ObjectType.Furniture:
                    AddSolidCollider();
                    break;
                    
                case ObjectType.Trigger:
                    AddTriggerCollider();
                    break;
                    
                case ObjectType.Decoration:
                    // Sin collider
                    break;
            }
        }

        private void AddSolidCollider()
        {
            var boxCol = gameObject.AddComponent<BoxCollider2D>();
            boxCol.isTrigger = false; // SÓLIDO
            
            if (autoSize)
            {
                var sprite = GetComponent<SpriteRenderer>();
                if (sprite != null && sprite.sprite != null)
                {
                    boxCol.size = sprite.sprite.bounds.size * (1f - shrinkAmount);
                    boxCol.offset = Vector2.zero;
                }
            }
            
            Debug.Log($"[QuickSetup] '{gameObject.name}' configured as SOLID collider");
        }

        private void AddTriggerCollider()
        {
            var boxCol = gameObject.AddComponent<BoxCollider2D>();
            boxCol.isTrigger = true; // TRIGGER
            
            if (autoSize)
            {
                var sprite = GetComponent<SpriteRenderer>();
                if (sprite != null && sprite.sprite != null)
                {
                    boxCol.size = sprite.sprite.bounds.size;
                    boxCol.offset = Vector2.zero;
                }
            }
            
            Debug.Log($"[QuickSetup] '{gameObject.name}' configured as TRIGGER");
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = type == ObjectType.Wall ? Color.red : 
                          type == ObjectType.Furniture ? Color.yellow :
                          type == ObjectType.Trigger ? Color.green : Color.gray;
            
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.3f);
        }
    }
}
