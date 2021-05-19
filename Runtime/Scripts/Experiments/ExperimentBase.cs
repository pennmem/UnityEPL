using System;
using System.Dynamic;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UnityEngine;
using KeyAction = System.Func<InputHandler, KeyMsg, bool>;


// TODO: it would be great to have a system that can handle the state more implicitly,
// such as changing it to an object that has Increment and Decrement state functions,
// is aware of the current timeline of the state, and takes a function that can inspect
// the current state and switch timelines
public abstract class ExperimentBase : EventLoop {
    public InterfaceManager manager;
    public GameObject microphoneTestMessage; // set in editor

    // dictionary containing lists of actions indexed
    // by the name of the function incrementing through
    // list of states
    protected StateMachine stateMachine;

    protected InputHandler inputHandler;

    public ExperimentBase(InterfaceManager _manager) {
        manager = _manager;

        inputHandler = new InputHandler(this, null);
        manager.inputHandler.RegisterChild(inputHandler);

        CleanSlate(); // clear display, disable keyHandler
    }

    public abstract StateMachine GetStateMachine();

    // executes state machine current function
    public void Run() {
        SaveState();
        CleanSlate();
        Do(new EventBase<StateMachine>(stateMachine.GetState(), stateMachine));
    }

    protected void CleanSlate() {
        inputHandler.active = false;
        manager.Do(new EventBase(() => {
            manager.ClearText();
            manager.ClearTitle();
        }));
    }

    public void FinishExperiment(StateMachine state) {
        state.PopTimeline(); // empty timeline, so this session won't run on load
        SaveState();
        Quit();
    }

    //////////
    // Worker Functions for common experiment tasks.
    //////////

    protected void IntroductionVideo(StateMachine state) {
        state.IncrementState();
        manager.Do(new EventBase<string, bool, Action>(manager.ShowVideo, 
                                                        "introductionVideo", true,
                                                        () => this.Do(new EventBase(Run))));
    }

    protected void CountdownVideo(StateMachine state) {
        ReportEvent("countdown", new Dictionary<string, object>());
        SendHostPCMessage("COUNTDOWN", null);

        state.IncrementState();
        manager.Do(new EventBase<string, bool, Action>(manager.ShowVideo, 
                                                            "countdownVideo", false, 
                                                            () => this.Do(new EventBase(Run))));
    }

    // NOTE: rather than use flags for the audio test, this is entirely based off of timings.
    // Since there is processing latency (which seems to be unity version dependent), this
    // is really a hack that lets us get through the mic test unscathed. More time critical
    // applications need a different approach
    protected void RecordTest(StateMachine state) {
        string wavPath =  System.IO.Path.Combine(manager.fileManager.SessionPath(), "microphone_test_" 
                    + DataReporter.TimeStamp().ToString("yyyy-MM-dd_HH_mm_ss") + ".wav");

        manager.Do(new EventBase(manager.lowBeep.Play));
        manager.Do(new EventBase<string>(manager.recorder.StartRecording, wavPath));
        manager.Do(new EventBase<string, string, string>(manager.ShowText, "microphone test recording", "Recording...", "red"));

        state.IncrementState();
        manager.DoIn(new EventBase(() => {
                        manager.ShowText("microphone test playing", "Playing...", "green");
                        manager.playback.clip = manager.recorder.StopRecording();
                        manager.playback.Play(); // can't block manager thread, but could block
                                                    // experiment to wait on play finishing;
                                                    // could also subscribe to Unity event, if
                                                    // there is one
            }), 
            manager.GetSetting("micTestDuration"));

        DoIn(new EventBase(() => {
            Run();
        }), manager.GetSetting("micTestDuration")*2);
    }

    protected void Encoding(WordStim word, int index) {
        // This needs to be wrapped, as it relies on data external to the state itself

        int[] limits = manager.GetSetting("stimulusInterval");
        int interval = InterfaceManager.rnd.Value.Next(limits[0], limits[1]);

        Dictionary<string, object> data = new Dictionary<string, object>();
        data.Add("word", word.word);
        data.Add("serialpos", index);
        data.Add("stim", word.stim);

        ReportEvent("word stimulus info", data);
        SendHostPCMessage("WORD", data);

        manager.Do(new EventBase<string, string>(manager.ShowText, "word stimulus", word.word));

        DoIn(new EventBase(() => { 
                                    CleanSlate();
                                    ReportEvent("clear word stimulus", new Dictionary<string, object>());
                                    SendHostPCMessage("ISI", new Dictionary<string, object>() {{"duration", interval}});

                                    DoIn(new EventBase(Run), interval);
                                }), 
                                manager.GetSetting("stimulusDuration"));
    }

