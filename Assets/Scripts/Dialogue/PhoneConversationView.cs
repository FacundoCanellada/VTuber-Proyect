using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Vista de conversación telefónica. Solo muestra/oculta los elementos UI.
/// El usuario posiciona el PhonePanel y WarningIcon donde quiera en el Canvas desde el Editor.
/// El código NUNCA modifica posiciones, anchors ni pivots.
/// </summary>
public class PhoneConversationView : MonoBehaviour
{
    [Header("Phone UI — Asignar desde el Inspector")]
    [SerializeField] private GameObject phonePanel;
    [SerializeField] private CanvasGroup phonePanelCanvasGroup;
    [SerializeField] private TypewriterEffect phoneTypewriterEffect;
    [SerializeField] private TMP_Text phoneBodyText;
    [SerializeField] private TMP_Text characterNameText;

    [Header("Warning Icon — Asignar desde el Inspector")]
    [SerializeField] private GameObject warningIconObject;
    [SerializeField] private CanvasGroup warningIconCanvasGroup;
    [SerializeField] private Image warningIconImage;
    [SerializeField] private TMP_Text warningFallbackText;

    [Header("Audio")]
    [SerializeField] private AudioSource cueAudioSource;
    [SerializeField] private AudioSource lineVoiceAudioSource;

    [Header("Animation")]
    [SerializeField] private PlayerAnimationController characterAnimationController;

    [Header("Timing")]
    [SerializeField] private float waitBetweenLines = 0.06f;
    [SerializeField] private float finalPause = 0.55f;
    [SerializeField] private float defaultAutoAdvanceDelay = 1.2f;

    [Header("Animaciones UI")]
    [Tooltip("Duración de la animación de entrada del texto (sube desde abajo + fade in).")]
    [SerializeField] private float lineAnimInDuration = 0.25f;
    [Tooltip("Distancia en Y que recorre el texto al entrar (sube desde abajo).")]
    [SerializeField] private float lineSlideDistance = 30f;
    [Tooltip("Duración del fade out al salir una línea.")]
    [SerializeField] private float lineAnimOutDuration = 0.15f;
    [Tooltip("Duración del pop-in inicial del warning.")]
    [SerializeField] private float warningPopDuration = 0.15f;
    [Tooltip("Tiempo que el warning queda visible antes de empezar a subir.")]
    [SerializeField] private float warningHoldDuration = 0.4f;
    [Tooltip("Duración de la fase de elevación + fade out.")]
    [SerializeField] private float warningRiseDuration = 0.6f;
    [Tooltip("Distancia en Y que sube el warning antes de desaparecer.")]
    [SerializeField] private float warningRiseDistance = 80f;
    [Tooltip("Escala pico al hacer pop.")]
    [SerializeField] private float warningPopScale = 1.3f;

    [Header("Estilo de Texto")]
    [SerializeField] private int runtimeFontSize = 30;
    [SerializeField] private Color runtimeTextColor = Color.black;
    [SerializeField] private Color runtimeTextOutlineColor = Color.white;
    [SerializeField] [Range(0f, 1f)] private float runtimeTextOutlineWidth = 0.3f;

    private PhoneConversationData _conversationData;
    private PhoneConversationLine[] _lines;
    private int _currentLineIndex;
    private bool _waitingForAdvance;
    private bool _lineCompletionHandled;
    private string _currentLineText;
    private System.Action _onComplete;
    private AudioClip[] _originalPhoneVoiceClips;

    private RectTransform _phonePanelRect;
    private Vector2 _phonePanelOriginalPos;
    private bool _phonePanelPosCached;

    private RectTransform _warningIconRect;
    private Vector2 _warningIconOriginalPos;
    private Vector3 _warningIconOriginalScale;
    private bool _warningPosCached;
    private Coroutine _warningAnimCoroutine;
    private Coroutine _cueCoroutine;

    public bool IsConversationActive { get; private set; }

    // ───────────────────────────── LIFECYCLE ─────────────────────────────

