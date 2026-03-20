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
    [Tooltip("Curva de animación de apertura. Recomendado: ease-out (sube rápido al principio y frena al final).")]
    [SerializeField] private AnimationCurve openCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Tooltip("Duración de la animación de apertura en segundos.")]
    [SerializeField] private float openDuration = 0.35f;
    [Tooltip("Duración de la animación de cierre (relativa a openDuration).")]
    [SerializeField] private float closeDurationMultiplier = 0.65f;

    [Header("Portrait")]
    [Tooltip("Imagen donde se renderiza el retrato del personaje.")]
    [SerializeField] private Image portraitImage;
    [Tooltip("Contenedor del portrait (puede tener frame, fondo, etc.). Se activa con delay.")]
    [SerializeField] private GameObject portraitContainer;
    [Tooltip("Segundos desde el inicio de la animación de apertura hasta que aparece el portrait.")]
    [SerializeField] private float portraitDelay = 0.18f;

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

    private void Awake()
    {
        // Estado inicial garantizado: invisible y escala aplastada
        ResetVisualState();
    }

    private void ResetVisualState()
    {
        if (dialogueBoxCanvasGroup != null)
        {
            dialogueBoxCanvasGroup.alpha = 0f;
            dialogueBoxCanvasGroup.blocksRaycasts = false;
            dialogueBoxCanvasGroup.interactable = false;
        }

        if (dialogueBoxRect != null)
        {
            Vector3 s = dialogueBoxRect.localScale;
            dialogueBoxRect.localScale = new Vector3(0.01f, s.y, s.z);
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
        bool portraitShown = false;

        while (elapsed < openDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / openDuration);
            float curveValue = openCurve.Evaluate(t);

            // Escala X: de 0.01 a 1.0
            if (dialogueBoxRect != null)
            {
                Vector3 s = dialogueBoxRect.localScale;
                dialogueBoxRect.localScale = new Vector3(Mathf.Lerp(0.01f, 1f, curveValue), s.y, s.z);
            }

            // Alpha: de 0 a 1
            if (dialogueBoxCanvasGroup != null)
                dialogueBoxCanvasGroup.alpha = curveValue;

            // Portrait aparece con delay
            if (!portraitShown && elapsed >= portraitDelay)
            {
                portraitShown = true;
                if (portraitContainer != null)
                    portraitContainer.SetActive(true);
            }

            yield return null;
        }

        // Garantizar estado final
        if (dialogueBoxRect != null)
        {
            Vector3 s = dialogueBoxRect.localScale;
            dialogueBoxRect.localScale = new Vector3(1f, s.y, s.z);
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

        if (dialogueBoxCanvasGroup != null)
        {
            dialogueBoxCanvasGroup.blocksRaycasts = false;
            dialogueBoxCanvasGroup.interactable = false;
        }

        float closeDuration = openDuration * closeDurationMultiplier;
        float elapsed = 0f;

        float startScaleX = dialogueBoxRect != null ? dialogueBoxRect.localScale.x : 1f;

        while (elapsed < closeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / closeDuration);
            float curveValue = openCurve.Evaluate(t);

            if (dialogueBoxRect != null)
            {
                Vector3 s = dialogueBoxRect.localScale;
                dialogueBoxRect.localScale = new Vector3(Mathf.Lerp(startScaleX, 0.01f, curveValue), s.y, s.z);
            }

            if (dialogueBoxCanvasGroup != null)
                dialogueBoxCanvasGroup.alpha = Mathf.Lerp(1f, 0f, curveValue);

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
}