    // protected void Distractor(StateMachine state) {
    //     int[] nums = new int[] { InterfaceManager.rnd.Value.Next(1, 10),
    //                              InterfaceManager.rnd.Value.Next(1, 10),
    //                              InterfaceManager.rnd.Value.Next(1, 10) };

    //     string problem = nums[0].ToString() + " + " + nums[1].ToString() + " + " + nums[2].ToString() + " = ";

    //     state.distractorProblem = nums;
    //     state.distractorAnswer = "";

    //     inputHandler.SetAction((KeyAction)DistractorAnswer);
    //     manager.Do(new EventBase<string, string>(manager.ShowText, "display distractor problem", problem));

    //     state.IncrementState();
    //     DoIn(new EventBase(Run), manager.distractorDuration);
    // }


    protected void Orientation(StateMachine state) {
        int[] limits = manager.GetSetting("stimulusInterval");
        int interval = InterfaceManager.rnd.Value.Next(limits[0], limits[1]);

        limits = manager.GetSetting("orientationDuration");
        int duration = InterfaceManager.rnd.Value.Next(limits[0], limits[1]);
        manager.Do(new EventBase<string, string>(manager.ShowText, "orientation stimulus", "+"));

        SendHostPCMessage("ORIENT", null);

        state.IncrementState();
        DoIn(new EventBase(() => {
                                    CleanSlate();
                                    SendHostPCMessage("ISI", new Dictionary<string, object>() {{"duration", interval}});
                                    DoIn(new EventBase(Run), interval);
                                }), 
                                duration);
    }

    protected void Recall(string wavPath) {

        manager.Do(new EventBase(() => {
                            // NOTE: unlike other events, that should be aligned to when they are called,
                            //       this event needs to be precisely aligned with the beginning of
                            //       recording.
                            manager.recorder.StartRecording(wavPath);
                            ReportEvent("start recall period", new Dictionary<string, object>());
                        }));

        int duration = manager.GetSetting("recallDuration");

        SendHostPCMessage("RECALL", new Dictionary<string, object>() {{"duration", duration}});

        manager.DoIn(new EventBase(() => {
                manager.recorder.StopRecording(); // FIXME: this call is SLOW
                manager.lowBeep.Play(); // TODO: we should wait for this beep to finish
        }), duration );

        DoIn(new EventBase(() => {
                ReportEvent("end recall period", new Dictionary<string, object>());
                Run();
        }), duration );

    }

    protected void FinalRecall(string wavPath) {
        // FIXME: this very much violates DRY

        manager.Do(new EventBase(() => {
                            // NOTE: unlike other events, that should be aligned to when they are called,
                            //       this event needs to be precisely aligned with the beginning of a
                            //       recording.
                            manager.recorder.StartRecording(wavPath);
                            ReportEvent("start final recall period", new Dictionary<string, object>());
                        }));

        int duration = manager.GetSetting("finalRecallDuration");

        SendHostPCMessage("FINAL RECALL", new Dictionary<string, object>() {{"duration", duration}});

        DoIn(new EventBase(() => {

                ReportEvent("end final recall period", new Dictionary<string, object>());
                Run();
        }), duration );

        manager.DoIn(new EventBase(() => {
            manager.recorder.StopRecording();
            manager.lowBeep.Play(); // TODO: we should wait for this beep to finish
        }), duration );
    }

    protected void RecallPrompt(StateMachine state) {
        manager.Do(new EventBase(() => {
                manager.highBeep.Play();
                manager.ShowText("display recall text", "*******");
            }));

        state.IncrementState();
        DoIn(new EventBase(Run), 500); // magic number is the duration of beep
    }
    
    
    protected void QuitPrompt(StateMachine state) {
        WaitForKey("subject/session confirmation", 
            "Running " + manager.GetSetting("participantCode") + " in session " 
            + (int)manager.GetSetting("session") + " of " + manager.GetSetting("experimentName") 
            + ".\nPress Y to continue, N to quit.", 
            (KeyAction)QuitOrContinue);
    }