    private void Awake()
    {
        AutoWireReferences();
        HideAllInstant();
    }

    private void Update()
    {
        if (!IsConversationActive)
            return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        bool zPressed = keyboard.zKey.wasPressedThisFrame;
        bool xPressed = keyboard.xKey.wasPressedThisFrame;

        if (!zPressed && !xPressed)
            return;

        if (phoneTypewriterEffect != null && phoneTypewriterEffect.IsTyping)
        {
            if (zPressed)
            {
                phoneTypewriterEffect.CompleteImmediate(_currentLineText);
                OnLineTypingComplete(_lines[_currentLineIndex]);
            }
            else
            {
                phoneTypewriterEffect.StopTyping();
                AdvanceLine(immediate: true);
            }
            return;
        }

        if (_waitingForAdvance)
            AdvanceLine();
    }

    // ───────────────────────────── PUBLIC API ─────────────────────────────

    public bool PlayIncomingCue(PhoneConversationData data)
    {
        if (data == null || !HasIncomingCue(data.openingSequence))
            return false;

        AutoWireReferences();

        StopAllCoroutines();
        _warningAnimCoroutine = null;
        _cueCoroutine = null;
        StopTypingAndClearText();

        _conversationData = data;
        IsConversationActive = false;

        SetCharacterName(data.characterName);

        _cueCoroutine = StartCoroutine(PlayIncomingCueRoutine(data.openingSequence, keepWarningVisibleAfterCue: true));
        return true;
    }

    public void StartConversation(PhoneConversationData data, System.Action onComplete)
    {
        StartConversation(data, false, onComplete);
    }

    public void StartConversation(PhoneConversationData data, bool skipIncomingCue, System.Action onComplete)
    {
        AutoWireReferences();

        // Parar la rutina del cue sin matar la animación del warning (que se auto-oculta sola).
        if (_cueCoroutine != null)
        {
            StopCoroutine(_cueCoroutine);
            _cueCoroutine = null;
        }

        StopTypingAndClearText();

        _conversationData = data;
        _lines = data.lines;
        _currentLineIndex = 0;
        _waitingForAdvance = false;
        _lineCompletionHandled = false;
        _currentLineText = string.Empty;
        _onComplete = onComplete;
        IsConversationActive = true;

        SetCharacterName(data.characterName);

        if (phoneTypewriterEffect != null && data.phoneVoiceClips != null && data.phoneVoiceClips.Length > 0)
        {
            _originalPhoneVoiceClips = phoneTypewriterEffect.voiceClips;
            phoneTypewriterEffect.voiceClips = data.phoneVoiceClips;
        }
        else
        {
            _originalPhoneVoiceClips = null;
        }

        ShowPhonePanel();

        ApplyPhoneTextStyle();

        StartCoroutine(BeginConversationRoutine(skipIncomingCue));
    }

    // ───────────────────────── CONVERSATION FLOW ──────────────────────────

    private IEnumerator BeginConversationRoutine(bool skipIncomingCue)
    {
        if (_conversationData != null && _conversationData.openingSequence.enabled)
            yield return StartCoroutine(PlayOpeningSequence(_conversationData.openingSequence, skipIncomingCue));

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        if (_currentLineIndex >= _lines.Length)
        {
            bool skipFinalPause = _lines.Length > 0 && _lines[_lines.Length - 1].skipFinalPause;
            StartCoroutine(FinishConversation(skipFinalPause, immediate: false));
            return;
        }

        StartCoroutine(BeginLineRoutine(_lines[_currentLineIndex]));
    }

