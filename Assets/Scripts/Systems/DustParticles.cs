using UnityEngine;

/// <summary>
/// Genera partículas de polvo flotante visibles donde entra la luz.
/// Usar con un ParticleSystem en el GameObject.
/// Posicionar el GameObject donde esté la ventana/luz.
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class DustParticles : MonoBehaviour
{
    [Header("Partículas")]
    [Tooltip("Cantidad de partículas por segundo.")]
    [Range(1, 80)] public int emissionRate = 20;
    [Tooltip("Tamaño mínimo de las motas de polvo (en unidades world).")]
    [Range(0.005f, 0.1f)] public float minSize = 0.01f;
    [Tooltip("Tamaño máximo de las motas de polvo.")]
    [Range(0.01f, 0.15f)] public float maxSize = 0.04f;
    [Tooltip("Tiempo de vida mínimo de cada partícula (segundos).")]
    public float minLifetime = 4f;
    [Tooltip("Tiempo de vida máximo de cada partícula (segundos).")]
    public float maxLifetime = 8f;

    [Header("Movimiento")]
    [Tooltip("Velocidad de deriva lenta de las partículas.")]
    [Range(0f, 0.1f)] public float driftSpeed = 0.02f;
    [Tooltip("Intensidad de la turbulencia (movimiento errático natural).")]
    [Range(0f, 0.5f)] public float turbulenceStrength = 0.08f;
    [Tooltip("Frecuencia de cambio de dirección del aire.")]
    [Range(0.05f, 1f)] public float turbulenceFrequency = 0.15f;
    [Tooltip("Gravedad sutil (positivo = sube, negativo = baja). El polvo real flota casi en el aire.")]
    [Range(-0.02f, 0.02f)] public float gravity = 0.005f;

    [Header("Apariencia")]
    [Tooltip("Color base de las partículas. Usa blanco con alpha bajo para polvo realista.")]
    public Color dustColor = new Color(1f, 1f, 0.95f, 0.3f);
    [Tooltip("Las partículas parpadean/brillan sutilmente al pasar por la luz.")]
    public bool enableTwinkle = true;

    [Header("Área de Spawn (Shape Box)")]
    [Tooltip("Tamaño del área donde aparecen las partículas (XY para 2D).")]
    public Vector3 spawnAreaSize = new Vector3(3f, 4f, 0.1f);

    private ParticleSystem _ps;

    private void Start()
    {
        _ps = GetComponent<ParticleSystem>();
        ConfigureParticleSystem();
    }

    private void OnValidate()
    {
        if (_ps == null) _ps = GetComponent<ParticleSystem>();
        if (_ps != null) ConfigureParticleSystem();
    }

    private void ConfigureParticleSystem()
    {
        // Main Module
        var main = _ps.main;
        main.loop = true;
        main.playOnAwake = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(minLifetime, maxLifetime);
        main.startSpeed = driftSpeed;
        main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
        main.startColor = dustColor;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.gravityModifier = gravity;
        main.maxParticles = 200;

        // Emission
        var emission = _ps.emission;
        emission.enabled = true;
        emission.rateOverTime = emissionRate;

        // Shape - Box para zona rectangular de luz
        var shape = _ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = spawnAreaSize;

        // Noise Module - Turbulencia natural del aire
        var noise = _ps.noise;
        noise.enabled = true;
        noise.strength = turbulenceStrength;
        noise.frequency = turbulenceFrequency;
        noise.scrollSpeed = 0.1f;
        noise.damping = true;
        noise.octaveCount = 2;
        noise.quality = ParticleSystemNoiseQuality.Medium;

        // Color over Lifetime - Fade in/out suave
        var col = _ps.colorOverLifetime;
        col.enabled = true;
        Gradient grad = new Gradient();
        
        if (enableTwinkle)
        {
            // Fade in → brillo pico → suave bajada → brillo secundario → fade out
            grad.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(dustColor.a, 0.15f),
                    new GradientAlphaKey(dustColor.a * 0.4f, 0.5f),
                    new GradientAlphaKey(dustColor.a * 0.8f, 0.7f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
        }
        else
        {
            // Fade in/out simple
            grad.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(dustColor.a, 0.2f),
                    new GradientAlphaKey(dustColor.a, 0.8f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
        }
        col.color = grad;

        // Size over Lifetime - Variación sutil
        var sol = _ps.sizeOverLifetime;
        sol.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0.8f);
        sizeCurve.AddKey(0.5f, 1f);
        sizeCurve.AddKey(1f, 0.6f);
        sol.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // Renderer
        var renderer = _ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortMode = ParticleSystemSortMode.Distance;
        // Material: usa el Default-Particle o cualquier sprite blanco cuadrado asignado en Inspector

        if (!_ps.isPlaying) _ps.Play();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.9f, 0.5f, 0.25f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, spawnAreaSize);
        Gizmos.color = new Color(1f, 0.9f, 0.5f, 0.08f);
        Gizmos.DrawCube(Vector3.zero, spawnAreaSize);
    }
}
