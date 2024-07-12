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
            btnNavigate = new Button();
            txtLog = new TextBox();
            SuspendLayout();
            // 
            // btnNavigate
            // 
            btnNavigate.Location = new Point(317, 34);
            btnNavigate.Name = "btnNavigate";
            btnNavigate.Size = new Size(510, 63);
            btnNavigate.TabIndex = 0;
            btnNavigate.Text = "前往： 国家智慧教育平台";
            btnNavigate.UseVisualStyleBackColor = true;
            btnNavigate.Click += btnNavigate_Click;
            // 
            // txtLog
            // 
            txtLog.Location = new Point(12, 533);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.Size = new Size(1192, 185);
            txtLog.TabIndex = 1;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1222, 730);
            Controls.Add(txtLog);
            Controls.Add(btnNavigate);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "电子教材下载：国家智慧教育平台";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_LoadAsync;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnNavigate;
        private TextBox txtLog;
    }
}
