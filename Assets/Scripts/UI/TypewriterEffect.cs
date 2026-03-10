using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TypewriterEffect : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Tiempo en segundos por cada carácter (más alto = más lento).")]
    public float typingSpeed = 0.09f;

    [Header("Audio")]
    [Tooltip("El AudioSource que reproducirá los sonidos.")]
    public AudioSource audioSource;
    [Tooltip("Los clips de voz que se usarán.")]
    public AudioClip[] voiceClips;
    [Tooltip("Volumen de la voz (0-1).")]
    [Range(0f, 1f)] public float voiceVolume = 0.3f;
    [Tooltip("Cada cuántos caracteres visibles se reproduce un sonido de voz.")]
    [Range(1, 10)] public int charsPerSound = 3;

    private TMP_Text _tmpText;
    private Coroutine _typingCoroutine;
    private int _lastPlayedClipIndex = -1;
    private int _visibleCharCount;

    public bool IsTyping { get; private set; }

    private void Awake()
    {
        _tmpText = GetComponent<TMP_Text>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource != null && voiceClips != null && voiceClips.Length > 0)
        {
            audioSource.playOnAwake = false;
        }
    }

    /// <summary>
    /// Starts the typing effect with the given text.
    /// </summary>
    public void ShowText(string text, System.Action onComplete = null)
    {
        ShowText(text, -1f, -1, onComplete);
    }

    /// <summary>
    /// Starts the typing effect with per-line voice overrides.
    /// Pass -1 for volumeOverride or charsPerSoundOverride to use the default values.
    /// </summary>
    public void ShowText(string text, float volumeOverride, int charsPerSoundOverride, System.Action onComplete = null)
    {
        if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
        float vol = volumeOverride >= 0f ? volumeOverride : voiceVolume;
        int cps = charsPerSoundOverride > 0 ? charsPerSoundOverride : charsPerSound;
        _typingCoroutine = StartCoroutine(TypeRoutine(text, vol, cps, onComplete));
    }

    /// <summary>
    /// Clears the text immediately without typing effect.
    /// </summary>
    public void ClearText()
    {
        if (_tmpText != null) _tmpText.text = "";
    }

    private IEnumerator TypeRoutine(string text, float effectiveVolume, int effectiveCharsPerSound, System.Action onComplete)
    {
        IsTyping = true;
        _tmpText.text = "";
        _visibleCharCount = 0;

        // Handle Unity Inspector newlines just in case user types literal "\n"
        text = text.Replace("\\n", "\n");

        // Rich Text parser (simple version)
        int i = 0;
        float wait = typingSpeed;

        while (i < text.Length)
        {
            if (text[i] == '<')
            {
                // Found potential tag start
                int closeIndex = text.IndexOf('>', i);
                if (closeIndex != -1)
                {
                    // It's a tag, append completely without waiting
                    string tag = text.Substring(i, closeIndex - i + 1);
                    _tmpText.text += tag;
                    i = closeIndex + 1;
                    continue; // Skip the delay
                }
            }

            _tmpText.text += text[i];

            if (!char.IsWhiteSpace(text[i]))
            {
                _visibleCharCount++;
                if (_visibleCharCount % effectiveCharsPerSound == 0)
                {
                    PlayVoiceSound(effectiveVolume);
                }
            }

            i++;
            yield return new WaitForSecondsRealtime(wait);
        }

        IsTyping = false;
        onComplete?.Invoke();
    }

    private void PlayVoiceSound(float volume)
    {
        if (audioSource == null || voiceClips == null || voiceClips.Length == 0) return;

        if (!audioSource.isPlaying)
        {
            int index = GetRandomClipIndex();
            _lastPlayedClipIndex = index;
            
            audioSource.clip = voiceClips[index];
            audioSource.volume = volume;
            audioSource.pitch = Random.Range(0.95f, 1.05f); 
            audioSource.Play();
        }
    }

    private int GetRandomClipIndex()
    {
        if (voiceClips == null || voiceClips.Length <= 1) return 0;

        int index;
        // Try to find a different index than the last one
        int maxAttempts = 10;
        do
        {
            index = Random.Range(0, voiceClips.Length);
            maxAttempts--;
        } while (index == _lastPlayedClipIndex && maxAttempts > 0);

        return index;
    }

    public void StopTyping()
    {
        if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
        IsTyping = false;
        if (audioSource != null) audioSource.Stop();
    }
}
