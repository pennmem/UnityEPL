using System;
using System.Threading;

#if TESTING
public class TestingEventLoop {
    public static int eventRepeats = 0;
    public static void NoArguments() {
                Console.WriteLine("SUCCESS: Event Without Arguments");
    }

    public static void Callback(Action callback) {
        callback();
    }

    public static void CallbackWithArgument(Action<string> callback, string arg) {
        callback(arg);
    }

    public static void RepeatingCall() {
        eventRepeats += 1;
        Console.WriteLine("Repeating event: {0}", HighResolutionDateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond);
    } 

    public static void CallbackFunction() {
        Console.WriteLine("SUCCESS: No args callback");
    }

    public static void CallbackFunction(string arg) {
        Console.WriteLine(arg);
    }

    public static void Main() {
        EventLoop loop = new EventLoop();
        loop.Start();

        loop.Do(new EventBase(NoArguments));
        loop.Do(new EventBase<Action>(Callback, CallbackFunction));
        loop.Do(new EventBase<Action<string>, string>(CallbackWithArgument, CallbackFunction, "SUCCESS: Args in Callback"));
        loop.Do(new EventBase(NoArguments));
        loop.DoRepeating(new EventBase(RepeatingCall), 10, 0, 50);

        // Simulating other program work
        Thread.Sleep(1000);

        loop.Stop();
        try {
            loop.Do(new EventBase<Action<string>, string>(CallbackWithArgument, CallbackFunction, "ERROR: Event processed after Stop"));
        } 
        catch {
           Console.WriteLine("SUCCESS: exception thrown when enqueing to stopped loop"); 
        }


        if(eventRepeats == 10) {
            Console.WriteLine("SUCCESS: RepeatingEvent");
        } else {
            Console.WriteLine("ERROR: RepeatingEvent");
        }
    }
}
#endif
