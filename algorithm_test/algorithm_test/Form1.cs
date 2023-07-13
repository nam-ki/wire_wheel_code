using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using static System.Net.Mime.MediaTypeNames;

namespace algorithm_test
{
    public partial class Form1 : Form
    {

        int block_size = 215;
        int c = 0;
        Mat binary_gray = new Mat();
        Mat binary = new Mat();
        Mat origin = new Mat();
        Mat gray = new Mat();
        OpenCvSharp.Point[][] contours_point;

        int canny_min = 0;
        int canny_max = 0;

        Mat embossing_img = new Mat();

        List<Rect> draw_rect_list = new List<Rect>();

        string filepath;
        Mat draw_img = new Mat();

        public Form1()
        {
            InitializeComponent();
            blocksize_val.Text = 3.ToString();
            c_val.Text = 0.ToString();

            blocksize_scr.Minimum = 3;
            blocksize_scr.Maximum = 5000 + 9;

            c_scr.Minimum = 0;
            c_scr.Maximum = 5000+9;

        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = "D:\\과제\\삭도검사로봇\\삭도 코드\\wire_wheel_code\\algorithm_test\\algorithm_test\\bin\\Debug";

            
        }

        private async void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            if (blocksize_scr.Value % 2 != 0)
            {


                blocksize_val.Text = blocksize_scr.Value.ToString();
                block_size = blocksize_scr.Value;
                c = c_scr.Value;
                Cv2.AdaptiveThreshold(gray, binary, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.Binary, blockSize: block_size, c: c);
                pictureBox3.Image = BitmapConverter.ToBitmap(binary);

                hough_func(binary);

                await Task.Run(() => calculate_label_area(binary));

                //contours_point = find_contour(binary);

                //await Task.Run(() => labeled_blob(contours_point));
            }
        }

        private async void c_scr_Scroll(object sender, ScrollEventArgs e)
        {
            if (blocksize_scr.Value % 2 != 0)
            {


                c_val.Text = c_scr.Value.ToString();
                block_size = blocksize_scr.Value;
                c = c_scr.Value;
                Cv2.AdaptiveThreshold(gray, binary, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.Binary, blockSize: block_size, c: c);
                pictureBox3.Image = BitmapConverter.ToBitmap(binary);

                hough_func(binary);

                await Task.Run(() => calculate_label_area(binary));

                //contours_point = find_contour(binary);

                //await Task.Run(() => labeled_blob(contours_point));
            }
        }

        //컨투어 찾는 함수
        private OpenCvSharp.Point[][] find_contour(Mat binary_img)
        {
            OpenCvSharp.Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(binary_img, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            return contours;
        }

        //찾은 컨투어로 이미지에 라벨링된 Blob 표시
        private void labeled_blob(OpenCvSharp.Point[][] contours_point)
        {
            Mat labeled = origin.Clone();
            Mat labeled_area = origin.Clone();


            foreach (OpenCvSharp.Point[] contour in contours_point)
            {
                //컨투어의 면적 계산
                double area = Cv2.ContourArea(contour);

                //라벨링된 blob의 경계선을 이미지에 그립니다.
                Cv2.DrawContours(labeled, new[] { contour }, -1, Scalar.Red, 2);
                Cv2.DrawContours(labeled_area, new[] { contour }, -1, Scalar.Red, 2);

                // Blob의 면적을 이미지에 표시합니다.
                Cv2.PutText(labeled_area, $"면적: {area}", contour[0], HersheyFonts.HersheyPlain, 1, Scalar.Blue, 2);
            }


            Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(5, 5));

            Mat erodedImage = new Mat();


            Cv2.Erode(labeled, erodedImage, kernel, iterations: 2);

            Mat erode_dilate_img = erodedImage.Clone();

            Cv2.Dilate(erodedImage, erode_dilate_img, kernel, iterations: 1);

            if (pictureBox4.InvokeRequired)
            {
                pictureBox4.Invoke(new MethodInvoker(delegate () { pictureBox4.Image = BitmapConverter.ToBitmap(labeled_area); }));
                pictureBox5.Invoke(new MethodInvoker(delegate () { pictureBox5.Image = BitmapConverter.ToBitmap(erodedImage); }));
                pictureBox6.Invoke(new MethodInvoker(delegate () { pictureBox6.Image = BitmapConverter.ToBitmap(erode_dilate_img); }));
            }
            else
            {
                pictureBox4.Image = BitmapConverter.ToBitmap(labeled_area);
                pictureBox5.Image = BitmapConverter.ToBitmap(erodedImage);
                pictureBox6.Image = BitmapConverter.ToBitmap(erode_dilate_img);
            }

        }

