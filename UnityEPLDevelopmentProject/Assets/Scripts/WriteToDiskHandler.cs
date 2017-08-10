using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[AddComponentMenu("UnityEPL/Handlers/Write to Disk Handler")]
public class WriteToDiskHandler : DataHandler 
{
	public enum FORMAT { JSON, CSV, SQL };
	public FORMAT outputFormat;

	private string rootDirectory = "~/Desktop";

	private bool useDirectoryStructure = false;
	private bool participantFirst = false;

	private bool writeAutomatically = true;
	private int framesPerWrite = 30;

	private System.Collections.Generic.List<DataPoint> waitingPoints = new System.Collections.Generic.List<DataPoint>();

	public void SetRootDirectory(string newPath)
	{ 
		rootDirectory = newPath;
	}
	public string GetRootDirectory()
	{
		return rootDirectory;
	}
	public void SetUseDirectoryStructure(bool newUse)
	{
		useDirectoryStructure = newUse;
	}
	public void SetParticipantFirst(bool isFirst)
	{
		participantFirst = isFirst;
	}
	public bool UseDirectoryStructure()
	{
		return useDirectoryStructure;
	}
	public bool ParticipantFirst()
	{
		return participantFirst;
	}

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



	void Update()
	{
		if (Time.frameCount % framesPerWrite == 0)
			DoWrite ();
	}

	protected override void HandleDataPoints(DataPoint[] dataPoints)
	{
		waitingPoints.AddRange (dataPoints);
	}

	public void DoWrite()
	{
		//do write
	}
}