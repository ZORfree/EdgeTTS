// See https://aka.ms/new-console-template for more information
using Edge_tts_sharp;
using Edge_tts_sharp.Model;
using Edge_tts_sharp.Utils;
using System;
using System.Web;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;

if (args.Contains("--self-test"))
{
    var failures = 0;
    void Check(string name, Action test)
    {
        try { test(); Console.WriteLine("PASS " + name); }
        catch (Exception e) { failures++; Console.WriteLine("FAIL " + name + ": " + e.Message); }
    }
    Check("签名采用上游 v2 格式", () =>
    {
        var method = typeof(Edge_tts).GetMethod("GenerateSecMsGecToken", BindingFlags.NonPublic | BindingFlags.Static)!;
        var token = (string)method.Invoke(null, null)!;
        if (!Regex.IsMatch(token, "^[0-9A-F]{16}-[0-9A-F]{64}$"))
            throw new Exception("预期 CS64-SHA256，实际为 " + token);
    });
    Check("签名与上游固定时间向量一致", () =>
    {
        var type = typeof(Edge_tts).Assembly.GetType("Edge_tts_sharp.EdgeToken");
        var method = type?.GetMethod("Generate", BindingFlags.Static | BindingFlags.NonPublic);
        if (method == null) throw new Exception("缺少可验证的 v2 签名实现");
        var vectors = new (long Time, string Token)[]
        {
            (0, "52BC7A17B50DFEC9-28C4394775C8751CD2F6859C030C06DB1489ACAB3B3A7651831FC26895177ED6"),
            (299, "52BC7A17B50DFEC9-28C4394775C8751CD2F6859C030C06DB1489ACAB3B3A7651831FC26895177ED6"),
            (300, "F7535A20A262E331-28675BF85C7106BBF20035A78FCB0B6890813EAD5C3B983EF3424931B720BABB"),
            (1735689600, "54404AFE67C9B44A-E72E51D8B4F5F75E9A96B7E20D7877227455BC310433BD30C817CD54A1D9313E"),
            (1793404800, "14644CB40F95B2FD-7F6523A09DA6131151D92FCDD6F9B731F08025D84FFA4186943FE58EF0C9A288")
        };
        foreach (var vector in vectors)
        {
            var actual = (string)method.Invoke(null, new object[] { DateTimeOffset.FromUnixTimeSeconds(vector.Time).UtcDateTime })!;
            if (actual != vector.Token) throw new Exception("时间 " + vector.Time + " 的签名不匹配：" + actual);
        }
    });
    foreach (var scenario in new[] { "handshake", "success", "rejected", "closed", "timeout", "callback", "save", "empty", "malformed" })
        Check("WebSocket " + scenario, () => MockRequest(scenario).GetAwaiter().GetResult());
    Environment.ExitCode = failures == 0 ? 0 : 1;
    return;
}

if (args.Contains("--online-test"))
{
    Edge_tts.Await = true;
    var onlineVoice = Edge_tts.GetVoice().First(v => v.ShortName == "zh-CN-XiaoxiaoNeural");
    var received = 0;
    Edge_tts.Invoke(new PlayOption { Text = "你好，这是连接测试。" }, onlineVoice, data =>
    {
        using var reader = new NAudio.Wave.Mp3FileReader(new MemoryStream(data.ToArray()));
        if (reader.Length == 0) throw new Exception("返回数据无法解码为非空 MP3");
        received = data.Count;
        Console.WriteLine("MP3 解码成功：" + reader.WaveFormat);
    });
    if (received == 0) throw new Exception("在线合成未收到音频");
    Console.WriteLine("PASS 在线合成，收到 " + received + " 字节");
    return;
}

Edge_tts.Await = true;
PlayOption option = new PlayOption
{
    Rate = 1,
    Text = ""
};
string msg = string.Empty;
Console.WriteLine("请输入代理地址(默认为空):");
string proxy = Console.ReadLine();
if (!string.IsNullOrEmpty(proxy))
{
    Edge_tts.Proxy = proxy;
}
//Console.WriteLine("请输入文本内容.");
option.Text = "hi geely";
// 获取xiaoxiao语音包
var voice = Edge_tts.GetVoice().FirstOrDefault(i=> i.Name == "Microsoft Server Speech Text to Speech Voice (zh-CN, XiaoxiaoNeural)");
// 文字转语音，并且设置语速
//Edge_tts.PlayText(option, voice);
MyFunc("hi geely", voice);
Edge_tts.PlayText(option, voice);
Console.WriteLine("自动输出");
Console.ReadLine();


