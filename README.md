# Freeware Remote Desktop App

A free, open-source remote desktop application for Windows, similar to AnyDesk but completely free and customizable.

## Features

- Remote screen viewing and control
- Mouse and keyboard input over network
- Efficient screen capture and compression
- Secure authentication
- Cross-machine connectivity
- Lightweight and fast

## Quick Start

### Prerequisites
- Windows 10 or later
- .NET 6.0 or later

### Building
```bash
dotnet build RemoteDesktop.sln
```

### Running

**Terminal 1 - Start Server:**
```bash
dotnet run --project RemoteDesktopServer
```

**Terminal 2 - Start Client:**
```bash
dotnet run --project RemoteDesktopClient
```

**In Client GUI:**
- Server IP: `localhost`
- Port: `5555`
- Password: `1234`
- Click Connect

## Architecture

- **RemoteDesktopServer**: Host application that captures screen and accepts connections
- **RemoteDesktopClient**: Client GUI application to view and control remote screen
- **RemoteDesktopCore**: Shared library with screen capture, input simulation, compression

## Technology Stack
- C# .NET 6.0
- WPF (Windows Presentation Foundation) for UI
- Windows API for screen capture
- TCP sockets for networking
- JPEG compression for efficiency

## License

MIT License
