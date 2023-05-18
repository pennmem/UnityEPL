using System;
using UnityEPL;


class DoorShuffle {
    public static byte TIMED_TRIAL = 0b_1000;

    public static byte[] TrialParameters(uint totalTrials) {
        if(0 != (totalTrials % 6)) {
            throw new System.Exception("number of trials not evenly divisible");
        }
        uint timedSplit = totalTrials / 2;

        byte[] trialParamsArray = new byte[totalTrials];
        for(int i=0; i<totalTrials; i++) {
            trialParamsArray[i] = (byte)((i < timedSplit ? 0b_1000 : 0b_0000) | (0b_0001 << i % 3));
        }

        trialParamsArray.ShuffleInPlace();

        return trialParamsArray;
    } 

    public static bool IsTimed(byte b) {
        return (b & TIMED_TRIAL) > 0;
    }

    public static uint DoorIndex(byte b) {
        return (uint)((b & 0b_0111) >> 1); 
    }


    public static int Main() {
        var bytes = TrialParameters(36);
        foreach(byte b in bytes){

            Console.Write(IsTimed(b));
            Console.Write(DoorIndex(b));
            Console.Write("\n\n");
        }

        return 1;
    }
}