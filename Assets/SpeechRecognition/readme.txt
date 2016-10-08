Notes:
------
	- Android speech recognition only allows 15 seconds of recorded data. So the recording will automaticly stop 15 seconds after has been started. The plugin has an auto restart functionality to listen to multiple commands in a row.
	- A working internet connection is necessary for speech recognition for all devices running an Android version below 4.1. 
	- On Android 4.1+ the users can choose to download language package so the speech recognition also works offline. This is done in the menu Settings -> Language and Input -> Voice Search
	- More information about the events used in the SpeechRecognizer can be found on: http://developer.android.com/reference/android/speech/SpeechRecognizer.html
	
Steps to include Android Speech Recognition in your app:
--------------------------------------------------------

1. Add the prefab Assets/SpeechRecognition/SpeechRecognition to your first scene.

2. Configuring the game object (all settings can be changed at runtime):
	- The value "Max Result" is the number of results the web request will return. When using offline recognition you will only receive the 'best' result (Android limitation).
	- Change the "Preferred Language" to the language you want the speech recognizer must use.
	- Enable all the events you want to listen to in the inspector tab. To improve performance, disable all the events you don't use.
	- Enable "Auto Restart" if you want the speech recognizer to be restarted on a loud sound (like speech). The value "Auto Restart Amp Threshold" is the threshold used to restart the speech recognizer when using Auto Restart.
	- Enable "Auto Restart On Resume" if you want the speech recognizer to be restarted when the app is resumed.
	- Enable "Disable Screen Lock On Result" if you want to avoid the screen locking when only using speech commands. The value "Screen Lock Timeout" is the time the plugin will wait before re-enabling the screen-lock.
	
	- Enable the Speech Dictionary if you want to use it.
	- Update the Dictionary to the values you want to use.

3. When you want to listen to the events and the results directly:
	- Add a class who implements the interface ISpeechRecognitionListener (or ISpeechRecognitionListenerJs for JavaScript)
	- Connect your listener in the Start of a monobehavior script by calling:
		SpeechRecognition.AddSpeechRecognitionListeren(ISpeechRecognitionListener listener);
		(or SpeechRecognition.AddSpeechRecognitionListerenJs(ISpeechRecognitionListener listener); for JavaScript)
	- Implement the interface.
	
4. When you are using the SpeechDictionary:
	- Add code in the Update of a MonoBehavior to listen to commands. See the Update function of Example.cs or ExampleJs.js for more information.
	
5. Add code to start the speech recognition.
	- When using a touch zone:
		* Set the touch zone by calling SpeechRecognition.SetTouchToListenZone(Rect zone, bool usingUISpace); When you can use the passed rectangle for UI elements, usingUISpace should be true.
		* Add a visible space so the users know where to press when they want to give a speech command.
	- When not using a touch zone:
		* Call SpeechRecognition.StartListening(); to start listening.
		* Call SpeechRecognition.StopListening(); to stop listening.
		