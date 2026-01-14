using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace VTuberProject.Lighting
{
    /// <summary>
    /// Manager centralizado para todas las luces interactivas de la escena
    /// Permite control global y configuración escalable
    /// </summary>
    public class InteractiveLightsManager : MonoBehaviour
    {
        [Header("Auto-Registration")]
        [Tooltip("Registrar automáticamente todas las luces al iniciar")]
        public bool autoRegisterOnStart = true;
        
        [Header("Registered Lights")]
        [Tooltip("Lista de todas las luces interactivas en la escena")]
        public List<InteractableLight> registeredLights = new List<InteractableLight>();
        
        [Header("Registered Switches")]
        [Tooltip("Lista de todos los interruptores en la escena")]
        public List<LightSwitch> registeredSwitches = new List<LightSwitch>();

        [Header("Global Controls")]
        [Tooltip("Permitir control global de todas las luces")]
        public bool allowGlobalControl = true;
        
        [Header("Debug")]
        public bool showDebugInfo = false;

        // Singleton (opcional)
        private static InteractiveLightsManager instance;
        public static InteractiveLightsManager Instance => instance;

        private void Awake()
        {
            // Setup singleton
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Debug.LogWarning("Multiple InteractiveLightsManager instances found. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            if (autoRegisterOnStart)
            {
                RegisterAllLights();
                RegisterAllSwitches();
            }
        }

        /// <summary>
        /// Encuentra y registra todas las luces interactivas de la escena
        /// </summary>
        [ContextMenu("Register All Lights")]
        public void RegisterAllLights()
        {
            registeredLights.Clear();
            InteractableLight[] lights = FindObjectsByType<InteractableLight>(FindObjectsSortMode.None);
            
            registeredLights.AddRange(lights);
            
            if (showDebugInfo)
            {
                Debug.Log($"InteractiveLightsManager: Registered {registeredLights.Count} lights");
            }
        }

        /// <summary>
        /// Encuentra y registra todos los interruptores de la escena
        /// </summary>
        [ContextMenu("Register All Switches")]
        public void RegisterAllSwitches()
        {
            registeredSwitches.Clear();
            LightSwitch[] switches = FindObjectsByType<LightSwitch>(FindObjectsSortMode.None);
            
            registeredSwitches.AddRange(switches);
            
            if (showDebugInfo)
            {
                Debug.Log($"InteractiveLightsManager: Registered {registeredSwitches.Count} switches");
            }
        }

        /// <summary>
        /// Registra una luz individualmente
        /// </summary>
        public void RegisterLight(InteractableLight light)
        {
            if (light != null && !registeredLights.Contains(light))
            {
                registeredLights.Add(light);
                
                if (showDebugInfo)
                {
                    Debug.Log($"Registered light: {light.gameObject.name}");
                }
            }
        }

        /// <summary>
        /// Desregistra una luz
        /// </summary>
        public void UnregisterLight(InteractableLight light)
        {
            if (registeredLights.Contains(light))
            {
                registeredLights.Remove(light);
                
                if (showDebugInfo)
                {
                    Debug.Log($"Unregistered light: {light.gameObject.name}");
                }
            }
        }

        /// <summary>
        /// Enciende todas las luces interactivas
        /// </summary>
        [ContextMenu("Turn On All Lights")]
        public void TurnOnAllLights()
        {
            if (!allowGlobalControl) return;
            
            foreach (var light in registeredLights)
            {
                if (light != null)
                {
                    light.TurnOn();
                }
            }
            
            if (showDebugInfo)
            {
                Debug.Log("All interactive lights turned ON");
            }
        }

        /// <summary>
        /// Apaga todas las luces interactivas
        /// </summary>
        [ContextMenu("Turn Off All Lights")]
        public void TurnOffAllLights()
        {
            if (!allowGlobalControl) return;
            
            foreach (var light in registeredLights)
            {
                if (light != null)
                {
                    light.TurnOff();
                }
            }
            
            if (showDebugInfo)
            {
                Debug.Log("All interactive lights turned OFF");
            }
        }

        /// <summary>
        /// Alterna todas las luces interactivas
        /// </summary>
        [ContextMenu("Toggle All Lights")]
        public void ToggleAllLights()
        {
            if (!allowGlobalControl) return;
            
            foreach (var light in registeredLights)
            {
                if (light != null)
                {
                    light.Toggle();
                }
            }
        }

        /// <summary>
        /// Establece el estado de todas las luces por nombre/tag
        /// </summary>
        public void SetLightsByName(string nameContains, bool state)
        {
            foreach (var light in registeredLights)
            {
                if (light != null && light.gameObject.name.Contains(nameContains))
                {
                    light.SetState(state);
                }
            }
        }

        /// <summary>
        /// Obtiene todas las luces que coinciden con un criterio
        /// </summary>
        public List<InteractableLight> GetLightsByName(string nameContains)
        {
            return registeredLights.Where(light => 
                light != null && light.gameObject.name.Contains(nameContains)
            ).ToList();
        }

        /// <summary>
        /// Obtiene el número de luces encendidas
        /// </summary>
        public int GetActiveLightsCount()
        {
            return registeredLights.Count(light => light != null && light.IsOn);
        }

        /// <summary>
        /// Obtiene el número total de luces registradas
        /// </summary>
        public int GetTotalLightsCount()
        {
            return registeredLights.Count;
        }

        /// <summary>
        /// Limpia referencias nulas
        /// </summary>
        [ContextMenu("Clean Null References")]
        public void CleanNullReferences()
        {
            registeredLights.RemoveAll(light => light == null);
            registeredSwitches.RemoveAll(sw => sw == null);
            
            if (showDebugInfo)
            {
                Debug.Log($"Cleaned null references. Lights: {registeredLights.Count}, Switches: {registeredSwitches.Count}");
            }
        }

        /// <summary>
        /// Configura un interruptor para controlar luces específicas
        /// </summary>
        public void SetupSwitch(LightSwitch lightSwitch, params string[] lightNames)
        {
            if (lightSwitch == null) return;

            lightSwitch.controlledLights.Clear();
            
            foreach (var name in lightNames)
            {
                var lights = GetLightsByName(name);
                foreach (var light in lights)
                {
                    if (!lightSwitch.controlledLights.Contains(light))
                    {
                        lightSwitch.controlledLights.Add(light);
                    }
                }
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"Setup switch '{lightSwitch.gameObject.name}' with {lightSwitch.controlledLights.Count} lights");
            }
        }

        /// <summary>
        /// Genera un reporte del sistema de luces
        /// </summary>
        [ContextMenu("Generate Lights Report")]
        public void GenerateLightsReport()
        {
            CleanNullReferences();
            
            int totalLights = GetTotalLightsCount();
            int activeLights = GetActiveLightsCount();
            int totalSwitches = registeredSwitches.Count;
            
            Debug.Log("=== INTERACTIVE LIGHTS REPORT ===");
            Debug.Log($"Total Lights: {totalLights}");
            Debug.Log($"Active Lights: {activeLights} ({(totalLights > 0 ? (activeLights * 100f / totalLights) : 0):F1}%)");
            Debug.Log($"Total Switches: {totalSwitches}");
            Debug.Log("================================");
            
            // Listar luces
            Debug.Log("\nLights:");
            foreach (var light in registeredLights)
            {
                if (light != null)
                {
                    Debug.Log($"  - {light.gameObject.name} [{(light.IsOn ? "ON" : "OFF")}]");
                }
            }
            
            // Listar interruptores
            Debug.Log("\nSwitches:");
            foreach (var sw in registeredSwitches)
            {
                if (sw != null)
                {
                    Debug.Log($"  - {sw.gameObject.name} (Controls: {sw.controlledLights.Count} lights)");
                }
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
