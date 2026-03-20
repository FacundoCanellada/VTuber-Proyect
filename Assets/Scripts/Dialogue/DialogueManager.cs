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

        dialogueBoxView.StartDialogue(data, HandleDialogueComplete);
    }

    private void HandleDialogueComplete()
    {
        IsDialogueActive = false;

        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(false);

        System.Action callback = _onDialogueComplete;
        _onDialogueComplete = null;
        callback?.Invoke();
    }
}
