namespace PriceTagTagger
{
    partial class FormMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripSplitButtonLoad = new System.Windows.Forms.ToolStripSplitButton();
            this.toolStripProgressBarLoading = new System.Windows.Forms.ToolStripProgressBar();
            this.backgroundWorkerLoadImage = new System.ComponentModel.BackgroundWorker();
            this.timerClear = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(881, 669);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.statusStrip1);
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(881, 669);
            this.panel1.TabIndex = 2;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSplitButtonLoad,
            this.toolStripProgressBarLoading});
            this.statusStrip1.Location = new System.Drawing.Point(0, 647);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(881, 22);
            this.statusStrip1.TabIndex = 4;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripSplitButtonLoad
            // 
            this.toolStripSplitButtonLoad.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripSplitButtonLoad.Image = ((System.Drawing.Image)(resources.GetObject("toolStripSplitButtonLoad.Image")));
            this.toolStripSplitButtonLoad.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSplitButtonLoad.Name = "toolStripSplitButtonLoad";
            this.toolStripSplitButtonLoad.Size = new System.Drawing.Size(74, 20);
            this.toolStripSplitButtonLoad.Text = "Load next";
            this.toolStripSplitButtonLoad.ButtonClick += new System.EventHandler(this.toolStripSplitButtonLoad_ButtonClick);
            // 
            // toolStripProgressBarLoading
            // 
            this.toolStripProgressBarLoading.Name = "toolStripProgressBarLoading";
            this.toolStripProgressBarLoading.Size = new System.Drawing.Size(100, 16);
            // 
            // backgroundWorkerLoadImage
            // 
            this.backgroundWorkerLoadImage.WorkerReportsProgress = true;
            this.backgroundWorkerLoadImage.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerLoadImage_DoWork);
            this.backgroundWorkerLoadImage.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorkerLoadImage_ProgressChanged);
            this.backgroundWorkerLoadImage.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerLoadImage_RunWorkerCompleted);
            // 
            // timerClear
            // 
            this.timerClear.Interval = 1000;
            this.timerClear.Tick += new System.EventHandler(this.timerClear_Tick);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(881, 669);
            this.Controls.Add(this.panel1);
            this.Name = "FormMain";
            this.Text = "Cascade Test";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitButtonLoad;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBarLoading;
        private System.ComponentModel.BackgroundWorker backgroundWorkerLoadImage;
        private System.Windows.Forms.Timer timerClear;
    }
}

