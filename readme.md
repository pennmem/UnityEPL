# UnityEPL Overview
UnityEPL (Unity Experiment Programming Library) can help researchers and scientists easily collect data from their Unity3D projects.  To use UnityEPL, all you need to do is download the asset from the asset store, import it, and add components to objects in your Unity3D scene.

There are two types of components: reporters and handlers.  Reporters allow you to choose what data you care about in your Unity3D project.  Handlers let you decide what to do with the data.

To add either type of component, select a game object in the object heierarchy (the left panel in the default Unity3D layout).  Then, select "Add Component" in the inspector (the right panel in the default Unity3D layout.)

![alt text](https://github.com/pennmem/UnityEPL/blob/master/images/add_component.png "Adding a UnityEPL component")

In the "Add Component" dropdown, select "UnityEPL," then "Handler" or "Reporter."  You will then be able to configure the component.

## Reporters

![alt text](https://github.com/pennmem/UnityEPL/blob/master/images/reporters.png "UnityEPL data reporters")

There are currently four reporters, each for collecting a different type of data about your project.

### WorldDataReporter

The world data reporter is for recording the position of Unity3D objects that exist inside the 2D or 3D simulation.  Put the component on the object whose world position you are concerned with.  Configure the data reporting parameters according to your preferences by editing the component in the inspector window.

The following parameters are available:
1. Report transform data. Whether or not to report the data contained in the objects transform component (position, rotation, and scale).
2. Frames per transform report.  If reporting transform data is desired, how many frames to wait between each report.  Disabled if report transform data is unchecked.
3. Report upon entering view.  If checked, an event will be logged whenever this object becomes visible to the player.  Please note that this includes children of the object, and uses Unity's renderer.isVisible member, so is affected by, for example, shadows.  More information here: (https://docs.unity3d.com/ScriptReference/Renderer-isVisible.html)
4. Report upon leaving view.  The counterpart to report upon entering view.

### InputReporter

The input reporter is for directly recording key strokes and mouse clicks in your experiment.  UnityEPL uses MacOS native level hooks to report high-accuraccy data on the MacOS platform.  For other platforms, the accuraccy of the data is limited to the frame rate of the project.  For example, 60 frames per second corresponds to 16.6 ms accuraccy for key and mouse events.  Please also note that mouse clicks are treated as keystrokes on non-MacOS platforms.

### ScriptedEventReporter

The scripted event reporter allows you to report custom events from your own scripts.  Call the "ReportScriptedEvent" method, and give it a name for your event, and the data that you want to record.  It will then be processed by any handlers handling the ScriptedEvventReporter.

### UIDataReporter

![alt text](https://github.com/pennmem/UnityEPL/blob/master/images/uidatareporter.png "UnityEPL UIDataReporter usage")

Similar to scripted event reporter, but its LogUIEvent method accepts only a string, so that you can subscribe it to Unity UI buttons in order to automatically report button clicks.

## Handlers

![alt text](https://github.com/pennmem/UnityEPL/blob/master/images/handlers.png "UnityEPL data handlers")

After you have added reporters to your project to select what data you want to collect, use handlers to put the data somewhere for your future analysis.  Currently there are only two options: DebugLog and WriteToDisk.

Every handler handles some subset of your reporters.  Each reporter should only be handled once.  You can click and drag reporters individually into the "reporters to handle" field shown above.  You can also click the "handle all reporters" button to easily have one handler handle all reporters.

### DebugLog

When reporters handled by debug log report data, the debug log handle simply writes the data to the Unity log.  This is useful for editor mode and debugging.  If deployed to an executable, this handler will write Unity's "player log" for the application. (https://docs.unity3d.com/Manual/LogFiles.html)

### WriteToDiskHandler.

This will write your data to UnityEPL's data folder.  The default output folder is the folder where the application is running, plus /data/experiment/participant/session_#/session.json . If no session is specified, however, no session folder will be created and data will be output directly into the participant folder.  The methods of the static UnityEPL class can be used to change this output path.  See "UnityEPL optional methods" below.

Currently, only jsonl output formatting is provided.  Jsonl output can be easily read by data analysis platforms.  For example, the following python code will load in a jsonl file into a pandas data scince dataframe:
``` python
import pandas
file_path = "/Path/to/your/data/session.jsonl"
dataframe = pandas.read_json(path_or_buf=file_path, lines=True)
```

# UnityEPL optional methods

The static class named "UnityEPL" provides optional methods for further managing your data and your experiment.  Although UnityEPL will function without any of these methods being called, you probably want to at least call "AddParticipant" so that your data is not reported under "unspecified participant."

## AddParticipant
One parameter: participant_ID, a string

Adds the given string as the name of a current participant.  Output is separated into folders for each
    
## ClearParticipants
Clears the current participants.  The default of "unspecified_participant" will then be used if no more participants are specified.

## GetParticipants
Returns an array of all the current participants.  This will always have at least "unspecified participant" as an entry.

## SetExperimentName
One parameter: expirmentName, a string

Sets the name of the experiment.  Output is separated into folders for each experiment.  The current experiment name is also included in output logs.

## GetParticipantFolder
The default output folder is the folder where the application is running, plus /data/experiment/participant/session_#/session.json.  If you have already overridden the default, however, this will throw a warning and return your specified path.

## GetDataPath
Gets the data output path. The default output folder is the folder where the application is running, plus /data/experiment/participant/session_#/session.json . If no session is specified, however, no session folder will be created and data will be output directly into the participant folder.

## SetDataPath
One parameter: newDataPath, a string

Sets a new data path for the output log.  This will override the default.

## SetSessionNumber
One parameter: newSessionNumber, an int

Sets the session number.  The session number is used to organize output into folders on a per participant basis.

The default output folder is the folder where the application is running, plus /data/experiment/participant/session_#/session.json . If no session is specified, however, no session folder will be created and data will be output directly into the participant folder.
