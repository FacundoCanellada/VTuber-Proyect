using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class DustMoteSpawner : MonoBehaviour
{
    // Script básico para generar partículas de polvo/luz (Dust Motes)
    // Instancia un sistema de partículas preconfigurado si no existe.
    
    [Header("Configuración Principal")]
    [Tooltip("Cantidad de partículas por segundo.")]
    [Range(0, 100)] public float emissionRate = 25f;
    [Tooltip("Color tintado de las partículas.")]
    public Color dustColor = new Color(1f, 1f, 1f, 0.15f);
    [Tooltip("Tamaño máximo de las partículas.")]
    [Range(0.01f, 0.2f)] public float maxSize = 0.05f;

    [Header("Área de Efecto")]
    [Tooltip("Mostrar gizmos para visualizar el área")]
    public bool showGizmos = true;
    
    // Usaremos el Shape Module del propio Particle System en el inspector.
    // Este script solo ayuda a inicializar valores por defecto agradables.
    
    private ParticleSystem _particleSystem;

    void OnValidate()
    {
        // Aplicar cambios en tiempo real en editor
        ApplySettings();
    }

    void Start()
    {
        SetupParticles();
    }

    void SetupParticles()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        
        // Si no está configurado, ponemos valores iniciales decentes.
        // Pero respetamos lo que pongas en el inspector.
        
        var main = _particleSystem.main;
        var emission = _particleSystem.emission;
        var render = _particleSystem.GetComponent<ParticleSystemRenderer>();
        var shape = _particleSystem.shape;

        // Configurar renderizado básico si es nuevo
        if (render.renderMode != ParticleSystemRenderMode.Billboard)
        {
             render.renderMode = ParticleSystemRenderMode.Billboard;
             render.sortMode = ParticleSystemSortMode.Distance; 
        }
        
        // Asegurar loop
        if (!main.loop) main.loop = true;
        
        // Configurar Shape Box si no está
        if (!shape.enabled)
        {
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(5f, 5f, 5f); // Tamaño por defecto pequeño
        }

        ApplySettings();
        
        if (!_particleSystem.isPlaying) _particleSystem.Play();
    }

    void ApplySettings()
    {
        if (_particleSystem == null) _particleSystem = GetComponent<ParticleSystem>();
        if (_particleSystem == null) return;

        var main = _particleSystem.main;
        var emission = _particleSystem.emission;
        var noise = _particleSystem.noise;
        var col = _particleSystem.colorOverLifetime;

        // 1. Movimiento: Velocidad inicial CASI CERO. El polvo no "viaja", flota.
        main.startSpeed = 0.05f; 
        
        // 2. Ruido (Simula turbulencia de aire)
        noise.enabled = true;
        noise.strength = 0.1f; // Suave
        noise.frequency = 0.3f; // Lento cambio de dirección
        noise.scrollSpeed = 0.2f;
        noise.damping = true;

        // 3. Color: Fade In y Fade Out para que no "aparezcan" de golpe
        col.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(dustColor, 0.0f), new GradientColorKey(dustColor, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.0f, 0.0f), new GradientAlphaKey(dustColor.a, 0.5f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        col.color = grad;

        // Colors
        main.startColor = dustColor;
        
        // Size
        main.startSize = new ParticleSystem.MinMaxCurve(maxSize * 0.5f, maxSize);

        // Emission
        emission.rateOverTime = emissionRate;
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        // Dibujar caja basada en el Shape del ParticleSystem
        ParticleSystem ps = GetComponent<ParticleSystem>();
        if (ps != null)
        {
             var shape = ps.shape;
             if (shape.enabled && shape.shapeType == ParticleSystemShapeType.Box)
             {
                 Gizmos.color = new Color(1, 1, 0, 0.2f);
                 Gizmos.matrix = transform.localToWorldMatrix;
                 Gizmos.DrawCube(shape.position, shape.scale);
             }
        }
    }
}
