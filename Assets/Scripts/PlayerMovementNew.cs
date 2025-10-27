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
    
    private PlayerAnimationController animationController;
    private Vector2 moveInput;
    private bool isRunning;

    private void Awake()
    {
        animationController = GetComponent<PlayerAnimationController>();
        
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
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
        // Calcular velocidad
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        Vector2 velocity = moveInput.normalized * currentSpeed;
        
        // Aplicar movimiento
        if (rb != null)
        {
            rb.linearVelocity = velocity;
            animationController.UpdateAnimation(rb.linearVelocity);
        }
        else
        {
            transform.Translate(velocity * Time.fixedDeltaTime);
            animationController.UpdateAnimationFromInput(moveInput.normalized, currentSpeed);
        }
    }
}
