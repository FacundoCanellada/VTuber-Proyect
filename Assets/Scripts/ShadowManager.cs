using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VTuberProject.Lighting
{
    /// <summary>
    /// Gestiona las sombras en toda la escena (Tilemaps y GameObjects)
    /// </summary>
    public class ShadowManager : MonoBehaviour
    {
        [Header("Global Shadow Settings")]
        [Tooltip("Calidad global de las sombras")]
        public ShadowQuality shadowQuality = ShadowQuality.Medium;
        
        [Tooltip("Intensidad global de las sombras")]
        [Range(0f, 1f)]
        public float globalShadowIntensity = 0.75f;
        
        [Tooltip("Suavizado de sombras")]
        [Range(0f, 1f)]
        public float shadowSoftness = 0.5f;

        [Header("Automatic Setup")]
        [Tooltip("Añadir automáticamente Shadow Casters a objetos en la escena")]
        // Desactivado por defecto para evitar cambios automáticos hasta verificar consola
        public bool autoSetupOnStart = false;
        
        [Tooltip("Layers que deben proyectar sombras")]
        public LayerMask shadowCasterLayers = -1;

        [Header("Tilemap Shadows")]
        [Tooltip("Tilemaps que deben proyectar sombras")]
        public List<Tilemap> tilemapsWithShadows = new List<Tilemap>();

        [Header("Manual Object Lists")]
        [Tooltip("GameObjects que proyectan sombras")]
        public List<GameObject> shadowCasterObjects = new List<GameObject>();
        
        [Tooltip("GameObjects que NO deben proyectar sombras")]
        public List<GameObject> excludedObjects = new List<GameObject>();

        public enum ShadowQuality
        {
            Low,
            Medium,
            High,
            Ultra
        }

        private void Start()
        {
            if (autoSetupOnStart)
            {
                SetupAllShadows();
            }
        }

        /// <summary>
        /// Configura sombras para todos los objetos en la escena
        /// </summary>
        [ContextMenu("Setup All Shadows")]
        public void SetupAllShadows()
        {
            // Configurar Tilemaps
            SetupTilemapShadows();
            
            // Configurar GameObjects individuales
            SetupGameObjectShadows();
            
            Debug.Log($"Shadow setup complete! {tilemapsWithShadows.Count} tilemaps, {shadowCasterObjects.Count} objects configured.");
        }

        /// <summary>
        /// Configura Composite Shadow Casters en Tilemaps
        /// </summary>
        private void SetupTilemapShadows()
        {
            // Auto-detectar tilemaps si la lista está vacía
            if (tilemapsWithShadows.Count == 0)
            {
                tilemapsWithShadows.AddRange(FindObjectsOfType<Tilemap>());
            }

            foreach (var tilemap in tilemapsWithShadows)
            {
                if (tilemap == null) continue;

                // Buscar o añadir Composite Shadow Caster 2D
                var compositeShadowCaster = tilemap.GetComponent<CompositeShadowCaster2D>();
                
                if (compositeShadowCaster == null)
                {
                    compositeShadowCaster = tilemap.gameObject.AddComponent<CompositeShadowCaster2D>();
                }

                // Buscar o añadir Composite Collider 2D (requerido para Composite Shadow Caster)
                var compositeCollider = tilemap.GetComponent<CompositeCollider2D>();
                
                if (compositeCollider == null)
                {
                    // Primero necesitamos un Tilemap Collider 2D
                    var tilemapCollider = tilemap.GetComponent<TilemapCollider2D>();
                    if (tilemapCollider == null)
                    {
                        tilemapCollider = tilemap.gameObject.AddComponent<TilemapCollider2D>();
                        tilemapCollider.usedByComposite = true;
                    }
                    
                    // Ahora el Composite Collider 2D
                    compositeCollider = tilemap.gameObject.AddComponent<CompositeCollider2D>();
                    
                    // Asegurar que tiene Rigidbody2D en modo Static
                    var rb2d = tilemap.GetComponent<Rigidbody2D>();
                    if (rb2d == null)
                    {
                        rb2d = tilemap.gameObject.AddComponent<Rigidbody2D>();
                    }
                    rb2d.bodyType = RigidbodyType2D.Static;
                }

                Debug.Log($"Tilemap '{tilemap.name}' configured for shadows");
            }
        }

        /// <summary>
        /// Configura Shadow Casters en GameObjects individuales
        /// </summary>
        private void SetupGameObjectShadows()
        {
            // Si no hay objetos en la lista, buscar todos los SpriteRenderers
            if (shadowCasterObjects.Count == 0)
            {
                SpriteRenderer[] allSprites = FindObjectsOfType<SpriteRenderer>();
                
                foreach (var sprite in allSprites)
                {
                    // Verificar si está en el layer correcto
                    if (((1 << sprite.gameObject.layer) & shadowCasterLayers) != 0)
                    {
                        // Verificar que no esté excluido
                        if (!excludedObjects.Contains(sprite.gameObject))
                        {
                            shadowCasterObjects.Add(sprite.gameObject);
                        }
                    }
                }
            }

            // Añadir Shadow Caster 2D a cada objeto
            foreach (var obj in shadowCasterObjects)
            {
                if (obj == null) continue;

                var shadowCaster = obj.GetComponent<ShadowCaster2D>();
                
                if (shadowCaster == null)
                {
                    shadowCaster = obj.AddComponent<ShadowCaster2D>();
                }

                // Configurar según la calidad
                shadowCaster.useRendererSilhouette = shadowQuality >= ShadowQuality.Medium;
                shadowCaster.selfShadows = shadowQuality >= ShadowQuality.High;

                Debug.Log($"Shadow Caster added to '{obj.name}'");
            }
        }

        /// <summary>
        /// Actualiza la intensidad de todas las sombras en las luces
        /// </summary>
        [ContextMenu("Update All Light Shadows")]
        public void UpdateAllLightShadows()
        {
            Light2D[] allLights = FindObjectsOfType<Light2D>();
            
            foreach (var light in allLights)
            {
                if (light.lightType != Light2D.LightType.Global)
                {
                    light.shadowsEnabled = true;
                    light.shadowIntensity = globalShadowIntensity;
                    light.shadowVolumeIntensity = globalShadowIntensity * shadowSoftness;
                }
            }
            
            Debug.Log($"Updated shadow settings for {allLights.Length} lights");
        }

        /// <summary>
        /// Activa/desactiva todas las sombras de la escena
        /// </summary>
        public void SetShadowsEnabled(bool enabled)
        {
            Light2D[] allLights = FindObjectsOfType<Light2D>();
            
            foreach (var light in allLights)
            {
                light.shadowsEnabled = enabled;
            }

            ShadowCaster2D[] allShadowCasters = FindObjectsOfType<ShadowCaster2D>();
            
            foreach (var caster in allShadowCasters)
            {
                caster.enabled = enabled;
            }
        }

        /// <summary>
        /// Limpia todos los Shadow Casters de la escena
        /// </summary>
        [ContextMenu("Remove All Shadow Casters")]
        public void RemoveAllShadowCasters()
        {
            ShadowCaster2D[] allShadowCasters = FindObjectsOfType<ShadowCaster2D>();
            
            foreach (var caster in allShadowCasters)
            {
                DestroyImmediate(caster);
            }
            
            CompositeShadowCaster2D[] allCompositeCasters = FindObjectsOfType<CompositeShadowCaster2D>();
            
            foreach (var caster in allCompositeCasters)
            {
                DestroyImmediate(caster);
            }
            
            shadowCasterObjects.Clear();
            
            Debug.Log("All shadow casters removed");
        }

#if UNITY_EDITOR
        [ContextMenu("Select All Shadow Casters")]
        private void SelectAllShadowCasters()
        {
            ShadowCaster2D[] allShadowCasters = FindObjectsOfType<ShadowCaster2D>();
            Selection.objects = System.Array.ConvertAll(allShadowCasters, caster => caster.gameObject);
            Debug.Log($"Selected {allShadowCasters.Length} shadow casters");
        }
#endif
    }
}
