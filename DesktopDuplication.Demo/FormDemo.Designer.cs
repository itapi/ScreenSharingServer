namespace DesktopDuplication.Demo
{
    partial class FormDemo
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
            this.UpdatedRegion = new System.Windows.Forms.Label();
            this.MovedRegion = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // UpdatedRegion
            // 
            this.UpdatedRegion.BackColor = System.Drawing.Color.Orange;
            this.UpdatedRegion.Location = new System.Drawing.Point(37, 109);
            this.UpdatedRegion.Name = "UpdatedRegion";
            this.UpdatedRegion.Size = new System.Drawing.Size(1, 1);
            this.UpdatedRegion.TabIndex = 0;
            // 
            // MovedRegion
            // 
            this.MovedRegion.BackColor = System.Drawing.Color.Purple;
            this.MovedRegion.Location = new System.Drawing.Point(308, 215);
            this.MovedRegion.Name = "MovedRegion";
            this.MovedRegion.Size = new System.Drawing.Size(1, 1);
            this.MovedRegion.TabIndex = 1;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(391, 357);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(117, 290);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(132, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "Start listening";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // FormDemo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(391, 357);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.MovedRegion);
            this.Controls.Add(this.UpdatedRegion);
            this.DoubleBuffered = true;
            this.Name = "FormDemo";
            this.Text = "Desktop Duplication API Demo";
            this.Load += new System.EventHandler(this.FormDemo_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label UpdatedRegion;
        private System.Windows.Forms.Label MovedRegion;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button button1;
    }
}

