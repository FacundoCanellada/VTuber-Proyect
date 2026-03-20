using UnityEngine;

[System.Serializable]
public struct DialogueLine
{
    [TextArea(2, 5)]
    [Tooltip("Texto que se escribe en el cuadro de diálogo.")]
    public string text;

    [Tooltip("Clip de voz actuada para esta línea (opcional, se reproduce al inicio de la línea).")]
    public AudioClip voiceClip;

    [Tooltip("Velocidad de tipeo en segundos por carácter. -1 = usar el default del TypewriterEffect.")]
    public float typingSpeed;

    [Tooltip("Cada cuántos caracteres suena el efecto de voz. -1 = usar el default del TypewriterEffect.")]
    [Range(-1, 10)]
    public int charsPerSound;

    [Tooltip("Modifica el LC (Level of Confidence) del TrustManager al reproducirse esta línea. 0 = sin cambio.")]
    public float lcModifier;

    [Tooltip("Si está activo, la línea avanza automáticamente sin esperar input del jugador.")]
    public bool autoAdvance;

    [Tooltip("Segundos de espera antes de auto-avanzar (solo tiene efecto si autoAdvance = true).")]
    public float pauseAfter;
}

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [Tooltip("Nombre del personaje que aparece en el cuadro de diálogo.")]
    public string characterName;

    [Tooltip("Retrato del personaje que aparece en la esquina del cuadro.")]
    public Sprite portrait;

    [Tooltip("(Opcional) Overrides los clips de voz del TypewriterEffect para toda la conversación. " +
             "Deja vacío para usar los clips ya configurados en el componente TypewriterEffect de la escena.")]
    public AudioClip[] characterVoiceClips;

    [Tooltip("Líneas de diálogo en orden.")]
    public DialogueLine[] lines;
}
