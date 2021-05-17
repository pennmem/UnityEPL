using System;
using System.Collections.Generic;


//#define TESTING

#if TESTING
class TestingRepFR {
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
    // TODO - Update test to use configuration system.
    var input_word_list = new List<string> {"Twist", "Schools", "Lands",
      "Guest", "Transport", "Tip", "Blessing", "Heavenly", "Sand", "Families",
      "Misery", "Guilty", "Civilian", "Either", "Address", "Orientation",
      "Walk", "Cane", "Medium", "Divide", "Aid", "Swept", "Envy", "Decision",
      "Technician", "Meetings", "Carriage", "Appreciated", "Came", "Obviously",
      "Effects", "Conclusion", "Belle", "Vendor", "Vain", "Witchcraft", "Dark",
      "Disability", "Demonstration", "Cloud", "Descent", "Virtual", "Travail",
      "Spy", "About", "Reader", "Afraid", "Might", "Reasons", "Study",
      "Helping", "Breath", "Stop", "Days", "Juror", "Sneak", "Yell", "Plan",
      "Dose", "Environmental", "Journalist", "Catch", "Gear", "Stopping",
      "Broken", "Rib", "Published", "Brown", "Heal", "Bare", "Freed",
      "Operator", "Sorts", "Insane", "As", "Poured", "Birthday", "Becomes",
      "Closing", "Whip", "Restore", "Insight", "Arrived", "Cars", "Emphasis",
      "Item", "Attempt", "Dynamics", "Exempt", "Merry", "Accelerate", "Ruin",
      "Spoiled", "Embrace", "Isolation", "Backyard", "Systems", "Cups", "Hat",
      "Panic", "Cluster", "Lonely", "Use", "Uncover", "Staff", "Born",
      "Contempt", "Electrical", "Boost", "Capacity", "Extreme", "Tradition",
      "Enroll", "Paper", "Administer", "Offering", "Possession", "Mystic",
      "Persuade", "Sandy", "Ok", "Obstacle", "Seventh", "Salvage", "Belong",
      "Orbit", "Truth", "Architecture", "Sound", "Costs", "Suburb", "Ghost",
      "New", "Appeared", "Smoked", "Photograph", "Economic", "Esteem", "Rally",
      "Acid", "Near", "Talent", "Tears", "Recognize", "Tonight", "Entitled",
      "Component", "Labor", "Tail", "Instruct", "Bulk", "Psychic", "Elderly",
      "Collaboration", "Peaceful", "Wore", "Privileges", "Gorgeous", "Strict",
      "Urban", "Insisted", "With", "Researcher", "Prayers", "Dismissed",
      "Resolved", "Chorus", "Everyday", "Loaded", "Hormone", "Retrieve", "Web",
      "Managed", "Potion", "Advice", "Directed", "Making", "Clan", "Desire",
      "Newly", "Released", "Deem", "Explaining", "South", "Corps", "Colleague",
      "Forehead", "Devotion", "Expend", "Questions", "Proposal", "Respects",
      "Beginning", "Properties", "Tower", "Intact", "Friend", "Primarily",
      "Blank", "Destined", "Regarding", "Act", "Colony", "Side", "Response",
      "Them", "Collecting", "Die", "Construct", "Unfair", "Prison",
      "Landscape", "Lamb", "Dozen", "Pursuit", "Trusting", "Muscles", "Target",
      "Sector", "Detailed", "Myself", "Violation", "Shore", "Appetite", "Far",
      "Troubled", "Eleven", "Subtle", "Horizon", "Very", "Wrapped", "Curtains",
      "Strengthen", "Sincerely", "Effectiveness", "Occasions", "Cabin",
      "Remembering", "Station", "Hart", "Others", "Sort", "Gate", "Collect",
      "Apartment", "Six", "Heavens", "Daughter", "Bet", "Progressive",
      "Agricultural", "Row", "Utterly", "Dirt", "Picking", "Designed", "Fit",
      "Lot", "Ten", "Out", "Engineer", "Remote", "Chase", "Message", "Monkey",
      "Indulgence", "Distrust", "Sooner", "Carbon", "Take", "Psychology",
      "Juvenile", "Existing", "Huh", "Pattern", "Soften", "Inspection", "Heir",
      "Bottom", "Verify", "Hesitation", "Its", "Numbers", "Condemn", "Sorrow",
      "Shelter", "Daring", "Pitiful", "Added", "Strategic", "Understand",
      "Sergeant", "Return", "Organs", "Poet", "Within", "Affairs", "Hello",
      "Smooth", "Secondary", "Doll", "Deed", "Stress", "Recorded", "Science",
      "Gauge", "Typical", "Sought", "Dogs", "Tire", "Persist", "Joy", "Raised",
      "Feel", "Inches", "Stay", "Showing", "Fat", "Most", "Sincere",
      "Concentration", "Akin", "Dominant", "Situation", "Foliage", "Inspector",
      "Shocked", "Noted", "Mere", "Now", "Resume", "Icon", "Grows", "Legacy",
      "Founded", "Bias", "Separation", "Asked", "Protecting", "Sits",
      "Sarcasm", "Volunteers", "Citizenship", "Neutral", "Tempt", "Referred",
      "Politician", "Brave", "Substantial", "Forest", "Soup", "Vanish",
      "Pillow", "Elegant", "Big", "Pick", "Superior", "Letter", "Rock",
      "Authorized", "Living", "Awfully", "Dedicate", "Map", "Wondered",
      "Unnatural", "Slot", "Handed", "Humor", "Precisely", "Pages",
      "Complexity", "Aspect", "American", "Pour", "Partially", "Habit",
      "Feudal", "Consul", "Articles", "Twilight", "Texture", "Adds",
      "Transform", "Sat", "Grace", "Gathering", "Necessary", "Commonly",
      "Butter", "Quarterback", "Reaching", "Properly", "Watch", "Go", "Booth",
      "Disabled", "Eloquent", "Sharply", "Formed", "Obstacles",
      "Representative", "Brings", "Substantially", "Roots", "Illegal",
      "Poetic", "Homeland", "Meeting", "Draws", "Bride", "Display", "Lighter",
      "Evolve", "Butterfly", "Consulate", "Deceased", "Secular", "Kneel",
      "Importance", "Genius", "Breaks", "Lights", "Forty", "Those", "Mantle",
      "Roman", "Enemy", "Made", "Surrounded", "Shoulders", "Scary", "Orderly",
      "Halt", "Garbage", "Torment", "Marvel", "Finding", "Even", "Ed",
      "Initially", "Establishment", "Uh", "Alike", "Perjury", "Snake", "Cab",
      "Essence", "Shows", "Lungs", "Skilled", "Cheer", "Leaning", "Acres",
      "Ham", "Disturbing", "Device", "Nurse", "Factory", "Cease", "Arrow",
      "Range", "Darling", "Become", "Personage", "Explain", "Abundant",
      "Thief", "Missed", "Seriously", "Prisoners", "Specifically", "Bucket",
      "Abuse", "Cliff", "Are", "Integrated", "Regime", "Equally", "Powers",
      "Lesser", "Property", "Locked", "Tempted", "Oil", "Succeeded", "King",
      "Crushed", "Diversity", "Kid", "Attached", "Trainer", "Generosity",
      "Paying", "Greek", "Necessity", "Handwriting", "Baseball", "Silence",
      "Recruit", "Fools", "Junior", "Rip", "Freeze", "Activist", "Wolf",
      "Sensitive", "If", "Kept", "Victims", "Tracks", "Flexibility",
      "Overwhelmed", "Vast", "Obey", "Lay", "Follow", "Lady", "Etc", "Halls",
      "Great", "Overwhelming", "Mark", "Instantly", "Stolen", "Weak", "Rose",
      "Sack", "Odd", "Expectation", "Boil", "Hotel", "Automobile", "Approved",
      "Strings", "Subsequent", "Therefore", "Fraction", "Maid", "Dollar",
      "Eminent", "Lecture", "Thinking", "Unpleasant", "Capital", "Liberal",
      "Microphone", "Drain", "Alternative", "Canadian", "Lists", "Sweep",
      "Interrupt", "Declare", "Lion", "Senate", "Engines", "Fell", "Companion",
      "Buddy", "Twisted", "Forbidden", "Tribe", "Moderate", "Father", "Christ",
      "Consecutive", "Leaves", "Mobile", "Legally", "Efficiency", "Issues",
      "Felony", "Computers", "Nice", "Gave", "Apply", "Elaborate",
      "Developmental", "Despise", "Swallow", "Did", "Sweater", "Individual",
      "Sands", "Pretty", "Hurry", "Downtown", "Reduction", "Blows", "Grey",
      "Throne", "Declared", "Engineering", "Committed", "Epidemic", "Commerce",
      "Pitcher", "Lived", "Gray", "Legislator", "Limb", "Treasure",
      "Submission", "Answering", "Latter", "Kings", "Accepting", "Parental",
      "Concert", "Wearing", "Crosses", "Keep", "Network", "Fed", "Suffered",
      "Militia", "Trip", "Faculty", "Ultimate", "Bible", "Greet", "Council",
      "Confuse", "Journalism", "Thin", "Plot", "Plane", "Forget", "Therapy",
      "Top", "Wealthy", "Pageant", "Fierce", "Religion", "Independence",
      "Dame", "Luck", "Mask", "Sword", "Cheat", "Prospect", "Actual",
      "Joining", "Vaguely", "Construction", "Taxpayer", "Section", "Tuck",
      "Songs", "Naked", "Returns", "Trivial", "Cousin", "Occupied", "Exceed",
      "Impatient", "Once", "Echo", "Felon", "Examine", "Vent", "Lieu",
      "Investigation", "Stern", "Striking", "Neither", "Tolerate", "Concern",
      "Monthly", "Organize", "Odds", "Perform", "Saw", "Ditch", "Fiber",
      "Thereby", "Senator", "Fingers", "Exception", "Candidate", "Stare",
      "Behavior", "Haunted", "Fork", "Deepest", "Visit", "Chairs", "Attack",
      "File", "Inflation", "Inquire", "Hockey", "Had", "Curiosity",
      "Diversion", "Tune", "Apple", "Quarters", "Drawing", "Legitimate",
      "Staring"};
      
    // FIXME: 
    var experiment = new RepFRExperiment(input_word_list);
    var session = experiment.GenerateSession();

    for (int i=0; i<session.Count; i++) {
      Console.WriteLine("-----------------------------------");
      Console.WriteLine("List {0} ({1}) - {2}", i, session[i].encoding_stim,
          session[i].encoding);
      Console.WriteLine("List {0} ({1}) - {2}", i, session[i].recall_stim,
          session[i].recall);

      StimCheck(session[i].encoding_stim, session[i].encoding);
      StimCheck(session[i].recall_stim, session[i].recall);
    }
  }
}
#endif