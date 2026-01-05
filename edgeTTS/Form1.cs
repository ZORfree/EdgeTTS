using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Edge_tts_sharp;
using Edge_tts_sharp.Model;
using NAudio.Wave;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

///1.实现输入文本框、语种选择框、试听|停止按钮、保存位置(默认当前目录)
///2.实现语速的选择变换
///3.实现该语种的所有发言人的音频一键合成
///4.实现格式转换位单通道16k 16bit的wav音频格式
///5.添加语速变换和进度条
///6.防止多次点击造成重复合成设计
namespace edgeTTS
{
    public partial class edgeTTS : Form
    {
        Dictionary<string, string> langMappings = new Dictionary<string, string>
        {
            { "zh", "汉语" },{ "en", "英语" },{ "af", "南非荷兰语" },
            { "sq", "阿尔巴尼亚" },{ "am", "阿姆哈拉语" },{ "ar", "阿拉伯语" },{ "az", "阿塞拜疆语" },{ "bn", "孟加拉语" },{ "bs", "波斯尼亚语" },
            { "bg", "保加利亚语" },{ "my", "缅甸语" },{ "ca", "加泰罗尼亚语" },{ "hr", "克罗地亚语" },{ "cs", "捷克语" },{ "da", "丹麦语" },
            { "nl", "荷兰语" },{ "et", "爱沙尼亚语" },{ "fil", "菲律宾语" },{ "fi", "芬兰语" },{ "fr", "法语" },{ "gl", "加利西亚语" },
            { "ka", "格鲁吉亚语" },{ "de", "德语" },{ "el", "希腊语" },{ "gu", "古吉拉特语" },{ "he", "希伯来语" },{ "hi", "印地语" },
            { "hu", "匈牙利语" },{ "is", "冰岛语" },{ "id", "印尼语" },{ "ga", "爱尔兰语" },{ "it", "意大利语" },{ "iu", " 伊努克提图特语" },{ "ja", "日语" },
            { "jv", "爪哇语" },{ "kn", "卡纳达语" },{ "kk", "哈萨克语" },{ "km", "高棉语" },{ "ko", "韩语" },{ "lo", "老挝语" },
            { "lv", "拉脱维亚语" },{ "lt", "立陶宛语" },{ "mk", "马其顿语" },{ "ms", "马来语" },{ "ml", "马拉雅拉姆语" },{ "mt", "马耳他语" },
            { "mr", "马拉地语" },{ "mn", "蒙古语" },{ "ne", "尼泊尔语" },{ "nb", "挪威语" },{ "ps", "普什图语" },{ "fa", "波斯语" },
            { "pl", "波兰语" },{ "pt", "葡萄牙语" },{ "ro", "罗马尼亚语" },{ "ru", "俄语" },{ "sr", "塞尔维亚语" },{ "si", "僧伽罗语" },
            { "sk", "斯洛伐克语" },{ "sl", "斯洛文尼亚语" },{ "so", "索马里语" },{ "es", "西班牙语" },{ "su", "巽他语" },{ "sw", "斯瓦希里语" },
            { "sv", "瑞典语" },{ "ta", "泰米尔语" },{ "te", "泰卢固语" },{ "th", "泰语" },{ "tr", "土耳其语" },{ "uk", "乌克兰语" },
            { "ur", "乌尔都语" },{ "uz", "乌兹别克语" },{ "vi", "越南语" },{ "cy", "威尔士语" },{ "zu", "祖鲁语" }

        };
        string currentDirectory = Directory.GetCurrentDirectory();
        //private IWavePlayer _device;
        private IWavePlayer _device = new WaveOutEvent(); // Create device
        private CancellationTokenSource _cts;




        public edgeTTS()
        {
            InitializeComponent();
            savePathTextBox.Text = currentDirectory;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.Text = "EdgeTTS By Zorfree V" + Assembly.GetExecutingAssembly().GetName().Version.ToString();

        }
        //private void edgeTTS_Load(object sender, EventArgs e)
        //{
        //    Edge_tts.Await = true;
        //    PlayOption option = new PlayOption
        //    {
        //        Rate = 1,
        //        Text = ""
        //    };

        //    //Console.WriteLine("请输入文本内容.");
        //    option.Text = "hi geely";
        //    // 获取xiaoxiao语音包
        //    var voice = Edge_tts.GetVoice().FirstOrDefault(i => i.Name == "Microsoft Server Speech Text to Speech Voice (zh-CN, XiaoxiaoNeural)");
        //    Edge_tts.PlayText(option, voice);
        //}

