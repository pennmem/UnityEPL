using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundRecorder : MonoBehaviour
{
	public string outputPath = System.IO.Path.GetFullPath(".");

	private AudioClip recording;

	public void StartRecording()
	{
		recording = Microphone.Start ("", true, 1, 44100);
	}

	public void StopRecording()
	{
		Microphone.End ("");
		string filePath = System.IO.Path.Combine (outputPath, "Recording" + System.DateTime.Now.Ticks);
		SavWav.Save (filePath, recording);
	}
}