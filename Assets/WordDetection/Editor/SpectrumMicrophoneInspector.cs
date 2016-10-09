﻿using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpectrumMicrophone))]
public class SpectrumMicrophoneInspector : Editor
{
    [MenuItem("GameObject/Create Other/Audio/Create Spectrum Microphone")]
    public static void MenuCreateMic()
    {
        GameObject go = new GameObject("SpectrumMicrophone");
        go.AddComponent<SpectrumMicrophone>();
    }

    public override void OnInspectorGUI()
    {
        SpectrumMicrophone item = target as SpectrumMicrophone;

        int captureTime = item.CaptureTime;
        int sampleRate = item.SampleRate;

        GUILayout.BeginHorizontal();
        GUILayout.Label("Capture Time:");
        item.CaptureTime = (int)GUILayout.HorizontalSlider(item.CaptureTime, 1, 16);
        item.CaptureTime = EditorGUILayout.IntField(item.CaptureTime);
        int log = (int)Mathf.Log(item.CaptureTime, 2);
        item.CaptureTime = (int)Mathf.Pow(2, log);
        item.CaptureTime = Mathf.Min(item.CaptureTime, 16);
        item.CaptureTime = Mathf.Max(item.CaptureTime, 1);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Sample Rate:");
        item.SampleRate = (int)GUILayout.HorizontalSlider(item.SampleRate, 1024, 65536);
        item.SampleRate = EditorGUILayout.IntField(item.SampleRate);
        log = (int)Mathf.Log(item.SampleRate, 2);
        item.SampleRate = (int)Mathf.Pow(2, log);
        item.SampleRate = Mathf.Min(item.SampleRate, 65536);
        item.SampleRate = Mathf.Max(item.SampleRate, 1024);
        GUILayout.EndHorizontal();

        if (captureTime != item.CaptureTime ||
            sampleRate != item.SampleRate)
        {
            if (EditorApplication.isPlaying)
            {
                item.CleanUp();
                item.InitData();
            }
        }
    }
}