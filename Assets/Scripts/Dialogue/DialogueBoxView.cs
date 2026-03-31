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
    private DialoguePlaybackCallbacks _playbackCallbacks;
    private DialoguePortraitSequence _defaultPortraitSequence;
    private DialogueLine _currentLineData;
    private DialoguePortraitSequence _currentPortraitSequence;
    private bool _lineCompletionHandled;
    private bool _skipFinalCloseDelayForPendingAdvance;
    private Coroutine _portraitRoutine;

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
        StartDialogue(data, DialoguePlaybackCallbacks.None, onComplete);
    }

    public void StartDialogue(DialogueData data, DialoguePlaybackCallbacks playbackCallbacks, System.Action onComplete)
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
        _lineCompletionHandled = false;
        _skipFinalCloseDelayForPendingAdvance = false;
        _playbackCallbacks = playbackCallbacks;
        _defaultPortraitSequence = ResolvePortraitSequence(data.defaultPortraitSequenceAsset, data.defaultPortraitSequence);

        // Nombre del personaje
        if (characterNameText != null)
            characterNameText.text = data.characterName;

        // Portrait
        if (portraitImage != null)
        {
            portraitImage.sprite = GetIdlePortraitSprite(_defaultPortraitSequence, data.portrait);
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
        _currentLineData = line;
        _currentPortraitSequence = ResolvePortraitSequence(line);
        _currentLineFullText = line.text;
        _waitingForAdvance = false;
        _lineCompletionHandled = false;

        ApplyPortraitSprite(GetIdlePortraitSprite(_currentPortraitSequence, GetIdlePortraitSprite(_defaultPortraitSequence, portraitImage != null ? portraitImage.sprite : null)));

        StopPortraitRoutine();
        _playbackCallbacks.NotifyLineStarted(line, _currentLineIndex, _lines.Length);

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
                () => OnLineTypingComplete(line, _currentPortraitSequence)
            );

            if (HasPortraitFrames(_currentPortraitSequence.typingFrames))
            {
                _portraitRoutine = StartCoroutine(BeginTypingPortraitLoopWhenTypingStarts(_currentPortraitSequence));
            }
        }
    }

    private void OnLineTypingComplete(DialogueLine line, DialoguePortraitSequence portraitSequence)
    {
        if (_lineCompletionHandled)
        {
            return;
        }

        _lineCompletionHandled = true;
        StopPortraitRoutine();
        StartCoroutine(HandleLineCompleted(line, portraitSequence));
    }

    private IEnumerator HandleLineCompleted(DialogueLine line, DialoguePortraitSequence portraitSequence)
    {
        if (HasPortraitFrames(portraitSequence.postTypingFrames))
        {
            yield return StartCoroutine(PlayPortraitFramesOnce(portraitSequence.postTypingFrames));
        }
        else
        {
            ApplyPortraitSprite(GetIdlePortraitSprite(portraitSequence, GetIdlePortraitSprite(_defaultPortraitSequence, portraitImage != null ? portraitImage.sprite : null)));
        }

        _playbackCallbacks.NotifyLineCompleted(line, _currentLineIndex, _lines.Length);

        if (line.autoAdvance)
        {
            _skipFinalCloseDelayForPendingAdvance = line.skipFinalCloseDelay;
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
        StopPortraitRoutine();
        _waitingForAdvance = false;
        _skipFinalCloseDelayForPendingAdvance = _currentLineData.skipFinalCloseDelay;
        _currentLineIndex++;
        StartCoroutine(AdvanceWithDelay(immediate));
    }

    private IEnumerator AdvanceWithDelay(bool immediate = false)
    {
        bool isEnd = _currentLineIndex >= _lines.Length;

        if (isEnd)
        {
            // Última línea: mantener el texto visible durante waitAfterLastLine, luego cerrar.
            if (!immediate && !_skipFinalCloseDelayForPendingAdvance && waitAfterLastLine > 0f)
                yield return new WaitForSecondsRealtime(waitAfterLastLine);

            if (typewriterEffect != null)
                typewriterEffect.ClearText();

            _skipFinalCloseDelayForPendingAdvance = false;

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

        OnLineTypingComplete(_currentLineData, _currentPortraitSequence);
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

        StopPortraitRoutine();

        if (typewriterEffect != null)
            typewriterEffect.ClearText();

        // Restaurar voice clips originales del TypewriterEffect
        if (_originalVoiceClips != null && typewriterEffect != null)
        {
            typewriterEffect.voiceClips = _originalVoiceClips;
            _originalVoiceClips = null;
        }

        _isAnimatingClose = false;
        _playbackCallbacks = DialoguePlaybackCallbacks.None;

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

    private DialoguePortraitSequence ResolvePortraitSequence(DialogueLine line)
    {
        if (line.portraitSequenceAsset != null)
        {
            return line.portraitSequenceAsset.sequence;
        }

        return line.usePortraitSequenceOverride ? line.portraitSequenceOverride : _defaultPortraitSequence;
    }

    private static DialoguePortraitSequence ResolvePortraitSequence(DialoguePortraitSequenceAsset asset, DialoguePortraitSequence fallback)
    {
        return asset != null ? asset.sequence : fallback;
    }

    private static bool HasPortraitFrames(DialoguePortraitFrame[] frames)
    {
        return frames != null && frames.Length > 0;
    }

    private Sprite GetIdlePortraitSprite(DialoguePortraitSequence sequence, Sprite fallback)
    {
        return sequence.idleSprite != null ? sequence.idleSprite : fallback;
    }

    private void ApplyPortraitSprite(Sprite sprite)
    {
        if (portraitImage != null && sprite != null)
        {
            portraitImage.sprite = sprite;
        }
    }

    private void StopPortraitRoutine()
    {
        if (_portraitRoutine != null)
        {
            StopCoroutine(_portraitRoutine);
            _portraitRoutine = null;
        }
    }

    private IEnumerator PlayTypingPortraitLoop(DialoguePortraitSequence sequence)
    {
        if (!HasPortraitFrames(sequence.typingFrames))
        {
            yield break;
        }

        while (typewriterEffect != null && typewriterEffect.IsTyping)
        {
            foreach (DialoguePortraitFrame frame in sequence.typingFrames)
            {
                if (typewriterEffect == null || !typewriterEffect.IsTyping)
                {
                    yield break;
                }

                yield return StartCoroutine(PlayPortraitFrame(frame, stopWhenTypingEnds: true));
            }
        }
    }

    private IEnumerator BeginTypingPortraitLoopWhenTypingStarts(DialoguePortraitSequence sequence)
    {
        while (typewriterEffect != null && !typewriterEffect.IsTyping)
        {
            yield return null;
        }

        if (typewriterEffect == null || !typewriterEffect.IsTyping)
        {
            yield break;
        }

        yield return StartCoroutine(PlayTypingPortraitLoop(sequence));
    }

    private IEnumerator PlayPortraitFramesOnce(DialoguePortraitFrame[] frames)
    {
        if (!HasPortraitFrames(frames))
        {
            yield break;
        }

        int maxRepeats = 1;
        foreach (DialoguePortraitFrame frame in frames)
        {
            maxRepeats = Mathf.Max(maxRepeats, Mathf.Max(1, frame.repeatCount));
        }

        for (int repeatIndex = 0; repeatIndex < maxRepeats; repeatIndex++)
        {
            foreach (DialoguePortraitFrame frame in frames)
            {
                int frameRepeats = Mathf.Max(1, frame.repeatCount);
                if (repeatIndex >= frameRepeats)
                {
                    continue;
                }

                ApplyPortraitSprite(frame.sprite);
                yield return new WaitForSecondsRealtime(Mathf.Max(0.01f, frame.duration));
            }
        }
    }

    private IEnumerator PlayPortraitFrame(DialoguePortraitFrame frame, bool stopWhenTypingEnds)
    {
        int repeatCount = Mathf.Max(1, frame.repeatCount);
        float duration = Mathf.Max(0.01f, frame.duration);

        for (int repeat = 0; repeat < repeatCount; repeat++)
        {
            if (stopWhenTypingEnds && (typewriterEffect == null || !typewriterEffect.IsTyping))
            {
                yield break;
            }

            ApplyPortraitSprite(frame.sprite);
            yield return new WaitForSecondsRealtime(duration);
        }
    }
}
