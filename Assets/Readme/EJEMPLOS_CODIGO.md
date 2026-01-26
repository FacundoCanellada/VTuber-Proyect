# 💻 Ejemplos de Código
## Sistema de Luces Interactivas - VTuber Project

---

## 🎯 Uso Básico desde Otros Scripts

### Controlar una luz específica

```csharp
using VTuberProject.Lighting;
using UnityEngine;

public class MiScript : MonoBehaviour
{
    public InteractableLight miLuz;
    
    void Start()
    {
        // Encender
        miLuz.TurnOn();
        
        // Apagar
        miLuz.TurnOff();
        
        // Alternar
        miLuz.Toggle();
        
        // Establecer estado
        miLuz.SetState(true);  // Encender
        miLuz.SetState(false); // Apagar
        
        // Con transición instantánea
        miLuz.SetState(true, instant: true);
        
        // Verificar estado
        if (miLuz.IsOn)
        {
            Debug.Log("La luz está encendida");
        }
    }
}
```

---

### Suscribirse a eventos de luz

```csharp
using VTuberProject.Lighting;
using UnityEngine;

public class ReactionToLight : MonoBehaviour
{
    public InteractableLight luz;
    
    void Start()
    {
        // Suscribirse al evento
        luz.OnLightToggled += OnLuzCambiada;
    }
    
    void OnLuzCambiada(bool estaEncendida)
    {
        if (estaEncendida)
        {
            Debug.Log("¡Se encendió la luz!");
            // Tu código aquí...
        }
        else
        {
            Debug.Log("Se apagó la luz");
            // Tu código aquí...
        }
    }
    
    void OnDestroy()
    {
        // Siempre desuscribirse para evitar leaks
        if (luz != null)
        {
            luz.OnLightToggled -= OnLuzCambiada;
        }
    }
}
```

---

### Control de interruptor desde código

```csharp
using VTuberProject.Lighting;
using UnityEngine;

public class ControlRemoto : MonoBehaviour
{
    public LightSwitch interruptor;
    
    void Update()
    {
        // Activar interruptor con otra tecla
        if (Input.GetKeyDown(KeyCode.L))
        {
            interruptor.ToggleLights();
        }
        
        // Encender todas las luces del interruptor
        if (Input.GetKeyDown(KeyCode.O))
        {
            interruptor.SetLights(true);
        }
        
        // Apagar todas las luces del interruptor
        if (Input.GetKeyDown(KeyCode.P))
        {
            interruptor.SetLights(false);
        }
    }
}
```

---

## 🌐 Uso del Manager Global

### Control global de todas las luces

```csharp
using VTuberProject.Lighting;
using UnityEngine;

public class ControlGlobalLuces : MonoBehaviour
{
    void Update()
    {
        // Encender todas las luces
        if (Input.GetKeyDown(KeyCode.F1))
        {
            InteractiveLightsManager.Instance.TurnOnAllLights();
        }
        
        // Apagar todas las luces
        if (Input.GetKeyDown(KeyCode.F2))
        {
            InteractiveLightsManager.Instance.TurnOffAllLights();
        }
        
        // Alternar todas
        if (Input.GetKeyDown(KeyCode.F3))
        {
            InteractiveLightsManager.Instance.ToggleAllLights();
        }
    }
}
```

---

### Buscar y controlar luces específicas

```csharp
using VTuberProject.Lighting;
using UnityEngine;
using System.Collections.Generic;

public class ControlPorNombre : MonoBehaviour
{
    void Start()
    {
        var manager = InteractiveLightsManager.Instance;
        
        // Encender todas las luces que contengan "Lamp" en el nombre
        manager.SetLightsByName("Lamp", true);
        
        // Apagar luces de cocina
        manager.SetLightsByName("Kitchen", false);
        
        // Obtener lista de luces específicas
        List<InteractableLight> lucesDelTecho = manager.GetLightsByName("Ceiling");
        
        foreach (var luz in lucesDelTecho)
        {
            Debug.Log($"Encontrada: {luz.gameObject.name}");
            luz.TurnOn();
        }
    }
}
```

---

### Obtener información del sistema