    private IEnumerator BeginLineRoutine(PhoneConversationLine line)
    {
        _lineCompletionHandled = false;
        _waitingForAdvance = false;
        _currentLineText = line.text;

        if (phoneTypewriterEffect != null)
            phoneTypewriterEffect.ClearText();

        yield return StartCoroutine(PlayCue(line.cue));

        // Animación de entrada: slide down + fade in.
        yield return StartCoroutine(AnimateLineIn());

        if (line.voiceClip != null && lineVoiceAudioSource != null)
            lineVoiceAudioSource.PlayOneShot(line.voiceClip);

        if (phoneTypewriterEffect != null)
        {
            phoneTypewriterEffect.ShowText(
                line.text,
                -1f,
                line.charsPerSound > 0 ? line.charsPerSound : -1,
                line.typingSpeed > 0f ? line.typingSpeed : -1f,
                () => OnLineTypingComplete(line)
            );
        }
        else
        {
            OnLineTypingComplete(line);
        }
    }

    private void OnLineTypingComplete(PhoneConversationLine line)
    {
        if (_lineCompletionHandled)
            return;

        _lineCompletionHandled = true;

        if (!line.autoAdvance)
        {
            _waitingForAdvance = true;
            return;
        }

        float delay = line.pauseAfter > 0f ? line.pauseAfter : defaultAutoAdvanceDelay;
        StartCoroutine(AutoAdvanceAfter(delay));
    }

    private IEnumerator AutoAdvanceAfter(float delay)
    {
        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        AdvanceLine();
    }

    private void AdvanceLine(bool immediate = false)
    {
        StopAllCoroutines();
        _warningAnimCoroutine = null;
        _cueCoroutine = null;
        HideWarningIcon();
        _waitingForAdvance = false;
        _currentLineIndex++;

        if (_currentLineIndex >= _lines.Length)
        {
            bool skipFinalPause = _lines.Length > 0 && _lines[_lines.Length - 1].skipFinalPause;
            StartCoroutine(FinishConversation(skipFinalPause, immediate));
            return;
        }

        StartCoroutine(AdvanceWithDelay(immediate));
    }

    private IEnumerator AdvanceWithDelay(bool immediate)
    {
        // Fade out de la línea actual.
        if (!immediate)
            yield return StartCoroutine(AnimateLineOut());

        if (phoneTypewriterEffect != null)
            phoneTypewriterEffect.ClearText();

        if (!immediate && waitBetweenLines > 0f)
            yield return new WaitForSecondsRealtime(waitBetweenLines);

        ShowCurrentLine();
    }

    private IEnumerator FinishConversation(bool skipFinalPause, bool immediate)
    {
        if (!immediate && !skipFinalPause && finalPause > 0f)
            yield return new WaitForSecondsRealtime(finalPause);

        // Fade out de la última línea.
        if (!immediate)
            yield return StartCoroutine(AnimateLineOut());

        if (phoneTypewriterEffect != null)
            phoneTypewriterEffect.ClearText();

        if (_originalPhoneVoiceClips != null && phoneTypewriterEffect != null)
        {
            phoneTypewriterEffect.voiceClips = _originalPhoneVoiceClips;
            _originalPhoneVoiceClips = null;
        }

        ResetCharacterAnimation();

        System.Action callback = _onComplete;
        _onComplete = null;

        HideAllInstant();
        IsConversationActive = false;
        callback?.Invoke();
    }

    // ──────────────────────── OPENING SEQUENCE ────────────────────────────

    private IEnumerator PlayOpeningSequence(PhoneConversationOpeningSequence seq, bool skipIncomingCue)
    {
        if (!skipIncomingCue && HasIncomingCue(seq))
            yield return StartCoroutine(PlayIncomingCueRoutine(seq, keepWarningVisibleAfterCue: true));

        PlayAnswerAnimation(seq);

        if (seq.answerClipDelay > 0f)
            yield return new WaitForSecondsRealtime(seq.answerClipDelay);

        if (seq.answerClip != null && cueAudioSource != null)
            cueAudioSource.PlayOneShot(seq.answerClip);

        if (seq.hideWarningIconWhenAnswered)
            HideWarningIcon();

        if (seq.dialogueStartDelayAfterAnswer > 0f)
            yield return new WaitForSecondsRealtime(seq.dialogueStartDelayAfterAnswer);

        HideWarningIcon();
    }

