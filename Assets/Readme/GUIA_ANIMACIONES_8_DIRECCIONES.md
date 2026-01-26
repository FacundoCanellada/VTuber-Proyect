# 🎮 Guía Completa: Sistema de Animaciones 8 Direcciones - Top Down

Esta guía te enseñará paso a paso cómo configurar un sistema de animaciones profesional para tu personaje con movimiento en 8 direcciones (arriba, abajo, izquierda, derecha y diagonales) usando Animator Controller y Blend Trees.

---

## 📋 Tabla de Contenidos
1. [Preparación de Animaciones](#1-preparación-de-animaciones)
2. [Crear el Animator Controller](#2-crear-el-animator-controller)
3. [Configurar Parámetros](#3-configurar-parámetros)
4. [Crear Estados y Blend Trees](#4-crear-estados-y-blend-trees)
5. [Configurar Transiciones](#5-configurar-transiciones)
6. [Configurar el GameObject](#6-configurar-el-gameobject)
7. [Probar el Sistema](#7-probar-el-sistema)

---

## 1. Preparación de Animaciones

### Animaciones Necesarias

Para un sistema completo de 8 direcciones necesitas:

#### **IDLE (8 clips):**
- `Idle_North` (mirando arriba)
- `Idle_NorthEast` (diagonal arriba-derecha)
- `Idle_East` (mirando derecha)
- `Idle_SouthEast` (diagonal abajo-derecha)
- `Idle_South` (mirando abajo)
- `Idle_SouthWest` (diagonal abajo-izquierda)
- `Idle_West` (mirando izquierda)
- `Idle_NorthWest` (diagonal arriba-izquierda)

#### **WALK (8 clips):**
- `Walk_North`
- `Walk_NorthEast`
- `Walk_East`
- `Walk_SouthEast`
- `Walk_South`
- `Walk_SouthWest`
- `Walk_West`
- `Walk_NorthWest`

#### **RUN (8 clips):**
- `Run_North`
- `Run_NorthEast`
- `Run_East`
- `Run_SouthEast`
- `Run_South`
- `Run_SouthWest`
- `Run_West`
- `Run_NorthWest`

### Ubicación de Archivos
Coloca todos tus Animation Clips en:
```
Assets/Animations/Player/
```

---

## 2. Crear el Animator Controller

### Paso 1: Crear el Controller
1. En Unity, ve a la carpeta `Assets/AnimationControllers/`
2. Click derecho → `Create` → `Animator Controller`
3. Nómbralo: **`PlayerAnimatorController`**

### Paso 2: Abrir el Animator Window
1. Doble click en `PlayerAnimatorController`
2. Se abrirá la ventana Animator (si no está visible: `Window` → `Animation` → `Animator`)

---

## 3. Configurar Parámetros

En la ventana **Animator**, ve a la pestaña **Parameters** (lado izquierdo).

### Crear los siguientes parámetros:

1. **Speed** (Float)
   - Click en `+` → `Float`
   - Nombre: `Speed`
   - Valor por defecto: `0`
   - **Función:** Controla la velocidad de movimiento (0 = quieto, mayor = más rápido)

2. **IsRunning** (Bool)
   - Click en `+` → `Bool`
   - Nombre: `IsRunning`
   - Valor por defecto: `false`
   - **Función:** Indica si el personaje está corriendo

3. **MoveX** (Float)
   - Click en `+` → `Float`
   - Nombre: `MoveX`
   - Valor por defecto: `0`
   - **Función:** Dirección horizontal (-1 = izquierda, 1 = derecha)

4. **MoveY** (Float)
   - Click en `+` → `Float`
   - Nombre: `MoveY`
   - Valor por defecto: `0`
   - **Función:** Dirección vertical (-1 = abajo, 1 = arriba)

---

## 4. Crear Estados y Blend Trees

### Estado 1: IDLE (Reposo)

#### Paso 1: Crear el Blend Tree
1. Click derecho en el grid del Animator → `Create State` → `From New Blend Tree`
2. Renombrar el estado a: **`Idle`**
3. Doble click en el estado `Idle` para entrar al Blend Tree

#### Paso 2: Configurar el Blend Tree de Idle
1. Selecciona el nodo del Blend Tree en el centro
2. En el **Inspector**, configura:
   - **Blend Type:** `2D Simple Directional`
   - **Parameters:**
     - Primera posición: `MoveX`
     - Segunda posición: `MoveY`

#### Paso 3: Agregar las 8 animaciones de Idle
1. En la sección **Motion**, haz click en `+` para agregar 8 motion fields
2. Arrastra cada Animation Clip de Idle y configura sus posiciones:

| Animation Clip | Pos X | Pos Y | Dirección |
|----------------|-------|-------|-----------|
| Idle_North | 0 | 1 | ↑ |
| Idle_NorthEast | 0.707 | 0.707 | ↗ |
| Idle_East | 1 | 0 | → |
| Idle_SouthEast | 0.707 | -0.707 | ↘ |
| Idle_South | 0 | -1 | ↓ |
| Idle_SouthWest | -0.707 | -0.707 | ↙ |
| Idle_West | -1 | 0 | ← |
| Idle_NorthWest | -0.707 | 0.707 | ↖ |

> **💡 Tip:** Los valores 0.707 son aproximadamente √2/2, lo que crea las diagonales perfectas a 45°.

#### Paso 4: Salir del Blend Tree
- Click en `Base Layer` en la parte superior del Animator para volver a la vista principal

---

### Estado 2: WALK (Caminar)

#### Paso 1: Crear el Blend Tree
1. Click derecho en el grid → `Create State` → `From New Blend Tree`
2. Renombrar a: **`Walk`**
3. Doble click para entrar

#### Paso 2: Configurar
1. **Blend Type:** `2D Simple Directional`
2. **Parameters:**
   - Primera: `MoveX`
   - Segunda: `MoveY`

#### Paso 3: Agregar las 8 animaciones de Walk
Configura igual que Idle pero con los clips de Walk:

| Animation Clip | Pos X | Pos Y |
|----------------|-------|-------|
| Walk_North | 0 | 1 |
| Walk_NorthEast | 0.707 | 0.707 |
| Walk_East | 1 | 0 |
| Walk_SouthEast | 0.707 | -0.707 |
| Walk_South | 0 | -1 |
| Walk_SouthWest | -0.707 | -0.707 |
| Walk_West | -1 | 0 |
| Walk_NorthWest | -0.707 | 0.707 |

#### Paso 4: Volver a Base Layer

---

### Estado 3: RUN (Correr)

#### Paso 1: Crear el Blend Tree
1. Click derecho → `Create State` → `From New Blend Tree`
2. Renombrar a: **`Run`**
3. Doble click para entrar

#### Paso 2: Configurar
1. **Blend Type:** `2D Simple Directional`
2. **Parameters:**
   - Primera: `MoveX`
   - Segunda: `MoveY`

#### Paso 3: Agregar las 8 animaciones de Run
| Animation Clip | Pos X | Pos Y |
|----------------|-------|-------|
| Run_North | 0 | 1 |
| Run_NorthEast | 0.707 | 0.707 |
| Run_East | 1 | 0 |
| Run_SouthEast | 0.707 | -0.707 |
| Run_South | 0 | -1 |
| Run_SouthWest | -0.707 | -0.707 |
| Run_West | -1 | 0 |
| Run_NorthWest | -0.707 | 0.707 |

#### Paso 4: Volver a Base Layer

---

## 5. Configurar Transiciones

Ahora debes conectar los estados para que transicionen correctamente.

### Transición 1: Idle → Walk

1. Click derecho en `Idle` → `Make Transition`
2. Arrastra la flecha hacia `Walk`
3. Selecciona la transición (la flecha blanca)
4. En el **Inspector**, configura:
   - **Has Exit Time:** ❌ Desactivar
   - **Transition Duration:** `0.25` (suave)
   - **Conditions:**
     - Click en `+`
     - Agregar: `Speed` `Greater` `0.1`

### Transición 2: Walk → Idle

1. Click derecho en `Walk` → `Make Transition` → Hacia `Idle`
2. Configurar:
   - **Has Exit Time:** ❌ Desactivar
   - **Transition Duration:** `0.25`
   - **Conditions:**
     - `Speed` `Less` `0.1`

### Transición 3: Walk → Run

1. `Walk` → `Make Transition` → Hacia `Run`
2. Configurar:
   - **Has Exit Time:** ❌ Desactivar
   - **Transition Duration:** `0.2`
   - **Conditions:**
     - `IsRunning` `true`

### Transición 4: Run → Walk

1. `Run` → `Make Transition` → Hacia `Walk`
2. Configurar:
   - **Has Exit Time:** ❌ Desactivar
   - **Transition Duration:** `0.2`
   - **Conditions:**
     - `IsRunning` `false`

### Transición 5: Run → Idle

1. `Run` → `Make Transition` → Hacia `Idle`
2. Configurar:
   - **Has Exit Time:** ❌ Desactivar
   - **Transition Duration:** `0.25`
   - **Conditions:**
     - `Speed` `Less` `0.1`

### Configurar Entry State
1. Asegúrate de que `Idle` sea el estado por defecto (debe tener una flecha naranja desde `Entry`)
2. Si no lo es, click derecho en `Idle` → `Set as Layer Default State`

---

## 6. Configurar el GameObject

### Paso 1: Preparar el Personaje
1. Selecciona tu GameObject del personaje en la jerarquía
2. Si no tiene un componente **Animator**, agrégalo:
   - Click en `Add Component`
   - Busca y agrega: `Animator`

### Paso 2: Asignar el Controller
1. Con el personaje seleccionado
2. En el componente **Animator** en el Inspector
3. Arrastra `PlayerAnimatorController` al campo **Controller**

### Paso 3: Agregar el Script de Animación
1. Click en `Add Component`
2. Busca: `Player Animation Controller`
3. Configurar en el Inspector:
   - **Animator:** Debería auto-asignarse, si no, arrástralo
   - **Run Threshold:** `5` (velocidad a partir de la cual corre)
   - **Animation Smooth Time:** `0.1` (suavizado)

### Paso 4: Agregar Control de Movimiento (Opcional)
Si quieres probar inmediatamente:
1. `Add Component` → `Player Movement Example`
2. Configurar:
   - **Walk Speed:** `3`
   - **Run Speed:** `6`
   - **Rb:** Asigna tu Rigidbody2D si usas física

---

## 7. Probar el Sistema

### Prueba en Play Mode
1. Click en **Play** ▶️
2. Usa las teclas:
   - **WASD / Flechas:** Movimiento en 8 direcciones
   - **Shift:** Mantén presionado para correr

### Debugging en el Animator
1. Con el juego en ejecución (Play Mode)
2. Abre la ventana **Animator** con tu Controller abierto
3. Verás en tiempo real:
   - Qué estado está activo (azul)
   - Los valores de los parámetros cambiando
   - Las transiciones activándose

### Ajustar Valores en Tiempo Real
Puedes ajustar los parámetros manualmente mientras el juego corre:
1. En el **Animator Window** → Pestaña **Parameters**
2. Mueve los sliders de `MoveX`, `MoveY`, `Speed`
3. Activa/desactiva `IsRunning`
4. Observa cómo cambian las animaciones

---

## 🎯 Resumen de Configuración

### Parámetros del Animator
```
- Speed (Float): Magnitud de la velocidad
- IsRunning (Bool): Si está corriendo
- MoveX (Float): Dirección horizontal (-1 a 1)
- MoveY (Float): Dirección vertical (-1 a 1)
```

### Estados
```
1. Idle → Blend Tree 2D (8 direcciones)
2. Walk → Blend Tree 2D (8 direcciones)
3. Run → Blend Tree 2D (8 direcciones)
```

### Transiciones
```
Idle ↔ Walk: Basado en Speed
Walk ↔ Run: Basado en IsRunning
Run → Idle: Basado en Speed
```

---

## 🔧 Consejos y Mejores Prácticas

### 1. Optimización
- Los Blend Trees 2D Simple Directional son más eficientes que otros tipos
- Los parámetros cacheados (con StringToHash) mejoran el rendimiento

### 2. Suavizado
- `Animation Smooth Time` controla qué tan suave son las transiciones
- Valores más altos (0.2-0.3) = más suave pero menos responsive
- Valores más bajos (0.05-0.1) = más directo pero puede verse brusco

### 3. Velocidades
- Ajusta `Run Threshold` según la velocidad de tu personaje
- Debe ser un valor intermedio entre walk y run speed

### 4. Animaciones Faltantes
Si no tienes todas las 8 direcciones aún, puedes:
- Usar 4 direcciones (N, S, E, W) y eliminar las diagonales
- Duplicar animaciones temporalmente
- Usar Blend Type `2D Freeform Directional` para menos precisión

### 5. Testing
- Activa `Gizmos` en la Game View para ver la dirección del movimiento
- Usa el Animator Window en Play Mode para debugging
- Ajusta las transiciones si se siente "perezoso" o "brusco"

---

## ❓ Solución de Problemas

### El personaje no anima
✅ Verifica que:
- El Animator tenga el Controller asignado
- El script PlayerAnimationController esté adjunto
- Los parámetros tengan los nombres exactos
- Haya animaciones asignadas en los Blend Trees

### Las animaciones no cambian de dirección
✅ Verifica que:
- MoveX y MoveY estén recibiendo valores
- Las posiciones en el Blend Tree sean correctas
- El Blend Type sea "2D Simple Directional"

### Las transiciones son muy bruscas
✅ Ajusta:
- Aumenta `Transition Duration` (0.3-0.5)
- Aumenta `Animation Smooth Time` en el script

### El personaje no corre
✅ Verifica:
- `IsRunning` se está activando
- La transición Walk → Run existe y tiene la condición correcta
- `Run Threshold` es menor que tu velocidad de correr

---

## 📚 Archivos Creados

```
Assets/
├── Animations/
│   └── Player/
│       ├── Idle_North.anim (y otros 7)
│       ├── Walk_North.anim (y otros 7)
│       └── Run_North.anim (y otros 7)
├── AnimationControllers/
│   └── PlayerAnimatorController.controller
└── Scripts/
    ├── PlayerAnimationController.cs
    └── PlayerMovementExample.cs
```

---

## 🎓 Siguiente Nivel

Una vez domines este sistema, puedes:
1. Agregar más estados (atacar, saltar, morir)
2. Implementar Animation Events
3. Usar Animation Layers para animaciones simultáneas
4. Agregar IK (Inverse Kinematics) para apuntar
5. Implementar Root Motion para movimiento más realista

---

## 📞 Soporte

Si tienes problemas:
1. Revisa la consola de Unity para errores
2. Verifica que todos los nombres coincidan exactamente
3. Asegúrate de que las animaciones estén configuradas para Loop
4. Comprueba que el Animator no esté desactivado

---

**¡Buena suerte con tu proyecto VTuber! 🎮✨**
