using Microsoft.WindowsAPICodePack.Dialogs;
using System.Data;
using System.Text;

namespace 삭륜검사로그확인프로그램
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

        string[][] log_value;

        string trial_data;

        Dictionary<string, List<int>> data_index_dict = new Dictionary<string, List<int>>();
        #endregion

        public Form1()
        {
            InitializeComponent();

            string OpenFilePath = System.Environment.CurrentDirectory;//초기 경로

            cofd.InitialDirectory = OpenFilePath;

            cofd.IsFolderPicker = true;

            checkedListBox1.SelectionMode = System.Windows.Forms.SelectionMode.One; //checkedlistbox에서 하나만 선택하도록 설정.
            checkedListBox2.SelectionMode = System.Windows.Forms.SelectionMode.One; //checkedlistbox에서 하나만 선택하도록 설정.
        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        //폴더 선택해서 안에 있는 파일명 가져와서 checkedlistbox에 표시
        private void button1_Click(object sender, EventArgs e)
        {

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

        //checked list box에서 더블클릭해서 파일 선택
        private void checkedListBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

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
                MessageBox.Show(open_text_name + "select");
                text_name = file_list[index];
            }


            read_file(text_name);


        }

        //선택한 파일 읽어오기
        //1,2023년 06월 14일,19시 13분 21초,597,16.555,343.67
        //(스캔 번호, 날짜, 시간, 스캔데이터 포인트 번호 ,x축 값, z축 값)
        private void read_file(string text_file_name)
        {
            StreamReader sr = new StreamReader(text_file_name);

            int i = 0;
            int count = 0;
            while (sr.Peek() >= 0)
            {
                //1.첫 라인을 읽어서 문자열로 변환
                String text_line = sr.ReadLine().ToString();

                //만약 text line이 분리자면 다음 라인으로 넘어감
                if (text_line == "TRIAL,DATE,TIME,INDEX,X,Z")
                {
                    if (count == 0)
                    {
                        String[] text_line_split_no = text_line.Split(",", StringSplitOptions.None);
                        //datagridview 열이름 설정
                        foreach (string column_name in text_line_split_no)
                        {
                            dataGridView1.Columns.Add(column_name, column_name);
                        }
                        count++;
                    }

                    continue;
                }

                //, 기준으로 문자열 분리
                String[] text_line_split = text_line.Split(",", StringSplitOptions.None);

                //datagridview에 데이터 표시
                dataGridView1.Rows.Add(text_line_split);



                i++;
            }
            sr.Close();

            //dictionary 값 확인
            //for (int j = 0; j < log_value_dictionary.Count; j++)
            //{
            //    KeyValuePair<string, (string, string, string)> entry = log_value_dictionary.ElementAt(j);
            //    WriteTextBox(textBox1, "key: " + entry.Key + ", value: " + entry.Value);
            //}
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

        //datagridview에서 row 가져와서 몇번 데이터를 호출하면 데이터 가져오기
        private void button2_Click(object sender, EventArgs e)
        {

            int datagridview_rowcount = dataGridView1.RowCount;

            if (dataGridView1.RowCount == 0)
            {
                return;
            }

            //첫번째 열(trial-number) 불러옴.



            List<int> index_data = new List<int>();

            DataGridViewColumn col = dataGridView1.Columns[0];//첫번째 열 설정.

            string col_trial_name = col.Name.ToString();

            MessageBox.Show("선택된 열의 이름은 " + col_trial_name + " 입니다.");

            int count = 0;

            int trial_num_check = 1;

            index_data.Add(count); // 트라이얼:1 데이터 시작점.

            for (int i = 0; i < datagridview_rowcount - 1; i++) // 맨 위의 행 이름들도 포함하므로 -1을 해줘야함.
            {
                DataGridViewRow row = dataGridView1.Rows[i];

                trial_data = row.Cells[0].Value.ToString();

                //WriteTextBox(textBox1, trial_data+"\t"+count+"\r\n");

                //count를 체크하면서 진행하다가 trial number가 바뀌면 전 인덱스부분을 전 트라이얼 넘버 마지막 index로 설정.  현 인덱스를 트라이얼 넘버 시작 index로 설정.
                if (trial_num_check != int.Parse(trial_data))
                {
                    int last_trial_index = i - 1; // 현재 카운트(인덱스)-1을 트라이얼 마지막 index로 설정.

                    index_data.Add(last_trial_index);

                    WriteTextBox(textBox1, $"index_data=({index_data[0]},{index_data[1]})");
                    WriteTextBox(textBox1, "\r\n");

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

                    WriteTextBox(textBox1, $"index_data=({index_data[0]},{index_data[1]})");
                    WriteTextBox(textBox1, "\r\n");

                    data_index_dict.Add(Convert.ToString(trial_num_check), index_data);
                }

                count++;


            }

            StringBuilder sb = new StringBuilder();
            foreach (var pair in data_index_dict)
            {
                sb.AppendLine($"Key: {pair.Key}, value: {pair.Value}\r\n");
            }

            WriteTextBox(textBox1, sb.ToString());


            //key값을 checkedlistbox2에 저장하여 표시
            foreach (var pair in data_index_dict)
            {
                checkedListBox2.Items.Add(pair.Key.ToString());
            }





            #region 첫번째 행의 값을 불러오는 코드
            //// -> 첫번째 행의 값을 불러오는 걸로 잘못 코딩. 근데 쓸만할듯?
            //DataGridViewRow firstrow = dataGridView1.Rows[0]; 

            //foreach (DataGridViewCell cell in firstrow.Cells)
            //{
            //    if (cell.Value != null)
            //    {
            //        WriteTextBox(textBox1,cell.Value.ToString()+" ");
            //    }
            //}

            //WriteTextBox(textBox1, "\r\n");

            #endregion
        }


        //checkboxlist에서 원하는 trial number 번호 선택.(데이터 번호 선택)
        private void checkedListBox2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (checkedListBox2.SelectedItem != null)
            {
                int index = checkedListBox2.IndexFromPoint(e.Location);

                string check_trial_num = checkedListBox2.Items[index].ToString();

                //선택한 항목만 체크표시하고 나머지는 해제
                for (int i = 0; i < checkedListBox2.Items.Count; i++)
                {
                    if (i == index)
                    {
                        checkedListBox2.SetItemCheckState(i, CheckState.Checked);
                    }
                    else
                    {
                        checkedListBox2.SetItemCheckState(i, CheckState.Unchecked);
                    }
                }
                MessageBox.Show(check_trial_num + " select");

                data_exist(check_trial_num);
            }


        }


        //checkboxlist에서 선택한 trial nuimber 번호에 맞는 데이터 선택해서 출력
        private void data_exist(string trial_num)
        {

            if (data_index_dict.ContainsKey(trial_num))
            {
                List<int> index_value = data_index_dict[trial_num];

                int start_index = index_value[0];

                int end_index = index_value[1];

                for (int i = start_index; i <= end_index; i++)
                {
                    DataGridViewRow firstrow = dataGridView1.Rows[i];

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
                }

            }

            else if (!data_index_dict.ContainsKey(trial_num))
            {
                MessageBox.Show(trial_num + "가 index dictionary에 존재하지 않습니다.");
                return;
            }

        }
    }
}