using UnityEngine;

/// <summary>
/// Singleton que coordina el sistema de diálogos.
/// Controla el flag IsDialogueActive que PlayerMovementNew respeta para bloquear el input.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("Referencias UI")]
    [Tooltip("El GameObject raíz del Canvas de diálogo. Se activa/desactiva automáticamente.")]
    [SerializeField] private GameObject dialogueCanvas;
    [Tooltip("El componente que maneja la animación del box y el typewriter.")]
    [SerializeField] private DialogueBoxView dialogueBoxView;
    [Tooltip("Vista de conversación telefónica/flotante. Opcional.")]
    [SerializeField] private PhoneConversationView phoneConversationView;

    /// <summary>
    /// True mientras hay un diálogo activo. PlayerMovementNew lo consulta para bloquear input y física.
    /// </summary>
    public bool IsDialogueActive { get; private set; }

    private System.Action _onDialogueComplete;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Empezar oculto
        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// Inicia un diálogo. Bloquea el input del jugador automáticamente hasta que termine.
    /// </summary>
    /// <param name="data">El ScriptableObject con el diálogo a reproducir.</param>
    /// <param name="onComplete">Callback opcional al terminar el diálogo.</param>
    public void StartDialogue(DialogueData data, System.Action onComplete = null)
    {
        StartDialogue(data, DialoguePlaybackCallbacks.None, onComplete);
    }

    public void StartDialogue(DialogueData data, DialoguePlaybackCallbacks playbackCallbacks, System.Action onComplete = null)
    {
        if (data == null || data.lines == null || data.lines.Length == 0)
        {
            Debug.LogWarning("[DialogueManager] El DialogueData es null o no tiene líneas.");
            return;
        }

        if (dialogueBoxView == null)
        {
            Debug.LogError("[DialogueManager] No se asignó el DialogueBoxView en el Inspector.");
            return;
        }

        IsDialogueActive = true;
        _onDialogueComplete = onComplete;

        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(true);

        dialogueBoxView.StartDialogue(data, playbackCallbacks, HandleDialogueComplete);
    }

    public void StartPhoneConversation(PhoneConversationData data, System.Action onComplete = null)
    {
        StartPhoneConversation(data, false, onComplete);
    }

    public void StartPhoneConversation(PhoneConversationData data, bool skipIncomingCue, System.Action onComplete = null)
    {
        if (data == null || data.lines == null || data.lines.Length == 0)
        {
            Debug.LogWarning("[DialogueManager] El PhoneConversationData es null o no tiene líneas.");
            return;
        }

        if (!EnsurePhoneConversationView())
        {
            Debug.LogError("[DialogueManager] No se asignó el PhoneConversationView en el Inspector.");
            return;
        }

        IsDialogueActive = true;
        _onDialogueComplete = onComplete;

        // Keep the canvas active — phone UI may live inside it.
        // Only hide the dialogue box view so it doesn't overlap.
        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(true);

        if (dialogueBoxView != null)
            dialogueBoxView.gameObject.SetActive(false);

        phoneConversationView.StartConversation(data, skipIncomingCue, HandleDialogueComplete);
    }

    public bool PlayPhoneIncomingCue(PhoneConversationData data)
    {
        if (data == null)
        {
            Debug.LogWarning("[DialogueManager] No se puede reproducir la llamada entrante porque el PhoneConversationData es null.");
            return false;
        }

        if (!EnsurePhoneConversationView())
        {
            Debug.LogError("[DialogueManager] No se pudo preparar el PhoneConversationView para la llamada entrante.");
            return false;
        }

        return phoneConversationView.PlayIncomingCue(data);
    }

    private void HandleDialogueComplete()
    {
        IsDialogueActive = false;

        // Re-enable the dialogue box for future use (it stays visually hidden via its own CanvasGroup alpha).
        if (dialogueBoxView != null)
            dialogueBoxView.gameObject.SetActive(true);

        System.Action callback = _onDialogueComplete;
        _onDialogueComplete = null;
        callback?.Invoke();

        // Only hide the canvas if the callback didn't start a new dialogue/phone conversation.
        if (!IsDialogueActive && dialogueCanvas != null)
            dialogueCanvas.SetActive(false);
    }

    private bool EnsurePhoneConversationView()
    {
        if (phoneConversationView != null)
        {
            return true;
        }

        phoneConversationView = FindFirstObjectByType<PhoneConversationView>();
        if (phoneConversationView != null)
        {
            return true;
        }

        GameObject runtimePhoneView = new GameObject("PhoneConversationRuntimeView");
        phoneConversationView = runtimePhoneView.AddComponent<PhoneConversationView>();
        return phoneConversationView != null;
    }
}
