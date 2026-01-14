using UnityEngine;
using System.Collections.Generic;

namespace VTuberProject.Lighting
{
    /// <summary>
    /// Interruptor que controla una o más luces interactivas
    /// Muestra indicador UI cuando el jugador está cerca
    /// </summary>
    public class LightSwitch : MonoBehaviour
    {
        [Header("Controlled Lights")]
        [Tooltip("Luces que controla este interruptor")]
        public List<InteractableLight> controlledLights = new List<InteractableLight>();
        
        [Header("Interaction Settings")]
        [Tooltip("Tecla para interactuar")]
        public KeyCode interactionKey = KeyCode.C;
        
        [Tooltip("Distancia máxima para interactuar")]
        [Range(0.5f, 5f)]
        public float interactionDistance = 2f;

        [Header("UI Indicator (Simple)")]
        [Tooltip("Mostrar indicador de texto simple")]
        public bool showSimpleIndicator = true;
        
        [Tooltip("Texto a mostrar")]
        public string indicatorText = "C";
        
        [Tooltip("Offset del indicador")]
        public Vector3 indicatorOffset = new Vector3(0, 1f, 0);

        [Header("Visual Feedback")]
        [Tooltip("Sprite del interruptor encendido")]
        public Sprite switchOnSprite;
        
        [Tooltip("Sprite del interruptor apagado")]
        public Sprite switchOffSprite;
        
        [Tooltip("SpriteRenderer del interruptor")]
        public SpriteRenderer switchRenderer;

        [Header("Audio")]
        [Tooltip("Sonido al activar el interruptor")]
        public AudioClip switchSound;
        
        [Range(0f, 1f)]
        public float switchSoundVolume = 0.7f;

        [Header("Debug")]
        public bool showDebugGizmos = true;
        public bool showDebugMessages = true;

        // Estado interno
        private AudioSource audioSource;
        private bool playerInRange = false;
        private bool lightsAreOn = false;
        private GUIStyle indicatorStyle;

        private void Start()
        {
            // Crear AudioSource si hay sonido
            if (switchSound != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.volume = switchSoundVolume;
            }

            // Verificar estado inicial de las luces
            UpdateSwitchVisual();
            
            // Suscribirse a eventos de las luces
            foreach (var light in controlledLights)
            {
                if (light != null)
                {
                    light.OnLightToggled += OnControlledLightToggled;
                }
            }

            // Setup GUI style para indicador
            if (showSimpleIndicator)
            {
                indicatorStyle = new GUIStyle();
                indicatorStyle.fontSize = 24;
                indicatorStyle.fontStyle = FontStyle.Bold;
                indicatorStyle.normal.textColor = Color.white;
                indicatorStyle.alignment = TextAnchor.MiddleCenter;
            }
            
            if (showDebugMessages)
            {
                Debug.Log($"[LightSwitch] '{gameObject.name}' initialized with {controlledLights.Count} lights");
            }
        }

        private void Update()
        {
            CheckPlayerDistance();
            HandleInteraction();
        }

        private void CheckPlayerDistance()
        {
            // Buscar jugador por proximidad (más robusto que por tag)
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            GameObject closestPlayer = null;
            float closestDistance = interactionDistance;

            foreach (GameObject obj in allObjects)
            {
                // Buscar por tag o nombre que contenga "Player"
                if (obj.CompareTag("Player") || obj.name.Contains("Player"))
                {
                    float distance = Vector3.Distance(transform.position, obj.transform.position);
                    if (distance <= interactionDistance && distance < closestDistance)
                    {
                        closestPlayer = obj;
                        closestDistance = distance;
                    }
                }
            }

            bool nowInRange = closestPlayer != null;

            if (nowInRange != playerInRange)
            {
                playerInRange = nowInRange;
                
                if (showDebugMessages)
                {
                    Debug.Log($"[LightSwitch] Player {(playerInRange ? "entered" : "left")} range of '{gameObject.name}'");
                }
            }
        }

        private void HandleInteraction()
        {
            if (playerInRange && Input.GetKeyDown(interactionKey))
            {
                if (showDebugMessages)
                {
                    Debug.Log($"[LightSwitch] '{gameObject.name}' activated!");
                }
                
                ToggleLights();
            }
        }

