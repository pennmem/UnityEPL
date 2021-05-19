using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

#if TESTING

public class RepFRRun {
  public StimWordList encoding;
  public StimWordList recall;
  public bool encoding_stim;
  public bool recall_stim;

  public RepFRRun(StimWordList encoding_list, StimWordList recall_list,
      bool set_encoding_stim=false, bool set_recall_stim=false) {
    encoding = encoding_list;
    recall = recall_list;
    encoding_stim = set_encoding_stim;
    recall_stim = set_recall_stim;
  }
}

// Provides random subsets of a word pool without replacement.
// TODO - This should be moved to a more general location for reuse.
public class RandomSubset {
  protected List<String> shuffled;
  protected int index;

  public RandomSubset(List<String> source_words) {
    shuffled = RepWordGenerator.Shuffle(source_words);
    index = 0;
  }

  public List<String> Get(int amount) {
    if ((shuffled.Count - index) < amount) {
      throw new IndexOutOfRangeException("Word list too small for session");
    }
    int index_now = index;
    index += amount;

    return shuffled.GetRange(index_now, amount);
  }
}

public class RepFRSession : List<RepFRRun> {
}


public class RepFRExperiment {
  protected List<string> source_words;
  protected List<string> blank_words;
  protected RepCounts rep_counts = null;
  protected int words_per_list;

  protected RepFRSession currentSession;

  public RepFRExperiment(List<string> source_word_list) {
    source_words = source_word_list;

    // TODO - Get these parameters from the config system.
    // Repetition specification:
    rep_counts = new RepCounts(3, 6).RepCnt(2, 3).RepCnt(1, 3);
    words_per_list = rep_counts.TotalWords();

    blank_words =
      new List<string>(Enumerable.Repeat(string.Empty, words_per_list));
  }

  //////////
  // Word/Stim list generation
  //////////

  public RepFRRun MakeRun(RandomSubset subset_gen, bool enc_stim,
      bool rec_stim) {
    var enclist = RepWordGenerator.Generate(rep_counts,
        subset_gen.Get(words_per_list), enc_stim);
    var reclist = RepWordGenerator.Generate(rep_counts, blank_words, rec_stim);
    return new RepFRRun(enclist, reclist, enc_stim, rec_stim);
  }


  public RepFRSession GenerateSession() {
    // Parameters retrieved from experiment config, given default
    // value if null.
    // Numbers of list types:
    int practice_lists = 1;
    int pre_no_stim_lists = 3;
    int encoding_only_lists = 4;
    int retrieval_only_lists = 4;
    int encoding_and_retrieval_lists = 4;
    int no_stim_lists = 10;
    
    RandomSubset subset_gen = new RandomSubset(source_words);


    var session = new RepFRSession();

    for (int i=0; i<practice_lists; i++) {
      session.Add(MakeRun(subset_gen, false, false));
    }
          
    for (int i=0; i<pre_no_stim_lists; i++) {
      session.Add(MakeRun(subset_gen, false, false));
    }

    var randomized_list = new RepFRSession();

    for (int i=0; i<encoding_only_lists; i++) {
      randomized_list.Add(MakeRun(subset_gen, true, false));
    }

    for (int i=0; i<retrieval_only_lists; i++) {
      randomized_list.Add(MakeRun(subset_gen, false, true));
    }

    for (int i=0; i<encoding_and_retrieval_lists; i++) {
      randomized_list.Add(MakeRun(subset_gen, true, true));
    }

    for (int i=0; i<no_stim_lists; i++) {
      randomized_list.Add(MakeRun(subset_gen, false, false));
    }

    session.AddRange(RepWordGenerator.Shuffle(randomized_list));

    return session;
  }
}

#endif
