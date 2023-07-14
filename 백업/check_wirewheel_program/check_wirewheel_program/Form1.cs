using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Media.TextFormatting;
using KIRO;

namespace check_wirewheel_program
{
    public partial class Form1 : Form
    {
        #region 전역 변수

        //List<string> file_list = new List<string>();

        string[] file_list;
        
        CommonOpenFileDialog cofd = new CommonOpenFileDialog();
        string folder_name;

        string text_name;

        Dictionary<string, (string, string, string)> log_value_dictionary = new Dictionary<string, (string, string, string)>();

        string trial_data;

        Dictionary<string, List<int>> data_index_dict = new Dictionary<string, List<int>>();

        //x값 배열
        List<double> x_value_list = new List<double>();

        //z값 배열
        List<double> z_value_list = new List<double>();


        List<double> x_value_list2 = new List<double>();
        List<double> z_value_list2 = new List<double>();

        //log datatable
        DataTable log_datatable = new DataTable();

        //log datatable2
        DataTable log_datatable2 = new DataTable();

        //데이터추출 데이터테이블
        DataTable normal_dt;
        DataTable damage_dt;

        string check_trial_num;

        DataGridViewRow firstrow;

        List<int> index_value;

        //정상 삭륜 파일 경로
        string filepath;

        #endregion

        #region 생성자
        public Form1()
        {
            InitializeComponent();

            string OpenFilePath = System.Environment.CurrentDirectory;//초기 경로

            cofd.InitialDirectory = OpenFilePath;

            cofd.IsFolderPicker = true;

            checkedListBox1.SelectionMode = System.Windows.Forms.SelectionMode.One; //checkedlistbox에서 하나만 선택하도록 설정.

            openFileDialog1.InitialDirectory = OpenFilePath;


        }
        #endregion


        #region event : form load
        private void Form1_Load(object sender, EventArgs e)
        {
            Global.RobotParam.GetSystemParam();

            
        }

        #endregion

        #region event : form closing
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Enabled = false;
        }

        #endregion


        #region event : 폴더 선택 후 파일명 checklist box1에 출력
        //폴더 선택해서 안에 있는 파일명 가져와서 checkedlistbox에 표시
        private void button1_Click(object sender, EventArgs e)
        {

            if (checkedListBox1.Items.Count > 0)
            {
                checkedListBox1.Items.Clear();
            }

            timer1.Enabled=false;
            vScrollBar1.Value = 1;
            textBox1.Text = 1.ToString();

            
            if (cofd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                folder_name = cofd.FileName;
                file_list = Directory.GetFiles(folder_name);
            }

            foreach (string file in file_list)
            {
                checkedListBox1.Items.Add(Path.GetFileName(file).ToString());
            }
        }

        #endregion

        #region event : checklist box1에서 파일 선택
        //checked list box에서 더블클릭해서 파일 선택
        private async void checkedListBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            timer1.Enabled = false;
            vScrollBar1.Value = 1;
            textBox1.Text = 1.ToString();

            if (checkedListBox1.SelectedItem != null)
            {
                int index = checkedListBox1.IndexFromPoint(e.Location);

                string open_text_name = checkedListBox1.Items[index].ToString();

                //선택한 항목만 체크표시하고 나머지는 해제
                for (int i = 0; i < checkedListBox1.Items.Count; i++)
                {
                    if (i == index)
                    {
                        checkedListBox1.SetItemCheckState(i, CheckState.Checked);
                    }
                    else
                    {
                        checkedListBox1.SetItemCheckState(i, CheckState.Unchecked);
                    }
                }
                MessageBox.Show(open_text_name + " select");
                text_name = file_list[index];
            }

            //파일 읽어오기
            await Task.Run(() => read_file(text_name,datagrid_num:1));
            //read_file(text_name);

            ////checkbox2에 trial number 불러오기
            await Task.Run(() => bring_trial_num(datagrid_num:1));

            //bring_trial_num();

