using System;
using System.Threading;
using UnityEngine.Networking;
using UnityEngine;

public class SoundRecorder : MonoBehaviour
{
    private AudioClip recording;
    private int startSample;
    private float startTime;
    private bool isRecording = false;
    private string nextOutputPath;

    private const int SECONDS_IN_MEMORY = 1200;
    private const int SAMPLE_RATE = 44100;

    void OnEnable()
    {
        //TODO: enable cycling through devices

        try  {
            recording = Microphone.Start("", true, SECONDS_IN_MEMORY, SAMPLE_RATE);
        }
        catch(Exception e) { // TODO
            ErrorNotification.Notify(e);
        }
    }

    void OnDisable()
    {
        // device = null;
        if(isRecording)
            StopRecording();
        Microphone.End("");
    }

    //using the system's default device
    public void StartRecording(string outputFilePath)
    {
        if (isRecording)
        {
            throw new UnityException("Already recording.  Please StopRecording first.");
        }

        nextOutputPath = outputFilePath;
        startSample = Microphone.GetPosition("");
        startTime = Time.unscaledTime;
        isRecording = true;
    }

    public AudioClip StopRecording()
    {
        if (!isRecording)
        {
            throw new UnityException("Not recording.  Please StartRecording first.");
        }
        isRecording = false;

        float recordingLength = Time.unscaledTime - startTime;

        int outputLength = Mathf.RoundToInt(SAMPLE_RATE * recordingLength);
        AudioClip croppedClip = AudioClip.Create("cropped recording", outputLength, 1, SAMPLE_RATE, false);

        float[] saveData = GetLastSamples(outputLength);

        croppedClip.SetData(saveData, 0);

        ThreadPool.QueueUserWorkItem((state) => SavWav.Save(nextOutputPath, croppedClip));
        return croppedClip;
    }

    public float[] GetLastSamples(int howManySamples)
    {
        float[] lastSamples = new float[howManySamples];
        if (startSample < recording.samples - howManySamples)
        {
            recording.GetData(lastSamples, startSample);
        }
        else
        {
            float[] tailData = new float[recording.samples - startSample];
            recording.GetData(tailData, startSample);
            float[] headData = new float[howManySamples - tailData.Length];
            recording.GetData(headData, 0);
            for (int i = 0; i < tailData.Length; i++)
                lastSamples[i] = tailData[i];
            for (int i = 0; i < headData.Length; i++)
                lastSamples[tailData.Length + i] = headData[i];
        }
        return lastSamples;
    }

    public AudioClip AudioClipFromDatapath(string datapath)
    {
        string url = "file:///" + datapath;
        UnityWebRequest audioFile = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV);
        //audioFile.timeout = 10; // timeout in ten seconds
        audioFile.SendWebRequest();
        while(!audioFile.isDone) {

            // FIXME
    
            Debug.Log("blocking");
            // block
        }

        if(audioFile.isNetworkError) {
            throw new System.Exception(audioFile.error);
        }
        if(audioFile.isHttpError) {
            throw new System.Exception(audioFile.responseCode.ToString());
        }

        return DownloadHandlerAudioClip.GetContent(audioFile);
    }

    void OnApplicationQuit()
    {
        if (isRecording)
            StopRecording();
    }
}