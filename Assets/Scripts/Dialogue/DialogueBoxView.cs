using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Maneja la animación de apertura/cierre del cuadro de diálogo,
/// el efecto typewriter, el portrait, y el skip con Z y X (estilo Undertale).
/// </summary>
public class DialogueBoxView : MonoBehaviour
{
    [Header("Box Animation")]
    [Tooltip("RectTransform del cuadro de diálogo. Se usa para la animación de scale X.")]
    [SerializeField] private RectTransform dialogueBoxRect;
    [Tooltip("CanvasGroup del cuadro. Se usa para la animación de alpha.")]
    [SerializeField] private CanvasGroup dialogueBoxCanvasGroup;
    [Tooltip("Duración de la animación de apertura en segundos.")]
    [SerializeField] private float openDuration = 0.5f;
    [Tooltip("Duración de la animación de cierre relativa a openDuration.")]
    [SerializeField] private float closeDurationMultiplier = 0.65f;

    [Header("Easing")]

    [Header("Portrait")]
    [Tooltip("Imagen donde se renderiza el retrato del personaje.")]
    [SerializeField] private Image portraitImage;
    [Tooltip("Contenedor del portrait (puede tener frame, fondo, etc.).")]
    [SerializeField] private GameObject portraitContainer;

    [Header("Texto")]
    [Tooltip("TMP_Text donde se muestra el nombre del personaje.")]
    [SerializeField] private TMP_Text characterNameText;
    [Tooltip("El componente TypewriterEffect en el texto del diálogo.")]
    [SerializeField] private TypewriterEffect typewriterEffect;

    [Header("Timing de Diálogo")]
    [Tooltip("Velocidad de tipeo en segundos/carácter para este cuadro de diálogo. Sobreescribe el default del TypewriterEffect. " +
             "Usa -1 para respetar el valor del TypewriterEffect o el override por línea del DialogueData.")]
    [SerializeField] private float defaultTypingSpeedOverride = 0.06f;
    [Tooltip("Segundos de pausa ANTES de mostrar cada línea nueva (da tiempo a que el texto anterior se limpie visualmente).")]
    [SerializeField] private float waitBetweenLines = 0.08f;
    [Tooltip("Segundos de espera después de la última línea antes de cerrar el cuadro. " +
             "Permite que el jugador lea el final del texto antes de que desaparezca.")]
    [SerializeField] private float waitAfterLastLine = 0.8f;

    [Header("Audio")]
    [Tooltip("AudioSource para reproducir el voiceClip one-shot de cada línea (opcional).")]
    [SerializeField] private AudioSource voiceLineAudioSource;

    // --- Estado interno ---
    private DialogueLine[] _lines;
    private int _currentLineIndex;
    private bool _isAnimatingOpen;
    private bool _isAnimatingClose;
    private bool _waitingForAdvance;
    private string _currentLineFullText;
    private AudioClip[] _originalVoiceClips;
    private System.Action _onComplete;
    private Vector2 _originalBoxSize;
    private bool _hasCachedOriginalBoxSize;

    private void Awake()
    {
        CacheOriginalBoxSize();
        // Estado inicial garantizado: invisible y escala aplastada
        ResetVisualState();
    }

    private void CacheOriginalBoxSize()
    {
        if (dialogueBoxRect == null || _hasCachedOriginalBoxSize)
        {
            return;
        }

        _originalBoxSize = dialogueBoxRect.sizeDelta;
        _hasCachedOriginalBoxSize = true;
    }

    private void ResetVisualState()
    {
        CacheOriginalBoxSize();

        if (dialogueBoxCanvasGroup != null)
        {
            dialogueBoxCanvasGroup.alpha = 0f;
            dialogueBoxCanvasGroup.blocksRaycasts = false;
            dialogueBoxCanvasGroup.interactable = false;
        }

        if (dialogueBoxRect != null)
        {
            dialogueBoxRect.localScale = Vector3.one;
            dialogueBoxRect.sizeDelta = new Vector2(0f, _originalBoxSize.y);
        }

        if (portraitContainer != null)
            portraitContainer.SetActive(false);
    }