        private void edgeTTS_Load(object sender, EventArgs e)
        {
            //Edge_tts.Proxy = "wss://webproxy.harui.qzz.io";
            Edge_tts.Await = true;
            var voices = Edge_tts.GetVoice();
            string defaultVoice = null;
            foreach (var item in voices)
            {
                //voice name isMicrosoft Server Speech Text to Speech Voice (zu-ZA, ThembaNeural), locale（语言） is zu-ZA, SuggestedCodec(音频类型) is audio-24khz-48kbitrate-mono-mp3

                //Console.WriteLine($"voice name is{item.Name}, locale（语言） is {item.Locale}, SuggestedCodec(音频类型) is {item.SuggestedCodec}");
                char[] delimiters = new char[] { '-' };
                string value;
                if (langMappings.TryGetValue(item.Locale.Split(delimiters)[0], out value))
                {
                    string itemVoice = item.Name.Replace("Microsoft Server Speech Text to Speech Voice", value);
                    languageCombo.Items.Add(itemVoice);
                    if (defaultVoice == null)
                    {
                        if (item.Name.Contains("zh-"))
                        {
                            defaultVoice = itemVoice;
                        }
                    }
                }
                else
                {
                    languageCombo.Items.Add(item.Name.Replace("Microsoft Server Speech Text to Speech Voice", "未知"));
                }
            }

            languageCombo.SelectedIndex = languageCombo.Items.IndexOf(defaultVoice);
        }

