# 🎮 Setup Rápido para Lamp_Ceiling_01
## Instrucciones específicas para tu proyecto

---

## ✅ Cambios Realizados

### 1. **Valores de Iluminación Ajustados**
He modificado los valores en [DayNightCycle.cs](Assets/Scripts/DayNightCycle.cs):

- **Día**: Intensidad 0.7 (antes 1.2) - Más tenue y difusa
- **Noche**: Intensidad 0.15 (antes 0.2) - Más oscura
- **Atardecer**: Intensidad 0.5 (antes 0.8) - Más contraste
- **Ventanas**: Nuevo multiplicador 0.4 (60% de reducción de brillo)
- **Colores**: Ajustados para ser más cálidos (día) y más fríos (noche)

### 2. **Nuevos Scripts Creados**

✅ **InteractableLight.cs** - Luz que se prende/apaga independientemente
✅ **LightSwitch.cs** - Interruptor con detección de jugador e indicador
✅ **InteractiveLightsManager.cs** - Manager opcional para control global
✅ **InteractionIndicator.cs** - Componente para el indicador UI animado

---

## 🚀 Pasos para Configurar Lamp_Ceiling_01

### **Paso 1: Preparar la Luz**

1. En Unity, selecciona tu GameObject `Lamp_Ceiling_01`
2. **Remuévela** de la lista `Indoor Lights` en `SceneLightingManager`
   - Esto evita que el ciclo día/noche la controle automáticamente
3. Añade el componente **InteractableLight**:
   ```
   Add Component → Scripts → Interactable Light
   ```
4. Configura los valores:
   ```
   Start On: false (o true si quieres que inicie encendida)
   On Intensity: 1.5 - 2.0 (ajusta según tu preferencia visual)
   Off Intensity: 0
   Transition Speed: 0.3
   ```

### **Paso 2: Crear el Interruptor**

1. En la Hierarchy, crea un GameObject vacío donde quieras el interruptor:
   ```
   Right Click → Create Empty
   Nombre: "Switch_Ceiling" (o el nombre que prefieras)
   ```
2. Posiciónalo en la pared o lugar donde estará físicamente el interruptor
3. Añade el componente **LightSwitch**:
   ```
   Add Component → Scripts → Light Switch
   ```
4. Configura:
   ```
   Interaction Key: C
   Interaction Distance: 2.0
   Player Tag: "Player" (verifica que tu jugador tenga este tag)
   ```
5. En la sección **Controlled Lights**:
   - Click en el "+" para añadir elemento
   - Arrastra `Lamp_Ceiling_01` desde la Hierarchy al slot

### **Paso 3: Crear el Indicador UI**

Hay dos opciones (recomiendo la A por simplicidad):

#### **Opción A - World Space (Recomendada)**

1. Crea un Canvas en modo World Space:
   ```
   Right Click en Hierarchy → UI → Canvas
   Nombre: "UI_WorldSpace" o similar
   ```
2. Selecciona el Canvas y configura:
   ```
   Render Mode: World Space
   Position: Ajusta según necesites
   Scale: (0.01, 0.01, 0.01) - Para que sea del tamaño apropiado
   Sorting Layer: Asegúrate que esté visible sobre el fondo
   ```
3. Dentro del Canvas, crea un texto:
   ```
   Right Click en Canvas → UI → Text - TextMeshPro
   Nombre: "InteractionIndicator"
   ```
   - Si es la primera vez usando TMP, Unity te pedirá importar resources → Hazlo
4. Configura el texto:
   ```
   Text: "C"
   Font Size: 36 (ajusta al gusto)
   Color: Blanco o el que prefieras
   Alignment: Center (horizontal y vertical)
   ```
5. Opcional - Añade el componente **InteractionIndicator** para animación:
   ```
   Add Component → Scripts → Interaction Indicator
   Animate: true
   Anim Type: Bounce
   Anim Speed: 2
   ```
6. **Guarda como Prefab**:
   - Arrastra el objeto "InteractionIndicator" desde la Hierarchy a la carpeta Assets
   - Esto creará el prefab reutilizable
7. **Elimina** el InteractionIndicator de la escena (se instanciará automáticamente)
8. Vuelve a `Switch_Ceiling` y configura:
   ```
   UI Indicator Prefab: Arrastra el prefab que acabas de crear
   Indicator Offset: (0, 1, 0) - Ajusta la altura sobre el interruptor
   Indicator Text: "C"
   Target Canvas: Déjalo en null (None) para World Space
   ```

#### **Opción B - Screen Space Overlay**

Si prefieres que el indicador esté en el UI principal:

1. Usa tu Canvas principal de UI (el que ya tienes)
2. Crea un hijo del Canvas con un TextMeshPro
3. Configúralo similar a la Opción A
4. Guarda como prefab
5. En el LightSwitch, asigna:
   ```
   UI Indicator Prefab: Tu prefab
   Target Canvas: Tu Canvas principal
   ```

