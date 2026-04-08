using UnityEngine;

/// <summary>
/// Datos de un indicador de misión/tarea que aparece en pantalla.
/// Crear via Assets > Create > UI > Mission Indicator.
/// </summary>
[CreateAssetMenu(fileName = "NewMissionIndicator", menuName = "UI/Mission Indicator")]
public class MissionIndicatorData : ScriptableObject
{
    [Tooltip("Título que aparece en el indicador (ej. 'Nueva Tarea').")]
    public string title;

    [Tooltip("Descripción breve de la misión/tarea.")]
    [TextArea(1, 3)]
    public string description;

    [Tooltip("Icono decorativo del indicador. Opcional.")]
    public Sprite icon;

    [Tooltip("Sonido que suena cuando el indicador aparece. Opcional.")]
    public AudioClip showSound;
}
