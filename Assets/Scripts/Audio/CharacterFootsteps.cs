using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Sistema de pasos extensible que detecta el tipo de superficie y reproduce sonidos aleatorios.
/// Requisitos: AudioSource, Rigidbody2D y objetos de suelo con SurfaceIdentifier.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class CharacterFootsteps : MonoBehaviour
{
    [Header("Configuración de Intervalos")]
    [Tooltip("Tiempo entre pasos al caminar")]
    [SerializeField] private float walkStepInterval = 0.5f;
    [Tooltip("Tiempo entre pasos al correr")]
    [SerializeField] private float runStepInterval = 0.3f;
    [Tooltip("Velocidad mínima para considerar que se está moviendo")]
    [SerializeField] private float velocityThreshold = 0.1f;
    [Tooltip("Velocidad a partir de la cual se considera correr")]
    [SerializeField] private float runSpeedThreshold = 4.5f;

    [Header("Configuración de Detección")]
    [Tooltip("Offset desde el pivote para detectar el suelo (útil si el pivote es el centro)")]
    [SerializeField] private Vector2 groundCheckOffset = Vector2.zero;
    [Tooltip("Radio de detección del suelo")]
    [SerializeField] private float groundCheckRadius = 0.2f;
    [Tooltip("Capas consideradas como suelo")]
    [SerializeField] private LayerMask groundLayer = ~0; // Por defecto todo

    [Header("Perfiles de Sonido")]
    [Tooltip("Lista de perfiles de sonido para cada superficie")]
    [SerializeField] private List<SurfaceProfile> surfaceProfiles = new List<SurfaceProfile>();
    
    // Referencias
    private AudioSource audioSource;
    private Rigidbody2D rb;
    
    // Estado interno
    private float stepTimer;
    private AudioClip lastClip;
    private Dictionary<SurfaceType, SurfaceProfile> profileDictionary = new Dictionary<SurfaceType, SurfaceProfile>();

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
        
        // Inicializar diccionario para búsqueda rápida
        foreach (var profile in surfaceProfiles)
        {
            if (profile != null && !profileDictionary.ContainsKey(profile.surfaceType))
            {
                profileDictionary.Add(profile.surfaceType, profile);
            }
        }
    }

    private void Update()
    {
        HandleFootsteps();
    }

    private void HandleFootsteps()
    {
        if (rb == null) return;

        // Propiedad compatible con Unity 6 (referencia tomada de PlayerMovementNew.cs)
        // Si usas una versión anterior, cambia 'linearVelocity' por 'velocity'
        float speed = rb.linearVelocity.magnitude;

        // Si no nos movemos lo suficiente, reseteamos el temporizador para que el próximo paso suene inmediato
        if (speed < velocityThreshold)
        {
            stepTimer = walkStepInterval; // forzar primer paso inmediato al arrancar
            return;
        }

        bool isRunning = speed > runSpeedThreshold;
        float currentInterval = isRunning ? runStepInterval : walkStepInterval;

        stepTimer += Time.deltaTime;

        if (stepTimer >= currentInterval)
        {
            PlayFootstep(isRunning);
            stepTimer = 0f;
        }
    }

    private void PlayFootstep(bool isRunning)
    {
        SurfaceType currentSurface = DetectSurface();

        if (profileDictionary.TryGetValue(currentSurface, out SurfaceProfile profile))
        {
            AudioClip[] clips = isRunning ? profile.runClips : profile.walkClips;

            if (clips != null && clips.Length > 0)
            {
                // Selección aleatoria evitando repetición inmediata si es posible
                AudioClip clip = GetRandomClip(clips);
                
                if (clip != null)
                {
                    // Variación natural de tono (pitch) y volumen
                    audioSource.pitch = Random.Range(0.95f, 1.05f); 
                    audioSource.volume = Random.Range(0.8f, 1.0f);
                    
                    audioSource.PlayOneShot(clip);
                    lastClip = clip;
                }
            }
        }
    }

    private AudioClip GetRandomClip(AudioClip[] clips)
    {
        if (clips.Length == 0) return null;
        if (clips.Length == 1) return clips[0];

        int attempts = 3;
        AudioClip newClip = clips[Random.Range(0, clips.Length)];

        // Intentar buscar uno diferente al anterior
        while (newClip == lastClip && attempts > 0)
        {
            newClip = clips[Random.Range(0, clips.Length)];
            attempts--;
        }

        return newClip;
    }

    private SurfaceType DetectSurface()
    {
        Vector2 checkPosition = (Vector2)transform.position + groundCheckOffset;
        Collider2D[] hits = Physics2D.OverlapCircleAll(checkPosition, groundCheckRadius, groundLayer);

        foreach (var hit in hits)
        {
            // Buscamos el componente SurfaceIdentifier en el objeto colisionado
            SurfaceIdentifier surface = hit.GetComponent<SurfaceIdentifier>();
            if (surface != null)
            {
                return surface.surfaceType;
            }
        }

        // Si no encontramos nada específico, devolvemos Standard
        return SurfaceType.Standard;
    }

    // Dibujar gizmo para visualizar la detección de suelo
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector2 checkPosition = (Vector2)transform.position + groundCheckOffset;
        Gizmos.DrawWireSphere(checkPosition, groundCheckRadius);
    }
}
