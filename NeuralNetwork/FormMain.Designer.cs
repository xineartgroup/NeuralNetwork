namespace NeuralNetwork
{
    partial class FormMain
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
            btnBrowse = new Button();
            txtFIle = new TextBox();
            btnTrain = new Button();
            txtConsole = new TextBox();
            btnRunInference = new Button();
            pictureDigit = new PictureBox();
            lblDigit = new Label();
            groupBox1 = new GroupBox();
            radioBlackOnWhite = new RadioButton();
            radioWhiteOnBlack = new RadioButton();
            btnStopTraining = new Button();
            nEpochs = new NumericUpDown();
            label1 = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureDigit).BeginInit();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nEpochs).BeginInit();
            SuspendLayout();
            // 
            // btnBrowse
            // 
            btnBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowse.Location = new Point(581, 14);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(140, 26);
            btnBrowse.TabIndex = 0;
            btnBrowse.Text = "Browse Model...";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += BtnBrowse_Click;
            // 
            // txtFIle
            // 
            txtFIle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtFIle.Location = new Point(12, 14);
            txtFIle.Name = "txtFIle";
            txtFIle.Size = new Size(560, 23);
            txtFIle.TabIndex = 1;
            txtFIle.Text = "C:\\Users\\obinn\\source\\repos\\AI\\NeuralNetwork\\src\\_mnist_png";
            // 
            // btnTrain
            // 
            btnTrain.Location = new Point(161, 46);
            btnTrain.Name = "btnTrain";
            btnTrain.Size = new Size(141, 26);
            btnTrain.TabIndex = 0;
            btnTrain.Text = "Start Training";
            btnTrain.UseVisualStyleBackColor = true;
            btnTrain.Click += BtnTrain_Click;
            // 
            // txtConsole
            // 
            txtConsole.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtConsole.Location = new Point(12, 172);
            txtConsole.Multiline = true;
            txtConsole.Name = "txtConsole";
            txtConsole.ScrollBars = ScrollBars.Both;
            txtConsole.Size = new Size(710, 277);
            txtConsole.TabIndex = 2;
            // 
            // btnRunInference
            // 
            btnRunInference.Enabled = false;
            btnRunInference.FlatStyle = FlatStyle.Popup;
            btnRunInference.Location = new Point(216, 125);
            btnRunInference.Name = "btnRunInference";
            btnRunInference.Size = new Size(28, 28);
            btnRunInference.TabIndex = 0;
            btnRunInference.Text = "=";
            btnRunInference.UseVisualStyleBackColor = true;
            btnRunInference.Click += BtnRunInference_Click;
            // 
            // pictureDigit
            // 
            pictureDigit.BackColor = Color.White;
            pictureDigit.BorderStyle = BorderStyle.Fixed3D;
            pictureDigit.Location = new Point(160, 113);
            pictureDigit.Name = "pictureDigit";
            pictureDigit.Size = new Size(50, 50);
            pictureDigit.SizeMode = PictureBoxSizeMode.Zoom;
            pictureDigit.TabIndex = 3;
            pictureDigit.TabStop = false;
            pictureDigit.Click += BtnBrowseImage_Click;
            // 
            // lblDigit
            // 
            lblDigit.BackColor = Color.White;
            lblDigit.BorderStyle = BorderStyle.Fixed3D;
            lblDigit.Font = new Font("Segoe UI", 24F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblDigit.Location = new Point(250, 113);
            lblDigit.Name = "lblDigit";
            lblDigit.Size = new Size(50, 50);
            lblDigit.TabIndex = 5;
            lblDigit.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(radioBlackOnWhite);
            groupBox1.Controls.Add(radioWhiteOnBlack);
            groupBox1.Location = new Point(12, 78);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(143, 85);
            groupBox1.TabIndex = 6;
            groupBox1.TabStop = false;
            groupBox1.Text = "Image Mode:";
            // 
            // radioBlackOnWhite
            // 
            radioBlackOnWhite.AutoSize = true;
            radioBlackOnWhite.Location = new Point(18, 47);
            radioBlackOnWhite.Name = "radioBlackOnWhite";
            radioBlackOnWhite.Size = new Size(106, 19);
            radioBlackOnWhite.TabIndex = 0;
            radioBlackOnWhite.TabStop = true;
            radioBlackOnWhite.Text = "Black On White";
            radioBlackOnWhite.UseVisualStyleBackColor = true;
            radioBlackOnWhite.CheckedChanged += RadioBlackOnWhite_CheckedChanged;
            // 
            // radioWhiteOnBlack
            // 
            radioWhiteOnBlack.AutoSize = true;
            radioWhiteOnBlack.Location = new Point(18, 22);
            radioWhiteOnBlack.Name = "radioWhiteOnBlack";
            radioWhiteOnBlack.Size = new Size(106, 19);
            radioWhiteOnBlack.TabIndex = 0;
            radioWhiteOnBlack.TabStop = true;
            radioWhiteOnBlack.Text = "White On Black";
            radioWhiteOnBlack.UseVisualStyleBackColor = true;
            radioWhiteOnBlack.CheckedChanged += RadioBlackOnWhite_CheckedChanged;
            // 
            // btnStopTraining
            // 
            btnStopTraining.Enabled = false;
            btnStopTraining.Location = new Point(160, 75);
            btnStopTraining.Name = "btnStopTraining";
            btnStopTraining.Size = new Size(141, 26);
            btnStopTraining.TabIndex = 0;
            btnStopTraining.Text = "Stop Training";
            btnStopTraining.UseVisualStyleBackColor = true;
            btnStopTraining.Click += BtnStopTraining_Click;
            // 
            // nEpochs
            // 
            nEpochs.Location = new Point(105, 46);
            nEpochs.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nEpochs.Name = "nEpochs";
            nEpochs.Size = new Size(50, 23);
            nEpochs.TabIndex = 7;
            nEpochs.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(54, 48);
            label1.Name = "label1";
            label1.Size = new Size(45, 15);
            label1.TabIndex = 8;
            label1.Text = "Epochs";
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(734, 461);
            Controls.Add(label1);
            Controls.Add(nEpochs);
            Controls.Add(groupBox1);
            Controls.Add(lblDigit);
            Controls.Add(pictureDigit);
            Controls.Add(txtConsole);
            Controls.Add(txtFIle);
            Controls.Add(btnRunInference);
            Controls.Add(btnStopTraining);
            Controls.Add(btnTrain);
            Controls.Add(btnBrowse);
            Name = "FormMain";
            Text = "Image to Digit Converter";
            FormClosing += FormMain_FormClosing;
            Load += FormMain_Load;
            ((System.ComponentModel.ISupportInitialize)pictureDigit).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nEpochs).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnBrowse;
        private TextBox txtFIle;
        private Button btnTrain;
        private TextBox txtConsole;
        private Button btnRunInference;
        private PictureBox pictureDigit;
        private Label lblDigit;
        private GroupBox groupBox1;
        private RadioButton radioBlackOnWhite;
        private RadioButton radioWhiteOnBlack;
        private Button btnStopTraining;
        private NumericUpDown nEpochs;
        private Label label1;
    }
}
