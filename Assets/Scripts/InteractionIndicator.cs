using UnityEngine;
using TMPro;

namespace VTuberProject.Lighting
{
    /// <summary>
    /// Componente simple para el indicador de interacción
    /// Se puede animar, pulsar, etc.
    /// </summary>
    public class InteractionIndicator : MonoBehaviour
    {
        [Header("Animation")]
        [Tooltip("Animar el indicador")]
        public bool animate = true;
        
        [Tooltip("Tipo de animación")]
        public AnimationType animType = AnimationType.Bounce;
        
        [Tooltip("Velocidad de animación")]
        [Range(0.5f, 5f)]
        public float animSpeed = 2f;
        
        [Tooltip("Amplitud de la animación")]
        [Range(0.1f, 1f)]
        public float animAmplitude = 0.2f;

        [Header("Fade")]
        [Tooltip("Fade in al aparecer")]
        public bool fadeIn = true;
        
        [Tooltip("Duración del fade (segundos)")]
        [Range(0.1f, 2f)]
        public float fadeDuration = 0.3f;

        [Header("References")]
        public CanvasGroup canvasGroup;
        public TextMeshProUGUI textComponent;
        public RectTransform rectTransform;

        public enum AnimationType
        {
            None,
            Bounce,
            Scale,
            Rotate,
            Pulse
        }

        private Vector3 originalPosition;
        private Vector3 originalScale;
        private float time;
        private float fadeProgress;

        private void Awake()
        {
            // Auto-setup components if not assigned
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            if (textComponent == null)
                textComponent = GetComponentInChildren<TextMeshProUGUI>();

            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            if (rectTransform != null)
            {
                originalPosition = rectTransform.localPosition;
                originalScale = rectTransform.localScale;
            }
            
            time = 0f;
            fadeProgress = fadeIn ? 0f : 1f;
            
            if (canvasGroup != null && fadeIn)
            {
                canvasGroup.alpha = 0f;
            }
        }

        private void Update()
        {
            time += Time.deltaTime;
            
            // Handle fade in
            if (fadeIn && fadeProgress < 1f)
            {
                fadeProgress += Time.deltaTime / fadeDuration;
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = Mathf.Clamp01(fadeProgress);
                }
            }

            // Handle animation
            if (animate && rectTransform != null)
            {
                switch (animType)
                {
                    case AnimationType.Bounce:
                        AnimateBounce();
                        break;
                    case AnimationType.Scale:
                        AnimateScale();
                        break;
                    case AnimationType.Rotate:
                        AnimateRotate();
                        break;
                    case AnimationType.Pulse:
                        AnimatePulse();
                        break;
                }
            }
        }

        private void AnimateBounce()
        {
            float y = Mathf.Sin(time * animSpeed) * animAmplitude;
            rectTransform.localPosition = originalPosition + new Vector3(0, y, 0);
        }

        private void AnimateScale()
        {
            float scale = 1f + Mathf.Sin(time * animSpeed) * animAmplitude;
            rectTransform.localScale = originalScale * scale;
        }

        private void AnimateRotate()
        {
            float rotation = Mathf.Sin(time * animSpeed) * (animAmplitude * 30f);
            rectTransform.localRotation = Quaternion.Euler(0, 0, rotation);
        }

        private void AnimatePulse()
        {
            float alpha = 0.5f + Mathf.Sin(time * animSpeed) * 0.5f;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = fadeIn ? Mathf.Min(alpha, fadeProgress) : alpha;
            }
        }

        /// <summary>
        /// Establece el texto del indicador
        /// </summary>
        public void SetText(string text)
        {
            if (textComponent != null)
            {
                textComponent.text = text;
            }
        }

        /// <summary>
        /// Oculta el indicador con fade out
        /// </summary>
        public void Hide(float duration = 0.3f)
        {
            if (canvasGroup != null)
            {
                StartCoroutine(FadeOut(duration));
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private System.Collections.IEnumerator FadeOut(float duration)
        {
            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
                yield return null;
            }
            
            gameObject.SetActive(false);
        }
    }
}
