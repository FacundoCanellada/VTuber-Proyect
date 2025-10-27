# Sistema de Animaciones - Referencia Rápida

## 🎯 Posiciones para Blend Tree 2D

Usa estas posiciones exactas para las 8 direcciones en tus Blend Trees:

```
Norte (↑):           X =  0.000, Y =  1.000
Noreste (↗):         X =  0.707, Y =  0.707
Este (→):            X =  1.000, Y =  0.000
Sureste (↘):         X =  0.707, Y = -0.707
Sur (↓):             X =  0.000, Y = -1.000
Suroeste (↙):        X = -0.707, Y = -0.707
Oeste (←):           X = -1.000, Y =  0.000
Noroeste (↖):        X = -0.707, Y =  0.707
```

## 📊 Tabla de Animaciones Requeridas

| Estado | Direcciones | Total Clips |
|--------|-------------|-------------|
| Idle   | 8           | 8 clips     |
| Walk   | 8           | 8 clips     |
| Run    | 8           | 8 clips     |
| **TOTAL** |         | **24 clips** |

## 🔧 Configuración del Inspector

### Componente Animator
```
Controller: PlayerAnimatorController
Avatar: None (para 2D)
Apply Root Motion: ❌ False
Update Mode: Normal
Culling Mode: Based On Renderers
```

### PlayerAnimationController
```
Animator: Auto-asignado
Run Threshold: 5.0
Animation Smooth Time: 0.1
```

### PlayerMovementExample (opcional)
```
Walk Speed: 3.0
Run Speed: 6.0
Rb: Tu Rigidbody2D
```

## ⚡ Checklist de Configuración

- [ ] Crear 24 Animation Clips (8 x 3 estados)
- [ ] Crear PlayerAnimatorController
- [ ] Agregar 4 parámetros (Speed, IsRunning, MoveX, MoveY)
- [ ] Crear 3 Blend Trees (Idle, Walk, Run)
- [ ] Configurar cada Blend Tree como 2D Simple Directional
- [ ] Asignar las 8 animaciones en cada Blend Tree
- [ ] Configurar las posiciones correctas (ver tabla arriba)
- [ ] Crear 5 transiciones entre estados
- [ ] Establecer Idle como estado por defecto
- [ ] Asignar Animator al GameObject
- [ ] Asignar Controller al Animator
- [ ] Agregar script PlayerAnimationController
- [ ] Probar en Play Mode

## 🎮 Controles por Defecto

```
Movimiento: WASD o Flechas
Correr: Mantener Shift (izquierdo o derecho)
```

## 💡 Valores Recomendados

### Transiciones
```
Idle ↔ Walk:     Duration: 0.25, Exit Time: OFF
Walk ↔ Run:      Duration: 0.20, Exit Time: OFF
Run → Idle:      Duration: 0.25, Exit Time: OFF
```

### Condiciones
```
Idle → Walk:     Speed > 0.1
Walk → Idle:     Speed < 0.1
Walk → Run:      IsRunning = true
Run → Walk:      IsRunning = false
Run → Idle:      Speed < 0.1
```

## 🐛 Debug Tips

1. **Ver parámetros en tiempo real:**
   - Play Mode → Animator Window → Parameters tab

2. **Ver estado activo:**
   - El estado activo se muestra en azul

3. **Probar manualmente:**
   - Mueve los sliders de MoveX/MoveY manualmente
   - Verás cómo cambian las animaciones

4. **Común error:** "Parameter does not exist"
   - Revisa mayúsculas/minúsculas
   - Los nombres deben ser exactos

## 📐 Matemática de las Diagonales

Para que las diagonales sean perfectas a 45°:
- Usamos √2/2 ≈ 0.707
- Esto mantiene una distancia de 1 desde el centro
- √(0.707² + 0.707²) = √(0.5 + 0.5) = √1 = 1

Puedes visualizarlo como un círculo unitario:
```
        (0, 1)
          |
(-0.7, 0.7)   (0.7, 0.7)
          |
(-1, 0)---+---(1, 0)
          |
(-0.7,-0.7)   (0.7,-0.7)
          |
        (0, -1)
```
