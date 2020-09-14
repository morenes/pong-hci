package ttui.marcelo.siri;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.InetAddress;
import java.net.Socket;

import android.content.Context;
import android.net.ConnectivityManager;
import android.net.NetworkInfo;
import android.net.wifi.WifiManager;
import android.text.format.Formatter;
import android.util.Log;

public class TCPClient {

	private static final int PORT = 8009;
	public static final int DIALOGO_NO_HAY_CONEXION =1;

	public ConnectivityManager connec;
	
	public final static String DIR_IP="192.168.137.1";
	public Context context;
	public WifiManager wifi;
	
	public TCPClient(Context context){
		this.context=context;
	}
//wa1yg5H7QJ
	
	public AppData startConversation(AppData ad) {
		Socket socket = null;
		AppData res=null;

		if (context==null){
			Log.e("TCP","no context");
			return null;
		}
		if(!checkConex()){
    		Log.e("TCP","no conexion");
			return null;
		}
		Log.e("Hasta","Aqui");
		try {
			socket = new Socket(InetAddress.getByName(DIR_IP), PORT);
			OutputStream os = socket.getOutputStream();
			Log.e("REQ","CODE: "+ad.getOPCode()+" DATA:"+ad.getText());
			os.write(AppData.toByteArray(ad));
			InputStream is = socket.getInputStream();
			
			AppData recibir = AppData.toAppData(is);
			if (new String(recibir.getText()).equals("")) res=null;
			else res=recibir;
			if (res!=null) Log.e("RESPONSE","CODE: "+res.getOPCode()+" DATA:"+res.getText());
			else Log.e("RESPONSE","NULL");
		} catch (IOException e) {
			System.err.println("¡La comunicacion no se pudo realizar correctamente!");
			e.printStackTrace();
			return null;
		}
		finally {
			try {
				if (socket!=null) socket.close();
			} catch (IOException e) {
				e.printStackTrace();
			}
		}
		return res;
	}
	public boolean checkConex(){
        boolean hayConexion = false;      
        NetworkInfo redwifi = connec.getNetworkInfo(ConnectivityManager.TYPE_WIFI);
        NetworkInfo reddata = connec.getNetworkInfo(ConnectivityManager.TYPE_MOBILE);
        WifiManager wm = wifi;
        String ip = Formatter.formatIpAddress(wm.getConnectionInfo().getIpAddress());
           
        //Si hay alguna red conectada, entonces hay internet
        if(redwifi.isConnected()||reddata.isConnected()) {
        	hayConexion = true;
        	Log.e("La wifi es: ",ip);
        }
        return hayConexion;
	}
	
}
