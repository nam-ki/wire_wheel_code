using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing.Imaging;


namespace algirhtm_test2
{
    public partial class Form1 : Form
    {
        string filepath;
        Mat origin = new Mat();
        int blocksize_value=3;
        int c_value = 0;
        Mat gray = new Mat();
        Mat binary = new Mat();
        Mat edge = new Mat();

        static int b_upper = 0;
        static int g_upper = 0;
        static int r_upper = 0;
        static int b_lower = 0;
        static int g_lower = 0;
        static int r_lower = 0;

        static int h_upper = 0;
        static int s_upper = 0;
        static int v_upper = 0;
        static int h_lower = 0;
        static int s_lower = 0;
        static int v_lower = 0;


        static Scalar hsvLower_s = new Scalar(89,0,161); //(89,0,161)
        static Scalar hsvUpper_s = new Scalar(99, 255, 255);//(99,255,255)

        static Scalar bgrLower_s = new Scalar(255, 102, 255);
        static Scalar bgrUpper_s = new Scalar(255, 102, 255);


        InputArray hsvLower = InputArray.Create(hsvLower_s);
        InputArray hsvUpper = InputArray.Create(hsvUpper_s);
        InputArray bgrLower = InputArray.Create(bgrLower_s);
        InputArray bgrUpper = InputArray.Create(bgrUpper_s);

        Mat hsv = new Mat();
        Mat hsv_mask = new Mat();
        Mat hsv_result_opening = new Mat();

        BitmapData imageData;

        Mat hsv_result = new Mat();
        Mat erode_img = new Mat();

        public Form1()
        {
            InitializeComponent();
        }

        

        private void Form1_Load(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = "D:\\과제\\삭도검사로봇\\삭도 코드\\wire_wheel_code\\algirhtm_test2\\algirhtm_test2";

            textBox1.Text = 3.ToString();
            textBox2.Text = 0.ToString();

            hScrollBar1.Minimum = 3;
            hScrollBar1.Maximum = 5000 + 9;

            hScrollBar2.Minimum = 0;
            hScrollBar2.Maximum = 5000 + 9;


            //bgr 설정(upper)
            textBox3.Text = 0.ToString();
            textBox4.Text = 0.ToString();
            textBox5.Text = 0.ToString();

            hScrollBar3.Minimum = 0;
            hScrollBar3.Maximum = 255+9;

            hScrollBar4.Minimum = 0;
            hScrollBar4.Maximum = 255+9;

            hScrollBar5.Minimum = 0;
            hScrollBar5.Maximum = 255+9;

            //bgr 설정(lower)
            textBox6.Text = 0.ToString();
            textBox7.Text = 0.ToString();
            textBox8.Text = 0.ToString();

            hScrollBar6.Minimum = 0;
            hScrollBar6.Maximum = 255+9;

            hScrollBar7.Minimum = 0;
            hScrollBar7.Maximum = 255+9;

            hScrollBar8.Minimum = 0;
            hScrollBar8.Maximum = 255+9;

            //hsv 설정(upper)
            textBox9.Text = 0.ToString();
            textBox10.Text = 0.ToString();
            textBox11.Text = 0.ToString();

            hScrollBar9.Minimum = 0;
            hScrollBar9.Maximum = 255+9;

            hScrollBar10.Minimum = 0;
            hScrollBar10.Maximum = 255+9;

            hScrollBar11.Minimum = 0;
            hScrollBar11.Maximum = 179+9;

            //hsv 설정(lower)
            textBox12.Text = 0.ToString();
            textBox13.Text = 0.ToString();
            textBox14.Text = 0.ToString();

            hScrollBar12.Minimum = 0;
            hScrollBar12.Maximum = 255+9;

            hScrollBar13.Minimum = 0;
            hScrollBar13.Maximum = 255+9;

            hScrollBar14.Minimum = 0;
            hScrollBar14.Maximum = 179+9;





        }

        private void button1_Click(object sender, EventArgs e)
        {
            





            openFileDialog1.Multiselect = false;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                
                filepath = openFileDialog1.FileName;
                
                MessageBox.Show(filepath);
            }

            origin = Cv2.ImRead(filepath);

            pictureBox1.Image = BitmapConverter.ToBitmap(origin);