### **Paso 4: Testing**

1. Presiona Play
2. Acércate al interruptor con tu personaje
3. Cuando estés en rango (2 metros), debería aparecer el indicador "C"
4. Presiona **C** para encender/apagar la luz
5. La luz debería hacer transición suave entre on/off

### **Paso 5: Ajustes Finos (Opcional)**

#### Si quieres sonidos:
```
1. En Lamp_Ceiling_01 (InteractableLight):
   - Turn On Sound: Arrastra tu audio clip
   - Turn Off Sound: Arrastra tu audio clip
   - Sound Volume: 0.5

2. En Switch_Ceiling (LightSwitch):
   - Switch Sound: Arrastra audio clip del click
   - Switch Sound Volume: 0.7
```

#### Si quieres efectos visuales:
```
En Lamp_Ceiling_01 (InteractableLight):
- Light Effect Object: Un GameObject con sprite glow/halo
- Light Sprite: SpriteRenderer que se transparenta cuando apaga
- Particles: Sistema de partículas (opcional)
```

#### Si quieres sprites de interruptor ON/OFF:
```
En Switch_Ceiling:
1. Añade un SpriteRenderer al GameObject
2. En LightSwitch configura:
   - Switch On Sprite: Sprite del interruptor activado
   - Switch Off Sprite: Sprite del interruptor desactivado
   - Switch Renderer: Arrastra el SpriteRenderer
```

---

## 🎯 Manager Opcional (Para múltiples luces)

Si planeas tener varias luces interactivas:

1. Crea un GameObject vacío: "InteractiveLightsManager"
2. Añade componente **InteractiveLightsManager**
3. Marca **Auto Register On Start**
4. Esto te permitirá:
   - Control global de todas las luces
   - Encender/apagar todo desde scripts
   - Generar reportes del sistema

Puedes usar el manager desde cualquier script:
```csharp
// Ejemplo:
InteractiveLightsManager.Instance.TurnOnAllLights();
InteractiveLightsManager.Instance.TurnOffAllLights();
```

---

## 🐛 Troubleshooting

### "El indicador no aparece"
- ✅ Verifica que tu jugador tenga el tag "Player"
- ✅ Aumenta `Interaction Distance` en el LightSwitch
- ✅ Asegúrate que el prefab del indicador está asignado
- ✅ Revisa que el Canvas tenga el Render Mode correcto
- ✅ Verifica los Sorting Layers si no es visible

### "La luz no se enciende/apaga"
- ✅ Verifica que Lamp_Ceiling_01 esté en `Controlled Lights`
- ✅ Asegúrate que el componente InteractableLight esté presente
- ✅ Revisa que la tecla C no esté siendo usada por otro sistema
- ✅ Mira la consola para errores

### "El ciclo día/noche aún controla la luz"
- ✅ Remuévela de la lista `Indoor Lights` en SceneLightingManager
- ✅ El sistema respeta las luces con InteractableLight si no están en listas automáticas

### "Performance issues"
- ✅ Las luces interactivas son eficientes, solo actualizan durante transiciones
- ✅ Si tienes muchas luces, considera ajustar `Transition Speed` a 0.1 para ser más rápida

---

## 📝 Próximos Pasos

1. ✅ **Configurar Lamp_Ceiling_01** siguiendo los pasos de arriba
2. ⏭️ **Probar y ajustar** valores de intensidad, colores, distancia
3. ⏭️ **Repetir el proceso** para otras luces que quieras controlar
4. ⏭️ **Ajustar los valores globales** del ciclo día/noche en el Inspector
5. ⏭️ **Crear más interruptores** para otras habitaciones/luces

---

## 💡 Tips Adicionales

- **Un switch, varias luces**: Puedes añadir múltiples luces al array `Controlled Lights`
- **Múltiples switches**: Varios switches pueden controlar la misma luz
- **Teclas diferentes**: Cada switch puede usar una tecla distinta
- **Testing rápido**: Usa los botones de Context Menu (Right Click en component)
  - `Find Nearby Lights` busca luces automáticamente
  - `Add Light By Name` conecta por nombre similar

---

## 🎨 Valores Recomendados según tu Referencia

Basándome en las imágenes que compartiste:

### **Para luz de techo (Lamp_Ceiling_01)**
```
On Intensity: 1.5
Color Temperature: Cálida (amarilla/naranja)
Radius/Range: Suficiente para cubrir la habitación
```

### **Durante el día**
```
La luz de techo APAGADA (ventanas iluminan naturalmente)
Global Light Intensity: 0.7
Window Lights Multiplier: 0.4
```

### **Durante la noche**
```
La luz de techo ON (si el jugador la prende)
Global Light Intensity: 0.15
Habitación oscura sin luz interior
```

---

¡Todo listo! Si necesitas ajustar algo más o tienes dudas, avísame! 🎉
