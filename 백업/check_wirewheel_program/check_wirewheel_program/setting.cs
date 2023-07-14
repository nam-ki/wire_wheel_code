using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KIRO;

namespace check_wirewheel_program
{
    public partial class setting : Form
    {
        public setting()
        {
            InitializeComponent();
        }

        private void setting_Load(object sender, EventArgs e)
        {
            PG_System.SelectedObject = Global.RobotParam.Mobile_System;
        }

        private void PG_System_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            Global.RobotParam.Mobile_System = (Mobile_System_def)PG_System.SelectedObject;
            Global.RobotParam.SetSystemParam(Global.RobotParam.Mobile_System, Global.RobotParam.Mobile_System.GetType());
        }
    }
}
