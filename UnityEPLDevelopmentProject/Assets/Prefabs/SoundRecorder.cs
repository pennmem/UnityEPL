using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundRecorder : MonoBehaviour
{
	public string outputPath = System.IO.Path.GetFullPath(".");

	public void RecordClip(int secondsDuration)
	{
		AudioClip recording = Microphone.Start ("", false, secondsDuration, 44100);
		string filePath = System.IO.Path.Combine(outputPath, "Recording" + System.DateTime.Now.Ticks);
		SavWav.Save (filePath, recording);
	}
}