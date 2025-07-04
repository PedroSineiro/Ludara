using System.Collections;

public class Dialog : EventBase
{
    public string[] sentences;
    private int currentSentence = 0;

    public override IEnumerator Execute(ButtonManager manager)
    {
        if (currentSentence < sentences.Length)
        {
            manager.dialogText.text = sentences[currentSentence];
            currentSentence++;
        }

        yield break;
    }

    public bool IsFinished()
    {
        return currentSentence >= sentences.Length;
    }
}
