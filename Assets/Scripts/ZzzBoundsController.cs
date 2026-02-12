using UnityEngine;

/// <summary>
/// Controla que el efecto Zzz no sobrepase los límites definidos (ej: paredes de la casa)
/// </summary>
public class ZzzBoundsController : MonoBehaviour
{
    [Header("Límites de Renderizado")]
    [Tooltip("Límite izquierdo (X mínimo) donde pueden aparecer las Zzz")]
    [SerializeField] private float minX = -10f;
    
    [Tooltip("Límite derecho (X máximo) donde pueden aparecer las Zzz")]
    [SerializeField] private float maxX = 10f;
    
    [Tooltip("Límite inferior (Y mínimo) donde pueden aparecer las Zzz")]
    [SerializeField] private float minY = -5f;
    
    [Tooltip("Límite superior (Y máximo) donde pueden aparecer las Zzz")]
    [SerializeField] private float maxY = 10f;

    [Header("Referencias")]
    [SerializeField] private ParticleSystem particleSystemZzz;
    [SerializeField] private SpriteRenderer[] spriteRenderersZzz;

    private void Start()
    {
        // Auto-detectar componentes si no están asignados
        if (particleSystemZzz == null)
        {
            particleSystemZzz = GetComponentInChildren<ParticleSystem>();
        }

        if (spriteRenderersZzz == null || spriteRenderersZzz.Length == 0)
        {
            spriteRenderersZzz = GetComponentsInChildren<SpriteRenderer>();
        }

        ConfigureParticleSystemBounds();
    }

    private void ConfigureParticleSystemBounds()
    {
        if (particleSystemZzz == null) return;

        // Configurar el sistema de partículas para que no salga de los límites
        var main = particleSystemZzz.main;
        main.cullingMode = ParticleSystemCullingMode.Automatic;

        // Opcional: Ajustar el tamaño del renderer bounds
        ParticleSystemRenderer renderer = particleSystemZzz.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            // Configurar el sorting layer para que esté dentro de la casa
            renderer.sortingLayerName = "Foreground"; // Ajusta según tu configuración
            renderer.sortingOrder = 5; // Ajusta según necesites
        }
    }

    private void LateUpdate()
    {
        // Método alternativo: Clipear sprites manualmente
        ClipSprites();
    }

    /// <summary>
    /// Deshabilita sprites que estén fuera de los límites definidos
    /// </summary>
    private void ClipSprites()
    {
        if (spriteRenderersZzz == null) return;

        foreach (var renderer in spriteRenderersZzz)
        {
            if (renderer == null) continue;

            Vector3 pos = renderer.transform.position;
            
            // Verificar si está dentro de los bounds
            bool isWithinBounds = pos.x >= minX && pos.x <= maxX &&
                                  pos.y >= minY && pos.y <= maxY;

            renderer.enabled = isWithinBounds;
        }
    }

    /// <summary>
    /// Dibuja los límites en el editor para visualización
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        
        // Dibujar rectángulo de límites
        Vector3 bottomLeft = new Vector3(minX, minY, 0);
        Vector3 bottomRight = new Vector3(maxX, minY, 0);
        Vector3 topRight = new Vector3(maxX, maxY, 0);
        Vector3 topLeft = new Vector3(minX, maxY, 0);

        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);
    }
}
