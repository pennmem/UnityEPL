using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public static class UnityEPL
{
    private static string[] participants = null;
    private static string experiment = null;
    private static string dataPath = null;
    private static int sessionNumber = -1;

    //iPhones require special DLLImport due to static linkage
    //Add this to other external functions if adding iPhone support
#if UNITY_IPHONE
	[DllImport ("__Internal")]
#else
    [DllImport("UnityEPLNativePlugin")]
#endif
    public static extern double StartCocoaPlugin();

    [DllImport("UnityEPLNativePlugin")]
    public static extern void StopCocoaPlugin();

    [DllImport("UnityEPLNativePlugin")]
    public static extern int PopKeyKeycode();

    [DllImport("UnityEPLNativePlugin")]
    public static extern double PopKeyTimestamp();

    [DllImport("UnityEPLNativePlugin")]
    public static extern int CountKeyEvents();

    [DllImport("UnityEPLNativePlugin")]
    public static extern int PopMouseButton();

    [DllImport("UnityEPLNativePlugin")]
    public static extern double PopMouseTimestamp();

    [DllImport("UnityEPLNativePlugin")]
    public static extern int CountMouseEvents();

    /// <summary>
    /// Adds the given string as the name of a current participant.  Output is separated into folders for each participant combination.  Participant names are also included in output logs.
    /// </summary>
    /// <param name="participant_ID">Participant identifier.</param>
    public static void AddParticipant(string participant_ID)
    {
        string[] new_participants;
        if (participants == null)
        {
            new_participants = new string[] { participant_ID };
        }
        else
        {
            new_participants = new string[participants.Length + 1];
            for (int i = 0; i < participants.Length; i++)
            {
                new_participants[i] = participants[i];
            }
            new_participants[participants.Length] = participant_ID;
        }
        participants = new_participants;
    }

    /// <summary>
    /// Clears the current participants.  The default of "unspecified_participant" will then be used if no more participants are specified.
    /// </summary>
    public static void ClearParticipants()
    {
        participants = null;
    }

    /// <summary>
    /// Returns an array of all the current participants.
    /// </summary>
    /// <returns>The participants.</returns>
    public static string[] GetParticipants()
    {
        if (participants == null)
            return new string[] { "unspecified_participant" };
        return participants;
    }

    /// <summary>
    /// Sets the name of the experiment.  Output is separated into folders for each experiment.  The current experiment name is also included in output logs.
    /// </summary>
    /// <param name="experimentName">Experiment name.</param>
    public static void SetExperimentName(string experimentName)
    {
        experiment = experimentName;
    }

    /// <summary>
    /// Gets the name of the experiment.
    /// </summary>
    /// <returns>The experiment name.</returns>
    public static string GetExperimentName()
    {
        if (experiment == null)
            return "unspecified_experiment";
        return experiment;
    }

    public static double MillisecondsSinceTheEpoch()
    {
        return DataPoint.ConvertToMillisecondsSinceEpoch(DataReporter.RealWorldTime());
    }

    /// <summary>
    /// Gets the data output folder for the current experiment and participant.
    /// 
    /// The default output folder is the folder where the application is running, plus /data/experiment/participant/session_#/session.json.  If you have already overridden the default, however, this will throw a warning and return your specified path.
    /// </summary>
    /// <returns>The participant folder.</returns>
    public static string GetParticipantFolder()
    {
        if (dataPath != null)
        {
            Debug.LogWarning("You have already set a non-default data path.  Returning that instead.");
            return dataPath;
        }

        string defaultRoot = "";
        if (Application.isEditor)
        {
            defaultRoot = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        }
        else
        {
            defaultRoot = System.IO.Path.GetFullPath(".");
        }
        defaultRoot = System.IO.Path.Combine(defaultRoot, "data");

        string directory = System.IO.Path.Combine(defaultRoot, UnityEPL.GetExperimentName());
        directory = System.IO.Path.Combine(directory, string.Join("", UnityEPL.GetParticipants()));
        return directory;
    }

    /// <summary>
    /// Gets the file path to the current output log.
    /// </summary>
    /// <returns>The data path.</returns>
    public static string GetDataPath()
    {
        if (dataPath != null)
            return dataPath;

        string directory = GetParticipantFolder();
        if (sessionNumber != -1)
            directory = System.IO.Path.Combine(directory, "session_" + sessionNumber.ToString());
        return directory;
    }

    /// <summary>
    /// Sets a new data path for the output log.  This will override the default.
    /// </summary>
    /// <param name="newDataPath">New data path.</param>
    public static void SetDataPath(string newDataPath)
    {
        dataPath = newDataPath;
    }

    /// <summary>
    /// Sets the session number.  The session number is used to organize output into folders on a per participant basis.
    /// 
    /// The default output folder is the folder where the application is running, plus /data/experiment/participant/session_#/session.json  If no session is specified, however, no session folder will be created and data will be output directly into the participant folder.
    /// </summary>
    /// <param name="newSessionNumber">New session number.</param>
    public static void SetSessionNumber(int newSessionNumber)
    {
        sessionNumber = newSessionNumber;
    }
}