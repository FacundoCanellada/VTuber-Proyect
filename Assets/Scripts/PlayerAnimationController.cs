using UnityEngine;

/// <summary>
/// Controlador de animaciones para el personaje con movimiento en 8 direcciones
/// Maneja las transiciones entre Idle, Walk y Run usando Blend Trees
/// </summary>
public class PlayerAnimationController : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Componente Animator del personaje")]
    [SerializeField] private Animator animator;
    
    [Header("Configuración de Movimiento")]
    [Tooltip("Velocidad a partir de la cual se activa la animación de correr")]
    [SerializeField] private float runThreshold = 5f;
    
    [Tooltip("Suavizado de las transiciones de animación (mayor = más suave)")]
    [SerializeField] private float animationSmoothTime = 0.1f;
    
    // IDs de los parámetros del Animator (optimización)
    private int speedParamID;
    private int isRunningParamID;
    private int moveXParamID;
    private int moveYParamID;
    
    // Valores actuales para suavizado
    private float currentSpeed;
    private Vector2 currentDirection;
    private Vector2 smoothVelocity;

    private void Awake()
    {
        // Si no se asignó el Animator, intentar obtenerlo del mismo GameObject
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("No se encontró un componente Animator en " + gameObject.name);
            }
        }
        
        // Cachear los IDs de los parámetros para mejor rendimiento
        speedParamID = Animator.StringToHash("Speed");
        isRunningParamID = Animator.StringToHash("IsRunning");
        moveXParamID = Animator.StringToHash("MoveX");
        moveYParamID = Animator.StringToHash("MoveY");
    }
    
    /// <summary>
    /// Actualiza los parámetros del Animator basándose en la velocidad y dirección del movimiento
    /// </summary>
    /// <param name="velocity">Vector de velocidad del personaje</param>
    public void UpdateAnimation(Vector2 velocity)
    {
        if (animator == null) return;
        
        // Calcular la velocidad (magnitud)
        float targetSpeed = velocity.magnitude;
        
        // Suavizar la velocidad
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime / animationSmoothTime);
        
        // Actualizar parámetro de velocidad
        animator.SetFloat(speedParamID, currentSpeed);
        
        // Determinar si está corriendo
        bool isRunning = currentSpeed > runThreshold;
        animator.SetBool(isRunningParamID, isRunning);
        
        // Si hay movimiento, actualizar la dirección
        if (velocity.magnitude > 0.01f)
        {
            // Normalizar la dirección
            Vector2 targetDirection = velocity.normalized;
            
            // Suavizar la dirección
            currentDirection = Vector2.SmoothDamp(
                currentDirection, 
                targetDirection, 
                ref smoothVelocity, 
                animationSmoothTime
            );
            
            // Actualizar parámetros de dirección
            animator.SetFloat(moveXParamID, currentDirection.x);
            animator.SetFloat(moveYParamID, currentDirection.y);
        }
        // Si no hay movimiento, mantener la última dirección para el Idle
    }
    
    /// <summary>
    /// Actualiza la animación basándose en input directo (sin física)
    /// Útil para sistemas que no usan Rigidbody
    /// </summary>
    /// <param name="inputDirection">Dirección del input (-1 a 1 en X e Y)</param>
    /// <param name="moveSpeed">Velocidad de movimiento actual</param>
    public void UpdateAnimationFromInput(Vector2 inputDirection, float moveSpeed)
    {
        Vector2 velocity = inputDirection * moveSpeed;
        UpdateAnimation(velocity);
    }
    
    /// <summary>
    /// Fuerza la dirección de la animación sin cambiar la velocidad
    /// Útil para cuando el personaje está quieto pero quieres que mire en cierta dirección
    /// </summary>
    public void SetDirection(Vector2 direction)
    {
        if (animator == null) return;
        
        direction = direction.normalized;
        animator.SetFloat(moveXParamID, direction.x);
        animator.SetFloat(moveYParamID, direction.y);
        currentDirection = direction;
    }
    
    /// <summary>
    /// Obtiene la dirección actual de la animación
    /// </summary>
    public Vector2 GetCurrentDirection()
    {
        return currentDirection;
    }
    
    /// <summary>
    /// Reinicia todos los parámetros del Animator a sus valores por defecto
    /// </summary>
    public void ResetAnimation()
    {
        if (animator == null) return;
        
        animator.SetFloat(speedParamID, 0f);
        animator.SetBool(isRunningParamID, false);
        currentSpeed = 0f;
    }

#if UNITY_EDITOR
    // Validación en el Editor
    private void OnValidate()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        if (runThreshold < 0f)
        {
            runThreshold = 0f;
        }
        
        if (animationSmoothTime < 0.01f)
        {
            animationSmoothTime = 0.01f;
        }
    }
#endif
}
