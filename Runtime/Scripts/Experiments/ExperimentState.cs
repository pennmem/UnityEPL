using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class Timeline<T> : IList<T> { //: IDeserializationCallback {
    protected List<T> items = new List<T>();
    protected bool reset_on_load;
    public virtual bool IsReadOnly { get { return false; } }
    public int index;
    public virtual int Count {get { return items.Count; } }

    public Timeline(IEnumerable<T> states, 
                    bool reset_on_load = false) {
        this.AddRange(states);
        this.reset_on_load = reset_on_load;
    }

    public Timeline(bool reset_on_load = false) {
        this.reset_on_load = reset_on_load;
    }

    virtual public bool IncrementState() {
        if(index < this.Count - 1 ) {
            index++;
            return true;
        }
        else {
            return false;
        }
    }

    virtual public bool DecrementState() {
        if(index > 0) {
            index--;
            return true;
        }
        else {
            return false;
        }
    }

    // void IDeserializationCallback.OnDeserialization(Object sender)
    // {
    //     // if reset is set, reset when the object is deserialized
    //     if(reset_on_load) {
    //         index = 0;
    //     }
    // }

    public virtual T this[int i] {
        get { return items[i]; }
        set { throw new NotSupportedException("Indexing is read only"); }
    }

    public T GetState() {
        return this[index];
    }

    virtual public int IndexOf(T item) {
        throw new NotSupportedException("Provided only for compatibility");
    }

    virtual public void Insert(int index, T item) {
        items.Insert(index, item);
    }

    virtual public void RemoveAt(int index) {
        items.RemoveAt(index);
    }

    virtual public void Add(T item) {
        items.Add(item);
    }

    virtual public void AddRange(IEnumerable<T> new_items) {
        items.AddRange(new_items);
    }

    virtual public void Clear() {
        items.Clear();
    }

    virtual public bool Contains(T item) {
        throw new NotSupportedException("Provided only for compatibility");
    }

    virtual public void CopyTo(T[] array, int index) {
        items.CopyTo(array, index);
    }

    virtual public bool Remove(T item) {
        throw new NotSupportedException("Provided only for compatibility");
    }

    virtual public IEnumerator<T> GetEnumerator() {
        return items.GetEnumerator();
    }

    IEnumerator System.Collections.IEnumerable.GetEnumerator() {
       return this.GetEnumerator();
    }
}

[Serializable]
public class ExperimentTimeline : Timeline<Action<StateMachine>> {
    public ExperimentTimeline(List<Action<StateMachine>> states, bool reset_on_load = false) : base(states, reset_on_load) {}
    // TODO: don't serialize functions
}

[Serializable]
public class LoopTimeline : ExperimentTimeline {
    public LoopTimeline(List<Action<StateMachine>> states, bool reset_on_load = false) : base(states, reset_on_load) {}

    // TODO: don't serialize functions

    override public bool IncrementState() {
        if(index < this.Count - 1 ) {
            index++;
        }
        else {
            index = 0;
        }
        return true;
    }

    override public bool DecrementState() {
        if(index > 0) {
            index--;
        }
        else {
            index = this.Count - 1;
        }
            return true;
    }
}

public class StateMachine : Dictionary<string, ExperimentTimeline> {
    // Must be a serializable type
    public dynamic currentSession;
    public bool isComplete {get; set; } = false;

    public StateMachine(dynamic currentSession) : base() {
        this.currentSession = currentSession;
    }

    public Action<StateMachine> GetState() {
        return GetTimeline(timelines.Peek()).GetState();
    }

    // LIFO queue describing state machine timelines,
    // the timeline visible with Peek is the current timeline
    protected Stack<string> timelines = new Stack<string>();

    public void IncrementState() {
        if(!GetTimeline(timelines.Peek()).IncrementState()) {
            PopTimeline();
        }
    }

    public void DecrementState() {
        if(!GetTimeline(timelines.Peek()).DecrementState()){
            PopTimeline();
        }
    }

    public void PushTimeline(string timeline) {
        if(this.ContainsKey(timeline)) {
            timelines.Push(timeline);
        }
        else {
            throw new Exception("State machine has no timeline " + timeline);
        }
    }

    public void PopTimeline() {
        timelines.Pop();
    }

    private ExperimentTimeline GetTimeline(string timeline) {
        // this throws a keyerror if not existing, which
        // is enough of an exception for now
        return this[timeline];
    }
}