        /// <summary>
        /// Alterna el estado de todas las luces controladas
        /// </summary>
        public void ToggleLights()
        {
            if (controlledLights.Count == 0)
            {
                Debug.LogWarning($"[LightSwitch] '{gameObject.name}' has no lights to control!");
                return;
            }

            foreach (var light in controlledLights)
            {
                if (light != null)
                {
                    light.Toggle();
                    
                    if (showDebugMessages)
                    {
                        Debug.Log($"[LightSwitch] Toggled '{light.gameObject.name}' - Now {(light.IsOn ? "ON" : "OFF")}");
                    }
                }
            }

            // Reproducir sonido
            if (audioSource != null && switchSound != null)
            {
                audioSource.PlayOneShot(switchSound, switchSoundVolume);
            }

            // Actualizar visual
            UpdateSwitchVisual();
        }

        /// <summary>
        /// Establece el estado de todas las luces
        /// </summary>
        public void SetLights(bool state)
        {
            foreach (var light in controlledLights)
            {
                if (light != null)
                {
                    light.SetState(state);
                }
            }
            
            UpdateSwitchVisual();
        }

        private void UpdateSwitchVisual()
        {
            // Verificar si alguna luz está encendida
            lightsAreOn = false;
            foreach (var light in controlledLights)
            {
                if (light != null && light.IsOn)
                {
                    lightsAreOn = true;
                    break;
                }
            }

            // Actualizar sprite del interruptor
            if (switchRenderer != null)
            {
                if (lightsAreOn && switchOnSprite != null)
                {
                    switchRenderer.sprite = switchOnSprite;
                }
                else if (!lightsAreOn && switchOffSprite != null)
                {
                    switchRenderer.sprite = switchOffSprite;
                }
            }
        }

        private void OnControlledLightToggled(bool state)
        {
            UpdateSwitchVisual();
        }

        private void OnGUI()
        {
            if (!showSimpleIndicator || !playerInRange) return;

            // Convertir posición del interruptor a screen space
            Vector3 worldPos = transform.position + indicatorOffset;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            // Solo mostrar si está frente a la cámara
            if (screenPos.z > 0)
            {
                // Unity GUI usa coordenadas invertidas en Y
                screenPos.y = Screen.height - screenPos.y;

                // Dibujar indicador centrado
                float width = 60;
                float height = 40;
                Rect rect = new Rect(screenPos.x - width/2, screenPos.y - height/2, width, height);
                
                // Fondo semi-transparente
                GUI.color = new Color(0, 0, 0, 0.7f);
                GUI.Box(rect, "");
                
                // Texto
                GUI.color = Color.white;
                GUI.Label(rect, indicatorText, indicatorStyle);
                
                // Resetear color
                GUI.color = Color.white;
            }
        }

        private void OnDestroy()
        {
            // Desuscribirse de eventos
            foreach (var light in controlledLights)
            {
                if (light != null)
                {
                    light.OnLightToggled -= OnControlledLightToggled;
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (!showDebugGizmos) return;

            // Dibujar rango de interacción
            Gizmos.color = playerInRange ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionDistance);

            // Dibujar líneas a las luces controladas
            Gizmos.color = Color.cyan;
            foreach (var light in controlledLights)
            {
                if (light != null)
                {
                    Gizmos.DrawLine(transform.position, light.transform.position);
                }
            }

            // Dibujar posición del indicador
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(transform.position + indicatorOffset, Vector3.one * 0.3f);
        }

        // Editor helpers
        [ContextMenu("Find Nearby Lights")]
        private void FindNearbyLights()
        {
            InteractableLight[] allLights = FindObjectsByType<InteractableLight>(FindObjectsSortMode.None);
            controlledLights.Clear();
            
            foreach (var light in allLights)
            {
                if (Vector3.Distance(transform.position, light.transform.position) <= interactionDistance * 2f)
                {
                    controlledLights.Add(light);
                }
            }
            
            Debug.Log($"Found {controlledLights.Count} nearby lights");
        }
    }
}
