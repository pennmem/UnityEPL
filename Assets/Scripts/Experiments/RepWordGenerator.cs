using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Stores a word and whether or not it should be stimulated during encoding.
public class WordStim {
  public string word;
  public bool stim;

  public WordStim(string new_word, bool new_stim = false) {
    word = new_word;
    stim = new_stim;
  }

  public override string ToString() {
    return String.Format("{0}:{1}", word, Convert.ToInt32(stim));
  }
}


// Stores the number of times to repeat a word, and the count of how many
// words should be repeated that many times.
public class RepCnt {
  public int rep;
  public int count;

  public RepCnt(int new_rep, int new_count) {
    rep = new_rep;
    count = new_count;
  }
}

// e.g. new RepCounts(3,6).RepCnt(2,3).RepCnt(1,3);
// Specifies 3 repeats of 6 words, 2 repeats of 3 words, 1 instance of 3 words.
public class RepCounts : List<RepCnt> {
  public RepCounts() { }

  public RepCounts(int rep, int count) {
    RepCnt(rep, count);
  }

  public RepCounts RepCnt(int rep, int count) {
    Add(new RepCnt(rep, count));
    return this;
  }

  public int TotalWords() {
    int total = 0;
    foreach (var r in this) {
      total += r.rep * r.count;
    }
    return total;
  }

  public int UniqueWords() {
    int total = 0;
    foreach (var r in this) {
      total += r.count;
    }
    return total;
  }
}


// If "i" goes past "limit", an exception is thrown with the stored message.
public class BoundedInt {
  private int limit;
  private string message;
  private int i_;
  public int i {
    get { Assert(i_); return i_; }
    set { i_ = value; }
  }

  public BoundedInt(int limit_, string message_) {
    limit = limit_;
    message = message_;
  }

  private void Assert(int i) {
    if (i >= limit) {
      throw new IndexOutOfRangeException(message);
    }
  }
}

public static class EnumeratorExtensions
{
    // Used to Copy StimWordList
    public static IEnumerable<T> ToEnumerable<T>(this IEnumerator<T> enumerator)
    {
      while(enumerator.MoveNext())
          yield return enumerator.Current;
    }
}

// Provides random subsets of a word pool without replacement.
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

// This class keeps a list of words associated with their stim states.
public class StimWordList : Timeline<WordStim> {

  public override bool IsReadOnly { get { return false; } }
  protected List<string> words_;
  public IList<string> words {
    get { return words_.AsReadOnly(); }
  }
  protected List<bool> stims_;
  public IList<bool> stims {
    get { return stims_.AsReadOnly(); }
  }
  public override int Count {
    get { return words_.Count; }
  }
  protected double score_;
  public double score {
      get { return score_; } }

  public StimWordList() {
    words_ = new List<string>();
    stims_ = new List<bool>();
    score_ = Double.NaN;
  }

  public StimWordList(List<string> word_list, List<bool> stim_list = null, double score=Double.NaN) {
    words_ = new List<string>(word_list);
    stims_ = new List<bool>(stim_list ?? new List<bool>());
    score_ = score;

    // Force the two lists to be the same size.
    if (stims_.Count > words_.Count) {
      stims_.RemoveRange(words_.Count, 0);
    }
    else {
      while (stims_.Count < words_.Count) {
        stims_.Add(false);
      }
    }
  }

  public StimWordList(List<WordStim> word_stim_list, double score=Double.NaN) {
    words_ = new List<string>();
    stims_ = new List<bool>();
    score_ = score;
    
    foreach (var ws in word_stim_list) {
      words_.Add(ws.word);
      stims_.Add(ws.stim);
    }
  }

  public void Add(string word, bool stim=false) {
    words_.Add(word);
    stims_.Add(stim);
  }

  public override void Add(WordStim word_stim) {
    Add(word_stim.word, word_stim.stim);
  }

  public void Insert(int index, string word, bool stim=false) {
    words_.Insert(index, word);
    stims_.Insert(index, stim);
  }

  public override void Insert(int index, WordStim word_stim) {
    Insert(index, word_stim.word, word_stim.stim);
  }

  public override IEnumerator<WordStim> GetEnumerator() {
    for (int i=0; i<words_.Count; i++) {
      yield return new WordStim(words_[i], stims_[i]);
    }
  }

  // override IEnumerator System.Collections.IEnumerable.GetEnumerator() {
  //   return this.GetEnumerator();
  // }

  // needed to allow writing to collection
  // when loading session in progress
  public override void Clear() {
    throw new NotSupportedException("method included only for compatibility");
  }

  public override bool Contains(WordStim item) {
    throw new NotSupportedException("method included only for compatibility");
  }

  public override void CopyTo(WordStim[] array, int arrayIndex) {
    if(array == null) throw new ArgumentNullException();

    if(arrayIndex < 0) throw new ArgumentOutOfRangeException();

    if(this.Count > array.Length - arrayIndex) throw new ArgumentException();

    GetEnumerator().ToEnumerable().ToArray().CopyTo(array, arrayIndex);
  }

  public override bool Remove(WordStim item) {
    throw new NotSupportedException("method included only for compatibility");
  }

  // Read-only indexed access.
  public override WordStim this[int i] {
    get { return new WordStim(words_[i], stims_[i]); }
  }

  public override string ToString() {
    string str = this[0].ToString();
    for (int i=1; i<this.Count; i++) {
      str += String.Format(", {0}", this[i]);
    }
    return str;
  }
}


// A list of words which will each be repeated the specified number of times.
class RepWordList : StimWordList {
  public int repeats;

  public RepWordList(int repeats_=1) {
    repeats = repeats_;
  }

