using UnityEngine;
using System.Collections;

namespace UndertaleEncounter
{
    public class ProjectileBase : MonoBehaviour
    {
        public enum State { NONE, SPAWNING, WAITING, WARNING, SHOOTING }

        [SerializeField] private float damage = 10.0f;
        [SerializeField] private Vector2 velocity = Vector2.zero;
        [SerializeField] private bool rotateTowardsVelocity = false;
        [SerializeField] private bool destroyOnHit = false;
        [SerializeField] private float maxDistance = 12.0f; // Unity meters

        private float _distanceTraveled = 0.0f;
        private bool _isFadingOut = false;
        private State _currentState = State.NONE;
        private AttackWaveData _data;
        private float _stateTimer = 0.0f;
        private Vector2 _originalVelocity = Vector2.zero;
        private float _timeLived = 0.0f;
        private Vector2 _initialPosition;
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void InjectData(AttackWaveData waveData)
        {
            _data = waveData;
            _initialPosition = transform.position;

            if (_data.spawnAnim != AttackWaveData.SpawnAnim.NONE)
            {
                _currentState = State.SPAWNING;
                _stateTimer = _data.spawnAnimDuration;
                StartCoroutine(PlaySpawnAnimRoutine());
            }
            else if (_data.waitDuration > 0)
            {
                _currentState = State.WAITING;
                _stateTimer = _data.waitDuration - _data.warningDuration;
            }
            else
            {
                StartShooting();
            }
        }

        private IEnumerator PlaySpawnAnimRoutine()
        {
            float elapsed = 0;
            Color c = _spriteRenderer.color;
            c.a = 0;
            _spriteRenderer.color = c;

            float initialRot = transform.eulerAngles.z + (360f * _data.spawnExtraSpins);
            float targetAngle = GetAimAngle();

            while (elapsed < _data.spawnAnimDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _data.spawnAnimDuration;
                
                // Fade
                c.a = t;
                _spriteRenderer.color = c;

                // Rotation if needed
                if (_data.spawnAnim == AttackWaveData.SpawnAnim.FADE_IN_AND_SPIN)
                {
                    // Ease Out Cubic approx: 1 - (1-t)^3
                    float easedT = 1f - Mathf.Pow(1f - t, 3f);
                    transform.eulerAngles = new Vector3(0, 0, Mathf.LerpUnclamped(initialRot, targetAngle, easedT));
                }
                yield return null;
            }
            
            c.a = 1;
            _spriteRenderer.color = c;
        }

        private float GetAimAngle()
        {
            if (_data == null) return transform.eulerAngles.z;

            switch (_data.aimType)
            {
                case AttackWaveData.AimType.FIXED_DIRECTION:
                    return Vector2.SignedAngle(Vector2.right, _data.fixedDirection);
                case AttackWaveData.AimType.AIM_AT_SOUL:
                    var soul = GameObject.FindWithTag("Soul");
                    if (soul) return Vector2.SignedAngle(Vector2.right, (soul.transform.position - transform.position).normalized);
                    break;
                case AttackWaveData.AimType.AIM_AT_CENTER:
                    // TODO: Reference BattleBox center
                    break;
            }
            return transform.eulerAngles.z;
        }

