using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEPL {

    /// <summary>
    /// This is attached to the dropdown menu which selects experiments.
    /// 
    /// It only needs to call UnityEPL.SetExperimentName().
    /// </summary>
    [RequireComponent(typeof(Dropdown))]
    public class ExperimentSelection : EventMonoBehaviour {
        protected override void AwakeOverride() {
            Dropdown dropdown = GetComponent<Dropdown>();

            List<string> experiments = new(Config.availableExperiments);

            dropdown.AddOptions(new List<string>(new string[] { "Select Task..." }));
            dropdown.AddOptions(experiments);
            SetExperimentMB();
        }


        public void SetExperimentMB() {
            DoMB(SetExperimentHelper);
        }
        protected void SetExperimentHelper() {
            Dropdown dropdown = GetComponent<Dropdown>();

            if (dropdown.captionText.text != "Select Task...") {
                Debug.Log("Task chosen");

                manager.LoadExperimentConfig(dropdown.captionText.text);
            }
        }
    }

}