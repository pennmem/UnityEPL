namespace UnityEPL {

    public static partial class Config {
        // GoldmineExperiment.cs
        public static string[] availableScenes { get { return Config.GetSetting<string[]>("availableScenes"); } }
    }

}