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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(edgeTTS));
            this.textBox = new System.Windows.Forms.TextBox();
            this.languageCombo = new System.Windows.Forms.ComboBox();
            this.speedTrackBar = new System.Windows.Forms.TrackBar();
            this.speedLabel = new System.Windows.Forms.Label();
            this.playStopButton = new System.Windows.Forms.Button();
            this.browseButton = new System.Windows.Forms.Button();
            this.savePathTextBox = new System.Windows.Forms.TextBox();
            this.synthesizeButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.allOfCheckBox = new System.Windows.Forms.CheckBox();
            this.allOfToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.progressLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.speedTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // textBox
            // 
            this.textBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.textBox.Location = new System.Drawing.Point(20, 20);
            this.textBox.Multiline = true;
            this.textBox.Name = "textBox";
            this.textBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox.Size = new System.Drawing.Size(540, 100);
            this.textBox.TabIndex = 0;
            this.textBox.Text = "你好，世界";
            // 
            // languageCombo
            // 
            this.languageCombo.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.languageCombo.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.languageCombo.Location = new System.Drawing.Point(85, 138);
            this.languageCombo.Name = "languageCombo";
            this.languageCombo.Size = new System.Drawing.Size(343, 23);
            this.languageCombo.TabIndex = 1;
            this.languageCombo.SelectedIndexChanged += new System.EventHandler(this.languageCombo_SelectedIndexChanged);
            // 
            // speedTrackBar
            // 
            this.speedTrackBar.Location = new System.Drawing.Point(85, 186);
            this.speedTrackBar.Maximum = 100;
            this.speedTrackBar.Minimum = -100;
            this.speedTrackBar.Name = "speedTrackBar";
            this.speedTrackBar.Size = new System.Drawing.Size(343, 45);
            this.speedTrackBar.TabIndex = 2;
            this.speedTrackBar.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.speedTrackBar.Scroll += new System.EventHandler(this.speedTrackBar_Scroll);
            // 
            // speedLabel
            // 
            this.speedLabel.Location = new System.Drawing.Point(17, 191);
            this.speedLabel.Name = "speedLabel";
            this.speedLabel.Size = new System.Drawing.Size(80, 20);
            this.speedLabel.TabIndex = 3;
            this.speedLabel.Text = "变速: 0";
            // 
            // playStopButton
            // 
            this.playStopButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.playStopButton.Location = new System.Drawing.Point(460, 186);
            this.playStopButton.Name = "playStopButton";
            this.playStopButton.Size = new System.Drawing.Size(100, 25);
            this.playStopButton.TabIndex = 4;
            this.playStopButton.Text = "试听";
            this.playStopButton.UseVisualStyleBackColor = false;
            this.playStopButton.Click += new System.EventHandler(this.playStopButton_Click);
            // 
            // browseButton
            // 
            this.browseButton.Location = new System.Drawing.Point(460, 240);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(100, 25);
            this.browseButton.TabIndex = 5;
            this.browseButton.Text = "浏览...";
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
            // 
            // savePathTextBox
            // 
            this.savePathTextBox.Location = new System.Drawing.Point(85, 242);
            this.savePathTextBox.Name = "savePathTextBox";
            this.savePathTextBox.ReadOnly = true;
            this.savePathTextBox.Size = new System.Drawing.Size(343, 23);
            this.savePathTextBox.TabIndex = 6;
            // 
            // synthesizeButton
            // 
            this.synthesizeButton.Location = new System.Drawing.Point(460, 291);
            this.synthesizeButton.Name = "synthesizeButton";
            this.synthesizeButton.Size = new System.Drawing.Size(100, 30);
            this.synthesizeButton.TabIndex = 7;
            this.synthesizeButton.Text = "合成";
            this.synthesizeButton.Click += new System.EventHandler(this.synthesizeButton_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(17, 245);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 15);
            this.label1.TabIndex = 8;
            this.label1.Text = "保存位置";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(17, 141);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(62, 20);
            this.label2.TabIndex = 9;
            this.label2.Text = "选择语种";
            // 
            // allOfCheckBox
            // 
            this.allOfCheckBox.AutoSize = true;
            this.allOfCheckBox.Location = new System.Drawing.Point(460, 138);
            this.allOfCheckBox.Name = "allOfCheckBox";
            this.allOfCheckBox.Size = new System.Drawing.Size(91, 19);
            this.allOfCheckBox.TabIndex = 10;
            this.allOfCheckBox.Text = "所有发言人";
            this.allOfToolTip.SetToolTip(this.allOfCheckBox, "合成该语种所有发言人");
            this.allOfCheckBox.UseVisualStyleBackColor = true;
            // 
            // allOfToolTip
            // 
            this.allOfToolTip.AutomaticDelay = 100;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(85, 300);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(343, 10);
            this.progressBar.TabIndex = 11;
            // 
            // progressLabel
            // 
            this.progressLabel.Location = new System.Drawing.Point(17, 298);
            this.progressLabel.Name = "progressLabel";
            this.progressLabel.Size = new System.Drawing.Size(66, 15);
            this.progressLabel.TabIndex = 12;
            this.progressLabel.Text = "合成进度";
            // 
            // edgeTTS
            // 
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(584, 340);
            this.Controls.Add(this.progressLabel);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.allOfCheckBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox);
            this.Controls.Add(this.languageCombo);
            this.Controls.Add(this.speedTrackBar);
            this.Controls.Add(this.speedLabel);
            this.Controls.Add(this.playStopButton);
            this.Controls.Add(this.browseButton);
            this.Controls.Add(this.savePathTextBox);
            this.Controls.Add(this.synthesizeButton);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "edgeTTS";
            this.Text = "EdgeTTS";
            this.Load += new System.EventHandler(this.edgeTTS_Load);
            ((System.ComponentModel.ISupportInitialize)(this.speedTrackBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

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

