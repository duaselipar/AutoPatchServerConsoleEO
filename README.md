# AutoPatchServerConsole

**AutoPatchServerConsole** is a lightweight and secure version handshake server for Eudemons Online private servers.  
It listens for client version requests and tells the client whether to start the game or download the latest patch.

---

## ✨ Features

- TCP-based version handshake on custom port
- Patch info sent via HTTP (custom domain and port supported)
- Smart patch delivery based on version difference (e.g. 1000 → 1001 → so on)
- Strict version validation (only numbers between 1000–99999 are accepted)
- Input sanitization to block malformed/bot packets
- INI configuration support (`ServerConfig.ini`)
- Colored console output with timestamps
- Auto log to daily file (`log/log-YYYY-MM-DD.txt`)
- Banner branding + customizable console title

---

## 🔧 Configuration

Create a file named `ServerConfig.ini`:

```ini
[config]
latest_patch=2102
listen_port=13532
web_hostname=knightfall.servegame.com
web_port=80
web_path=patches
```

---

## 📁 Output Example

```text
[04:09:24] [INFO] Load Config Success
[04:09:24] [INFO] Autopatch Server started on port 13532
[04:09:24] [INFO] Server IP/Host : yourserver.com | 124.33.454.231
[04:09:24] [INFO] Website Port : 8880
[04:09:24] [INFO] Latest Version : 2102
[04:09:24] [INFO] Patch folder : patches
[04:09:24] [INFO] Client will patch from http://yourserver.com:8880/patches/2102.exe
```


---

## 🛡️ License

MIT — free to use and modify.