    private IEnumerator PlayCue(PhoneConversationCue cue)
    {
        if (cue.triggerCharacterEmote && characterAnimationController != null)
            characterAnimationController.TryStartEmote();

        float ringDuration = Mathf.Max(0f, cue.ringDuration);
        float iconDuration = cue.warningIconDuration > 0f ? cue.warningIconDuration : ringDuration;
        float totalCueDuration = Mathf.Max(ringDuration, iconDuration);

        if (cue.playRingBeforeLine && cue.ringClip != null && cueAudioSource != null)
            cueAudioSource.PlayOneShot(cue.ringClip);

        if (cue.showWarningIcon)
            ShowWarningIcon(cue);

        if (totalCueDuration > 0f)
            yield return new WaitForSecondsRealtime(totalCueDuration);

        HideWarningIcon();
    }

    private IEnumerator PlayIncomingCueRoutine(PhoneConversationOpeningSequence seq, bool keepWarningVisibleAfterCue)
    {
        ShowWarningIcon(seq.warningMode, seq.warningIconSprite);

        int repeatCount = Mathf.Max(1, seq.ringRepeatCount);
        float ringDuration = Mathf.Max(0f, seq.ringDuration);
        float ringInterval = Mathf.Max(0f, seq.ringInterval);

        for (int i = 0; i < repeatCount; i++)
        {
            if (seq.ringClip != null && cueAudioSource != null)
                cueAudioSource.PlayOneShot(seq.ringClip);

            float wait = ringDuration;
            if (i < repeatCount - 1)
                wait += ringInterval;

            if (wait > 0f)
                yield return new WaitForSecondsRealtime(wait);
        }

        if (!keepWarningVisibleAfterCue)
            HideWarningIcon();
    }

    // ──────────────────────── ANIMATION HELPERS ───────────────────────────

    private void PlayAnswerAnimation(PhoneConversationOpeningSequence seq)
    {
        if (characterAnimationController == null)
            return;

        characterAnimationController.SetDirection(Vector2.down);

        bool played = characterAnimationController.TryPlayOneShotState(
            seq.answerAnimationStateName,
            seq.answerAnimationCrossFadeDuration
        );

        if (!played && seq.triggerCharacterEmoteOnAnswer)
            characterAnimationController.TryStartEmote();
    }

    private void ResetCharacterAnimation()
    {
        if (characterAnimationController == null)
            return;

        characterAnimationController.ResetAnimation();
        characterAnimationController.SetDirection(Vector2.down);
        characterAnimationController.TryPlayOneShotState("Idle", 0.15f);
    }

    // ───────────────────────── WARNING ICON ───────────────────────────────

    private void ShowWarningIcon(PhoneConversationCue cue)
    {
        ShowWarningIcon(cue.warningMode, cue.warningIconSprite);
    }

    private void ShowWarningIcon(PhoneConversationWarningMode warningMode, Sprite lineIconSprite)
    {
        if (warningIconObject == null)
        {
            Debug.LogWarning("[PhoneConversationView] warningIconObject no está asignado. Asignalo en el Inspector.");
            return;
        }

        CacheWarningIconPosition();

        Sprite iconSprite = ResolveWarningSprite(warningMode, lineIconSprite);

        if (warningIconImage != null)
        {
            if (iconSprite != null)
            {
                warningIconImage.sprite = iconSprite;
                warningIconImage.enabled = true;
            }
            else
            {
                warningIconImage.enabled = false;
            }
        }

        if (warningFallbackText != null)
        {
            bool useFallback = iconSprite == null;
            warningFallbackText.gameObject.SetActive(useFallback);
            if (useFallback)
                warningFallbackText.text = "!";
        }

        warningIconObject.SetActive(true);

        // Cancelar animación anterior si hay una corriendo.
        StopWarningAnim();
        _warningAnimCoroutine = StartCoroutine(WarningFullAnimation());
    }

    private void HideWarningIcon()
    {
        StopWarningAnim();
        ResetWarningVisual();
    }

    private void StopWarningAnim()
    {
        if (_warningAnimCoroutine != null)
        {
            StopCoroutine(_warningAnimCoroutine);
            _warningAnimCoroutine = null;
        }
    }

