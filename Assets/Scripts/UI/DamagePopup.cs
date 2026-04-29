using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace UndertaleEncounter
{
    public class DamagePopup : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private Image healthBarBG;
        [SerializeField] private Image healthBarFill;
        [SerializeField] private CanvasGroup canvasGroup;
        
        [Header("Animation Settings")]
        [SerializeField] private float jumpHeight = 30f; 
        [SerializeField] private float duration = 1.0f;

        private Material _healthMaterialInstance;

        private void Awake()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (healthBarFill != null)
            {
                _healthMaterialInstance = new Material(healthBarFill.material);
                healthBarFill.material = _healthMaterialInstance;
            }
        }

        public void Setup(float damage, float currentHp, float maxHp)
        {
            if (damage <= 0)
            {
                damageText.text = "MISS";
                damageText.color = Color.gray;
                if (healthBarBG != null) healthBarBG.gameObject.SetActive(false);
            }
            else
            {
                damageText.text = Mathf.CeilToInt(damage).ToString();
                damageText.color = new Color(1f, 0f, 0f); // Rojo Undertale
                if (healthBarBG != null) healthBarBG.gameObject.SetActive(true);
            }

            StartCoroutine(AnimationRoutine(damage, currentHp, maxHp));
        }

        private IEnumerator AnimationRoutine(float damage, float currentHp, float maxHp)
        {
            float elapsed = 0f;

            // Guardar posición inicial del texto
            Vector3 textStartPos = damageText.rectTransform.localPosition;
            
            // Barra de vida inicial y final
            float startFill = Mathf.Clamp01((currentHp + damage) / maxHp);
            float endFill = Mathf.Clamp01(currentHp / maxHp);
            
            if (_healthMaterialInstance != null)
                _healthMaterialInstance.SetFloat("_health", startFill);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Animación del texto (salto y caída ligera)
                float yOffset = Mathf.Sin(t * Mathf.PI) * jumpHeight;
                damageText.rectTransform.localPosition = textStartPos + new Vector3(0, yOffset, 0);

                // La barra drena
                if (_healthMaterialInstance != null)
                {
                    float drainT = Mathf.Clamp01(t * 2f); // Drena más rápido
                    _healthMaterialInstance.SetFloat("_health", Mathf.Lerp(startFill, endFill, drainT));
                }

                // Desvanecimiento global al final
                if (t > 0.7f)
                {
                    float alphaT = (t - 0.7f) / 0.3f;
                    if (canvasGroup != null) canvasGroup.alpha = 1f - alphaT;
                }

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
