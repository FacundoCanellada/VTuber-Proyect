# 🎨 Cómo Crear Animaciones con Sprites - Tutorial Paso a Paso

## 📖 Tutorial Visual Completo

### Método 1: Desde Sprite Sheet (Una sola imagen con todos los frames)

#### Paso 1: Preparar el Sprite Sheet
```
1. Selecciona tu imagen en el Project
2. Inspector → Texture Type: "Sprite (2D and UI)"
3. Sprite Mode: "Multiple"
4. Pixels Per Unit: 100 (o el que uses en tu proyecto)
5. Filter Mode: "Point (no filter)" para pixel art
6. Compression: "None" para mejor calidad
7. Click "Apply"
```

#### Paso 2: Cortar el Sprite Sheet
```
1. Click en "Sprite Editor" (botón en Inspector)
2. En la ventana Sprite Editor:
   
   Opción A - Slice Automático:
   ├─ Click "Slice" (arriba)
   ├─ Type: "Automatic"
   ├─ Method: "Delete Existing"
   └─ Click "Slice"
   
   Opción B - Grid Manual:
   ├─ Click "Slice" (arriba)
   ├─ Type: "Grid By Cell Count" o "Grid By Cell Size"
   ├─ Column & Row: número de frames
   └─ Click "Slice"

3. Renombrar cada sprite:
   ├─ Click en cada recuadro
   ├─ Arriba verás "Name"
   └─ Renombra: Idle_North_0, Idle_North_1, etc.

4. Click "Apply" (arriba a la derecha)
5. Cierra el Sprite Editor
```

#### Paso 3: Crear la Animación
```
1. Window → Animation → Animation (abre la ventana)
2. Selecciona tu personaje en la Hierarchy
3. En la ventana Animation:
   ├─ Click "Create New Clip"
   └─ Guarda como "Idle_North.anim" en Assets/Animations/Player/

4. En el Project, busca tu Sprite Sheet
5. Click en la flechita para expandirlo
6. Verás todos los sprites cortados
7. Selecciona los sprites que quieres (Ctrl+Click)
8. Arrástralos TODOS a la ventana Animation
   └─ Unity creará los keyframes automáticamente
```

#### Paso 4: Ajustar la Animación
```
En la ventana Animation:

1. Samples (arriba): Define FPS
   ├─ 6-10 FPS: Animación lenta/retro
   ├─ 12-15 FPS: Pixel art estándar
   └─ 24-30 FPS: Animación fluida

2. Verificar que loop esté activo:
   ├─ Selecciona el clip en el Project
   ├─ Inspector → Loop Time: ✅
   └─ Loop Pose: ✅

3. Preview:
   └─ Click en Play (▶) en la ventana Animation
```

---

### Método 2: Sprites Individuales (Archivos separados)

#### Paso 1: Importar Sprites
```
1. Arrastra tus sprites a Assets/Animations/Player/
2. Selecciona TODOS los sprites importados
3. En Inspector:
   ├─ Texture Type: "Sprite (2D and UI)"
   ├─ Sprite Mode: "Single"
   ├─ Pixels Per Unit: 100
   ├─ Filter Mode: "Point (no filter)" (pixel art)
   └─ Click "Apply"
```

#### Paso 2: Crear Animación Rápido
```
1. Selecciona tu personaje en Hierarchy
2. Window → Animation → Animation
3. Click "Create New Clip"
4. Guarda: "Idle_North.anim"
5. En el Project:
   ├─ Organiza tus sprites en orden (renombra si es necesario)
   ├─ Idle_North_1.png
   ├─ Idle_North_2.png
   └─ Idle_North_3.png
6. Selecciona TODOS los sprites (Shift + Click)
7. Arrástralos a la ventana Animation
   └─ Se crearán los keyframes automáticamente
```

#### Paso 3: Ajustar FPS y Loop
```
Igual que el Método 1, Paso 4
```

---

## 🎯 Agregar Animaciones al Blend Tree

Una vez que tengas tus Animation Clips creados:

### En el Animator Window:

```
1. Abre tu PlayerAnimatorController (doble click)
2. Entra al Blend Tree "Idle" (doble click)
3. Selecciona el Blend Tree (nodo central)
4. En Inspector → Motion:
   
   Para cada dirección:
   ├─ Click en el círculo (⊙) del Motion
   ├─ Selecciona tu Animation Clip (ej: Idle_North)
   ├─ Configura Pos X y Pos Y:
   │  
   │  Norte:      X = 0.0,    Y = 1.0
   │  Noreste:    X = 0.707,  Y = 0.707
   │  Este:       X = 1.0,    Y = 0.0
   │  Sureste:    X = 0.707,  Y = -0.707
   │  Sur:        X = 0.0,    Y = -1.0
   │  Suroeste:   X = -0.707, Y = -0.707
   │  Oeste:      X = -1.0,   Y = 0.0
   │  Noroeste:   X = -0.707, Y = 0.707
   │
   └─ Repite para las 8 direcciones

5. Vuelve a Base Layer (click arriba)
```

