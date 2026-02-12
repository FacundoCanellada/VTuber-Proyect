# Soluciones Visuales (Sin Scripts Nuevos)

Entendido. Aquí tienes las soluciones directas usando las herramientas nativas de Unity, sin crear scripts extra.

## Problema 1: Zzz sobrepasando los límites (Solución Definitiva)

Como usas un Sistema de Partículas, la forma correcta de "cortarlas" visualmente es usar un **Sprite Mask**. Esto ocultará cualquier parte de la "Zzz" que salga del área permitida.

1.  **Crear la Máscara:**
    *   En la Jerarquía, clic derecho -> `2D Object` -> `Sprite Mask`.
    *   Nómbralo "HouseInteriorMask".
    *   Colócalo cubriendo SOLO el interior de la casa (el área donde las Zzz SÍ deben verse).
    *   Usa un Sprite cuadrado simple para la máscara y estíralo si hace falta.

2.  **Configurar las Partículas Zzz:**
    *   Selecciona tu objeto de partículas "Zzz".
    *   Busca el componente **Particle System Renderer** (usualmente al final del componente Particle System, pestaña "Renderer").
    *   Busca la opción **Masking**.
    *   Cámbiala de `No Masking` a `Visible Inside Mask`.
    *   En **Mask Interaction**, selecciona `Visible Inside Mask`.

**Resultado:** Las Zzz solo se dibujarán cuando estén "dentro" del Sprite Mask blanco que creaste. Cuando salgan a la zona negra, desaparecerán instantáneamente.

---

## Problema 2: El Personaje oscurece la Luz (Window_StarLight)

Si el personaje tapa la luz, es porque el Sprite/Efecto de la luz se está dibujando "detrás" del personaje. Como tu personaje cambia su orden automáticamente (para estar delante o detrás de muebles), a veces se pone "delante" de la luz.

Necesitamos forzar que la Luz se dibuje SIEMPRE encima de todo.

1.  **Selecciona el objeto `Window_StarLight`** en tu escena.

2.  **Busca su componente visual:**
    *   Si es un **Light 2D**:
        *   Busca la propiedad **Light Order** (a veces está en "General" o "Blend Style").
        *   Ponle un valor MUY ALTO, por ejemplo: **1000**.
    
    *   Si es un **Sprite Renderer** (usado como cookie o brillo falso):
        *   Busca **Sorting Layer**.
        *   Cámbialo a una capa superior, como "Effects" o crea una nueva llamada "Overlays".
        *   O simplemente pon **Order in Layer** en **1000**.

**¿Por qué pasa esto?**
Tu personaje usa un script (`YDepthSorter`) que le asigna un orden entre -100 y 100 (aprox). Si tu luz tiene orden 0, el personaje la tapará. Al ponerle orden 1000 a la luz, aseguramos que siempre se "pinte" encima del personaje, iluminándolo correctamente.
