using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

class AutoPatchServer
{
    static void Main()
    {
        var config = ReadConfig("ServerConfig.ini");
        LogInfo("Load Config Success");

        string latestVersion = config["latest_patch"];
        int port = int.Parse(config["listen_port"]);
        string hostname = config["web_hostname"];
        string path = config["web_path"];
        string webPort = config.ContainsKey("web_port") ? config["web_port"] : "80";
        string resolvedIP = ResolveHostIP(hostname);

        string patchURL = (webPort == "80")
            ? $"http://{hostname}/{path}/{latestVersion}.exe"
            : $"http://{hostname}:{webPort}/{path}/{latestVersion}.exe";

        LogInfo($"Autopatch Server started on port {port}");
        LogInfo($"Server IP/Host : {(hostname == resolvedIP ? resolvedIP : $"{hostname} | {resolvedIP}")}");
        LogInfo($"Latest Version : {latestVersion}");
        LogInfo($"Patch folder : {path}");
        LogInfo($"Client will patch from {patchURL}");

        TcpListener server = new TcpListener(IPAddress.Any, port);
        server.Start();

        while (true)
        {
            using (TcpClient client = server.AcceptTcpClient())
            using (NetworkStream stream = client.GetStream())
            {
                IPEndPoint remoteEP = client.Client.RemoteEndPoint as IPEndPoint;

                byte[] buffer = new byte[1024];
                int read = stream.Read(buffer, 0, buffer.Length);
                if (read <= 0) continue;

                string clientVersion = Encoding.ASCII.GetString(buffer, 0, read).Trim('\0', '\n', '\r', ' ');

                if (!IsValidVersion(clientVersion))
                {
                    LogDrop($"{remoteEP?.Address}:{remoteEP?.Port} invalid version: \"{clientVersion}\"");
                    continue;
                }

                LogClient($"{remoteEP?.Address}:{remoteEP?.Port} version: {clientVersion}");

                if (clientVersion == latestVersion)
                {
                    SendMessage(stream, "READY");
                }
                else
                {
                    string updateHost = (webPort == "80") ? hostname : $"{hostname}:{webPort}";
                    string updateMsg = $"UPDATE {updateHost} {path}/{latestVersion}.exe";
                    SendMessage(stream, updateMsg);
                }
            }
        }
    }

    static void SendMessage(NetworkStream stream, string msg)
    {
        byte[] data = Encoding.ASCII.GetBytes(msg + "\0");
        stream.Write(data, 0, data.Length);
    }

    static bool IsValidVersion(string input)
    {
        if (!Regex.IsMatch(input, @"^\d+$")) return false;
        if (input.Length < 4 || input.Length > 5) return false;
        if (!int.TryParse(input, out int ver)) return false;
        if (ver < 1000 || ver > 99999) return false;

        foreach (char c in input)
        {
            if (c < 32 || c > 126) return false;
        }

        return true;
    }

    static Dictionary<string, string> ReadConfig(string filePath)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (!File.Exists(filePath))
        {
            LogError("Config file not found.");
            Environment.Exit(1);
        }

        foreach (string line in File.ReadAllLines(filePath))
        {
            string trimmed = line.Trim();
            if (trimmed.StartsWith("[") || trimmed.StartsWith(";") || trimmed == "" || trimmed.StartsWith("//"))
                continue;

            string[] parts = trimmed.Split(new char[] { '=' }, 2);
            if (parts.Length == 2)
            {
                dict[parts[0].Trim()] = parts[1].Split(new[] { "//" }, StringSplitOptions.None)[0].Trim();
            }
        }
        return dict;
    }

    static string ResolveHostIP(string host)
    {
        try
        {
            IPAddress[] addresses = Dns.GetHostAddresses(host);
            foreach (var ip in addresses)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();
            }
        }
        catch { }
        return host;
    }

    // === LOGGING ===
    static void LogInfo(string msg) => LogColored("INFO", msg, ConsoleColor.Green);
    static void LogClient(string msg) => LogColored("CLIENT", msg, ConsoleColor.Cyan);
    static void LogDrop(string msg) => LogColored("DROP", msg, ConsoleColor.Red);
    static void LogError(string msg) => LogColored("ERROR", msg, ConsoleColor.Red);

    static void LogColored(string tag, string message, ConsoleColor color)
    {
        string timestamp = $"[{DateTime.Now:HH:mm:ss}]";
        Console.ForegroundColor = color;
        Console.WriteLine($"{timestamp} [{tag}] {message}");
        Console.ResetColor();
    }
}