    private void ResetWarningVisual()
    {
        if (warningIconCanvasGroup != null)
            warningIconCanvasGroup.alpha = 0f;

        if (warningIconObject != null)
            warningIconObject.SetActive(false);

        if (_warningPosCached && _warningIconRect != null)
        {
            _warningIconRect.anchoredPosition = _warningIconOriginalPos;
            _warningIconRect.localScale = _warningIconOriginalScale;
        }

        if (warningFallbackText != null)
            warningFallbackText.gameObject.SetActive(false);
    }

    // ───────────────────────── SHOW / HIDE ────────────────────────────────

    private void ShowPhonePanel()
    {
        if (phonePanel != null)
            phonePanel.SetActive(true);

        CachePhonePanelPosition();

        if (phonePanelCanvasGroup != null)
        {
            phonePanelCanvasGroup.alpha = 0f;
            phonePanelCanvasGroup.blocksRaycasts = false;
            phonePanelCanvasGroup.interactable = false;
        }
    }

    private void HideAllInstant()
    {
        StopTypingAndClearText();
        HideWarningIcon();

        if (phonePanelCanvasGroup != null)
        {
            phonePanelCanvasGroup.alpha = 0f;
            phonePanelCanvasGroup.blocksRaycasts = false;
            phonePanelCanvasGroup.interactable = false;
        }

        // Restaurar posición original para la próxima vez.
        if (_phonePanelPosCached && _phonePanelRect != null)
            _phonePanelRect.anchoredPosition = _phonePanelOriginalPos;

        if (phonePanel != null)
            phonePanel.SetActive(false);
    }

    private void StopTypingAndClearText()
    {
        if (phoneTypewriterEffect != null)
        {
            phoneTypewriterEffect.StopTyping();
            phoneTypewriterEffect.ClearText();
        }
    }

    private void ApplyPhoneTextStyle()
    {
        if (phoneBodyText == null)
            return;

        phoneBodyText.fontSize = runtimeFontSize;
        phoneBodyText.color = runtimeTextColor;
        phoneBodyText.faceColor = runtimeTextColor;
        phoneBodyText.outlineColor = runtimeTextOutlineColor;
        phoneBodyText.outlineWidth = runtimeTextOutlineWidth;
        phoneBodyText.alignment = TextAlignmentOptions.Center;
        phoneBodyText.enableWordWrapping = true;
        phoneBodyText.raycastTarget = false;
    }

    private void SetCharacterName(string charName)
    {
        if (characterNameText == null)
            return;
        characterNameText.text = charName;
        characterNameText.gameObject.SetActive(!string.IsNullOrWhiteSpace(charName));
    }

    // ─────────────────────── UI ANIMATIONS ──────────────────────────────

    private void CachePhonePanelPosition()
    {
        if (_phonePanelPosCached)
            return;

        if (phonePanel != null)
            _phonePanelRect = phonePanel.GetComponent<RectTransform>();

        if (_phonePanelRect != null)
        {
            _phonePanelOriginalPos = _phonePanelRect.anchoredPosition;
            _phonePanelPosCached = true;
        }
    }

    private void CacheWarningIconPosition()
    {
        if (_warningPosCached)
            return;

        if (warningIconObject != null)
            _warningIconRect = warningIconObject.GetComponent<RectTransform>();

        if (_warningIconRect != null)
        {
            _warningIconOriginalPos = _warningIconRect.anchoredPosition;
            _warningIconOriginalScale = _warningIconRect.localScale;
            _warningPosCached = true;
        }
    }

