using System.Text;
using System.Net.Sockets;

public class SocketState
{
    public const int BufferSize = 1024;

    private byte[] _buffer = new byte[BufferSize];
    public byte[] Buffer
    {
        get { return _buffer; }
    }

    private StringBuilder _receivedText = new StringBuilder();
    public StringBuilder ReceivedText
    {
        get { return _receivedText; }
    }

    public Socket? WorkSocket { get; set; }
}