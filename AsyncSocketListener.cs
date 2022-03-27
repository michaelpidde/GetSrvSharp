using System.Net;
using System.Net.Sockets;
using System.Text;

public class AsyncSocketListener
{
    public static ManualResetEvent done = new ManualResetEvent(false);

    public static void Start(Config config)
    {
        IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ipAddress = ipHostInfo.AddressList[0];
        var localEndpoint = new IPEndPoint(ipAddress, 6660);
        var socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            socket.Bind(localEndpoint);
            socket.Listen(100);

            while(true)
            {
                done.Reset();
                socket.BeginAccept(new AsyncCallback(AcceptCallback), socket);
                // Block the current thread until a signal is received
                done.WaitOne();
            }
        } 
        catch(Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    public static void AcceptCallback(IAsyncResult result)
    {
        // Signal the main thread to continue
        done.Set();

        var socket = (Socket)result.AsyncState;
        Socket handler = socket.EndAccept(result);

        var state = new SocketState();
        state.WorkSocket = handler;
        handler.BeginReceive(
            state.Buffer, 0, SocketState.BufferSize, SocketFlags.None, new AsyncCallback(ReadCallback), state);
    }

    public static void ReadCallback(IAsyncResult result)
    {
        var sendContent = String.Empty;

        // Get state object and handler socket from the async state object
        var state = (SocketState)result.AsyncState;
        Socket handler = state.WorkSocket;

        int bytesRead = handler.EndReceive(result);
        if (bytesRead > 0)
        {
            state.ReceivedText.Append(Encoding.ASCII.GetString(state.Buffer, 0, bytesRead));

            var requestHandler = new RequestHandler(Log, state.ReceivedText.ToString());

            sendContent += requestHandler.GetResponse();
            Send(handler, sendContent);
        }
    }

    public static void Send(Socket handler, string content)
    {
        byte[] sendBytes = Encoding.ASCII.GetBytes(content);
        handler.BeginSend(sendBytes, 0, sendBytes.Length, 0, new AsyncCallback(SendCallback), handler);
    }

    public static void SendCallback(IAsyncResult result)
    {
        try
        {
            Socket handler = (Socket)result.AsyncState;
            int bytesSent = handler.EndSend(result);
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        } 
        catch(Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    public static void Log(string message)
    {
        Console.WriteLine(message);
    }
}
