using System;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using RemoteDesktopCore;

namespace RemoteDesktopClient
{
    public partial class MainWindow : Window
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private bool _connected = false;
        private bool _mouseDown = false;
        private byte _mouseButton = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (_connected)
            {
                Disconnect();
                return;
            }

            string serverIP = ServerIPTextBox.Text;
            if (!int.TryParse(PortTextBox.Text, out int port))
            {
                StatusText.Text = "Status: Invalid port number";
                return;
            }

            string password = PasswordBox.Password;

            ConnectButton.IsEnabled = false;
            StatusText.Text = "Status: Connecting...";

            Task.Run(() => ConnectToServer(serverIP, port, password));
        }

        private void ConnectToServer(string serverIP, int port, string password)
        {
            try
            {
                _client = new TcpClient();
                _client.Connect(serverIP, port);
                _stream = _client.GetStream();

                // Send authentication
                byte[] authMessage = NetworkProtocol.BuildAuthenticationMessage(password);
                _stream.Write(authMessage, 0, authMessage.Length);
                _stream.Flush();

                // Wait for first frame to confirm authentication
                byte[] buffer = new byte[1024 * 1024];
                int bytesRead = _stream.Read(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        StatusText.Text = "Status: Authentication failed";
                        ConnectButton.IsEnabled = true;
                    });
                    return;
                }

                _connected = true;
                Dispatcher.Invoke(() =>
                {
                    StatusText.Text = "Status: Connected";
                    ConnectButton.Content = "Disconnect";
                    ConnectButton.IsEnabled = true;
                    ScreenImage.Focus();
                });

                // Process first frame
                ProcessScreenData(buffer, bytesRead);

                // Receive loop
                while (_connected)
                {
                    bytesRead = _stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                        break;

                    ProcessScreenData(buffer, bytesRead);
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    StatusText.Text = $"Status: Error - {ex.Message}";
                    ConnectButton.IsEnabled = true;
                    ConnectButton.Content = "Connect";
                });
            }
            finally
            {
                _connected = false;
                _stream?.Dispose();
                _client?.Close();

                Dispatcher.Invoke(() =>
                {
                    StatusText.Text = "Status: Disconnected";
                    ConnectButton.Content = "Connect";
                    ConnectButton.IsEnabled = true;
                });
            }
        }

        private void ProcessScreenData(byte[] buffer, int length)
        {
            try
            {
                if (buffer[0] != NetworkProtocol.MSG_SCREEN_DATA)
                    return;

                int width = BitConverter.ToInt32(buffer, 1);
                int height = BitConverter.ToInt32(buffer, 5);
                int quality = buffer[9];
                int imageLength = BitConverter.ToInt32(buffer, 10);

                byte[] imageData = new byte[imageLength];
                Array.Copy(buffer, 14, imageData, 0, imageLength);

                Bitmap bitmap = ImageCompression.DecompressImageFromJpeg(imageData);

                Dispatcher.Invoke(() =>
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = new MemoryStream(imageData);
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();

                    ScreenImage.Source = bitmapImage;
                });

                bitmap?.Dispose();
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    StatusText.Text = $"Status: Frame error - {ex.Message}";
                });
            }
        }

        private void Disconnect()
        {
            _connected = false;
            try
            {
                if (_stream != null)
                {
                    byte[] disconnectMessage = new byte[] { NetworkProtocol.MSG_DISCONNECT };
                    _stream.Write(disconnectMessage, 0, 1);
                    _stream.Flush();
                }
            }
            catch { }
        }

        private void ScreenImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_connected)
                return;

            Point pos = e.GetPosition(ScreenImage);
            int x = (int)pos.X;
            int y = (int)pos.Y;

            try
            {
                byte[] message = NetworkProtocol.BuildMouseMoveMessage(x, y);
                _stream.Write(message, 0, message.Length);
                _stream.Flush();
            }
            catch { }
        }

        private void ScreenImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_connected)
                return;

            _mouseDown = true;
            _mouseButton = e.ChangedButton == MouseButton.Left ? 
                NetworkProtocol.MOUSE_BUTTON_LEFT : NetworkProtocol.MOUSE_BUTTON_RIGHT;

            try
            {
                byte[] message = NetworkProtocol.BuildMouseClickMessage(_mouseButton);
                _stream.Write(message, 0, message.Length);
                _stream.Flush();
            }
            catch { }
        }

        private void ScreenImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _mouseDown = false;
        }

        private void ScreenImage_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Scroll wheel support can be added here
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!_connected)
                return;

            byte keyCode = (byte)KeyInterop.VirtualKeyFromKey(e.Key);

            try
            {
                byte[] message = NetworkProtocol.BuildKeyPressMessage(keyCode);
                _stream.Write(message, 0, message.Length);
                _stream.Flush();
            }
            catch { }

            e.Handled = true;
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnClosed(EventArgs e)
        {
            Disconnect();
            base.OnClosed(e);
        }
    }
}
