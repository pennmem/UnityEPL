using UnityEngine;
using TMPro;
using System.Collections.Generic;

public static class UnityUtilities {
    public static bool IsMacOS() {
        return Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer;
    }

    public static float FindMaxFittingFontSize(List<string> strings, TextMeshProUGUI textComponent) {
        string oldText = textComponent.text;
        bool oldAutosizing = textComponent.enableAutoSizing;
        textComponent.enableAutoSizing = true;
        float maxFontSize = 300;
        foreach (var str in strings) {
            textComponent.text = str;
            textComponent.ForceMeshUpdate();
            if (textComponent.fontSize < maxFontSize) {
                maxFontSize = textComponent.fontSize;
            }
        }
        textComponent.enableAutoSizing = oldAutosizing;
        textComponent.text = oldText;

        return maxFontSize;
    }
}