```csharp
using VTuberProject.Lighting;
using UnityEngine;

public class InfoSistema : MonoBehaviour
{
    void Start()
    {
        var manager = InteractiveLightsManager.Instance;
        
        // Obtener totales
        int totalLuces = manager.GetTotalLightsCount();
        int lucesEncendidas = manager.GetActiveLightsCount();
        
        Debug.Log($"Luces: {lucesEncendidas}/{totalLuces} encendidas");
        
        // Generar reporte completo
        manager.GenerateLightsReport();
    }
}
```

---

## 🎮 Integración con Sistemas de Juego

### Sistema de cortes de luz

```csharp
using VTuberProject.Lighting;
using UnityEngine;
using System.Collections;

public class CorteDeLuz : MonoBehaviour
{
    public float duracionCorte = 5f;
    
    public void IniciarCorte()
    {
        StartCoroutine(CorteTemporalDeLuz());
    }
    
    IEnumerator CorteTemporalDeLuz()
    {
        // Apagar todas las luces
        InteractiveLightsManager.Instance.TurnOffAllLights();
        
        Debug.Log("¡Corte de luz!");
        
        // Esperar
        yield return new WaitForSeconds(duracionCorte);
        
        // Restaurar luces
        InteractiveLightsManager.Instance.TurnOnAllLights();
        
        Debug.Log("Luz restaurada");
    }
}
```

---

### Sistema de habitaciones

```csharp
using VTuberProject.Lighting;
using UnityEngine;
using System.Collections.Generic;

public class Habitacion : MonoBehaviour
{
    [Header("Luces de esta habitación")]
    public List<InteractableLight> lucesHabitacion;
    
    [Header("Interruptores")]
    public List<LightSwitch> interruptores;
    
    private bool jugadorEnHabitacion = false;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorEnHabitacion = true;
            Debug.Log($"Entraste a {gameObject.name}");
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorEnHabitacion = false;
            Debug.Log($"Saliste de {gameObject.name}");
            
            // Apagar luces automáticamente al salir
            ApagarLuces();
        }
    }
    
    public void EncenderLuces()
    {
        foreach (var luz in lucesHabitacion)
        {
            if (luz != null)
                luz.TurnOn();
        }
    }
    
    public void ApagarLuces()
    {
        foreach (var luz in lucesHabitacion)
        {
            if (luz != null)
                luz.TurnOff();
        }
    }
    
    public bool TodasLucesEncendidas()
    {
        foreach (var luz in lucesHabitacion)
        {
            if (luz != null && !luz.IsOn)
                return false;
        }
        return true;
    }
}
```

---

### Sistema de tareas/objetivos

```csharp
using VTuberProject.Lighting;
using UnityEngine;

public class ObjetivoEncenderLuz : MonoBehaviour
{
    public InteractableLight luzObjetivo;
    public GameObject marcadorUI;
    
    private bool completado = false;
    
    void Start()
    {
        // Suscribirse al evento
        if (luzObjetivo != null)
        {
            luzObjetivo.OnLightToggled += VerificarObjetivo;
        }
        
        // Mostrar marcador
        if (marcadorUI != null)
        {
            marcadorUI.SetActive(true);
        }
    }
    
    void VerificarObjetivo(bool estaEncendida)
    {
        if (estaEncendida && !completado)
        {
            completado = true;
            Debug.Log("¡Objetivo completado!");
            
            // Ocultar marcador
            if (marcadorUI != null)
            {
                marcadorUI.SetActive(false);
            }
            
            // Dar recompensa, siguiente objetivo, etc.
            RecompensarJugador();
        }
    }
    
    void RecompensarJugador()
    {
        // Tu código de recompensas aquí
        Debug.Log("Recompensa otorgada");
    }
    
    void OnDestroy()
    {
        if (luzObjetivo != null)
        {
            luzObjetivo.OnLightToggled -= VerificarObjetivo;
        }
    }
}
```

---

### Sistema de horror/suspenso

