using UnityEngine;

[CreateAssetMenu(fileName = "NewPortraitSequence", menuName = "Dialogue/Portrait Sequence")]
public class DialoguePortraitSequenceAsset : ScriptableObject
{
    [Tooltip("Secuencia reutilizable de retrato para múltiples líneas o diálogos.")]
    public DialoguePortraitSequence sequence;
}

[System.Serializable]
public struct DialoguePortraitFrame
{
    [Tooltip("Sprite que se muestra en este paso de la secuencia.")]
    public Sprite sprite;

    [Tooltip("Cuántas veces se repite este frame antes de pasar al siguiente.")]
    [Min(1)]
    public int repeatCount;

    [Tooltip("Duración en segundos de cada repetición de este frame.")]
    [Min(0.01f)]
    public float duration;
}

[System.Serializable]
public struct DialoguePortraitSequence
{
    [Tooltip("Sprite base cuando el personaje está quieto o cuando no hay animación configurada.")]
    public Sprite idleSprite;

    [Tooltip("Frames que se reproducen en loop mientras el texto se está escribiendo.")]
    public DialoguePortraitFrame[] typingFrames;

    [Tooltip("Frames que se reproducen una sola vez al terminar de escribir la línea, antes de permitir avance o cerrar.")]
    public DialoguePortraitFrame[] postTypingFrames;
}

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

    [Tooltip("Si está activo, esta línea usa su propia secuencia de retrato en vez de la default del diálogo.")]
    public bool usePortraitSequenceOverride;

    [Tooltip("Preset reutilizable de secuencia de retrato para esta línea. Tiene prioridad sobre el override inline.")]
    public DialoguePortraitSequenceAsset portraitSequenceAsset;

    [Tooltip("Secuencia de retrato específica de esta línea.")]
    public DialoguePortraitSequence portraitSequenceOverride;

    [Tooltip("Si esta línea es la última y auto-avanza, omite el waitAfterLastLine global tras terminar la secuencia final de retrato.")]
    public bool skipFinalCloseDelay;
}

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [Tooltip("Nombre del personaje que aparece en el cuadro de diálogo.")]
    public string characterName;

    [Tooltip("Retrato del personaje que aparece en la esquina del cuadro.")]
    public Sprite portrait;

    [Tooltip("Preset reutilizable para la secuencia default del diálogo. Si se asigna, tiene prioridad sobre la inline.")]
    public DialoguePortraitSequenceAsset defaultPortraitSequenceAsset;

    [Tooltip("Secuencia default de retrato para este diálogo. Si una línea no tiene override, usa esta.")]
    public DialoguePortraitSequence defaultPortraitSequence;

    [Tooltip("(Opcional) Overrides los clips de voz del TypewriterEffect para toda la conversación. " +
             "Deja vacío para usar los clips ya configurados en el componente TypewriterEffect de la escena.")]
    public AudioClip[] characterVoiceClips;

    [Tooltip("Líneas de diálogo en orden.")]
    public DialogueLine[] lines;
}
