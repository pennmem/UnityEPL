using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEPL {

    [AddComponentMenu("UnityEPL/Reporters/World Data Reporter")]
    public class WorldDataReporter : DataReporter {
        public bool reportTransform = true;
        public int framesPerTransformReport = 30;
        public bool reportView = true;
        public int framesPerViewReport = 30;

        private Dictionary<Camera, bool> camerasToInViewfield = new();

        void Update() {
            if (reportTransform) CheckTransformReport();
            if (reportView) CheckViewReport();
        }

        void Start() {
            if (reportView && GetComponent<Collider>() == null) {
                ErrorNotifier.Error(
                    new UnityException("You have selected enter/exit viewfield reporting for " + gameObject.name + " but there is no collider on the object. " +
                                       "This feature uses collision detection to compare with camera bounds and other objects.  Please add a collider or " +
                                       "unselect viewfield enter/exit reporting."));
            }
        }


        public void DoTransformReportMB(Dictionary<string, object> extraData = null) {
            DoMB(DoTransformReportHelper, extraData);
        }
        public void DoTransformReportHelper(Dictionary<string, object> extraData = null) {
            var transformDict = (extraData != null) ? new Dictionary<string, object>(extraData) : new();
            transformDict.Add("positionX", transform.position.x);
            transformDict.Add("positionY", transform.position.y);
            transformDict.Add("positionZ", transform.position.z);
            transformDict.Add("rotationX", transform.position.x);
            transformDict.Add("rotationY", transform.position.y);
            transformDict.Add("rotationZ", transform.position.z);
            transformDict.Add("scaleX", transform.position.x);
            transformDict.Add("scaleY", transform.position.y);
            transformDict.Add("scaleZ", transform.position.z);
            transformDict.Add("object reporting id", reportingID);
            eventQueue.Enqueue(new DataPoint(gameObject.name + " transform", transformDict));
        }

        private void CheckTransformReport() {
            if (Time.frameCount % framesPerTransformReport == 0) {
                DoTransformReportMB();
            }
        }

        private void CheckViewReport() {
            if (Time.frameCount % framesPerViewReport == 0) {
                DoViewReport();
            }
        }

        //untested accuraccy, requires collider
        private void DoViewReport() {
            Camera[] cameras = FindObjectsOfType<Camera>();

            foreach (Camera thisCamera in cameras) {
                Plane[] frustrumPlanes = GeometryUtility.CalculateFrustumPlanes(thisCamera);
                Collider objectCollider = GetComponent<Collider>();

                // raycast to center mass
                Physics.Linecast(thisCamera.transform.position, gameObject.transform.position, out RaycastHit lineOfSightHit);
                bool lineOfSight = lineOfSightHit.collider.Equals(gameObject.GetComponent<Collider>());
                bool inView = GeometryUtility.TestPlanesAABB(frustrumPlanes, objectCollider.bounds) && lineOfSight;

                string eventName = "";

                if (!reportView)
                    continue;

                Dictionary<string, object> dataDict = new() {
                    { "cameraName", thisCamera.name },
                    { "isInView", inView },
                };
                eventName = gameObject.name.ToLower() + "InView";
                eventQueue.Enqueue(new DataPoint(eventName, dataDict));
            }
        }
    }

}