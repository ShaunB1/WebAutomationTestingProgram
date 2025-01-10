using System.Net.WebSockets;
using System.Text;

namespace AutomationTestingProgram.Actions;

public class LogStream
{
    private static readonly MemoryStream _logStream = new MemoryStream();
    private static readonly StreamWriter _logWriter = new StreamWriter(_logStream){ AutoFlush = true };

    public static async Task Execute(WebSocket webSocket)
    {
        Console.SetOut(_logWriter);

        var buffer = new byte[1024 * 4];
        
        _logStream.Position = 0;

        while (webSocket.State == WebSocketState.Open)
        {
            if (_logStream.Length > _logStream.Position)
            {
                var newLogLength = _logStream.Length - _logStream.Position;
                var logBytes = new byte[newLogLength];
                _logStream.Read(logBytes, 0, (int)newLogLength);
                
                var logMsg = Encoding.UTF8.GetString(logBytes);
                var bytesToSend = Encoding.UTF8.GetBytes(logMsg);
                
                await webSocket.SendAsync(new ArraySegment<byte>(bytesToSend), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}