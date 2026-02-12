# Guía de Corrección de Problemas Visuales

## Problema 1: Las Zzz sobrepasan los límites de la casa

### Solución A: Usando Bounds Controller (Script Creado)

1. **Agregar el script al GameObject de las Zzz:**
   - Selecciona el GameObject que contiene el efecto Zzz en la jerarquía
   - Add Component → `ZzzBoundsController`

2. **Configurar los límites:**
   - En el Inspector, ajusta los valores de **Límites de Renderizado**:
     - `Min X`: Límite izquierdo de la casa (ej: -8)
     - `Max X`: Límite derecho de la casa (ej: 8)
     - `Min Y`: Límite inferior (ej: -3)
     - `Max Y`: Límite superior (ej: 10)
   
3. **Asignar referencias:**
   - Si las Zzz son un **Particle System**, arrastra el componente a `Particle System Zzz`
   - Si son **Sprites**, arrastra los Sprite Renderers al array `Sprite Renderers Zzz`

4. **Visualizar límites:**
   - Selecciona el GameObject en la jerarquía
   - Verás un rectángulo cyan en la Scene View mostrando los límites
   - Ajusta los valores hasta que el rectángulo cubra solo el interior de la casa

### Solución B: Usando Sprite Mask (Método Manual)

Si prefieres un enfoque visual más preciso:

1. **Crear un Sprite Mask:**
   ```
   Hierarchy → Click derecho → 2D Object → Sprite Mask
   ```

2. **Configurar el Sprite Mask:**
   - Posiciónalo en la escena cubriendo el área interior de la casa
   - En el Inspector:
     - `Sprite`: Usa un sprite rectangular blanco o crea uno
     - `Alpha Cutoff`: 0.01
     - `Interaction`: Visible Inside Mask

3. **Configurar las Zzz para usar el Mask:**
   - Selecciona el GameObject de las Zzz
   - En el **Sprite Renderer** (si son sprites):
     - `Mask Interaction`: Visible Inside Mask
   - En el **Particle System Renderer** (si son partículas):
     - `Mask Interaction`: Visible Inside Mask

### Solución C: Ajustar Sorting Layers

1. **Crear un nuevo Sorting Layer:**
   - Edit → Project Settings → Tags and Layers
   - Expande **Sorting Layers**
   - Agrega: `Interior Effects`

2. **Asignar el layer a las Zzz:**
   - Selecciona el GameObject de las Zzz
   - En el Sprite Renderer / Particle System Renderer:
     - `Sorting Layer`: Interior Effects
     - `Order in Layer`: 10

3. **Configurar la cámara:**
   - Selecciona la Main Camera
   - En el componente **Camera**:
     - Asegúrate que `Culling Mask` incluya solo los layers dentro de la casa

---

## Problema 2: Window_StarLight se oscurece cuando el personaje pasa

### Causa del Problema

El sistema `YDepthSorter` del personaje cambia su sorting order dinámicamente. Cuando el personaje tiene un sorting order mayor que las luces, las "tapa" visualmente.

### Solución Principal: Configurar Layers Correctamente

#### Paso 1: Crear Sorting Layers Necesarios

1. **Abrir configuración de layers:**
   - Edit → Project Settings → Tags and Layers
   - Sección: **Sorting Layers**

2. **Asegúrate de tener estos layers (en este orden):**
   ```
   0. Default
   1. Background
   2. Lighting        ← NUEVO (si no existe)
   3. Foreground
   4. Character
   5. Effects
   ```

#### Paso 2: Asignar el Script WindowLightFixLayer

1. **Agregar a cada luz de ventana:**
   - Selecciona el GameObject `Window_StarLight` en la jerarquía
   - Add Component → `WindowLightFixLayer`

2. **Configurar en el Inspector:**
   ```
   Light Sorting Layer: "Lighting"
   Light Sorting Order: 100
   Target Sorting Layers: ["Default", "Background", "Foreground"]
   Blend Mode: Additive
   ```

