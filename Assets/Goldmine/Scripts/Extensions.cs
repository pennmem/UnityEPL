using System.Collections.Generic;

public static class QueueExtensions {
    public static void Add<T>(this Queue<T> queue, T val) {
        queue.Enqueue(val);
    }
}