        //확률적 허프변환 함수
        private void hough_func(Mat binary_img)
        {

            //점 성분을 제거하기 위해 모폴로지를 통해 노이즈 성분을 최대한 제거.

            Mat binary_to_dilate = new Mat();
            Mat binary_to_dilate_to_erode = new Mat();
            Mat binary_to_dilate_to_erode_to_dilate = new Mat();
            Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(3, 3));

            Cv2.Dilate(binary_img, binary_to_dilate, kernel, new OpenCvSharp.Point(-1, -1), iterations: 1);
            Cv2.Erode(binary_to_dilate, binary_to_dilate_to_erode, kernel, new OpenCvSharp.Point(-1,-1), iterations: 1);
            Cv2.Dilate(binary_to_dilate_to_erode, binary_to_dilate_to_erode_to_dilate, kernel, new OpenCvSharp.Point(-1, -1), iterations: 1);

            // 에지 검출을 위해 Canny 알고리즘을 적용합니다.
            Mat edges = new Mat();
            //Cv2.Canny(binary_to_dilate, edges, canny_min, canny_max); //0,19
            Cv2.Canny(binary_img, edges, canny_min, canny_max); //0,19

            // 확률적 허프 변환을 수행하여 직선 검출
            LineSegmentPoint[] lines = Cv2.HoughLinesP(edges, 1, Cv2.PI / 180, 0, 70, 10);

            //확률적 허프 변환에서 직선 부분 저장
            List<LineSegmentPoint> horizontalLines = new List<LineSegmentPoint>();

            Mat line_img = origin.Clone();

            // 가로줄만 남기고 다른 선들을 제거합니다.
            foreach (LineSegmentPoint line in lines)
            {
                // 선의 기울기를 계산합니다.
                double angle = Math.Atan2(line.P2.Y - line.P1.Y, line.P2.X - line.P1.X) * 180.0 / Math.PI;

                // 기울기가 -45도에서 45도 사이인 경우에만 가로줄로 간주하고 그립니다.
                //if (angle > -30 && angle < 30)
                if (angle == 0)
                {
                    Cv2.Line(line_img, line.P1, line.P2, Scalar.Red, 2);
                    horizontalLines.Add(line);
                }
            }

            pictureBox9.Image = BitmapConverter.ToBitmap(edges);
            pictureBox6.Image = BitmapConverter.ToBitmap(line_img);

            Mat line_center_point_img = line_img.Clone();

            // 저장된 직선의 중심점 표시
            foreach (LineSegmentPoint line in horizontalLines)
            {
                OpenCvSharp.Point center = new OpenCvSharp.Point((line.P1.X + line.P2.X) / 2, (line.P1.Y + line.P2.Y) / 2);

                // 중심점 표시
                Cv2.Circle(line_center_point_img, center, 5, Scalar.Blue, -1);
            }

