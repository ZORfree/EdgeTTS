using Edge_tts_sharp.Model;
using System;
using System.Resources;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Edge_tts_sharp.Utils;
using System.Threading;


namespace Edge_tts_sharp
{
    public class Edge_tts
    {
        /// <summary>
        /// 调试模式
        /// </summary>
        public static bool Debug = false;
        /// <summary>
        /// 同步模式
        /// </summary>
        public static bool Await = false;
        /// <summary>
        /// 代理地址
        /// </summary>
        public static string Proxy = null;
        
        static string GenerateSecMsGecToken()
        {
            return EdgeToken.Generate(DateTime.UtcNow);
        }
        static string GetGUID()
        {
            return Guid.NewGuid().ToString().Replace("-","");
        }
        /// <summary>
        /// 讲一个浮点型数值转换为百分比数值
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        static string FromatPercentage(double input)
        {
            string output;

            if (input < 0)
            {
                output = input.ToString("+#;-#;0") + "%";
            }
            else
            {
                output = input.ToString("+#;-#;0") + "%";
            }
            return output;
        }
        static string ConvertToAudioFormatWebSocketString(string outputformat)
        {
            return "Content-Type:application/json; charset=utf-8\r\nPath:speech.config\r\n\r\n{\"context\":{\"synthesis\":{\"audio\":{\"metadataoptions\":{\"sentenceBoundaryEnabled\":\"false\",\"wordBoundaryEnabled\":\"false\"},\"outputFormat\":\"" + outputformat + "\"}}}}";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lang">输出语言</param>
        /// <param name="voice">音源名</param>
        /// <param name="rate">语速，-100% - 100% 之间的值，无需传递百分号</param>
        /// <param name="text"></param>
        /// <returns></returns>
        static string ConvertToSsmlText(string lang, string voice, int rate, int volume, string text)
        {
            return $"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis'  xml:lang='{lang}'><voice name='{voice}'><prosody pitch='+0Hz' rate ='{FromatPercentage(rate)}' volume='{volume}'>{text}</prosody></voice></speak>";
        }
        static string ConvertToSsmlWebSocketString(string requestId, string lang, string voice,int rate, int volume, string msg)
        {
            return $"X-RequestId:{requestId}\r\nContent-Type:application/ssml+xml\r\nPath:ssml\r\n\r\n{ConvertToSsmlText(lang, voice, rate, volume, msg)}";
        }
        /// <summary>
        /// 语言转文本，将结果返回到回调函数中
        /// </summary>
        /// <param name="option">播放参数</param>
        /// <param name="voice">音源参数</param>
        public static void Invoke(PlayOption option, eVoice voice, Action<List<byte>> callback, IProgress<List<byte>> progress = null)
        {
            var task = InvokeAsync(option, voice, callback, progress);
            if (Await) task.GetAwaiter().GetResult();
            else _ = ObserveAsync(task);
        }

        /// <summary>连接及整个合成请求的最长等待时间。</summary>
        public static TimeSpan RequestTimeout = TimeSpan.FromSeconds(120);

        /// <summary>等待 turn.end，完成保存和回调；连接、协议和回调错误通过 Task 传播。</summary>
        public static async Task InvokeAsync(PlayOption option, eVoice voice, Action<List<byte>> callback, IProgress<List<byte>> progress = null)
        {
            if (option == null) throw new ArgumentNullException(nameof(option));
            if (voice == null) throw new ArgumentNullException(nameof(voice));
            if (RequestTimeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(RequestTimeout));
            var timeout = RequestTimeout;
            var binary = new List<byte>();
            var completed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var url = $"wss://speech.platform.bing.com/consumer/speech/synthesize/readaloud/edge/v1?TrustedClientToken={EdgeToken.TrustedClientToken}&ConnectionId={GetGUID()}&Sec-MS-GEC={GenerateSecMsGecToken()}&Sec-MS-GEC-Version={EdgeToken.Version}";
            if (!string.IsNullOrEmpty(Proxy)) url = Proxy.TrimEnd('/') + "/" + url;

            using (var wss = new Wss(url))
            using (var deadline = new CancellationTokenSource(timeout))
            using (deadline.Token.Register(() =>
                completed.TrySetException(new TimeoutException("语音合成请求超时。"))))
            {
                wss.AddHeader("Origin", "chrome-extension://jdiccldimpdaibmpdkjnbmckianbfold");
                wss.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36 Edg/143.0.0.0");
                wss.AddHeader("Accept-Encoding", "gzip, deflate, br, zstd");
                wss.AddHeader("Accept-Language", "en-US,en;q=0.9");
                wss.AddHeader("Pragma", "no-cache");
                wss.AddHeader("Cache-Control", "no-cache");
                wss.AddHeader("Cookie", "muid=" + GetGUID().ToUpperInvariant() + ";");
                wss.OnLog += log =>
                {
                    if (Debug) Console.WriteLine($"[{log.level}] {log.msg}");
                };
                wss.OnError += error => completed.TrySetException(error);
                wss.OnColse += (sender, e) =>
                    completed.TrySetException(new WebSocketException("服务端在 turn.end 之前关闭连接：" + e.Reason));
                wss.OnMessage += (sender, e) =>
                {
                    if (completed.Task.IsCompleted) return;
                    try
                    {
                        if (e.IsText)
                        {
                            if (Debug) Console.WriteLine(e.Data);
                            if (!e.Data.Split(new[] { "\r\n" }, StringSplitOptions.None).Contains("Path:turn.end")) return;
                            if (binary.Count == 0) throw new InvalidDataException("服务端未返回音频数据。");
                            if (!string.IsNullOrEmpty(option.SavePath))
                                File.WriteAllBytes(option.SavePath, binary.ToArray());
                            callback?.Invoke(binary);
                            completed.TrySetResult(true);
                        }
                        else if (e.IsBinary)
                        {
                            var data = e.RawData;
                            if (data.Length < 2) throw new InvalidDataException("音频帧缺少头长度。");
                            int headerLength = (data[0] << 8) | data[1];
                            int offset = headerLength + 2;
                            if (offset > data.Length) throw new InvalidDataException("音频帧头长度超出消息范围。");
                            var headers = Encoding.UTF8.GetString(data, 2, headerLength);
                            if (!headers.Split(new[] { "\r\n" }, StringSplitOptions.None).Contains("Path:audio"))
                                throw new InvalidDataException("收到未知二进制消息。");
                            if (offset == data.Length) return;
                            var chunk = data.Skip(offset).ToList();
                            binary.AddRange(chunk);
                            progress?.Report(chunk);
                        }
                    }
                    catch (Exception error) { completed.TrySetException(error); }
                };

                // 超时覆盖连接及发送阶段；Dispose 会中止尚未完成的接收。
                try
                {
                    await wss.ConnectAsync(deadline.Token).ConfigureAwait(false);
                    await wss.SendAsync(ConvertToAudioFormatWebSocketString(voice.SuggestedCodec), deadline.Token).ConfigureAwait(false);
                    await wss.SendAsync(ConvertToSsmlWebSocketString(GetGUID(), voice.Locale, voice.Name,
                        option.Rate, (int)(option.Volume * 100), option.Text), deadline.Token).ConfigureAwait(false);
                    await completed.Task.ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (deadline.IsCancellationRequested)
                {
                    throw new TimeoutException("语音合成请求超时。");
                }
                finally
                {
                    // 观察超时产生的异常，避免连接阶段失败时遗留未观察的任务。
                    if (completed.Task.IsFaulted) _ = completed.Task.Exception;
                }
            }
        }

        private static async Task ObserveAsync(Task task)
        {
            try { await task.ConfigureAwait(false); }
            catch (Exception error)
            {
                System.Diagnostics.Trace.TraceError(error.ToString());
                Console.Error.WriteLine(error);
            }
        }
        /// <summary>
        /// 另存为mp3文件
        /// </summary>
        /// <param name="option">播放参数</param>
        /// <param name="voice">音源参数</param>
        public static void SaveAudio(PlayOption option, eVoice voice)
        {
            if (string.IsNullOrEmpty(option.SavePath))
            {
                throw new Exception("保存路径为空，请核对参数后重试.");
            }
            Invoke(option, voice, null);
        }
        /// <summary>
        /// 调用微软Edge接口，文字转语音
        /// </summary>
        /// <param name="option">播放参数</param>
        /// <param name="voice">音源参数</param>
        public static void PlayText(PlayOption option, eVoice voice)
        {
            Invoke(option, voice, (_binary) =>
            {
                Audio.PlayToByteAsync(_binary.ToArray(), option.Volume);
            });
        }
        /// <summary>
        /// 获取一个`AudioPlayer`的对象
        /// </summary>
        /// <param name="option">播放参数</param>
        /// <param name="voice">音源参数</param>
        /// <returns></returns>
        public static AudioPlayer GetPlayer(PlayOption option, eVoice voice)
        {
            AudioPlayer player = null;
            InvokeAsync(option, voice, (_binary) =>
            {
                player = new AudioPlayer(_binary.ToArray(), option.Volume);
            }).GetAwaiter().GetResult();
            return player;
        }
        /// <summary>
        /// 同步等待播放音频结束
        /// </summary>
        /// <param name="option">播放参数</param>
        /// <param name="voice">音源参数</param>
        //public static void PlayTextAsync(PlayOption option, eVoice voice)
        //{
        //    List<byte> buffer = new List<byte>();
        //    var audioStreamer = new Mp3AudioStreamer();
        //    var report = new Progress<List<byte>>((binary) =>
        //    {
        //        audioStreamer.OnAudioReceived(binary.ToArray());
        //    });
        //    Invoke(option, voice, null, report);

        //    audioStreamer.Stop();
        //}
        
        /// <summary>
        /// 获取支持的音频列表
        /// </summary>
        /// <returns></returns>
        public static List<eVoice> GetVoice()
        {
            var voiceList = Tools.GetEmbedText("Edge_tts_sharp.Source.VoiceList.json");
            return Tools.StringToJson<List<eVoice>>(voiceList);
        }
    }
}
