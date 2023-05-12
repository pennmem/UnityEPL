using System;
using System.Threading;
using UnityEngine.Networking;
using UnityEngine;
using Unity.Collections;
using System.Threading.Tasks;

namespace UnityEPL {

    public class SoundRecorder : EventMonoBehaviour {
        protected override void AwakeOverride() { }

        private AudioClip recording;
        private int startSample;
        private float startTime;
        private bool isRecording = false;
        private string nextOutputPath;

        private const int SECONDS_IN_MEMORY = 1200;
        private const int SAMPLE_RATE = 44100;

        protected void OnEnable() {
            //TODO: enable cycling through devices
            try {
                recording = Microphone.Start("", true, SECONDS_IN_MEMORY, SAMPLE_RATE);
            } catch (Exception e) { // TODO
                ErrorNotifier.Error(e);
            }
        }

        protected void OnDisable() {
            // device = null;
            if (isRecording)
                StopRecording();
            Microphone.End("");
        }

        protected void OnApplicationQuit() {
            if (isRecording)
                StopRecording();
        }

        // TODO: JPB: (needed) (refactor) SoundRecorder should add a function that does start and stop
        //            Record(int duration, string outputFilePath)

        //using the system's default device
        public void StartRecording(string outputFilePath) {
            Do(StartRecordingHelper, outputFilePath.ToNativeText());
        }
        public void StartRecordingMB(string outputFilePath) {
            DoMB(StartRecordingHelper, outputFilePath.ToNativeText());
        }
        protected void StartRecordingHelper(NativeText outputFilePath) {
            if (isRecording) {
                throw new UnityException("Already recording.  Please StopRecording first.");
            }

            nextOutputPath = outputFilePath.ToString();
            startSample = Microphone.GetPosition("");
            startTime = Time.unscaledTime;
            isRecording = true;
            outputFilePath.Dispose();
        }

        public Task<AudioClip> StopRecording() {
            return DoGetRelaxed(StopRecordingHelper);
        }
        public AudioClip StopRecordingMB() {
            return DoGetMB(StopRecordingHelper);
        }
        protected AudioClip StopRecordingHelper() {
            if (!isRecording) {
                throw new UnityException("Not recording.  Please StartRecording first.");
            }
            isRecording = false;

            float recordingLength = Time.unscaledTime - startTime;

            int outputLength = Mathf.RoundToInt(SAMPLE_RATE * recordingLength);
            AudioClip croppedClip = AudioClip.Create("cropped recording", outputLength, 1, SAMPLE_RATE, false);

            float[] saveData = GetLastSamplesMB(outputLength);

            croppedClip.SetData(saveData, 0);

            ThreadPool.QueueUserWorkItem((state) => SavWav.Save(nextOutputPath, croppedClip));
            return croppedClip;
        }

        public Task<float[]> GetLastSamples(int howManySamples) {
            return DoGetRelaxed(GetLastSamplesHelper, howManySamples);
        }
        public float[] GetLastSamplesMB(int howManySamples) {
            return DoGetMB(GetLastSamplesHelper, howManySamples);
        }
        public float[] GetLastSamplesHelper(int howManySamples) {
            float[] lastSamples = new float[howManySamples];
            if (startSample < recording.samples - howManySamples) {
                recording.GetData(lastSamples, startSample);
            } else {
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

        // TODO: JPB: (bug) Fix AudioClipFromDatapathHelper and make it public 
        private Task<AudioClip> AudioClipFromDatapath(string datapath) {
            return DoGetRelaxed(AudioClipFromDatapathHelper, datapath.ToNativeText());
        }
        private AudioClip AudioClipFromDatapathMB(string datapath) {
            return DoGetMB(AudioClipFromDatapathHelper, datapath.ToNativeText());
        }
        protected AudioClip AudioClipFromDatapathHelper(NativeText datapath) {
            string url = "file:///" + datapath.ToString();
            UnityWebRequest audioFile = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV);
            //audioFile.timeout = 10; // timeout in ten seconds
            audioFile.SendWebRequest();
            while (!audioFile.isDone) {

                // FIXME

                Debug.Log("blocking");
                // block
            }

            if (audioFile.isNetworkError) {
                throw new System.Exception(audioFile.error);
            }
            if (audioFile.isHttpError) {
                throw new System.Exception(audioFile.responseCode.ToString());
            }

            datapath.Dispose();
            return DownloadHandlerAudioClip.GetContent(audioFile);
        }
    }

}