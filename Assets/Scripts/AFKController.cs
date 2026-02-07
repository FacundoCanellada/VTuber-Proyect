using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controlador del sistema AFK (Away From Keyboard).
/// Detecta inactividad y activa las animaciones de sueño.
/// </summary>
public class AFKController : MonoBehaviour
{
    [Header("Configuración AFK")]
    [Tooltip("Tiempo en segundos sin actividad para entrar en modo AFK")]
    [SerializeField] private float afkDelay = 60f;
    [Tooltip("Umbral de movimiento del mouse para considerar actividad (en pixeles)")]
    [SerializeField] private float mouseThreshold = 10f;

    [Header("Referencias")]
    [SerializeField] private PlayerAnimationController animationController;
    [SerializeField] private GameObject zzzVisualEffect; // Objeto Zzz (Texto o Partículas)

    private float inactivityTimer;
    private bool isAFK;
    private Vector2 lastMousePosition;
    private Animator animator;

    private void Start()
    {
        if (animationController == null)
        {
            animationController = GetComponent<PlayerAnimationController>();
        }
        
        animator = GetComponent<Animator>();

        // Inicializar posición del mouse
        if (Mouse.current != null)
        {
            lastMousePosition = Mouse.current.position.ReadValue();
        }
        
        // Asegurar que el Zzz empiece apagado
        if (zzzVisualEffect != null) zzzVisualEffect.SetActive(false);
    }

    private void Update()
    {
        if (CheckForInput())
        {
            ResetAFKTimer();
        }
        else
        {
            inactivityTimer += Time.deltaTime;

            if (inactivityTimer >= afkDelay && !isAFK)
            {
                GoAFK();
            }
        }
        
        // Lógica para mostrar/ocultar el Zzz synced con la animación
        UpdateZzzEffect();
    }

    /// <summary>
    /// Maneja la visibilidad del efecto Zzz basándose en el estado actual de la animación
    /// </summary>
    private void UpdateZzzEffect()
    {
        if (!isAFK || zzzVisualEffect == null || animator == null) return;

        // Verificamos si la animación actual es la de dormir
        // Buscamos "Sleep" en el nombre del estado (ej: "AFK_System.AFK_Sleep")
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        // Nota: Ajusta "Sleep" si tu estado se llama diferente
        bool isSleeping = stateInfo.IsName("AFK_Sleep") || 
                          stateInfo.IsName("AFK_System.AFK_Sleep") || 
                          stateInfo.IsName("Base Layer.AFK_System.AFK_Sleep");

        if (isSleeping && !zzzVisualEffect.activeSelf)
        {
            zzzVisualEffect.SetActive(true);
        }
        else if (!isSleeping && zzzVisualEffect.activeSelf)
        {
            // Ocultar si estamos en transición (sentándose o levantándose)
            zzzVisualEffect.SetActive(false);
        }
    }

    /// <summary>
    /// Comprueba si hay algún input del usuario (Teclado o Mouse)
    /// </summary>
    private bool CheckForInput()
    {
        // 1. Verificar Teclado (Solo teclas relevantes para moverse o interactuar, no "anyKey")
        if (Keyboard.current != null)
        {
            // Solo nos interesa si el jugador intenta MOVERSE
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) return true;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) return true;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) return true;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) return true;
            
            // Opcional: También despertar con Espacio o Enter (Interacción)
            if (Keyboard.current.spaceKey.isPressed) return true;
            if (Keyboard.current.enterKey.isPressed) return true;
        }

        // NOTA: Ignoramos el Mouse intencionalmente para que no despierte al moverse
        // El mouse solo servirá para resetear el timer si NO estamos AFK (opcional), 
        // pero una vez dormido, solo el teclado despierta.
        
        return false;
    }

    /// <summary>
    /// Se llama cuando se detecta actividad. Resetea el contador y despierta al personaje.
    /// </summary>
    private void ResetAFKTimer()
    {
        inactivityTimer = 0f;

        if (isAFK)
        {
            WakeUp();
        }
    }

    private void GoAFK()
    {
        isAFK = true;
        Debug.Log("[AFK System] Jugador inactivo. Entrando en modo AFK.");
        
        if (animationController != null)
        {
            animationController.SetAFK(true);
        }
    }

    private void WakeUp()
    {
        isAFK = false;
        
        if (zzzVisualEffect != null) zzzVisualEffect.SetActive(false);
        
        Debug.Log("[AFK System] Actividad detectada. Saliendo de AFK.");
        
        if (animationController != null)
        {
            animationController.SetAFK(false);
        }
    }

    /// <summary>
    /// Devuelve TRUE si el personaje está AFK o está en pleno proceso de levantarse.
    /// Esto sirve para bloquear el movimiento hasta que termine la animación.
    /// </summary>
    public bool IsBlockingMovement()
    {
        // 1. Si estamos oficialmente AFK, bloqueamos.
        if (isAFK) return true;

        // 2. Si NO estamos AFK, pero la animación sigue siendo "Wake" (levantándose), bloqueamos.
        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            
            // Verifica si estamos en el estado de levantarse (asegúrate que el nombre coincida con tu Animator)
            bool isWakingUp = stateInfo.IsName("AFK_Wake") || 
                              stateInfo.IsName("AFK_System.AFK_Wake") ||
                              stateInfo.IsTag("Wake"); // Tip: Puedes ponerle el Tag "Wake" al estado en el Animator
            
            if (isWakingUp) return true;
        }

        return false;
    }
}
