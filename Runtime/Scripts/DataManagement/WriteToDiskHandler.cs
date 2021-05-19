﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("UnityEPL/Handlers/Updated Write to Disk Handler")]
public class WriteToDiskHandler : DataHandler
{
    //more output formats may be added in the future
    public enum FORMAT { JSON_LINES };
    public FORMAT outputFormat;

    [HideInInspector]
    [SerializeField]
    private bool writeAutomatically = true;
    [HideInInspector]
    [SerializeField]
    private int framesPerWrite = 30;

    private System.Collections.Generic.Queue<DataPoint> waitingPoints = new System.Collections.Generic.Queue<DataPoint>();

    public void SetWriteAutomatically(bool newAutomatically)
    {
        writeAutomatically = newAutomatically;
    }
    public bool WriteAutomatically()
    {
        return writeAutomatically;
    }
    public void SetFramesPerWrite(int newFrames)
    {
        if (newFrames > 0)
            framesPerWrite = newFrames;
    }
    public int GetFramesPerWrite()
    {
        return framesPerWrite;
    }

    protected override void Update()
    {
        base.Update();

        if (Time.frameCount % framesPerWrite == 0)
            DoWrite();
    }

    protected override void HandleDataPoints(DataPoint[] dataPoints)
    {
        foreach (DataPoint dataPoint in dataPoints)
            waitingPoints.Enqueue(dataPoint);
    }

    /// <summary>
    /// Writes data from the waitingPoints queue to disk.  The waitingPoints queue will be automatically updated whenever reporters report data.
    /// 
    /// DoWrite() will also be automatically be called periodically according to the settings in the component inspector window, but you can invoke this manually if desired.
    /// </summary>
    public void DoWrite()
    {
        while (waitingPoints.Count > 0)
        {
            string directory = im.fileManager.SessionPath(); 
            if(directory == null) {
                return;
            }
            System.IO.Directory.CreateDirectory(directory);
            string filePath = System.IO.Path.Combine(directory, "unnamed_file");

            DataPoint dataPoint = waitingPoints.Dequeue();
            string writeMe = "unrecognized type";
            string extensionlessFileName = "session";//DataReporter.GetStartTime ().ToString("yyyy-MM-dd HH mm ss");
            switch (outputFormat)
            {
                case FORMAT.JSON_LINES:
                    writeMe = dataPoint.ToJSON();
                    filePath = System.IO.Path.Combine(directory, extensionlessFileName + ".jsonl");
                    break;
            }
            System.IO.File.AppendAllText(filePath, writeMe + System.Environment.NewLine);
        }
    }
}