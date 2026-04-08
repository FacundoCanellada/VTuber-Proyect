using UnityEngine;

public enum PhoneConversationWarningMode
{
    None,
    UseAssignedSprite,
    UsePerLineSprite,
}

[System.Serializable]
public struct PhoneConversationOpeningSequence
{
    [Tooltip("Si está activo, reproduce la secuencia de llamada antes de la primera línea.")]
    public bool enabled;

    [Tooltip("Clip del teléfono sonando antes de atender.")]
    public AudioClip ringClip;

    [Tooltip("Cuánto dura el ring inicial antes de atender.")]
    [Min(0f)]
    public float ringDuration;

    [Tooltip("Cuántas veces se repite el sonido de ring antes de atender. Usa 2 para una llamada más marcada.")]
    [Min(1)]
    public int ringRepeatCount;

    [Tooltip("Pausa adicional entre rings cuando ringRepeatCount es mayor que 1.")]
    [Min(0f)]
    public float ringInterval;

    [Tooltip("Si está activo, muestra el icono de advertencia durante el ring.")]
    public bool showWarningIconDuringRing;

    [Tooltip("Sprite del icono para la llamada. Si está vacío, usa el default del asset.")]
    public Sprite warningIconSprite;

    [Tooltip("Cómo resolver el sprite del icono durante la llamada.")]
    public PhoneConversationWarningMode warningMode;

    [Tooltip("Clip del momento en que Ahiiruw atiende el teléfono.")]
    public AudioClip answerClip;

    [Tooltip("Delay entre que empieza la animación de atender y suena el clip de respuesta.")]
    [Min(0f)]
    public float answerClipDelay;

    [Tooltip("Nombre exacto del estado del Animator para la animación de atender. Opcional.")]
    public string answerAnimationStateName;

    [Tooltip("Crossfade usado al entrar a la animación de atender si answerAnimationStateName está configurado.")]
    [Min(0f)]
    public float answerAnimationCrossFadeDuration;

    [Tooltip("Si está activo, dispara el emote/animación del personaje al atender.")]
    public bool triggerCharacterEmoteOnAnswer;

    [Tooltip("Tiempo después de atender en el que empieza a mostrarse el diálogo.\n" +
             "Esto permite que las líneas aparezcan mientras aún corre la animación de atender.")]
    [Min(0f)]
    public float dialogueStartDelayAfterAnswer;

    [Tooltip("Si está activo, oculta el icono de advertencia al atender.\n" +
             "Si está desactivado, el icono sigue visible hasta que aparezca la primera línea.")]
    public bool hideWarningIconWhenAnswered;
}

[System.Serializable]
public struct PhoneConversationCue
{
    [Tooltip("Si está activo, reproduce un ring/tono antes de mostrar la línea.")]
    public bool playRingBeforeLine;

    [Tooltip("Clip del tono o ring antes de la línea.")]
    public AudioClip ringClip;

    [Tooltip("Duración de la pausa/cue antes de mostrar la línea.")]
    [Min(0f)]
    public float ringDuration;

    [Tooltip("Si está activo, muestra un icono de advertencia sobre el personaje durante el cue.")]
    public bool showWarningIcon;

    [Tooltip("Sprite del icono para esta línea. Solo se usa si warningMode = UsePerLineSprite.")]
    public Sprite warningIconSprite;

    [Tooltip("Cómo resolver el sprite del icono de advertencia.")]
    public PhoneConversationWarningMode warningMode;

    [Tooltip("Tiempo que permanece visible el icono durante el cue. 0 = usar ringDuration.")]
    [Min(0f)]
    public float warningIconDuration;

    [Tooltip("Si está activo, intenta disparar el emote del personaje antes de la línea.")]
    public bool triggerCharacterEmote;
}

[System.Serializable]
public struct PhoneConversationLine
{
    [TextArea(2, 5)]
    [Tooltip("Texto que se escribe en el diálogo telefónico/flotante.")]
    public string text;

    [Tooltip("Clip one-shot opcional al inicio de la línea.")]
    public AudioClip voiceClip;

    [Tooltip("Velocidad de tipeo en segundos por carácter. -1 = usar el default del TypewriterEffect.")]
    public float typingSpeed;

    [Tooltip("Cada cuántos caracteres suena el efecto de voz. -1 = usar el default del TypewriterEffect.")]
    [Range(-1, 10)]
    public int charsPerSound;

    [Tooltip("Si está activo, la línea avanza automáticamente sin esperar input del jugador.")]
    public bool autoAdvance;

    [Tooltip("Segundos de espera antes de auto-avanzar.")]
    public float pauseAfter;

    [Tooltip("Si esta línea es la última, omite la pausa final antes de cerrar la secuencia telefónica.")]
    public bool skipFinalPause;

    [Tooltip("Cue opcional antes de mostrar la línea (ring, icono, emote).")]
    public PhoneConversationCue cue;
}

[CreateAssetMenu(fileName = "NewPhoneConversation", menuName = "Dialogue/Phone Conversation Data")]
public class PhoneConversationData : ScriptableObject
{
    [Tooltip("Nombre del personaje que habla por teléfono.")]
    public string characterName;

    [Tooltip("Clips de voz para el typewriter del modo teléfono.")]
    public AudioClip[] phoneVoiceClips;

    [Tooltip("Sprite default del icono de advertencia para la llamada entrante.")]
    public Sprite defaultWarningIconSprite;

    [Tooltip("Secuencia global previa a la conversación: ring, atender, emote, etc.")]
    public PhoneConversationOpeningSequence openingSequence;

    [Tooltip("Líneas de la conversación telefónica.")]
    public PhoneConversationLine[] lines;

    [Tooltip("Clip que suena al colgar el teléfono (al terminar todas las líneas de diálogo). Opcional.")]
    public AudioClip hangUpClip;
}