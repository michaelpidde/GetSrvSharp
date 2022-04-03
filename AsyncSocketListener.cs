﻿using System.Net;
using System.Net.Sockets;
using System.Text;

public class AsyncSocketListener {
    public ManualResetEvent done = new ManualResetEvent(false);
    private readonly Config _config;

    public AsyncSocketListener(Config config) {
        _config = config;
    }

    public void Start() {
        IPHostEntry? ipHostInfo = null;
        try {
            ipHostInfo = Dns.GetHostEntry(_config.Host);
        } catch(SocketException e) {
            Log($"Error: {e.Message}");
            Environment.Exit(1);
        }

        IPAddress ipAddress = ipHostInfo!.AddressList[0];
        var localEndpoint = new IPEndPoint(ipAddress, _config.Port);
        var socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try {
            socket.Bind(localEndpoint);
            socket.Listen(100);
            Console.WriteLine($"Server started at port {_config.Port}");

            while(true) {
                done.Reset();
                socket.BeginAccept(new AsyncCallback(AcceptCallback), socket);
                // Block the current thread until a signal is received
                done.WaitOne();
            }
        } catch(Exception e) {
            Log(e.ToString());
        }
    }

    public void AcceptCallback(IAsyncResult result) {
        // Signal the main thread to continue
        done.Set();

        var socket = result.AsyncState as Socket;
        Socket handler = socket.EndAccept(result);

        var state = new SocketState();
        state.WorkSocket = handler;
        handler.BeginReceive(
            state.Buffer, 0, SocketState.BufferSize, SocketFlags.None, new AsyncCallback(ReadCallback), state);
    }

    public void ReadCallback(IAsyncResult result) {
        var sendContent = String.Empty;

        // Get state object and handler socket from the async state object
        var state = result.AsyncState as SocketState;
        Socket handler = state.WorkSocket;

        int bytesRead = handler.EndReceive(result);
        if(bytesRead > 0) {
            state.ReceivedText.Append(Encoding.ASCII.GetString(state.Buffer, 0, bytesRead));

            var requestHandler = new RequestHandler(Log, state.ReceivedText.ToString(), _config);

            sendContent += requestHandler.GetResponse();
            Send(handler, sendContent);
        }
    }

    public void Send(Socket handler, string content) {
        byte[] sendBytes = Encoding.ASCII.GetBytes(content);
        handler.BeginSend(sendBytes, 0, sendBytes.Length, 0, new AsyncCallback(SendCallback), handler);
    }

    public void SendCallback(IAsyncResult result) {
        try {
            var handler = result.AsyncState as Socket;
            int bytesSent = handler.EndSend(result);
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        } catch(Exception e) {
            Log(e.ToString());
        }
    }

    public void Log(string message) {
        using StreamWriter file = new(_config.LogFile.FullName, append: true);
        file.WriteLineAsync(message);
    }
}
