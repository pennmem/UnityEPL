using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEPL {

    [RequireComponent(typeof(UnityEngine.UI.Text))]
    public class BuildDateDisplayer : EventMonoBehaviour {
        protected override void AwakeOverride() {}

        [Tooltip("Date/time format.")]
        public string format = "g";    // see: https://msdn.microsoft.com/en-us/library/az4se3k1%28v=vs.110%29.aspx

        void Start() {
            GetComponent<UnityEngine.UI.Text>().text = BuildInfo.ToString(format);
        }
    }
}
