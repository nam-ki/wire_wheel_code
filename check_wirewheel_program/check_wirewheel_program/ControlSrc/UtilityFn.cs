using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace KIRO
{
    public class Utility
    {
        static string strLogCompare = string.Empty;

        #region 1. fn_LogWrite
        /// <summary>
        /// 로그 파일을 생성을 하며, 출력을 함. 생성 폴더는 실행 파일이 하위 폴더 "Log"
        /// </summary>
        /// <param name="type"></param>
        /// <param name="strClass"></param>
        /// <param name="Function"></param>
        /// <param name="message"></param>
        public static void fn_LogWrite(object type, string strClass, string Function, object message)
        {
            string DirPath = Environment.CurrentDirectory + @"\Log";
            string FilePath = DirPath + "\\Log_" + DateTime.Today.ToString("MM월dd일") + ".log";
            string temp, temp1;

            DirectoryInfo di = new DirectoryInfo(DirPath);
            FileInfo fi = new FileInfo(FilePath);

            temp = string.Format("[{0}] {1}\t{2}\t{3}\t{4}",
                    DateTime.Now, Convert.ToString(type), strClass, Function, Convert.ToString(message));
            temp1 = string.Format("{0}\t{1}\t{2}\t{3}",
                    Convert.ToString(type), strClass, Function, Convert.ToString(message));

            if (strLogCompare == temp1)
                return;

            try
            {
                if (!di.Exists) Directory.CreateDirectory(DirPath);
                if (!fi.Exists)
                {
                    using (StreamWriter sw = new StreamWriter(FilePath))
                    {
                        sw.WriteLine(temp);
                        sw.Close();
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(FilePath))
                    {
                        sw.WriteLine(temp);
                        sw.Close();
                    }
                }

                strLogCompare = temp1;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        #endregion

        #region 2. CSV File to DataTable
        /// <summary>
        /// CSV Filt to DataTable변환
        /// </summary>
        /// <param name="strFilePath">CSV 파일 전체 경로 및 파일 이름 입력</param>
        /// <returns></returns>
        public DataTable ConvertCSVtoDataTable(string strFilePath, Type code)
        {
            try
            {
                DataTable dt = new DataTable();
                using (StreamReader sr = new StreamReader(strFilePath))
                {
                    if (sr.BaseStream.Length == 0)
                        return dt;

                    string[] headers = sr.ReadLine().Split(',');
                    foreach (string header in headers)
                    {
                        dt.Columns.Add(new DataColumn(header, code));
                    }

                    while (!sr.EndOfStream)
                    {
                        string[] rows = sr.ReadLine().Split(',');
                        DataRow dr = dt.NewRow();
                        for (int i = 0; i < rows.Length; i++)
                        {
                            try
                            {
                                if (rows[i] == string.Empty)
                                {
                                    if (typeof(string) == code)
                                        dr[i] = string.Empty;
                                    else
                                        dr[i] = 0;
                                }
                                else
                                    dr[i] = rows[i];
                            }
                            catch (Exception ex)
                            {
                                //Global.Log.Fatal($"An exception occurred from {MethodBase.GetCurrentMethod().Name}", ex);
                            }

                        }
                        dt.Rows.Add(dr);
                    }

                }
                return dt;
            }
            catch (Exception ex)
            {
                //Global.Log.Fatal($"An exception occurred from {MethodBase.GetCurrentMethod().Name}", ex);
            }

            return null;

        }
        #endregion

        #region 3. SetCharSeries(Chart 그래프를 그려줌)
        /// <summary>
        /// .Net Char 그래프를 생성하여 줌
        /// </summary>
        /// <param name="CharCtrl">Chart Control</param>
        /// <param name="SeriesName">Series name "라인 명칭"</param>
        /// <param name="type">라인 타입</param>
        /// <param name="CharColor">라인 색상</param>
        /// <param name="DataX">X축 값</param>
        /// <param name="DataZ">Y축 값</param>
        /// <returns></returns>
        public Series SetCharSeries(Chart CharCtrl, string SeriesName, SeriesChartType type, Color CharColor, double[] DataX, double[] DataZ, int[] BeadInfoPoint = null)
        {
            Series AddSeries = null;
            int PointOffset = 640;
            //Task.Run(() =>
            //{
            bool NameCheck = false;
            for (int i = 0; i < CharCtrl.Series.Count; i++)
            {
                if (CharCtrl.Series[i].Name == SeriesName)
                {
                    CharCtrl.Series[SeriesName].Points.Clear();
                    AddSeries = CharCtrl.Series[SeriesName];
                    NameCheck = true;
                    break;
                }
            }

            if (NameCheck == false)
            {
                AddSeries = CharCtrl.Series.Add(SeriesName);
            }

            //AddSeries.LegendText = SeriesName;
            AddSeries.ChartType = type;
            AddSeries.Color = CharColor;


            CharCtrl.Series[SeriesName].IsVisibleInLegend = true;
            CharCtrl.Series[SeriesName].IsValueShownAsLabel = false;
            CharCtrl.Series[SeriesName].BorderWidth = 2;
            CharCtrl.ChartAreas[0].AxisX.LabelStyle.Format = "0.0";
            CharCtrl.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            CharCtrl.ChartAreas[0].AxisY.MajorGrid.Enabled = false;

            if (CharCtrl.Name == "Bead2DSurfaceView_Front")
            {
                CharCtrl.ChartAreas[0].AxisX.Interval = 5;
                CharCtrl.ChartAreas[0].AxisY.Minimum = 50;
                CharCtrl.ChartAreas[0].AxisY.Maximum = 130;
                PointOffset = 640;
            }
            else if (CharCtrl.Name == "Bead2DSurfaceView_Real")
            {
                CharCtrl.ChartAreas[0].AxisX.Interval = 20;
                CharCtrl.ChartAreas[0].AxisY.Interval = 30;
                CharCtrl.ChartAreas[0].AxisY.Minimum = 190;
                CharCtrl.ChartAreas[0].AxisY.Maximum = 415;
                PointOffset = 1024;
            }
            else
            {
                //CharCtrl.ChartAreas[0].AxisX.Interval = 2;
                //CharCtrl.ChartAreas[0].AxisY.Minimum = 50;
                //CharCtrl.ChartAreas[0].AxisY.Maximum = 130;
            }

            for (int k = 0; k < DataX.Length; k++)
            {
                if (BeadInfoPoint != null && k < PointOffset - BeadInfoPoint[2] && k > PointOffset - BeadInfoPoint[3])
                {
                    AddSeries.Points.AddXY(DataX[k], DataZ[k]);
                    CharCtrl.Series[SeriesName].Points[k].Color = Color.Red;
                    //AddSeries.Color = Color.Red;
                }
                else
                {
                    AddSeries.Points.AddXY(DataX[k], DataZ[k]);
                    AddSeries.Color = CharColor;
                }


            }

            CharCtrl.Update();
            return AddSeries;
        }

        public Series SetCharSeries(Chart CharCtrl, Series CharSeries, string SeriesName, SeriesChartType type, Color CharColor, double[] DataX, double[] DataZ, int[] BeadInfoPoint = null)
        {
            Series AddSeries = null;
            int PointOffset = 640;
            //Task.Run(() =>
            //{
            bool NameCheck = false;
            for (int i = 0; i < CharCtrl.Series.Count; i++)
            {
                if (CharCtrl.Series[i].Name == SeriesName)
                {
                    CharCtrl.Series[SeriesName].Points.Clear();
                    AddSeries = CharCtrl.Series[SeriesName];
                    NameCheck = true;
                    break;
                }
            }

            if (NameCheck == false)
            {
                AddSeries = CharCtrl.Series.Add(SeriesName);
            }

            AddSeries.LegendText = SeriesName;
            AddSeries.ChartType = type;
            AddSeries.Color = CharColor;


            for (int k = 0; k < DataX.Length; k++)
            {
                if (BeadInfoPoint != null && k < PointOffset - BeadInfoPoint[2] && k > PointOffset - BeadInfoPoint[3])
                {
                    AddSeries.Points.AddXY(DataX[k], DataZ[k]);
                    CharSeries.Points[k].Color = Color.Red;
                    //AddSeries.Color = Color.Red;
                }
                else
                {
                    AddSeries.Points.AddXY(DataX[k], DataZ[k]);
                    AddSeries.Color = CharColor;
                }


            }

            //CharCtrl.Update();
            AddSeries.Dispose();
            return AddSeries;
        }

        public Series SetCharSeries(Chart CharCtrl, string SeriesName, SeriesChartType type, Color CharColor, double[] DataX, double[] DataZ, int Offset = 0)
        {
            Series AddSeries = null;
            bool NameCheck = false;
            for (int i = 0; i < CharCtrl.Series.Count; i++)
            {
                if (CharCtrl.Series[i].Name == SeriesName)
                {
                    CharCtrl.Series[i].Enabled = true;
                    CharCtrl.Series[SeriesName].Points.Clear();
                    AddSeries = CharCtrl.Series[SeriesName];
                    NameCheck = true;
                    break;
                }
            }

            if (NameCheck == false)
            {
                AddSeries = CharCtrl.Series.Add(SeriesName);
            }

            AddSeries.LegendText = SeriesName;
            AddSeries.ChartType = type;
            AddSeries.Color = CharColor;


            CharCtrl.Series[SeriesName].IsVisibleInLegend = true;
            CharCtrl.Series[SeriesName].IsValueShownAsLabel = false;
            CharCtrl.ChartAreas[0].AxisX.LabelStyle.Format = "0.0";

            if (CharCtrl.Name == "Bead2DSurfaceView_Front")
            {
                CharCtrl.ChartAreas[0].AxisX.Interval = 2;
                CharCtrl.ChartAreas[0].AxisY.Minimum = 50;
                CharCtrl.ChartAreas[0].AxisY.Maximum = 130;
            }
            else if (CharCtrl.Name == "Bead2DSurfaceView_Real")
            {
                CharCtrl.ChartAreas[0].AxisX.Interval = 20;
                CharCtrl.ChartAreas[0].AxisY.Interval = 30;
                CharCtrl.ChartAreas[0].AxisY.Minimum = 190;
                CharCtrl.ChartAreas[0].AxisY.Maximum = 415;

            }
            else
            {
                CharCtrl.ChartAreas[0].AxisX.Interval = 2;
                CharCtrl.ChartAreas[0].AxisY.Minimum = 50;
                CharCtrl.ChartAreas[0].AxisY.Maximum = 130;
            }

            for (int k = 0; k < DataX.Length; k++)
            {
                AddSeries.Points.AddXY(DataX[k], DataZ[k] + Offset);
            }

            return AddSeries;
        }

        public Series SetCharSeries(Chart CharCtrl, string SeriesName, SeriesChartType type, Color CharColor, DataTable Data, int Offset = 0, int[] LablePoint = null)
        {
            Series AddSeries = null;
            bool NameCheck = false;

            double[] DataX = GetDataTable_RawdataArry(Data, "X_Axis");
            double[] DataZ = GetDataTable_RawdataArry(Data, "Z_Axis");

            for (int i = 0; i < CharCtrl.Series.Count; i++)
            {
                if (CharCtrl.Series[i].Name == SeriesName)
                {
                    CharCtrl.Series[i].Enabled = true;
                    CharCtrl.Series[SeriesName].Points.Clear();
                    AddSeries = CharCtrl.Series[SeriesName];
                    NameCheck = true;
                    break;
                }
            }

            if (NameCheck == false)
            {
                AddSeries = CharCtrl.Series.Add(SeriesName);
            }

            AddSeries.LegendText = SeriesName;
            AddSeries.ChartType = type;
            AddSeries.Color = CharColor;


            CharCtrl.Series[SeriesName].IsVisibleInLegend = true;
            CharCtrl.Series[SeriesName].IsValueShownAsLabel = false;
            CharCtrl.ChartAreas[0].AxisX.LabelStyle.Format = "0.0";
            CharCtrl.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            CharCtrl.ChartAreas[0].AxisY.MajorGrid.Enabled = false;

            if (CharCtrl.Name == "Bead2DSurfaceView_Front")
            {
                CharCtrl.ChartAreas[0].AxisX.Interval = 2;
                CharCtrl.ChartAreas[0].AxisY.Minimum = 50;
                CharCtrl.ChartAreas[0].AxisY.Maximum = 130;
            }
            else if (CharCtrl.Name == "Bead2DSurfaceView_Real")
            {
                CharCtrl.ChartAreas[0].AxisX.Interval = 20;
                CharCtrl.ChartAreas[0].AxisY.Interval = 30;
                CharCtrl.ChartAreas[0].AxisY.Minimum = 190;
                CharCtrl.ChartAreas[0].AxisY.Maximum = 415;

            }
            else
            {
                CharCtrl.ChartAreas[0].AxisX.Interval = 2;
                CharCtrl.ChartAreas[0].AxisY.Minimum = 50;
                CharCtrl.ChartAreas[0].AxisY.Maximum = 130;
            }

            for (int k = 0; k < DataX.Length; k++)
            {
                AddSeries.Points.AddXY(DataX[k], DataZ[k] + Offset);
            }

            if (LablePoint != null)
            {
                AddSeries.Points[LablePoint[0]].Label = "중심점";
                AddSeries.Points[LablePoint[0]].Color = Color.Green;
                AddSeries.Points[LablePoint[1]].Label = "최고높이";
                AddSeries.Points[LablePoint[1]].Color = Color.Gold;

                AddSeries.Points[LablePoint[2]].Label = "Slope 시작점";
                AddSeries.Points[LablePoint[2]].Color = Color.Red;
                AddSeries.Points[LablePoint[3]].Label = "Slope끝점";
                AddSeries.Points[LablePoint[3]].Color = Color.Red;
            }

            return AddSeries;
        }
        #endregion

        #region 3.1 차트에서 선택된 Series를 감춘다
        /// <summary>
        /// 차트에서 선택된 Series를 감춘다
        /// </summary>
        /// <param name="CharCtrl">컨트롤 명</param>
        /// <param name="SeriesName">숨기고자 하는 Series</param>
        public void SetCharHide(Chart CharCtrl, string SeriesName)
        {
            for (int i = 0; i < CharCtrl.Series.Count; i++)
            {
                if (CharCtrl.Series[i].Name == SeriesName)
                {
                    CharCtrl.Series[i].Enabled = false;
                    return;
                }
            }
            return;
        }
        #endregion

        #region 4. 레지스터 저장, 읽기, 삭제
        /// <summary>
        /// 레지스터 항목을 삭제합니다.
        /// </summary>
        /// <param name="strSubKey">서브키에 대항하는 항목 삭제</param>
        /// <returns>
        /// ture : 성공
        /// false : 실태
        /// </returns>
        public bool DeleteReg(string strSubKey)
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\", true);
            try
            { //하위 폴더(레지스트리)가 있으면 삭제 안됨. 
                //if(rk !=null) rkDeleteSubKey(strSubKey); 
                //하위 폴더(레지스트리) 가 있던 없던 걍 삭제... 
                if (rk != null) rk.DeleteSubKeyTree(strSubKey);
            }
            catch (Exception ex)
            {
                //Global.Log.Fatal($"An exception occurred from {MethodBase.GetCurrentMethod().Name}", ex);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 레지스터에 값을 저장 합니다.
        /// </summary>
        /// <param name="strAppName">응용프로그램 이름</param>
        /// <param name="strSubKey">서브키 이름</param>
        /// <param name="strKey">하위키 이름</param>
        /// <param name="strValue">설정 값</param>
        /// <returns>
        /// ture : 성공
        /// false : 실태
        /// </returns>
        public bool WriteReg(string strAppName, string strSubKey, string strKey, string strValue)
        {
            RegistryKey rkReg = Registry.CurrentUser.OpenSubKey("SOFTWARE\\" + strAppName, true);

            //null 이면 폴더(레지스트리)가 없으므로 만듬... 
            if (rkReg == null) rkReg = Registry.CurrentUser.CreateSubKey("SOFTWARE\\" + strAppName);
            //OpenSubKey (하위폴더(레지스트리 이름) , 쓰기 선택 True 쓰기 False 및 인자가 없다면 읽기) 
            RegistryKey rkSub = Registry.CurrentUser.OpenSubKey("SOFTWARE\\" + strAppName + "\\" + strSubKey, true);
            if (rkSub == null) rkSub = Registry.CurrentUser.CreateSubKey("SOFTWARE\\" + strAppName + "\\" + strSubKey);
            try
            {
                rkSub.SetValue(strKey, strValue);
            }
            catch (Exception ex)
            {
                //Global.Log.Fatal($"An exception occurred from {MethodBase.GetCurrentMethod().Name}", ex);
                return false;
            }
            return true;
        }
        public bool WriteReg(string strAppName, string strSubKey, string strSubKey01, string strKey, string strValue)
        {
            RegistryKey rkReg = Registry.CurrentUser.OpenSubKey("SOFTWARE\\" + strAppName, true);

            //null 이면 폴더(레지스트리)가 없으므로 만듬... 
            if (rkReg == null) rkReg = Registry.CurrentUser.CreateSubKey("SOFTWARE\\" + strAppName);
            //OpenSubKey (하위폴더(레지스트리 이름) , 쓰기 선택 True 쓰기 False 및 인자가 없다면 읽기) 
            RegistryKey rkSub = Registry.CurrentUser.OpenSubKey("SOFTWARE\\" + strAppName + "\\" + strSubKey, true);
            if (rkSub == null) rkSub = Registry.CurrentUser.CreateSubKey("SOFTWARE\\" + strAppName + "\\" + strSubKey);

            RegistryKey rkSub1 = Registry.CurrentUser.OpenSubKey("SOFTWARE\\" + strAppName + "\\" + strSubKey + "\\" + strSubKey01, true);
            if (rkSub1 == null) rkSub1 = Registry.CurrentUser.CreateSubKey("SOFTWARE\\" + strAppName + "\\" + strSubKey + "\\" + strSubKey01);

            try
            {
                rkSub1.SetValue(strKey, strValue);
            }
            catch (Exception ex)
            {
                //Global.Log.Fatal($"An exception occurred from {MethodBase.GetCurrentMethod().Name}", ex);
                return false;
            }
            return true;
        }
        /// <summary>
        /// 레지스터에 값을 읽어 옵니다.
        /// </summary>
        /// <param name="strAppName">응용프로그램 이름</param>
        /// <param name="strSubKey">서키 이름</param>
        /// <param name="strKey">하위키 이름</param>
        /// <returns>하위키 값</returns>
        public string ReadReg(string strAppName, string strSubKey, string strKey)
        {
            RegistryKey reg;
            try
            {
                if (Registry.CurrentUser.OpenSubKey("SOFTWARE\\" + strAppName) == null)
                    return string.Empty;

                reg = Registry.CurrentUser.OpenSubKey("SOFTWARE\\" + strAppName).OpenSubKey(strSubKey);
            }
            catch (Exception ex)
            {
                //Global.Log.Fatal($"An exception occurred from {MethodBase.GetCurrentMethod().Name}", ex);
                return string.Empty;
            }

            if (reg == null)
                return string.Empty;

            return reg.GetValue(strKey, "").ToString();
        }

        public string ReadReg(string strAppName, string strSubKey, string strSubKey01, string strKey)
        {
            RegistryKey reg;
            try
            {
                if (Registry.CurrentUser.OpenSubKey("SOFTWARE\\" + strAppName + "\\" + strSubKey) == null)
                    return string.Empty;

                reg = Registry.CurrentUser.OpenSubKey("SOFTWARE\\" + strAppName + "\\"+ strSubKey).OpenSubKey(strSubKey01);
            }
            catch (Exception ex)
            {
                //Global.Log.Fatal($"An exception occurred from {MethodBase.GetCurrentMethod().Name}", ex);
                return string.Empty;
            }

            if (reg == null)
                return string.Empty;

            return reg.GetValue(strKey, "").ToString();
        }
        #endregion

        //#region 5. 환경 설정 변수 읽어 및 쓰기
        ///// <summary>
        ///// App.config 사용자 환경 설정 파일 가져오기
        ///// </summary>
        ///// <param name="ConfigType">구조체(Class) 타입 지정</param>
        ///// <param name="obj">구조체를 Object로 변환</param>
        ///// <returns>사용자 설정 파일을 Object형태로 변환</returns>
        //public object GetConfigurationfromAppConfig(Type ConfigType, Object obj)
        //{
        //    FieldInfo[] fields = ConfigType.GetFields();

        //    foreach (var Data in fields)
        //    {
        //        if (Data.FieldType.Name == typeof(string).Name)
        //            Data.SetValue(obj, ConfigurationManager.AppSettings[Data.Name]);
        //        else if (Data.FieldType.Name == typeof(bool).Name)
        //            Data.SetValue(obj, Convert.ToBoolean(ConfigurationManager.AppSettings[Data.Name]));
        //        else if (Data.FieldType.Name == typeof(Int32).Name)
        //            Data.SetValue(obj, Convert.ToInt32(ConfigurationManager.AppSettings[Data.Name]));
        //        else if (Data.FieldType.Name == typeof(double).Name)
        //            Data.SetValue(obj, Convert.ToDouble(ConfigurationManager.AppSettings[Data.Name]));
        //    }

        //    return obj;
        //}

        public object GetConfigurationfromRegistry(Type ConfigType, Object obj)
        {
            FieldInfo[] fields = ConfigType.GetFields();
            var AppName = "KIRO";
            var SubKey = "TAB_CTRL";

            foreach (var Data in fields)
            {
                if (Data.FieldType.Name == typeof(string).Name)
                {
                    var strkey = Data.Name.Replace("m_", "");
                    strkey = Data.Name.Replace("_", "");
                    string result = Global.pUtilt.ReadReg(AppName, SubKey, strkey);

                    if (result != string.Empty)
                        Data.SetValue(obj, Convert.ToString(result));
                }
                else if (Data.FieldType.Name == typeof(bool).Name)
                {
                    var strkey = Data.Name.Replace("m_", "");
                    strkey = Data.Name.Replace("_", "");

                    string result = Global.pUtilt.ReadReg(AppName, SubKey, strkey);

                    if (result != string.Empty)
                        Data.SetValue(obj, Convert.ToBoolean(result));

                }
                else if (Data.FieldType.Name == typeof(Int32).Name)
                {
                    var strkey = Data.Name.Replace("m_", "");
                    strkey = Data.Name.Replace("_", "");

                    string result = Global.pUtilt.ReadReg(AppName, SubKey, strkey);

                    if (result != string.Empty)
                        Data.SetValue(obj, Convert.ToInt32(result));
                }
                else if (Data.FieldType.Name == typeof(double).Name)
                {
                    var strkey = Data.Name.Replace("m_", "");
                    strkey = Data.Name.Replace("_", "");

                    string result = Global.pUtilt.ReadReg(AppName, SubKey, strkey);

                    if (result != string.Empty)
                        Data.SetValue(obj, Convert.ToDouble(result));
                }
                //else if (Data.FieldType.Name == typeof(TimeSpan).Name)
                //{
                //    var strkey = Data.Name.Replace("m_", "");
                //    strkey = Data.Name.Replace("_", "");

                //    string result = PipeControl_Define.pUtilt.ReadReg(AppName, SubKey, strkey);

                //    if (result != string.Empty)
                //        Data.SetValue(obj, Convert.ToDateTime(result));
                //}
            }

            return obj;
        }

        public void SetConfigurationfromRegistry(Type ConfigType, Object obj)
        {
            FieldInfo[] fields = ConfigType.GetFields();
            var AppName = "KIRO";
            var SubKey = "TAB_CTRL";

            foreach (var Data in fields)
            {
                var strkey = Data.Name.Replace("m_", "");
                strkey = Data.Name.Replace("_", "");
                Global.pUtilt.WriteReg(AppName, SubKey, strkey, Data.GetValue(obj).ToString());
            }
        }

        /// <summary>
        /// 사용자 설정 환경 저장
        /// </summary>
        /// <param name="key">저장하고자하는 항목</param>
        /// <param name="value">항목 값</param>
        public static void SetAppConfig(string key, string value)
        {
            if (value == "False")
                value = "false";
            else if (value == "True")
                value = "true";

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            KeyValueConfigurationCollection cfgCollection = config.AppSettings.Settings;
            config.AppSettings.Settings[key].Value = value;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
        }

        #endregion

        #region 6. CSV 파일로 저장을 함.
        /// <summary>
        /// CSV 파일로 2개의 배열 데이터를 저장을 함
        /// </summary>
        /// <param name="FileName">저장할 파일 이름(확장자 제외)</param>
        /// <param name="DataX">double 형태의 배열 데이터</param>
        /// <param name="DataZ">double 형태의 배열 데이터</param>
        public void SetDataSaveByCSV(string FileName, double[] DataX, double[] DataZ)
        {
            if (FileName != string.Empty)
            {
                DataTable dtBeadRawData = new DataTable();
                dtBeadRawData.Columns.Add(new DataColumn("X_Axis", typeof(double)));
                dtBeadRawData.Columns.Add(new DataColumn("Z_Axis", typeof(double)));

                string DirPath = Environment.CurrentDirectory + @"\Data";
                string FilePath = string.Empty;// = DirPath + DateTime.Today.ToString("MM월dd일ss초") + ".csv";

                DirectoryInfo dirinfo = new DirectoryInfo(DirPath);
                //DirectoryInfo.Exists로 폴더 존재유무 확인
                if (dirinfo.Exists)
                {
                    FilePath = DirPath + "\\[" + (dirinfo.GetFiles().Length + 1) + "]_" + FileName + "_" + DateTime.Now.ToString("HH시mm분ss초") + ".csv";
                }
                else
                {
                    dirinfo.Create();
                    FilePath = DirPath + "\\[" + (dirinfo.GetFiles().Length + 1) + "]_" + FileName + "_" + DateTime.Now.ToString("HH시mm분ss초") + ".csv";
                }


                try
                {
                    FileInfo fileDel = new FileInfo(FilePath);

                    if (fileDel.Exists)
                        fileDel.Delete();

                    using (StreamWriter outfile = new StreamWriter(FilePath))
                    {
                        //컬럼 이름들을 ","로 나누고 저장.
                        string line = string.Join(",", dtBeadRawData.Columns.Cast<object>());
                        outfile.WriteLine(line);

                        for (int Loop = 0; Loop < DataX.Length; Loop++)
                            outfile.Write("{0}, {1} \r\n", DataX[Loop], DataZ[Loop]);

                        outfile.Close();
                    }
                }
                catch (Exception ex)
                {
                    //Global.Log.Fatal($"An exception occurred from {MethodBase.GetCurrentMethod().Name}", ex);
                }
            }
        }

        public void SetDataSaveByCSV(string DirPath, string FileName, double[] DataX, double[] DataZ)
        {
            if (FileName != string.Empty)
            {
                DataTable dtBeadRawData = new DataTable();
                dtBeadRawData.Columns.Add(new DataColumn("X_Axis", typeof(double)));
                dtBeadRawData.Columns.Add(new DataColumn("Z_Axis", typeof(double)));

                string FilePath = string.Empty;// = DirPath + DateTime.Today.ToString("MM월dd일ss초") + ".csv";

                DirectoryInfo dirinfo = new DirectoryInfo(DirPath);
                //DirectoryInfo.Exists로 폴더 존재유무 확인
                if (dirinfo.Exists)
                {
                    FilePath = DirPath + FileName + ".csv";
                }
                else
                {
                    FilePath = DirPath + FileName + ".csv";
                }


                try
                {
                    FileInfo fileDel = new FileInfo(FilePath);

                    if (fileDel.Exists)
                        fileDel.Delete();

                    using (StreamWriter outfile = new StreamWriter(FilePath))
                    {
                        //컬럼 이름들을 ","로 나누고 저장.
                        string line = string.Join(",", dtBeadRawData.Columns.Cast<object>());
                        outfile.WriteLine(line);

                        for (int Loop = 0; Loop < DataX.Length; Loop++)
                            outfile.Write("{0}, {1} \r\n", DataX[Loop], DataZ[Loop]);

                        outfile.Close();
                    }
                }
                catch (Exception ex)
                {
                    //Global.Log.Fatal($"An exception occurred from {MethodBase.GetCurrentMethod().Name}", ex);
                }
            }
        }

        public void SetDataSaveByCSV(string FileName, string[] ColumsName, double[][] DataX, bool isDataHeader = true)
        {
            if (FileName != string.Empty)
            {
                DataTable dtBeadRawData = new DataTable();

                foreach (string Name in ColumsName)
                {
                    dtBeadRawData.Columns.Add(new DataColumn(Name, typeof(double)));
                }


                string DirPath = Environment.CurrentDirectory + @"\CSV";
                string FilePath = string.Empty;// = DirPath + DateTime.Today.ToString("MM월dd일ss초") + ".csv";

                DirectoryInfo dirinfo = new DirectoryInfo(DirPath);
                //DirectoryInfo.Exists로 폴더 존재유무 확인
                if (dirinfo.Exists)
                {
                    if (isDataHeader)
                        FilePath = DirPath + "\\[" + (dirinfo.GetFiles().Length + 1) + "]_" + FileName + "_" + DateTime.Now.ToString("HH시mm분ss초") + ".csv";
                    else
                        FilePath = DirPath + "\\[" + (dirinfo.GetFiles().Length + 1) + "]_" + FileName + ".csv";
                }
                else
                {
                    dirinfo.Create();

                    if (isDataHeader)
                        FilePath = DirPath + "\\[" + (dirinfo.GetFiles().Length + 1) + "]_" + FileName + "_" + DateTime.Now.ToString("HH시mm분ss초") + ".csv";
                    else
                        FilePath = DirPath + "\\[" + (dirinfo.GetFiles().Length + 1) + "]_" + FileName + ".csv";
                }


                try
                {
                    FileInfo fileDel = new FileInfo(FilePath);

                    if (fileDel.Exists)
                        fileDel.Delete();

                    using (StreamWriter outfile = new StreamWriter(FilePath))
                    {
                        //컬럼 이름들을 ","로 나누고 저장.
                        string line = string.Join(",", dtBeadRawData.Columns.Cast<object>());
                        outfile.WriteLine(line);

                        for (int Loop = 0; Loop < DataX[0].Length; Loop++)
                        {
                            for (int Column = 0; Column < ColumsName.Length - 1; Column++)
                            {
                                outfile.Write("{0}, ", DataX[Column][Loop]);
                            }
                            outfile.Write("{0} ", DataX[ColumsName.Length][Loop]);
                            outfile.Write("\r\n");
                        }
                        outfile.Close();
                    }
                }
                catch (Exception ex)
                {
                    //Global.Log.Fatal($"An exception occurred from {MethodBase.GetCurrentMethod().Name}", ex);
                }
            }
        }

        public void SetDataSaveByCSV(string DirPath, string FileName, string[] ColumsName, double[][] DataX, bool isDataHeader = true)
        {
            if (FileName != string.Empty)
            {
                DataTable dtBeadRawData = new DataTable();

                foreach (string Name in ColumsName)
                {
                    dtBeadRawData.Columns.Add(new DataColumn(Name, typeof(double)));
                }


                string FilePath = string.Empty;// = DirPath + DateTime.Today.ToString("MM월dd일ss초") + ".csv";

                DirectoryInfo dirinfo = new DirectoryInfo(DirPath);
                //DirectoryInfo.Exists로 폴더 존재유무 확인
                if (dirinfo.Exists)
                {
                    if (isDataHeader)
                        FilePath = DirPath + FileName + ".csv";
                    else
                        FilePath = DirPath + FileName + ".csv";
                }
                else
                {
                    if (isDataHeader)
                        FilePath = DirPath + FileName + ".csv";
                    else
                        FilePath = DirPath + FileName + ".csv";
                }


                try
                {
                    FileInfo fileDel = new FileInfo(FilePath);

                    if (fileDel.Exists)
                        fileDel.Delete();

                    using (StreamWriter outfile = new StreamWriter(FilePath))
                    {
                        //컬럼 이름들을 ","로 나누고 저장.
                        string line = string.Join(",", dtBeadRawData.Columns.Cast<object>());
                        outfile.WriteLine(line);

                        for (int Loop = 0; Loop < DataX[0].Length; Loop++)
                        {
                            for (int Column = 0; Column < ColumsName.Length - 1; Column++)
                            {
                                outfile.Write("{0}, ", DataX[Column][Loop]);
                            }
                            outfile.Write("{0} ", DataX[ColumsName.Length - 1][Loop]);
                            outfile.Write("\r\n");
                        }
                        outfile.Close();
                    }
                }
                catch (Exception ex)
                {
                    //Global.Log.Fatal($"An exception occurred from {MethodBase.GetCurrentMethod().Name}", ex);
                }
            }
        }

        public void SetDataSaveByCSV(string DirPath, string FileName, DataTable Data)
        {
            if (FileName != string.Empty)
            {
                DataTable dtBeadRawData = new DataTable();

                double[] DataX = GetDataTable_RawdataArry(Data, "X_Axis");
                double[] DataZ = GetDataTable_RawdataArry(Data, "Z_Axis");

                dtBeadRawData.Columns.Add(new DataColumn("X_Axis", typeof(double)));
                dtBeadRawData.Columns.Add(new DataColumn("Z_Axis", typeof(double)));

                string FilePath = string.Empty;// = DirPath + DateTime.Today.ToString("MM월dd일ss초") + ".csv";

                DirectoryInfo dirinfo = new DirectoryInfo(DirPath);
                //DirectoryInfo.Exists로 폴더 존재유무 확인
                if (dirinfo.Exists)
                {
                    FilePath = DirPath + FileName + ".csv";
                }
                else
                {
                    FilePath = DirPath + FileName + ".csv";
                }


                try
                {
                    FileInfo fileDel = new FileInfo(FilePath);

                    if (fileDel.Exists)
                        fileDel.Delete();

                    using (StreamWriter outfile = new StreamWriter(FilePath))
                    {
                        //컬럼 이름들을 ","로 나누고 저장.
                        string line = string.Join(",", dtBeadRawData.Columns.Cast<object>());
                        outfile.WriteLine(line);

                        for (int Loop = 0; Loop < DataX.Length; Loop++)
                            outfile.Write("{0}, {1} \r\n", DataX[Loop], DataZ[Loop]);

                        outfile.Close();
                    }
                }
                catch (Exception ex)
                {
                    //Global.Log.Fatal($"An exception occurred from {MethodBase.GetCurrentMethod().Name}", ex);
                }
            }
        }

        public void SetDataSaveByCSV(string FileName, DataTable Data)
        {
            if (FileName != string.Empty)
            {
                DataTable dtBeadRawData = new DataTable();

                double[] DataX = GetDataTable_RawdataArry(Data, "X_Axis");
                double[] DataZ = GetDataTable_RawdataArry(Data, "Z_Axis");

                dtBeadRawData.Columns.Add(new DataColumn("X_Axis", typeof(double)));
                dtBeadRawData.Columns.Add(new DataColumn("Z_Axis", typeof(double)));

                string DirPath = Environment.CurrentDirectory + @"\BeadData";
                string FilePath = string.Empty;// = DirPath + DateTime.Today.ToString("MM월dd일ss초") + ".csv";

                DirectoryInfo dirinfo = new DirectoryInfo(DirPath);
                //DirectoryInfo.Exists로 폴더 존재유무 확인
                if (dirinfo.Exists)
                {
                    FilePath = DirPath + "\\[" + (dirinfo.GetFiles().Length + 1) + "]_" + FileName + "_" + DateTime.Now.ToString("HH시mm분ss초") + ".csv";
                }
                else
                {
                    dirinfo.Create();
                    FilePath = DirPath + "\\[" + (dirinfo.GetFiles().Length + 1) + "]_" + FileName + "_" + DateTime.Now.ToString("HH시mm분ss초") + ".csv";
                }


                try
                {
                    FileInfo fileDel = new FileInfo(FilePath);

                    if (fileDel.Exists)
                        fileDel.Delete();

                    using (StreamWriter outfile = new StreamWriter(FilePath))
                    {
                        //컬럼 이름들을 ","로 나누고 저장.
                        string line = string.Join(",", dtBeadRawData.Columns.Cast<object>());
                        outfile.WriteLine(line);

                        for (int Loop = 0; Loop < DataX.Length; Loop++)
                            outfile.Write("{0}, {1} \r\n", DataX[Loop], DataZ[Loop]);

                        outfile.Close();
                    }
                }
                catch (Exception ex)
                {
                    //Global.Log.Fatal($"An exception occurred from {MethodBase.GetCurrentMethod().Name}", ex);
                }
            }
        }
        #endregion

        #region 6-1. 데이터 연속 저장
        /// <summary>
        /// 데이터 연속 저장
        /// </summary>
        /// <param name="FileName">저장 이름</param>
        /// <param name="DataType">확장자명</param>
        /// <param name="WriteData">데이터 열</param>
        /// <param name="Column">터이터 컬럼명</param>
        /// <returns> 데이터 저장 유/무</returns>

        public string GetSetDataFile(bool Clear = false)
        {
            if (Clear)
                SaveFileName = string.Empty;

            return SaveFileName;
        }

        private string SaveFileName;
        public bool SetDataSave(string FileName, string DataType, object[] WriteData, string[] Column, bool isSaveFilenNameDelete = false)
        {
            if (FileName != string.Empty)
            {
                if (isSaveFilenNameDelete)
                    SaveFileName = string.Empty;

                if (WriteData.Length != Column.Length)
                    return false;

                string DirPath = Environment.CurrentDirectory + @"\Data";
                string FilePath = string.Empty;

                DirectoryInfo dirinfo = new DirectoryInfo(DirPath);
                //DirectoryInfo.Exists로 폴더 존재유무 확인
                if (dirinfo.Exists)
                {
                    if (FileName != string.Empty)
                    {
                        FilePath = DirPath + "\\" + FileName + "_" + DateTime.Now.ToString("MM월dd일") + "." + DataType;

                        SaveFileName = FilePath;
                    }
                    //else
                    //    FilePath = SaveFileName;
                }
                else
                {
                    dirinfo.Create();
                    FilePath = DirPath + "\\" + FileName + "_" + DateTime.Now.ToString("MM월dd일") + "." + DataType;
                    SaveFileName = FilePath;
                }


                try
                {
                    // 삭제 - 먼저 삭제할 파일을 FileInfo로 연다.
                    FileInfo fileDinfo = new FileInfo(FilePath);

                    if (!fileDinfo.Exists)
                    {
                        using (StreamWriter outfile = new StreamWriter(FilePath))
                        {
                            string strData = string.Empty;
                            foreach (string strColumn in Column)
                            {
                                strData += strColumn + ",";
                            }
                            outfile.WriteLine(strData);


                            strData = string.Empty;
                            foreach (var data in WriteData)
                            {
                                strData += data + ",";
                            }
                            outfile.WriteLine(strData);
                            outfile.Close();
                        }
                    }
                    else
                    {
                        using (StreamWriter outfile = File.AppendText(FilePath))
                        {
                            string strData = string.Empty;
                            foreach (var data in WriteData)
                            {
                                strData += data + ",";
                            }
                            outfile.WriteLine(strData);
                            outfile.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Global.Log.Fatal($"An exception occurred from {MethodBase.GetCurrentMethod().Name}", ex);
                }
            }
            return false;
        }
        #endregion

        #region 7. 현재 디렉토에 파일 순서 및 날짜를 표기하여 최종 저장할 경로명을 반환을 함
        /// <summary>
        /// 현재 디렉토에 파일 순서 및 날짜를 표기하여 최종 저장할 경로명을 반환을 함
        /// </summary>
        /// <param name="DirName">디렉토리 이름</param>
        /// <param name="FileName">파일명</param>
        /// <param name="Type">저장할 확장자 타입</param>
        /// <returns>최종 저장 경로</returns>

        public string GetCurreDirAndTime(string DirName, string FileName, string Type)
        {
            if (FileName != string.Empty)
            {
                string DirPath = Environment.CurrentDirectory + @DirName;
                string FilePath = string.Empty;// = DirPath + DateTime.Today.ToString("MM월dd일ss초") + ".csv";

                DirectoryInfo dirinfo = new DirectoryInfo(DirPath);
                //DirectoryInfo.Exists로 폴더 존재유무 확인
                if (dirinfo.Exists)
                {
                    FilePath = DirPath + "\\[" + (dirinfo.GetFiles().Length + 1) + "]_" + FileName + "_" + DateTime.Now.ToString("HH시mm분ss초") + "." + Type;
                }
                else
                {
                    dirinfo.Create();
                    FilePath = DirPath + "\\[" + (dirinfo.GetFiles().Length + 1) + "]_" + FileName + "_" + DateTime.Now.ToString("HH시mm분ss초") + "." + Type;
                }

                return FilePath;
            }

            return string.Empty;
        }
        #endregion

        #region 8. 차트항목 중 그래프 X, Y축의 값을 라벨링 하여 줌
        private ToolTip tt = null;
        /// <summary>
        /// 차트항목 중 그래프 X, Y축의 값을 라벨링 하여 줌
        /// </summary>
        /// <param name="CharCtrl">차트 컨트롤 변수</param>
        /// <param name="position">현재 마우스 위치값</param>
        public void SetChartToolTip(Chart CharCtrl, Point position)
        {
            if (tt == null) tt = new ToolTip();
            Point tl = Point.Empty;

            ChartArea ca = CharCtrl.ChartAreas[0];

            if (InnerPlotPositionClientRectangle(CharCtrl, ca).Contains(position))
            {

                Axis ax = ca.AxisX;
                Axis ay = ca.AxisY;
                double x = ax.PixelPositionToValue(position.X);
                double y = ay.PixelPositionToValue(position.Y);
                string s = DateTime.FromOADate(x).ToShortDateString();
                if (position != tl)
                    tt.SetToolTip(CharCtrl, string.Format("X={0:0.00} ; {1:0.00}", x, y));
                tl = position;
            }
            else tt.Hide(CharCtrl);
        }

        private RectangleF InnerPlotPositionClientRectangle(Chart chart, ChartArea CA)
        {
            RectangleF IPP = CA.InnerPlotPosition.ToRectangleF();
            RectangleF CArp = ChartAreaClientRectangle(chart, CA);

            float pw = CArp.Width / 100f;
            float ph = CArp.Height / 100f;

            return new RectangleF(CArp.X + pw * IPP.X, CArp.Y + ph * IPP.Y,
                                    pw * IPP.Width, ph * IPP.Height);
        }

        private RectangleF ChartAreaClientRectangle(Chart chart, ChartArea CA)
        {
            RectangleF CAR = CA.Position.ToRectangleF();
            float pw = chart.ClientSize.Width / 100f;
            float ph = chart.ClientSize.Height / 100f;
            return new RectangleF(pw * CAR.X, ph * CAR.Y, pw * CAR.Width, ph * CAR.Height);
        }
        #endregion

        #region 9. 프로퍼티 컬럼 폭 조절
        /// <summary>
        /// 프로퍼티 컬럼 폭 조절
        /// </summary>
        /// <param name="p_oGrid">프로퍼트 컨트롤</param>
        /// <param name="p_iWidth">폭 길이</param>
        public void SetLabelColumnWidth(PropertyGrid p_oGrid, int p_iWidth)
        {
            if (p_oGrid == null) return;

            var oFieldInfo = p_oGrid.GetType().GetField("gridView", BindingFlags.Instance | BindingFlags.NonPublic);
            if (oFieldInfo == null) return;

            if (!(oFieldInfo.GetValue(p_oGrid) is Control oView)) return;

            var oMethodInfo = oView.GetType().GetMethod("MoveSplitterTo", BindingFlags.Instance | BindingFlags.NonPublic);
            if (oMethodInfo == null) return;

            oMethodInfo.Invoke(oView, new object[] { p_iWidth });
        }
        #endregion

        #region 10. 배열을 구조체, 구조체를 배열로 변환
        /// <summary>
        /// byte 배열을 구조체로
        /// </summary>
        /// <typeparam name="T">인터페이스 이름</typeparam>
        /// <param name="data"></param>
        /// <returns>인터페이스 결과</returns>
        public static T ByteToStructure<T>(byte[] data) where T : struct
        {
            IntPtr buff = Marshal.AllocHGlobal(data.Length); // 배열의 크기만큼 비관리 메모리 영역에 메모리를 할당한다.
            Marshal.Copy(data, 0, buff, data.Length); // 배열에 저장된 데이터를 위에서 할당한 메모리 영역에 복사한다.
            T obj = (T)Marshal.PtrToStructure(buff, typeof(T)); // 복사된 데이터를 구조체 객체로 변환한다.
            Marshal.FreeHGlobal(buff); // 비관리 메모리 영역에 할당했던 메모리를 해제함

            if (Marshal.SizeOf(typeof(T)) != data.Length)// (((PACKET_DATA)obj).TotalBytes != data.Length) // 구조체와 원래의 데이터의 크기 비교
            {
                throw new Exception();
            }
            return obj; // 구조체 리턴
        }

        /// <summary>
        /// 구조체를 byte 배열로
        /// </summary>
        /// <param name="obj">구조체 데이터</param>
        /// <returns>Byte arrary</returns>
        public static byte[] StructureToByte(object obj)
        {
            int datasize = Marshal.SizeOf(obj);//((PACKET_DATA)obj).TotalBytes; // 구조체에 할당된 메모리의 크기를 구한다.
            IntPtr buff = Marshal.AllocHGlobal(datasize); // 비관리 메모리 영역에 구조체 크기만큼의 메모리를 할당한다.
            Marshal.StructureToPtr(obj, buff, false); // 할당된 구조체 객체의 주소를 구한다.
            byte[] data = new byte[datasize]; // 구조체가 복사될 배열
            Marshal.Copy(buff, data, 0, datasize); // 구조체 객체를 배열에 복사
            Marshal.FreeHGlobal(buff); // 비관리 메모리 영역에 할당했던 메모리를 해제함

            return data; // 배열을 리턴
        }
        #endregion

        #region 11. 데이터 테이블 중 Rows 데이터를 double 형태의 Array로 변환
        /// <summary>
        /// 데이터 테이블 중 Rows 데이터를 double 형태의 Array로 변환
        /// </summary>
        /// <param name="dt">데이터 테이블 이름</param>
        /// <param name="ColName">변환하고자 하는 컬럼명</param>
        /// <returns>double type의 Arrary </returns>
        public double[] GetDataTable_RawdataArry(DataTable dt, string ColName)
        {
            return dt.AsEnumerable().Select(s => s.Field<double>(ColName)).ToArray<double>();
        }
        #endregion

        #region 12. 데이터 테이블 중 찾고자 하는 데이터 중 근사값을 찾아 줌
        /// <summary>
        /// 데이터 테이블 중 찾고자 하는 데이터 중 근사값을 찾아 줌
        /// </summary>
        /// <param name="Data">검색하고자 하는 DataTable</param>
        /// <param name="Index">찾고자 하는 값</param>
        /// <param name="PositiveValue">찾고자 하는 양수 값, ref : 찾은 데이터 테이블 위치</param>
        /// <param name="NegativeValue">찾고자 하는 음수 값, ref : 찾은 데이터 테이블 위치</param>
        public void SerchbyApproximateValue(DataTable Data, double Index, ref int PositiveValue, ref int NegativeValue)
        {
            double Abs(double number) => (number < 0) ? -number : number;
            double min = double.MaxValue;
            double target = Index;

            for (int i = 0; i < Data.Rows.Count; i++)
            {
                double Value = Convert.ToDouble(Data.Rows[i][0]);

                if (Abs(Value - target) < min)
                {
                    min = Abs(Value - target); // 최소값 알고리즘
                    PositiveValue = i;
                }
            }

            min = double.MaxValue;
            target = Index * -1;

            for (int i = 0; i < Data.Rows.Count; i++)
            {
                double Value = Convert.ToDouble(Data.Rows[i][0]);

                if (Abs(Value - target) < min)
                {
                    min = Abs(Value - target); // 최소값 알고리즘
                    NegativeValue = i;
                }
            }
        }
        public void SerchbyApproximateValue(DataTable Data, string Columns, double Index, ref int PositiveValue, ref int NegativeValue)
        {
            double Abs(double number) => (number < 0) ? -number : number;
            double min = double.MaxValue;
            double target = Index;

            for (int i = 0; i < Data.Rows.Count; i++)
            {
                double Value = Convert.ToDouble(Data.Rows[i][Columns]);

                if (Abs(Value - target) < min)
                {
                    min = Abs(Value - target); // 최소값 알고리즘
                    PositiveValue = i;
                }
            }

            min = double.MaxValue;
            target = Index * -1;

            for (int i = 0; i < Data.Rows.Count; i++)
            {
                double Value = Convert.ToDouble(Data.Rows[i][Columns]);

                if (Abs(Value - target) < min)
                {
                    min = Abs(Value - target); // 최소값 알고리즘
                    NegativeValue = i;
                }
            }
        }
        #endregion

        #region 13. 시스템 부하 없이 지연하기
        /// <summary>
        /// 시스템 부하 없이 Delay 함수 MS
        /// </summary>
        /// <param name="MS">(단위 : MS)
        ///
        public DateTime Delays(int MS)
        {
            Thread.Sleep(MS);

            //DateTime ThisMoment = DateTime.Now;
            //TimeSpan duration = new TimeSpan(0, 0, 0, 0, MS);
            //DateTime AfterWards = ThisMoment.Add(duration);

            //while (AfterWards >= ThisMoment)
            //{
            //    System.Windows.Forms.Application.DoEvents();
            //    ThisMoment = DateTime.Now;
            //}

            return DateTime.Now;
        }
        #endregion

        #region 14. 각도 변환(Degree, Radian)
        public double DegreeToRadian(double angle)
        {
            return angle * (Math.PI / 180);
        }

        public double RadianToDegree(double Radian)
        {
            return Radian * (180 / Math.PI);
        }
        #endregion

        #region 15. 표준 편차 구하기 - GetStandardDeviation(valueArray, average)

        /// <summary>
        /// 표준 편차 구하기
        /// </summary>
        /// <param name="valueArray">값 배열</param>
        /// <param name="average">평균</param>
        /// <returns>표준 편차</returns>
        public double GetStandardDeviation(double[] valueArray, double average, int size)
        {
            int valueCount = size;

            if (valueCount == 0)
            {
                return 0d;
            }

            double standardDeviation = 0d;
            double variance = 0d;

            try
            {
                for (int i = 0; i < valueCount; i++)
                {
                    variance += Math.Pow(valueArray[i] - average, 2);
                }

                standardDeviation = Math.Sqrt(SafeDivide(variance, valueCount));
            }
            catch (Exception)
            {
                throw;
            }

            return standardDeviation;
        }

        /// <summary>
        /// 안전하게 나누기
        /// </summary>
        /// <param name="value1">값 1</param>
        /// <param name="value2">값 2</param>
        /// <returns>나눈 값</returns>
        private double SafeDivide(double value1, double value2)
        {
            double result = 0d;

            try
            {
                if ((value1 == 0) || (value2 == 0))
                {
                    return 0d;
                }

                result = value1 / value2;
            }
            catch
            {
            }

            return result;
        }
        #endregion

        #region 16. 함수 특정시간 초과 여부 확인
        /// <summary>
        /// 현재 시간을 비교하기 위해 현재 시간 측정
        /// </summary>
        DateTime ThisMoment = new DateTime();
        /// <summary>
        /// 함수 체크 시작 시간 변수
        /// </summary>
        DateTime AfterWards = new DateTime();
        /// <summary>
        /// 설정된 시간 이상으로 함수내부에서 수행 여부를 확인
        /// </summary>
        /// <param name="isStart">초기 시작시 ture, 함수 시간 초가 여부 확인시 false</param>
        /// <param name="CheckTime">설정 시간</param>
        /// <returns>true : 시간 초과, false : 설정 시간 이내 수행 중</returns>
        public bool FuctionTimerOverCheck(bool isStart, int CheckTime)
        {
            TimeSpan duration = new TimeSpan(0, 0, 0, CheckTime, 0);
            if (isStart)
            {
                ThisMoment = DateTime.Now;
                AfterWards = ThisMoment.Add(duration);
            }

            ThisMoment = DateTime.Now;

            if (ThisMoment >= AfterWards)
                return true;

            return false;
        }
        #endregion


        public InterPolate interPolation = new InterPolate();
        public DataTable ArcInterpolation(double StartXPos, double StartYPos, double TargetXPos, double TargetYPos, double Arc)
        {
            double Length = Math.Sqrt(Math.Pow(TargetXPos - StartXPos, 2) + Math.Pow(TargetYPos - StartYPos, 2));
            double AcrHeight = (Arc / 90.0) * Length;

            DataTable dtx = new DataTable();

            var x = (Math.Cos(DegreeToRadian(Arc)) * Length / 2);
            var y = (Math.Sin(DegreeToRadian(Arc)) * Length / 2);

            interPolation.y_k[0] = StartYPos;
            interPolation.y_k[1] = AcrHeight;
            interPolation.y_k[2] = TargetYPos;

            interPolation.x_k[0] = StartXPos;
            interPolation.x_k[1] = (TargetXPos - StartXPos) / 2;//   Length /2;
            interPolation.x_k[2] = TargetXPos;

            dtx = interPolation.GenerateInterpolate();

            return dtx;

        }

        public DataTable Linear_Interpolation(double StartXPos, double StartYPos, double TargetXPos, double TargetYPos, double InterScale = 100)
        {
            if (StartXPos == TargetXPos && StartYPos == TargetYPos)
                return null;

            double ScaleY = (TargetYPos - StartYPos) / InterScale;
            double ScaleX = (TargetXPos - StartXPos) / InterScale;

            DataTable InterData = new DataTable();
            DataRow Data = InterData.NewRow();

            InterData.Columns.Add(new DataColumn("X_Axis", typeof(double)));
            InterData.Columns.Add(new DataColumn("Y_Axis", typeof(double)));


            Data = InterData.NewRow();
            Data["X_Axis"] = StartXPos;
            Data["Y_Axis"] = StartYPos;
            InterData.Rows.Add(Data);


            for (int i = 1; i < InterScale + 1; i++)
            {
                Data = InterData.NewRow();
                if (i == 1)
                {
                    Data["X_Axis"] = StartXPos + ScaleX;
                    Data["Y_Axis"] = StartYPos + ScaleY;
                }
                else
                {
                    Data["X_Axis"] = ScaleX + Convert.ToDouble(InterData.Rows[i - 1]["X_Axis"]); ;
                    Data["Y_Axis"] = ScaleY + Convert.ToDouble(InterData.Rows[i - 1]["Y_Axis"]);
                }

                InterData.Rows.Add(Data);
            }

            if (InterData.Rows.Count > 0)
                return InterData;

            return null;
        }

        public class InterPolate
        {
            public int Sampling = 3;
            public int DataScale = 1000;
            public double[] y_k = null;
            public double[] x_k = null;
            public double maxVal = -1.0;
            public double minVal = 1.0;

            double[] yp = null;
            double[] xp = null;
            public const double deltaX = 0.4;    // sample time 

            public InterPolate()
            {
                y_k = new double[Sampling];
                x_k = new double[Sampling];
                yp = new double[DataScale];
                xp = new double[DataScale];
            }


            public void Interpolate_Init()
            {
                for (int i = 1; i <= Sampling; i++)
                {
                    x_k[i - 1] = deltaX * (double)i;
                }
            }


            double Interpolate(double xp)
            {
                double enumerator;
                double denominator;
                double yp;
                int i, k;
                yp = 0.0;
                for (i = 0; i < Sampling; i++)
                {
                    enumerator = 1.0;
                    denominator = 1.0;
                    /* calculate the Lk elements */
                    for (k = 0; k < Sampling; k++)
                    {
                        if (k != i)
                        {
                            /* enumerator and denominator for one L element */
                            enumerator = enumerator * (xp - x_k[k]);
                            denominator = denominator * (x_k[i] - x_k[k]);
                        }
                    }
                    /* put every thing thogether to yp*/
                    if (denominator != 0.0)
                        yp = yp + y_k[i] * enumerator / denominator;
                    else
                        yp = 0.0;
                }
                return yp;
            }

            public DataTable GenerateInterpolate()
            {
                DataTable InterData = new DataTable();
                DataRow Data = InterData.NewRow();

                InterData.Columns.Add(new DataColumn("X_Axis", typeof(double)));
                InterData.Columns.Add(new DataColumn("Y_Axis", typeof(double)));

                minVal = x_k[0];
                maxVal = x_k[Sampling - 1];
                for (int i = 0; i < DataScale + 1; i++)
                {
                    Data = InterData.NewRow();
                    var yp = Interpolate((double)(i) * x_k[Sampling - 1] / (double)(DataScale));
                    var xp = (double)(i) * x_k[Sampling - 1] / (double)(DataScale);
                    Data["X_Axis"] = xp;
                    Data["Y_Axis"] = yp;
                    InterData.Rows.Add(Data);
                }

                return InterData;
            }
        }

       
    }


}
