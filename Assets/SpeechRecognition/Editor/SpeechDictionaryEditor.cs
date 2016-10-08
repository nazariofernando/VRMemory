using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(SpeechDictionary))] 
public class SpeechDictionaryEditor : Editor {

	private string newCommandName = "";
	private string newSpeechText = "";

	public override void OnInspectorGUI() {
		SpeechDictionary speechDictionary = (SpeechDictionary) target;
		speechDictionary.enableDictionary = EditorGUILayout.Toggle("Enabled",speechDictionary.enableDictionary);
		bool enabledBackup = GUI.enabled;
		EditorGUILayout.Separator();
		GUI.enabled = speechDictionary.enabled;
		{
			string removeKey = "";
			int removeKeyIndex = -1;
			Dictionary<string,List<string>> commands = speechDictionary.commands; 
			foreach(KeyValuePair<string,List<string>> kvp in commands){
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.LabelField(kvp.Key);
					if(GUILayout.Button("X")){
						commands.Remove(kvp.Key);
					}
				}
				EditorGUILayout.EndHorizontal();

				for(int i = 0; i < kvp.Value.Count; i++){
					EditorGUILayout.BeginHorizontal();
					{
						commands[kvp.Key][i] = EditorGUILayout.TextField("  Speech text",kvp.Value[i].ToLower());
						if(GUILayout.Button("X")){
							removeKey = kvp.Key;
							removeKeyIndex = i;
						}
					}
					EditorGUILayout.EndHorizontal();

				}

				EditorGUILayout.BeginHorizontal();
				{
					newSpeechText = EditorGUILayout.TextField("  Speech text",newSpeechText).ToLower();
					if(GUILayout.Button("+")){
						commands[kvp.Key].Add(newSpeechText);
						newSpeechText = "";
					}
				}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Space();
			}
			if(removeKeyIndex >= 0){
				commands[removeKey].RemoveAt(removeKeyIndex);
			}

			EditorGUILayout.Space();
			newCommandName = EditorGUILayout.TextField("New Command",newCommandName);
			if(GUILayout.Button("Add Command")){
				commands.Add(newCommandName,new List<string>());
				newCommandName = "";
			}

		}
		GUI.enabled = enabledBackup;

		//myTarget.MyValue = EditorGUILayout.IntSlider("Val-you", myTarget.MyValue, 1, 10);
	}
}
