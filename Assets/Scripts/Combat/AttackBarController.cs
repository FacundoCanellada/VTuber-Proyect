using UnityEngine;
using System.Collections;

namespace UndertaleEncounter
{
    public class AttackBarController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float speed = 10.0f;
        [SerializeField] private float perfectThreshold = 0.5f;
        [SerializeField] private float missThreshold = 8.0f;

        [Header("References")]
        [SerializeField] private SpriteRenderer background;
        [SerializeField] private SpriteRenderer stick;

        private bool _isStopped = false;
        private float _startX;
        private float _endX;
        
        public System.Action<float, bool> OnAttackComplete;

        public void Initialize(float width)
        {
            StopAllCoroutines();
            
            if (background != null && stick != null)
            {
                // No tocamos escalas ni modos de dibujo.
                // Usamos los bounds del mundo para determinar el ancho real visual del fondo
                // y posicionar el stick relativamente a este objeto.
                float worldHalfWidth = background.bounds.size.x / 2f;
                
                _startX = -worldHalfWidth;
                _endX = worldHalfWidth;
                
                stick.transform.localPosition = new Vector3(_startX, 0, 0);
                stick.color = Color.white;
                stick.sortingOrder = 11; 
                stick.gameObject.SetActive(true);
            }

            _isStopped = false;
            StartCoroutine(MovementRoutine());
        }

        private IEnumerator MovementRoutine()
        {
            float t = 0;
            while (!_isStopped)
            {
                float x = Mathf.Lerp(_startX, _endX + 1.0f, t); // Pass a bit further to trigger auto-miss
                stick.transform.localPosition = new Vector3(x, 0, 0);
                
                if (x > _endX)
                {
                    CalculateResult();
                    yield break;
                }

                t += Time.deltaTime * (speed / (_endX - _startX));
                yield return null;
            }
        }

        private void Update()
        {
            if (!_isStopped && InputReader.Instance != null && InputReader.Instance.UI.Confirm.triggered)
            {
                CalculateResult();
            }
        }

        private void CalculateResult()
        {
            _isStopped = true;
            
            float distance = Mathf.Abs(stick.transform.localPosition.x); // Center is 0
            float damageMultiplier = 0;
            bool isPerfect = false;

            if (distance <= perfectThreshold)
            {
                damageMultiplier = 1.5f;
                isPerfect = true;
                StartCoroutine(FlashRoutine(Color.yellow));
            }
            else if (distance <= missThreshold)
            {
                damageMultiplier = 1.0f - (distance / missThreshold);
                damageMultiplier = Mathf.Max(0.2f, damageMultiplier);
                StartCoroutine(FlashRoutine(Color.white));
            }
            else
            {
                damageMultiplier = 0f;
                StartCoroutine(FlashRoutine(Color.gray));
            }

            // Delay for feedback before calling complete
            StartCoroutine(DelayedComplete(damageMultiplier, isPerfect));
        }

        private IEnumerator FlashRoutine(Color color)
        {
            for (int i = 0; i < 4; i++)
            {
                stick.color = color;
                yield return new WaitForSeconds(0.05f);
                stick.color = Color.clear;
                yield return new WaitForSeconds(0.05f);
            }
            stick.gameObject.SetActive(false);
        }

        private IEnumerator DelayedComplete(float multiplier, bool isPerfect)
        {
            yield return new WaitForSeconds(0.5f);
            OnAttackComplete?.Invoke(multiplier, isPerfect);
            gameObject.SetActive(false); // Disable instead of destroy
        }
    }
}
