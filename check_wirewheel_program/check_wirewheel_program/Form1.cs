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

        string[] file_list;
        
        CommonOpenFileDialog cofd = new CommonOpenFileDialog();
        string folder_name;

        string text_name;

        //trial nubmer 저장 리스트
        string[] normal_trial_num_list;
        string[] damage_trial_num_list;

        //x값 배열
        List<double> x_value_list = new List<double>();

        //z값 배열
        List<double> z_value_list = new List<double>();


        List<double> x_value_list2 = new List<double>();
        List<double> z_value_list2 = new List<double>();

        //log datatable(마모)
        DataTable log_datatable = new DataTable();

        //log datatable2(정상)
        DataTable log_datatable2 = new DataTable();

        string col_trial_name;

        string check_trial_num = "1"; // 초반 trial num 설정.

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
            damage_trial_num_list= await Task.Run(() => bring_trial_num(log_datatable));

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
        private string[] bring_trial_num(DataTable data_tb)
        {


            
            int datatable_rowcount;

            
            datatable_rowcount = data_tb.Rows.Count;
            if(datatable_rowcount == 0)
            {
                return null;
            }
            

            //첫번째 컬럼 이름 불러옴(trial)
            col_trial_name = data_tb.Columns[0].ColumnName;

            //trail number 이름 저장
            object[] trial_data = data_tb.Select().Select(x => x[col_trial_name]).Distinct().ToArray();
            string[] trial_num = trial_data.Cast<string>().ToArray();

            //각 trial number 첫번째 인덱스 반환
            



            return trial_num;
            

        }

        #endregion

        #region method : 선택한 번호에 맞는 데이터 출력
        //2:정상 1:마모
        //checkboxlist에서 선택한 trial nuimber 번호에 맞는 데이터 선택해서 출력
        private async void data_exist(string trial_num)
        {

            //x축 데이터, z축 데이터 list 제작.
            if (normal_trial_num_list.Contains(trial_num) || damage_trial_num_list.Contains(trial_num))
            {

                DataRow[] dataRows = log_datatable.Select($"{col_trial_name} = '{trial_num}'");
                if (dataRows == null || dataRows.Length == 0)
                {
                    return;
                }

                DataTable data1 = dataRows.CopyToDataTable();

                DataRow[] dataRows2 = log_datatable2.Select($"{col_trial_name} = '{trial_num}'");
                if (dataRows2 == null || dataRows2.Length == 0)
                {
                    return;
                }
                DataTable data2 = dataRows2.CopyToDataTable();

                //DataTable data1 = log_datatable.Select($"{col_trial_name} = {trial_num}").CopyToDataTable();

                //DataTable data2 = log_datatable2.Select($"{col_trial_name} = {trial_num}").CopyToDataTable();


                x_value_list = data1.Select().Select(x => Convert.ToDouble(x["X"])).ToList();
                z_value_list = data1.Select().Select(x => Convert.ToDouble(x["Z"])).ToList();

                x_value_list2 = data2.Select().Select(x => Convert.ToDouble(x["X"])).ToList();
                z_value_list2 = data2.Select().Select(x => Convert.ToDouble(x["Z"])).ToList();

                //그래프 출력
                await Task.Run(() => show_chart());

                

            }

            else if (!(normal_trial_num_list.Contains(trial_num) || damage_trial_num_list.Contains(trial_num)))
                {
                    timer1.Enabled = false;

                    MessageBox.Show(trial_num + "가 index dictionary에 존재하지 않습니다.");
                    return;
                }




            }

        #endregion

        #region method: chart 출력

        private void show_chart()
        {
                //x,z 데이터 list를 chart에 출력
                if (chart1.InvokeRequired)
                {
                    chart1.Invoke(new MethodInvoker(delegate
                    { // Chart에 데이터 추가 및 표시



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

                        try
                        {


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

                    try
                    {


                        series.Points.DataBindXY(x_value_list, z_value_list);// 데이터 포인트 추가
                        series2.Points.DataBindXY(x_value_list2, z_value_list2);
                    }
                    catch (Exception ex)
                    {
                        return;
                    }


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

            trial_num_index = log_datatable2.Rows.Count / normal_trial_num_list.Length * (Convert.ToInt32(check_trial_num)-1);

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
                
                filepath = openFileDialog1.FileName;
                
                MessageBox.Show(filepath);
            }

            //파일 읽어오기
            await Task.Run(() => read_file(filepath,datagrid_num:2));
            

            
            normal_trial_num_list = await Task.Run(() => bring_trial_num(log_datatable2));

        }

        #endregion

        #region event : chartview button click
        //작은 데이터의 갯수에 데이터를 강제로 맞춰주고 chart 출력
        private void button3_Click(object sender, EventArgs e)
        {

            if(dataGridView1.RowCount > dataGridView2.RowCount)
            {
                

                int lower_count = log_datatable2.Rows.Count;
                int upper_count = log_datatable.Rows.Count;

                for (int i = lower_count; i < upper_count; i++)
                {
                    int last_index = log_datatable.Rows.Count - 1;
                    log_datatable.Rows.RemoveAt(last_index);
                }

                dataGridView1.DataSource = log_datatable;

                vScrollBar1.Maximum = Convert.ToInt32(dataGridView2.Rows[lower_count-1].Cells[0].Value)+9;

                

            }
            else if (dataGridView1.RowCount < dataGridView2.RowCount)
            {
               

                int lower_count = log_datatable.Rows.Count;
                int upper_count = log_datatable2.Rows.Count;

                for (int i = lower_count; i < upper_count; i++)
                {
                    int last_index = log_datatable2.Rows.Count - 1;
                    log_datatable2.Rows.RemoveAt(last_index);
                }

                

                dataGridView2.DataSource = log_datatable2;

                vScrollBar1.Maximum = Convert.ToInt32(dataGridView1.Rows[lower_count-1].Cells[0].Value)+9;

                

            }

            timer1.Enabled = true;
        }
        #endregion
    }
}