```csharp
using VTuberProject.Lighting;
using UnityEngine;
using System.Collections;

public class EventoSuspenso : MonoBehaviour
{
    public InteractableLight[] lucesEvento;
    public AudioClip sonidoMiedo;
    
    public void IniciarEvento()
    {
        StartCoroutine(SecuenciaSuspenso());
    }
    
    IEnumerator SecuenciaSuspenso()
    {
        // Parpadear luces
        for (int i = 0; i < 3; i++)
        {
            foreach (var luz in lucesEvento)
            {
                luz.TurnOff();
            }
            
            yield return new WaitForSeconds(0.2f);
            
            foreach (var luz in lucesEvento)
            {
                luz.TurnOn();
            }
            
            yield return new WaitForSeconds(0.2f);
        }
        
        // Apagar todas
        foreach (var luz in lucesEvento)
        {
            luz.TurnOff();
        }
        
        // Sonido
        if (sonidoMiedo != null)
        {
            AudioSource.PlayClipAtPoint(sonidoMiedo, Camera.main.transform.position);
        }
        
        yield return new WaitForSeconds(2f);
        
        // Encender de nuevo
        foreach (var luz in lucesEvento)
        {
            luz.TurnOn();
        }
    }
}
```

---

## 🎨 Integración con Ciclo Día/Noche

### Auto-encendido basado en hora

```csharp
using VTuberProject.Lighting;
using UnityEngine;

public class LuzAutomatica : MonoBehaviour
{
    public DayNightCycle ciclo;
    public InteractableLight luz;
    
    [Header("Configuración")]
    public float horaEncender = 19f;
    public float horaApagar = 7f;
    
    private bool debeMantenerseEncendida = false;
    
    void Update()
    {
        if (ciclo == null || luz == null) return;
        
        float horaActual = ciclo.currentTime;
        
        // Determinar si debe estar encendida
        if (horaEncender > horaApagar)
        {
            // Caso normal: enciende por la noche
            debeMantenerseEncendida = horaActual >= horaEncender || horaActual < horaApagar;
        }
        else
        {
            // Caso raro
            debeMantenerseEncendida = horaActual >= horaEncender && horaActual < horaApagar;
        }
        
        // Actualizar estado
        if (debeMantenerseEncendida && !luz.IsOn)
        {
            luz.TurnOn();
        }
        else if (!debeMantenerseEncendida && luz.IsOn)
        {
            luz.TurnOff();
        }
    }
}
```

---

### Ajuste de intensidad según hora

```csharp
using VTuberProject.Lighting;
using UnityEngine;

public class IntensidadDinamica : MonoBehaviour
{
    public DayNightCycle ciclo;
    public InteractableLight luz;
    
    [Header("Intensidades")]
    public float intensidadDia = 0.5f;
    public float intensidadNoche = 1.5f;
    
    void Update()
    {
        if (ciclo == null || luz == null) return;
        
        // Obtener período del día
        var periodo = ciclo.GetCurrentPeriod();
        
        // Ajustar intensidad
        switch (periodo)
        {
            case DayNightCycle.TimeOfDay.Day:
                luz.onIntensity = intensidadDia;
                break;
            
            case DayNightCycle.TimeOfDay.Night:
                luz.onIntensity = intensidadNoche;
                break;
                
            case DayNightCycle.TimeOfDay.Sunrise:
            case DayNightCycle.TimeOfDay.Sunset:
                luz.onIntensity = Mathf.Lerp(intensidadDia, intensidadNoche, 0.5f);
                break;
        }
    }
}
```

---

## 🎬 Cutscenes y Secuencias

### Control de luces en cutscene

```csharp
using VTuberProject.Lighting;
using UnityEngine;
using System.Collections;

public class CutsceneManager : MonoBehaviour
{
    public InteractableLight[] lucesEscena;
    
    public IEnumerator PlayCutscene()
    {
        // Apagar todas las luces
        foreach (var luz in lucesEscena)
        {
            luz.TurnOff();
        }
        
        yield return new WaitForSeconds(2f);
        
        // Tu cutscene aquí...
        Debug.Log("Cutscene en progreso...");
        
        yield return new WaitForSeconds(3f);
        
        // Restaurar luces
        foreach (var luz in lucesEscena)
        {
            luz.TurnOn();
        }
    }
}
```

---

## 🔧 Helpers y Utilidades

