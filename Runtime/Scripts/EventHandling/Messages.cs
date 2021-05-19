using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;


public class MessageEvent<T> : EventBase<T> {
    public T msg;
    public MessageEvent(Action<T> action, T msg) : base(action, msg) {
        this.msg = msg;
    }
}

public class MessageHandler<T> {
    /*
    Message handler passes an object of a given type
    to an existing event queue using a fixed action, used in,
    for example, key handling, where a function should be
    registered to handle keys within the thread of it's
    construction.
    */
    protected EventQueue host;
    protected Action<T> action;

    public MessageHandler(EventQueue host, Action<T> action) {
        this.host = host;
        this.action = action;
    }

    public MessageHandler() {}

    public virtual void Do(T msg) {
        Do(new MessageEvent<T>(action, msg));
    }
    protected void Do(MessageEvent<T> msg) {
        host.Do(msg);
    }
}

public class MessageTreeNode<T> : MessageHandler<T> {
    protected List<MessageTreeNode<T>> children = new List<MessageTreeNode<T>>();

    // override action to have a return value, which suppresses
    // propagation down the tree
    new protected Func<MessageTreeNode<T>, T, bool> action;
    // public volatile bool active { get; set; } = true;
    public volatile bool active = true;


    protected MessageTreeNode() {}

    public MessageTreeNode(EventQueue host, Func<MessageTreeNode<T>, T, bool> action) {
        this.host = host;
        this.action = action;
    }

    private void Propagate(T msg) {
        foreach(var child in this.children) {
            child.Do(msg);
        }
    }

    public override void Do(T msg) {
        // handle passes event to host queue
        // to be invoke'd in its thread

        base.Do(new MessageEvent<T>( (arg) => {
            // operator short circuits if not active
            if( active && (action?.Invoke(this, arg) ?? false)) {
                Propagate(arg);
            }
        }, msg));
    }

    public virtual void SetAction(Func<MessageTreeNode<T>, T, bool> action) {
        // should only be run from host's thread
        this.action = action;
    }

    public void RegisterChild(MessageTreeNode<T> child) {
        if(!children.Contains<MessageTreeNode<T>>(child)){
            children.Add(child);
        }
    }

    public void UnRegisterChild(MessageTreeNode<T> child) {
        if(children.Contains<MessageTreeNode<T>>(child)){
            children.Remove(child);
        }
    }
}
