using System.Collections;
using System;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

[System.Serializable]
public struct IntroLine
{
    [TextArea] public string text;
    [Tooltip("Volumen de voz para esta línea (-1 = usar valor por defecto del TypewriterEffect).")]
    [Range(-1f, 1f)] public float voiceVolume;
    [Tooltip("Cada cuántos caracteres visibles se reproduce voz (-1 = usar valor por defecto).")]
    public int charsPerSound;
    [Tooltip("Tiempo en segundos por carácter para esta línea (-1 = usar valor por defecto del TypewriterEffect).")]
    public float typingSpeed;
    [Tooltip("Cuánto permanece la línea visible tras terminar de escribirse (-1 = usar valor por defecto del IntroController).")]
    public float displayDuration;
}

public enum DecisionPromptHideMode
{
    Instant,
    FadeOut
}

public class IntroController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("El contenedor del texto principal (para animar fade/slide).")]
    public RectTransform textContainer; 
    [Tooltip("El componente TypewriterEffect.")]
    public TypewriterEffect typewriter;

    private CanvasGroup _textCanvasGroup;
    
    [Header("Decision UI")]
    [Tooltip("El contenedor de las opciones de decisión (Si/No).")]
    public GameObject decisionPanel;
    [Tooltip("El texto de la opción 'Si'.")]
    public TMP_Text yesOptionText;
    [Tooltip("El botón de selección de la opción 'Si'.")]
    public Button yesButton;
    [Tooltip("El texto de la opción 'No'.")]
    public TMP_Text noOptionText;
    [Tooltip("El botón de selección de la opción 'No'.")]
    public Button noButton;

    [Header("Audio")]
    [Tooltip("AudioSource para la música de intro (suena al inicio, NO loop).")]
    public AudioSource introMusicSource;
    [Tooltip("Clip de música que suena durante la intro (se reproduce una sola vez).")]
    public AudioClip introMusicClip;
    [Range(0f, 1f)] public float introMusicVolume = 0.12f;

    [Tooltip("AudioSource para la música de la habitación (loop, se activa al confirmar).")]
    public AudioSource roomMusicSource;
    [Tooltip("Clip de música ambiental de la habitación (loop, se combina con la intro).")]
    public AudioClip roomMusicClip;
    [Range(0f, 1f)] public float roomMusicVolume = 0.15f;

    [Tooltip("AudioSource dedicado para efectos de sonido UI. Si no se asigna, se crea uno automáticamente.")]
    public AudioSource sfxAudioSource;
    [Tooltip("Sonido al cambiar de opción (Navegación UI, uno solo).")]
    public AudioClip navigationSFX;
    [Range(0f, 1f)] public float navigationSFXVolume = 0.7f;
    [Tooltip("Primer sonido al confirmar la selección.")]
    public AudioClip confirmSFX;
    [Range(0f, 1f)] public float confirmSFXVolume = 0.8f;
    [Tooltip("Segundo sonido al confirmar (suena después del primero).")]
    public AudioClip confirmSFX2;
    [Range(0f, 1f)] public float confirmSFX2Volume = 0.8f;
    [Tooltip("Tiempo en segundos entre el primer y el segundo sonido de confirmación.")]
    public float confirmSFXDelay = 0.035f;
    [Tooltip("Cantidad máxima de veces que puede sonar la música de intro antes de detenerse.")]
    [Range(1, 3)] public int introMusicPlayCount = 3;
    [Tooltip("Crossfade entre una repetición y la siguiente para que el reinicio no se escuche cortado.")]
    public float introMusicLoopCrossfadeDuration = 0.35f;
    [Tooltip("Fade out automático al final de la última repetición de la música de intro.")]
    public float introMusicEndFadeDuration = 0.75f;

    [Header("Dialogues Sequence")]
    [Tooltip("Lista de líneas de intro con texto y ajustes de voz por línea.")]
    public IntroLine[] introLines;

    // Obsolete but kept to avoid breaking serialized references if any (though logic will change)
    // [TextArea] public string introDialogueText = "¿Quieres ser mi amigo?"; 

    public string optionYesLabel = "Zhy ta bien"; 
    public string optionNoLabel = "Nel perro";    

    [Header("Settings")]
    public float initialDelay = 0.8f;
    public float textReadingTime = 1.5f;
    public float fadeOutTime = 0.6f;
    [Tooltip("Fallback para líneas cortas como 'Oye' si no se les asigna typingSpeed manualmente.")]
    public float shortPromptTypingSpeed = 0.05f;
    [Tooltip("Texto corto que debe salir más rápido por defecto si no tiene override manual.")]
    public string shortPromptText = "Oye";

    [Header("Animation Settings")]
    [Tooltip("Cuánto sube la opción seleccionada al navegar.")]
    public float optionMoveDistance = 40f;
    public float optionMoveDuration = 0.2f;
    [Tooltip("Distancia extra que sube la opción al confirmar (ponerse amarillo).")]
    public float confirmBounceDistance = 25f;
    [Tooltip("Duración de la animación de bounce al confirmar.")]
    public float confirmBounceDuration = 0.18f;
    [Tooltip("Curva para la animación de Slide Up del texto.")]
    public AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Tooltip("Curva para el Fade In del texto.")]
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Tooltip("Altura desde la que sube el texto en unidades de UI.")]
    public float slideDistance = 50f;
    [Tooltip("Duración de la animación de entrada del texto.")]
    public float textEntranceDuration = 0.8f;
    [Tooltip("Cómo desaparece el texto principal al confirmar la decisión.")]
    public DecisionPromptHideMode decisionPromptHideMode = DecisionPromptHideMode.Instant;
    [Tooltip("Duración del fade del texto principal al confirmar, si se usa FadeOut.")]
    public float decisionPromptFadeDuration = 0.12f;

    [Header("Ending Sequence")]
    [Tooltip("Duración de la pantalla negra sin texto antes de revelar la escena.")]
    public float blackScreenDuration = 2.5f;
    public float zoomDuration = 2f;
    [Tooltip("CinemachineCamera de la escena (obligatorio para el zoom).")]
    public CinemachineCamera cinemachineCamera;
    [Tooltip("OrthographicSize al hacer zoom in (más chico = más cerca).")]
    public float targetZoomSize = 1.5f;
    [Tooltip("OrthographicSize final de la cámara tras el ending. Ajustar aquí para controlar qué tan cerca queda. (-1 = leer automáticamente de la cámara al iniciar).")]
    public float finalCameraSize = -1f;

    [Header("Blur (Desenfoque)")]
    [Tooltip("Intensidad máxima del blur al inicio del ending (1.5 = fuerte).")]
    [Range(0.1f, 1.5f)] public float maxBlurRadius = 1.5f;
    [Tooltip("Duración del fade del blur (de borroso a normal).")]
    public float blurFadeDuration = 2.5f;

    [Header("Colors")]
    [Tooltip("Color de la opción seleccionada al navegar.")]
    public Color selectedColor = Color.white;
    [Tooltip("Color normal de las opciones (cyan).")]
    public Color normalColor = Color.cyan;
    [Tooltip("Color al confirmar la opción (amarillo).")]
    public Color confirmColor = Color.yellow;

    [Header("Game Control")]
    [Tooltip("El Panel Negro que cubre toda la pantalla.")]
    public CanvasGroup blackOverlayCanvasGroup;

    private bool _decisionActive = false;
    private bool _selectedYes = true; // Default selection (e.g. Yes on right/left?)

    private Vector2 _yesOriginalPos;
    private Vector2 _noOriginalPos;
    private Vector2 _textContainerOriginalPos;

    // Blur
    private Volume _blurVolume;
    private DepthOfField _dof;
    private Coroutine _introMusicRoutine;
    private Coroutine _introMusicFadeRoutine;
    private bool _introMusicStopRequested;
    private bool _introMusicFadeStarted;
    private AudioSource _introMusicSecondarySource;
    private AudioSource _confirmPrimaryAudioSource;
    private AudioSource _confirmSecondaryAudioSource;

    private const float LegacyConfirmSFXDelay = 0.1f;
    private const float UpgradedConfirmSFXDelay = 0.035f;

    private void Awake()
    {
        // 0. Immediate Setup
        if (blackOverlayCanvasGroup != null) 
        {
            blackOverlayCanvasGroup.alpha = 1f; 
            blackOverlayCanvasGroup.gameObject.SetActive(true);
        }

        // Setup Fade
        if (textContainer != null)
        {
            _textContainerOriginalPos = textContainer.anchoredPosition; // Store stable position
            _textCanvasGroup = textContainer.GetComponent<CanvasGroup>();
            if (_textCanvasGroup == null) _textCanvasGroup = textContainer.gameObject.AddComponent<CanvasGroup>();
            _textCanvasGroup.alpha = 0f; // Start invisible
        }

        // Crear SFX AudioSource si no se asignó
        if (sfxAudioSource == null)
        {
            sfxAudioSource = gameObject.AddComponent<AudioSource>();
            sfxAudioSource.playOnAwake = false;
            sfxAudioSource.loop = false;
        }

        _introMusicSecondarySource = CreateChildAudioSource("_IntroMusicSecondary", introMusicSource);
        _confirmPrimaryAudioSource = CreateChildAudioSource("_ConfirmPrimary", sfxAudioSource);
        _confirmSecondaryAudioSource = CreateChildAudioSource("_ConfirmSecondary", sfxAudioSource);

        // Store original positions for UI Animation
        if (yesButton != null) _yesOriginalPos = yesButton.GetComponent<RectTransform>().anchoredPosition;
        if (noButton != null) _noOriginalPos = noButton.GetComponent<RectTransform>().anchoredPosition;

        // Upgrade suave para escenas que siguen teniendo el delay original serializado.
        if (Mathf.Approximately(confirmSFXDelay, LegacyConfirmSFXDelay))
        {
            confirmSFXDelay = UpgradedConfirmSFXDelay;
        }
    }

    private void Start()
    {
        // PAUSA REAL DEL JUEGO
        Time.timeScale = 0f;

        // Ensure background is black
        if (Camera.main != null)
        {
            Camera.main.backgroundColor = Color.black;
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
        }

        // Setup blur effect (se activa desde el principio)
        SetupBlurEffect();

        // Setup initial fade state
        if (_textCanvasGroup != null) _textCanvasGroup.alpha = 0f;
        if (decisionPanel != null) decisionPanel.SetActive(false);

        // Iniciar música de intro con repeticiones limitadas y fade al final.
        if (introMusicSource != null && introMusicClip != null)
        {
            introMusicSource.clip = introMusicClip;
            introMusicSource.loop = false;
            introMusicSource.playOnAwake = false;
            introMusicSource.volume = introMusicVolume;
            _introMusicRoutine = StartCoroutine(PlayIntroMusicSequence());
        }

        // Start sequence (Wait for one frame to ensure UI is ready)
        StartCoroutine(IntroSequence());
    }

    private void Update()
    {
        if (_decisionActive)
        {
            HandleInput();
            // Re-check: HandleInput puede desactivar _decisionActive al confirmar
            if (_decisionActive)
                UpdateSelectionVisuals();
        }
    }

    private IEnumerator IntroSequence()
    {
        // 1. Initial Delay
        yield return new WaitForSecondsRealtime(initialDelay);

        // 2. Iterate through all texts
        if (introLines != null && typewriter != null)
        {
            for (int i = 0; i < introLines.Length; i++)
            {
                IntroLine line = introLines[i];

                // Stop any previous animations/fades
                if (_textCanvasGroup != null) _textCanvasGroup.alpha = 0f;
                // Clear text so we don't see previous text fading up
                typewriter.ClearText();

                // Reset Pos for entrance
                if (textContainer != null) 
                {
                   textContainer.anchoredPosition = _textContainerOriginalPos;
                }

                // Typewriter + Entrance simultaneously
                bool typingFinished = false;
                typewriter.ShowText(line.text, line.voiceVolume, line.charsPerSound, GetEffectiveTypingSpeed(line), () => typingFinished = true);

                // Animate Entrance (Slide Up + Fade In) concurrent with typing
                yield return StartCoroutine(AnimateTextEntrance());
                
                // Wait for typing to complete
                while (!typingFinished) yield return null;
                
                // Wait for reading time
                yield return new WaitForSecondsRealtime(GetEffectiveDisplayDuration(line));

                // Fade Out ONLY if NOT the last text
                if (i < introLines.Length - 1)
                {
                    yield return StartCoroutine(AnimateTextExit());
                }
            }
        }

        // 3. Show Decision (Keep last text visible)
        ShowDecision();
    }

    private float GetEffectiveTypingSpeed(IntroLine line)
    {
        if (line.typingSpeed > 0f)
        {
            return line.typingSpeed;
        }

        if (!string.IsNullOrWhiteSpace(shortPromptText) &&
            string.Equals(line.text?.Trim(), shortPromptText.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return shortPromptTypingSpeed;
        }

        return -1f;
    }

    private float GetEffectiveDisplayDuration(IntroLine line)
    {
        return line.displayDuration > 0f ? line.displayDuration : textReadingTime;
    }

    private IEnumerator PlayIntroMusicSequence()
    {
        if (introMusicSource == null || introMusicClip == null)
        {
            yield break;
        }

        int repetitions = Mathf.Max(1, introMusicPlayCount);
        float loopCrossfade = Mathf.Clamp(introMusicLoopCrossfadeDuration, 0f, introMusicClip.length * 0.45f);
        float endFadeDuration = Mathf.Clamp(introMusicEndFadeDuration, 0f, introMusicClip.length * 0.9f);
        AudioSource currentSource = introMusicSource;
        AudioSource nextSource = _introMusicSecondarySource != null ? _introMusicSecondarySource : introMusicSource;

        PrepareIntroMusicSource(currentSource, introMusicVolume);
        currentSource.Play();

        for (int playIndex = 0; playIndex < repetitions && !_introMusicStopRequested; playIndex++)
        {
            bool isLastPlay = playIndex == repetitions - 1;

            if (!isLastPlay)
            {
                float leadTime = Mathf.Max(0f, introMusicClip.length - loopCrossfade);
                if (leadTime > 0f)
                {
                    yield return new WaitForSecondsRealtime(leadTime);
                }

                if (_introMusicStopRequested)
                {
                    yield break;
                }

                PrepareIntroMusicSource(nextSource, 0f);
                nextSource.Play();

                if (loopCrossfade > 0f)
                {
                    yield return StartCoroutine(CrossfadeAudioSources(currentSource, nextSource, loopCrossfade));
                }
                else
                {
                    currentSource.Stop();
                    nextSource.volume = introMusicVolume;
                }

                currentSource.Stop();
                currentSource.volume = introMusicVolume;

                AudioSource temp = currentSource;
                currentSource = nextSource;
                nextSource = temp;
                continue;
            }

            float fullVolumeDuration = Mathf.Max(0f, introMusicClip.length - endFadeDuration);
            if (fullVolumeDuration > 0f)
            {
                yield return new WaitForSecondsRealtime(fullVolumeDuration);
            }

            if (_introMusicStopRequested || !currentSource.isPlaying)
            {
                yield break;
            }

            if (endFadeDuration > 0f)
            {
                yield return StartCoroutine(FadeAudioSource(currentSource, currentSource.volume, 0f, endFadeDuration, true));
            }
            else
            {
                currentSource.Stop();
            }

            currentSource.volume = introMusicVolume;
        }
    }

    private void PrepareIntroMusicSource(AudioSource source, float startVolume)
    {
        if (source == null)
        {
            return;
        }

        source.clip = introMusicClip;
        source.loop = false;
        source.playOnAwake = false;
        source.volume = startVolume;
        source.Stop();
    }

    private IEnumerator CrossfadeAudioSources(AudioSource fromSource, AudioSource toSource, float duration)
    {
        if (fromSource == null || toSource == null || duration <= 0f)
        {
            yield break;
        }

        float timer = 0f;
        float fromStartVolume = fromSource.volume;
        while (timer < duration && !_introMusicStopRequested)
        {
            timer += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(timer / duration);
            fromSource.volume = Mathf.Lerp(fromStartVolume, 0f, progress);
            toSource.volume = Mathf.Lerp(0f, introMusicVolume, progress);
            yield return null;
        }

        fromSource.volume = 0f;
        toSource.volume = introMusicVolume;
    }

    private IEnumerator FadeAudioSource(AudioSource source, float fromVolume, float toVolume, float duration, bool stopAtEnd)
    {
        if (source == null)
        {
            yield break;
        }

        if (duration <= 0f)
        {
            source.volume = toVolume;
            if (stopAtEnd)
            {
                source.Stop();
            }
            yield break;
        }

        float timer = 0f;
        while (timer < duration && source.isPlaying)
        {
            timer += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(fromVolume, toVolume, Mathf.Clamp01(timer / duration));
            yield return null;
        }

        source.volume = toVolume;
        if (stopAtEnd)
        {
            source.Stop();
        }
    }

    private IEnumerator AnimateTextExit()
    {
        if (_textCanvasGroup == null) yield break;

        Vector2 startPos = _textContainerOriginalPos;
        Vector2 endPos = startPos + new Vector2(0, slideDistance);

        float timer = 0f;
        while (timer < fadeOutTime)
        {
            float progress = timer / fadeOutTime;
            float slideEval = slideCurve.Evaluate(progress);
            float fadeEval = fadeCurve.Evaluate(progress);

            _textCanvasGroup.alpha = Mathf.Lerp(1f, 0f, fadeEval);
            if (textContainer != null)
                textContainer.anchoredPosition = Vector2.Lerp(startPos, endPos, slideEval);

            timer += Time.unscaledDeltaTime;
            yield return null;
        }
        _textCanvasGroup.alpha = 0f;
    }

    private IEnumerator AnimateTextEntrance()
    {
        if (textContainer == null) yield break;

        Vector2 endPos = _textContainerOriginalPos;
        Vector2 startPos = endPos - new Vector2(0, slideDistance);
        
        float timer = 0f;
        while (timer < textEntranceDuration)
        {
            float progress = timer / textEntranceDuration;
            float slideEval = slideCurve.Evaluate(progress);
            float fadeEval = fadeCurve.Evaluate(progress);

            textContainer.anchoredPosition = Vector2.Lerp(startPos, endPos, slideEval);
            if (_textCanvasGroup != null) _textCanvasGroup.alpha = Mathf.Lerp(0f, 1f, fadeEval);

            // Use unscaledDeltaTime because timeScale is 0
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        textContainer.anchoredPosition = endPos;
        if (_textCanvasGroup != null) _textCanvasGroup.alpha = 1f;
    }

    private void ShowDecision()
    {
        _decisionActive = true;
        if (decisionPanel != null) decisionPanel.SetActive(true);
        
        // Setup Button Labels ONLY if not empty (Evita borrar el texto si está vacío en inspector)
        if (yesOptionText != null && !string.IsNullOrEmpty(optionYesLabel)) 
            yesOptionText.text = optionYesLabel;
            
        if (noOptionText != null && !string.IsNullOrEmpty(optionNoLabel)) 
            noOptionText.text = optionNoLabel;

        UpdateSelectionVisuals();
    }

    private void HandleInput()
    {
        bool changed = false;

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (!_selectedYes) changed = true; // Was No, now Yes
            _selectedYes = true; // Assuming A is Left (Si)
            UpdateSelectionVisuals();
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (_selectedYes) changed = true; // Was Yes, now No
            _selectedYes = false; // Assuming D is Right (No)
            UpdateSelectionVisuals();
        }

        if (changed)
        {
            if (sfxAudioSource != null && navigationSFX != null)
                sfxAudioSource.PlayOneShot(navigationSFX, navigationSFXVolume);
        }
    
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Z))
        {
            MakeDecision();
        }
    }

    private void PlayConfirmSounds()
    {
        double startTime = AudioSettings.dspTime + 0.02d;
        float delay = Mathf.Max(0f, confirmSFXDelay);

        if (_confirmPrimaryAudioSource != null && confirmSFX != null)
        {
            _confirmPrimaryAudioSource.Stop();
            _confirmPrimaryAudioSource.clip = confirmSFX;
            _confirmPrimaryAudioSource.volume = confirmSFXVolume;
            _confirmPrimaryAudioSource.PlayScheduled(startTime);
        }

        if (_confirmSecondaryAudioSource != null && confirmSFX2 != null)
        {
            _confirmSecondaryAudioSource.Stop();
            _confirmSecondaryAudioSource.clip = confirmSFX2;
            _confirmSecondaryAudioSource.volume = confirmSFX2Volume;
            _confirmSecondaryAudioSource.PlayScheduled(startTime + delay);
        }
    }

    private void UpdateSelectionVisuals()
    {
        // Highlight logic
        if (yesButton == null || noButton == null) return;

        RectTransform yesRect = yesButton.GetComponent<RectTransform>();
        RectTransform noRect = noButton.GetComponent<RectTransform>();

        // Interpolation factor for smooth animation (Time.unscaledDeltaTime * speed)
        float speed = 10f; 
        float step = Time.unscaledDeltaTime * speed;

        if (yesOptionText != null)
        {
            yesOptionText.color = _selectedYes ? selectedColor : normalColor;
            // Target Position
            Vector2 targetYes = _yesOriginalPos + (_selectedYes ? new Vector2(0, optionMoveDistance) : Vector2.zero);
            yesRect.anchoredPosition = Vector2.Lerp(yesRect.anchoredPosition, targetYes, step);
        }

        if (noOptionText != null)
        {
            noOptionText.color = !_selectedYes ? selectedColor : normalColor;
            // Target Position
            Vector2 targetNo = _noOriginalPos + (!_selectedYes ? new Vector2(0, optionMoveDistance) : Vector2.zero);
            noRect.anchoredPosition = Vector2.Lerp(noRect.anchoredPosition, targetNo, step);
        }
    }

    private void MakeDecision()
    {
        _decisionActive = false;

        // Cambiar a amarillo la opción elegida
        if (_selectedYes && yesOptionText != null)
            yesOptionText.color = confirmColor;
        else if (!_selectedYes && noOptionText != null)
            noOptionText.color = confirmColor;

        // Dos sonidos de confirmación en secuencia
        PlayConfirmSounds();
        
        if (_selectedYes)
        {
            TrustManager.Instance.ModifyLC(9f);
        }
        else
        {
            TrustManager.Instance.ModifyLC(-9f);
        }

        // Iniciar música de habitación (loop) - se combina con la intro que sigue sonando
        if (roomMusicSource != null && roomMusicClip != null)
        {
            roomMusicSource.clip = roomMusicClip;
            roomMusicSource.loop = true;
            roomMusicSource.volume = roomMusicVolume;
            roomMusicSource.Play();
        }

        // Animar slide-up de las opciones y luego iniciar ending
        StartCoroutine(ConfirmAnimationThenEnding());
    }

    private IEnumerator ConfirmAnimationThenEnding()
    {
        // Pausa para que se vea el color amarillo antes de animar
        yield return new WaitForSecondsRealtime(0.35f);

        // --- Bounce extra en la opción confirmada (amarillo) ---
        RectTransform selectedRect = _selectedYes
            ? (yesButton != null ? yesButton.GetComponent<RectTransform>() : null)
            : (noButton != null ? noButton.GetComponent<RectTransform>() : null);

        if (selectedRect != null && confirmBounceDistance > 0f)
        {
            Vector2 bounceStart = selectedRect.anchoredPosition;
            Vector2 bounceTarget = bounceStart + new Vector2(0, confirmBounceDistance);
            float bt = 0f;
            while (bt < confirmBounceDuration)
            {
                float ease = Mathf.SmoothStep(0f, 1f, bt / confirmBounceDuration);
                selectedRect.anchoredPosition = Vector2.Lerp(bounceStart, bounceTarget, ease);
                bt += Time.unscaledDeltaTime;
                yield return null;
            }
            selectedRect.anchoredPosition = bounceTarget;
        }

        // Animar ambas opciones subiendo y desapareciendo
        RectTransform yesRect = yesButton != null ? yesButton.GetComponent<RectTransform>() : null;
        RectTransform noRect = noButton != null ? noButton.GetComponent<RectTransform>() : null;

        Vector2 yesStart = yesRect != null ? yesRect.anchoredPosition : Vector2.zero;
        Vector2 noStart = noRect != null ? noRect.anchoredPosition : Vector2.zero;
        float confirmSlideDistance = optionMoveDistance * 2f;

        float startAlphaYes = yesOptionText != null ? 1f : 0f;
        float startAlphaNo = noOptionText != null ? 1f : 0f;
        float promptStartAlpha = _textCanvasGroup != null ? _textCanvasGroup.alpha : 0f;

        if (decisionPromptHideMode == DecisionPromptHideMode.Instant && _textCanvasGroup != null)
        {
            _textCanvasGroup.alpha = 0f;
        }

        float timer = 0f;
        float duration = 0.4f;
        while (timer < duration)
        {
            float progress = timer / duration;
            float ease = Mathf.SmoothStep(0f, 1f, progress);

            if (yesRect != null)
            {
                yesRect.anchoredPosition = yesStart + new Vector2(0, confirmSlideDistance * ease);
                if (yesOptionText != null)
                {
                    Color c = yesOptionText.color;
                    c.a = Mathf.Lerp(startAlphaYes, 0f, ease);
                    yesOptionText.color = c;
                }
            }
            if (noRect != null)
            {
                noRect.anchoredPosition = noStart + new Vector2(0, confirmSlideDistance * ease);
                if (noOptionText != null)
                {
                    Color c = noOptionText.color;
                    c.a = Mathf.Lerp(startAlphaNo, 0f, ease);
                    noOptionText.color = c;
                }
            }

            if (_textCanvasGroup != null && decisionPromptHideMode == DecisionPromptHideMode.FadeOut)
            {
                float promptProgress = decisionPromptFadeDuration > 0f
                    ? Mathf.Clamp01(timer / decisionPromptFadeDuration)
                    : 1f;
                _textCanvasGroup.alpha = Mathf.Lerp(promptStartAlpha, 0f, promptProgress);
            }

            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        // Start Ending Sequence (Blur + Zoom)
        BeginIntroMusicFadeOut(3f);
        StartCoroutine(EndingSequence());
    }

    private void BeginIntroMusicFadeOut(float duration)
    {
        if (_introMusicFadeStarted)
        {
            return;
        }

        _introMusicFadeStarted = true;
        _introMusicStopRequested = true;

        if (_introMusicRoutine != null)
        {
            StopCoroutine(_introMusicRoutine);
            _introMusicRoutine = null;
        }

        if (_introMusicFadeRoutine != null)
        {
            StopCoroutine(_introMusicFadeRoutine);
        }

        _introMusicFadeRoutine = StartCoroutine(FadeOutIntroMusic(duration));
    }

    private IEnumerator FadeOutIntroMusic(float duration)
    {
        AudioSource primary = introMusicSource;
        AudioSource secondary = _introMusicSecondarySource;

        bool primaryPlaying = primary != null && primary.isPlaying;
        bool secondaryPlaying = secondary != null && secondary.isPlaying;
        if (!primaryPlaying && !secondaryPlaying) yield break;

        float primaryStartVol = primaryPlaying ? primary.volume : 0f;
        float secondaryStartVol = secondaryPlaying ? secondary.volume : 0f;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(timer / duration);
            if (primaryPlaying)
            {
                primary.volume = Mathf.Lerp(primaryStartVol, 0f, progress);
            }
            if (secondaryPlaying)
            {
                secondary.volume = Mathf.Lerp(secondaryStartVol, 0f, progress);
            }
            yield return null;
        }

        if (primary != null)
        {
            primary.Stop();
            primary.volume = introMusicVolume;
        }
        if (secondary != null)
        {
            secondary.Stop();
            secondary.volume = introMusicVolume;
        }

        _introMusicFadeRoutine = null;
    }

    private AudioSource CreateChildAudioSource(string childName, AudioSource templateSource)
    {
        GameObject audioObject = new GameObject(childName);
        audioObject.transform.SetParent(transform, false);

        AudioSource source = audioObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.volume = 1f;

        if (templateSource != null)
        {
            source.outputAudioMixerGroup = templateSource.outputAudioMixerGroup;
            source.spatialBlend = templateSource.spatialBlend;
            source.priority = templateSource.priority;
            source.pitch = templateSource.pitch;
            source.panStereo = templateSource.panStereo;
            source.reverbZoneMix = templateSource.reverbZoneMix;
        }

        return source;
    }

    private IEnumerator EndingSequence()
    {
        // 0. Pausa breve para que el jugador vea el color amarillo de confirmación
        yield return new WaitForSecondsRealtime(0.5f);

        // 2. Ocultar toda la UI de intro
        if (decisionPanel != null) decisionPanel.SetActive(false);
        if (_textCanvasGroup != null) _textCanvasGroup.alpha = 0f;

        // 3. Pantalla negra sin texto (2-3 segundos)
        yield return new WaitForSecondsRealtime(blackScreenDuration);

        // 4. Restaurar timeScale para que Cinemachine funcione
        Time.timeScale = 1f;

        // 5. Secuencia de cámara: empieza cercana y borrosa, luego vuelve a normal
        if (cinemachineCamera != null)
        {
            float normalSize = finalCameraSize > 0f ? finalCameraSize : cinemachineCamera.Lens.OrthographicSize;
            // Poner cámara cerca ANTES de revelar la escena
            cinemachineCamera.Lens.OrthographicSize = targetZoomSize;

            // FASE 1: Desvanecer overlay negro → revela mundo borroso y cercano
            float timer = 0f;
            while (timer < zoomDuration)
            {
                float progress = timer / zoomDuration;
                float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

                if (blackOverlayCanvasGroup != null)
                    blackOverlayCanvasGroup.alpha = Mathf.Lerp(1f, 0f, smoothProgress);

                timer += Time.deltaTime;
                yield return null;
            }
            if (blackOverlayCanvasGroup != null) blackOverlayCanvasGroup.alpha = 0f;

            // Mantener un momento (borroso y cerca)
            yield return new WaitForSeconds(1f);

            // FASE 2: Zoom out + blur se limpia simultáneamente a la MISMA velocidad
            timer = 0f;
            while (timer < zoomDuration)
            {
                float progress = Mathf.Clamp01(timer / zoomDuration);
                float smooth = Mathf.SmoothStep(0f, 1f, progress);
                cinemachineCamera.Lens.OrthographicSize = Mathf.Lerp(targetZoomSize, normalSize, smooth);

                if (_dof != null)
                    _dof.gaussianMaxRadius.Override(Mathf.Lerp(maxBlurRadius, 0f, smooth));

                timer += Time.deltaTime;
                yield return null;
            }
            cinemachineCamera.Lens.OrthographicSize = normalSize;
        }
        else
        {
            // Fallback sin Cinemachine: solo desvanecer overlay + blur
            Debug.LogWarning("[IntroController] No se asignó CinemachineCamera. El zoom no funcionará.");
            float timer = 0f;
            while (timer < zoomDuration)
            {
                float progress = timer / zoomDuration;
                if (blackOverlayCanvasGroup != null)
                    blackOverlayCanvasGroup.alpha = Mathf.Lerp(1f, 0f, progress);
                timer += Time.deltaTime;
                yield return null;
            }
            if (blackOverlayCanvasGroup != null) blackOverlayCanvasGroup.alpha = 0f;

            yield return new WaitForSeconds(1f);

            timer = 0f;
            while (timer < blurFadeDuration)
            {
                float progress = timer / blurFadeDuration;
                if (_dof != null)
                    _dof.gaussianMaxRadius.Override(Mathf.Lerp(maxBlurRadius, 0f, progress));
                timer += Time.deltaTime;
                yield return null;
            }
        }

        // 6. Limpiar blur completamente
        if (_dof != null) _dof.gaussianMaxRadius.Override(0f);
        if (_blurVolume != null) Destroy(_blurVolume.gameObject);

        // 7. Desactivar overlay y controller
        if (blackOverlayCanvasGroup != null) blackOverlayCanvasGroup.gameObject.SetActive(false);
        this.enabled = false;
    }

    private void SetupBlurEffect()
    {
        // Habilitar post-procesado en la cámara
        if (Camera.main != null)
        {
            var camData = Camera.main.GetUniversalAdditionalCameraData();
            if (camData != null)
                camData.renderPostProcessing = true;
        }

        // Crear Volume global para el blur
        var volumeObj = new GameObject("_IntroBlurVolume");
        _blurVolume = volumeObj.AddComponent<Volume>();
        _blurVolume.isGlobal = true;
        _blurVolume.priority = 100;
        _blurVolume.profile = ScriptableObject.CreateInstance<VolumeProfile>();

        _dof = _blurVolume.profile.Add<DepthOfField>(true);
        _dof.mode.Override(DepthOfFieldMode.Gaussian);
        _dof.gaussianStart.Override(0f);
        _dof.gaussianEnd.Override(0.01f);
        _dof.gaussianMaxRadius.Override(maxBlurRadius);
    }
}
