using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("UnityEPL/Reporters/World Data Reporter")]
public class WorldDataReporter : DataReporter
{
    public string reportingID = "Object ID not set.";
    public bool reportTransform = true;
    public int framesPerTransformReport = 60;
    public bool reportEntersView = true;
    public bool reportLeavesView = true;

    private Dictionary<Camera, bool> camerasToInViewfield = new Dictionary<Camera, bool>();

    void Update()
    {
        if (reportTransform) CheckTransformReporting();
        if (reportEntersView || reportLeavesView) CheckView();
    }

    void Start()
    {
        if ((reportEntersView || reportLeavesView) && GetComponent<Collider>() == null)
        {
            throw new UnityException("You have selected enter/exit viewfield reporting for " + gameObject.name + " but there is no collider on the object." +
                                      "  This feature uses collision detection to compare with camera bounds and other objects.  Please add a collider or " +
                                      "unselect viewfield enter/exit reporting.");
        }
    }

    public void DoReport(System.Collections.Generic.Dictionary<string, object> extraData = null)
    {
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
        eventQueue.Enqueue(new DataPoint(gameObject.name + " transform", RealWorldFrameDisplayTime(), transformDict));
    }

    private void CheckTransformReporting()
    {
        if (Time.frameCount % framesPerTransformReport == 0)
        {
            DoReport();
        }
    }

    //untested accuraccy, requires collider
    void CheckView()
    {
        bool enteredViewfield = false;
        bool leftViewfield = false;

        Camera[] cameras = FindObjectsOfType<Camera>();

        foreach (Camera thisCamera in cameras)
        {
            Plane[] frustrumPlanes = GeometryUtility.CalculateFrustumPlanes(thisCamera);
            Collider objectCollider = GetComponent<Collider>();

            RaycastHit lineOfSightHit;
            Physics.Linecast(thisCamera.transform.position, gameObject.transform.position, out lineOfSightHit);
            bool lineOfSight = lineOfSightHit.collider.Equals(gameObject.GetComponent<Collider>());
            bool inView = GeometryUtility.TestPlanesAABB(frustrumPlanes, objectCollider.bounds) && lineOfSight;
            if (inView && (!camerasToInViewfield.ContainsKey(thisCamera) || camerasToInViewfield[thisCamera] == false))
            {
                camerasToInViewfield[thisCamera] = true;
                enteredViewfield = true;
            }
            else if (!inView && camerasToInViewfield.ContainsKey(thisCamera) && camerasToInViewfield[thisCamera] == true)
            {
                camerasToInViewfield[thisCamera] = false;
                leftViewfield = true;
            }

            string eventName = "";


            if (!(enteredViewfield || leftViewfield))
                continue;

            Dictionary<string, object> dataDict = new Dictionary<string, object>();
            dataDict.Add("camera", thisCamera.name);
            if (enteredViewfield && reportEntersView)
            {
                eventName = gameObject.name + " enters view";
                eventQueue.Enqueue(new DataPoint(eventName, RealWorldFrameDisplayTime(), dataDict));
            }
            if (leftViewfield && reportLeavesView)
            {
                eventName = gameObject.name + " leaves view";
                eventQueue.Enqueue(new DataPoint(eventName, RealWorldFrameDisplayTime(), dataDict));
            }
        }
    }
}