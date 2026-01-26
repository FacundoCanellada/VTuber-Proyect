# Sistema de Iluminación Interactiva
## VTuber Project - Guía de Uso

---

## 📋 Resumen de Cambios

### 1. Ajustes de Iluminación del Ciclo Día/Noche

Se han ajustado los valores en `DayNightCycle.cs` para una iluminación más balanceada:

#### **Día (Mañana)**
- **Color**: Cálido y suave `(1f, 0.92f, 0.85f)`
- **Intensidad**: 0.7 (tenue y difusa, no completamente brillante)
- Referencia: Habitación iluminada suavemente

#### **Amanecer/Atardecer**
- **Color Amanecer**: `(1f, 0.7f, 0.4f)`
- **Color Atardecer**: Rojizo/Anaranjado `(1f, 0.45f, 0.25f)`
- **Intensidad**: 0.5 (menos intensidad, más contraste)
- Referencia: Minecraft con shaders

#### **Noche**
- **Color**: Frío `(0.35f, 0.45f, 0.65f)` - Simula luz artificial exterior
- **Intensidad**: 0.15 (muy reducida)
- La habitación se ve oscura a menos que se prendan las luces interiores

#### **Brillo de Cortinas/Ventanas**
- Nuevo parámetro: `windowLightMultiplier = 0.4` (60% de reducción)
- Las luces que entran por las ventanas ahora están reducidas automáticamente
- Evita el efecto de "quemar" la escena

---

## 🔧 Nuevos Componentes

### 1. **InteractableLight.cs**
Componente para luces que se pueden encender/apagar independientemente del ciclo día/noche.

**Características:**
- ✅ Transiciones suaves de encendido/apagado
- ✅ Soporte para efectos visuales (sprites, partículas)
- ✅ Sonidos al encender/apagar
- ✅ Sistema de eventos para otros scripts
- ✅ Completamente escalable y reutilizable

**Uso:**
1. Añade `InteractableLight` a un GameObject con `Light2D`
2. Configura intensidades (On/Off)
3. Opcional: Añade efectos visuales y sonidos

```csharp
// Código de ejemplo:
interactableLight.TurnOn();
interactableLight.TurnOff();
interactableLight.Toggle();
```

---

### 2. **LightSwitch.cs**
Interruptor que controla una o más luces interactivas.

**Características:**
- ✅ Detección de proximidad del jugador
- ✅ Indicador UI automático (aparece al acercarse)
- ✅ Configuración de tecla (por defecto: C)
- ✅ Puede controlar múltiples luces
- ✅ Visual feedback (sprites de interruptor on/off)
- ✅ Sonidos de interruptor

**Configuración:**
1. Crea un GameObject para el interruptor
2. Añade componente `LightSwitch`
3. Arrastra las luces que quieres controlar a `Controlled Lights`
4. Configura la distancia de interacción
5. Asigna el prefab del indicador UI
6. (Opcional) Asigna sprites y sonidos

**Helpers en Context Menu:**
- `Find Nearby Lights`: Busca luces cercanas automáticamente
- `Add Light By Name`: Busca luces con nombre similar

---

### 3. **InteractiveLightsManager.cs**
Manager centralizado para todas las luces interactivas (opcional pero recomendado).

**Características:**
- ✅ Registro automático de todas las luces
- ✅ Control global de luces
- ✅ Búsqueda por nombre/criterios
- ✅ Reportes del sistema
- ✅ Singleton para acceso global

**Uso:**
1. Crea un GameObject vacío: "InteractiveLightsManager"
2. Añade el componente `InteractiveLightsManager`
3. Marca `Auto Register On Start` para registro automático

**Funciones útiles:**
```csharp
// Acceso desde cualquier script:
InteractiveLightsManager.Instance.TurnOnAllLights();
InteractiveLightsManager.Instance.TurnOffAllLights();
InteractiveLightsManager.Instance.SetLightsByName("Lamp", true);
```

---

### 4. **InteractionIndicator.cs**
Componente para el indicador UI que aparece sobre los interruptores.

**Características:**
- ✅ Animaciones (Bounce, Scale, Rotate, Pulse)
- ✅ Fade in/out automático
- ✅ Configurable completamente

---

## 🎮 Setup Paso a Paso

### Para Lamp_Ceiling_01 (Ejemplo)

#### 1. Configurar la Luz como Interactiva

```
1. Selecciona el GameObject "Lamp_Ceiling_01"
2. Añade componente: InteractableLight
3. Configura:
   - Start On: false (o true si quieres que inicie encendida)
   - On Intensity: 1.0 - 2.0 (ajusta al gusto)
   - Off Intensity: 0
   - Transition Speed: 0.3
4. (Opcional) Añade sonidos y efectos
```

#### 2. Crear el Interruptor

```
1. Crea un GameObject vacío: "Switch_Ceiling_01"
2. Posiciónalo donde quieres el interruptor
3. Añade componente: LightSwitch
4. Configura:
   - Interaction Key: C
   - Interaction Distance: 2.0
   - Player Tag: "Player"
5. En "Controlled Lights":
   - Arrastra "Lamp_Ceiling_01" al array
```

#### 3. Crear el Indicador UI (Simple)

