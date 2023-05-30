namespace UnityEPL {

    public static partial class Config {
        // RepFRExperiment.cs
        public static int[] wordRepeats { get { return Config.GetSetting<int[]>("wordRepeats"); } }
        public static int[] wordCounts { get { return Config.GetSetting<int[]>("wordCounts"); } }
        public static int[] recallDelay { get { return Config.GetSetting<int[]>("recallDelay"); } }
        public static int[] stimulusInterval { get { return Config.GetSetting<int[]>("stimulusInterval"); } }
        public static int restDuration { get { return Config.GetSetting<int>("restDuration"); } }
        public static int practiceLists { get { return Config.GetSetting<int>("practiceLists"); } }
        public static int preNoStimLists { get { return Config.GetSetting<int>("preNoStimLists"); } }
        public static int encodingOnlyLists { get { return Config.GetSetting<int>("encodingOnlyLists"); } }
        public static int retrievalOnlyLists { get { return Config.GetSetting<int>("retrievalOnlyLists"); } }
        public static int encodingAndRetrievalLists { get { return Config.GetSetting<int>("encodingAndRetrievalLists"); } }
        public static int noStimLists { get { return Config.GetSetting<int>("noStimLists"); } }

        // ltpRepFRExperiment.cs
        public static int[] restLists { get { return Config.GetSetting<int[]>("restLists"); } }

        // CPSExperiment.cs
        public static string video { get { return Config.GetSetting<string>("video"); } }
    }

}