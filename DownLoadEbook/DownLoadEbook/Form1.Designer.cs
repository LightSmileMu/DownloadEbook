namespace DownLoadEbook
{
    partial class MainForm
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
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.bg_worker = new System.ComponentModel.BackgroundWorker();
            this.bg_downBookImg = new System.ComponentModel.BackgroundWorker();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(258, 153);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "获取书籍名称";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(352, 153);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "下载书籍图片";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressBar1.Location = new System.Drawing.Point(0, 244);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(639, 18);
            this.progressBar1.TabIndex = 2;
            // 
            // bg_worker
            // 
            this.bg_worker.WorkerReportsProgress = true;
            this.bg_worker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bg_worker_DoWork);
            this.bg_worker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bg_worker_ProgressChanged);
            this.bg_worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bg_worker_RunWorkerCompleted);
            // 
            // bg_downBookImg
            // 
            this.bg_downBookImg.WorkerReportsProgress = true;
            this.bg_downBookImg.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bg_downBookImg_DoWork);
            this.bg_downBookImg.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bg_downBookImg_ProgressChanged);
            this.bg_downBookImg.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bg_downBookImg_RunWorkerCompleted);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(639, 262);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.ComponentModel.BackgroundWorker bg_worker;
        private System.ComponentModel.BackgroundWorker bg_downBookImg;
    }
}

