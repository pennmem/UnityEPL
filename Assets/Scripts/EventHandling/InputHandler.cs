using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using UnityEngine;

public class InputHandler : MessageTreeNode<KeyMsg> {
    public InputHandler(EventQueue host, Func<InputHandler, KeyMsg, bool> action) {
        this.host = host;
        SetAction(action);
    }

    public void Key(string key, bool pressed) {
        Do(new KeyMsg(key, pressed));
    }
    public virtual void SetAction(Func<InputHandler, KeyMsg, bool> action) {
        // should only be run from host's thread
        this.action = (node, msg) => action((InputHandler)node, msg);
    }
}

public struct KeyMsg {
    public string key;
    public bool down;

    public KeyMsg(string key, bool down) {
        this.key = key;
        this.down = down;
    }
}