using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this superclass implements an interface for retrieving behavioral events from a queue
public abstract class DataReporter : MonoBehaviour
{
    private static System.DateTime realWorldStartTime;

    private static bool nativePluginRunning = false;
    private static bool startTimeInitialized = false;

    protected System.Collections.Generic.Queue<DataPoint> eventQueue = new Queue<DataPoint>();

    private static double OSStartTime;
    private static float unityTimeStartTime;

    protected bool IsMacOS()
    {
        return Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer;
    }

    void Awake()
    {
        if (!startTimeInitialized)
        {
            realWorldStartTime = System.DateTime.UtcNow;
            unityTimeStartTime = Time.realtimeSinceStartup;
            startTimeInitialized = true;
        }

        if (IsMacOS() && !nativePluginRunning)
        {
            OSStartTime = UnityEPL.StartCocoaPlugin();
            nativePluginRunning = true;
        }

        if (QualitySettings.vSyncCount == 0)
            Debug.LogWarning("vSync is off!  This will cause tearing, which will prevent meaningful reporting of frame-based time data.");
    }

    void OnDestroy()
    {
        if (IsMacOS() && nativePluginRunning)
        {
            UnityEPL.StopCocoaPlugin();
            nativePluginRunning = false;
        }
    }

    /// <summary>
    /// The number of data points currently queued in this object.
    /// 
    /// Datapoints are dequeued when read. (Usually when  handled by a DataHandler.)
    /// </summary>
    /// <returns>The data point count.</returns>
    public int UnreadDataPointCount()
    {
        return eventQueue.Count;
    }

    /// <summary>
    /// If you want to be responsible yourself for handling data points, instead of letting DataHandlers handle them, you can call this.
    /// 
    /// Read datapoints will be dequeueud and not read by other users of this object.
    /// </summary>
    /// <returns>The data points.</returns>
    /// <param name="count">How many data points to read.</param>
    public DataPoint[] ReadDataPoints(int count)
    {
        if (eventQueue.Count < count)
        {
            throw new UnityException("Not enough data points!  Check UnreadDataPointCount first.");
        }

        DataPoint[] dataPoints = new DataPoint[count];
        for (int i = 0; i < count; i++)
        {
            dataPoints[i] = eventQueue.Dequeue();
        }

        return dataPoints;
    }

    protected System.DateTime RealWorldFrameDisplayTime()
    {
        double secondsSinceUnityStart = Time.unscaledTime - unityTimeStartTime;
        return GetStartTime().AddSeconds(secondsSinceUnityStart);
    }

    protected System.DateTime OSXTimestampToTimestamp(double OSXTimestamp)
    {
        double secondsSinceOSStart = OSXTimestamp - OSStartTime;
        return GetStartTime().AddSeconds(secondsSinceOSStart);
    }

    /// <summary>
    /// Returns the System.DateTime time at startup plus the (higher precision) unity time elapsed since startup, resulting in a value representing the current real world time.
    /// </summary>
    /// <returns>The real world time.</returns>
    public static System.DateTime RealWorldTime()
    {
        double secondsSinceUnityStart = Time.realtimeSinceStartup - unityTimeStartTime;
        return GetStartTime().AddSeconds(secondsSinceUnityStart);
    }


    public static System.DateTime GetStartTime()
    {
        return realWorldStartTime;
    }
}