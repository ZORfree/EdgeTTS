using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Edge_tts_sharp
{
    public class MessageEventArgs : EventArgs
    {
        public bool IsText { get; set; }
        public bool IsBinary { get; set; }
        public string Data { get; set; }
        public byte[] RawData { get; set; }
    }

    public class CloseEventArgs : EventArgs
    {
        public ushort Code { get; set; }
        public string Reason { get; set; }
        public bool WasClean { get; set; }
    }

    public class Log
    {
        public level level;
        public string msg;
    }

    public enum level
    {
        info,
        error
    }

    public class Wss : IDisposable
    {
        private ClientWebSocket _ws;
        private CancellationTokenSource _cts;
        private int _disposed;
        internal event Action<Exception> OnError;

        public event Action<Log> OnLog;
        public event EventHandler<MessageEventArgs> OnMessage;
        public event EventHandler<CloseEventArgs> OnColse; // Keeping original spelling

        public string WssAddress { get; set; }

        public Wss(string url)
        {
            WssAddress = url;
            _ws = new ClientWebSocket();
            _cts = new CancellationTokenSource();
        }

        public void AddHeader(string key, string value)
        {
            if (string.Equals(key, "User-Agent", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    _ws.Options.SetRequestHeader(key, value);
                }
                catch (ArgumentException)
                {
                    // Ignore User-Agent error in .NET Framework
                }
            }
            else
            {
                _ws.Options.SetRequestHeader(key, value);
            }
        }

        public bool Run()
        {
            try
            {
                // Connect synchronously-ish for compatibility with existing code structure
                ConnectAsync().GetAwaiter().GetResult();
                return _ws.State == WebSocketState.Open;
            }
            catch (Exception e)
            {
                OnLog?.Invoke(new Log { level = level.error, msg = $"Connection failed: {e.Message}" });
                return false;
            }
        }

        public Task ConnectAsync() => ConnectAsync(CancellationToken.None);

        internal async Task ConnectAsync(CancellationToken cancellationToken)
        {
            try
            {
                using (var connection = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken))
                {
                    connection.CancelAfter(TimeSpan.FromSeconds(10));
                    try
                    {
                        await _ws.ConnectAsync(new Uri(WssAddress), connection.Token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException e) when (!cancellationToken.IsCancellationRequested && !_cts.IsCancellationRequested)
                    {
                        throw new TimeoutException("WebSocket 连接超时。", e);
                    }
                }
                OnLog?.Invoke(new Log { level = level.info, msg = "WebSocket Connected" });
                
                // Start receiving loop
                _ = ReceiveLoop();
            }
            catch (Exception e)
            {
                OnLog?.Invoke(new Log { level = level.error, msg = $"WebSocket Connect Exception: {e}" });
                throw;
            }
        }

        private async Task ReceiveLoop()
        {
            var buffer = new byte[1024 * 4];
            try
            {
                while (_ws.State == WebSocketState.Open && !_cts.IsCancellationRequested)
                {
                    var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token).ConfigureAwait(false);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        OnColse?.Invoke(this, new CloseEventArgs 
                        { 
                            Code = (ushort)(result.CloseStatus ?? WebSocketCloseStatus.NormalClosure), 
                            Reason = result.CloseStatusDescription, 
                            WasClean = true 
                        });
                        await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", _cts.Token).ConfigureAwait(false);
                    }
                    else
                    {
                        var data = new List<byte>();
                        data.AddRange(new ArraySegment<byte>(buffer, 0, result.Count));

                        while (!result.EndOfMessage)
                        {
                            result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token).ConfigureAwait(false);
                            data.AddRange(new ArraySegment<byte>(buffer, 0, result.Count));
                        }

                        var msgArgs = new MessageEventArgs();
                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            msgArgs.IsText = true;
                            msgArgs.Data = Encoding.UTF8.GetString(data.ToArray());
                        }
                        else if (result.MessageType == WebSocketMessageType.Binary)
                        {
                            msgArgs.IsBinary = true;
                            msgArgs.RawData = data.ToArray();
                            // For compatibility, also set Data if needed, or leave it. 
                            // The original usage checked IsText/IsBinary.
                            // But original code sometimes regex-ed binary data? No, it used e.RawData for binary.
                            // Check Edge_tts.cs usage: 
                            // e.IsBinary -> regex on e.Data? No:
                            // var requestId = Regex.Match(e.Data, ...) -> e.Data is used even for binary? 
                            // Wait, WebSocketSharp's MessageEventArgs might populate Data for binary too?
                            // Let's check typical usage. 
                            // In Edge_tts.cs: 
                            // else if (e.IsBinary) { var data = e.RawData; var requestId = Regex.Match(e.Data, ...); }
                            // So YES, e.Data (string) is used even when IsBinary is true! 
                            // We must decode it.
                            msgArgs.Data = Encoding.UTF8.GetString(data.ToArray()); 
                        }
                        
                        OnMessage?.Invoke(this, msgArgs);
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                 OnLog?.Invoke(new Log { level = level.error, msg = $"WebSocket Receive Error: {e}" });
                 OnError?.Invoke(e);
                 OnColse?.Invoke(this, new CloseEventArgs { Reason = e.Message });
            }
        }

        public void Send(string msg)
        {
            // Sync wrapper
            SendAsync(msg).GetAwaiter().GetResult();
        }

        public Task SendAsync(string msg) => SendAsync(msg, CancellationToken.None);

        internal async Task SendAsync(string msg, CancellationToken cancellationToken)
        {
            if (_ws.State != WebSocketState.Open) throw new WebSocketException("WebSocket is not open.");
            var bytes = Encoding.UTF8.GetBytes(msg);
            using (var sending = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken))
                await _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, sending.Token).ConfigureAwait(false);
            OnLog?.Invoke(new Log { level = level.info, msg = $"WebSocket sent: {msg}" });
        }

        public void Close()
        {
            _cts.Cancel();
            _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", CancellationToken.None).GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
            _cts.Cancel();
            _ws.Dispose();
            _cts.Dispose();
        }
    }
}
