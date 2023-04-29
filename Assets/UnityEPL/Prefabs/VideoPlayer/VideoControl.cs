using System;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Video;

namespace UnityEPL {

    public class VideoControl : EventMonoBehaviour {
        protected override void AwakeOverride() { }

        public RectTransform videoTransform;
        public VideoPlayer videoPlayer;

        protected bool skippable;
        protected KeyCode pauseToggleKey = KeyCode.P;
        protected KeyCode deactivateKey = KeyCode.Space;
        protected TaskCompletionSource<bool> videoFinished;

        protected void Update() {
            if (Input.GetKeyDown(pauseToggleKey)) {
                if (videoPlayer.isPlaying) {
                    videoPlayer.Pause();
                } else {
                    videoPlayer.Play();
                }
            }
            if (skippable && Input.GetKeyDown(deactivateKey)) {
                videoPlayer.Stop();
                OnLoopPointReached(videoPlayer);
            }
        }

        protected void OnEnable() {
            videoPlayer.loopPointReached += OnLoopPointReached;
            videoPlayer.errorReceived += OnErrorReceived;
        }

        protected void OnDisable() {
            // clear player
            RenderTexture.active = videoPlayer.targetTexture;
            GL.Clear(true, true, Color.black);
            RenderTexture.active = null;
            videoPlayer.loopPointReached -= OnLoopPointReached;
            videoPlayer.errorReceived -= OnErrorReceived;
        }

        public void SetVideo(string videoPath, bool skippable) {
            Do(SetVideoHelper, videoPath.ToNativeText(), (Bool)skippable);
        }
        public void SetVideoMB(string videoPath, bool skippable) {
            DoMB(SetVideoHelper, videoPath.ToNativeText(), (Bool)skippable);
        }
        protected void SetVideoHelper(NativeText videoPath, Bool skippable) {
            this.videoPlayer.url = "file://" + videoPath;
            this.skippable = skippable;
            videoPath.Dispose();
        }

        public Task PlayVideo() {
            return DoWaitFor(PlayVideoHelper);
        }
        public Task PlayVideoMB() {
            return DoWaitForMB(PlayVideoHelper);
        }
        protected async Task PlayVideoHelper() {
            videoFinished = new();

            gameObject.SetActive(true);
            GameObject textDisplay = GameObject.Find(gameObject.name).transform.Find("VideoPlayerCanvas").transform.Find("Text").gameObject;
            textDisplay.SetActive(skippable);

            videoPlayer.Play();
            await videoFinished.Task;
        }

        public async Task<bool> IsPlaying() {
            return await DoGet(IsPlayingHelper);
        }
        public bool IsPlayingMB() {
            return DoGetMB(IsPlayingHelper);
        }
        protected Bool IsPlayingHelper() {
            return gameObject.activeSelf;
        }

        protected void OnLoopPointReached(VideoPlayer vp) {
            gameObject.SetActive(false);
            videoFinished.SetResult(true);
        }
        protected void OnErrorReceived(VideoPlayer vp, string message) {
            gameObject.SetActive(false);
            videoFinished.SetException(new Exception(message));
        }
    }

}