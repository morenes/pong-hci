package ttui.marcelo.siri;

import java.io.IOException;
import java.io.InputStream;
import java.nio.ByteBuffer;

import android.util.Log;


public class AppData {
	public static final byte BAD = 0;
	public static final byte OK = 1;
	public static final byte REQUEST = 5;
	public static final byte POWER_ADD = 10;
	public static final byte POWER_REMOVE = 11;
	public static final byte VIBR=12;
	public static final byte LOSE=13;
	public static final byte SUR=14;
	public static final byte PLAY = 15;
	public static final byte PAUSE = 16;
	public static final byte PTYPE= 20;

	private byte OPCode;
	private String text;


	public static AppData toAppData(InputStream conn) throws IOException {
		// Opcode
		byte[] tmp = new byte[1];
		readn(conn, tmp);
		byte opcode = tmp[0];

		// Filename length & filename
		tmp = new byte[1];
		readn(conn, tmp);
		byte text_length = tmp[0];
		byte[] texto = new byte[text_length];
		if (text_length != 0)
			readn(conn, texto);

		return new AppData(opcode, new String(texto));
	}
	
	public static int readn(InputStream is, byte[] buf) throws IOException
	{
		int read = 0;
		int remaining = buf.length;
		while (remaining > 0)
		{
			byte[] tmp = new byte[remaining];
			int actuallyread = is.read(tmp);

			if (actuallyread > 0)
			{
				System.arraycopy(tmp, 0, buf, read, actuallyread);

				read += actuallyread;
				remaining -= actuallyread;
			}
		}

		return read;
	}
	
	public static byte[] toByteArray(AppData ad) {
		int longitudBufferGenerar =ad.getText_length()+2;
		ByteBuffer bb = ByteBuffer.allocate(longitudBufferGenerar);
		bb.put(ad.getOPCode());
		bb.put(ad.getText_length());
		bb.put(ad.getText().getBytes());

		return bb.array();
	}

	/**
	 * @param oPCode
	 * @param text
	 */
	public AppData(byte oPCode, String text) {

		OPCode = oPCode;
		this.text = text;
	}

	public final byte getOPCode() {
		return OPCode;
	}

	public final void setOPCode(byte oPCode) {
		OPCode = oPCode;
	}

	public final byte getText_length() {
		if (text == null)
			return 0;
		return (byte) (text.length());
	}

	public final String getText() {
		return text;
	}

	public final void setText(String text) {
		this.text = text;
	}
}
