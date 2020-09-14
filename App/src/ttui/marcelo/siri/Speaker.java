package ttui.marcelo.siri;

import java.util.HashMap;
import java.util.Locale;

import android.content.Context;
import android.media.AudioManager;
import android.speech.tts.TextToSpeech;
import android.speech.tts.TextToSpeech.OnInitListener;
import android.speech.tts.UtteranceProgressListener;
import android.util.Log;

public class Speaker extends UtteranceProgressListener implements OnInitListener {

	public TextToSpeech tts;
	
	private boolean ready = false;
	private boolean allowed = false;
	
	public Speaker(Context context){
		tts = new TextToSpeech(context, this);	
		tts.setOnUtteranceProgressListener(this);
	}	
	
	public boolean isAllowed(){
		return allowed;
	}
	
	public void allow(boolean allowed){
		this.allowed = allowed;
	}

	@Override
	public void onInit(int status) {
		if(status == TextToSpeech.SUCCESS){
			// Change this to match your
			// locale
			tts.setLanguage(Locale.US);
			ready = true;
		}else{
			ready = false;
		}
	}
	
	public void speak(String text){
		
		// Speak only if the TTS is ready
		// and the user has allowed speech
		
		if(ready && allowed) {
			HashMap<String, String> hash = new HashMap<String,String>();
			hash.put(TextToSpeech.Engine.KEY_PARAM_UTTERANCE_ID, 
					String.valueOf(AudioManager.STREAM_NOTIFICATION));
			tts.speak(text, TextToSpeech.QUEUE_FLUSH, hash);
		}
	}
	
	public void pause(int duration){
		HashMap<String, String> hash = new HashMap<String,String>();
		hash.put(TextToSpeech.Engine.KEY_PARAM_UTTERANCE_ID, 
				String.valueOf(AudioManager.STREAM_NOTIFICATION));
		tts.playSilence(duration, TextToSpeech.QUEUE_FLUSH, hash);
	}
		
	// Free up resources
	public void destroy(){
		tts.shutdown();
	}


	@Override
	public void onStart(String utteranceId) {
		
	}

	@Override
	public void onDone(String utteranceId) {
		
	}

	@Override
	@Deprecated
	public void onError(String utteranceId) {
		// TODO Auto-generated method stub
		
	}
	
}