    private IEnumerator AnimateLineIn()
    {
        if (_phonePanelRect == null || phonePanelCanvasGroup == null || lineAnimInDuration <= 0f)
        {
            if (phonePanelCanvasGroup != null)
                phonePanelCanvasGroup.alpha = 1f;
            yield break;
        }

        // Empieza ABAJO y sube hasta la posición original.
        Vector2 startPos = _phonePanelOriginalPos - new Vector2(0f, lineSlideDistance);
        float elapsed = 0f;

        _phonePanelRect.anchoredPosition = startPos;
        phonePanelCanvasGroup.alpha = 0f;

        while (elapsed < lineAnimInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / lineAnimInDuration);
            float ease = EaseOutQuad(t);

            _phonePanelRect.anchoredPosition = Vector2.Lerp(startPos, _phonePanelOriginalPos, ease);
            phonePanelCanvasGroup.alpha = ease;

            yield return null;
        }

        _phonePanelRect.anchoredPosition = _phonePanelOriginalPos;
        phonePanelCanvasGroup.alpha = 1f;
    }

    private IEnumerator AnimateLineOut()
    {
        if (phonePanelCanvasGroup == null || lineAnimOutDuration <= 0f)
        {
            if (phonePanelCanvasGroup != null)
                phonePanelCanvasGroup.alpha = 0f;
            yield break;
        }

        // Sale hacia ARRIBA + fade out.
        Vector2 endPos = _phonePanelPosCached
            ? _phonePanelOriginalPos + new Vector2(0f, lineSlideDistance)
            : (_phonePanelRect != null ? _phonePanelRect.anchoredPosition + new Vector2(0f, lineSlideDistance) : Vector2.zero);
        Vector2 currentPos = _phonePanelRect != null ? _phonePanelRect.anchoredPosition : Vector2.zero;

        float elapsed = 0f;
        float startAlpha = phonePanelCanvasGroup.alpha;

        while (elapsed < lineAnimOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / lineAnimOutDuration);
            float ease = EaseInQuad(t);

            if (_phonePanelRect != null)
                _phonePanelRect.anchoredPosition = Vector2.Lerp(currentPos, endPos, ease);

            phonePanelCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, ease);
            yield return null;
        }

        phonePanelCanvasGroup.alpha = 0f;

        // Restaurar posición para la siguiente animación de entrada.
        if (_phonePanelRect != null && _phonePanelPosCached)
            _phonePanelRect.anchoredPosition = _phonePanelOriginalPos;
    }

    /// <summary>
    /// Animación completa auto-contenida: POP IN → HOLD → SUBE + FADE OUT → se oculta sola.
    /// </summary>
    private IEnumerator WarningFullAnimation()
    {
        if (_warningIconRect == null || warningIconCanvasGroup == null)
        {
            if (warningIconCanvasGroup != null)
                warningIconCanvasGroup.alpha = 1f;
            yield break;
        }

        Vector3 popScale = _warningIconOriginalScale * warningPopScale;
        float elapsed;

        // ── FASE 1: POP IN — aparece con un scale punch ──
        _warningIconRect.anchoredPosition = _warningIconOriginalPos;
        _warningIconRect.localScale = Vector3.zero;
        warningIconCanvasGroup.alpha = 1f;

        // 1a: Scale 0 → popScale (overshoot)
        elapsed = 0f;
        float halfPop = warningPopDuration * 0.6f;
        while (elapsed < halfPop)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / halfPop);
            _warningIconRect.localScale = Vector3.Lerp(Vector3.zero, popScale, EaseOutQuad(t));
            yield return null;
        }

        // 1b: popScale → tamaño normal (settle)
        elapsed = 0f;
        float settleDur = warningPopDuration * 0.4f;
        while (elapsed < settleDur)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / settleDur);
            _warningIconRect.localScale = Vector3.Lerp(popScale, _warningIconOriginalScale, EaseOutQuad(t));
            yield return null;
        }

        _warningIconRect.localScale = _warningIconOriginalScale;

        // ── FASE 2: HOLD — visible quieto ──
        if (warningHoldDuration > 0f)
            yield return new WaitForSecondsRealtime(warningHoldDuration);

        // ── FASE 3: SUBE + FADE OUT ──
        Vector2 riseEnd = _warningIconOriginalPos + new Vector2(0f, warningRiseDistance);
        elapsed = 0f;

        while (elapsed < warningRiseDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / warningRiseDuration);
            float easePos = EaseOutQuad(t);
            float easeAlpha = EaseInQuad(t); // fade out se acelera al final

            _warningIconRect.anchoredPosition = Vector2.Lerp(_warningIconOriginalPos, riseEnd, easePos);
            warningIconCanvasGroup.alpha = 1f - easeAlpha;

            yield return null;
        }

        // ── AUTO-HIDE ──
        warningIconCanvasGroup.alpha = 0f;
        warningIconObject.SetActive(false);
        _warningIconRect.anchoredPosition = _warningIconOriginalPos;
        _warningIconRect.localScale = _warningIconOriginalScale;
        _warningAnimCoroutine = null;
    }

    private static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
    private static float EaseInQuad(float t) => t * t;

    private static float EaseOutBack(float t)
    {
        const float c = 1.70158f;
        float t1 = t - 1f;
        return 1f + (c + 1f) * t1 * t1 * t1 + c * t1 * t1;
    }

    // ─────────────────────── AUTO-WIRE ────────────────────────────────────

    private void AutoWireReferences()
    {
        if (characterAnimationController == null)
            characterAnimationController = FindFirstObjectByType<PlayerAnimationController>();

        if (phonePanel != null)
        {
            if (phonePanelCanvasGroup == null)
                phonePanelCanvasGroup = phonePanel.GetComponent<CanvasGroup>();
            if (phoneTypewriterEffect == null)
                phoneTypewriterEffect = phonePanel.GetComponentInChildren<TypewriterEffect>(true);
        }

        if (phoneTypewriterEffect != null && phoneBodyText == null)
            phoneBodyText = phoneTypewriterEffect.GetComponent<TMP_Text>();

        if (phoneTypewriterEffect != null && phoneTypewriterEffect.audioSource == null)
        {
            AudioSource src = phoneTypewriterEffect.GetComponent<AudioSource>();
            if (src == null)
            {
                src = phoneTypewriterEffect.gameObject.AddComponent<AudioSource>();
                src.playOnAwake = false;
            }
            phoneTypewriterEffect.audioSource = src;
        }

        if (warningIconObject != null)
        {
            if (warningIconCanvasGroup == null)
                warningIconCanvasGroup = warningIconObject.GetComponent<CanvasGroup>();
            if (warningIconImage == null)
                warningIconImage = warningIconObject.GetComponent<Image>();
            if (warningFallbackText == null)
                warningFallbackText = warningIconObject.GetComponentInChildren<TMP_Text>(true);
        }

        EnsureAudioSources();
    }

    private void EnsureAudioSources()
    {
        if (cueAudioSource == null)
            cueAudioSource = CreateChildAudioSource("PhoneCueAudio");
        if (lineVoiceAudioSource == null)
            lineVoiceAudioSource = CreateChildAudioSource("PhoneLineVoiceAudio");
    }

    private AudioSource CreateChildAudioSource(string objectName)
    {
        Transform existing = transform.Find(objectName);
        AudioSource src = existing != null ? existing.GetComponent<AudioSource>() : null;
        if (src != null)
        {
            src.playOnAwake = false;
            return src;
        }

        GameObject child = new GameObject(objectName);
        child.transform.SetParent(transform, false);
        src = child.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = false;
        return src;
    }

    // ─────────────────────────── HELPERS ──────────────────────────────────

    private Sprite ResolveWarningSprite(PhoneConversationWarningMode mode, Sprite lineSprite)
    {
        // UseAssignedSprite o UsePerLineSprite: usar el sprite que viene por parámetro.
        if (mode != PhoneConversationWarningMode.None && lineSprite != null)
            return lineSprite;

        // Fallback al default del ScriptableObject.
        return _conversationData != null ? _conversationData.defaultWarningIconSprite : null;
    }

    private static bool HasIncomingCue(PhoneConversationOpeningSequence seq)
    {
        return seq.enabled && (seq.ringClip != null || seq.showWarningIconDuringRing);
    }
}
