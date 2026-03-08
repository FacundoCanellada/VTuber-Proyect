using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

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
    public AudioSource backgroundMusicSource;
    [Tooltip("AudioSource dedicado para efectos de sonido UI. Si no se asigna, se crea uno automáticamente.")]
    public AudioSource sfxAudioSource;
    [Tooltip("Sonido de fondo (loop) para la decisión.")]
    public AudioClip decisionLoopClip;
    [Tooltip("Sonido al cambiar de opción (Navegación UI).")]
    public AudioClip navigationSFX;
    [Range(0f, 1f)] public float navigationSFXVolume = 0.7f;
    [Tooltip("Sonido al confirmar la selección (botón de aceptar).")]
    public AudioClip confirmSFX;
    [Range(0f, 1f)] public float confirmSFXVolume = 0.8f;

    [Header("Dialogues Sequence")]
    [Tooltip("Lista de textos que aparecerán uno tras otro.")]
    [TextArea] public string[] introTexts; // Replaces single text

    // Obsolete but kept to avoid breaking serialized references if any (though logic will change)
    // [TextArea] public string introDialogueText = "¿Quieres ser mi amigo?"; 

    public string optionYesLabel = "Zhy ta bien"; 
    public string optionNoLabel = "Nel perro";    

    [Header("Settings")]
    public float initialDelay = 1f;
    public float textReadingTime = 3f; // Time to read each text
    public float fadeOutTime = 1f;

    [Header("Animation Settings")]
    public float optionMoveDistance = 20f; // Distance to move up when selected
    public float optionMoveDuration = 0.2f; // Animation duration
    [Tooltip("Curva para la animación de Slide Up del texto.")]
    public AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Tooltip("Curva para el Fade In del texto.")]
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Tooltip("Altura desde la que sube el texto en unidades de UI.")]
    public float slideDistance = 50f;
    [Tooltip("Duración de la animación de entrada del texto.")]
    public float textEntranceDuration = 1.5f;

    [Header("Ending Sequence")]
    public AudioClip endingSong;
    public float zoomDuration = 2f;
    [Tooltip("CinemachineCamera de la escena (obligatorio para el zoom).")]
    public CinemachineCamera cinemachineCamera;
    [Tooltip("OrthographicSize al hacer zoom in (más chico = más cerca). El valor normal se lee automáticamente.")]
    public float targetZoomSize = 1.5f;

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

        // Store original positions for UI Animation
        if (yesButton != null) _yesOriginalPos = yesButton.GetComponent<RectTransform>().anchoredPosition;
        if (noButton != null) _noOriginalPos = noButton.GetComponent<RectTransform>().anchoredPosition;
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
        if (backgroundMusicSource != null) 
        {
            backgroundMusicSource.loop = true;
            backgroundMusicSource.volume = 0.2f; // "Sonido de fondo muy bajito"
            backgroundMusicSource.Play();
        }

        // Start sequence (Wait for one frame to ensure UI is ready)
        StartCoroutine(IntroSequence());
    }

    private void Update()
    {
        if (_decisionActive)
        {
            HandleInput();
            // Continuously update visuals for smooth animation
            UpdateSelectionVisuals();
        }
    }

    private IEnumerator IntroSequence()
    {
        // 1. Initial Delay
        yield return new WaitForSecondsRealtime(initialDelay);

        // 2. Iterate through all texts
        if (introTexts != null && typewriter != null)
        {
            for (int i = 0; i < introTexts.Length; i++)
            {
                string line = introTexts[i];

                // Stop any previous animations/fades
                if (_textCanvasGroup != null) _textCanvasGroup.alpha = 0f;
                // Clear text so we don't see previous text fading up
                typewriter.ClearText();

                // Reset Pos for entrance
                if (textContainer != null) 
                {
                   textContainer.anchoredPosition = _textContainerOriginalPos;
                }

                // Animate Entrance (Slide Up + Fade In)
                yield return StartCoroutine(AnimateTextEntrance());

                // Typewriter
                bool typingFinished = false;
                typewriter.ShowText(line, () => typingFinished = true);
                
                // Wait for typing
                while (!typingFinished) yield return null;
                
                // Wait for reading time
                yield return new WaitForSecondsRealtime(textReadingTime);

                // Fade Out ONLY if NOT the last text
                if (i < introTexts.Length - 1)
                {
                    yield return StartCoroutine(AnimateTextExit());
                }
            }
        }

        // 3. Show Decision (Keep last text visible)
        ShowDecision();
    }

    private IEnumerator AnimateTextExit()
    {
        if (_textCanvasGroup == null) yield break;
        float timer = 0f;
        while (timer < fadeOutTime)
        {
            float progress = timer / fadeOutTime;
            _textCanvasGroup.alpha = Mathf.Lerp(1f, 0f, progress);
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

        // Setup looping sound for decision phase
        if (backgroundMusicSource != null && decisionLoopClip != null)
        {
            backgroundMusicSource.clip = decisionLoopClip;
            backgroundMusicSource.Play(); // Restart/Play loop
        }

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
            // Reproducir sonido de navegación con AudioSource dedicado
            if (sfxAudioSource != null && navigationSFX != null)
            {
                sfxAudioSource.PlayOneShot(navigationSFX, navigationSFXVolume);
            }
        }
    
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E))
        {
            MakeDecision();
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

        // Sonido de confirmación (botón)
        if (sfxAudioSource != null && confirmSFX != null)
        {
            sfxAudioSource.PlayOneShot(confirmSFX, confirmSFXVolume);
        }
        
        if (_selectedYes)
        {
            TrustManager.Instance.ModifyLC(9f);
        }
        else
        {
            TrustManager.Instance.ModifyLC(-9f);
        }

        // Start Ending Sequence (Song + Blur + Zoom)
        StartCoroutine(EndingSequence());
    }

    private IEnumerator EndingSequence()
    {
        // 1. Reproducir canción de confirmación
        if (backgroundMusicSource != null && endingSong != null)
        {
            backgroundMusicSource.Stop();
            backgroundMusicSource.clip = endingSong;
            backgroundMusicSource.loop = false;
            backgroundMusicSource.Play();
        }

        // 2. Ocultar UI de decisión
        if (decisionPanel != null) decisionPanel.SetActive(false);
        if (_textCanvasGroup != null) _textCanvasGroup.alpha = 0f;

        // 3. Restaurar timeScale ANTES del zoom para que Cinemachine funcione
        Time.timeScale = 1f;

        // 4. Zoom con Cinemachine + Blur
        if (cinemachineCamera != null)
        {
            float startSize = cinemachineCamera.Lens.OrthographicSize;
            float timer = 0f;

            // FASE 1: Overlay se desvanece → revela el mundo BORROSO + Zoom In
            while (timer < zoomDuration)
            {
                float progress = timer / zoomDuration;
                float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

                cinemachineCamera.Lens.OrthographicSize = Mathf.Lerp(startSize, targetZoomSize, smoothProgress);

                if (blackOverlayCanvasGroup != null)
                    blackOverlayCanvasGroup.alpha = Mathf.Lerp(1f, 0f, smoothProgress);

                timer += Time.deltaTime;
                yield return null;
            }
            cinemachineCamera.Lens.OrthographicSize = targetZoomSize;
            if (blackOverlayCanvasGroup != null) blackOverlayCanvasGroup.alpha = 0f;

            // Mantener un momento (borroso y cerca)
            yield return new WaitForSeconds(1f);

            // FASE 2: Blur se limpia + Zoom Out simultáneo
            timer = 0f;
            float totalDuration = Mathf.Max(zoomDuration, blurFadeDuration);
            while (timer < totalDuration)
            {
                // Zoom out
                float zoomProgress = Mathf.Clamp01(timer / zoomDuration);
                float smoothZoom = Mathf.SmoothStep(0f, 1f, zoomProgress);
                cinemachineCamera.Lens.OrthographicSize = Mathf.Lerp(targetZoomSize, startSize, smoothZoom);

                // Blur fade out
                float blurProgress = Mathf.Clamp01(timer / blurFadeDuration);
                if (_dof != null)
                    _dof.gaussianMaxRadius.Override(Mathf.Lerp(maxBlurRadius, 0f, blurProgress));

                timer += Time.deltaTime;
                yield return null;
            }
            cinemachineCamera.Lens.OrthographicSize = startSize;
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

            // Solo blur fade
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

        // 5. Limpiar blur completamente
        if (_dof != null) _dof.gaussianMaxRadius.Override(0f);
        if (_blurVolume != null) Destroy(_blurVolume.gameObject);

        // 6. Desactivar overlay y controller
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