    /// <summary>
    /// Llamado por DialogueManager para iniciar la secuencia de diálogo.
    /// </summary>
    public void StartDialogue(DialogueData data, System.Action onComplete)
    {
        // Detener cualquier coroutine anterior (por si se interrumpe)
        StopAllCoroutines();
        CacheOriginalBoxSize();
        ResetVisualState();

        _lines = data.lines;
        _currentLineIndex = 0;
        _onComplete = onComplete;
        _waitingForAdvance = false;
        _isAnimatingOpen = false;
        _isAnimatingClose = false;

        // Nombre del personaje
        if (characterNameText != null)
            characterNameText.text = data.characterName;

        // Portrait
        if (portraitImage != null)
        {
            portraitImage.sprite = data.portrait;
        }

        // Override de voice clips si el DialogueData trae los suyos
        if (typewriterEffect != null && data.characterVoiceClips != null && data.characterVoiceClips.Length > 0)
        {
            _originalVoiceClips = typewriterEffect.voiceClips;
            typewriterEffect.voiceClips = data.characterVoiceClips;
        }
        else
        {
            _originalVoiceClips = null;
        }

        StartCoroutine(AnimateOpen());
    }

    // ─────────────────────────────────────────────────────────
    // ANIMACIÓN DE APERTURA
    // ─────────────────────────────────────────────────────────

    private IEnumerator AnimateOpen()
    {
        _isAnimatingOpen = true;

        if (portraitContainer != null)
            portraitContainer.SetActive(false);

        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / openDuration);

            // La apertura usa la misma lógica del cierre pero invertida:
            // ancho 0 -> ancho original y alpha 0 -> 1 con EaseOutQuad.
            float ease = EaseOutQuad(t);

            if (dialogueBoxRect != null)
            {
                dialogueBoxRect.sizeDelta = new Vector2(Mathf.Lerp(0f, _originalBoxSize.x, ease), _originalBoxSize.y);
            }

            if (dialogueBoxCanvasGroup != null)
                dialogueBoxCanvasGroup.alpha = ease;

