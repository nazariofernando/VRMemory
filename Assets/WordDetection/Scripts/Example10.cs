using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Example of verbal commands
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class Example10 : Example4
{
    public MeshRenderer m_PrefabIdle = null;
    public MeshRenderer[] m_expressions = null;

    private DateTime m_timerClearMic = DateTime.MinValue;

    private String m_command = "Noise";

    private bool m_recordingSample = false;

    private bool m_wordsChanged = false;

	private MeshRenderer m_Noise = null;

    private MeshRenderer Duplicate(MeshRenderer mr)
    {
        GameObject go = (GameObject)Instantiate(mr.gameObject, Vector3.zero, Quaternion.identity);
        go.name = mr.transform.parent.name;
        return go.GetComponent<MeshRenderer>();
    }

    /// <summary>
    /// Initialize the example
    /// </summary>
    protected override void Start()
    {
        if (null == AudioWordDetection ||
            null == Mic)
        {
            Debug.LogError("Missing meta references");
            return;
        }

        m_Noise = Duplicate(m_PrefabIdle);
        for (int index = 0; index < m_expressions.Length; ++index)
        {
            m_expressions[index] = Duplicate(m_expressions[index]);
        }

        Dictionary<String, WordDetails> words = new Dictionary<String, WordDetails>();

        WordDetails noise = new WordDetails() {Label = "Noise"};
        AudioWordDetection.Words.Add(noise);

        // prepopulate words
        foreach (MeshRenderer expression in m_expressions)
        {
            //Debug.Log(val);
            String command = expression.name;
            WordDetails details = new WordDetails() { Label = command };
            AudioWordDetection.Words.Add(details);
            words[command] = details;
#if !UNITY_WEBPLAYER
            try
            {
                string path = string.Format("Assets/{0}_{1}.profile", GetType().Name, command);
                if (File.Exists(path))
                {
                    using (
                        FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                        )
                    {
                        using (BinaryReader br = new BinaryReader(fs))
                        {
                            AudioWordDetection.LoadWord(br, details);
                            //Debug.Log(string.Format("Loaded profile: {0}", path));
                            details.Label = command;
                        }
                    }
                }
                else
                {
                    Debug.Log(string.Format("Profile not available for: {0}", path));
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("Failed to load word: {0}", ex));
            }
#endif
        }

        //subscribe detection event
        AudioWordDetection.WordDetectedEvent += WordDetectedHandler;
    }

    /// <summary>
    /// Handle word detected event
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void WordDetectedHandler(object sender, WordDetection.WordEventArgs args)
    {
        // skip detection while recording
        if (m_recordingSample)
        {
            return;
        }

        if (null == args.Details ||
            string.IsNullOrEmpty(args.Details.Label))
        {
            m_command = "Noise";
        }
        else
        {
            m_command = args.Details.Label;
            if (m_command != "Noise")
            {
                m_timerClearMic = DateTime.Now + TimeSpan.FromMilliseconds(200);
            }
        }
    }

    void SaveProfile(WordDetails details)
    {
#if !UNITY_WEBPLAYER
        string path = string.Format("Assets/{0}_{1}.profile", GetType().Name, details.Label);
        if (!string.IsNullOrEmpty(path))
        {
            try
            {
                using (
                    FileStream fs = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write,
                        FileShare.ReadWrite)
                    )
                {
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {
                        AudioWordDetection.SaveWord(bw, details);
                        //Debug.Log(string.Format("Save profile: {0}", details.Label));
                    }
                }
            }
            catch (Exception)
            {
                Debug.LogError(string.Format("Failed to save profile: {0}", details.Label));
            }
        }
#endif
    }

    /// <summary>
    /// GUI event
    /// </summary>
    protected override void OnGUI()
    {
        if (null == AudioWordDetection ||
            null == Mic ||
            string.IsNullOrEmpty(Mic.DeviceName))
        {
            return;
        }

        GUILayout.Label(string.Empty);

        GUILayout.Label(m_command);

        Color backgroundColor = GUI.backgroundColor;

        for (int wordIndex = 0; wordIndex < AudioWordDetection.Words.Count; ++wordIndex)
        {
            if (m_wordsChanged)
            {
                m_wordsChanged = false;
                GUIUtility.ExitGUI();
            }

            if (!m_recordingSample &&
                AudioWordDetection.ClosestIndex == wordIndex)
            {
                GUI.backgroundColor = Color.red;
            }
            else
            {
                GUI.backgroundColor = backgroundColor;
            }

            WordDetails noise = GetWord(WORD_NOISE);

            if (null == noise)
            {
                continue;
            }

            if (wordIndex > 0)
            {
                GUI.enabled = null != noise.SpectrumReal;
            }

            GUILayout.BeginHorizontal(GUILayout.MinWidth(600));
            WordDetails details = AudioWordDetection.Words[wordIndex];

            if (GUILayout.Button("Play", GUILayout.Height(45), GUILayout.Width(75)))
            {
                if (null != details.Audio)
                {
                    if (NormalizeWave)
                    {
                        GetComponent<AudioSource>().PlayOneShot(details.Audio, 0.1f);
                    }
                    else
                    {
                        GetComponent<AudioSource>().PlayOneShot(details.Audio);
                    }
                }

                // show profile
                RefExample.OverrideSpectrumImag = true;
                RefExample.SpectrumImag = details.SpectrumReal;
            }

            if (wordIndex == 0)
            {
                GUILayout.Label(details.Label, GUILayout.Width(150), GUILayout.Height(45));
            }
            else
            {
                details.Label = GUILayout.TextField(details.Label, GUILayout.Width(150), GUILayout.Height(45));
            }

            Color oldColor = GUI.color;
            if (m_recordingSample)
            {
                GUI.color = Color.green;
            }
            GUILayout.Button(string.Format("{0}",
                    (null == details.SpectrumReal) ? "not set" : "set"), GUILayout.Width(75), GUILayout.Height(45));
            if (m_recordingSample)
            {
                GUI.color = oldColor;
            }

            Event e = Event.current;
            if (null != e)
            {
                Rect rect = GUILayoutUtility.GetLastRect();
                bool overButton = rect.Contains(e.mousePosition);

                if (m_buttonIndex == -1 &&
                    m_timerStart == DateTime.MinValue &&
                    Input.GetMouseButton(0) &&
                    overButton)
                {
                    //Debug.Log("Initial button down");
                    m_buttonIndex = wordIndex;
                    m_startPosition = Mic.GetPosition();
                    m_timerStart = DateTime.Now + TimeSpan.FromSeconds(Mic.CaptureTime);
                    m_recordingSample = true;
                }
                if (m_buttonIndex == wordIndex)
                {
                    bool buttonUp = Input.GetMouseButtonUp(0);
                    if (m_timerStart > DateTime.Now &&
                        !buttonUp)
                    {
                        //Debug.Log("Button still pressed");
                        m_recordingSample = true;
                    }
                    else if (m_timerStart != DateTime.MinValue &&
                        m_timerStart < DateTime.Now)
                    {
                        //Debug.Log("Button timed out");
                        SetupWordProfile(false);
                        m_timerStart = DateTime.MinValue;
                        m_buttonIndex = -1;
                        m_recordingSample = false;
                        Mic.ClearData();
                    }
                    else if (m_timerStart != DateTime.MinValue &&
                        buttonUp &&
                        m_buttonIndex != -1)
                    {
                        //Debug.Log("Button is no longer pressed");
                        SetupWordProfile(true);
                        m_timerStart = DateTime.MinValue;
                        m_buttonIndex = -1;
                        m_recordingSample = false;
                        Mic.ClearData();
                        SaveProfile(details);
                    }
                }
            }
            //GUILayout.Label(details.Score.ToString());
            //GUILayout.Label(string.Format("{0}", details.GetMinScore(DateTime.Now - TimeSpan.FromSeconds(1))));
            GUILayout.EndHorizontal();

            if (wordIndex > 0)
            {
                GUI.enabled = null != noise.SpectrumReal;
            }

            GUILayout.Space(10);
        }

        GUI.backgroundColor = backgroundColor;
    }

    void Update()
    {
        // skip detection while recording
        if (m_recordingSample)
        {
            return;
        }

        bool found = false;
        foreach (MeshRenderer expression in m_expressions)
        {
            String command = expression.name;
            if (command == m_command)
            {
                expression.enabled = true;
                found = true;
            }
            else
            {
                expression.enabled = false;
            }
        }
        m_Noise.enabled = !found;

        if (m_timerClearMic != DateTime.MinValue)
        {
            if (m_timerClearMic < DateTime.Now)
            {
                m_timerClearMic = DateTime.MinValue;
                Mic.ClearData();
            }
        }
    }
}