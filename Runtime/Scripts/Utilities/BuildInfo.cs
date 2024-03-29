﻿using System.Reflection;

[assembly: AssemblyVersion("1.0.0")]

namespace UnityEPL {

    public static class BuildInfo {
        public static System.Version Version() {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }

        public static System.DateTime Date() {
            System.Version version = Version();
            System.DateTime startDate = new System.DateTime(2000, 1, 1, 0, 0, 0);
            System.TimeSpan span = new System.TimeSpan(version.Build, 0, 0, version.Revision * 2);
            System.DateTime buildDate = startDate.Add(span);
            return buildDate;
        }

        public static string ToString(string format = null) {
            System.DateTime date = Date();
            return date.ToString(format);
        }
    }

}
