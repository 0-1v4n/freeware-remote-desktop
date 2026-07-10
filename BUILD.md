# Freeware Remote Desktop - Build Instructions

## Prerequisites
- .NET 6.0 SDK or later
- Windows 10/11 (for server screen capture)
- Visual Studio 2022 (optional, for IDE development)

## Build Steps

### 1. Clone the Repository
```bash
git clone https://github.com/0-1v4n/freeware-remote-desktop.git
cd freeware-remote-desktop
```

### 2. Restore Dependencies
```bash
dotnet restore
```

### 3. Build the Solution
```bash
dotnet build RemoteDesktop.sln -c Release
```

### 4. Build Individual Projects
```bash
# Core library
dotnet build RemoteDesktopCore -c Release

# Server
dotnet build RemoteDesktopServer -c Release

# Client
dotnet build RemoteDesktopClient -c Release
```

## Running the Application

### Start Server
```bash
dotnet run --project RemoteDesktopServer --no-build
# or directly
./RemoteDesktopServer/bin/Release/net6.0-windows/RemoteDesktopServer.exe
```

### Start Client
```bash
dotnet run --project RemoteDesktopClient --no-build
# or directly
./RemoteDesktopClient/bin/Release/net6.0-windows/RemoteDesktopClient.exe
```

## Project Structure

```
RemoteDesktop.sln
├── RemoteDesktopCore/
│   ├── RemoteDesktopCore.csproj
│   ├── ScreenCapture.cs
│   ├── ImageCompression.cs
│   ├── NetworkProtocol.cs
│   └── InputSimulator.cs
├── RemoteDesktopServer/
│   ├── RemoteDesktopServer.csproj
│   └── Program.cs
├── RemoteDesktopClient/
│   ├── RemoteDesktopClient.csproj
│   ├── App.xaml
│   ├── App.xaml.cs
│   ├── MainWindow.xaml
│   └── MainWindow.xaml.cs
└── README.md
```

## Configuration

Default settings:
- Server Port: 5555
- Password: 1234
- Capture Interval: 50ms
- JPEG Quality: 75%

Modify these values in the respective source files.

## Troubleshooting

### "Unable to connect to server"
- Ensure server is running on the correct IP/port
- Check firewall settings
- Verify password is correct

### "Screen capture failed"
- Ensure Windows is running the server
- Check display driver compatibility
- Try running as Administrator

### "Poor image quality"
- Increase JPEG compression quality in code
- Reduce capture interval for smoother playback
- Check network bandwidth

## Development

To modify the protocol or add features:
1. Edit `NetworkProtocol.cs` for protocol changes
2. Edit `ScreenCapture.cs` for capture improvements
3. Edit `InputSimulator.cs` for input handling
4. Update Server's `Program.cs` for server-side logic
5. Update Client's `MainWindow.xaml.cs` for UI updates

## Testing

Run unit tests (when available):
```bash
dotnet test
```

## Publishing

Create self-contained executable:
```bash
dotnet publish RemoteDesktopServer -c Release -r win-x64 --self-contained
dotnet publish RemoteDesktopClient -c Release -r win-x64 --self-contained
```

## Performance Tips

- Run server on same LAN for best performance
- Adjust capture interval for network conditions
- Reduce JPEG quality for slower connections
- Use dedicated network connection if possible

## Contributing

Feel free to submit issues and enhancement requests!

## License

MIT License
