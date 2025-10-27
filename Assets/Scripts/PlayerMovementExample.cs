using UnityEngine;

/// <summary>
/// Ejemplo de controlador de movimiento que integra el sistema de animaciones
/// Este script muestra cómo usar el PlayerAnimationController
/// </summary>
[RequireComponent(typeof(PlayerAnimationController))]
public class PlayerMovementExample : MonoBehaviour
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
        // Obtener input con WASD directamente
        moveInput = Vector2.zero;
        
        if (Input.GetKey(KeyCode.W)) moveInput.y += 1f;
        if (Input.GetKey(KeyCode.S)) moveInput.y -= 1f;
        if (Input.GetKey(KeyCode.A)) moveInput.x -= 1f;
        if (Input.GetKey(KeyCode.D)) moveInput.x += 1f;
        
        // Detectar si está corriendo (con Shift)
        isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }

    private void FixedUpdate()
    {
        // Calcular velocidad
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        Vector2 velocity = moveInput.normalized * currentSpeed;
        
        // Aplicar movimiento (si usas Rigidbody2D)
        if (rb != null)
        {
            rb.linearVelocity = velocity;
            
            // Actualizar animación con la velocidad del Rigidbody
            animationController.UpdateAnimation(rb.linearVelocity);
        }
        else
        {
            // Si no usas física, mueve con Transform
            transform.Translate(velocity * Time.fixedDeltaTime);
            
            // Actualizar animación con el input
            animationController.UpdateAnimationFromInput(moveInput.normalized, currentSpeed);
        }
    }
}
