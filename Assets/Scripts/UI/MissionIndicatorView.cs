using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Singleton que muestra indicadores de misión/tarea en pantalla.
///
/// CONFIGURACIÓN:
/// 1. Agrega este componente a un GameObject dentro del Canvas (ej. "MissionIndicatorRoot").
/// 2. Asigna en el Inspector: panelRoot, canvasGroup, titleText, descriptionText, iconImage, audioSource.
/// 3. Posiciona el panel donde quieras que descanse en pantalla (posición final visible).
///    El sistema calcula la posición fuera de pantalla a la izquierda automáticamente.
/// 4. En cualquier script, llama: MissionIndicatorView.Instance?.Show(miMissionIndicatorData);
/// </summary>
public class MissionIndicatorView : MonoBehaviour
{
    public static MissionIndicatorView Instance { get; private set; }

    [Header("Referencias UI — Asignar desde el Inspector")]
    [Tooltip("RectTransform del panel completo del indicador.")]
    [SerializeField] private RectTransform panelRoot;
    [Tooltip("CanvasGroup del panel (para controlar alpha). Puede ser el mismo GameObject que panelRoot.")]
    [SerializeField] private CanvasGroup canvasGroup;
    [Tooltip("Texto del título de la tarea (ej. 'Nueva Tarea').")]
    [SerializeField] private TMP_Text titleText;
    [Tooltip("Texto de la descripción de la tarea.")]
    [SerializeField] private TMP_Text descriptionText;
    [Tooltip("Imagen del icono decorativo. Opcional.")]
    [SerializeField] private Image iconImage;
    [Tooltip("AudioSource para el sonido de aparición. Opcional.")]
    [SerializeField] private AudioSource audioSource;

    [Header("Animación")]
    [Tooltip("Duración del deslizamiento de entrada (izquierda → posición final).")]
    [SerializeField] private float slideInDuration = 0.35f;
    [Tooltip("Duración del fade-in junto con el slide-in.")]
    [SerializeField] private float fadeInDuration = 0.2f;
    [Tooltip("Cuántos segundos permanece visible el indicador antes de empezar a desaparecer.")]
    [SerializeField] private float holdDuration = 3.5f;
    [Tooltip("Duración del deslizamiento de salida (posición final → izquierda) + fade-out.")]
    [SerializeField] private float slideOutDuration = 0.3f;
    [Tooltip("Distancia en X desde la posición final hasta donde empieza/termina fuera de pantalla.")]
    [SerializeField] private float slideOffsetX = 700f;

    private Vector2 _restingPos;
    private bool _posCached;
    private Coroutine _animCoroutine;

    // ─────────────────────────── LIFECYCLE ───────────────────────────────

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

        CacheRestingPosition();
        HideInstant();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // ──────────────────────────── PUBLIC API ─────────────────────────────

    /// <summary>
    /// Muestra el indicador con los datos provistos. Si ya hay uno visible, lo reemplaza.
    /// Llamar desde cualquier parte del juego: MissionIndicatorView.Instance?.Show(data);
    /// </summary>
    public void Show(MissionIndicatorData data)
    {
        if (data == null)
            return;

        if (panelRoot == null || canvasGroup == null)
        {
            Debug.LogWarning("[MissionIndicatorView] Faltan referencias en el Inspector (panelRoot / canvasGroup).");
            return;
        }

        CacheRestingPosition();

        // Llenar contenido
        if (titleText != null)
            titleText.text = data.title;

        if (descriptionText != null)
            descriptionText.text = data.description;

        if (iconImage != null)
        {
            iconImage.sprite = data.icon;
            iconImage.gameObject.SetActive(data.icon != null);
        }

        // Sonido
        if (data.showSound != null && audioSource != null)
            audioSource.PlayOneShot(data.showSound);

        // Activar el panel antes de la coroutine (puede estar inactivo entre animaciones).
        panelRoot.gameObject.SetActive(true);

        // Arrancar animación (cancela la anterior si había)
        if (_animCoroutine != null)
            StopCoroutine(_animCoroutine);

        _animCoroutine = StartCoroutine(LifecycleAnimation());
    }

    /// <summary>
    /// Oculta el indicador inmediatamente (sin animación).
    /// </summary>
    public void HideImmediate()
    {
        if (_animCoroutine != null)
        {
            StopCoroutine(_animCoroutine);
            _animCoroutine = null;
        }
        HideInstant();
    }

    // ───────────────────────────── ANIMATION ──────────────────────────────

    private IEnumerator LifecycleAnimation()
    {
        Vector2 offscreenPos = _restingPos - new Vector2(slideOffsetX, 0f);

        // ── FASE 1: SLIDE IN de izquierda a derecha + fade in ──
        panelRoot.anchoredPosition = offscreenPos;
        canvasGroup.alpha = 0f;
        panelRoot.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < slideInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / slideInDuration);
            float easePos = EaseOutCubic(t);
            float easeAlpha = Mathf.Clamp01(elapsed / Mathf.Max(fadeInDuration, 0.01f));

            panelRoot.anchoredPosition = Vector2.Lerp(offscreenPos, _restingPos, easePos);
            canvasGroup.alpha = easeAlpha;
            yield return null;
        }

        panelRoot.anchoredPosition = _restingPos;
        canvasGroup.alpha = 1f;

        // ── FASE 2: HOLD ──
        if (holdDuration > 0f)
            yield return new WaitForSecondsRealtime(holdDuration);

        // ── FASE 3: SLIDE OUT hacia la izquierda + fade out ──
        elapsed = 0f;
        while (elapsed < slideOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / slideOutDuration);
            float ease = EaseInCubic(t);

            panelRoot.anchoredPosition = Vector2.Lerp(_restingPos, offscreenPos, ease);
            canvasGroup.alpha = 1f - ease;
            yield return null;
        }

        HideInstant();
        _animCoroutine = null;
    }

    private void HideInstant()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        if (panelRoot != null)
        {
            if (_posCached)
                panelRoot.anchoredPosition = _restingPos - new Vector2(slideOffsetX, 0f);
            panelRoot.gameObject.SetActive(false);
        }
    }

    private void CacheRestingPosition()
    {
        if (_posCached || panelRoot == null)
            return;

        _restingPos = panelRoot.anchoredPosition;
        _posCached = true;
    }

    // ───────────────────────── EASING ─────────────────────────────────────

    private static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
    private static float EaseInCubic(float t) => t * t * t;
}