    protected void MicTestPrompt(StateMachine state) {
        manager.Do(new EventBase<string, string>(manager.ShowTitle, "microphone test title", "Microphone Test"));
        WaitForKey("microphone test prompt", "Press any key to record a sound after the beep.", (KeyAction)AnyKey);
    }

    protected void ConfirmStart(StateMachine state) {
        WaitForKey("confirm start", "Please let the experimenter know \n" +
                "if you have any questions about \n" +
                "what you just did.\n\n" +
                "If you think you understand, \n" +
                "Please explain the task to the \n" +
                "experimenter in your own words.\n\n" +
                "Press any key to continue \n" +
                "to the first list.", (KeyAction)AnyKey);
    }
    
    protected virtual void Quit() {
        CleanSlate();
        ReportEvent("experiment quit", null);
        SendHostPCMessage("EXIT", null);

        if(stateMachine.isComplete){
            manager.Do(new EventBase<string, string>(manager.ShowText, "session end", "Yay! Session Complete."));
        }
        Stop();
        manager.DoIn(new EventBase(manager.LaunchLauncher), 10000);
    }


    //////////
    // Utility Functions
    //////////

    protected void WaitForKey(string tag, string prompt, Func<InputHandler, KeyMsg, bool> handler) {
        manager.Do(new EventBase<string, string>(manager.ShowText, tag, prompt));
        inputHandler.SetAction(handler);
        inputHandler.active = true;
        
    }

    protected void WaitForKey(string tag, string prompt, string key) {
        manager.Do(new EventBase<string, string>(manager.ShowText, tag, prompt));

        inputHandler.SetAction(
            (handler, msg) => {
                if(msg.down && msg.key == key) {
                    handler.active = false;
                    stateMachine.IncrementState();
                    Do(new EventBase(Run));
                    return false;
                }
                return true;
            }
        );
        inputHandler.active = true;
    }

    protected void WaitForTime(int milliseconds) {
        // convert to milliseconds
        DoIn(new EventBase(Run), milliseconds); 
    }

    //////////
    // Key Handling functions--register action to Input Handler, which then receives messages from
    // manager. Input may also be handled by registering another handler as a child of manager.
    //////////

    // TODO: rather than accessing stateMachine, these should receive a closure on state from
    // their caller

    // protected bool DistractorAnswer(InputHandler handler, KeyMsg msg) {
    //     // FIXME: this needs a different scratchpad than state with the new structure

    //     int Sum(int[] arg){
    //         int sum = 0;
    //         for(int i=0; i < arg.Length; i++) {
    //             sum += arg[i];
    //         }
    //         return sum;
    //     }

    //     if(!msg.down) {
    //         return true;
    //     }

    //     string message = "distractor update";
    //     var key = msg.key;
    //     bool correct = false;

    //     // enter only numbers
    //     if(Regex.IsMatch(key, @"\d$")) {
    //         key = key[key.Length-1].ToString(); // Unity gives numbers as Alpha# or Keypad#
    //         if(state.distractorAnswer.Length < 3) {
    //             state.distractorAnswer = state.distractorAnswer + key;
    //         }
    //         message = "modify distractor answer";
    //     }
    //     // delete key removes last character from answer
    //     else if(key == "delete" || key == "backspace") {
    //         if(state.distractorAnswer != "") {
    //             state.distractorAnswer = state.distractorAnswer.Substring(0, state.distractorAnswer.Length - 1);
    //         }
    //         message = "modify distractor answer";
    //     }
    //     // submit answer and play tone depending on right or wrong answer 
    //     else if(key == "enter" || key == "return") {
    //         int result;
    //         int.TryParse(state.distractorAnswer, out result) ;
    //         correct = result == Sum(state.distractorProblem);

    //         message = "distractor answered";
    //         if(correct) {
    //             manager.Do(new EventBase(manager.lowBeep.Play));
    //         } 
    //         else {
    //             manager.Do(new EventBase(manager.lowerBeep.Play));
    //         }

