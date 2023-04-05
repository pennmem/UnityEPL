using UnityEngine;

public static class UnityUtilities {
    public static void Quit() {
        Debug.Log("Quitting");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        //no more calls to Run past this point
    }
}
