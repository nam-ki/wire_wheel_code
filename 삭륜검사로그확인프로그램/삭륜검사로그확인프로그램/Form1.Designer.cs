namespace 삭륜검사로그확인프로그램
{
    partial class Form1
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
            button1 = new Button();
            listBox1 = new ListBox();
            checkedListBox1 = new CheckedListBox();
            folderBrowserDialog1 = new FolderBrowserDialog();
            textBox1 = new TextBox();
            dataGridView1 = new DataGridView();
            checkedListBox2 = new CheckedListBox();
            button2 = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(1686, 51);
            button1.Name = "button1";
            button1.Size = new Size(183, 41);
            button1.TabIndex = 0;
            button1.Text = "Folder Open";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // listBox1
            // 
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 15;
            listBox1.Location = new Point(12, 299);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(120, 94);
            listBox1.TabIndex = 2;
            // 
            // checkedListBox1
            // 
            checkedListBox1.FormattingEnabled = true;
            checkedListBox1.Location = new Point(12, 153);
            checkedListBox1.Name = "checkedListBox1";
            checkedListBox1.Size = new Size(120, 94);
            checkedListBox1.TabIndex = 1;
            checkedListBox1.MouseDoubleClick += checkedListBox1_MouseDoubleClick;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(1413, 157);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(505, 613);
            textBox1.TabIndex = 3;
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(901, 157);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowTemplate.Height = 25;
            dataGridView1.Size = new Size(490, 613);
            dataGridView1.TabIndex = 4;
            // 
            // checkedListBox2
            // 
            checkedListBox2.FormattingEnabled = true;
            checkedListBox2.Location = new Point(12, 428);
            checkedListBox2.Name = "checkedListBox2";
            checkedListBox2.Size = new Size(120, 94);
            checkedListBox2.TabIndex = 5;
            checkedListBox2.MouseDoubleClick += checkedListBox2_MouseDoubleClick;
            // 
            // button2
            // 
            button2.Location = new Point(1413, 60);
            button2.Name = "button2";
            button2.Size = new Size(163, 42);
            button2.TabIndex = 6;
            button2.Text = "datagridview value";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1981, 824);
            Controls.Add(button2);
            Controls.Add(checkedListBox2);
            Controls.Add(dataGridView1);
            Controls.Add(textBox1);
            Controls.Add(listBox1);
            Controls.Add(checkedListBox1);
            Controls.Add(button1);
            Name = "Form1";
            Text = "Form1";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private ListBox listBox1;
        private CheckedListBox checkedListBox1;
        private FolderBrowserDialog folderBrowserDialog1;
        private TextBox textBox1;
        private DataGridView dataGridView1;
        private CheckedListBox checkedListBox2;
        private Button button2;
    }
}