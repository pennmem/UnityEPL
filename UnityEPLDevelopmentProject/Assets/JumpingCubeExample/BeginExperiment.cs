using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeginExperiment : MonoBehaviour 
{
    public UnityEngine.UI.InputField participantNameInput;

    public void Begin()
    {
        UnityEPL.SetExperimentName("Jumping Cube Experiment");
        UnityEPL.AddParticipant(participantNameInput.text);
    }
}
