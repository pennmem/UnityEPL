using System;
using UnityEngine;
using UnityEngine.Video;

public class VideoControl : MonoBehaviour
{
    public UnityEngine.RectTransform videoTransform;
    private UnityEngine.KeyCode pauseToggleKey;
    private UnityEngine.KeyCode deactivateKey;
    public UnityEngine.Video.VideoPlayer videoPlayer;
    public Action callback = null;

    public bool skippable;

    void Update()
    {
        if (Input.GetKeyDown(pauseToggleKey))
        {
            if (videoPlayer.isPlaying) {
                videoPlayer.Pause();
            }
            else {
                videoPlayer.Play();
            }
        }
        if (skippable && Input.GetKeyDown(deactivateKey))
        {
            videoPlayer.Stop();
            gameObject.SetActive(false);
        }
    }

    void OnDisable() {
        // clear player
        RenderTexture.active = videoPlayer.targetTexture;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = null;;

        if(callback != null) {
            callback();
        }
        callback = null;
    }

    public void Start() {
        pauseToggleKey = KeyCode.P;
        deactivateKey = KeyCode.Space;
        videoPlayer.loopPointReached += (vp) => gameObject.SetActive(false);
    }

    public void StartVideo(string video, bool _skippable, Action onDone) {
        videoPlayer.url = "file://" + video;

        callback = onDone;
        gameObject.SetActive(true);

        skippable = _skippable;
        GameObject textDisplay = GameObject.Find(gameObject.name).transform.Find("VideoPlayerCanvas").transform.Find("Text").gameObject;
        textDisplay.SetActive(skippable);

        videoPlayer.Play();
    }

    // legacy support
    public void StartVideo() {
        skippable = true;
        gameObject.SetActive(true);
        videoPlayer.Play();
    }
    
    public bool IsPlaying()
    {
        return gameObject.activeSelf;
    }

}
