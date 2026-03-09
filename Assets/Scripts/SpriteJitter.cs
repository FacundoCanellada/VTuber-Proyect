using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Efecto estilo Undertale: al presionar W/S (o Up/Down) rápidamente,
/// el sprite se "bugea" cambiando entre los frames de arriba/abajo rápidamente.
/// Solo alterna el parámetro MoveY del Animator. No toca posiciones.
/// </summary>
public class SpriteJitter : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Tiempo máximo entre pulsaciones opuestas para activar el jitter (segundos).")]
    [SerializeField] private float inputWindow = 0.15f;
    [Tooltip("Duración del efecto de jitter una vez activado (segundos).")]
    [SerializeField] private float jitterDuration = 0.25f;
    [Tooltip("Intervalo entre cambios de sprite durante el jitter (segundos).")]
    [SerializeField] private float flickInterval = 0.03f;

    private Animator _animator;

    private float _lastUpTime = -1f;
    private float _lastDownTime = -1f;
    private bool _lastWasUp;

    private float _jitterTimer;
    private float _flickTimer;
    private bool _isJittering;

    private int _moveYParamID;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _moveYParamID = Animator.StringToHash("MoveY");
    }

    private void LateUpdate()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame)
        {
            float timeSinceDown = Time.time - _lastDownTime;
            if (!_lastWasUp && timeSinceDown < inputWindow && timeSinceDown > 0f)
                TriggerJitter();
            _lastUpTime = Time.time;
            _lastWasUp = true;
        }

        if (keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame)
        {
            float timeSinceUp = Time.time - _lastUpTime;
            if (_lastWasUp && timeSinceUp < inputWindow && timeSinceUp > 0f)
                TriggerJitter();
            _lastDownTime = Time.time;
            _lastWasUp = false;
        }

        if (_isJittering)
        {
            _jitterTimer -= Time.deltaTime;
            _flickTimer -= Time.deltaTime;

            if (_flickTimer <= 0f)
            {
                _flickTimer = flickInterval;
                float currentY = _animator.GetFloat(_moveYParamID);
                _animator.SetFloat(_moveYParamID, currentY > 0 ? -1f : 1f);
            }

            if (_jitterTimer <= 0f)
                _isJittering = false;
        }
    }

    private void TriggerJitter()
    {
        _isJittering = true;
        _jitterTimer = jitterDuration;
        _flickTimer = 0f;
    }
}
