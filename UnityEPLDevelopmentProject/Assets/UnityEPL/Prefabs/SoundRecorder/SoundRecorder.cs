using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundRecorder : MonoBehaviour
{
    private AudioClip recording;

    //using the system's default device
    public void StartRecording(int secondsMaxLength)
    {
        recording = Microphone.Start("", true, secondsMaxLength, 44100);
    }

    public void StopRecording(string outputFilePath)
    {
        Microphone.End("");
        SavWav.Save(outputFilePath, recording);
    }

    public AudioClip GetLastClip()
    {
        return recording;
    }
}