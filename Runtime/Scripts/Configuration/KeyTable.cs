using System;
using System.Collections.Generic;
using UnityEngine;

public static class KeyLookup {

// Note: does not support modifier keys, so characters accessed by
// the shift key are not available through this system
static Dictionary<int, string> OSXKeyCodes = new Dictionary<int, string> {
{6, "Z"},
{16, "Y"},
{7, "X"},
{13, "W"},
{9, "V"},
{126, "UpArrow"},
{32, "U"},
{17, "T"},
{1, "S"},
{124, "RightArrow"},
{15, "R"},
{12, "Q"},
{116, "PageUp"},
{121, "PageDown"},
{35, "P"},
{31, "O"},
{45, "N"},
{46, "M"},
{123, "LeftArrow"},
{37, "L"},
{40, "K"},
{38, "J"},
{34, "I"},
{4, "H"},
{5, "G"},
{3, "F"},
{53, "Escape"},
{14, "E"},
{125, "DownArrow"},
{51, "Delete"},
{2, "D"},
{8, "C"},
{11, "B"},
{0, "A"},
{30, "]"},
{42, "Slash"},
{33, "["},
{24, "="},
{81, "="},
{41, ";"},
{44, "/"},
{75, "/"},
{47, "."},
{65, "."},
{27, "-"},
{78, "-"},
{43, ","},
{69, "+"},
{67, "*"},
{49, "Space"},
{39, "'"},
{25, "9"},
{92, "9"},
{28, "8"},
{91, "8"},
{26, "7"},
{89, "7"},
{22, "6"},
{88, "6"},
{23, "5"},
{87, "5"},
{21, "4"},
{86, "4"},
{20, "3"},
{85, "3"},
{19, "2"},
{84, "2"},
{18, "1"},
{83, "1"},
{29, "0"},
{82, "0"},
{36, "Return"},
{76, "Enter"}
};

public static string get(int code, bool isOSX=true) {
    if(isOSX) {
        string value;
        if(OSXKeyCodes.TryGetValue(code, out value)) {
                return (value ?? "none").ToLower();
        }
    }
    else {
        // FIXME: Unity reports Alpha# or Keypad# for number keys
        // the right way to handle this is likely to switch from
        // strings to the Unity enum and have the keytable translate
        // to the enum type
        return (Enum.GetName(typeof(KeyCode), code) ?? "none").ToLower();
    }
    
    return "none";
}
}