    //         Do(new EventBase(Run));
    //         state.distractorProblem = "";
    //         state.distractorAnswer = "";

    //         handler.active = false; // stop the handler from reporting any more keys
    //     }

    //     string problem = state.distractorProblem[0].ToString() + " + " 
    //                     + state.distractorProblem[1].ToString() + " + " 
    //                     + state.distractorProblem[2].ToString() + " = ";
    //     string answer = state.distractorAnswer;
    //     manager.Do(new EventBase<string, string>(manager.ShowText, message, problem + answer));
    //     ReportDistractor(message, correct, problem, answer);
    //     return false;
    // }

    protected bool AnyKey(InputHandler handler, KeyMsg msg) {
        if(msg.down) {
            handler.active = false; // also done by CleanSlate
            stateMachine.IncrementState();
            Do(new EventBase(Run));
            return false;
        }
        return true;
    }

    protected bool QuitOrContinue(InputHandler handler, KeyMsg msg) {
        if(msg.down && msg.key == "y") {
            stateMachine.IncrementState();
            this.Do(new EventBase(Run));
            return false;
        }
        else if(msg.down && msg.key == "n") {
            Quit();
            return false;
        }
        return true;
    }

    protected bool RepeatOrContinue(InputHandler handler, KeyMsg msg) {
        if(msg.down && msg.key == "n") {
            // repeat the previous state
            stateMachine.DecrementState();
            handler.active = false;
            Do(new EventBase(Run));
            return false;
        }
        else if(msg.down && msg.key == "y") {
            // proceed to the next state
            stateMachine.IncrementState();
            handler.active = false;
            Do(new EventBase(Run));
            return false;
        }
        return true;
    }

    protected bool LoopOrContinue(InputHandler handler, KeyMsg msg) {
        if(msg.down && msg.key == "n") {
            // this brings us back to the top of a loop timeline
            stateMachine.IncrementState();
            handler.active = false;
            Do(new EventBase(Run));
            return false;
        }
        else if(msg.down && msg.key == "y") {
            // this ends the loop timeline and resumes the outer scope
            stateMachine.PopTimeline();
            handler.active = false;
            Do(new EventBase(Run));
            return false;
        }
        return true;
    }

    //////////
    // Saving, Reporting, and Loading state logic
    //////////

    protected void ReportBeepPlayed(string beep, string duration) {
        var dataDict = new Dictionary<string, object>() { { "sound name", beep }, 
                                                          { "sound duration", duration } };
        ReportEvent("Sound Played", dataDict);
    }

    protected void SendHostPCMessage(string type, Dictionary<string, object> data) {
        manager.Do(new EventBase<string, Dictionary<string, object>>(manager.SendHostPCMessage, 
                                                                     type, data));
    }

    protected void ReportEvent(string type, Dictionary<string, object> data) {
        manager.Do(new EventBase<string, Dictionary<string, object>>(manager.ReportEvent, 
                                                                     type, data));
    }

    private void ReportDistractor(string type, bool correct, string problem, string answer)
    {
        Dictionary<string, object> dataDict = new Dictionary<string, object>();
        dataDict.Add("correct", correct);
        dataDict.Add("problem", problem);
        dataDict.Add("answer", answer);
        ReportEvent(type, dataDict);
        SendHostPCMessage("MATH", dataDict);
    }
    
    public virtual void SaveState() {
        string path = System.IO.Path.Combine(manager.fileManager.SessionPath(), "experiment_state.json");
        // TODO: StreamWriter to save
        JsonConvert.SerializeObject(stateMachine);
    }

    public virtual StateMachine LoadState(string participant, int session) {
        var logPath = System.IO.Path.Combine(manager.fileManager.SessionPath(participant, session), 
                                             "experiment_state.json");
        if(System.IO.File.Exists(logPath)) {
            string json = System.IO.File.ReadAllText(logPath);
            StateMachine state = JsonConvert.DeserializeObject<StateMachine>(json);

            if(state.isComplete) {
                ErrorNotification.Notify(new InvalidOperationException("Session Already Complete"));
            }
            return state;
        }
        else {
            return null;
        }
    }
}