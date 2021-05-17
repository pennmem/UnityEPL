# UnityEPL-FR
FR experimental tasks built using UnityEPL

Please navigate to UnityEPL-FR/Assets/Scripts/FRExperimentSettings.cs for documentation of FR experiment design.

# Adding a new experiment
To add a new experiment, you will need to modify FRExperimentSettings.cs.

First, create a static method of the FRExperimentSettings that creates and returns a ExperimentSettings struct.  This function should have three parts:
  1. Declare the settings struct:
```
ExperimentSettings MyNewSettings = new ExperimentSettings();
```
Alternatively, base your settings off an existing experiment.  For example:
     
```
ExperimentSettings MyNewSettings = GetFR1Settings();
```
  2. Adjust the member variables of the ExperimentSettings object according to your need.
        For a full account of what each of the member variables does, refer to the comments in the ExperimentSettings struct code.
  3. Return the settings struct:
```
return MyNewSettings;
```
        
Second, add your new settings to the activeExperimentSettings member array of FRExperimentSettings.  For example:
```
private static ExperimentSettings[] activeExperimentSettings = { GetFR1Settings(),
                                                                 GetCatFR1Settings (),
                                                                 GetFR6Settings(),
                                                                 GetCatFR6Settings(),
                                                                 GetPS5Settings(),
                                                                 GetCatPS5Settings(),
                                                                 GetPS4Settings(),
                                                                 GetCatPS4Settings(),
                                                                 GetTICLFRSettings(),
                                                                 GetTICLCatFRSettings(),
                                                                 GetTestFR1Settings(),
                                                                 GetMyNewSettings() }; //THIS IS A CALL TO THE METHOD YOU CREATED!
```

# Prefabs maintained with this project
RamulatorInterface, VAD, and WordListGenerator are maintained with this project.  For notes on using these prefabs, please see the notes in /Assets/Prefab/ThePrefab/ .
