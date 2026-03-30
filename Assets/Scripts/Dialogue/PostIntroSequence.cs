using System.Collections;
using UnityEngine;

/// <summary>
/// Ejecuta la secuencia post-intro:
/// 1. Bloquea el input del jugador.
/// 2. Mueve el personaje hacia el sur (de frente a la cámara) durante unos pasos.
/// 3. Lo detiene mirando al sur.
/// 4. Inicia el diálogo de introducción.
/// 
/// Llamar Begin() al final de IntroController.EndingSequence().
/// </summary>
public class PostIntroSequence : MonoBehaviour
{
    [Header("Referencias del Personaje")]
    [Tooltip("El script de movimiento del jugador (para bloquear el input durante la cinemática).")]
    [SerializeField] private PlayerMovementNew playerMovement;
    [Tooltip("El Rigidbody2D del personaje (para moverlo manualmente durante la cinemática).")]
    [SerializeField] private Rigidbody2D characterRb;
    [Tooltip("El controlador de animaciones del personaje.")]
    [SerializeField] private PlayerAnimationController animationController;

    [Header("Caminata hacia el Sur")]
    [Tooltip("Velocidad a la que el personaje camina hacia el sur durante la cinemática.")]
    [SerializeField] private float walkSpeed = 2.5f;
    [Tooltip("Cuántos segundos camina el personaje hacia el sur antes de detenerse.")]
    [SerializeField] private float walkDuration = 0.9f;
    [Tooltip("Pausa en segundos una vez que el personaje se detiene, antes de que aparezca el cuadro de diálogo.")]
    [SerializeField] private float settleDelay = 0.4f;

    [Header("Diálogo de Intro")]
    [Tooltip("El DialogueData ScriptableObject con el diálogo que se reproduce al terminar la cinemática.")]
    [SerializeField] private DialogueData introDialogueData;

    [Header("Secuencia Telefónica Opcional")]
    [Tooltip("Conversación telefónica que arranca al terminar el cuadro de diálogo principal.")]
    [SerializeField] private PhoneConversationData followUpPhoneConversation;
    [Tooltip("Delay opcional antes de arrancar la conversación telefónica.")]
    [SerializeField] private float followUpPhoneConversationDelay = 0.15f;

    /// <summary>
    /// Bloquea el input del jugador inmediatamente (sin iniciar la cinemática).
    /// Llamar desde IntroController antes de restaurar Time.timeScale para que el
    /// jugador no pueda moverse durante el efecto de cámara.
    /// </summary>
    public void BlockInputImmediate()
    {
        if (playerMovement != null)
            playerMovement.IsInputBlocked = true;
    }

    /// <summary>
    /// Inicia la secuencia post-intro. Llamar desde IntroController.EndingSequence().
    /// </summary>
    public void Begin()
    {
        if (!ValidateReferences()) return;
        StartCoroutine(Sequence());
    }

    private IEnumerator Sequence()
    {
        // 1. Bloquear input del jugador (modo cinemático — física libre para que nosotros la controlemos)
        playerMovement.IsInputBlocked = true;

        // 2. Caminar hacia el sur
        Vector2 southVelocity = Vector2.down * walkSpeed;
        float elapsed = 0f;

        while (elapsed < walkDuration)
        {
            characterRb.linearVelocity = southVelocity;
            animationController.UpdateAnimation(southVelocity);
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // 3. Detener y fijar la dirección sur (idle de frente a la cámara)
        characterRb.linearVelocity = Vector2.zero;
        animationController.ResetAnimation();
        animationController.UpdateAnimation(Vector2.zero);
        animationController.SetDirection(Vector2.down);

        // 4. Breve pausa para que el idle se vea natural
        yield return new WaitForSeconds(settleDelay);

        // 5. Iniciar diálogo. IsInputBlocked se mantiene en true hasta que el diálogo termine.
        //    PlayerMovementNew.FixedUpdate zerarea la velocidad mientras IsInputBlocked+IsDialogueActive estén activos.
        if (introDialogueData != null && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(introDialogueData, OnDialogueComplete);
        }
        else
        {
            Debug.LogWarning("[PostIntroSequence] No hay DialogueData asignado o DialogueManager no está en la escena.");
            // Si no hay diálogo, liberar el input de todas formas para no bloquear al jugador
            playerMovement.IsInputBlocked = false;
        }
    }

    private void OnDialogueComplete()
    {
        if (followUpPhoneConversation != null && DialogueManager.Instance != null)
        {
            StartCoroutine(BeginPhoneConversationAfterDelay());
            return;
        }

        playerMovement.IsInputBlocked = false;
        Debug.Log("[PostIntroSequence] Diálogo de intro finalizado. Control devuelto al jugador.");
    }

    private IEnumerator BeginPhoneConversationAfterDelay()
    {
        if (followUpPhoneConversationDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(followUpPhoneConversationDelay);
        }

        DialogueManager.Instance.StartPhoneConversation(followUpPhoneConversation, OnPhoneConversationComplete);
    }

    private void OnPhoneConversationComplete()
    {
        playerMovement.IsInputBlocked = false;
        Debug.Log("[PostIntroSequence] Conversación telefónica finalizada. Control devuelto al jugador.");
    }

    private bool ValidateReferences()
    {
        if (playerMovement == null)
        {
            Debug.LogError("[PostIntroSequence] Falta la referencia a PlayerMovementNew.");
            return false;
        }
        if (characterRb == null)
        {
            Debug.LogError("[PostIntroSequence] Falta la referencia al Rigidbody2D del personaje.");
            return false;
        }
        if (animationController == null)
        {
            Debug.LogError("[PostIntroSequence] Falta la referencia al PlayerAnimationController.");
            return false;
        }
        return true;
    }
}
