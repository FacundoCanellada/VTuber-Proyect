using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// Controlador de movimiento con WASD usando el NUEVO Input System
/// </summary>
[RequireComponent(typeof(PlayerAnimationController))]
public class PlayerMovementNew : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float wsJitterSpeedMultiplier = 0.5f;
    
    [Header("Referencias")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private AFKController afkController;
    [SerializeField] private SpriteJitter spriteJitter;
    
    private PlayerAnimationController animationController;
    private Vector2 moveInput;
    private bool isRunning;
    private int lastHorizontalInput;
    private int lastVerticalInput = -1;
    private bool forceVerticalJitter;

    private void Awake()
    {
        animationController = GetComponent<PlayerAnimationController>();
        
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (afkController == null) afkController = GetComponent<AFKController>(); // Intentar autodetectar
        if (spriteJitter == null) spriteJitter = GetComponent<SpriteJitter>();

        // Configuración de seguridad para movimiento
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            // Aseguramos que solo la rotación esté bloqueada, NO la posición
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.simulated = true;
        }

        // FIX CRITICO: Desactivar Root Motion del Animator para que no sobreescriba el movimiento
        // Si el Animator tiene "Apply Root Motion", el personaje se quedará quieto aunque tenga velocidad
        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.applyRootMotion = false;
        }
    }

    private void Update()
    {
        // Obtener input del teclado actual
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        bool upPressed = keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed;
        bool downPressed = keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed;
        bool leftPressed = keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed;
        bool rightPressed = keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed;

        bool upPressedThisFrame = keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame;
        bool downPressedThisFrame = keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame;
        bool leftPressedThisFrame = keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame;
        bool rightPressedThisFrame = keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame;
        bool emotePressedThisFrame = keyboard.cKey.wasPressedThisFrame;
        bool movementAttempted = upPressed || downPressed || leftPressed || rightPressed;
        bool emoteCancelledThisFrame = false;

        if (animationController != null && animationController.IsMovementLockedByEmote() && movementAttempted)
        {
            animationController.CancelEmote();
            emoteCancelledThisFrame = true;
        }

        bool afkBlocksMovement = afkController != null && afkController.IsBlockingMovement();
        if (emotePressedThisFrame && !emoteCancelledThisFrame && !afkBlocksMovement && animationController != null && !animationController.IsMovementLockedByEmote())
        {
            animationController.TryStartEmote();
        }

        if (animationController != null && animationController.IsMovementLockedByEmote())
        {
            moveInput = Vector2.zero;
            forceVerticalJitter = false;
            animationController.SetJitterDirectionLock(false);
            isRunning = false;
            return;
        }

        forceVerticalJitter = upPressed && downPressed;
        
        float horizontalInput = ResolveAxis(rightPressed, leftPressed, rightPressedThisFrame, leftPressedThisFrame, ref lastHorizontalInput);
        float verticalInput = forceVerticalJitter ? 0f : ResolveAxis(upPressed, downPressed, upPressedThisFrame, downPressedThisFrame, ref lastVerticalInput);
        moveInput = new Vector2(horizontalInput, verticalInput);

        if (animationController != null)
        {
            animationController.SetJitterDirectionLock(forceVerticalJitter);
        }
        
        // Shift para correr
        isRunning = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
    }

    private float ResolveAxis(bool positivePressed, bool negativePressed, bool positivePressedThisFrame, bool negativePressedThisFrame, ref int lastAxisDirection)
    {
        if (positivePressedThisFrame)
        {
            lastAxisDirection = 1;
        }

        if (negativePressedThisFrame)
        {
            lastAxisDirection = -1;
        }

        if (positivePressed && negativePressed)
        {
            if (lastAxisDirection == 0)
            {
                lastAxisDirection = positivePressedThisFrame ? 1 : -1;
            }

            return lastAxisDirection;
        }

        if (positivePressed)
        {
            lastAxisDirection = 1;
            return 1f;
        }

        if (negativePressed)
        {
            lastAxisDirection = -1;
            return -1f;
        }

        return 0f;
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        if (animationController != null && animationController.IsMovementLockedByEmote())
        {
            rb.linearVelocity = Vector2.zero;
            animationController.SetJitterDirectionLock(false);
            animationController.UpdateAnimation(Vector2.zero);
            return;
        }
        
        // Bloqueo por AFK (Evita el "patinado")
        if (afkController != null && afkController.IsBlockingMovement())
        {
            rb.linearVelocity = Vector2.zero;
            if (animationController != null)
            {
                animationController.SetJitterDirectionLock(false);
            }
            return;
        }

        // Calcular velocidad
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        if (forceVerticalJitter)
        {
            currentSpeed *= Mathf.Clamp(wsJitterSpeedMultiplier, 0.1f, 1f);
        }
        Vector2 velocity = moveInput.normalized * currentSpeed;
        
        // Aplicar movimiento con velocity (compatible con todas las versiones)
        rb.linearVelocity = velocity;
        
        animationController.UpdateAnimation(velocity);
    }

    // DEBUG: Detectar colisiones invisibles
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.LogWarning($"[COLLISION] Golpeando contra: '{collision.gameObject.name}' en la capa: {LayerMask.LayerToName(collision.gameObject.layer)}");
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Solo avisar si estamos intentando movernos pero estamos bloqueados
        if (moveInput.magnitude > 0.1f)
        {
            // Dibujar una línea roja hacia el objeto con el que chocamos para verlo en la escena
            Debug.DrawLine(transform.position, collision.transform.position, Color.red);
            // No spammear tanto la consola, pero mostrarlo
            if (Time.frameCount % 60 == 0) // Cada ~1 segundo
            {
                 Debug.Log($"[BLOQUEO] Algo impide el paso: '{collision.gameObject.name}'");
            }
        }
    }
}