3. **Si la luz tiene un sprite visible:**
   - Arrastra el Sprite Renderer al campo `Sprite Renderer`
   - El script lo configurará automáticamente

#### Paso 3: Configurar el Light2D Component

1. **Selecciona Window_StarLight**
2. **En el componente Light 2D:**
   - `Light Order`: 100 (o mayor)
   - `Blend Style`: Additive
   - `Intensity`: Ajustar según necesites (ej: 1.5)
   - `Target Sorting Layers`: 
     - ✅ Default
     - ✅ Background
     - ✅ Foreground
     - ❌ Character (desmarcar esto)
     - ❌ Lighting

#### Paso 4: Configurar el Personaje

1. **Selecciona el GameObject del Player**
2. **En el Sprite Renderer:**
   - `Sorting Layer`: Character
   - `Order in Layer`: Lo manejará YDepthSorter automáticamente

3. **En el script YDepthSorter:**
   - `Base Sorting Order`: 0
   - `Precision`: 10
   - Esto hará que el personaje tenga orders entre -100 y 100 aproximadamente

### Solución Alternativa: Ajustar Light Order

Si lo anterior no funciona completamente:

1. **Aumentar el Light Order de las ventanas:**
   - Selecciona cada Window_StarLight
   - Light 2D Component:
     - `Light Order`: 200 (muy alto)

2. **Asegurar que el personaje NO tape las luces:**
   - En el Player, componente Sprite Renderer:
     - `Sorting Layer`: Character (debe ser diferente a Lighting)

### Solución Avanzada: Render Pipeline Adjustment

Si el problema persiste:

1. **Abrir el Universal Render Pipeline Asset:**
   - Project → Settings → UniversalRenderPipelineAsset
   - Doble click para editar

2. **Configurar 2D Renderer:**
   - En la sección **Renderer List**, selecciona el 2D Renderer
   - En el 2D Renderer Asset:
     - `Blend Styles`: Asegurar que Additive esté configurado
     - `Light Blend Styles → Custom`:
       - Blend Mode: Additive
       - Render Pass: Before Transparent

---

## Testing y Verificación

### Para las Zzz:

1. Activa el sistema AFK
2. Espera a que aparezcan las Zzz
3. Verifica que NO se vean en las áreas negras fuera de la casa
4. Si se ven, ajusta los bounds en `ZzzBoundsController`

### Para las Luces:

1. Mueve el personaje cerca de Window_StarLight
2. Las luces deben mantener su brillo constante
3. Si se oscurecen:
   - Verifica el Sorting Layer del personaje vs la luz
   - Aumenta el `Light Order` de la luz
   - Verifica que la luz no tenga el layer "Character" en Target Sorting Layers

---

## Configuración Rápida (Resumen)

### Zzz Bounds:
```
GameObject: [Objeto Zzz]
Add Component: ZzzBoundsController
Min X: -8, Max X: 8
Min Y: -3, Max Y: 10
```

### Window Lights:
```
GameObject: Window_StarLight
Add Component: WindowLightFixLayer
Sorting Layer: Lighting
Sorting Order: 100

Light 2D Component:
- Light Order: 100+
- Target Sorting Layers: Default, Background, Foreground (NO Character)
```

### Player:
```
Sprite Renderer:
- Sorting Layer: Character
- Order in Layer: (manejado por YDepthSorter)
```

---

## Troubleshooting

### Las Zzz aún se ven fuera:
- Verifica que el script esté activo
- Ajusta los bounds manualmente en el Inspector
- Considera usar Sprite Mask si el problema persiste

### Las luces siguen oscureciéndose:
- Asegúrate que el personaje NO esté en el layer "Lighting"
- Aumenta Light Order a 500
- Verifica que Target Sorting Layers NO incluya "Character"
- Prueba cambiar Blend Mode a "Multiply" y luego devolver a "Additive"

### Problemas de rendimiento:
- En ZzzBoundsController, desactiva `auto update` en Start() si las Zzz son estáticas
- Limita el número de Sprite Renderers en el array
