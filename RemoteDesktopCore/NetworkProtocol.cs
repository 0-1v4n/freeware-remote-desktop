using System;
using System.IO;

namespace RemoteDesktopCore
{
    /// <summary>
    /// Defines the network protocol messages between client and server
    /// </summary>
    public static class NetworkProtocol
    {
        // Message types
        public const byte MSG_SCREEN_DATA = 0x01;
        public const byte MSG_MOUSE_MOVE = 0x02;
        public const byte MSG_MOUSE_CLICK = 0x03;
        public const byte MSG_KEY_PRESS = 0x04;
        public const byte MSG_AUTHENTICATE = 0x05;
        public const byte MSG_DISCONNECT = 0x06;

        // Mouse button types
        public const byte MOUSE_BUTTON_LEFT = 0x01;
        public const byte MOUSE_BUTTON_RIGHT = 0x02;
        public const byte MOUSE_BUTTON_MIDDLE = 0x03;

        /// <summary>
        /// Builds a screen data message packet
        /// </summary>
        public static byte[] BuildScreenDataMessage(byte[] imageData, int width, int height, int quality)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.WriteByte(MSG_SCREEN_DATA);
                ms.Write(BitConverter.GetBytes(width), 0, 4);
                ms.Write(BitConverter.GetBytes(height), 0, 4);
                ms.WriteByte((byte)quality);
                ms.Write(BitConverter.GetBytes(imageData.Length), 0, 4);
                ms.Write(imageData, 0, imageData.Length);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Builds a mouse move message packet
        /// </summary>
        public static byte[] BuildMouseMoveMessage(int x, int y)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.WriteByte(MSG_MOUSE_MOVE);
                ms.Write(BitConverter.GetBytes(x), 0, 4);
                ms.Write(BitConverter.GetBytes(y), 0, 4);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Builds a mouse click message packet
        /// </summary>
        public static byte[] BuildMouseClickMessage(byte button)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.WriteByte(MSG_MOUSE_CLICK);
                ms.WriteByte(button);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Builds a key press message packet
        /// </summary>
        public static byte[] BuildKeyPressMessage(byte keyCode)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.WriteByte(MSG_KEY_PRESS);
                ms.WriteByte(keyCode);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Builds an authentication message packet
        /// </summary>
        public static byte[] BuildAuthenticationMessage(string password)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.WriteByte(MSG_AUTHENTICATE);
                byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
                ms.Write(BitConverter.GetBytes(passwordBytes.Length), 0, 4);
                ms.Write(passwordBytes, 0, passwordBytes.Length);
                return ms.ToArray();
            }
        }
    }
}
