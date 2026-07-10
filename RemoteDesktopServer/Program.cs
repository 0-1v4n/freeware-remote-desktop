using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using RemoteDesktopCore;

namespace RemoteDesktopServer
{
    class Program
    {
        private static TcpListener _tcpListener;
        private const int PORT = 5555;
        private const string PASSWORD = "1234";
        private const int CAPTURE_INTERVAL = 50;
        private const int COMPRESSION_QUALITY = 75;
        private static bool _running = true;

        static void Main(string[] args)
        {
            Console.WriteLine("=== Remote Desktop Server ===");
            Console.WriteLine($"Starting server on port {PORT}...");
            Console.WriteLine($"Password: {PASSWORD}");
            var (width, height) = ScreenCapture.GetScreenDimensions();
            Console.WriteLine($"Screen dimensions: {width}x{height}");

            try
            {
                _tcpListener = new TcpListener(IPAddress.Any, PORT);
                _tcpListener.Start();
                Console.WriteLine($"✓ Server listening on port {PORT}");
                Console.WriteLine("Waiting for connections...");
                Console.WriteLine("Press Ctrl+C to stop server\n");

                while (_running)
                {
                    if (_tcpListener.Pending())
                    {
                        TcpClient client = _tcpListener.AcceptTcpClient();
                        string clientIp = ((IPEndPoint)client.Client.RemoteEndPoint)?.Address.ToString() ?? "Unknown";
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Client connected: {clientIp}");

                        ClientHandler handler = new ClientHandler(client, PASSWORD, CAPTURE_INTERVAL, COMPRESSION_QUALITY);
                        Task.Run(() => HandleClient(handler));
                    }
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                _running = false;
                _tcpListener?.Stop();
                Console.WriteLine("\nServer stopped.");
            }
        }

        static void HandleClient(ClientHandler handler)
        {
            try
            {
                handler.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Client error: {ex.Message}");
            }
            finally
            {
                handler.Dispose();
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Client disconnected");
            }
        }
    }

    class ClientHandler : IDisposable
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private string _password;
        private int _captureInterval;
        private int _compressionQuality;
        private bool _authenticated = false;
        private int _framesSent = 0;

        public ClientHandler(TcpClient client, string password, int captureInterval, int compressionQuality)
        {
            _client = client;
            _password = password;
            _captureInterval = captureInterval;
            _compressionQuality = compressionQuality;
        }

        public void Start()
        {
            using (_stream = _client.GetStream())
            {
                if (!AuthenticateClient())
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Authentication failed");
                    return;
                }

                _authenticated = true;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ✓ Client authenticated");

                var (width, height) = ScreenCapture.GetScreenDimensions();
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Screen size: {width}x{height}");

                while (_client.Connected)
                {
                    try
                    {
                        Bitmap screenshot = ScreenCapture.CaptureScreen();
                        if (screenshot != null)
                        {
                            byte[] compressedData = ImageCompression.CompressImageToJpeg(screenshot, _compressionQuality);
                            byte[] message = NetworkProtocol.BuildScreenDataMessage(compressedData, screenshot.Width, screenshot.Height, _compressionQuality);

                            _stream.Write(message, 0, message.Length);
                            _stream.Flush();

                            _framesSent++;
                            if (_framesSent % 30 == 0)
                            {
                                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Frames sent: {_framesSent}, Data size: {compressedData.Length} bytes");
                            }

                            screenshot.Dispose();
                        }

                        if (_stream.DataAvailable)
                        {
                            ProcessClientCommand();
                        }

                        Thread.Sleep(_captureInterval);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Frame transmission error: {ex.Message}");
                        break;
                    }
                }
            }
        }

        private bool AuthenticateClient()
        {
            try
            {
                byte[] buffer = new byte[1024];
                int bytesRead = _stream.Read(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                    return false;

                if (buffer[0] != NetworkProtocol.MSG_AUTHENTICATE)
                    return false;

                int passwordLength = BitConverter.ToInt32(buffer, 1);
                string receivedPassword = System.Text.Encoding.UTF8.GetString(buffer, 5, passwordLength);

                return receivedPassword == _password;
            }
            catch
            {
                return false;
            }
        }

        private void ProcessClientCommand()
        {
            try
            {
                byte[] buffer = new byte[1024];
                int bytesRead = _stream.Read(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                    return;

                byte messageType = buffer[0];

                switch (messageType)
                {
                    case NetworkProtocol.MSG_MOUSE_MOVE:
                        int x = BitConverter.ToInt32(buffer, 1);
                        int y = BitConverter.ToInt32(buffer, 5);
                        InputSimulator.MoveMouse(x, y);
                        break;

                    case NetworkProtocol.MSG_MOUSE_CLICK:
                        byte button = buffer[1];
                        if (button == NetworkProtocol.MOUSE_BUTTON_LEFT)
                            InputSimulator.LeftClick();
                        else if (button == NetworkProtocol.MOUSE_BUTTON_RIGHT)
                            InputSimulator.RightClick();
                        break;

                    case NetworkProtocol.MSG_KEY_PRESS:
                        byte keyCode = buffer[1];
                        InputSimulator.PressKey(keyCode);
                        break;

                    case NetworkProtocol.MSG_DISCONNECT:
                        _client.Close();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Command processing error: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _stream?.Dispose();
            _client?.Close();
            _client?.Dispose();
        }
    }
}
