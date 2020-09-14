using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;


public class TextEventArgs : EventArgs
{
    public TextEventArgs(AppData ad)
    {
        this.ad = ad;
    }

    public AppData ad { get; protected set; }
}


public class SynSocketListener
{
    private const int PORT = 8009;
    // Incoming data from the client.
    public event EventHandler rec;

    public void StartListening()
    {
        // Data buffer for incoming data.
        byte[] bytes = new Byte[1024];

        // Establish the local endpoint for the socket.
        // Dns.GetHostName returns the name of the 
        // host running the application.
        IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
        IPAddress ipAddress = ipHostInfo.AddressList[0];
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, PORT);
        System.Diagnostics.Debug.WriteLine(ipAddress);

        // Create a TCP/IP socket.
        Socket listener = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);

        // Bind the socket to the local endpoint and 
        // listen for incoming connections.
        try
        {
            listener.Bind(localEndPoint);
            listener.Listen(10);

            // Start listening for connections.
            while (true)
            {
                System.Diagnostics.Debug.WriteLine("Waiting for a connection...");
                // Program is suspended while waiting for an incoming connection.
                Socket handler = listener.Accept();
                bytes = new byte[1024];
                int bytesRec = handler.Receive(bytes);
                AppData ap = AppData.toAppData(bytes);
                System.Diagnostics.Debug.WriteLine("REQ->CODE: " + ap.getOPCode() + " DATA: " + ap.getText());
                TextEventArgs e = new TextEventArgs(ap);
                
                if (rec != null) 
                    rec(this,e);

                // Echo the data back to the client.
                System.Diagnostics.Debug.WriteLine("RESPONSE->CODE: " + ap.getOPCode() + " DATA: " + ap.getText());
                handler.Send(AppData.toByteArray(ap));
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }

        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.WriteLine(e.ToString());
        }

        Console.WriteLine("\nPress ENTER to continue...");
        Console.Read();
    }


}

public class AppData {
	public  const byte BAD = 0;
    public  const byte OK = 1;
    public  const byte REQUEST = 5;
    public  const byte POWER_ADD = 10;
    public  const byte POWER_REMOVE = 11;
    public  const byte VIBR = 12;
    public  const byte LOSE = 13;
    public const byte SUR = 14;
    public  const byte PLAY = 15;
    public  const byte PAUSE = 16;
    public  const byte PTYPE = 20;


	private byte OPCode;
	private String text;


	public static AppData toAppData(byte[] array){
		byte opcode = array[0];
		int l=array[1];
        String s= Encoding.ASCII.GetString(array, 2, l);
		return new AppData(opcode,s);
	}
	
	public static byte[] toByteArray(AppData ad) {
		int l =ad.Text_length() +2;
        byte[] bytes = new byte[l];
		bytes[0]=ad.getOPCode();
		bytes[1]=ad.Text_length();
        byte[] msg = Encoding.ASCII.GetBytes(ad.getText());
        for (int i = 0; i < msg.Length; i++)
            bytes[2 + i] = msg[i];
        return bytes;
	}

    public AppData(byte oPCode, String text)
    {

		OPCode = oPCode;
		this.text = text;
	}

	public  byte getOPCode() {
		return OPCode;
	}

	public  void setOPCode(byte oPCode) {
		OPCode = oPCode;
	}

	public  byte Text_length() {
		if (text == null)
			return 0;
		return (byte) (text.Length);
	}

	public  String getText() {
		return text;
	}

	public  void setText(String text) {
		this.text = text;
	}
}