            //timer1.Enabled = true;

        }

        #endregion

        #region method:선택한 파일 읽은 후 datagrid에 출력
        //선택한 파일 읽어오기
        //1,2023년 06월 14일,19시 13분 21초,597,16.555,343.67
        //(스캔 번호, 날짜, 시간, 스캔데이터 포인트 번호 ,x축 값, z축 값)
        private void read_file(string text_file_name,int datagrid_num)
        {
            StreamReader sr = new StreamReader(text_file_name);
            
            if(datagrid_num == 1)
            {
                //datagridview1 초기화

                if (dataGridView1.Rows.Count != 0)
                {
                    if (dataGridView1.InvokeRequired)
                    {
                        dataGridView1.Invoke(new MethodInvoker(delegate {
                            dataGridView1.Columns.Clear();



                        }));
                    }
                    else
                    {
                        dataGridView1.Columns.Clear();


                    }

                    log_datatable = new DataTable();
                }
            }
            else if(datagrid_num == 2)
            {
                //datagridview2 초기화

                if (dataGridView2.Rows.Count != 0)
                {
                    if (dataGridView2.InvokeRequired)
                    {
                        dataGridView2.Invoke(new MethodInvoker(delegate {
                            dataGridView2.Columns.Clear();



                        }));
                    }
                    else
                    {
                        dataGridView2.Columns.Clear();


                    }

                    log_datatable2 = new DataTable();
                }
            }

            
            

            


            char delimiter = ','; // 구분자 설정 (쉼표)

            
            bool isFirstLine = true;

            // 첫번째 줄만 datatable 이름으로 바꾸고 나머지는 data로 저장.
            using (StreamReader reader = new StreamReader(text_file_name))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] values = line.Split(delimiter);

                    if (isFirstLine)
                    {
                        // 첫 번째 줄인 경우, 열 이름으로 사용하여 DataTable의 열을 생성합니다.
                        if(datagrid_num == 1)
                        {
                            foreach (string columnName in values)
                            {
                                log_datatable.Columns.Add(columnName);
                            }
                        }
                        else
                        {
                            foreach (string columnName in values)
                            {
                                log_datatable2.Columns.Add(columnName);
                            }
                        }
                        isFirstLine = false;
                    }
                    else
                    {
                        if (line != "TRIAL,DATE,TIME,INDEX,X,Z")
                        {
                            // 데이터 행을 DataTable에 추가합니다.

                            if (datagrid_num == 1)
                            {
                                DataRow row = log_datatable.NewRow();
                                for (int j = 0; j < values.Length; j++)
                                {
                                    row[j] = values[j];
                                }
                                log_datatable.Rows.Add(row);
                            }
                            else
                            {
                                DataRow row = log_datatable2.NewRow();
                                for (int j = 0; j < values.Length; j++)
                                {
                                    row[j] = values[j];
                                }
                                log_datatable2.Rows.Add(row);
                            }
                            
                        }
                        
                    }
                }
            }

            

            if (datagrid_num == 1 && dataGridView1.Rows.Count == 0)
            {
                if (dataGridView1.InvokeRequired)
                {
                    dataGridView1.Invoke(new MethodInvoker(delegate { dataGridView1.DataSource = log_datatable; }));
                }
                else
                {
                    dataGridView1.DataSource = log_datatable;
                }
            }
                
            if (datagrid_num == 2 && dataGridView2.Rows.Count == 0)
            {
                if (dataGridView2.InvokeRequired)
                {
                    dataGridView2.Invoke(new MethodInvoker(delegate { dataGridView2.DataSource = log_datatable2; }));
                }
                else
                {
                    dataGridView2.DataSource = log_datatable2;
                }
            }
            
        }

        #endregion

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

        

        #region method : 데이터 trial number 가져오기
        private void bring_trial_num(int datagrid_num)
        {


            int datagridview_rowcount;
            


            if (datagrid_num == 1)
            {
                

                datagridview_rowcount = dataGridView1.RowCount;
                if (dataGridView1.RowCount == 0)
                {
                    return;
                }
            }
            else
            {
                

                datagridview_rowcount = dataGridView2.RowCount;
                if (dataGridView2.RowCount == 0)
                {
                    return;
                }
            }

            

            //첫번째 열(trial-number) 불러옴.
            List<int> index_data = new List<int>();

            if (datagrid_num == 1)
            {
                

                DataGridViewColumn col = dataGridView1.Columns[0];//첫번째 열 설정.
                string col_trial_name = col.Name.ToString();
                
            }
            else
            {
                

                DataGridViewColumn col = dataGridView2.Columns[0];//첫번째 열 설정.
                string col_trial_name = col.Name.ToString();
                
            }

            
            

            //MessageBox.Show("선택된 열의 이름은 " + col_trial_name + " 입니다.");

            int count = 0;
            
            int trial_num_check = 1;

            index_data.Add(count); // 트라이얼:1 데이터 시작점.

            data_index_dict.Clear();

            int last_trial_num;
            int start_trial_num=0;
            bool check_start = true;

            for (int i = 0; i < datagridview_rowcount - 1; i++) // 맨 위의 행 이름들도 포함하므로 -1을 해줘야함.
            {
                if (datagrid_num == 1)
                {
                    DataGridViewRow row = dataGridView1.Rows[i];
                    trial_data = row.Cells[0].Value.ToString();
                }
                else
                {
                    DataGridViewRow row = dataGridView2.Rows[i];
                    trial_data = row.Cells[0].Value.ToString();
                }
                

                

                if(check_start == true)
                {
                    start_trial_num = int.Parse(trial_data);
                    check_start = false;
                }

                //WriteTextBox(textBox1, trial_data+"\t"+count+"\r\n");

                //count를 체크하면서 진행하다가 trial number가 바뀌면 전 인덱스부분을 전 트라이얼 넘버 마지막 index로 설정.  현 인덱스를 트라이얼 넘버 시작 index로 설정.
                if (trial_num_check != int.Parse(trial_data))
                {
                    int last_trial_index = i - 1; // 현재 카운트(인덱스)-1을 트라이얼 마지막 index로 설정.

                    index_data.Add(last_trial_index);

                    //WriteTextBox(textBox1, $"index_data=({index_data[0]},{index_data[1]})");
                    //WriteTextBox(textBox1, "\r\n");

                    data_index_dict.Add(Convert.ToString(trial_num_check), index_data);

                    //index_data 리스트 초기화 후 현재 시점부터 카운트부터 다음 트라이얼 시작으로 설정.
                    index_data = new List<int>();



                    int start_trial_index = i;
                    index_data.Add(start_trial_index); // 현재 카운트(인덱스)를 트라이얼 마지막 index로 설정.

                    trial_num_check++; // 다음 트라이얼 넘버를 비교하기 위해 증가
                }

                //마지막 행에 도달했을 때 코드
                else if (count == datagridview_rowcount - 2)
                {
                    int last_trial_index = i; // 현재 카운트(인덱스)을 트라이얼 마지막 index로 설정.

                    index_data.Add(last_trial_index);

                    //WriteTextBox(textBox1, $"index_data=({index_data[0]},{index_data[1]})");
                    //WriteTextBox(textBox1, "\r\n");

                    data_index_dict.Add(Convert.ToString(trial_num_check), index_data);

                    last_trial_num = trial_num_check;
                }

                count++;


            }

            StringBuilder sb = new StringBuilder();
            foreach (var pair in data_index_dict)
            {
                sb.AppendLine($"Key: {pair.Key}, value: {pair.Value}\r\n");
            }

            //WriteTextBox(textBox1, sb.ToString());

            if (vScrollBar1.InvokeRequired)
            {
                vScrollBar1.Invoke(new MethodInvoker(delegate {
                    textBox1.Text = start_trial_num.ToString();
                    vScrollBar1.Minimum = start_trial_num;
                    vScrollBar1.Maximum = trial_num_check+9;
                }));
            }
            else
            {
                textBox1.Text = start_trial_num.ToString();
                vScrollBar1.Minimum = start_trial_num;
                vScrollBar1.Maximum = trial_num_check+9;
            }

            check_trial_num = Convert.ToString(start_trial_num);

            


           
        }

        #endregion


        


        #region method : 선택한 번호에 맞는 데이터 출력
        //checkboxlist에서 선택한 trial nuimber 번호에 맞는 데이터 선택해서 출력
        private void data_exist(string trial_num)
        {
            
            index_value = new List<int>();

            if (data_index_dict.ContainsKey(trial_num))
            {
                index_value = data_index_dict[trial_num];

                int start_index = index_value[0];

                int end_index = index_value[1];

                x_value_list.Clear();
                z_value_list.Clear();

                x_value_list2.Clear();
                z_value_list2.Clear();

                

                for (int i = start_index; i <= end_index; i++)
                {
                    firstrow = dataGridView1.Rows[i];

                    string trial_number = firstrow.Cells[0].Value.ToString();
                    string point_number = firstrow.Cells[3].Value.ToString();
                    string x_value = firstrow.Cells[4].Value.ToString();
                    string z_value = firstrow.Cells[5].Value.ToString();

                    

                    x_value_list.Add(double.Parse(x_value));
                    z_value_list.Add(double.Parse(z_value));

                }

                if (dataGridView2.RowCount != 0) {
                    for (int i = start_index; i <= end_index; i++)
                    {
                        firstrow = dataGridView2.Rows[i];

                        string trial_number = firstrow.Cells[0].Value.ToString();
                        string point_number = firstrow.Cells[3].Value.ToString();
                        string x_value = firstrow.Cells[4].Value.ToString();
                        string z_value = firstrow.Cells[5].Value.ToString();

                        ////trial number 추출값 확인용
                        //WriteTextBox(textBox1, "던질거");
                        //WriteTextBox(textBox1, $"{trial_number} {point_number} {x_value} {z_value}" + " ");
                        //WriteTextBox(textBox1, "\r\n");

                        //WriteTextBox(textBox1, "원본");
                        //foreach (DataGridViewCell cell in firstrow.Cells)
                        //{
                        //    if (cell.Value != null)
                        //    {
                        //        WriteTextBox(textBox1, cell.Value.ToString() + " ");
                        //    }
                        //}

                        //WriteTextBox(textBox1, "\r\n");

                        x_value_list2.Add(double.Parse(x_value));
                        z_value_list2.Add(double.Parse(z_value));

                    }
                }


                if (chart1.InvokeRequired)
                {
                    chart1.Invoke(new MethodInvoker(delegate { // Chart에 데이터 추가 및 표시

                        

                        chart1.Series.Clear(); // 기존 시리즈 초기화


                        Series series = chart1.Series.Add("damaged wheel"); // 새로운 시리즈 생성
                                                                            //Series series = new Series("Data"); // 새로운 시리즈 생성

                        Series series2 = chart1.Series.Add("original wheel");


                        //테스트
                        series.ChartType = SeriesChartType.Line; // 시리즈의 차트 유형 설정
                        series.MarkerSize = 2;
                        series.Color = Color.Red;

                        chart1.ChartAreas[0].AxisX.Interval = 1;
                        chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
                        chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = false;

                        //차트 배경 이미지 설정
                        //chart1.ChartAreas[0].BackImage = "D:\\과제\\삭도검사로봇\\삭도 코드\\wire_wheel_code\\check_wirewheel_program\\check_wirewheel_program\\bin\\Debug\\modeling_image.png";

                        //chart1.ChartAreas[0].BackImageWrapMode = ChartImageWrapMode.Scaled;
                        //chart1.ChartAreas[0].BackImage = "D:\\과제\\삭도검사로봇\\삭도 코드\\wire_wheel_code\\check_wirewheel_program\\check_wirewheel_program\\bin\\Debug\\modeling_image.png";


                        

                        



                        chart1.ChartAreas[0].AxisY.Minimum = Global.RobotParam.Mobile_System.ChartYMin; // 335
                        chart1.ChartAreas[0].AxisY.Maximum = Global.RobotParam.Mobile_System.ChartYMax;//345;

                        chart1.ChartAreas[0].AxisX.Minimum = Global.RobotParam.Mobile_System.ChartXMin;//-36;
                        chart1.ChartAreas[0].AxisX.Maximum = Global.RobotParam.Mobile_System.ChartXMax;//36;

                        chart1.Series[0].IsVisibleInLegend = true;
                        chart1.Series[0].IsValueShownAsLabel = false;
                        chart1.Series[0].ChartType = SeriesChartType.Line;
                        chart1.Series[0].LegendText = "legend1:damaged wheel";

                        series2.ChartType = SeriesChartType.Line; // 시리즈의 차트 유형 설정
                        series2.MarkerSize = 2;
                        series2.Color = Color.Blue;


                        chart1.ChartAreas[0].AxisX2.Interval = 1;
                        chart1.ChartAreas[0].AxisX2.MajorGrid.Enabled = false;
                        chart1.ChartAreas[0].AxisY2.MajorGrid.Enabled = false;

                        series2.YAxisType = AxisType.Secondary;
                        series2.XAxisType = AxisType.Secondary;

                        

                        chart1.ChartAreas[0].AxisY2.Minimum = Global.RobotParam.Mobile_System.ChartYMin2; // 335
                        chart1.ChartAreas[0].AxisY2.Maximum = Global.RobotParam.Mobile_System.ChartYMax2;//345;

                        chart1.ChartAreas[0].AxisX2.Minimum = Global.RobotParam.Mobile_System.ChartXMin2;//-36;
                        chart1.ChartAreas[0].AxisX2.Maximum = Global.RobotParam.Mobile_System.ChartXMax2;//36;

                        chart1.Series[1].IsVisibleInLegend = true;
                        chart1.Series[1].IsValueShownAsLabel = false;
                        chart1.Series[1].ChartType = SeriesChartType.Line;
                        chart1.Series[1].LegendText = "legned2:original_wheel";




                        //차트 그래프 설정(series1)




                        //series.ChartType = SeriesChartType.Line; // 시리즈의 차트 유형 설정
                        //series.MarkerSize = 2;
                        //series.Color = Color.Red;

                        //chart1.ChartAreas[0].AxisX.Interval = 1;
                        //chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
                        //chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = false;

                        ////차트 배경 이미지 설정
                        ////chart1.ChartAreas[0].BackImage = "D:\\과제\\삭도검사로봇\\삭도 코드\\wire_wheel_code\\check_wirewheel_program\\check_wirewheel_program\\bin\\Debug\\modeling_image.png";



                        //chart1.ChartAreas[0].AxisY.Minimum = Global.RobotParam.Mobile_System.ChartYMin; // 335
                        //chart1.ChartAreas[0].AxisY.Maximum = Global.RobotParam.Mobile_System.ChartYMax;//345;

                        //chart1.ChartAreas[0].AxisX.Minimum = Global.RobotParam.Mobile_System.ChartXMin;//-36;
                        //chart1.ChartAreas[0].AxisX.Maximum = Global.RobotParam.Mobile_System.ChartXMax;//36;

                        //chart1.Series[0].IsVisibleInLegend = false;
                        //chart1.Series[0].IsValueShownAsLabel = false;
                        //chart1.Series[0].ChartType = SeriesChartType.Line;
                        //chart1.Series[0].LegendText = "damaged wheel";

                        //차트 그래프 설정(series2)



                        //series2.ChartType = SeriesChartType.Line; // 시리즈의 차트 유형 설정
                        //series2.MarkerSize = 2;
                        //series2.Color = Color.Blue;

                        //chart1.ChartAreas[0].AxisX.Interval = 1;
                        //chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
                        //chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = false;


                        //chart1.ChartAreas[1].AxisY.Minimum = Global.RobotParam.Mobile_System.ChartYMin; // 335
                        //chart1.ChartAreas[1].AxisY.Maximum = Global.RobotParam.Mobile_System.ChartYMax;//345;

                        //chart1.ChartAreas[1].AxisX.Minimum = Global.RobotParam.Mobile_System.ChartXMin;//-36;
                        //chart1.ChartAreas[1].AxisX.Maximum = Global.RobotParam.Mobile_System.ChartXMax;//36;

                        //chart1.Series[1].IsVisibleInLegend = false;
                        //chart1.Series[1].IsValueShownAsLabel = false;
                        //chart1.Series[1].ChartType = SeriesChartType.Line;
                        //chart1.Series[1].LegendText = "original wheel";



                        try
                        {

                            //chart1.Series[0].Points.DataBindXY(x_value_list, z_value_list);
                            //chart1.Series[1].Points.DataBindXY(x_value_list2, z_value_list2);
                            series.Points.DataBindXY(x_value_list, z_value_list);// 데이터 포인트 추가
                            series2.Points.DataBindXY(x_value_list2, z_value_list2);
                        }
                        catch (Exception ex)
                        {
                            return;
                        }
                    })); 

                }

                else
                {
                    // Chart에 데이터 추가 및 표시
                    chart1.Series.Clear(); // 기존 시리즈 초기화
                    Series series = chart1.Series.Add("Data"); // 새로운 시리즈 생성
                                                               //Series series = new Series("Data"); // 새로운 시리즈 생성

                    Series series2 = chart1.Series.Add("Data2");

                    series.ChartType = SeriesChartType.Line; // 시리즈의 차트 유형 설정
                    series.MarkerSize = 2;
                    series.Color = Color.Red;

                    chart1.ChartAreas[0].AxisX.Interval = 1;
                    chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
                    chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = false;


                    chart1.ChartAreas[0].AxisY.Minimum = Global.RobotParam.Mobile_System.ChartYMin; // 335
                    chart1.ChartAreas[0].AxisY.Maximum = Global.RobotParam.Mobile_System.ChartYMax;//345;

                    chart1.ChartAreas[0].AxisX.Minimum = Global.RobotParam.Mobile_System.ChartXMin;//-36;
                    chart1.ChartAreas[0].AxisX.Maximum = Global.RobotParam.Mobile_System.ChartXMax;//36;

                    chart1.Series[0].IsVisibleInLegend = false;
                    chart1.Series[0].IsValueShownAsLabel = false;
                    chart1.Series[0].ChartType = SeriesChartType.Line;

                    //차트 그래프 설정(series2)
                    series2.ChartType = SeriesChartType.Line; // 시리즈의 차트 유형 설정
                    series2.MarkerSize = 2;
                    series2.Color = Color.Blue;

                    chart1.ChartAreas[0].AxisX.Interval = 1;
                    chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
                    chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = false;


                    chart1.ChartAreas[0].AxisY.Minimum = Global.RobotParam.Mobile_System.ChartYMin; // 335
                    chart1.ChartAreas[0].AxisY.Maximum = Global.RobotParam.Mobile_System.ChartYMax;//345;

                    chart1.ChartAreas[0].AxisX.Minimum = Global.RobotParam.Mobile_System.ChartXMin;//-36;
                    chart1.ChartAreas[0].AxisX.Maximum = Global.RobotParam.Mobile_System.ChartXMax;//36;

                    chart1.Series[0].IsVisibleInLegend = false;
                    chart1.Series[0].IsValueShownAsLabel = false;
                    chart1.Series[0].ChartType = SeriesChartType.Line;

                    try
                    {
                        series.Points.DataBindXY(x_value_list, z_value_list);// 데이터 포인트 추가
                        series2.Points.DataBindXY(x_value_list2, z_value_list2);
                    }
                    catch (Exception ex)
                    {
                        return;
                    }

                    //series.Points.DataBindXY(x_value_list, z_value_list); // 데이터 포인트 추가
                }


                //// Chart에 데이터 추가 및 표시
                //chart1.Series.Clear(); // 기존 시리즈 초기화
                //Series series = chart1.Series.Add("Data"); // 새로운 시리즈 생성
                ////Series series = new Series("Data"); // 새로운 시리즈 생성
                //series.ChartType = SeriesChartType.Line; // 시리즈의 차트 유형 설정
                //series.MarkerSize = 2;
                //series.Color = Color.Red;

                //chart1.ChartAreas[0].AxisX.Interval = 1;
                //chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
                //chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = false;


                //chart1.ChartAreas[0].AxisY.Minimum = Global.RobotParam.Mobile_System.ChartYMin; // 335
                //chart1.ChartAreas[0].AxisY.Maximum = Global.RobotParam.Mobile_System.ChartYMax;//345;

                //chart1.ChartAreas[0].AxisX.Minimum = Global.RobotParam.Mobile_System.ChartXMin;//-36;
                //chart1.ChartAreas[0].AxisX.Maximum = Global.RobotParam.Mobile_System.ChartXMax;//36;

                //chart1.Series[0].IsVisibleInLegend = false;
                //chart1.Series[0].IsValueShownAsLabel = false;
                //chart1.Series[0].ChartType = SeriesChartType.Line;


                //series.Points.DataBindXY(x_value_list, z_value_list); // 데이터 포인트 추가


                //chart1.Series.Add(series); // 시리즈를 Chart에 추가

            }

            else if (!data_index_dict.ContainsKey(trial_num))
            {
                timer1.Enabled = false;
                
                MessageBox.Show(trial_num + "가 index dictionary에 존재하지 않습니다.");
                return;
            }
            
        }

        #endregion


        #region event : 설정
        //설정으로 chart axis 설정
        private void 설정ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            setting setting = new setting();
            setting.ShowDialog();
        }
        #endregion

        #region timer : 데이터 chart 실시간
        private async void timer1_Tick(object sender, EventArgs e)
        {
            
            await Task.Run(() =>  data_exist(check_trial_num));
            
        }
        #endregion

        #region event : 스크롤 변화해서 데이터 변화
        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            
            check_trial_num = Convert.ToString(vScrollBar1.Value);
            textBox1.Text = vScrollBar1.Value.ToString();

            //trial num 첫번째 인덱스로 datagridview 위치이동
            int trial_num_index;

            trial_num_index = data_index_dict[check_trial_num][0];

            dataGridView1.FirstDisplayedScrollingRowIndex = trial_num_index;
            dataGridView2.FirstDisplayedScrollingRowIndex = trial_num_index;
            dataGridView1.Refresh();
            dataGridView2.Refresh();

            timer1.Enabled = true;
            


        }

        #endregion

        #region event:정상 삭륜 데이터 선택

        private async void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.Multiselect = false;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //string fileName = Path.GetFileName(openFileDialog1.FileName);
                filepath = openFileDialog1.FileName;
                //image_path = filePath + "\\" + fileName;
                MessageBox.Show(filepath);
            }

            //파일 읽어오기
            await Task.Run(() => read_file(filepath,datagrid_num:2));
            //read_file(text_name);

            //checkbox2에 trial number 불러오기
            await Task.Run(() => bring_trial_num(datagrid_num: 2));

        }

        #endregion


        #region event : chartview button click
        //graphview
        private void button3_Click(object sender, EventArgs e)
        {

            if(dataGridView1.RowCount > dataGridView2.RowCount)
            {
                //int lower_count = dataGridView2.RowCount;
                //int upper_count = dataGridView1.RowCount;

                //for(int i=lower_count+1; i < upper_count; i++)
                //{
                //    dataGridView1.Rows.RemoveAt(i);
                //}


                //vScrollBar1.Maximum = (int)dataGridView1.Rows[lower_count].Cells[0].Value;

                int lower_count = log_datatable2.Rows.Count;
                int upper_count = log_datatable.Rows.Count;

                for (int i = lower_count; i < upper_count; i++)
                {
                    int last_index = log_datatable.Rows.Count - 1;
                    log_datatable.Rows.RemoveAt(last_index);
                }

                dataGridView1.DataSource = log_datatable;

                vScrollBar1.Maximum = Convert.ToInt32(dataGridView2.Rows[lower_count-1].Cells[0].Value)+9;

                //bring_trial_num(1);

            }
            else if (dataGridView1.RowCount < dataGridView2.RowCount)
            {
                //int lower_count = dataGridView1.RowCount;
                //int upper_count = dataGridView2.RowCount;

                //for (int i = lower_count + 1; i < upper_count; i++)
                //{
                //    int last_index = dataGridView2.RowCount-1;
                //    dataGridView2.Rows.RemoveAt(last_index);
                //}

                //vScrollBar1.Maximum = (int)dataGridView1.Rows[lower_count].Cells[0].Value;

                int lower_count = log_datatable.Rows.Count;
                int upper_count = log_datatable2.Rows.Count;

                for (int i = lower_count; i < upper_count; i++)
                {
                    int last_index = log_datatable2.Rows.Count - 1;
                    log_datatable2.Rows.RemoveAt(last_index);
                }

                //datagird2 초기화

                //dataGridView2.Columns.Clear();

                //datagrid2 재작성

                dataGridView2.DataSource = log_datatable2;

                vScrollBar1.Maximum = Convert.ToInt32(dataGridView1.Rows[lower_count-1].Cells[0].Value)+9;

                //bring_trial_num(2);

            }

            timer1.Enabled = true;
        }
        #endregion
    }
}
