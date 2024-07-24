namespace SmartEduDownloader
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnDownloadBooks = new Button();
            txtLog = new TextBox();
            btnDownloadVideos = new Button();
            SuspendLayout();
            // 
            // btnDownloadBooks
            // 
            btnDownloadBooks.Location = new Point(250, 72);
            btnDownloadBooks.Name = "btnDownloadBooks";
            btnDownloadBooks.Size = new Size(219, 63);
            btnDownloadBooks.TabIndex = 0;
            btnDownloadBooks.Text = "下载教材电子版";
            btnDownloadBooks.UseVisualStyleBackColor = true;
            btnDownloadBooks.Click += btnNavigate_Click;
            // 
            // txtLog
            // 
            txtLog.Location = new Point(12, 533);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.Size = new Size(1192, 185);
            txtLog.TabIndex = 1;
            // 
            // btnDownloadVideos
            // 
            btnDownloadVideos.Location = new Point(651, 72);
            btnDownloadVideos.Name = "btnDownloadVideos";
            btnDownloadVideos.Size = new Size(219, 63);
            btnDownloadVideos.TabIndex = 2;
            btnDownloadVideos.Text = "下载视频教程 全套";
            btnDownloadVideos.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1222, 730);
            Controls.Add(btnDownloadVideos);
            Controls.Add(txtLog);
            Controls.Add(btnDownloadBooks);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "电子教材下载：国家智慧教育平台";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_LoadAsync;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnDownloadBooks;
        private TextBox txtLog;
        private Button btnDownloadVideos;
    }
}
