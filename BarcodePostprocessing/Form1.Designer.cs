namespace BarcodePostprocessing
{
    partial class Form1
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
            this.lstInputFiles = new System.Windows.Forms.ListBox();
            this.btnAddInputFile = new System.Windows.Forms.Button();
            this.lblInputFiles = new System.Windows.Forms.Label();
            this.btnMergeAndSave = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtMergedDataInput = new System.Windows.Forms.TextBox();
            this.btnOpenMergedData = new System.Windows.Forms.Button();
            this.btnOpenOfficialData = new System.Windows.Forms.Button();
            this.txtOfficialDataInput = new System.Windows.Forms.TextBox();
            this.btnCompareAndSave = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstInputFiles
            // 
            this.lstInputFiles.FormattingEnabled = true;
            this.lstInputFiles.ItemHeight = 20;
            this.lstInputFiles.Location = new System.Drawing.Point(41, 51);
            this.lstInputFiles.Name = "lstInputFiles";
            this.lstInputFiles.Size = new System.Drawing.Size(687, 164);
            this.lstInputFiles.TabIndex = 0;
            // 
            // btnAddInputFile
            // 
            this.btnAddInputFile.Location = new System.Drawing.Point(734, 51);
            this.btnAddInputFile.Name = "btnAddInputFile";
            this.btnAddInputFile.Size = new System.Drawing.Size(158, 72);
            this.btnAddInputFile.TabIndex = 1;
            this.btnAddInputFile.Text = "Add Input File...";
            this.btnAddInputFile.UseVisualStyleBackColor = true;
            this.btnAddInputFile.Click += new System.EventHandler(this.btnAddInputFile_Click);
            // 
            // lblInputFiles
            // 
            this.lblInputFiles.AutoSize = true;
            this.lblInputFiles.Location = new System.Drawing.Point(37, 13);
            this.lblInputFiles.Name = "lblInputFiles";
            this.lblInputFiles.Size = new System.Drawing.Size(192, 20);
            this.lblInputFiles.TabIndex = 2;
            this.lblInputFiles.Text = "Select input files to merge";
            // 
            // btnMergeAndSave
            // 
            this.btnMergeAndSave.Location = new System.Drawing.Point(734, 148);
            this.btnMergeAndSave.Name = "btnMergeAndSave";
            this.btnMergeAndSave.Size = new System.Drawing.Size(158, 67);
            this.btnMergeAndSave.TabIndex = 4;
            this.btnMergeAndSave.Text = "Merge and save as...";
            this.btnMergeAndSave.UseVisualStyleBackColor = true;
            this.btnMergeAndSave.Click += new System.EventHandler(this.btnMergeAndSave_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(41, 258);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(148, 20);
            this.label1.TabIndex = 5;
            this.label1.Text = "Compare with stock";
            // 
            // txtMergedDataInput
            // 
            this.txtMergedDataInput.Location = new System.Drawing.Point(41, 300);
            this.txtMergedDataInput.Name = "txtMergedDataInput";
            this.txtMergedDataInput.Size = new System.Drawing.Size(533, 26);
            this.txtMergedDataInput.TabIndex = 6;
            // 
            // btnOpenMergedData
            // 
            this.btnOpenMergedData.Location = new System.Drawing.Point(580, 296);
            this.btnOpenMergedData.Name = "btnOpenMergedData";
            this.btnOpenMergedData.Size = new System.Drawing.Size(148, 34);
            this.btnOpenMergedData.TabIndex = 7;
            this.btnOpenMergedData.Text = "Merged data...";
            this.btnOpenMergedData.UseVisualStyleBackColor = true;
            this.btnOpenMergedData.Click += new System.EventHandler(this.btnOpenMergedData_Click);
            // 
            // btnOpenOfficialData
            // 
            this.btnOpenOfficialData.Location = new System.Drawing.Point(580, 336);
            this.btnOpenOfficialData.Name = "btnOpenOfficialData";
            this.btnOpenOfficialData.Size = new System.Drawing.Size(148, 34);
            this.btnOpenOfficialData.TabIndex = 9;
            this.btnOpenOfficialData.Text = "Stock data...";
            this.btnOpenOfficialData.UseVisualStyleBackColor = true;
            this.btnOpenOfficialData.Click += new System.EventHandler(this.btnOpenOfficialData_Click);
            // 
            // txtOfficialDataInput
            // 
            this.txtOfficialDataInput.Location = new System.Drawing.Point(41, 340);
            this.txtOfficialDataInput.Name = "txtOfficialDataInput";
            this.txtOfficialDataInput.Size = new System.Drawing.Size(533, 26);
            this.txtOfficialDataInput.TabIndex = 8;
            // 
            // btnCompareAndSave
            // 
            this.btnCompareAndSave.Location = new System.Drawing.Point(734, 296);
            this.btnCompareAndSave.Name = "btnCompareAndSave";
            this.btnCompareAndSave.Size = new System.Drawing.Size(158, 74);
            this.btnCompareAndSave.TabIndex = 10;
            this.btnCompareAndSave.Text = "Compare and save as...";
            this.btnCompareAndSave.UseVisualStyleBackColor = true;
            this.btnCompareAndSave.Click += new System.EventHandler(this.btnCompareAndSave_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel3,
            this.toolStripStatusLabel2});
            this.statusStrip1.Location = new System.Drawing.Point(0, 414);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(917, 30);
            this.statusStrip1.TabIndex = 11;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(157, 25);
            this.toolStripStatusLabel1.Text = "(c) Christoph 2016";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(153, 25);
            this.toolStripStatusLabel2.Text = "v 0.1 (30.12.2016)";
            // 
            // toolStripStatusLabel3
            // 
            this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            this.toolStripStatusLabel3.Size = new System.Drawing.Size(16, 25);
            this.toolStripStatusLabel3.Text = "|";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(917, 444);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.btnCompareAndSave);
            this.Controls.Add(this.btnOpenOfficialData);
            this.Controls.Add(this.txtOfficialDataInput);
            this.Controls.Add(this.btnOpenMergedData);
            this.Controls.Add(this.txtMergedDataInput);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnMergeAndSave);
            this.Controls.Add(this.lblInputFiles);
            this.Controls.Add(this.btnAddInputFile);
            this.Controls.Add(this.lstInputFiles);
            this.Name = "Form1";
            this.Text = "Barcode Postprocessing";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lstInputFiles;
        private System.Windows.Forms.Button btnAddInputFile;
        private System.Windows.Forms.Label lblInputFiles;
        private System.Windows.Forms.Button btnMergeAndSave;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtMergedDataInput;
        private System.Windows.Forms.Button btnOpenMergedData;
        private System.Windows.Forms.Button btnOpenOfficialData;
        private System.Windows.Forms.TextBox txtOfficialDataInput;
        private System.Windows.Forms.Button btnCompareAndSave;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel3;
    }
}

