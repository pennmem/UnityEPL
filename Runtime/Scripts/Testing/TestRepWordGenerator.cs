using System;
using System.Collections.Generic;

#if TESTING
class TestingRepWord {
  public static void StimCheck(bool stim_state, StimWordList wordlst) {
    int stim_count = 0;
    foreach (var w in wordlst) {
      if (w.stim) {
        stim_count++;
      }
    }

    if (stim_state) {
      if (stim_count == 0 || stim_count == wordlst.Count) {
        Console.WriteLine("ERROR: Stim distribution does not look randomized.");
      }
      else {
        Console.WriteLine("Stim true test passed with {0} of {1}.", stim_count,
            wordlst.Count);
      }
    }
    else {
      if (stim_count == 0) {
        Console.WriteLine("Stim false successful.");
      }
      else {
        Console.WriteLine("ERROR: Stim false failed.");
      }
    }
  }

  public static void Main() {
    // Alternate calling modality.
    var repeats1 = new RepWordList(new List<string>
        {"cat", "dog", "fish", "bird", "shark", "tiger"}, 3);
    var repeats2 = new RepWordList(new List<string>
        {"corn", "wheat", "rice"}, 2);
    var singles = new RepWordList(new List<string>
        {"red", "blue", "green"});

    StimWordList wordlst1 = RepWordGenerator.Generate(
        new List<RepWordList>{repeats1, repeats2},
        singles, true);
    var rep_counts1 = new RepCounts(3, 6).RepCnt(2, 3).RepCnt(1, 3);

    Console.WriteLine(wordlst1);
    Console.WriteLine("Goal {0} words, generated {1} words.",
        rep_counts1.TotalWords(), wordlst1.Count);

    StimCheck(true, wordlst1);

    var rep_counts2 = new RepCounts(3, 6).RepCnt(2, 3).RepCnt(1, 3);
    var input_word_list = new List<string> {"cat", "dog", "fish", "bird",
      "shark", "tiger", "corn", "wheat", "rice", "red", "blue", "green",
      "Mercury", "Venus", "Earth", "Mars"};

    StimWordList wordlst2 = RepWordGenerator.Generate(rep_counts2,
        input_word_list, false);

    Console.WriteLine(wordlst2);
    Console.WriteLine("Goal {0} words, generated {1} words.",
        rep_counts2.TotalWords(), wordlst2.Count);

    StimCheck(false, wordlst2);

    StimWordList wordlst3 = RepWordGenerator.Generate(rep_counts2,
        input_word_list, true);

    Console.WriteLine(wordlst3);
    Console.WriteLine("Goal {0} words, generated {1} words.",
        rep_counts2.TotalWords(), wordlst2.Count);

    StimCheck(true, wordlst3);
  }
}
#endif
