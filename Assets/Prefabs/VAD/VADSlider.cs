using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VADSlider : MonoBehaviour
{
	public UnityEngine.UI.Slider slider;
	public UnityEngine.UI.Text display;
	public VoiceActivityDetection VAD;

	public void OnSliderChanged()
	{
		float value = slider.value;
		display.text = value.ToString ("#.####");
		VAD.speakingThreshold = value;
	}

}