            // 결과 이미지 출력
            if (pictureBox7.InvokeRequired)
            {
                pictureBox4.Invoke(new MethodInvoker(delegate () { pictureBox4.Image = BitmapConverter.ToBitmap(binary_to_dilate); })); // canny edge 전 dilate
                pictureBox5.Invoke(new MethodInvoker(delegate () { pictureBox5.Image = BitmapConverter.ToBitmap(binary_to_dilate_to_erode); })); //canny edge 전 dilate,erode
                pictureBox11.Invoke(new MethodInvoker(delegate () { pictureBox11.Image = BitmapConverter.ToBitmap(binary_to_dilate_to_erode_to_dilate); })); //canny edge 전 dilate,erode,dilate
                pictureBox7.Invoke(new MethodInvoker(delegate () { pictureBox2.Image = BitmapConverter.ToBitmap(line_center_point_img); }));
            }
            else
            {
                pictureBox4.Image = BitmapConverter.ToBitmap(binary_to_dilate); // canny edge 전 dilate
                pictureBox5.Image = BitmapConverter.ToBitmap(binary_to_dilate_to_erode); //canny edge 전 dilate,erode
                pictureBox11.Image = BitmapConverter.ToBitmap(binary_to_dilate_to_erode_to_dilate); //canny edge 전 dilate,erode,dilate
                pictureBox7.Image = BitmapConverter.ToBitmap(line_center_point_img);
            }

        }

        private void canny_min_Scr_Scroll(object sender, ScrollEventArgs e)
        {
            canny_min_val.Text = canny_min_Scr.Value.ToString();

            canny_min = canny_min_Scr.Value;
            canny_max = canny_max_scr.Value;

            hough_func(binary);
        }

        private void canny_max_scr_Scroll(object sender, ScrollEventArgs e)
        {
            canny_max_val.Text = canny_max_scr.Value.ToString();

            canny_min = canny_min_Scr.Value;
            canny_max = canny_max_scr.Value;

            hough_func(binary);
        }

        #region 메소드 : 텍스트박스 표시
        private void WriteTextBox(TextBox tb, string txt)
        {
            try
            {
                tb.Invoke(new MethodInvoker(delegate () // 이부분 잘 모르겠음.
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

        //이미지 라벨 후 면적 계산
        private void calculate_label_area(Mat binary_img)
        {

            Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(3, 3));

            Mat dilateImage = new Mat();


            //Cv2.MorphologyEx(binary_img, dilateImage, MorphTypes.Open, kernel, iterations: 1);

            //Cv2.MorphologyEx(binary_img, dilateImage, MorphTypes.Dilate, kernel, iterations: 3); //파단

            Cv2.MorphologyEx(binary_img, dilateImage, MorphTypes.Dilate, kernel, iterations: 6); //파단

            //Cv2.MorphologyEx(binary_img, dilateImage, MorphTypes.Erode, kernel, iterations: 5);

            //Mat erode_dilate_img = erodedImage.Clone();

            //Cv2.Dilate(erodedImage, erode_dilate_img, kernel, iterations: 1);
            pictureBox13.Image = BitmapConverter.ToBitmap(dilateImage);

            Mat labels = new Mat();



            ////테스트를 위해 임의로 파단 부분을 roi로 설정.
            //Rect rect = new Rect(0, (origin.Height / 2) - 100, origin.Width, 300);
            //origin = origin.SubMat(rect);
            //binary_img = binary_img.SubMat(rect);

            int cnt = Cv2.ConnectedComponents(dilateImage, labels);

            //int cnt = Cv2.ConnectedComponents(binary_img, labels);




            Mat img2 = new Mat(origin.Size(), MatType.CV_8UC3);
            var random = new Random();

            

            //if (pictureBox4.InvokeRequired)
            //{
            //    pictureBox4.Invoke(new MethodInvoker(delegate () { pictureBox4.Image = BitmapConverter.ToBitmap(erodedImage); }));
            //    pictureBox5.Invoke(new MethodInvoker(delegate () { pictureBox5.Image = BitmapConverter.ToBitmap(erode_dilate_img); }));
                
            //}
            //else
            //{
            //    pictureBox4.Image = BitmapConverter.ToBitmap(erodedImage);
            //    pictureBox5.Image = BitmapConverter.ToBitmap(erode_dilate_img);
                
            //}

            //라벨별 면적 저장

            Dictionary<int,double> labeledAreas = new Dictionary<int,double>();

            int areaThreshold = 500; // Set the area threshold here
            int areaThreshold2 = 1000000;
            int count = 0;

            //파단 부분 포인트 리스트 저장
            List<OpenCvSharp.Point> damage_point_list = new List<OpenCvSharp.Point>();


            WriteTextBox(textBox1, "면적 계산 시작\r\n");
            for (int i = 0; i < cnt; i++)
            {
                byte[] color = new byte[3];
                for (int j = 0; j < 3; j++)
                {
                    color[j] = (byte)random.Next(0, 255);
                }

                Mat mask = new Mat();
                Cv2.Compare(labels, i, mask, CmpType.EQ);

                Cv2.BitwiseAnd(origin, origin, img2, mask);

                Moments moments = Cv2.Moments(mask, true);
                double area = moments.M00;

                if (area > areaThreshold && area < areaThreshold2)
                {
                    img2.SetTo(new Scalar(color[0], color[1], color[2]), mask);

                    int centerX = (int)(moments.M10 / moments.M00);
                    int centerY = (int)(moments.M01 / moments.M00);

                    int label = i;
                    string widthText = $" Width : {area}";

                    
                    WriteTextBox(textBox1, $"label:{label}" + widthText+"\r\n");
                    

                    OpenCvSharp.Point labelPosition = new OpenCvSharp.Point(centerX, centerY);
                    Cv2.PutText(img2, Convert.ToString(label), labelPosition, HersheyFonts.HersheyPlain, 1, Scalar.Red);

                    if (!labeledAreas.ContainsKey(label))
                    {
                        labeledAreas[label] = area;
                    }
                    
                    count++;

                }
            }
            WriteTextBox(textBox1, $"count={count}\r\n");
            WriteTextBox(textBox1, "면적 계산 끝\r\n");

            //if (pictureBox8.InvokeRequired)
            //{
            //    pictureBox8.Invoke(new MethodInvoker(delegate () { pictureBox8.Image = BitmapConverter.ToBitmap(img2); }));

            //}
            //else
            //{
            //    pictureBox8.Image = BitmapConverter.ToBitmap(img2);

            //}

            //면적들 모두 더한 다음 평균 구해서 평균보다 작은 라벨에 점 찍기


            // 면적 평균 계산
            double averageArea = CalculateAverageArea(labeledAreas);

            Mat point_img = img2.Clone();

            // 평균보다 작은 면적을 가진 라벨의 중심에 점 표시
            int count_point = 0;
            foreach (var item in labeledAreas)
            {
                int label = item.Key;
                double area = item.Value;

                if (area < averageArea/2)
                {
                    // 라벨의 중심 좌표 계산
                    OpenCvSharp.Point centroid = CalculateCentroid(labels, label);

                    // 이미지에 점 표시
                    Cv2.Circle(point_img, centroid, 5, Scalar.Red, -1);

                    damage_point_list.Add(centroid);

                    // textbox에 출력
                    WriteTextBox(textBox1, $"label:{label} width:{area}\r\n");

                    count_point++;
                }
            }

            WriteTextBox(textBox1, $"point_label_num = {count_point}\r\n");

            //포인트를 기점으로 사각형 형성

            Mat draw_rect = point_img.Clone();
            
            Rect rect_damage = new Rect();

            draw_rect_list.Clear();
            foreach (OpenCvSharp.Point point in damage_point_list)
            {
                OpenCvSharp.Point start_point = new OpenCvSharp.Point(point.X - 125, point.Y - 100);
                //OpenCvSharp.Point end_point = new OpenCvSharp.Point(point.X + 100, point.Y + 100);
                int size = 200;

                //if(point.X - 50 + 100 > origin.Width || point.Y - 100 + size > origin.Height)
                //{
                //    rect_damage = new Rect(start_point, new OpenCvSharp.Size(origin.Width - (point.X - 50), origin.Height - (point.Y - 100)));
                //}

                //else if(point.X-50+100 > origin.Width)
                //{
                //    rect_damage = new Rect(start_point, new OpenCvSharp.Size(origin.Width-(point.X-50), size));
                //}
                //else if(point.Y-100+size > origin.Height)
                //{
                //    rect_damage = new Rect(start_point, new OpenCvSharp.Size(100, origin.Height-(point.Y-100)));
                //}
                //else if(point.X - 50 < 0 || point.Y - 100  < 0)
                //{
                //    start_point = new OpenCvSharp.Point(0, 0);
                //    rect_damage = new Rect(start_point, new OpenCvSharp.Size(100, size));
                //}
                //else if (point.X - 50 < 0 )
                //{
                //    start_point = new OpenCvSharp.Point(0, point.Y-100);
                //    rect_damage = new Rect(start_point, new OpenCvSharp.Size(100, size));
                //}
                //else if (point.Y - 100 < 0)
                //{
                //    start_point = new OpenCvSharp.Point(point.X - 50, 0);
                //    rect_damage = new Rect(start_point, new OpenCvSharp.Size(100, size));
                //}
                //else
                //{
                //    rect_damage = new Rect(start_point, new OpenCvSharp.Size(100, size));
                //}
                rect_damage = new Rect(start_point, new OpenCvSharp.Size(size+50, size-50));

                Cv2.Rectangle(draw_rect, rect_damage, Scalar.Blue, 5, LineTypes.AntiAlias);

                draw_rect_list.Add(rect_damage);
            }

            ////사각형 부분 추출
            //Mat draw_img = new Mat();
            //Mat merge_img = new Mat();

            //bool check = false;
            //count = 0;
            //if(draw_rect_list.Count >= 2)
            //{
            //    merge_img = new Mat();
            //    int check_width = 0;
            //    int check_height = 0;
            //    foreach (Rect rect in draw_rect_list)
            //    {
                    
            //        if(count == 0) 
            //        {
                        
            //            merge_img = origin.SubMat(rect);
            //            count++;
            //            check_width = merge_img.Width;
            //            check_height = merge_img.Height;
            //        }
            //        else
            //        {
            //            draw_img = origin.SubMat(rect);
            //            Cv2.Resize(draw_img,draw_img,new OpenCvSharp.Size(check_width,check_height));
            //            Cv2.HConcat(new Mat[] { merge_img, draw_img },merge_img);
            //        }
            //    }
            //    check = true;
            //}
            //else if(draw_rect_list.Count == 1)
            //{
            //    merge_img = new Mat();
            //    merge_img = origin.SubMat(draw_rect_list[0]);

            //    check = true;
            //}
            

            
            
            if (pictureBox8.InvokeRequired)
            {
                pictureBox8.Invoke(new MethodInvoker(delegate () { pictureBox8.Image = BitmapConverter.ToBitmap(img2); }));
                pictureBox12.Invoke(new MethodInvoker(delegate () { pictureBox12.Image = BitmapConverter.ToBitmap(point_img); }));
                pictureBox14.Invoke(new MethodInvoker(delegate () { pictureBox14.Image = BitmapConverter.ToBitmap(draw_rect); }));
                
                
                
            }
            else
            {
                pictureBox8.Image = BitmapConverter.ToBitmap(img2);
                pictureBox12.Image = BitmapConverter.ToBitmap(point_img);
                pictureBox14.Image = BitmapConverter.ToBitmap(draw_rect);

                
                
                
            }

           //if(check == true)
           // {
           //     pictureBox15.Invoke(new MethodInvoker(delegate () { pictureBox15.Image = BitmapConverter.ToBitmap(merge_img); }));
           // }
       








        }

        #region 엠보싱 기법 적용

        //mat source는 grayscale
        private Mat Embossing_method(Mat source)
        {
            Mat result = new Mat();

            // 커널 생성
            float[,] kernelData = new float[,]
            {
        { -1, -1, 0 },
        { -1, 0, 1 },
        { 0, 1, 1 }
            };

            Mat kernel = new Mat(3, 3, MatType.CV_32F, kernelData);

            // 필터링 수행
            Cv2.Filter2D(source, result, MatType.CV_8U, kernel, new OpenCvSharp.Point(-1, -1), 128);

            return result;
        }
        #endregion

        #region image sharpening

        private Mat image_sharpening(Mat original_img)
        {
            Mat sharpening_image = original_img.Clone();

            float[] data = new float[9] { 0, -1, 0, -1, 5, -1, 0, -1, 0 };
            Mat kernel = new Mat(3, 3, MatType.CV_32F, data);

            Cv2.Filter2D(original_img, sharpening_image, original_img.Type(), kernel, new OpenCvSharp.Point(0, 0));

            pictureBox10.Image = BitmapConverter.ToBitmap(sharpening_image);

            return sharpening_image;
        }

        #endregion

        // 면적들의 평균 계산 함수
        static double CalculateAverageArea(Dictionary<int, double> labeledAreas)
        {
            double totalArea = 0;
            int labelCount = labeledAreas.Count;

            foreach (var area in labeledAreas.Values)
            {
                totalArea += area;
            }

            return (double)totalArea / labelCount;
        }

        // 라벨의 중심 좌표 계산 함수
        static OpenCvSharp.Point CalculateCentroid(Mat labels, int label)
        {

            Mat mask = new Mat();
            Cv2.Compare(labels, label, mask, CmpType.EQ);

            Moments moments = Cv2.Moments(mask, false);
            double m00 = moments.M00;

            int centerX = (int)(moments.M10 / moments.M00);
            int centerY = (int)(moments.M01 / moments.M00);

            return new OpenCvSharp.Point(centerX, centerY);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //사각형 부분 추출
            
            draw_img = origin.SubMat(draw_rect_list[0]);

            pictureBox15.Invoke(new MethodInvoker(delegate () { pictureBox15.Image = BitmapConverter.ToBitmap(draw_img); }));

            Mat draw_img_gray = new Mat();
            Mat draw_img_binary = new Mat();

            Cv2.CvtColor(draw_img, draw_img_gray, ColorConversionCodes.BGR2GRAY);
            Cv2.AdaptiveThreshold(draw_img_gray, draw_img_binary, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.BinaryInv, blockSize: block_size, c: c);

            //Cv2.Canny(draw_img, draw_img, 0, 0);

            pictureBox16.Invoke(new MethodInvoker(delegate () { pictureBox16.Image = BitmapConverter.ToBitmap(draw_img_gray); }));

        }

        private async void button1_Click(object sender, EventArgs e)
        {
            

            openFileDialog1.Multiselect = false;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //string fileName = Path.GetFileName(openFileDialog1.FileName);
                filepath = openFileDialog1.FileName;
                //image_path = filePath + "\\" + fileName;
                MessageBox.Show(filepath);
            }

            origin = Cv2.ImRead(filepath);




            //Cv2.Resize(origin, origin, new OpenCvSharp.Size(500, 500));
            pictureBox1.Image = BitmapConverter.ToBitmap(origin);

            //image sharpening
            Mat sharpening_img = new Mat();
            sharpening_img = image_sharpening(origin);
            pictureBox10.Image = BitmapConverter.ToBitmap(sharpening_img);



            //grayscale
            //Cv2.CvtColor(sharpening_img, gray, ColorConversionCodes.BGR2GRAY);

            Cv2.CvtColor(origin, gray, ColorConversionCodes.BGR2GRAY);

            pictureBox2.Image = BitmapConverter.ToBitmap(gray);

            //embossing_img = Embossing_method(gray);

            //pictureBox3.Image = BitmapConverter.ToBitmap(embossing_img);

            //await Task.Run(() => calculate_label_area(embossing_img));




            Cv2.AdaptiveThreshold(gray, binary, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.Binary, blockSize: block_size, c: c);
            pictureBox3.Image = BitmapConverter.ToBitmap(binary);

            hough_func(binary);

            await Task.Run(() => calculate_label_area(binary));
        }

        private void pictureBox15_DoubleClick(object sender, EventArgs e)
        {
            Cv2.ImWrite("find_crack.jpg", draw_img);
        }
    }
}
