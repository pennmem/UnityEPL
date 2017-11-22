using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[AddComponentMenu("UnityEPL/Reporters/Input Reporter")]
public class InputReporter : DataReporter
{

    public bool reportKeyStrokes = false;
    public bool reportMouseClicks = false;
    public bool reportMousePosition = false;
    public int framesPerMousePositionReport = 60;
    private Dictionary<int, bool> keyDownStates = new Dictionary<int, bool>();
    private Dictionary<int, bool> mouseDownStates = new Dictionary<int, bool>();

    private int lastMousePositionReportFrame;

    void Update()
    {
        if (reportMouseClicks)
            CollectMouseEvents();
        if (reportKeyStrokes)
            CollectKeyEvents();
        if (reportMousePosition && Time.frameCount - lastMousePositionReportFrame > framesPerMousePositionReport)
            CollectMousePosition();
    }

    void CollectMouseEvents()
    {
        if (IsMacOS())
        {
            int eventCount = UnityEPL.CountMouseEvents();
            if (eventCount >= 1)
            {
                int mouseButton = UnityEPL.PopMouseButton();
                double timestamp = UnityEPL.PopMouseTimestamp();
                bool downState;
                mouseDownStates.TryGetValue(mouseButton, out downState);
                mouseDownStates[mouseButton] = !downState;
                ReportMouse(mouseButton, mouseDownStates[mouseButton], OSXTimestampToTimestamp(timestamp));
            }
        }
    }

    private void ReportMouse(int mouseButton, bool pressed, System.DateTime timestamp)
    {
        Dictionary<string, object> dataDict = new Dictionary<string, object>();
        dataDict.Add("key code", mouseButton);
        dataDict.Add("is pressed", pressed);
        eventQueue.Enqueue(new DataPoint("mouse press/release", timestamp, dataDict));
    }

    void CollectKeyEvents()
    {
        if (IsMacOS())
        {
            int eventCount = UnityEPL.CountKeyEvents();
            if (eventCount >= 1)
            {
                int keyCode = UnityEPL.PopKeyKeycode();
                double timestamp = UnityEPL.PopKeyTimestamp();
                bool downState;
                keyDownStates.TryGetValue(keyCode, out downState);
                keyDownStates[keyCode] = !downState;
                ReportKey(keyCode, keyDownStates[keyCode], OSXTimestampToTimestamp(timestamp));
            }
        }
        else
        {
            foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(keyCode))
                {
                    ReportKey((int)keyCode, true, DataReporter.RealWorldTime());
                }
                if (Input.GetKeyUp(keyCode))
                {
                    ReportKey((int)keyCode, false, DataReporter.RealWorldTime());
                }
            }
        }
    }

    private void ReportKey(int keyCode, bool pressed, System.DateTime timestamp)
    {
        Dictionary<string, object> dataDict = new Dictionary<string, object>();
        dataDict.Add("key code", keyCode);
        dataDict.Add("is pressed", pressed);
        string label = "key press/release";
        if (!IsMacOS())
            label = "key/mouse press/release";
        eventQueue.Enqueue(new DataPoint(label, timestamp, dataDict));
    }

    void CollectMousePosition()
    {
        Dictionary<string, object> dataDict = new Dictionary<string, object>();
        dataDict.Add("position", Input.mousePosition);
        eventQueue.Enqueue(new DataPoint("mouse position", DataReporter.RealWorldTime(), dataDict));
        lastMousePositionReportFrame = Time.frameCount;
    }
}