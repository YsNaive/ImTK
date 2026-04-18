using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ImTK;

namespace dashboard.Dashboard.Communication
{
    public class ConnectionWindow : WindowView
    {
        public override string displayName => "Connection";

        private readonly IntField _portField;
        private readonly IntField _socketIdField;
        private readonly TextElement _statusText;

        public static WebSocketService? CurrentService { get; private set; }

        [MainMenu("視窗/主機連線")]
        public static void OpenWindow()
        {
            WindowView.Open<ConnectionWindow>();
        }

        public ConnectionWindow()
        {
            minSize = new System.Numerics.Vector2(300, 150);

            CurrentService = new WebSocketService(OnDataReceived);

            var cachedConfig = Core.CacheHandler.LoadConnectionConfig();

            _portField = new IntField("Port", cachedConfig.Port);
            _socketIdField = new IntField("Socket ID", cachedConfig.SocketId);
            _statusText = new TextElement("Disconnected");

            CurrentService.OnStatusChanged += (status) =>
            {
                _statusText.text = status;
            };

            _portField.RegisterValueChanged(Reconnect);
            _socketIdField.RegisterValueChanged(Reconnect);

            this.Add(_portField);
            this.Add(_socketIdField);
            this.Add(_statusText);

            // Auto connect on startup with cached values
            Reconnect();
        }

        private void OnDataReceived(List<byte> buffer)
        {
            Protocol.PacketParser.ProcessBuffer(buffer);
        }

        private void Reconnect()
        {
            if (CurrentService == null) return;

            Core.CacheHandler.SaveConnectionConfig(_portField.value, _socketIdField.value);

            _ = Task.Run(async () =>
            {
                await CurrentService.ConnectAsync(_portField.value, _socketIdField.value);
            });
        }

        public override void Close()
        {
            base.Close();
            _ = Task.Run(async () =>
            {
                if (CurrentService != null)
                {
                    await CurrentService.DisconnectAsync();
                }
            });
        }
    }
}