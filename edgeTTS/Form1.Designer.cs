using System.Drawing;
using System.Windows.Forms;

namespace edgeTTS
{
    partial class edgeTTS
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(edgeTTS));
            textBox = new TextBox();
            languageCombo = new ComboBox();
            speedTrackBar = new TrackBar();
            speedLabel = new Label();
            playStopButton = new Button();
            browseButton = new Button();
            savePathTextBox = new TextBox();
            synthesizeButton = new Button();
            label1 = new Label();
            label2 = new Label();
            allOfCheckBox = new CheckBox();
            allOfToolTip = new ToolTip(components);
            progressBar = new ProgressBar();
            progressLabel = new Label();
            ((System.ComponentModel.ISupportInitialize)speedTrackBar).BeginInit();
            SuspendLayout();
            // 
            // textBox
            // 
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.Font = new Font("Segoe UI", 10F);
            textBox.Location = new Point(20, 20);
            textBox.Multiline = true;
            textBox.Name = "textBox";
            textBox.ScrollBars = ScrollBars.Vertical;
            textBox.Size = new Size(540, 100);
            textBox.TabIndex = 0;
            textBox.Text = "你好，世界";
            // 
            // languageCombo
            // 
            languageCombo.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            languageCombo.AutoCompleteSource = AutoCompleteSource.ListItems;
            languageCombo.Location = new Point(85, 138);
            languageCombo.Name = "languageCombo";
            languageCombo.Size = new Size(343, 23);
            languageCombo.TabIndex = 1;
            languageCombo.SelectedIndexChanged += languageCombo_SelectedIndexChanged;
            // 
            // speedTrackBar
            // 
            speedTrackBar.Location = new Point(85, 186);
            speedTrackBar.Maximum = 100;
            speedTrackBar.Minimum = -100;
            speedTrackBar.Name = "speedTrackBar";
            speedTrackBar.Size = new Size(343, 45);
            speedTrackBar.TabIndex = 2;
            speedTrackBar.TickStyle = TickStyle.Both;
            speedTrackBar.Scroll += speedTrackBar_Scroll;
            // 
            // speedLabel
            // 
            speedLabel.Location = new Point(17, 191);
            speedLabel.Name = "speedLabel";
            speedLabel.Size = new Size(80, 20);
            speedLabel.TabIndex = 3;
            speedLabel.Text = "变速: 0";
            // 
            // playStopButton
            // 
            playStopButton.BackColor = Color.FromArgb(0, 192, 0);
            playStopButton.Location = new Point(460, 186);
            playStopButton.Name = "playStopButton";
            playStopButton.Size = new Size(100, 25);
            playStopButton.TabIndex = 4;
            playStopButton.Text = "试听";
            playStopButton.UseVisualStyleBackColor = false;
            playStopButton.Click += playStopButton_Click;
            // 
            // browseButton
            // 
            browseButton.Location = new Point(460, 240);
            browseButton.Name = "browseButton";
            browseButton.Size = new Size(100, 25);
            browseButton.TabIndex = 5;
            browseButton.Text = "浏览...";
            browseButton.Click += browseButton_Click;
            // 
            // savePathTextBox
            // 
            savePathTextBox.Location = new Point(85, 242);
            savePathTextBox.Name = "savePathTextBox";
            savePathTextBox.ReadOnly = true;
            savePathTextBox.Size = new Size(343, 23);
            savePathTextBox.TabIndex = 6;
            // 
            // synthesizeButton
            // 
            synthesizeButton.Location = new Point(460, 291);
            synthesizeButton.Name = "synthesizeButton";
            synthesizeButton.Size = new Size(100, 30);
            synthesizeButton.TabIndex = 7;
            synthesizeButton.Text = "合成";
            synthesizeButton.Click += synthesizeButton_Click;
            // 
            // label1
            // 
            label1.Location = new Point(17, 245);
            label1.Name = "label1";
            label1.Size = new Size(66, 15);
            label1.TabIndex = 8;
            label1.Text = "保存位置";
            // 
            // label2
            // 
            label2.Location = new Point(17, 141);
            label2.Name = "label2";
            label2.Size = new Size(62, 20);
            label2.TabIndex = 9;
            label2.Text = "选择语种";
            // 
            // allOfCheckBox
            // 
            allOfCheckBox.AutoSize = true;
            allOfCheckBox.Location = new Point(460, 138);
            allOfCheckBox.Name = "allOfCheckBox";
            allOfCheckBox.Size = new Size(91, 19);
            allOfCheckBox.TabIndex = 10;
            allOfCheckBox.Text = "所有发言人";
            allOfToolTip.SetToolTip(allOfCheckBox, "合成该语种所有发言人");
            allOfCheckBox.UseVisualStyleBackColor = true;
            // 
            // allOfToolTip
            // 
            allOfToolTip.AutomaticDelay = 100;
            // 
            // progressBar
            // 
            progressBar.Location = new Point(85, 300);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(343, 10);
            progressBar.TabIndex = 11;
            // 
            // progressLabel
            // 
            progressLabel.Location = new Point(17, 298);
            progressLabel.Name = "progressLabel";
            progressLabel.Size = new Size(66, 15);
            progressLabel.TabIndex = 12;
            progressLabel.Text = "合成进度";
            // 
            // edgeTTS
            // 
            BackColor = Color.FromArgb(250, 250, 250);
            ClientSize = new Size(584, 340);
            Controls.Add(progressLabel);
            Controls.Add(progressBar);
            Controls.Add(allOfCheckBox);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(textBox);
            Controls.Add(languageCombo);
            Controls.Add(speedTrackBar);
            Controls.Add(speedLabel);
            Controls.Add(playStopButton);
            Controls.Add(browseButton);
            Controls.Add(savePathTextBox);
            Controls.Add(synthesizeButton);
            Font = new Font("Segoe UI", 9F);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "edgeTTS";
            Text = "EdgeTTS";
            Load += edgeTTS_Load;
            ((System.ComponentModel.ISupportInitialize)speedTrackBar).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private TextBox textBox;
        private ComboBox languageCombo;
        private TrackBar speedTrackBar;
        private Label speedLabel;
        private Button playStopButton;
        private Button browseButton;
        private TextBox savePathTextBox;
        private Button synthesizeButton;
        private Label label1;
        private Label label2;
        private CheckBox allOfCheckBox;
        private ToolTip allOfToolTip;
        private ProgressBar progressBar;
        private Label progressLabel;
    }
}

