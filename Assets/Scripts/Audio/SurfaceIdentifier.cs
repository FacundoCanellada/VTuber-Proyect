using UnityEngine;

/// <summary>
/// Componente para identificar el tipo de superficie de un objeto.
/// Añadir esto a los objetos del suelo (alfombras, pasto, madera, etc.)
/// Asegúrate de que el objeto tenga un Collider2D (puede ser Trigger).
/// </summary>
public class SurfaceIdentifier : MonoBehaviour
{
    public SurfaceType surfaceType;
}