**Opción A - World Space (Más fácil):**
```
1. En la escena, crea: UI > Canvas
2. Canvas Settings:
   - Render Mode: World Space
   - Adjust scale to something small (0.01, 0.01, 0.01)
3. Dentro del Canvas crea: Text - TextMeshPro
4. Configura el texto: "C"
5. Ajusta tamaño, color, etc.
6. Guarda como Prefab: "InteractionIndicator_Prefab"
7. En LightSwitch:
   - Asigna el prefab a "UI Indicator Prefab"
   - Deja "Target Canvas" en null (World Space)
   - Ajusta "Indicator Offset" (ej: 0, 1, 0)
```

**Opción B - Screen Space:**
```
1. Usa el Canvas principal de la UI
2. Crea el indicador como hijo del Canvas
3. Guarda como prefab
4. En LightSwitch asigna el Canvas en "Target Canvas"
```

#### 4. Setup Manager (Recomendado)

```
1. Crea GameObject vacío: "InteractiveLightsManager"
2. Añade componente: InteractiveLightsManager
3. Marca "Auto Register On Start"
4. Opcional: Context Menu > Generate Lights Report
```

---

## 📝 Notas Importantes

### Excluir Luces del Ciclo Día/Noche

Para que `Lamp_Ceiling_01` no sea controlada por el sistema automático de día/noche:

1. Ve a `SceneLightingManager` en tu escena
2. En la lista "Indoor Lights", **remueve** `Lamp_Ceiling_01`
3. Ahora solo se controla por el interruptor

### Escalabilidad

El sistema está diseñado para ser escalable:

- **Un interruptor → Múltiples luces**: Un switch puede controlar varias luces
- **Múltiples interruptores → Una luz**: Varios switches pueden controlar la misma luz
- **Búsqueda automática**: Usa los context menu helpers para conectar luces automáticamente
- **Manager centralizado**: Control global opcional para cutscenes, eventos, etc.

---

## 🎨 Ajustes Recomendados por Horario

### Día (8:00 - 18:00)
- Global Light Intensity: 0.7
- Color: Cálido (1f, 0.92f, 0.85f)
- Luces interiores: OFF (apagadas automáticamente)
- Ventanas: Intensidad reducida (x0.4)

### Atardecer (18:00 - 20:00)
- Global Light Intensity: 0.5
- Color: Rojizo/Anaranjado (1f, 0.45f, 0.25f)
- Contraste más marcado
- Las luces interiores empiezan a encenderse (19:00)

### Noche (20:00 - 6:00)
- Global Light Intensity: 0.15
- Color: Frío/Azulado (0.35f, 0.45f, 0.65f)
- Luces interiores: ON o controladas por interruptores
- Cuando se prenden luces interiores, la habitación se ve normal/iluminada

---

## 🔍 Debug y Testing

### Context Menu Actions

**InteractableLight:**
- Ninguno (usa Inspector para testing)

**LightSwitch:**
- `Find Nearby Lights`: Auto-conecta luces cercanas
- `Add Light By Name`: Busca por nombre similar

**InteractiveLightsManager:**
- `Register All Lights`: Re-registra todas las luces
- `Register All Switches`: Re-registra todos los switches
- `Turn On All Lights`: Debug - enciende todo
- `Turn Off All Lights`: Debug - apaga todo
- `Generate Lights Report`: Reporte completo del sistema
- `Clean Null References`: Limpia referencias rotas

### Gizmos

Activa Gizmos en Scene View para ver:
- **LightSwitch**: Rango de interacción (esfera), conexiones a luces (líneas)
- **InteractableLight**: Radio de la luz (esfera)

---

## 🚀 Próximos Pasos

1. ✅ Aplica el componente `InteractableLight` a `Lamp_Ceiling_01`
2. ✅ Crea un interruptor con `LightSwitch`
3. ✅ Crea un prefab simple de indicador UI
4. ✅ Prueba la interacción con la tecla C
5. ✅ Ajusta valores de intensidad, colores, y distancia a tu gusto
6. 🔄 Repite el proceso para otras luces que quieras controlar
7. 🎨 Ajusta los valores del ciclo día/noche en `DayNightCycle` según referencias

---

## 💡 Tips

- **Performance**: Las luces interactivas solo actualizan cuando están en transición
- **UI Indicator**: Puedes usar cualquier prefab de UI, solo necesita un TextMeshPro
- **Múltiples Keys**: Si quieres diferentes teclas por interruptor, cámbialo en cada LightSwitch
- **Events**: `InteractableLight.OnLightToggled` permite reaccionar desde otros scripts
- **Sin Manager**: El sistema funciona sin `InteractiveLightsManager`, es opcional

---

## 🎯 Valores Actuales del Sistema

```csharp
// DayNightCycle - Valores por defecto ajustados:
dayIntensity = 0.7f           // Antes: 1.2f
nightIntensity = 0.15f        // Antes: 0.2f
sunsetIntensity = 0.5f        // Antes: 0.8f
windowLightMultiplier = 0.4f  // NUEVO - Reduce 60% brillo ventanas

// Colores ajustados:
dayColor = (1f, 0.92f, 0.85f)      // Más cálido y suave
sunsetColor = (1f, 0.45f, 0.25f)   // Más rojizo
nightColor = (0.35f, 0.45f, 0.65f) // Más frío (luz artificial)
```

---

¡Sistema listo para usar! 🎉
