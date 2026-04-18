using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace dashboard.Dashboard.Communication
{
    public class WebSocketService
    {
        private ClientWebSocket? _webSocket;
        private CancellationTokenSource? _cancellationTokenSource;
        private readonly List<byte> _buffer = new List<byte>();
        private readonly Action<List<byte>> _onDataReceived;

        public event Action<string>? OnStatusChanged;

        public string Status { get; private set; } = "Disconnected";

        public WebSocketService(Action<List<byte>> onDataReceived)
        {
            _onDataReceived = onDataReceived;
        }

        private void SetStatus(string status)
        {
            Status = status;
            OnStatusChanged?.Invoke(status);
        }

        public async Task ConnectAsync(int port, int socketId)
        {
            await DisconnectAsync();

            _webSocket = new ClientWebSocket();
            _cancellationTokenSource = new CancellationTokenSource();

            string uriString = $"ws://localhost:{port}/vexrobotics.vexcode/device?id={socketId}";
            // Console.WriteLine($"[WebSocket] Attempting connection to: {uriString}"); // TODO: Integrate into Debug Log system
            SetStatus($"Connecting to {uriString}...");

            try
            {
                await _webSocket.ConnectAsync(new Uri(uriString), _cancellationTokenSource.Token);
                // Console.WriteLine("[WebSocket] Connection established successfully."); // TODO: Integrate into Debug Log system
                SetStatus("Connected");

                // Start background receive loop
                _ = ReceiveLoopAsync();
            }
            catch (Exception ex)
            {
                SetStatus($"Connection failed: {ex.Message}");
            }
        }

        public async Task DisconnectAsync()
        {
            if (_webSocket != null)
            {
                if (_webSocket.State == WebSocketState.Open || _webSocket.State == WebSocketState.Connecting)
                {
                    try
                    {
                        if (_cancellationTokenSource != null)
                        {
                            _cancellationTokenSource.Cancel();
                        }

                        _webSocket.Abort();
                    }
                    catch { }
                }

                _webSocket.Dispose();
                _webSocket = null;
            }

            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

            _buffer.Clear();
            SetStatus("Disconnected");
        }

        private async Task ReceiveLoopAsync()
        {
            if (_webSocket == null || _cancellationTokenSource == null) return;

            var rcvBuffer = new byte[1024];

            try
            {
                while (_webSocket.State == WebSocketState.Open && !_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(rcvBuffer), _cancellationTokenSource.Token);

                    // Console.WriteLine($"[WebSocket] Received {result.Count} bytes. Type: {result.MessageType}, EndOfMessage: {result.EndOfMessage}"); // TODO: Integrate into Debug Log system

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await DisconnectAsync();
                        SetStatus("Connection closed by server.");
                        break;
                    }
                    else if (result.MessageType == WebSocketMessageType.Binary || result.MessageType == WebSocketMessageType.Text)
                    {
                        for (int i = 0; i < result.Count; i++)
                        {
                            _buffer.Add(rcvBuffer[i]);
                        }

                        if (_buffer.Count > 0)
                        {
                            _onDataReceived(_buffer);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                SetStatus($"Receive error: {ex.Message}");
                await DisconnectAsync();
            }
        }
    }
}