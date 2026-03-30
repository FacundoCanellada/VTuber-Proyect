using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PhoneConversationView : MonoBehaviour
{
    [Header("Phone UI")]
    [SerializeField] private GameObject phonePanel;
    [SerializeField] private CanvasGroup phonePanelCanvasGroup;
    [SerializeField] private RectTransform phonePanelRect;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private TypewriterEffect phoneTypewriterEffect;

    [Header("World Anchor")]
    [SerializeField] private Transform phoneDialogueTarget;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private Vector3 phoneDialogueWorldOffset = new Vector3(0f, 2.2f, 0f);

    [Header("Warning Icon")]
    [SerializeField] private GameObject warningIconObject;
    [SerializeField] private CanvasGroup warningIconCanvasGroup;
    [SerializeField] private RectTransform warningIconRect;
    [SerializeField] private Image warningIconImage;
    [SerializeField] private Vector3 warningIconWorldOffset = new Vector3(0f, 1.25f, 0f);

    [Header("Audio")]
    [SerializeField] private AudioSource cueAudioSource;
    [SerializeField] private AudioSource lineVoiceAudioSource;

    [Header("Animation")]
    [SerializeField] private PlayerAnimationController characterAnimationController;

    [Header("Timing")]
    [SerializeField] private float waitBetweenLines = 0.06f;
    [SerializeField] private float finalPause = 0.55f;

    private PhoneConversationData _conversationData;
    private PhoneConversationLine[] _lines;
    private int _currentLineIndex;
    private bool _waitingForAdvance;
    private bool _lineCompletionHandled;
    private string _currentLineText;
    private System.Action _onComplete;
    private AudioClip[] _originalPhoneVoiceClips;

    public bool IsConversationActive { get; private set; }

    private void Awake()
    {
        HideAllInstant();
    }

    private void LateUpdate()
    {
        if (!IsConversationActive)
        {
            return;
        }

        UpdateAnchors();
    }

    private void Update()
    {
        if (!IsConversationActive)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        bool zPressed = keyboard.zKey.wasPressedThisFrame;
        bool xPressed = keyboard.xKey.wasPressedThisFrame;

        if (!zPressed && !xPressed)
        {
            return;
        }

        if (phoneTypewriterEffect != null && phoneTypewriterEffect.IsTyping)
        {
            if (zPressed)
            {
                phoneTypewriterEffect.CompleteImmediate(_currentLineText);
                OnLineTypingComplete(_lines[_currentLineIndex]);
            }
            else
            {
                phoneTypewriterEffect.StopTyping();
                AdvanceLine(immediate: true);
            }

            return;
        }

        if (_waitingForAdvance)
        {
            AdvanceLine();
        }
    }

    public void StartConversation(PhoneConversationData data, System.Action onComplete)
    {
        StopAllCoroutines();
        HideAllInstant();

        _conversationData = data;
        _lines = data.lines;
        _currentLineIndex = 0;
        _waitingForAdvance = false;
        _lineCompletionHandled = false;
        _currentLineText = string.Empty;
        _onComplete = onComplete;
        IsConversationActive = true;

        if (characterNameText != null)
        {
            characterNameText.text = data.characterName;
        }

        if (phoneTypewriterEffect != null && data.phoneVoiceClips != null && data.phoneVoiceClips.Length > 0)
        {
            _originalPhoneVoiceClips = phoneTypewriterEffect.voiceClips;
            phoneTypewriterEffect.voiceClips = data.phoneVoiceClips;
        }
        else
        {
            _originalPhoneVoiceClips = null;
        }

        if (phonePanel != null)
        {
            phonePanel.SetActive(true);
        }

        if (phonePanelCanvasGroup != null)
        {
            phonePanelCanvasGroup.alpha = 1f;
            phonePanelCanvasGroup.blocksRaycasts = false;
            phonePanelCanvasGroup.interactable = false;
        }

        StartCoroutine(BeginConversationRoutine());
    }

    private IEnumerator BeginConversationRoutine()
    {
        if (_conversationData != null && _conversationData.openingSequence.enabled)
        {
            yield return StartCoroutine(PlayOpeningSequence(_conversationData.openingSequence));
        }

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        if (_currentLineIndex >= _lines.Length)
        {
            bool skipFinalPause = _lines != null && _lines.Length > 0 && _lines[_lines.Length - 1].skipFinalPause;
            StartCoroutine(FinishConversation(skipFinalPause, immediate: false));
            return;
        }

        StartCoroutine(BeginLineRoutine(_lines[_currentLineIndex]));
    }

    private IEnumerator BeginLineRoutine(PhoneConversationLine line)
    {
        _lineCompletionHandled = false;
        _waitingForAdvance = false;
        _currentLineText = line.text;

        if (phoneTypewriterEffect != null)
        {
            phoneTypewriterEffect.ClearText();
        }

        yield return StartCoroutine(PlayCue(line.cue));

        if (line.voiceClip != null && lineVoiceAudioSource != null)
        {
            lineVoiceAudioSource.PlayOneShot(line.voiceClip);
        }

        if (phoneTypewriterEffect != null)
        {
            phoneTypewriterEffect.ShowText(
                line.text,
                -1f,
                line.charsPerSound > 0 ? line.charsPerSound : -1,
                line.typingSpeed > 0f ? line.typingSpeed : -1f,
                () => OnLineTypingComplete(line)
            );
        }
        else
        {
            OnLineTypingComplete(line);
        }
    }

    private IEnumerator PlayCue(PhoneConversationCue cue)
    {
        if (cue.triggerCharacterEmote && characterAnimationController != null)
        {
            characterAnimationController.TryStartEmote();
        }

        float ringDuration = Mathf.Max(0f, cue.ringDuration);
        float iconDuration = cue.warningIconDuration > 0f ? cue.warningIconDuration : ringDuration;
        float totalCueDuration = Mathf.Max(ringDuration, iconDuration);

        if (cue.playRingBeforeLine && cue.ringClip != null && cueAudioSource != null)
        {
            cueAudioSource.PlayOneShot(cue.ringClip);
        }

        if (cue.showWarningIcon)
        {
            ShowWarningIcon(cue);
        }

        if (totalCueDuration > 0f)
        {
            yield return new WaitForSecondsRealtime(totalCueDuration);
        }

        HideWarningIcon();
    }

    private IEnumerator PlayOpeningSequence(PhoneConversationOpeningSequence openingSequence)
    {
        if (openingSequence.showWarningIconDuringRing)
        {
            ShowWarningIcon(openingSequence.warningMode, openingSequence.warningIconSprite);
        }

        if (openingSequence.ringClip != null && cueAudioSource != null)
        {
            cueAudioSource.PlayOneShot(openingSequence.ringClip);
        }

        if (openingSequence.ringDuration > 0f)
        {
            yield return new WaitForSecondsRealtime(openingSequence.ringDuration);
        }

        if (openingSequence.answerClipDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(openingSequence.answerClipDelay);
        }

        if (openingSequence.answerClip != null && cueAudioSource != null)
        {
            cueAudioSource.PlayOneShot(openingSequence.answerClip);
        }

        if (openingSequence.triggerCharacterEmoteOnAnswer && characterAnimationController != null)
        {
            characterAnimationController.TryStartEmote();
        }

        if (openingSequence.hideWarningIconWhenAnswered)
        {
            HideWarningIcon();
        }

        if (openingSequence.dialogueStartDelayAfterAnswer > 0f)
        {
            yield return new WaitForSecondsRealtime(openingSequence.dialogueStartDelayAfterAnswer);
        }

        if (!openingSequence.hideWarningIconWhenAnswered)
        {
            HideWarningIcon();
        }
    }

    private void OnLineTypingComplete(PhoneConversationLine line)
    {
        if (_lineCompletionHandled)
        {
            return;
        }

        _lineCompletionHandled = true;

        if (line.autoAdvance)
        {
            StartCoroutine(AutoAdvanceAfter(Mathf.Max(0f, line.pauseAfter)));
        }
        else
        {
            _waitingForAdvance = true;
        }
    }

    private IEnumerator AutoAdvanceAfter(float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSecondsRealtime(delay);
        }

        AdvanceLine();
    }

    private void AdvanceLine(bool immediate = false)
    {
        StopAllCoroutines();
        HideWarningIcon();
        _waitingForAdvance = false;
        _currentLineIndex++;

        if (_currentLineIndex >= _lines.Length)
        {
            bool skipFinalPause = _lines != null && _lines.Length > 0 && _lines[_lines.Length - 1].skipFinalPause;
            StartCoroutine(FinishConversation(skipFinalPause, immediate));
            return;
        }

        StartCoroutine(AdvanceWithDelay(immediate));
    }

    private IEnumerator AdvanceWithDelay(bool immediate)
    {
        if (phoneTypewriterEffect != null)
        {
            phoneTypewriterEffect.ClearText();
        }

        if (!immediate && waitBetweenLines > 0f)
        {
            yield return new WaitForSecondsRealtime(waitBetweenLines);
        }

        ShowCurrentLine();
    }

    private IEnumerator FinishConversation(bool skipFinalPause, bool immediate)
    {
        if (!immediate && !skipFinalPause && finalPause > 0f)
        {
            yield return new WaitForSecondsRealtime(finalPause);
        }

        if (phoneTypewriterEffect != null)
        {
            phoneTypewriterEffect.ClearText();
        }

        if (_originalPhoneVoiceClips != null && phoneTypewriterEffect != null)
        {
            phoneTypewriterEffect.voiceClips = _originalPhoneVoiceClips;
            _originalPhoneVoiceClips = null;
        }

        System.Action callback = _onComplete;
        _onComplete = null;

        HideAllInstant();
        IsConversationActive = false;
        callback?.Invoke();
    }

    private void UpdateAnchors()
    {
        Camera cameraToUse = worldCamera != null ? worldCamera : Camera.main;
        if (cameraToUse == null || phoneDialogueTarget == null)
        {
            return;
        }

        if (phonePanelRect != null && phonePanel != null && phonePanel.activeSelf)
        {
            phonePanelRect.position = cameraToUse.WorldToScreenPoint(phoneDialogueTarget.position + phoneDialogueWorldOffset);
        }

        if (warningIconRect != null && warningIconObject != null && warningIconObject.activeSelf)
        {
            warningIconRect.position = cameraToUse.WorldToScreenPoint(phoneDialogueTarget.position + warningIconWorldOffset);
        }
    }

    private void ShowWarningIcon(PhoneConversationCue cue)
    {
        ShowWarningIcon(cue.warningMode, cue.warningIconSprite);
    }

    private void ShowWarningIcon(PhoneConversationWarningMode warningMode, Sprite lineIconSprite)
    {
        if (warningIconObject == null)
        {
            return;
        }

        if (warningIconImage != null)
        {
            Sprite iconSprite = warningMode == PhoneConversationWarningMode.UsePerLineSprite && lineIconSprite != null
                ? lineIconSprite
                : (_conversationData != null ? _conversationData.defaultWarningIconSprite : null);

            if (iconSprite != null)
            {
                warningIconImage.sprite = iconSprite;
            }
        }

        warningIconObject.SetActive(true);
        if (warningIconCanvasGroup != null)
        {
            warningIconCanvasGroup.alpha = 1f;
        }
    }

    private void HideWarningIcon()
    {
        if (warningIconCanvasGroup != null)
        {
            warningIconCanvasGroup.alpha = 0f;
        }

        if (warningIconObject != null)
        {
            warningIconObject.SetActive(false);
        }
    }

    private void HideAllInstant()
    {
        HideWarningIcon();

        if (phonePanelCanvasGroup != null)
        {
            phonePanelCanvasGroup.alpha = 0f;
            phonePanelCanvasGroup.blocksRaycasts = false;
            phonePanelCanvasGroup.interactable = false;
        }

        if (phonePanel != null)
        {
            phonePanel.SetActive(false);
        }
    }
}