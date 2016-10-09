using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object=UnityEngine.Object;

/// <summary>
/// Example of verbal commands
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class Example16 : Example4
{
    public MeshRenderer VideoStage = null;

    public Material MaterialIdle = null;
    public Material MaterialVideo1 = null;
    public Material MaterialVideo2 = null;
    public Material MaterialVideo3 = null;
    public Material MaterialVideo4 = null;
    public Material MaterialVideo5 = null;

    public AudioClip Audio1 = null;
    public AudioClip Audio2 = null;
    public AudioClip Audio3 = null;
    public AudioClip Audio4 = null;
    public AudioClip Audio5 = null;

    private DateTime m_timerClearMic = DateTime.MinValue;

    private bool m_recordingSample = false;

    private bool m_wordsChanged = false;

    public AudioSource GoatAudioSource = null;

    private DateTime m_timerGoat = DateTime.MinValue;

#if !UNITY_XBOX360 && !UNITY_XBOXONE
    private WebCamTexture m_webTexture = null;
#endif
    private string m_webDevice = string.Empty;

    enum Commands
    {
        Noise,
        One,
        Two,
        Three,
        Four,
        Five,
    }

    Commands m_command = Commands.Noise;

    public class Mapping
    {
        public Material m_material = null;
        public AudioClip m_audioClip = null;
    }

    Dictionary<Commands, Mapping> m_map = new Dictionary<Commands, Mapping>();

#if !UNITY_XBOX360 && !UNITY_XBOXONE
    private void UpdateWebCam()
    {
        DestroyWebCam();
        if (!string.IsNullOrEmpty(m_webDevice))
        {
            Debug.Log(string.Format("Creating Web Camera for: {0}", m_webDevice));
            m_webTexture = new WebCamTexture(m_webDevice, 640, 480, 30);
            m_webTexture.Play();
            MaterialIdle.mainTexture = m_webTexture;
        }
    }

    private void DestroyWebCam()
    {
        if (null != m_webTexture)
        {
            m_webTexture.Stop();
            Object.DestroyImmediate(m_webTexture, true);
            m_webTexture = null;
        }
    }
#endif

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

        if (null == VideoStage)
        {
            Debug.LogError("Missing Video Stage");
            return;
        }

        m_map.Add(Commands.Noise, new Mapping() { m_material = MaterialIdle, m_audioClip = null });
        m_map.Add(Commands.One, new Mapping() { m_material = MaterialVideo1, m_audioClip = Audio1 });
        m_map.Add(Commands.Two, new Mapping() { m_material = MaterialVideo2, m_audioClip = Audio2 });
        m_map.Add(Commands.Three, new Mapping() { m_material = MaterialVideo3, m_audioClip = Audio3 });
        m_map.Add(Commands.Four, new Mapping() { m_material = MaterialVideo4, m_audioClip = Audio4 });
        m_map.Add(Commands.Five, new Mapping() { m_material = MaterialVideo5, m_audioClip = Audio5 });

        // prepopulate words
        foreach (string val in Enum.GetNames(typeof(Commands)))
        {
            //Debug.Log(val);
            WordDetails details = new WordDetails() {Label = val};
            AudioWordDetection.Words.Add(details);

#if !UNITY_WEBPLAYER
            try
            {
                string path = string.Format("Assets/{0}_{1}.profile", GetType().Name, val);
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
                            details.Label = val;
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

#if !UNITY_XBOX360 && !UNITY_XBOXONE
    private void OnDestroy()
    {
        DestroyWebCam();
    }
#endif

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
            m_command = Commands.Noise;
            return;
        }

        m_command = (Commands)Enum.Parse(typeof(Commands), args.Details.Label, false);
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

#if !UNITY_XBOX360 && !UNITY_XBOXONE
        if (string.IsNullOrEmpty(m_webDevice))
        {
            WebCamDevice[] devices = WebCamTexture.devices;
            GUILayout.BeginHorizontal(GUILayout.Width(Screen.width));
            GUILayout.FlexibleSpace();
            GUILayout.Label("Choose a camera:");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            foreach (WebCamDevice device in devices)
            {
                GUILayout.BeginHorizontal(GUILayout.Width(Screen.width));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(device.name, GUILayout.MinHeight(40)))
                {
                    m_webDevice = device.name;
                    UpdateWebCam();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        }
#endif

        GUILayout.Label(string.Empty);

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
                GUILayout.Label(details.Label, GUILayout.Width(100), GUILayout.Height(45));
            }
            else
            {
                details.Label = GUILayout.TextField(details.Label, GUILayout.Width(100), GUILayout.Height(45));
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

        if (null == VideoStage)
        {
            return;
        }

        if (m_map.ContainsKey(m_command))
        {
            if (m_timerGoat < DateTime.Now)
            {
                if (m_map[m_command].m_material)
                {
                    VideoStage.material = m_map[m_command].m_material;
                    if (VideoStage.material.mainTexture)
                    {
                        MovieTexture movieTexture = VideoStage.material.mainTexture as MovieTexture;
                        if (movieTexture)
                        {
                            movieTexture.Stop();
                            movieTexture.Play();

                            if (m_map[m_command].m_audioClip)
                            {
                                GoatAudioSource.mute = false;
                                GoatAudioSource.loop = false;
                                m_timerGoat = DateTime.Now + TimeSpan.FromSeconds(m_map[m_command].m_audioClip.length);
                                GoatAudioSource.PlayOneShot(m_map[m_command].m_audioClip);
                            }
                        }
                    }
                }
            }
        }

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