// 保存音频
static void SaveAudio()
{
    PlayOption option = new PlayOption
    {
        Rate = 0,
        Text = "Hello EdgeTTs",
        SavePath = "C:\\audio"
    };
    // 获取xiaoxiao语音包
    var voice = Edge_tts.GetVoice().FirstOrDefault(i => i.Name == "Microsoft Server Speech Text to Speech Voice (zh-CN, XiaoxiaoNeural)");
    Edge_tts.SaveAudio(option, voice);
}
// 文本转语音
static void TextToAudio()
{
    PlayOption option = new PlayOption
    {
        Rate = 0,
        Text = "Hello EdgeTTs",
    };
    var voice = Edge_tts.GetVoice().First();
    Edge_tts.PlayText(option, voice);
}
// 自定义接口使用
static void MyFunc(string msg, eVoice voice)
{
    PlayOption option = new PlayOption
    {
        Rate = 0,
        Text = msg,
    };
    Edge_tts.Invoke(option, voice, libaray =>
    {
        // 写入自己的操作
        // ...
        Console.WriteLine($"接收到{libaray.Count}字节的音频数据");
    } );
}
// 获取一个PlayerAudio对象
static void getPlayer(string msg, eVoice voice)
{
    PlayOption option = new PlayOption
    {
        Rate = 0,
        Text = msg,
    };
    var player = Edge_tts.GetPlayer(option, voice);

    Console.WriteLine("开始播放");
    player.PlayAsync();
    Thread.Sleep(3000);


    Console.WriteLine("暂停播放");
    player.Pause();
    Thread.Sleep(3000);

    Console.WriteLine("继续播放");
    player.PlayAsync();
    Thread.Sleep(5000);

    player.Stop();
    Console.WriteLine("结束播放");
}

