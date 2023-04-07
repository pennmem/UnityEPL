using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEPL {

    [AddComponentMenu("UnityEPL/Reporters/World Data Reporter2")]
    public class WorldDataReporter : DataReporter {
        public bool reportTransform = true;
        public int framesPerTransformReport = 30;
        public bool reportView = true;
        public int framesPerViewReport = 30;

        private Dictionary<Camera, bool> camerasToInViewfield = new Dictionary<Camera, bool>();

        void Update() {
            if (reportTransform) CheckTransformReport();
            if (reportView) CheckViewReport();
        }

        void Start() {
            if (reportView && GetComponent<Collider>() == null) {
                throw new UnityException("You have selected enter/exit viewfield reporting for " + gameObject.name + " but there is no collider on the object." +
                                          "  This feature uses collision detection to compare with camera bounds and other objects.  Please add a collider or " +
                                          "unselect viewfield enter/exit reporting.");
            }
        }

        public void DoTransformReport(System.Collections.Generic.Dictionary<string, object> extraData = null) {
            if (extraData == null)
                extraData = new Dictionary<string, object>();
            System.Collections.Generic.Dictionary<string, object> transformDict = new System.Collections.Generic.Dictionary<string, object>(extraData);
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
            eventQueue.Enqueue(new DataPoint(gameObject.name + " transform", TimeStamp(), transformDict));
        }

        private void CheckTransformReport() {
            if (Time.frameCount % framesPerTransformReport == 0) {
                DoTransformReport();
            }
        }

        private void CheckViewReport() {
            if (Time.frameCount % framesPerViewReport == 0) {
                DoViewReport();
            }

            //untested accuraccy, requires collider
            void DoViewReport() {
                Camera[] cameras = FindObjectsOfType<Camera>();

                foreach (Camera thisCamera in cameras) {
                    Plane[] frustrumPlanes = GeometryUtility.CalculateFrustumPlanes(thisCamera);
                    Collider objectCollider = GetComponent<Collider>();

                    RaycastHit lineOfSightHit;

                    // raycast to center mass
                    Physics.Linecast(thisCamera.transform.position, gameObject.transform.position, out lineOfSightHit);
                    bool lineOfSight = lineOfSightHit.collider.Equals(gameObject.GetComponent<Collider>());
                    bool inView = GeometryUtility.TestPlanesAABB(frustrumPlanes, objectCollider.bounds) && lineOfSight;

                    string eventName = "";

                    if (!reportView)
                        continue;

                    Dictionary<string, object> dataDict = new Dictionary<string, object>();
                    dataDict.Add("cameraName", thisCamera.name);
                    dataDict.Add("isInView", inView);
                    eventName = gameObject.name.ToLower() + "InView";
                    eventQueue.Enqueue(new DataPoint(eventName, TimeStamp(), dataDict));
                }
            }
        }
    }

}