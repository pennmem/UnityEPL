using System;
using UnityEngine;

public static class ErrorNotification {
    public static InterfaceManager mainThread = null;

    public static void Notify(Exception e) {
        UnityEngine.Debug.Log(e);
        if(mainThread == null) {

            throw e;
           // throw new ApplicationException("Main thread not registered to event notifier.");
        }

        mainThread.Do(new EventBase<Exception>(mainThread.Notify, e));
    }
}

public class ErrorPopup : MonoBehaviour {
    public Rect windowRect;

    void OnGUI() {

    }

}