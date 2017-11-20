using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StroopExperiment : MonoBehaviour
{
	public TextDisplayer textDisplayer;
    public ScriptedEventReporter answerEventReporter;

	IEnumerator Start()
    {
        UnityEPL.SetExperimentName("Stroop Test");

		string[] color_words = new string[] { "Red", "Green", "Blue" };
        Color[] colors = new Color[] { Color.red, Color.green, Color.blue };

		for (int i = 0; i < 10; i++)
		{
            int random_word_index = Random.Range(0, color_words.Length-1);
            int random_color_index = Random.Range(0, colors.Length - 1);

            textDisplayer.DisplayText ("color word stimulus", color_words [i]);
            textDisplayer.ChangeColor(colors[random_color_index]);

            yield return WaitForAndReportAnswer(random_word_index == random_color_index);
		}
	}

    IEnumerator WaitForAndReportAnswer(bool colorAndWordAreTheSame)
    {
        while (!Input.GetKeyDown(KeyCode.Y) && !Input.GetKeyDown(KeyCode.N))
            yield return null;

        Dictionary<string, object> eventData = new Dictionary<string, object>();
        bool participant_answered_correctly = Input.GetKeyDown(KeyCode.Y) == colorAndWordAreTheSame;
        eventData.Add("answer correct", participant_answered_correctly);
        eventData.Add("color and word were the same", colorAndWordAreTheSame);
        //answerEventReporter.ReportScriptedEvent("stimulus response", eventData);
    }
}