### Crear interruptor desde código

```csharp
using VTuberProject.Lighting;
using UnityEngine;

public class CrearInterruptor : MonoBehaviour
{
    public void CrearNuevoInterruptor(Vector3 posicion, InteractableLight luzAControlar)
    {
        // Crear GameObject
        GameObject switchObj = new GameObject("Switch_Generated");
        switchObj.transform.position = posicion;
        
        // Añadir componente
        LightSwitch lightSwitch = switchObj.AddComponent<LightSwitch>();
        
        // Configurar
        lightSwitch.interactionKey = KeyCode.C;
        lightSwitch.interactionDistance = 2f;
        lightSwitch.playerTag = "Player";
        
        // Añadir luz
        lightSwitch.controlledLights.Add(luzAControlar);
        
        Debug.Log($"Interruptor creado en {posicion}");
    }
}
```

---

### Registrar luces dinámicamente

```csharp
using VTuberProject.Lighting;
using UnityEngine;

public class RegistroManual : MonoBehaviour
{
    void Start()
    {
        var manager = InteractiveLightsManager.Instance;
        
        // Encontrar una luz específica
        GameObject luzObj = GameObject.Find("Lamp_Ceiling_01");
        if (luzObj != null)
        {
            InteractableLight luz = luzObj.GetComponent<InteractableLight>();
            if (luz != null)
            {
                manager.RegisterLight(luz);
                Debug.Log("Luz registrada manualmente");
            }
        }
    }
}
```

---

## 💡 Tips y Trucos

### Verificar si el jugador tiene luz cerca

```csharp
using VTuberProject.Lighting;
using UnityEngine;

public class DetectorLuz : MonoBehaviour
{
    public float radioDeteccion = 5f;
    
    void Update()
    {
        // Encontrar todas las luces
        var manager = InteractiveLightsManager.Instance;
        
        int lucesEnRango = 0;
        
        foreach (var luz in manager.registeredLights)
        {
            if (luz != null && luz.IsOn)
            {
                float distancia = Vector3.Distance(transform.position, luz.transform.position);
                if (distancia <= radioDeteccion)
                {
                    lucesEnRango++;
                }
            }
        }
        
        if (lucesEnRango > 0)
        {
            Debug.Log($"Hay {lucesEnRango} luces cerca");
        }
    }
}
```

---

### Guardar estado de luces

```csharp
using VTuberProject.Lighting;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LightSaveData
{
    public string nombreLuz;
    public bool estaEncendida;
}

public class LightsSaveSystem : MonoBehaviour
{
    void GuardarEstadoLuces()
    {
        var manager = InteractiveLightsManager.Instance;
        List<LightSaveData> datos = new List<LightSaveData>();
        
        foreach (var luz in manager.registeredLights)
        {
            if (luz != null)
            {
                datos.Add(new LightSaveData
                {
                    nombreLuz = luz.gameObject.name,
                    estaEncendida = luz.IsOn
                });
            }
        }
        
        // Guardar datos (ejemplo con PlayerPrefs)
        string json = JsonUtility.ToJson(new { luces = datos });
        PlayerPrefs.SetString("LightsState", json);
        PlayerPrefs.Save();
        
        Debug.Log("Estado de luces guardado");
    }
    
    void CargarEstadoLuces()
    {
        string json = PlayerPrefs.GetString("LightsState", "");
        if (string.IsNullOrEmpty(json)) return;
        
        // Parsear y aplicar
        // (Implementación depende de tu sistema de guardado)
        
        Debug.Log("Estado de luces cargado");
    }
}
```

---

## 🚀 Optimización

### Pool de indicadores

```csharp
using UnityEngine;
using System.Collections.Generic;

public class IndicatorPool : MonoBehaviour
{
    public GameObject indicatorPrefab;
    public int poolSize = 5;
    
    private Queue<GameObject> pool = new Queue<GameObject>();
    
    void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(indicatorPrefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }
    
    public GameObject GetIndicator()
    {
        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            return Instantiate(indicatorPrefab);
        }
    }
    
    public void ReturnIndicator(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
```

---

¡Usa estos ejemplos como referencia para tu proyecto! 🎉
