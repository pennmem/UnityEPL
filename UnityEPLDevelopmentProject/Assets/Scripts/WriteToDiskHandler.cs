using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("UnityEPL/Handlers/Write to Disk Handler")]
public class WriteToDiskHandler : DataHandler 
{
	public enum FORMAT { JSON, CSV, SQL };
	public FORMAT outputFormat;

	[HideInInspector]
	[SerializeField]
	private bool writeAutomatically = true;
	[HideInInspector]
	[SerializeField]
	private int framesPerWrite = 30;

	private System.Collections.Generic.List<DataPoint> waitingPoints = new System.Collections.Generic.List<DataPoint>();


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
		base.Update ();

		if (Time.frameCount % framesPerWrite == 0)
			DoWrite ();
	}

	protected override void HandleDataPoints(DataPoint[] dataPoints)
	{
		waitingPoints.AddRange (dataPoints);
	}

	public void DoWrite()
	{
		string directory = UnityEPL.GetDataPath();
		System.IO.Directory.CreateDirectory (directory);
		string filePath = System.IO.Path.Combine (directory, "unnamed_file");

		foreach (DataPoint dataPoint in waitingPoints)
		{
			string writeMe = "unrecognized type";
			string extensionlessFileName = DataReporter.GetStartTime ().ToString("yyyy-MM-dd HH mm ss");
			switch (outputFormat)
			{
				case FORMAT.CSV:
					writeMe = dataPoint.ToCSV ();
					filePath = System.IO.Path.Combine(directory, extensionlessFileName + ".csv");
					break;
				case FORMAT.JSON:
					writeMe = dataPoint.ToJSON ();
					filePath = System.IO.Path.Combine(directory, extensionlessFileName + ".json");
					break;
				case FORMAT.SQL:
					writeMe = dataPoint.ToSQL ();
					filePath = System.IO.Path.Combine(directory, extensionlessFileName + ".sql");
					break;
			}
			System.IO.File.AppendAllText(filePath, writeMe + System.Environment.NewLine);
		}
	}
}