        private void languageCombo_SelectedIndexChanged(object sender, EventArgs e)
        {


        }
        private void StartUpdateProgress()
        {
            // 此处可用Timer完成而不是手动循环，但不建议使用UI线程上的Timer
            Task.Run(() =>
            {
                while (!_cts.IsCancellationRequested)
                {
                    Console.WriteLine("状态检测当前播放状态" + _device.PlaybackState);
                    if (_device.PlaybackState == PlaybackState.Stopped)
                    {
                        BeginInvoke(new Action(UpdateProgress));

                        Thread.Sleep(100);
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
            });
        }
        private void UpdateProgress()
        {
            playStopButton.Text = "试听";
            playStopButton.BackColor = Color.Green;
            textBox.Enabled = true;
            DisposeAll();
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folder = new FolderBrowserDialog();
            folder.Description = "请选择合成音频保存的目录";
            if (folder.ShowDialog() == DialogResult.OK)
            {
                string path = folder.SelectedPath; // "e:/go"
                savePathTextBox.Text = path;
            }
        }

        private void playStopButton_Click(object sender, EventArgs e)
        {


            if (_device.PlaybackState == PlaybackState.Playing)
            {
                StopAction();
                //DisposeAll();

            }
            else
            {
                playStopButton.Text = "停止";
                playStopButton.BackColor = Color.Red;
                textBox.Enabled = false;
                _cts = new CancellationTokenSource();

                char[] delimiters = new char[] { '(' };
                var voice = Edge_tts.GetVoice().FirstOrDefault(i => i.Name.Contains(languageCombo.Text.Split(delimiters)[1]));
                string msg = textBox.Text.Replace("#", " ");
                if (voice != null)
                {
                    if (!string.IsNullOrWhiteSpace(msg))
                    {
                        //Edge_tts.PlayText(msg, voice);
                        // 文字转语音，并且设置语速
                        //Edge_tts.PlayText(msg, voice, speedTrackBar.Value);
                        // 回调函数的第一个参数是binary数据
                        PlayOption option = new PlayOption
                        {
                            Rate = speedTrackBar.Value,
                            Text = msg,
                        };

                        Edge_tts.Invoke(option, voice, (_binary) =>
                        {
                            //Console.WriteLine(_device.PlaybackState);

                            var ms = new MemoryStream(_binary.ToArray());
                            var sr = new StreamMediaFoundationReader(ms);
                            _device.Init(sr);
                            //_device.PlaybackStopped += Device_OnPlaybackStopped;
                            PlayAction();
                            StartUpdateProgress(); // 界面更新线程

                        });
                    }
                }
            }

        }
        private void Device_OnPlaybackStopped(object obj, StoppedEventArgs arg)
        {
            StopAction();
        }

        private void StopAction()
        {
            _device?.Stop();
        }

        private void PlayAction()
        {
            _device?.Play();
        }

        private void PauseAction()
        {
            _device?.Pause();
        }

        private void DisposeDevice()
        {
            if (_device != null)
            {
                _device.PlaybackStopped -= Device_OnPlaybackStopped;
                _device.Dispose();
            }
        }

        private void DisposeAll()
        {
            DisposeDevice();
            _cts?.Cancel();
            _cts?.Dispose();
        }

        private void speedTrackBar_Scroll(object sender, EventArgs e)
        {
            //变速 : 0
            speedLabel.Text = "变速: " + speedTrackBar.Value;
        }

        private void synthesizeButton_Click(object sender, EventArgs e)
        {
            textBox.Enabled = false;
            synthesizeButton.Enabled = false;
            string[] msgs = textBox.Text.Split(new char[] { '#' });
            string languageComboText = languageCombo.Text;
            int speedTrackBarValue = speedTrackBar.Value;
            string savePathText = savePathTextBox.Text;
            string speed_type = "normal";

            if (speedTrackBarValue > 20)
            {
                speed_type = "fast";

            }
            else if (speedTrackBarValue < -20)
            {
                speed_type = "slow";
            }

            String outputPath = Path.Combine(savePathText, "Output").ToString();

            new Thread(() =>
            {

                if (!Directory.Exists(outputPath))
                {
                    Directory.CreateDirectory(outputPath);
                }

                TextWriter wav_scp = new StreamWriter(Path.Combine(outputPath, "edge_tts.txt"));
                TextWriter wav_lab = new StreamWriter(Path.Combine(outputPath, "edge_tts.lab"));
                for (int j = 0; j < msgs.Length; j++)
                {
                    string msg = msgs[j];

                    if (!string.IsNullOrWhiteSpace(msg))
                    {

                        DateTime now = DateTime.Now;
                        string nowString = now.ToString().Replace(" ", "-").Replace("/", "-").Replace(":", "_");
                        string savePath = Path.Combine(outputPath, nowString).ToString();

                        if (!Directory.Exists(savePath))
                        {
                            Directory.CreateDirectory(savePath);
                        }
                        if (allOfCheckBox.Checked)
                        {
                            char[] delimiters = new char[] { '(', '-', ')' };
                            Console.WriteLine(languageComboText.Split(delimiters)[1]);
                            var voices = Edge_tts.GetVoice().FindAll(i => i.Name.Contains(languageComboText.Split(delimiters)[1] + "-"));
                            int count = voices.Count;
                            BeginInvoke(new Action(() => { progressBar.Maximum = count; }));

                            int N = 0;
                            foreach (var item in voices)
                            {

                                char[] t_delimiters = new char[] { '(', ')' };
                                string saveName = $"{item.Name.Split(t_delimiters)[1].Replace(",", "_").Replace(" ", "") + j.ToString()}.wav";

                                //Console.WriteLine($"voice name is{item.Name}, locale（语言） is {item.Locale}, SuggestedCodec(音频类型) is {item.SuggestedCodec}");
                                // 只保存，不播放
                                //Edge_tts.SaveAudio(msg, item, speedTrackBarValue, Path.Combine(savePath, $"{saveName}.mp3"));
                                PlayOption option = new PlayOption
                                {
                                    Rate = speedTrackBarValue,
                                    Text = msg,
                                };
                                //Edge_tts.Await = true;
                                Edge_tts.Invoke(option, item, (_binary) =>
                                {
                                    Console.WriteLine("进入回调");
                                    var outFile = Path.Combine(savePath, saveName);
                                    var ms = new MemoryStream(_binary.ToArray());
                                    var reader = new StreamMediaFoundationReader(ms);
                                    var outFormat = new WaveFormat(16000, 1);
                                    var resampler = new MediaFoundationResampler(reader, outFormat);
                                    WaveFileWriter.CreateWaveFile(outFile, resampler);
                                });
                                wav_scp.WriteLine(nowString + "/" + saveName);
                                wav_lab.WriteLine(saveName + "\t" + speed_type + "\t" + msg + "\tpinyin\t1");

                                N++;
                                BeginInvoke(new Action(() => { progressBar.Value = N; }));

                            }
                        }
                        else
                        {
                            char[] delimiters = new char[] { '(', ')' };
                            var voice = Edge_tts.GetVoice().FirstOrDefault(i => i.Name.Contains(languageComboText.Split(delimiters)[1]));
                            //string savePath = Path.Combine(outputPath, nowString).ToString();
                            string saveName = $"{voice.Name.Split(delimiters)[1].Replace(",", "_").Replace(" ", "") + j.ToString()}.wav";
                            PlayOption option = new PlayOption
                            {
                                Rate = speedTrackBarValue,
                                Text = msg,
                            };
                            Edge_tts.Await = true;
                            Edge_tts.Invoke(option, voice, (_binary) =>
                            {
                                var outFile = Path.Combine(savePath, saveName);
                                var ms = new MemoryStream(_binary.ToArray());
                                var reader = new StreamMediaFoundationReader(ms);
                                var outFormat = new WaveFormat(16000, 1);
                                var resampler = new MediaFoundationResampler(reader, outFormat);
                                WaveFileWriter.CreateWaveFile(outFile, resampler);
                            });
                            wav_scp.WriteLine(nowString + "/" + saveName);
                            wav_lab.WriteLine(saveName + "\t" + speed_type + "\t" + msg + "\tpinyin\t1");

                            BeginInvoke(new Action(() => { progressBar.Value = progressBar.Maximum; }));
                        }
                    }

                }
                wav_scp.Close();
                wav_lab.Close();
                Console.WriteLine("已经关闭拉");



                BeginInvoke(new Action(() =>
                {
                    textBox.Enabled = true;
                    synthesizeButton.Enabled = true;
                }));
            }).Start();
        }
    }
}
