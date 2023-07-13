using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace test_nam
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            chart1.Series.Clear(); //default series를 삭제한다.
            Series series = chart1.Series.Add("graph1");
            series.ChartType = SeriesChartType.Line;
            series.MarkerSize = 2;
            series.Color = Color.Red;

            chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = false;

            chart1.ChartAreas[0].AxisX.Minimum = -36;//-36;
            chart1.ChartAreas[0].AxisY.Maximum = 36;
            chart1.ChartAreas[0].AxisY.Minimum = -30; // 335
            chart1.ChartAreas[0].AxisY.Maximum = 30;//345;


            List<string> x_value = new List<string>();
            List<string> y_value = new List<string>();

            for(int i = -20; i < 20; i++)
            {
                x_value.Add(Convert.ToString(i));
            }

            for (int i = -20; i < 20; i++)
            {
                x_value[i] = Convert.ToInt64(i);
            }

            for (int i=-20;i< 20; i++)
            {
                y_value.Add(Convert.ToString(i));
            }

            series.Points.DataBindXY(x_value, y_value);

            
        }
    }
}
