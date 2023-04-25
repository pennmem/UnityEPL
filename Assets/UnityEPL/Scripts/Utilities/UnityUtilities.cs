using UnityEngine;

public static class UnityUtilities {
    public static bool IsMacOS() {
        return Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer;
    }
}
