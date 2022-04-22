namespace JanuityUI
{
    partial class WeightCalibration
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WeightCalibration));
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.RegSpan = new System.Windows.Forms.Button();
            this.Ralibrate = new System.Windows.Forms.Button();
            this.RegOffset = new System.Windows.Forms.Button();
            this.loadTx = new System.Windows.Forms.TextBox();
            this.spantx = new System.Windows.Forms.TextBox();
            this.offsettx = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.WClose = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // RegSpan
            // 
            this.RegSpan.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(124)))), ((int)(((byte)(162)))));
            this.RegSpan.Font = new System.Drawing.Font("Times New Roman", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RegSpan.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.RegSpan.Location = new System.Drawing.Point(476, 295);
            this.RegSpan.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.RegSpan.Name = "RegSpan";
            this.RegSpan.Size = new System.Drawing.Size(147, 46);
            this.RegSpan.TabIndex = 21;
            this.RegSpan.Text = "Reg Span";
            this.RegSpan.UseVisualStyleBackColor = false;
            this.RegSpan.Click += new System.EventHandler(this.RegSpan_Click);
            // 
            // Ralibrate
            // 
            this.Ralibrate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(124)))), ((int)(((byte)(162)))));
            this.Ralibrate.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Ralibrate.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.Ralibrate.Location = new System.Drawing.Point(115, 481);
            this.Ralibrate.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Ralibrate.Name = "Ralibrate";
            this.Ralibrate.Size = new System.Drawing.Size(189, 48);
            this.Ralibrate.TabIndex = 20;
            this.Ralibrate.Text = "Calibrate";
            this.Ralibrate.UseVisualStyleBackColor = false;
            this.Ralibrate.Click += new System.EventHandler(this.Calibrate_Click);
            // 
            // RegOffset
            // 
            this.RegOffset.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(124)))), ((int)(((byte)(162)))));
            this.RegOffset.Font = new System.Drawing.Font("Times New Roman", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RegOffset.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.RegOffset.Location = new System.Drawing.Point(476, 185);
            this.RegOffset.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.RegOffset.Name = "RegOffset";
            this.RegOffset.Size = new System.Drawing.Size(147, 46);
            this.RegOffset.TabIndex = 19;
            this.RegOffset.Text = " Reg offset";
            this.RegOffset.UseVisualStyleBackColor = false;
            this.RegOffset.Click += new System.EventHandler(this.Regoffset_Click);
            // 
            // loadTx
            // 
            this.loadTx.BackColor = System.Drawing.Color.White;
            this.loadTx.Location = new System.Drawing.Point(291, 399);
            this.loadTx.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.loadTx.Name = "loadTx";
            this.loadTx.Size = new System.Drawing.Size(148, 26);
            this.loadTx.TabIndex = 18;
            // 
            // spantx
            // 
            this.spantx.BackColor = System.Drawing.Color.White;
            this.spantx.Location = new System.Drawing.Point(291, 306);
            this.spantx.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.spantx.Name = "spantx";
            this.spantx.Size = new System.Drawing.Size(148, 26);
            this.spantx.TabIndex = 17;
            // 
            // offsettx
            // 
            this.offsettx.BackColor = System.Drawing.Color.White;
            this.offsettx.Location = new System.Drawing.Point(291, 196);
            this.offsettx.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.offsettx.Name = "offsettx";
            this.offsettx.Size = new System.Drawing.Size(148, 26);
            this.offsettx.TabIndex = 16;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(286, 63);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(210, 26);
            this.label4.TabIndex = 15;
            this.label4.Text = "Weight Calculation";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(212, 398);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 26);
            this.label3.TabIndex = 14;
            this.label3.Text = "Load";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(212, 304);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 26);
            this.label2.TabIndex = 13;
            this.label2.Text = "Span";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(202, 196);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 26);
            this.label1.TabIndex = 12;
            this.label1.Text = "Offset";
            // 
            // WClose
            // 
            this.WClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(124)))), ((int)(((byte)(162)))));
            this.WClose.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.WClose.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.WClose.Location = new System.Drawing.Point(452, 481);
            this.WClose.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.WClose.Name = "WClose";
            this.WClose.Size = new System.Drawing.Size(189, 48);
            this.WClose.TabIndex = 23;
            this.WClose.Text = "Cancel";
            this.WClose.UseVisualStyleBackColor = false;
            this.WClose.Click += new System.EventHandler(this.WClose_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(235, 117);
            this.textBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(310, 26);
            this.textBox1.TabIndex = 24;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(447, 399);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 26);
            this.label5.TabIndex = 25;
            this.label5.Text = "LBs";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.label6.Location = new System.Drawing.Point(78, 249);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(612, 26);
            this.label6.TabIndex = 26;
            this.label6.Text = "Click \"Reg Offset\" without placing any weight in the kiosk";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.label7.Location = new System.Drawing.Point(80, 346);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(610, 26);
            this.label7.TabIndex = 27;
            this.label7.Text = "Click \"Reg Span\" after placing the known weight in Kiosk";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.label8.Location = new System.Drawing.Point(156, 440);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(467, 26);
            this.label8.TabIndex = 28;
            this.label8.Text = "Enter the lbs value of the known weight used";
            // 
            // WeightCalibration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(124)))), ((int)(((byte)(162)))));
            this.ClientSize = new System.Drawing.Size(782, 574);
            this.ControlBox = false;
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.WClose);
            this.Controls.Add(this.RegSpan);
            this.Controls.Add(this.Ralibrate);
            this.Controls.Add(this.RegOffset);
            this.Controls.Add(this.loadTx);
            this.Controls.Add(this.spantx);
            this.Controls.Add(this.offsettx);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "WeightCalibration";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "WeightCalibration";
            this.Load += new System.EventHandler(this.WeightCalibration_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.Button RegSpan;
        private System.Windows.Forms.Button Ralibrate;
        private System.Windows.Forms.Button RegOffset;
        private System.Windows.Forms.TextBox loadTx;
        private System.Windows.Forms.TextBox spantx;
        private System.Windows.Forms.TextBox offsettx;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button WClose;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
    }
}