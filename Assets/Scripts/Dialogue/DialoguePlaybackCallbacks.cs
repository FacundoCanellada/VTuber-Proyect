using System;

public struct DialoguePlaybackContext
{
    public DialogueLine Line { get; }
    public int LineIndex { get; }
    public int TotalLineCount { get; }

    public bool IsFirstLine => LineIndex == 0;
    public bool IsLastLine => TotalLineCount > 0 && LineIndex == TotalLineCount - 1;

    public DialoguePlaybackContext(DialogueLine line, int lineIndex, int totalLineCount)
    {
        Line = line;
        LineIndex = lineIndex;
        TotalLineCount = totalLineCount;
    }
}

public struct DialoguePlaybackCallbacks
{
    public Action<DialoguePlaybackContext> onLineStarted;
    public Action<DialoguePlaybackContext> onLineCompleted;

    public static DialoguePlaybackCallbacks None => default;

    public void NotifyLineStarted(DialogueLine line, int lineIndex, int totalLineCount)
    {
        onLineStarted?.Invoke(new DialoguePlaybackContext(line, lineIndex, totalLineCount));
    }

    public void NotifyLineCompleted(DialogueLine line, int lineIndex, int totalLineCount)
    {
        onLineCompleted?.Invoke(new DialoguePlaybackContext(line, lineIndex, totalLineCount));
    }
}