            //그레이 스케일
            
            Cv2.CvtColor(origin, gray, ColorConversionCodes.BGR2GRAY);

            pictureBox2.Image = BitmapConverter.ToBitmap(gray);

            algorithm(gray);

            YCrCb_algorithm(origin);
        }


        private async void algorithm(Mat gray_img)
        {
            

            //adaptvie binary
            

            Cv2.AdaptiveThreshold(gray_img, binary, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.Binary, blockSize: blocksize_value, c: c_value);

            //canny edge
            
            Cv2.Canny(binary, edge, 0, 0);

            //mopology_erode

            

            Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(3, 3));

            Cv2.MorphologyEx(binary, erode_img, MorphTypes.Erode, kernel, iterations: 1);

            //윤곽선
            OpenCvSharp.Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(binary, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);


            //윤곽선 그리기
            Mat contour_image = origin.Clone();
            
            foreach (OpenCvSharp.Point[] contour in contours)
            {
               
                Cv2.DrawContours(contour_image, new[] { contour }, -1, Scalar.Red, 2);
                
            }

            pictureBox3.Invoke(new MethodInvoker(delegate () { pictureBox3.Image = BitmapConverter.ToBitmap(binary); }));
            pictureBox4.Invoke(new MethodInvoker(delegate () { pictureBox4.Image = BitmapConverter.ToBitmap(edge); }));
            pictureBox5.Invoke(new MethodInvoker(delegate () { pictureBox5.Image = BitmapConverter.ToBitmap(erode_img); }));
            pictureBox6.Invoke(new MethodInvoker(delegate () { pictureBox6.Image = BitmapConverter.ToBitmap(contour_image); }));

            //pictureBox3.Image = BitmapConverter.ToBitmap(binary);
            //pictureBox4.Image = BitmapConverter.ToBitmap(edge);
            //pictureBox5.Image = BitmapConverter.ToBitmap(erode_img);
            //pictureBox6.Image = BitmapConverter.ToBitmap(contour_image);

