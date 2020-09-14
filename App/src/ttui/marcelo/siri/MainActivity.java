package ttui.marcelo.siri;

import java.util.ArrayList;
import java.util.LinkedList;
import java.util.Timer;
import java.util.TimerTask;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

import android.net.ConnectivityManager;
import android.speech.RecognitionListener;
import android.speech.RecognizerIntent;
import android.speech.SpeechRecognizer;
import android.speech.tts.TextToSpeech;
import android.speech.tts.TextToSpeech.OnUtteranceCompletedListener;
import android.app.Activity;
import android.app.AlertDialog;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.hardware.Camera;
import android.hardware.Camera.Parameters;
import android.net.wifi.WifiManager;
import android.os.Bundle;
import android.os.Vibrator;
import android.util.Log;
import android.view.ContextMenu;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.view.WindowManager;
import android.view.ContextMenu.ContextMenuInfo;
import android.widget.CompoundButton;
import android.widget.CompoundButton.OnCheckedChangeListener;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;
import android.widget.ToggleButton;

public class MainActivity extends android.support.v7.app.ActionBarActivity implements
RecognitionListener, OnUtteranceCompletedListener {
	private final int TIME_TICK=1500;
	protected static final long VIBRATION_TIME = 350;
	private final int CHECK_CODE = 0x1; 
	private Speaker speaker;
	private final int NUM_PATRONES=7;
	private TextView returnedText;
	private ToggleButton toggleButton,toggleButton2;
	private ProgressBar progressBar;
	private SpeechRecognizer speech = null;
	private Intent recognizerIntent;
	private String LOG_TAG = "VoiceRecognitionActivity";
	String patterns[];
	float volume;
	boolean bool=false;
	int lastPat=-1;
	private String num;
	private TCPClient client;
	private Context context;
	private RecognitionListener recog;
	private final String powers="smaller|enlarge|freeze|wall|tangible|type";
	private int player=0;
	private LinkedList<String> listPower;
	private LinkedList<AppData> requests;
	private Camera cam;
	
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		
		getWindow().addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
		//getWindow().setFormat(PixelFormat.TRANSLUCENT);
		getWindow().setFlags(WindowManager.LayoutParams.FLAG_FULLSCREEN,
				WindowManager.LayoutParams.FLAG_FULLSCREEN);
		
		setContentView(R.layout.activity_main);
		client = new TCPClient(this);
		context=this;
		recog=this;
		client.wifi= (WifiManager) getSystemService(WIFI_SERVICE);
		client.connec = (ConnectivityManager) this
				.getSystemService(Context.CONNECTIVITY_SERVICE);
		
		returnedText = (TextView) findViewById(R.id.textView1);
		progressBar = (ProgressBar) findViewById(R.id.progressBar1);
		toggleButton = (ToggleButton) findViewById(R.id.toggleButton1);
		toggleButton2 = (ToggleButton) findViewById(R.id.toggleButton2);
		progressBar.setVisibility(View.INVISIBLE);
		
		recognizerIntent = new Intent(RecognizerIntent.ACTION_RECOGNIZE_SPEECH);
		recognizerIntent.putExtra(RecognizerIntent.EXTRA_LANGUAGE_PREFERENCE,
				"en");
		recognizerIntent.putExtra(RecognizerIntent.EXTRA_CALLING_PACKAGE,
				this.getPackageName());
		recognizerIntent.putExtra(RecognizerIntent.EXTRA_LANGUAGE_MODEL,
				RecognizerIntent.LANGUAGE_MODEL_WEB_SEARCH);
		recognizerIntent.putExtra(RecognizerIntent.EXTRA_MAX_RESULTS, 3);
		
		//Button ON/OFF
		OnCheckedChangeListener c=new OnCheckedChangeListener() {

			@Override
			public void onCheckedChanged(CompoundButton buttonView,
					boolean isChecked) {
				if (buttonView.getId()==toggleButton2.getId()) switchLight(isChecked);
				else{
					if (!bool){
						speech = SpeechRecognizer.createSpeechRecognizer(context);
						speech.setRecognitionListener(recog);
						bool=true;
					}
					if (isChecked) {
						progressBar.setVisibility(View.VISIBLE);
						progressBar.setIndeterminate(true);
						speech.startListening(recognizerIntent);
						
					} else {
						progressBar.setIndeterminate(false);
						progressBar.setVisibility(View.INVISIBLE);
						speech.stopListening();
					}
				}
			}
		};
		toggleButton.setOnCheckedChangeListener(c);
		toggleButton2.setOnCheckedChangeListener(c);
		//PATTERNS
		patterns=new String[NUM_PATRONES];
		
		patterns[0]="(?i)^.*?(use|spend|choose).*?(power|bonus)";
		patterns[1]="(?i)^.*?("+powers+")";
		patterns[2]="(?i).*(resume|play|start)";
		patterns[3]="(?i).*(pause|stop)";
		patterns[4]="(?i)^.*?help";
		patterns[5]="(?i)^.*?(hello|hi|good morning|good afternoon|good night)";
		patterns[6]="(?i)^.*?(what|thing).*?(can|could|know).*?(do|make|able)";
		
		listPower=new LinkedList<String>();
		requests=new LinkedList<AppData>();
	    checkTTS();
	    showDialog("Which player you are?","Choose the player","Player2","Player1");
	    
	}
	public void imageClick(View v){
		switch(v.getId()){
		case R.id.power_enlarge: usePowerAsk("enlarge");break;
		case R.id.power_smaller: usePowerAsk("smaller");break;
		case R.id.power_freeze: usePowerAsk("freeze");break;
		case R.id.power_wall: usePowerAsk("wall");break;
		case R.id.power_tangible: usePowerAsk("tangible");break;
		}
	}
	private void usePowerAsk(String s){
		showDialog("Are you sure of using?",s,"No","Yes");
	}
    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        getMenuInflater().inflate(R.menu.main, menu);
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        int id = item.getItemId();
        if (id == R.id.action_power) {
        	Intent myIntent = new Intent(this, ListViewActivity.class);
        	myIntent.putExtra("powers",listPower);
        	this.startActivityForResult(myIntent,2);
        } else if (id == R.id.action_play) send(new AppData(AppData.PLAY,player+""));
        else if (id == R.id.action_pause) send(new AppData(AppData.PAUSE,player+""));
        else if (id == R.id.action_surrender) send(new AppData(AppData.SUR,player+""));
        return super.onOptionsItemSelected(item);
    }
    private void switchLight(boolean on){
    	if (on){
    		cam = Camera.open();     
    		Parameters p = cam.getParameters();
    		p.setFlashMode(Parameters.FLASH_MODE_TORCH);
    		cam.setParameters(p);
    		cam.startPreview();
    	}else{
    		if (cam!=null){
    			cam.stopPreview();  
    			cam.release();
    		}
    	}
    }
	private void loop(){
		Timer timer=new Timer();
		timer.scheduleAtFixedRate(new TimerTask() {
			  @Override
			  public void run() {
				  requests.addLast(new AppData(AppData.REQUEST,player+""));
				  send(requests.removeFirst());
				  
			  }
			}, 0, TIME_TICK);
	}
	private void showDialog(String title,final String message,final String button1,final String button2){
		AlertDialog.Builder builder = new AlertDialog.Builder(
				this);
		builder.setTitle(title)
				.setMessage(message)
				.setCancelable(false)
				.setPositiveButton(button1,
						new DialogInterface.OnClickListener() {
							public void onClick(DialogInterface dialog,
									int id) {
								if (button1.equals("No")) ;
								else if (button2!=null){
									Toast.makeText(getApplicationContext(),button1,Toast.LENGTH_SHORT).show();
									player=1;
									loop();
								}else send(new AppData(AppData.PLAY,player+"lose"));
							}
						});
		if (button2!=null)builder.setNegativeButton(button2,
						new DialogInterface.OnClickListener() {
							public void onClick(DialogInterface dialog,
									int id) {
								if (button1.equals("No")) usePower(message);
								else{
									Toast.makeText(getApplicationContext(),button2,Toast.LENGTH_SHORT).show();
									player=0;
									loop();
								}
							}
						});
		builder.show();
	}
	private void checkTTS(){
		Log.e("Chech","tts");
		Intent check = new Intent();
		check.setAction(TextToSpeech.Engine.ACTION_CHECK_TTS_DATA);
		startActivityForResult(check, CHECK_CODE);
	}
	
	public void send(final AppData ap){
		new Thread(new Runnable() {
			@Override
			public void run() {
				AppData res=client.startConversation(ap);
				if (res==null) return;
				byte code=res.getOPCode();
				String data=res.getText();
				if (code==AppData.POWER_ADD){
					String [] array=data.split(">");
					for (String s : array){
						listPower.add(s);
						showImage(s,true);
					}
				}else if (code==AppData.POWER_REMOVE){
					String [] array=data.split(">");
					for (String s : array) {
						listPower.remove(s);
						showImage(s,false);
					}
				}else if (code==AppData.VIBR){
					Vibrator v = (Vibrator) context.getSystemService(Context.VIBRATOR_SERVICE);
					v.vibrate(VIBRATION_TIME);
				}else if (code==AppData.LOSE){
					speaker.speak("Start again");
					runOnUiThread(new Runnable() {
						@Override
						public void run() {
							showDialog("New match","Please press the START button","START",null);
						}
					});
					
				}
			}
		}).start();
	}
	private void showImage(final String text,final boolean show){
		runOnUiThread(new Runnable() {
			@Override
			public void run() {
				int value= show ? View.VISIBLE : View.INVISIBLE;
				switch(text){
				case "enlarge": findViewById(R.id.power_enlarge).setVisibility(value); break;
				case "smaller": findViewById(R.id.power_smaller).setVisibility(value); break;
				case "freeze": findViewById(R.id.power_freeze).setVisibility(value); break;
				case "type": 
					View view=findViewById(R.id.power_type);
					view.setVisibility(value); 
					if (show) registerForContextMenu(view);
					else unregisterForContextMenu(view);
					break;
				case "tangible": findViewById(R.id.power_tangible).setVisibility(value); break;
				case "wall": findViewById(R.id.power_wall).setVisibility(value); break;
				}
				
			}
		});
		
	}
	@Override
	   public void onCreateContextMenu(ContextMenu menu, View v, ContextMenuInfo menuInfo) {
	      super.onCreateContextMenu(menu, v, menuInfo);
	      menu.setHeaderTitle("Colors");
	      menu.add(0, v.getId(), 0, "red");
	      menu.add(0, v.getId(), 0, "blue");
	      menu.add(0, v.getId(), 0, "yellow");
	      menu.add(0, v.getId(), 0, "green");
	      menu.add(0, v.getId(), 0, "white");
	   }
	 
	   @Override
	   public boolean onContextItemSelected(MenuItem item) {
		  String name=item.getTitle().toString();
	      if (name !=null) {
	    	  usePower("type>"+name);
	      } else return false;
	      return true;
	   }
	   
	public boolean usePower(String text){
		Log.e("UsePower",text);
		if (listPower.contains(text)){
			listPower.remove(text);
			showImage(text,false);
			send(new AppData(AppData.POWER_ADD,player+text));
			return true;
		} else if (text.startsWith("type>")&&listPower.contains("type")){
			String color=text.split(">")[1];
				Toast.makeText(context, "You have chosen color "+color,
					Toast.LENGTH_SHORT).show();
			listPower.remove("type");
			showImage("type",false);
			send(new AppData(AppData.PTYPE,player+color));
			return true;
		}
		else return false;
	}
	
	public void handleText(ArrayList<String> texts){
		String text = null;
		int indexTex=0;
		int indexPat=0;
		boolean fin=false;
		text=texts.get(0);
		if (text.contains("quit")||text.contains("exit")){
			lastPat=-1;
			speaker.speak("Do you want to do something?");
			return;
		}
		if (lastPat>=0){
			text=texts.get(0);
			text=text.toLowerCase();

			switch(lastPat){
			case 1:
				num=null;
				while(indexTex<texts.size()){
					text=texts.get(indexTex);
					Pattern p=Pattern.compile(patterns[lastPat]);
					Matcher m=p.matcher(text);
					if (m.find()){
						num=m.group(1);
						Log.e("RecienMatch",num);
						lastPat=-1;
						boolean available=usePower(num);
						if (!available) speaker.speak("You do not have that power-up");
						else speaker.speak("Using the power up"+num);
						break;
					}
					indexTex++;
				}
				if (num==null) speaker.speak("This power up does not exist");
				break;
			case 2:
				send(new AppData(AppData.PLAY,player+""));
				break;
			case 3:
				send(new AppData(AppData.PAUSE,player+""));
				break;
			case 4:
				if (text.contains("pause")){
					speaker.speak("You can say pause or play whenever you want");
				}else if (text.contains("power")){ 
					speaker.speak("The format use power and the name of the power");
				} else{
					speaker.speak("No help available for that");
				}
				lastPat=-1;
				break;
			}
		}
		else while((indexTex<texts.size())&&(!fin)){
			text=texts.get(indexTex);
			text=text.toLowerCase();
			Log.e("Text",text);
			lastPat=-1;
			indexPat=0;
			while((indexPat<patterns.length)&&(!fin)){
				Pattern p=Pattern.compile(patterns[indexPat]);
				Matcher m=p.matcher(text);
				Log.e("Patron",patterns[indexPat]);
				//Match with the pattern
				if(m.find()){
					returnedText.setText(text);
					switch(indexPat){
					case 0:
						text=text.substring(m.end());
						p=Pattern.compile(patterns[indexPat+1]);
						m=p.matcher(text);
						if (m.find()){
							text=text.substring(m.end());
							usePower(text);
						}else{
							speaker.speak("Which power-up do you want to use?");
							lastPat=indexPat+1;
						}
						break;
					case 2:
						send(new AppData(AppData.PLAY,player+""));
						break;
					case 3:
						send(new AppData(AppData.PAUSE,player+""));
						break;
					case 4:
						Log.e("text",text);
						if (text.contains("pause")){
							speaker.speak("You can say pause or play whenever you want");
						}else if (text.contains("power")){ 
							speaker.speak("The format use power and the name of the power");
						} else{
							speaker.speak("For what do you want help?, power-ups, control of the game, play/pause?");
							lastPat=9;
						}
						break;
					case 5:
						speaker.speak(m.group(1)+",Can I help you? Say help and the thing you want");
						lastPat=-2;
						break;
					case 6:
						speaker.speak("I can do a lot, send a message, call someone, set an alarm, read your incoming message. What do you want?");
						lastPat=-2;
						break;
					}
					fin=true;
				}//End Matches
				if (indexPat==0)indexPat+=2;
				else indexPat++;
		    }//End Patterns
			indexTex++;
		}//End Texts
		if (indexTex==texts.size()){
			returnedText.setText(texts.get(0));
		}
		
	}
	//LIFECYCLE
	@Override
	public void onResume() {
		super.onResume();
	}

	@Override
	protected void onPause() {
		super.onPause();
		
	}
	protected void onDestroy() {	
		super.onDestroy();
		speaker.destroy();
		if (speech != null) {
			speech.destroy();
			Log.i(LOG_TAG, "destroy");
		}
	}
	
	//SPEECH METHODS
	@Override
	public void onBeginningOfSpeech() {
		Log.e(LOG_TAG, "onBeginningOfSpeech");
		progressBar.setIndeterminate(false);
		progressBar.setMax(10);
	}

	@Override
	public void onBufferReceived(byte[] buffer) {
		Log.e(LOG_TAG, "onBufferReceived: " + buffer);
	}

	@Override
	public void onEndOfSpeech() {
		Log.e(LOG_TAG, "onEndOfSpeech");
		progressBar.setIndeterminate(true);
		toggleButton.setChecked(false);
	}

	@Override
	public void onError(int errorCode) {
		String errorMessage = getErrorText(errorCode);
		Log.e(LOG_TAG, "FAILED " + errorMessage);
		returnedText.setText(errorMessage);
		toggleButton.setChecked(false);
	}

	@Override
	public void onEvent(int arg0, Bundle arg1) {
		Log.e(LOG_TAG, "onEvent");
	}

	@Override
	public void onPartialResults(Bundle arg0) {
		Log.e(LOG_TAG, "onPartialResults");
	}

	@Override
	public void onReadyForSpeech(Bundle arg0) {
		Log.e(LOG_TAG, "onReadyForSpeech");
	}

	@Override
	public void onResults(Bundle results) {
		Log.e(LOG_TAG, "onResults");
		ArrayList<String> matches = results
				.getStringArrayList(SpeechRecognizer.RESULTS_RECOGNITION);
		String text = "";
		handleText(matches);

	}
	@Override
	public void onRmsChanged(float rmsdB) {
		Log.e(LOG_TAG, "onRmsChanged: " + rmsdB);
		progressBar.setProgress((int) rmsdB);
	}
	
	@Override
	public void onUtteranceCompleted(String utteranceId) {
		Log.e("uitee","num"+lastPat);
		if (lastPat!=-1){
			runOnUiThread(new Runnable() {
				public void run() {
					toggleButton.setChecked(true);
				}
			});
		} else{
				runOnUiThread(new Runnable() {
					public void run() {
						toggleButton.setChecked(false);
						
					}
				});
		}
	}
	
	public static String getErrorText(int errorCode) {
		String message;
		switch (errorCode) {
		case SpeechRecognizer.ERROR_AUDIO:
			message = "Audio recording error";
			break;
		case SpeechRecognizer.ERROR_CLIENT:
			message = "Client side error";
			break;
		case SpeechRecognizer.ERROR_INSUFFICIENT_PERMISSIONS:
			message = "Insufficient permissions";
			break;
		case SpeechRecognizer.ERROR_NETWORK:
			message = "Network error";
			break;
		case SpeechRecognizer.ERROR_NETWORK_TIMEOUT:
			message = "Network timeout";
			break;
		case SpeechRecognizer.ERROR_NO_MATCH:
			message = "No match";
			break;
		case SpeechRecognizer.ERROR_RECOGNIZER_BUSY:
			message = "RecognitionService busy";
			break;
		case SpeechRecognizer.ERROR_SERVER:
			message = "error from server";
			break;
		case SpeechRecognizer.ERROR_SPEECH_TIMEOUT:
			message = "No speech input";
			break;
		default:
			message = "Didn't understand, please try again.";
			break;
		}
		Log.e("ChechERR",message);
		return message;
	}

	 @Override
		protected void onActivityResult(int requestCode, int resultCode, Intent data) {
			if(requestCode == CHECK_CODE){
				if(resultCode == TextToSpeech.Engine.CHECK_VOICE_DATA_PASS){

					speaker = new Speaker(this);
					speaker.allow(true);
					speaker.tts.setOnUtteranceCompletedListener(this);
				}else {
		            Intent install = new Intent();
		            install.setAction(TextToSpeech.Engine.ACTION_INSTALL_TTS_DATA);
		            startActivity(install);
		        }
			}
			if (requestCode == 2) {
				String name;
		        if (data!=null){
		        	name=data.getStringExtra("result");
		        	if (name!=null) usePower(name);
		        }
		        
		    }
		}
		public void mensajeInternet() {
			runOnUiThread(new Runnable() {
				@SuppressWarnings("deprecation")
				public void run() {
					showDialog(TCPClient.DIALOGO_NO_HAY_CONEXION);
					String mensaje = "No hay conexión a Internet";
					Toast.makeText(context, mensaje,
							Toast.LENGTH_SHORT).show();
				}
			});
		}

}