using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using UnityEngine; // to read resource files packaged with Unity

public class ltpRepFRExperiment : RepFRExperiment {
  public ltpRepFRExperiment(InterfaceManager _manager) : base(_manager) {}

  //////////
  // State Machine Constructor Functions
  //////////

  public override StateMachine GetStateMachine() {
    // TODO: some of these functions could be re imagined with wrappers, where the
    // state machine has functions that take parameters and return functions, such
    // as using a single function for the 'repeatlast' state that takes a prompt
    // to show or having WaitForKey wrap an action. It's not clear whether 
    // this improves clarity or reusability at all,
    // so I've deferred this. If it makes sense to do this or make more use of
    // wrapper functions that add state machine information, please do.
    StateMachine stateMachine = base.GetStateMachine();

    // TODO: reformat
    stateMachine["Run"] = new ExperimentTimeline(new List<Action<StateMachine>> {IntroductionPrompt,
                                                                                IntroductionVideo,
                                                                                RepeatVideo,
                                                                                MicrophoneTest, // runs MicrophoneTest states
                                                                                RepeatMicTest,
                                                                                QuitPrompt,
                                                                                Practice, // runs Practice states
                                                                                ConfirmStart,
                                                                                MainLoop, // runs MainLoop states
                                                                                FinalRecallInstructions,
                                                                                RecallPrompt,
                                                                                FinalRecall,
                                                                                FinishExperiment});

    // though it is largely the same as the main loop,
    // practice is a conceptually distinct state machine
    // that just happens to overlap with MainLoop
    stateMachine["Practice"] = new LoopTimeline(new List<Action<StateMachine>> {StartTrial,
                                                                                NextPracticeListPrompt,
                                                                                Rest,
                                                                                CountdownVideo,
                                                                                EncodingDelay,
                                                                                Encoding,
                                                                                Rest,
                                                                                RecallPrompt,
                                                                                Recall,
                                                                                EndPracticeTrial});

    stateMachine["MainLoop"] = new LoopTimeline(new List<Action<StateMachine>> {StartTrial,
                                                                                NextListPrompt,
                                                                                Rest,
                                                                                CountdownVideo,
                                                                                EncodingDelay,
                                                                                Encoding,
                                                                                Rest,
                                                                                RecallPrompt,
                                                                                Recall,
                                                                                EndTrial});

    stateMachine["MicrophoneTest"] = new LoopTimeline(new List<Action<StateMachine>> {MicTestPrompt,
                                                                                      RecordTest,
                                                                                      RepeatMicTest});

    return stateMachine;
  }

  //////////
  // Wait Functions
  //////////

  protected override void StartTrial(StateMachine state) {
    Dictionary<string, object> data = new Dictionary<string, object>();
    data.Add("trial", state.currentSession.GetListIndex());
    // data.Add("stim", currentSession[state.listIndex].encoding_stim);

    ReportEvent("start trial", data);

    var restLists = manager.GetSetting("restLists");

    // check if this list exists in the configuration rest list
    // if(state.currentSession.GetList().restList) {
    if(Array.IndexOf(manager.GetSetting("restLists"), state.currentSession.GetListIndex()) != -1) {
      Do(new EventBase<StateMachine>(WaitForResearcher, state));
    } 
    else {
      state.IncrementState();
      Run();
    }
  }

  protected void WaitForResearcher(StateMachine state) {
    WaitForKey("participant break",
                "It's time for a short break, please " + 
                "wait for the researcher to come check on you " +
                "before continuing the experiment. \n\n" +
                "Researcher: press space to resume the experiment.", 
                "space");
  }

  protected void FinalRecallInstructions(StateMachine state) {
    state.IncrementState();
    WaitForKey("final recall instructions", "You will now have ten minutes to recall as many words as you can from any you studied today. As before, please say any word that comes to mind. \n\n Press Any Key to Continue.", AnyKey); 
  }

  protected void FinalRecall(StateMachine state) {
    state.IncrementState();
    string path = System.IO.Path.Combine(manager.fileManager.SessionPath(), "ffr.wav");
    FinalRecall(path);
  }
}