// 本地服务验证真实握手、消息顺序和失败退出，不依赖微软服务可用性。
static async Task MockRequest(string scenario)
{
    using var listener = new TcpListener(IPAddress.Loopback, 0);
    listener.Start();
    var port = ((IPEndPoint)listener.LocalEndpoint).Port;
    using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(5));
    var server = Task.Run(async () =>
    {
        using var client = await listener.AcceptTcpClientAsync(deadline.Token);
        using var stream = client.GetStream();
        var request = new StringBuilder();
        var one = new byte[1];
        while (!request.ToString().EndsWith("\r\n\r\n"))
        {
            if (await stream.ReadAsync(one, deadline.Token) == 0) throw new Exception("握手提前结束");
            request.Append((char)one[0]);
        }
        var headers = request.ToString();
        if (scenario == "handshake")
        {
            if (!headers.Contains("Sec-MS-GEC-Version=2-") || !Regex.IsMatch(headers, "ConnectionId=[0-9a-f]{32}"))
                throw new Exception("握手缺少 v2 版本或 ConnectionId");
        }
        if (scenario == "rejected")
        {
            await stream.WriteAsync(Encoding.ASCII.GetBytes("HTTP/1.1 403 Forbidden\r\nContent-Length: 0\r\nConnection: close\r\n\r\n"), deadline.Token);
            return;
        }
        var key = Regex.Match(headers, @"Sec-WebSocket-Key: (.*)\r\n", RegexOptions.IgnoreCase).Groups[1].Value.Trim();
        var accept = Convert.ToBase64String(SHA1.HashData(Encoding.ASCII.GetBytes(key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));
        await stream.WriteAsync(Encoding.ASCII.GetBytes("HTTP/1.1 101 Switching Protocols\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Accept: " + accept + "\r\n\r\n"), deadline.Token);
        using var socket = WebSocket.CreateFromStream(stream, true, null, Timeout.InfiniteTimeSpan);
        var buffer = new byte[8192];
        for (var i = 0; i < 2; i++)
        {
            WebSocketReceiveResult message;
            do { message = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), deadline.Token); } while (!message.EndOfMessage);
        }
        if (scenario == "timeout") { await Task.Delay(2000, deadline.Token); return; }
        if (scenario == "closed")
        {
            await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "test early close", deadline.Token);
            return;
        }
        await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("Path:turn.start\r\n\r\n")), WebSocketMessageType.Text, true, deadline.Token);
        await Task.Delay(100, deadline.Token);
        var audioHeaders = Encoding.UTF8.GetBytes("X-Test:中文\r\nPath:audio\r\nContent-Type:audio/mpeg\r\n");
        var frame = new byte[2 + audioHeaders.Length + 4];
        frame[0] = (byte)(audioHeaders.Length >> 8); frame[1] = (byte)audioHeaders.Length;
        audioHeaders.CopyTo(frame, 2);
        new byte[] { 0xFF, 0xFB, 0x90, 0x64 }.CopyTo(frame, 2 + audioHeaders.Length);
        if (scenario == "malformed") frame = new byte[] { 0x7F, 0xFF, 0 };
        if (scenario != "empty")
        {
            // 同一二进制消息拆成多个 WebSocket 片段。
            await socket.SendAsync(new ArraySegment<byte>(frame, 0, 2), WebSocketMessageType.Binary, false, deadline.Token);
            await socket.SendAsync(new ArraySegment<byte>(frame, 2, frame.Length - 2), WebSocketMessageType.Binary, true, deadline.Token);
        }
        await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("Path:turn.end\r\n\r\n")), WebSocketMessageType.Text, true, deadline.Token);
        // 保持连接：客户端必须在 turn.end 完成，不能依赖服务端关闭。
        try { await socket.ReceiveAsync(new ArraySegment<byte>(buffer), deadline.Token); }
        catch (WebSocketException) { }
    });
    Edge_tts.Proxy = "ws://127.0.0.1:" + port;
    Edge_tts.Await = true;
    var timeout = typeof(Edge_tts).GetField("RequestTimeout");
    var oldTimeout = timeout?.GetValue(null);
    if (scenario == "timeout") timeout?.SetValue(null, TimeSpan.FromMilliseconds(300));
    var audio = Array.Empty<byte>();
    var savePath = scenario == "save" ? Path.GetTempFileName() : "";
    Exception? error = null;
    var invocation = Task.Run(() =>
    {
        try
        {
            Edge_tts.Invoke(new PlayOption { Text = "test", SavePath = savePath }, Edge_tts.GetVoice().First(), bytes =>
            {
                if (scenario == "callback") throw new InvalidOperationException("callback failure");
                audio = bytes.ToArray();
            });
        }
        catch (Exception e) { error = e; }
    });
    try
    {
        await invocation.WaitAsync(TimeSpan.FromSeconds(3));
        if (scenario == "rejected" && (error == null || !error.ToString().Contains("403"))) throw new Exception("应传播包含 403 的完整连接错误");
        if (scenario == "closed" && error == null) throw new Exception("提前关闭必须报错");
        if (scenario == "timeout" && error is not TimeoutException) throw new Exception("应抛出 TimeoutException");
        if (scenario == "callback" && error?.Message != "callback failure") throw new Exception("回调异常没有传播");
        if ((scenario == "empty" || scenario == "malformed") && error is not InvalidDataException) throw new Exception("应传播协议/空音频错误");
        if (scenario == "save" && !File.ReadAllBytes(savePath).SequenceEqual(new byte[] { 0xFF, 0xFB, 0x90, 0x64 })) throw new Exception("未在 turn.end 保存完整音频");
        if (scenario == "success" || scenario == "handshake" || scenario == "save")
        {
            if (error != null) throw error;
            if (!audio.SequenceEqual(new byte[] { 0xFF, 0xFB, 0x90, 0x64 })) throw new Exception("未等待 turn.end 或音频头解析错误");
        }
        if (scenario != "timeout") await server.WaitAsync(TimeSpan.FromSeconds(1));
    }
    finally
    {
        Edge_tts.Proxy = null;
        if (savePath.Length > 0) File.Delete(savePath);
        if (timeout != null) timeout.SetValue(null, oldTimeout);
        deadline.Cancel();
        try { await server; } catch (OperationCanceledException) { }
    }
}
