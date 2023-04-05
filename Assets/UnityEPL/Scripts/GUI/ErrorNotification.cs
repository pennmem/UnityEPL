using System;
using UnityEngine;

public static class ErrorNotification {
    public static InterfaceManager manager = null;

    public static void Notify(Exception e) {
        UnityEngine.Debug.Log(e);
        if (manager == null) {
            throw new ApplicationException("Main thread not registered to event notifier", e);
        }

        manager.Notify(e);
    }
}

public class ErrorPopup : MonoBehaviour {
    public Rect windowRect;

    void OnGUI() {

    }

}