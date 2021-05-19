using System;
using System.Linq;
using System.Collections.Generic;

#if TESTING
class TestUniformity {
    static public void Print2DJaggedArray(int[][] A)
    {
        foreach (int[] row in A)
        {
            foreach (int element in row)
            {
                  Console.Write("{0,6:0000}", element);
            }
            Console.WriteLine();
        }
    }

    public static double UniformKLDivergence(int[] measured, double qx) {
        double stat = 0;
        double px = 0;
        double total = 0;
        for(int i=0; i < measured.Length; i++) {
            px = measured[i] / (double)measured.Sum();
            total += px;
            stat +=  px * Math.Log(px / qx);
        }
        return stat;
    }

    public static void Main() {
        var repeats1 = new RepWordList(new List<string>
            {"3", "3", "3", "3", "3", "3"}, 3);
        var repeats2 = new RepWordList(new List<string>
            {"2","2","2",}, 2);
        var singles = new RepWordList(new List<string>
            {"1","1","1"});

        var rep_counts1 = new RepCounts(3, 6).RepCnt(2, 3).RepCnt(1, 3);

        // C# automatically 0's memory, but jagged arrays are clunky
        int[][] measured = new int[3][];
        measured[0] = new int[rep_counts1.TotalWords()];
        measured[1] = new int[rep_counts1.TotalWords()];
        measured[2] = new int[rep_counts1.TotalWords()];

        var testRepeats = 2000;
        foreach(var _ in Enumerable.Range(0, testRepeats)){
             var repWordList = RepWordGenerator.Generate(
                            new List<RepWordList>{repeats1, repeats2},
                            singles, true);

             if(!Double.IsFinite(repWordList.score)) {
                 Console.WriteLine("List generated with repeat presentation");
             }

             for(var i=0; i<repWordList.Count; i++) {
                measured[int.Parse(repWordList.words[i]) - 1][i] += 1;
             }
        }

        Print2DJaggedArray(measured);
        for(var i=0; i<measured.Length; i++) {
            var score = UniformKLDivergence(measured[i], 1.0f/measured[i].Length);
            Console.WriteLine("Score {0,8:0.0000} for {1}p words", score, i+1);
        }
    }
}
#endif