            yield return null;
        }

        // Garantizar estado final
        if (dialogueBoxRect != null)
        {
            dialogueBoxRect.localScale = Vector3.one;
            dialogueBoxRect.sizeDelta = _originalBoxSize;
        }

        if (dialogueBoxCanvasGroup != null)
        {
            dialogueBoxCanvasGroup.alpha = 1f;
            dialogueBoxCanvasGroup.blocksRaycasts = true;
            dialogueBoxCanvasGroup.interactable = true;
        }

        if (portraitContainer != null)
            portraitContainer.SetActive(true);

        _isAnimatingOpen = false;

        // Comenzar primera línea
        ShowCurrentLine();
    }

    // ─────────────────────────────────────────────────────────
    // REPRODUCCIÓN DE LÍNEAS
    // ─────────────────────────────────────────────────────────

    private void ShowCurrentLine()
    {
        // Guardia de seguridad: normalmente el avance de líneas va por AdvanceLine().
        if (_currentLineIndex >= _lines.Length)
        {
            StartCoroutine(AnimateClose());
            return;
        }

        DialogueLine line = _lines[_currentLineIndex];
        _currentLineFullText = line.text;
        _waitingForAdvance = false;

        // Aplicar modificador de LC
        if (line.lcModifier != 0f && TrustManager.Instance != null)
            TrustManager.Instance.ModifyLC(line.lcModifier);

        // Reproducir voiceClip one-shot si tiene (actriz de voz, etc.)
        if (line.voiceClip != null && voiceLineAudioSource != null)
        {
            voiceLineAudioSource.PlayOneShot(line.voiceClip);
        }

        // Determinar overrides para el typewriter.
        // Prioridad: per-línea > defaultTypingSpeedOverride del DialogueBoxView > TypewriterEffect default
        float typingSpeed = line.typingSpeed > 0f
            ? line.typingSpeed
            : (defaultTypingSpeedOverride >= 0f ? defaultTypingSpeedOverride : -1f);
        int charsPerSound = line.charsPerSound > 0 ? line.charsPerSound : -1;

        if (typewriterEffect != null)
        {
            typewriterEffect.ShowText(
                line.text,
                -1f,          // volume: usa el default del TypewriterEffect
                charsPerSound,
                typingSpeed,
                () => OnLineTypingComplete(line)
            );
        }
    }

    private void OnLineTypingComplete(DialogueLine line)
    {
        if (line.autoAdvance)
        {
            StartCoroutine(AutoAdvanceAfter(Mathf.Max(0f, line.pauseAfter)));
        }
        else
        {
            _waitingForAdvance = true;
        }
    }

    private IEnumerator AutoAdvanceAfter(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        AdvanceLine();
    }

    // immediate = true: usado por X para saltar sin delays (cierra instantáneamente si es la última)
    private void AdvanceLine(bool immediate = false)
    {
        StopAllCoroutines();
        _waitingForAdvance = false;
        _currentLineIndex++;
        StartCoroutine(AdvanceWithDelay(immediate));
    }

    private IEnumerator AdvanceWithDelay(bool immediate = false)
    {
        bool isEnd = _currentLineIndex >= _lines.Length;

        if (isEnd)
        {
            // Última línea: mantener el texto visible durante waitAfterLastLine, luego cerrar.
            if (!immediate && waitAfterLastLine > 0f)
                yield return new WaitForSecondsRealtime(waitAfterLastLine);

            if (typewriterEffect != null)
                typewriterEffect.ClearText();

            yield return StartCoroutine(AnimateClose());
        }
        else
        {
            // Línea intermedia: limpiar, esperar y mostrar la siguiente.
            if (typewriterEffect != null)
                typewriterEffect.ClearText();

            if (!immediate && waitBetweenLines > 0f)
                yield return new WaitForSecondsRealtime(waitBetweenLines);

            ShowCurrentLine();
        }
    }

    /// <summary>
    /// Muestra el texto completo de la línea actual de golpe (sin typewriter).
    /// </summary>
    private void CompleteCurrentLineImmediate()
    {
        if (typewriterEffect != null)
            typewriterEffect.CompleteImmediate(_currentLineFullText);

        _waitingForAdvance = true;
    }

    // ─────────────────────────────────────────────────────────
    // INPUT (Z y X — estilo Undertale)
    // ─────────────────────────────────────────────────────────

    private void Update()
    {
        // No procesar input mientras el box está animándose
        if (_isAnimatingOpen || _isAnimatingClose) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        bool zPressed = kb.zKey.wasPressedThisFrame;
        bool xPressed = kb.xKey.wasPressedThisFrame;

        if (!zPressed && !xPressed) return;

        if (typewriterEffect != null && typewriterEffect.IsTyping)
        {
            if (zPressed)
            {
                // Z mientras tipea → completar la línea instantáneamente
                CompleteCurrentLineImmediate();
            }
            else
            {
                // X mientras tipea → saltar directamente (sin delays, cierra instantáneo si es la última línea)
                typewriterEffect.StopTyping();
                AdvanceLine(immediate: true);
            }
        }
        else if (_waitingForAdvance)
        {
            // Z o X cuando terminó de tipear → avanzar a la siguiente línea
            AdvanceLine();
        }
    }

    // ─────────────────────────────────────────────────────────
    // ANIMACIÓN DE CIERRE
    // ─────────────────────────────────────────────────────────

    private IEnumerator AnimateClose()
    {
        _isAnimatingClose = true;
        _waitingForAdvance = false;

        if (portraitContainer != null)
            portraitContainer.SetActive(false);

        if (dialogueBoxCanvasGroup != null)
        {
            dialogueBoxCanvasGroup.blocksRaycasts = false;
            dialogueBoxCanvasGroup.interactable = false;
        }

        float closeDuration = openDuration * closeDurationMultiplier;
        float elapsed = 0f;

        while (elapsed < closeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / closeDuration);

            // EaseInQuad: empieza suave y acelera hacia el cierre. Decisivo y limpio.
            float ease = EaseInQuad(t);

            if (dialogueBoxRect != null)
                dialogueBoxRect.sizeDelta = new Vector2(Mathf.Lerp(_originalBoxSize.x, 0f, ease), _originalBoxSize.y);

            if (dialogueBoxCanvasGroup != null)
                dialogueBoxCanvasGroup.alpha = Mathf.Lerp(1f, 0f, ease);

            yield return null;
        }

        // Limpiar y restaurar
        if (portraitContainer != null)
            portraitContainer.SetActive(false);

        if (typewriterEffect != null)
            typewriterEffect.ClearText();

        // Restaurar voice clips originales del TypewriterEffect
        if (_originalVoiceClips != null && typewriterEffect != null)
        {
            typewriterEffect.voiceClips = _originalVoiceClips;
            _originalVoiceClips = null;
        }

        _isAnimatingClose = false;

        // Notificar al DialogueManager que terminó
        _onComplete?.Invoke();
    }

    // ─────────────────────────────────────────────────────────
    // EASING MATEMÁTICO
    // ─────────────────────────────────────────────────────────

    /// <summary>
    /// Ease Out Quad: desaceleración suave, sin overshoot. Ideal para alpha.
    /// </summary>
    private static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);

    /// <summary>
    /// Ease In Quad: aceleración suave desde 0. Cierre decisivo y limpio.
    /// </summary>
    private static float EaseInQuad(float t) => t * t;
}
