using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StroopExperiment : MonoBehaviour
{
	public TextDisplayer textDisplayer;
    public ScriptedEventReporter answerEventReporter;
    public UnityEngine.UI.InputField participantNameInput;
    public GameObject continueButton;

    private bool continue_pressed = false;

    private const float interstimulus_interval = 1f;

	IEnumerator Start()
    {
        UnityEPL.SetExperimentName("Stroop Test");

        yield return GetParticipantName();

        yield return ShowInstructions();

        yield return DoTest();
	}

    /// <summary>
    /// Call this from the "continue" button.
    /// </summary>
    public void Continue()
    {
        continue_pressed = true;
    }

    private IEnumerator WaitForContinue()
    {
        while (!continue_pressed)
            yield return null;
        continue_pressed = false;
    }

    private IEnumerator GetParticipantName()
    {
        yield return WaitForContinue();

        UnityEPL.AddParticipant(participantNameInput.text);
        participantNameInput.gameObject.SetActive(false);
    }

    private IEnumerator ShowInstructions()
    {
        textDisplayer.DisplayText("text instructions", "You will see the names of colors printed as words.  Sometimes, the color of the print will match the name of the color, but sometimes it wont. \n\n" +
                                  "Press Y if the name and the color match, and press N if they don't.");

        yield return WaitForContinue();

        continueButton.SetActive(false);
    }

    private IEnumerator DoTest()
    {
        string[] color_words = new string[] { "Red", "Green", "Blue" };
        Color[] colors = new Color[] { Color.red, Color.green, Color.blue };

        //display ten colored stimuli and wait for the participant's answer each time
        for (int i = 0; i < 10; i++)
        {
            //display nothing for a time between stimuli
            textDisplayer.ClearText();
            yield return new WaitForSeconds(interstimulus_interval);

            //choose a random color word and random color
            int random_word_index = Random.Range(0, color_words.Length);
            int random_color_index = Random.Range(0, colors.Length);

            //display the word in that color
            textDisplayer.DisplayText("color word stimulus", color_words[random_word_index]);
            textDisplayer.ChangeColor(colors[random_color_index]);

            yield return WaitForAndReportAnswer(random_word_index == random_color_index);
        }

        textDisplayer.OriginalColor();
        textDisplayer.DisplayText("end message", "Thank you!  The test is over.");
    }

    private IEnumerator WaitForAndReportAnswer(bool colorAndWordAreTheSame)
    {
        //wait for the participant to input their answer
        float startTime = Time.unscaledTime;
        while (!Input.GetKeyDown(KeyCode.Y) && !Input.GetKeyDown(KeyCode.N))
            yield return null;
        float answerTime = Time.unscaledTime;

        //report the time taken, whether the answer was correct, and whether the color and word matched
        Dictionary<string, object> eventData = new Dictionary<string, object>();
        bool participant_answered_correctly = Input.GetKeyDown(KeyCode.Y) == colorAndWordAreTheSame;
        eventData.Add("answer correct", participant_answered_correctly);
        eventData.Add("color and word were the same", colorAndWordAreTheSame);
        eventData.Add("time taken", answerTime - startTime);
        answerEventReporter.ReportScriptedEvent("stimulus response", eventData);
    }
}