        private void Update()
        {
            if (_currentState != State.NONE && _currentState != State.SHOOTING)
            {
                _stateTimer -= Time.deltaTime;

                if (_currentState == State.SPAWNING)
                {
                    if (_stateTimer <= 0)
                    {
                        if (_data.waitDuration > 0)
                        {
                            _currentState = State.WAITING;
                            _stateTimer = _data.waitDuration - _data.warningDuration;
                        }
                        else StartShooting();
                    }
                }
                else if (_currentState == State.WAITING)
                {
                    UpdateWaitBehavior();
                    if (_stateTimer <= 0)
                    {
                        if (_data.warningDuration > 0)
                        {
                            _currentState = State.WARNING;
                            _stateTimer = _data.warningDuration;
                            StartCoroutine(PlayWarningAnimRoutine());
                        }
                        else StartShooting();
                    }
                }
                else if (_currentState == State.WARNING)
                {
                    UpdateWaitBehavior();
                    if (_stateTimer <= 0) StartShooting();
                }
                return;
            }

            // Shooting logic
            if (_currentState == State.SHOOTING)
            {
                _timeLived += Time.deltaTime;
                UpdateAdvancedMovement();

                if (velocity != Vector2.zero)
                {
                    Vector2 step = velocity * Time.deltaTime;
                    transform.Translate(step, Space.World);
                    _distanceTraveled += step.magnitude;
                }
                else if (_data && _data.moveType == AttackWaveData.MoveType.SINE_WAVE)
                {
                    _distanceTraveled += (_originalVelocity * Time.deltaTime).magnitude;
                }

                if (rotateTowardsVelocity && velocity != Vector2.zero)
                {
                    float angle = Vector2.SignedAngle(Vector2.right, velocity);
                    transform.eulerAngles = new Vector3(0, 0, angle);
                }

                if (_distanceTraveled >= maxDistance && !_isFadingOut)
                {
                    _isFadingOut = true;
                    StartCoroutine(FadeOutAndDestroy());
                }
            }
        }

        private IEnumerator FadeOutAndDestroy()
        {
            float elapsed = 0;
            float duration = 0.3f;
            Color c = _spriteRenderer.color;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                c.a = 1f - (elapsed / duration);
                _spriteRenderer.color = c;
                yield return null;
            }
            Destroy(gameObject);
        }

        private void UpdateWaitBehavior()
        {
            if (_data.waitBehavior == AttackWaveData.WaitBehavior.LOOK_AT_SOUL || _data.waitBehavior == AttackWaveData.WaitBehavior.LOOK_AT_CENTER)
            {
                float targetAngle = GetAimAngle();
                float currentAngle = transform.eulerAngles.z;
                transform.eulerAngles = new Vector3(0, 0, Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * 10.0f));
            }
        }

        private IEnumerator PlayWarningAnimRoutine()
        {
            int loops = 6;
            float step = _data.warningDuration / loops;
            for (int i = 0; i < loops; i++)
            {
                _spriteRenderer.color = (i % 2 == 0) ? _data.warningColor1 : _data.warningColor2;
                yield return new WaitForSeconds(step);
            }
            _spriteRenderer.color = Color.white;
        }

        private void StartShooting()
        {
            _currentState = State.SHOOTING;
            _spriteRenderer.color = Color.white;

            float angleRad = GetAimAngle() * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
            velocity = dir * _data.moveSpeed;
            _originalVelocity = velocity;
            rotateTowardsVelocity = _data.rotateTowardsVelocity;
            _initialPosition = transform.position;
        }

        private void UpdateAdvancedMovement()
        {
            if (_data == null) return;

            switch (_data.moveType)
            {
                case AttackWaveData.MoveType.HOMING:
                    var soul = GameObject.FindWithTag("Soul");
                    if (soul)
                    {
                        Vector2 targetDir = (soul.transform.position - transform.position).normalized;
                        Vector2 currentDir = velocity.normalized;
                        Vector2 newDir = Vector2.MoveTowards(currentDir, targetDir, _data.homingTurnSpeed * Time.deltaTime).normalized;
                        velocity = newDir * _data.moveSpeed;
                    }
                    break;
                case AttackWaveData.MoveType.SINE_WAVE:
                    Vector2 perp = new Vector2(-_originalVelocity.y, _originalVelocity.x).normalized;
                    Vector2 sineOffset = perp * Mathf.Sin(_timeLived * _data.sineFrequency) * _data.sineAmplitude;
                    Vector2 linearPos = _initialPosition + _originalVelocity * _timeLived;
                    transform.position = linearPos + sineOffset;
                    velocity = Vector2.zero;
                    break;
                case AttackWaveData.MoveType.BOOMERANG:
                    velocity = Vector2.MoveTowards(velocity, -_originalVelocity, _data.boomerangAcceleration * Time.deltaTime);
                    break;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Soul"))
            {
                var soul = other.GetComponent<SoulController>();
                if (soul)
                {
                    soul.TakeDamage(damage);
                    if (destroyOnHit) Destroy(gameObject);
                }
            }
        }
    }
}
