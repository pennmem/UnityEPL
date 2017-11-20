using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoControl : MonoBehaviour
{
    public UnityEngine.KeyCode pauseToggleKey = UnityEngine.KeyCode.Space;
    public UnityEngine.KeyCode deactivateKey = UnityEngine.KeyCode.Escape;
    public UnityEngine.Video.VideoPlayer videoPlayer;
    public bool deactivateWhenFinished = true;

    void Update()
    {
        if (Input.GetKeyDown(pauseToggleKey))
        {
            if (videoPlayer.isPlaying)
                videoPlayer.Pause();
            else
                videoPlayer.Play();
        }
        if (Input.GetKeyDown(deactivateKey))
        {
            videoPlayer.Stop();
            gameObject.SetActive(false);
        }
        if (videoPlayer.time >= videoPlayer.clip.length)
        {
            gameObject.SetActive(false);
        }
    }

    public void StartVideo()
    {
        gameObject.SetActive(true);
    }

    public bool IsPlaying()
    {
        return gameObject.activeSelf;
    }
}
