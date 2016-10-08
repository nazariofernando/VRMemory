#pragma strict

class SpeechRecognitionExampleJs extends MonoBehaviour implements ISpeechRecognitionListenerJs{

	private var fontStyle : GUIStyle = new GUIStyle();
	
	private var lastResults : String = "";
	
	private static var relativePosition : Vector2 = new Vector2(1,1); //used for the SpeechDictionary example

	private var touchZone : Rect = new Rect(0,Screen.height/4f,Screen.width,Screen.height/4f);

	private var currentOrientation : ScreenOrientation = ScreenOrientation.Portrait;

	//example of ISpeechRecognitionListener
	function OnBeginningOfSpeech ()
	{}

	function OnBufferReceived (buffer:String)
	{}

	function OnEndOfSpeech ()
	{}

	function OnError (error:int, errorMessage:String)
	{
		lastResults = errorMessage;
	}

	function OnEvent (eventType:int, bundle:String)
	{}

	function OnPartialResults (partialResults:String)
	{}

	function OnReadyForSpeech (bundle:String)
	{}

	function OnResults (results:String)
	{
		lastResults = results.Replace('\n',',');
	}

	function OnRmsChanged (rmsdB:float)
	{}
	
	function OnChangeState (newState:int)
	{
		if(newState == SpeechRecognition.NOT_INITIALIZED){
			Camera.main.backgroundColor = Color.red;
		}else if(newState == SpeechRecognition.IDLE){
			Camera.main.backgroundColor = Color.blue;
		}else if(newState == SpeechRecognition.LISTENING_TO_SOUND){
			Camera.main.backgroundColor = Color.yellow;
		}else if(newState == SpeechRecognition.LISTINING_TO_SPEECH_INIT){
			Camera.main.backgroundColor = new Color(1f,0.5f,0);
		}else if(newState == SpeechRecognition.LISTENING_TO_SPEECH){
			Camera.main.backgroundColor = Color.green;
		}
	}

	function Start () {
		fontStyle.fontSize = 16;
		fontStyle.alignment = TextAnchor.MiddleCenter;

		SpeechRecognition.AddSpeechRecognitionListerenJs(this);

		SpeechRecognition.SetTouchToListenZone(touchZone,true);
	}

	function Update () {
		if(SpeechRecognition.CommandRecognized("UP")){
			relativePosition = new Vector2(relativePosition.x,Mathf.Clamp(relativePosition.y - 1,0,2));
		}
		if(SpeechRecognition.CommandRecognized("DOWN")){
			relativePosition = new Vector2(relativePosition.x,Mathf.Clamp(relativePosition.y + 1,0,2));
		}
		if(SpeechRecognition.CommandRecognized("LEFT")){
			relativePosition = new Vector2(Mathf.Clamp(relativePosition.x - 1,0,2), relativePosition.y);
		}
		if(SpeechRecognition.CommandRecognized("RIGHT")){
			relativePosition = new Vector2(Mathf.Clamp(relativePosition.x + 1,0,2), relativePosition.y);
		}

		if(currentOrientation != Screen.orientation){
			currentOrientation = Screen.orientation;
			touchZone = new Rect(0,Screen.height/4f,Screen.width,Screen.height/4f);
			SpeechRecognition.SetTouchToListenZone(touchZone,true);
		}
	}
	
	function OnGUI(){
		var enabledBackup = GUI.enabled;

		GUI.Label(new Rect(0,0,Screen.width,Screen.height/8f),lastResults,fontStyle);

		if(SpeechRecognition.GetTouchToListenEnabled()){
			GUI.DrawTexture(touchZone,Resources.Load("SpeechRecognitionExample/grey",Texture2D));
			GUI.Label(touchZone, "Press here to listen to speech",fontStyle);
		}else{
			if(GUI.Button(new Rect(0,Screen.height/4f,Screen.width/2f,Screen.height/4f), "Start listening")){
				SpeechRecognition.StartListening();
			}

			if(GUI.Button(new Rect(Screen.width/2f,Screen.height/4f,Screen.width/2f,Screen.height/4f), "Stop listening")){
				SpeechRecognition.StopListening();
			}
		}

		SpeechRecognition.SetTouchToListenEnabled(GUI.Toggle(new Rect(0,Screen.height*1f/8f,Screen.width/2f,Screen.height/8), SpeechRecognition.GetTouchToListenEnabled(), "Touch to listen"));
		SpeechRecognition.instance.autoRestart = GUI.Toggle(new Rect(Screen.width/2f,Screen.height*1f/8f,Screen.width/2f,Screen.height/8), SpeechRecognition.instance.autoRestart, "Auto restart on sound");

		GUI.DrawTexture(new Rect((Screen.width-Screen.height/2f)/2f,Screen.height/2f,Screen.height/2f,Screen.height/2f),Resources.Load("SpeechRecognitionExample/3x3_grid",Texture2D));

		GUI.DrawTexture(new Rect((Screen.width-Screen.height/2f)/2f+Screen.height*relativePosition.x/6f,Screen.height/2f+Screen.height*relativePosition.y/6f,Screen.height/6f,Screen.height/6f),Resources.Load("SpeechRecognitionExample/x",Texture2D));

		GUI.enabled = enabledBackup;
	}

}