            await Task.Run(() => { calculate_label_area(binary); });



        }

        private async void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            if (hScrollBar1.Value % 2 != 0)
            {


                textBox1.Text = hScrollBar1.Value.ToString();
                blocksize_value = hScrollBar1.Value;
                

                await Task.Run(() => { algorithm(gray); });
                
            }
        }

        private async void hScrollBar2_Scroll(object sender, ScrollEventArgs e)
        {
            if (hScrollBar1.Value % 2 != 0)
            {


                textBox2.Text = hScrollBar2.Value.ToString();
                
                c_value = hScrollBar2.Value;

                await Task.Run(() => { algorithm(gray); });

            }
        }

        //라벨링
        private void calculate_label_area(Mat binary_img)
        {


            //Mat img_labels = new Mat();
            //Mat stats = new Mat();
            //Mat centroids = new Mat();
            //int numOfLables = Cv2.ConnectedComponentsWithStats(binary_img, img_labels, stats, centroids);

            
            //Mat result = origin.Clone();

            //// 레이블에 맞게 색상 설정
            //Scalar[] labelColors = new Scalar[numOfLables];
            //for (int label = 0; label < numOfLables; label++)
            //{
            //    labelColors[label] = Scalar.RandomColor();
            //}

            //// 색칠된 이미지 생성
            
            
            //for (int y = 0; y < result.Rows; y++)
            //{
            //    for (int x = 0; x < result.Cols; x++)
            //    {
            //        int label = img_labels.At<int>(x, y);
            //        if (label > 0)
            //        {
            //            result.Set(y, x, labelColors[label]);
            //        }
            //    }
            //}


            ////for (int label = 1; label < numOfLables; label++)
            ////{
            ////    int left = stats.At<int>(label, (int)ConnectedComponentsTypes.Left);
            ////    int top = stats.At<int>(label, (int)ConnectedComponentsTypes.Top);
            ////    int width = stats.At<int>(label, (int)ConnectedComponentsTypes.Width);
            ////    int height = stats.At<int>(label, (int)ConnectedComponentsTypes.Height);
            ////    Cv2.Rectangle(result, new Rect(left, top, width, height), Scalar.Red, 2);
            ////}

            //pictureBox7.Invoke(new MethodInvoker(delegate () { pictureBox7.Image = BitmapConverter.ToBitmap(result); }));

        }


        //색차이 알고리즘 작성(YCrCb 사용)
        private void YCrCb_algorithm(Mat color_img)
        {
            

            //hsv로 변경

            Cv2.CvtColor(color_img, hsv, ColorConversionCodes.BGR2HSV);



            if (pictureBox7.InvokeRequired)
            {
                pictureBox7.Invoke(new MethodInvoker(delegate () { pictureBox7.Image = BitmapConverter.ToBitmap(hsv); }));
            }
            else
            {
                pictureBox7.Image = BitmapConverter.ToBitmap(hsv);
            }




            

            

            Cv2.InRange(hsv, hsvLower_s, hsvUpper_s, hsv_mask);

            if (pictureBox8.InvokeRequired)
            {
                pictureBox8.Invoke(new MethodInvoker(delegate () { pictureBox8.Image = BitmapConverter.ToBitmap(hsv_mask); }));
            }
            else
            {
                pictureBox8.Image = BitmapConverter.ToBitmap(hsv_mask);
            }

            

            //Cv2.BitwiseAnd(color_img, hsv_mask.CvtColor(ColorConversionCodes.GRAY2BGR), hsv_result);

            Cv2.BitwiseAnd(hsv,hsv, hsv_result, mask: hsv_mask);

            Cv2.CvtColor(hsv_result, hsv_result, ColorConversionCodes.HSV2BGR);


            if (pictureBox9.InvokeRequired)
            {
                pictureBox9.Invoke(new MethodInvoker(delegate () { pictureBox9.Image = BitmapConverter.ToBitmap(hsv_result); }));
            }
            else
            {
                pictureBox9.Image = BitmapConverter.ToBitmap(hsv_result);
            }





            //Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(5, 5));

            //Cv2.MorphologyEx(hsv_result, hsv_result_opening, MorphTypes.Open, kernel, iterations: 3);

            //if (pictureBox10.InvokeRequired)
            //{
            //    pictureBox10.Invoke(new MethodInvoker(delegate () { pictureBox10.Image = BitmapConverter.ToBitmap(hsv_result_opening); }));
            //}
            //else
            //{
            //    pictureBox10.Image = BitmapConverter.ToBitmap(hsv_result_opening);
            //}



        }

        private void dispose_algorithm()
        {
            //List<PictureBox> picturebox_list = new List<PictureBox>(4) { pictureBox7, pictureBox8, pictureBox9, pictureBox10 };

            //foreach(PictureBox picturebox in picturebox_list)
            //{

            //    if (picturebox.InvokeRequired)
            //    {
            //        picturebox.Invoke(new MethodInvoker(delegate () { picturebox.Dispose(); }));
            //    }
            //    else
            //    {
            //        picturebox.Dispose();
            //    }

                

            //}
        }

        //b_upper
        private async void hScrollBar3_Scroll(object sender, ScrollEventArgs e)
        {
            b_upper = hScrollBar3.Value;
            textBox3.Text = b_upper.ToString();

            bgrUpper_s = new Scalar(b_upper, g_upper, r_upper);

            bgrUpper = InputArray.Create(bgrUpper_s);

            await Task.Run(() => dispose_algorithm());

            await Task.Run(() => YCrCb_algorithm(origin));
        }

        //g_upper
        private async void hScrollBar4_Scroll(object sender, ScrollEventArgs e)
        {
            g_upper = hScrollBar4.Value;
            textBox4.Text = g_upper.ToString();

            bgrUpper_s = new Scalar(b_upper, g_upper, r_upper);

            bgrUpper = InputArray.Create(bgrUpper_s);

            await Task.Run(() => dispose_algorithm());

            await Task.Run(() => YCrCb_algorithm(origin));
        }

        //r_upper
        private async void hScrollBar5_Scroll(object sender, ScrollEventArgs e)
        {
            r_upper = hScrollBar5.Value;
            textBox5.Text = r_upper.ToString();

            bgrUpper_s = new Scalar(b_upper, g_upper, r_upper);

            bgrUpper = InputArray.Create(bgrUpper_s);

            await Task.Run(() => dispose_algorithm());

            await Task.Run(() => YCrCb_algorithm(origin));
        }

        //b_lower
        private async void hScrollBar8_Scroll(object sender, ScrollEventArgs e)
        {
            b_lower = hScrollBar8.Value;
            textBox8.Text = b_lower.ToString();

            bgrLower_s = new Scalar(b_lower, g_lower, r_lower);

            bgrLower = InputArray.Create(bgrLower_s);

            

            await Task.Run(() => YCrCb_algorithm(origin));
        }

        //g_lower
        private async void hScrollBar7_Scroll(object sender, ScrollEventArgs e)
        {
            g_lower = hScrollBar7.Value;
            textBox7.Text = g_lower.ToString();

            bgrLower_s = new Scalar(b_lower, g_lower, r_lower);

            bgrLower = InputArray.Create(bgrLower_s);

            

            await Task.Run(() => YCrCb_algorithm(origin));
        }

        //r_lower
        private async void hScrollBar6_Scroll(object sender, ScrollEventArgs e)
        {
            r_lower = hScrollBar6.Value;

            textBox6.Text = r_lower.ToString();

            bgrLower_s = new Scalar(b_lower, g_lower, r_lower);

            bgrLower = InputArray.Create(bgrLower_s);

            

            await Task.Run(() => YCrCb_algorithm(origin));
        }

        //h_upper
        private async void hScrollBar14_Scroll(object sender, ScrollEventArgs e)
        {
            h_upper = hScrollBar14.Value;

            textBox14.Text = h_upper.ToString();

            hsvUpper_s = new Scalar(h_upper, s_upper, v_upper);

            hsvUpper = InputArray.Create(hsvUpper_s);

            

            await Task.Run(() => YCrCb_algorithm(origin));

        }

        //s_upper
        private async void hScrollBar13_Scroll(object sender, ScrollEventArgs e)
        {
            s_upper = hScrollBar13.Value;

            textBox13.Text = s_upper.ToString();


            hsvUpper_s = new Scalar(h_upper, s_upper, v_upper);

            hsvUpper = InputArray.Create(hsvUpper_s);

            

            await Task.Run(() => YCrCb_algorithm(origin));
        }

        //v_upper
        private async void hScrollBar12_Scroll(object sender, ScrollEventArgs e)
        {
            v_upper = hScrollBar12.Value;

            textBox12.Text = v_upper.ToString();

            hsvUpper_s = new Scalar(h_upper, s_upper, v_upper);

            hsvUpper = InputArray.Create(hsvUpper_s);

           

            await Task.Run(() => YCrCb_algorithm(origin));
        }

        //h_lower
        private async void hScrollBar11_Scroll(object sender, ScrollEventArgs e)
        {
            h_lower = hScrollBar11.Value;

            textBox11.Text = h_lower.ToString();

            hsvLower_s = new Scalar(h_lower, s_lower, v_lower);

            hsvLower = InputArray.Create(hsvLower_s);

            

            await Task.Run(() => YCrCb_algorithm(origin));
        }

        //s_lower
        private async void hScrollBar10_Scroll(object sender, ScrollEventArgs e)
        {
            s_lower = hScrollBar10.Value;

            textBox10.Text = s_lower.ToString();

            hsvLower_s = new Scalar(h_lower, s_lower, v_lower);

            hsvLower = InputArray.Create(hsvLower_s);

            

            await Task.Run(() => YCrCb_algorithm(origin));
        }

        //v_lower
        private async void hScrollBar9_Scroll(object sender, ScrollEventArgs e)
        {
            v_lower = hScrollBar9.Value;

            textBox9.Text = v_lower.ToString();

            hsvLower_s = new Scalar(h_lower, s_lower, v_lower);

            hsvLower = InputArray.Create(hsvLower_s);

           

            await Task.Run(() => YCrCb_algorithm(origin));
        }

        

        #region 메소드 : 텍스트박스 표시
        private void WriteTextBox(TextBox tb, string txt)
        {
            try
            {
                tb.Invoke(new MethodInvoker(delegate () 
                {
                    tb.AppendText(txt); //텍스트 상자 tb에 txt를 추가합니다.
                    tb.ScrollToCaret(); //마지막 라인으로 스크롤하기.
                }));
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        #endregion

        
    }
}
