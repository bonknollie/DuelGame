# DuelGame - Dedicated Server

## Building the Server

### Windows Server Build
```powershell
# From Unity Editor: File → Build Settings
# 1. Select "Windows" platform
# 2. Check "Server Build" option
# 3. Build to "Builds/Server/"
```

### Linux Server Build
```bash
# From Unity Editor: File → Build Settings
# 1. Select "Linux Dedicated Server" platform
# 2. Build to "Builds/LinuxServer/"
```

## Running the Server

### Windows
```powershell
.\DuelGame.exe -batchmode -nographics -logFile server.log
```

### Linux
```bash
./DuelGame.x86_64 -batchmode -nographics -logFile server.log
```

## Server Configuration

The `ServerLauncher` component automatically starts the server when running in headless mode (no graphics).

- **Port:** Default 7777 (configured in NetworkManager)
- **Max Players:** 2 (1v1 duel)
- **Transport:** Unity Transport (UDP)

## Client Connection

Clients connect using the IP address of the server:
1. Launch client build
2. Click "Start Client" 
3. Default connects to 127.0.0.1:7777 (localhost)
4. Edit NetworkManager component to change IP/port

## Logs

Server logs are written to:
- Windows: `server.log` in executable directory
- Linux: `server.log` in executable directory

## Notes

- Add `ServerLauncher` component to a GameObject in your scene
- Ensure NetworkManager is in the scene
- For hosting over internet, configure port forwarding (UDP 7777)
