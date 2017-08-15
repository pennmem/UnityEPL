using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WordListGenerator))]
[CanEditMultipleObjects]
public class WordListGeneratorEditor : Editor 
{
	public override void OnInspectorGUI()
	{
		WordListGenerator currentTarget = (WordListGenerator)target;

		if (GUILayout.Button ("Load word list"))
		{
			string selected_path = EditorUtility.OpenFilePanel ("Select word list", "", "");
			string[] words = System.IO.File.ReadAllLines (selected_path);
			currentTarget.unshuffled_words = words;
			EditorUtility.SetDirty (target);
		}
		GUILayout.Label ("Words loaded: " + currentTarget.unshuffled_words.Length.ToString ());
	}
}