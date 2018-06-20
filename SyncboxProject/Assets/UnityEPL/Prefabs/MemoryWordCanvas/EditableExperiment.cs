using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditableExperiment : MonoBehaviour
{
	public TextDisplayer textDisplayer;


	IEnumerator Start()
	{
		//this is a coroutine which can be used to display words via the text displayer

		//for example:
		string[] stimuli = new string[] { "Apple", "Banana", "Pear" };
		for (int i = 0; i < stimuli.Length; i++)
		{
			textDisplayer.DisplayText ("fruit stimulus", stimuli [i]);
			yield return new WaitForSecondsRealtime (1);
			textDisplayer.ClearText ();
			yield return new WaitForSecondsRealtime (1);
		}
	}
}