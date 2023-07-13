using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;




namespace RGB_detect_algorithm
{
    public partial class Form1 : Form
    {

        string file_path;
        string file_name;
        Mat original_img = new Mat();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            file_path = System.IO.Directory.GetCurrentDirectory();

            openFileDialog1.InitialDirectory = file_path;

            openFileDialog1.Multiselect = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                file_name = openFileDialog1.FileName;
            }
            else
            {
                MessageBox.Show("파일이 선택되지 않았습니다.");
                Close();
            }

            //image read
            original_img = Cv2.ImRead(file_name);

            //image show in picturebox1
            pictureBox1.Image = BitmapConverter.ToBitmap(original_img);


        }
    }
}