  public RepWordList(List<string> word_list, int repeats_=1,
      List<bool> stim_list = null)
      : base(word_list, stim_list) {
    repeats = repeats_;
  }

  public void SetStim(int index, bool state=true) {
    stims_[index] = state;
  }
}


// Generates well-spaced RepFR wordlists with open-loop stimulation assigned.
class RepWordGenerator {
  // TODO - This should be moved to a more general location for reuse.
  // Fisher-Yates shuffle
  public static List<T> Shuffle<T>(IList<T> list) {
    var shuf = new List<T>(list);
    for (int i=shuf.Count-1; i>0; i--) {
      int j = InterfaceManager.rnd.Value.Next(i+1);
      T tmp = shuf[i];
      shuf[i] = shuf[j];
      shuf[j] = tmp;
    }
    
    return shuf;
  }

  // perm is the permutation to be assigned to the specified repword_lists,
  // interpreted in order.  If the first word in the first RepWordList is to
  // be repeated 3 times, the first three indices in perm are its locations
  // in the final list.  The score is a sum of the inverse
  // distances-minus-one between all neighboring repeats of each word.  Word
  // lists with repeats spaced farther receive the lowest scores, and word
  // lists with adjacent repeats receive a score of infinity.
  public static double SpacingScore(List<int> perm,
      List<RepWordList> repword_lists) {
    var split = new List<List<int>>();
    int offset = 0;
    foreach (var wl in repword_lists) {
      for (int w=0; w<wl.Count; w++) {
        var row = new List<int>();
        for (int r=0; r<wl.repeats; r++) {
          row.Add(perm[w*wl.repeats + r + offset]);
        }
        split.Add(row);
      }
      offset += wl.Count * wl.repeats;
    }

    double score = 0;

    foreach (var s in split) {
      s.Sort();

      for (int i=0; i<s.Count-1; i++) {
        double dist = s[i+1] - s[i];
        score += 1.0 / (dist-1);
        // score += (Math.Abs(dist) > 1) ? 0 : Double.PositiveInfinity;
      }
    }

    return score;
  }

  // Prepares a list of repeated words with better than random spacing,
  // while keeping the repeats associated with their stim state.
  public static StimWordList SpreadWords(
          List<RepWordList> repword_lists,
          double top_percent_spaced = 0.2) {

    int word_len = 0;
    foreach (var wl in repword_lists) {
      word_len += wl.Count * wl.repeats;
    }

    var arrangements = new List<Tuple<double, List<int>>>();

    int iterations = Convert.ToInt32(100/top_percent_spaced);

    for (int i=0; i<iterations; i++) {
      double score = 1.0/0;
      int give_up = 20;
      var perm = new List<int>();
      while (give_up > 0 && double.IsInfinity(score)) {
        var range = Enumerable.Range(0, word_len).ToList();
        perm = Shuffle(range);

        score = SpacingScore(perm, repword_lists);
        give_up--;
      }
      arrangements.Add(new Tuple<double, List<int>>(score, perm));
    }

    arrangements.Sort((a,b) => a.Item1.CompareTo(b.Item1));
    var wordlst = new List<WordStim>();
    foreach (var wl in repword_lists) {
      foreach (var word_stim in wl) {
        for (int i=0; i<wl.repeats; i++) {
          wordlst.Add(word_stim);
        }
      }
    }

    var words_spread = new List<WordStim>(wordlst);

    for (int i=0; i<wordlst.Count; i++) {
      words_spread[arrangements[0].Item2[i]] = wordlst[i];
    }

    return new StimWordList(words_spread, score: arrangements[0].Item1);
  }

  public static void AssignRandomStim(RepWordList rw) {
    for (int i=0; i<rw.Count; i++) {
      bool stim = Convert.ToBoolean(InterfaceManager.rnd.Value.Next(2));
      rw.SetStim(i, stim);
    }
  }

  // Create a RepFR open-stim word list from specified lists of words to be
  // repeated and list of words to use once.
  public static StimWordList Generate(
      List<RepWordList> repeats,
      RepWordList singles,
      bool do_stim,
      double top_percent_spaced=0.2) {

    if (do_stim) {
      // Open-loop stim assigned here.
      foreach (var rw in repeats) {
        AssignRandomStim(rw);
      }
      AssignRandomStim(singles);
    }

    StimWordList prepared_words = SpreadWords(repeats, top_percent_spaced);

    foreach (var word_stim in singles) {
      int insert_at = InterfaceManager.rnd.Value.Next(prepared_words.Count+1);
      prepared_words.Insert(insert_at, word_stim);
    }
    
    return prepared_words;
  }

  // Create a RepFR open-stim word list from a list of repetitions and counts,
  // and a list of candidate words.
  public static StimWordList Generate(
      RepCounts rep_cnts,
      List<string> input_words,
      bool do_stim,
      double top_percent_spaced=0.2) {

    var shuffled = Shuffle(input_words);

    var repeats = new List<RepWordList>();
    var singles = new RepWordList();

    var shuf = new BoundedInt(shuffled.Count,
        "Words required exceeded input word list size.");
    foreach (var rc in rep_cnts) {
      if (rc.rep == 1) {
        for (int i=0; i<rc.count; i++) {
          singles.Add(shuffled[shuf.i++]);
        }
      }
      else if (rc.rep > 1 && rc.count > 0) {
        var rep_words = new RepWordList(rc.rep);
        for (int i=0; i<rc.count; i++) {
          rep_words.Add(shuffled[shuf.i++]);
        }
        repeats.Add(rep_words);
      }
    }

    return Generate(repeats, singles, do_stim, top_percent_spaced);
  }
}
