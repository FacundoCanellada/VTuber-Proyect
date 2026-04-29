using UnityEngine;
using System.Collections;

namespace UndertaleEncounter
{
    public class SoulController : MonoBehaviour
    {
        [SerializeField] private float iFramesDuration = 1.0f;
        [SerializeField] private SoundData damageSound;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Vector2 soulSize = new Vector2(0.16f, 0.16f);

        private SoulModeStrategy _currentStrategy;
        private SoulMode _currentMode;
        private float _iFramesTimer = 0.0f;
        private bool _isInvulnerable = false;
        private Color _originalColor = Color.white;

        private void Start()
        {
            SetMode(SoulMode.RED_FREE_MOVE);
        }

        public void SetMode(SoulMode mode)
        {
            _currentMode = mode;
            switch (mode)
            {
                case SoulMode.RED_FREE_MOVE:
                    _currentStrategy = new RedMovementStrategy(this);
                    break;
                default:
                    _currentStrategy = new RedMovementStrategy(this);
                    break;
            }

            if (spriteRenderer != null && _currentStrategy != null)
            {
                _originalColor = _currentStrategy.GetColor();
                if (!_isInvulnerable)
                {
                    spriteRenderer.color = _originalColor;
                }
            }
        }

        private void Update()
        {
            if (_currentStrategy != null)
            {
                Vector2 velocity = _currentStrategy.ProcessMovement(Time.deltaTime);
                transform.Translate(velocity * Time.deltaTime);
                ClampToBox();
            }

            if (_isInvulnerable)
            {
                _iFramesTimer -= Time.deltaTime;
                
                // Flicker effect
                float alpha = (Mathf.Sin(_iFramesTimer * 30.0f) + 1.0f) / 2.0f;
                Color c = _originalColor;
                c.a = Mathf.Lerp(0.2f, 1.0f, alpha);
                spriteRenderer.color = c;

                if (_iFramesTimer <= 0)
                {
                    _isInvulnerable = false;
                    spriteRenderer.color = _originalColor;
                }
            }
        }

        private void ClampToBox()
        {
            if (BattleBox.Instance == null) return;

            Rect bounds = BattleBox.Instance.GetInnerBounds();
            Vector3 pos = transform.position;

            float halfWidth = soulSize.x / 2f;
            float halfHeight = soulSize.y / 2f;

            pos.x = Mathf.Clamp(pos.x, bounds.xMin + halfWidth, bounds.xMax - halfWidth);
            pos.y = Mathf.Clamp(pos.y, bounds.yMin + halfHeight, bounds.yMax - halfHeight);

            transform.position = pos;
        }

        public void TakeDamage(float amount)
        {
            if (_isInvulnerable) return;

            Debug.Log("Soul took damage: " + amount);

            if (damageSound != null)
            {
                // TODO: AudioManager.PlayGlobal(damageSound);
            }

            BattleManager.Instance.DamagePlayer(amount);

            _isInvulnerable = true;
            _iFramesTimer = iFramesDuration;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Projectile collision will be handled here or by the projectile
        }
    }
}
