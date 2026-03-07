using System.Collections;
using TMPro;
using UnityEngine;
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
    [Tooltip("Sonido de fondo (loop) para la decisión.")]
    public AudioClip decisionLoopClip;
    [Tooltip("Sonido al cambiar de opción (Navegación UI).")]
    public AudioClip navigationSFX;

    [Header("Dialogues")]
    [TextArea] public string introDialogueText = "¿Quieres ser mi amigo?";
    public string optionYesLabel = "Zhy ta bien"; // Label for the Positive choice
    public string optionNoLabel = "Nel perro";    // Label for the Negative choice

    [Header("Settings")]
    public float initialDelay = 1f;
    public float decisionAppearanceTime = 15f;
    [Tooltip("Curva para la animación de Slide Up del texto.")]
    public AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Tooltip("Curva para el Fade In del texto.")]
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Tooltip("Altura desde la que sube el texto en unidades de UI.")]
    public float slideDistance = 50f;
    [Tooltip("Duración de la animación de entrada del texto.")]
    public float textEntranceDuration = 1.5f;

    [Header("Colors")]
    public Color selectedColor = Color.cyan;
    public Color normalColor = Color.white;

    [Header("Game Control")]
    [Tooltip("El Panel Negro que cubre toda la pantalla.")]
    public CanvasGroup blackOverlayCanvasGroup;

    private bool _decisionActive = false;
    private bool _selectedYes = true; // Default selection (e.g. Yes on right/left?)

    private void Awake()
    {
        // 0. Immediate Setup (Antes de que nada se renderice si es posible)
        if (blackOverlayCanvasGroup != null) 
        {
            blackOverlayCanvasGroup.alpha = 1f; // Pantalla negra TOTAL
            blackOverlayCanvasGroup.gameObject.SetActive(true);
        }

        // Setup initial text fading (CanvasGroup)
        if (textContainer != null)
        {
            _textCanvasGroup = textContainer.GetComponent<CanvasGroup>();
            // If it doesn't exist, we add it automatically for the Fade effect
            if (_textCanvasGroup == null) _textCanvasGroup = textContainer.gameObject.AddComponent<CanvasGroup>();
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
        }
    }

    private IEnumerator IntroSequence()
    {
        // 1. Wait standard 1 second delay (Realtime because TimeScale is 0)
        yield return new WaitForSecondsRealtime(initialDelay);

        // 2. Animate Text Entrance (Slide Up + Fade In) (Using unscaled time)
        StartCoroutine(AnimateTextEntrance());

        // 3. Start Typing
        // "En el segundo 1 aparece texto... Voz de Ahiiruw..."
        if (typewriter != null)
        {
            typewriter.ShowText(introDialogueText);
        }

        // 4. Wait until the decision time arrives
        float timeRemaining = decisionAppearanceTime - initialDelay;
        if (timeRemaining > 0) yield return new WaitForSecondsRealtime(timeRemaining);

        // 5. Show Decision
        ShowDecision();
    }

    private IEnumerator AnimateTextEntrance()
    {
        if (textContainer == null) yield break;

        Vector2 endPos = textContainer.anchoredPosition;
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
            // Reproducir sonido de navegación
            if (backgroundMusicSource != null && navigationSFX != null)
            {
                backgroundMusicSource.PlayOneShot(navigationSFX);
            }
        }
    
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E))
        {
            MakeDecision();
        }
    }

    private void UpdateSelectionVisuals()
    {
        // Solicitud del usuario: Solo cambiar el color del TEXTO.
        // Aseguramos que el Alpha sea 1 (visible) siempre.
        
        if (yesOptionText != null)
        {
            Color targetColor = _selectedYes ? selectedColor : normalColor;
            targetColor.a = 1f; // Force Alpha to 1
            yesOptionText.color = targetColor;
        }

        if (noOptionText != null)
        {
            Color targetColor = !_selectedYes ? selectedColor : normalColor;
            targetColor.a = 1f; // Force Alpha to 1
            noOptionText.color = targetColor;
        }
    }

    private void MakeDecision()
    {
        _decisionActive = false;
        
        if (_selectedYes)
        {
            // "zhy ta bien" -> LC +9% (43 -> 52%)
           TrustManager.Instance.ModifyLC(9f);
           Debug.Log("Jugador aceptó la amistad: Ahiiruw se vuelve más activa.");
        }
        else
        {
            // "nel perro" -> LC -9% (43 -> 34%)
            TrustManager.Instance.ModifyLC(-9f);
            Debug.Log("Jugador rechazó la amistad: Relación distante inicial.");
        }

        // Proceed to end intro sequence
        StartCoroutine(EndIntroSequence());
    }

    private IEnumerator EndIntroSequence()
    {
        // 1. Hide decision panel immediately
        if (decisionPanel != null) decisionPanel.SetActive(false);

        // 2. Hide intro text immediately
        if (_textCanvasGroup != null) 
        {
            _textCanvasGroup.alpha = 0f;
        }
        else if (textContainer != null)
        {
             // Si por alguna razón no hay canvas group, desactiva el objeto
             textContainer.gameObject.SetActive(false);
        }

        // 3. Fade Out Black Overlay
        if (blackOverlayCanvasGroup != null)
        {
            float timer = 0f;
            float duration = 2.0f; // 2 seconds fade out
            while (timer < duration)
            {
                float progress = timer / duration;
                // Fade from 1 (black) to 0 (transparent)
                blackOverlayCanvasGroup.alpha = Mathf.Lerp(1f, 0f, progress);
                // Also fade out intro music
                if (backgroundMusicSource != null) backgroundMusicSource.volume = Mathf.Lerp(0.2f, 0f, progress);

                timer += Time.unscaledDeltaTime; // Use unscaled time
                yield return null;
            }
            blackOverlayCanvasGroup.alpha = 0f;
            blackOverlayCanvasGroup.gameObject.SetActive(false);
        }

        // 4. Enable Gameplay Logic (Resume Time)
        Time.timeScale = 1f;

        // 5. Destroy IntroController or just disable.
        this.enabled = false; 
    }
}
