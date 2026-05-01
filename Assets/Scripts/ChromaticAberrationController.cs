using UnityEngine;

/// <summary>
/// Controlador del efecto de Chromatic Aberration (aberración cromática).
/// Permite animar la intensidad del efecto en tiempo real asignando
/// el material del shader Custom/ChromaticAberration.
/// </summary>
public class ChromaticAberrationController : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Material creado a partir del shader Custom/ChromaticAberration")]
    [SerializeField] private Material chromaticAberrationMaterial;

    [Header("Intensidad")]
    [Tooltip("Intensidad base del efecto (0 = sin efecto, 0.05 = máximo)")]
    [SerializeField, Range(0f, 0.05f)] private float intensity = 0.005f;

    [Header("Pulso (opcional)")]
    [Tooltip("Animar la intensidad con un pulso sinusoidal")]
    [SerializeField] private bool enablePulse = false;
    [Tooltip("Intensidad mínima durante el pulso")]
    [SerializeField, Range(0f, 0.05f)] private float pulseMin = 0f;
    [Tooltip("Intensidad máxima durante el pulso")]
    [SerializeField, Range(0f, 0.05f)] private float pulseMax = 0.01f;
    [Tooltip("Velocidad del pulso en ciclos por segundo")]
    [SerializeField] private float pulseSpeed = 1f;

    private static readonly int IntensityProperty = Shader.PropertyToID("_Intensity");

    private void OnValidate()
    {
        ApplyIntensity(intensity);
    }

    private void Update()
    {
        if (chromaticAberrationMaterial == null) return;

        float currentIntensity = intensity;

        if (enablePulse)
        {
            float t = (Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
            currentIntensity = Mathf.Lerp(pulseMin, pulseMax, t);
        }

        ApplyIntensity(currentIntensity);
    }

    /// <summary>
    /// Cambia la intensidad del efecto desde otros scripts.
    /// </summary>
    public void SetIntensity(float value)
    {
        intensity = Mathf.Clamp(value, 0f, 0.05f);
        ApplyIntensity(intensity);
    }

    private void ApplyIntensity(float value)
    {
        if (chromaticAberrationMaterial != null)
            chromaticAberrationMaterial.SetFloat(IntensityProperty, value);
    }
}
