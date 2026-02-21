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
            btnBrowseImage = new Button();
            pictureDigit = new PictureBox();
            label1 = new Label();
            lblDigit = new Label();
            groupBox1 = new GroupBox();
            radioBlackOnWhite = new RadioButton();
            radioWhiteOnBlack = new RadioButton();
            ((System.ComponentModel.ISupportInitialize)pictureDigit).BeginInit();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // btnBrowse
            // 
            btnBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowse.Location = new Point(344, 14);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(108, 26);
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
            txtFIle.Size = new Size(326, 23);
            txtFIle.TabIndex = 1;
            txtFIle.Text = "C:\\Users\\obinn\\source\\repos\\AI\\NeuralNetwork\\src\\_mnist_png\\training";
            // 
            // btnTrain
            // 
            btnTrain.Location = new Point(206, 46);
            btnTrain.Name = "btnTrain";
            btnTrain.Size = new Size(246, 26);
            btnTrain.TabIndex = 0;
            btnTrain.Text = "Start Training";
            btnTrain.UseVisualStyleBackColor = true;
            btnTrain.Click += BtnTrain_Click;
            // 
            // txtConsole
            // 
            txtConsole.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtConsole.Location = new Point(12, 149);
            txtConsole.Multiline = true;
            txtConsole.Name = "txtConsole";
            txtConsole.ScrollBars = ScrollBars.Both;
            txtConsole.Size = new Size(440, 180);
            txtConsole.TabIndex = 2;
            // 
            // btnRunInference
            // 
            btnRunInference.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnRunInference.Enabled = false;
            btnRunInference.Location = new Point(206, 119);
            btnRunInference.Name = "btnRunInference";
            btnRunInference.Size = new Size(111, 23);
            btnRunInference.TabIndex = 0;
            btnRunInference.Text = "Get Digit";
            btnRunInference.UseVisualStyleBackColor = true;
            btnRunInference.Click += BtnRunInference_Click;
            // 
            // btnBrowseImage
            // 
            btnBrowseImage.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnBrowseImage.Location = new Point(206, 90);
            btnBrowseImage.Name = "btnBrowseImage";
            btnBrowseImage.Size = new Size(111, 23);
            btnBrowseImage.TabIndex = 0;
            btnBrowseImage.Text = "Browse Image...";
            btnBrowseImage.UseVisualStyleBackColor = true;
            btnBrowseImage.Click += BtnBrowseImage_Click;
            // 
            // pictureDigit
            // 
            pictureDigit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            pictureDigit.BackColor = Color.White;
            pictureDigit.BorderStyle = BorderStyle.Fixed3D;
            pictureDigit.Location = new Point(323, 92);
            pictureDigit.Name = "pictureDigit";
            pictureDigit.Size = new Size(50, 50);
            pictureDigit.SizeMode = PictureBoxSizeMode.Zoom;
            pictureDigit.TabIndex = 3;
            pictureDigit.TabStop = false;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.Location = new Point(377, 102);
            label1.Name = "label1";
            label1.Size = new Size(21, 21);
            label1.TabIndex = 4;
            label1.Text = "=";
            // 
            // lblDigit
            // 
            lblDigit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblDigit.BackColor = Color.White;
            lblDigit.BorderStyle = BorderStyle.Fixed3D;
            lblDigit.Font = new Font("Segoe UI", 24F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblDigit.Location = new Point(402, 92);
            lblDigit.Name = "lblDigit";
            lblDigit.Size = new Size(50, 50);
            lblDigit.TabIndex = 5;
            lblDigit.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(radioBlackOnWhite);
            groupBox1.Controls.Add(radioWhiteOnBlack);
            groupBox1.Location = new Point(12, 43);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(188, 100);
            groupBox1.TabIndex = 6;
            groupBox1.TabStop = false;
            groupBox1.Text = "Image Mode:";
            // 
            // radioBlackOnWhite
            // 
            radioBlackOnWhite.AutoSize = true;
            radioBlackOnWhite.Location = new Point(17, 59);
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
            radioWhiteOnBlack.Location = new Point(17, 34);
            radioWhiteOnBlack.Name = "radioWhiteOnBlack";
            radioWhiteOnBlack.Size = new Size(106, 19);
            radioWhiteOnBlack.TabIndex = 0;
            radioWhiteOnBlack.TabStop = true;
            radioWhiteOnBlack.Text = "White On Black";
            radioWhiteOnBlack.UseVisualStyleBackColor = true;
            radioWhiteOnBlack.CheckedChanged += RadioBlackOnWhite_CheckedChanged;
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(464, 341);
            Controls.Add(groupBox1);
            Controls.Add(lblDigit);
            Controls.Add(label1);
            Controls.Add(pictureDigit);
            Controls.Add(txtConsole);
            Controls.Add(txtFIle);
            Controls.Add(btnBrowseImage);
            Controls.Add(btnRunInference);
            Controls.Add(btnTrain);
            Controls.Add(btnBrowse);
            Name = "FormMain";
            Text = "Image to Digit Converter";
            Load += FormMain_Load;
            ((System.ComponentModel.ISupportInitialize)pictureDigit).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnBrowse;
        private TextBox txtFIle;
        private Button btnTrain;
        private TextBox txtConsole;
        private Button btnRunInference;
        private Button btnBrowseImage;
        private PictureBox pictureDigit;
        private Label label1;
        private Label lblDigit;
        private GroupBox groupBox1;
        private RadioButton radioBlackOnWhite;
        private RadioButton radioWhiteOnBlack;
    }
}
