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

    private bool wasLastVisible = false;

    void Update()
    {
        if (reportTransform) CheckTransformReporting();
        if (reportEntersView || reportLeavesView) CheckView();
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

    private void CheckView()
    {
        bool isVisible = IsVisible();
        string eventName = "unnamed event";
        if (isVisible == true && wasLastVisible == false && reportEntersView)
        {
            eventName = reportingID + " enters view";
            eventQueue.Enqueue(new DataPoint(eventName, RealWorldFrameDisplayTime(), new Dictionary<string, object>() { { "object reporting id", reportingID } }));
        }
        if (isVisible == false && wasLastVisible == true && reportLeavesView)
        {
            eventName = reportingID + " leaves view";
            eventQueue.Enqueue(new DataPoint(eventName, RealWorldFrameDisplayTime(), new Dictionary<string, object>() { { "object reporting id", reportingID } }));
        }
        wasLastVisible = isVisible;
    }

    private bool IsVisible()
    {
        bool thisIsVisible = false;
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        List<bool> renderesVisible = new List<bool>();
        foreach (Renderer theRenderer in renderers)
            renderesVisible.Add(theRenderer.isVisible);
        foreach (bool isVisible in renderesVisible)
            thisIsVisible |= isVisible;
        return thisIsVisible;
    }
}