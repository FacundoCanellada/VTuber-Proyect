using UnityEngine;

[CreateAssetMenu(fileName = "New Surface Profile", menuName = "Audio/Surface Profile")]
public class SurfaceProfile : ScriptableObject
{
    [Header("Identificador de Superficie")]
    public SurfaceType surfaceType;
    
    [Header("Sonidos de Pasos")]
    [Tooltip("Sonidos al caminar (usará uno al azar)")]
    public AudioClip[] walkClips;
    
    [Tooltip("Sonidos al correr (usará uno al azar)")]
    public AudioClip[] runClips;
    
    [Header("Otros Sonidos (Extensible)")]
    [Tooltip("Sonido al aterrizar de un salto")]
    public AudioClip[] landClips;
}
