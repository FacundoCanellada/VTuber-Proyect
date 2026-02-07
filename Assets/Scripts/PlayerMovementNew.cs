using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controlador de movimiento con WASD usando el NUEVO Input System
/// </summary>
[RequireComponent(typeof(PlayerAnimationController))]
public class PlayerMovementNew : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    
    [Header("Referencias")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private AFKController afkController;
    
    private PlayerAnimationController animationController;
    private Vector2 moveInput;
    private bool isRunning;

    private void Awake()
    {
        animationController = GetComponent<PlayerAnimationController>();
        
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (afkController == null) afkController = GetComponent<AFKController>(); // Intentar autodetectar

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
        
        moveInput = Vector2.zero;
        
        // WASD
        if (keyboard.wKey.isPressed) moveInput.y += 1f;
        if (keyboard.sKey.isPressed) moveInput.y -= 1f;
        if (keyboard.aKey.isPressed) moveInput.x -= 1f;
        if (keyboard.dKey.isPressed) moveInput.x += 1f;
        
        // Shift para correr
        isRunning = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
    }

    private void FixedUpdate()
    {
        if (rb == null) return;
        
        // Bloqueo por AFK (Evita el "patinado")
        if (afkController != null && afkController.IsBlockingMovement())
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Calcular velocidad
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
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