---

## 🔄 Workflow Recomendado

### Para crear las 24 animaciones:

```
1. IDLE (8 animaciones):
   ├─ Idle_North.anim
   ├─ Idle_NorthEast.anim
   ├─ Idle_East.anim
   ├─ Idle_SouthEast.anim
   ├─ Idle_South.anim
   ├─ Idle_SouthWest.anim
   ├─ Idle_West.anim
   └─ Idle_NorthWest.anim

2. WALK (8 animaciones):
   └─ (Igual pero con Walk_...)

3. RUN (8 animaciones):
   └─ (Igual pero con Run_...)
```

---

## 💡 Tips Importantes

### ✅ DO (Hacer):
- Nombra consistentemente: `Estado_Direccion_Frame`
- Usa Samples consistente en todas las animaciones del mismo tipo
- Marca Loop Time en animaciones cíclicas
- Organiza sprites en carpetas por estado
- Usa Filter Mode "Point" para pixel art

### ❌ DON'T (No hacer):
- Mezclar FPS diferentes en animaciones similares
- Olvidar activar Loop Time
- Usar nombres inconsistentes
- Dejar sprites sin importar correctamente

---

## 🎮 Verificar que Todo Funciona

### Test Rápido:

```
1. Selecciona tu personaje en Hierarchy
2. Window → Animation → Animation
3. Selecciona cada animación del dropdown
4. Click Play (▶) para ver la animación
5. Verifica que:
   ├─ Los frames se reproduzcan en orden
   ├─ La velocidad sea correcta
   └─ Loop funcione correctamente
```

### Test en Blend Tree:

```
1. Abre el Animator Window
2. Entra al Blend Tree de Idle
3. Mueve los sliders de MoveX y MoveY
4. Observa cómo cambian las animaciones
5. Verifica las 8 direcciones
```

---

## 📊 Ejemplo Práctico

### Si tienes un Sprite Sheet de Idle con 4 frames por dirección:

```
Estructura del Sprite Sheet:
┌─────────────────────────────────┐
│ N_0  N_1  N_2  N_3              │  Fila 1: North
│ NE_0 NE_1 NE_2 NE_3             │  Fila 2: NorthEast
│ E_0  E_1  E_2  E_3              │  Fila 3: East
│ SE_0 SE_1 SE_2 SE_3             │  Fila 4: SouthEast
│ S_0  S_1  S_2  S_3              │  Fila 5: South
│ SW_0 SW_1 SW_2 SW_3             │  Fila 6: SouthWest
│ W_0  W_1  W_2  W_3              │  Fila 7: West
│ NW_0 NW_1 NW_2 NW_3             │  Fila 8: NorthWest
└─────────────────────────────────┘

Proceso:
1. Sprite Editor → Slice → Grid By Cell Count
   ├─ Columns: 4
   └─ Rows: 8

2. Renombrar cada sprite:
   ├─ Fila 1: Idle_North_0, Idle_North_1, Idle_North_2, Idle_North_3
   ├─ Fila 2: Idle_NorthEast_0, etc.
   └─ ...

3. Crear 8 animaciones:
   ├─ Idle_North: usa sprites North_0 a North_3
   ├─ Idle_NorthEast: usa sprites NorthEast_0 a NorthEast_3
   └─ ...

4. Agregar al Blend Tree en las posiciones correspondientes
```

---

## 🚨 Problemas Comunes

### "No veo los sprites en la ventana Animation"
```
✅ Solución:
- Verifica que el personaje tenga un SpriteRenderer
- Asegúrate de tener el GameObject seleccionado
- Verifica que los sprites estén importados como Sprite (2D and UI)
```

### "La animación no hace loop"
```
✅ Solución:
- Selecciona el Animation Clip en Project
- Inspector → Loop Time: ✅ Activar
```

### "Los sprites se ven borrosos"
```
✅ Solución:
- Selecciona los sprites
- Inspector → Filter Mode: "Point (no filter)"
- Compression: "None"
- Apply
```

### "La animación va muy rápido/lento"
```
✅ Solución:
- Window → Animation → Animation
- Cambia el valor de "Samples" (FPS)
- Valores comunes: 6, 12, 24, 30
```

---

## 🎬 Resumen del Proceso Completo

```
1. Importar sprites → Configurar como Sprite 2D
2. (Si es Sprite Sheet) → Slice en Sprite Editor
3. Crear Animation Clip → Window → Animation → Create
4. Arrastrar sprites a la timeline
5. Ajustar FPS (Samples) y activar Loop
6. Repetir para las 24 animaciones
7. Asignar en los Blend Trees del Animator
8. Configurar posiciones (X, Y) en cada Motion
9. Probar en Play Mode
```

---

**¡Ya puedes crear todas tus animaciones! 🎨✨**
