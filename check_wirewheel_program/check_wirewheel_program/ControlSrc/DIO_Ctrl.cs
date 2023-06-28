
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KIRO
{
    public class DIO_Ctrl
    {
        public EzMotorParam DIO01_Param = new EzMotorParam();
        public EzMotorParam DIO02_Param = new EzMotorParam();
        public EzMotorParam DIO03_Param = new EzMotorParam();
        public EzMotorParam DIO04_Param = new EzMotorParam();
        public List<EzMotorParam> m_EzMotorInfo = new List<EzMotorParam>();

        uint SetBuffer_DIO_01 = 0;
        uint ResetBuffer_DIO_01 = 0xffff00FF;
        uint SetBuffer_DIO_02 = 0;
        uint ResetBuffer_DIO_02 = 0xffff00FF;
        uint SetBuffer_DIO_03 = 0;
        uint ResetBuffer_DIO_03 = 0xffff00FF;
        uint SetBuffer_DIO_04 = 0;
        uint ResetBuffer_DIO_04 = 0xffff00FF;

        int DeviceNumber = 10;
        public bool Open()
        {
            bool ret;
            //DIO01_Param.DriverIP = Global.RobotParam.Mobile_System.SolenoidIP;
            //DIO01_Param.ControlName = CTRL_DIO.DIO_01;
            //m_EzMotorInfo.Add(DIO01_Param);

            //DIO02_Param.DriverIP = Global.RobotParam.Mobile_System.LED_Ctrl01_IP;
            //DIO02_Param.ControlName = CTRL_DIO.DIO_02;
            //m_EzMotorInfo.Add(DIO02_Param);

            //DIO03_Param.DriverIP = Global.RobotParam.Mobile_System.LED_Ctrl02_IP;
            //DIO03_Param.ControlName = CTRL_DIO.DIO_03;
            //m_EzMotorInfo.Add(DIO03_Param);

            //DIO04_Param.DriverIP = Global.RobotParam.Mobile_System.LED_Ctrl03_IP;
            //DIO04_Param.ControlName = CTRL_DIO.DIO_04;
            //m_EzMotorInfo.Add(DIO04_Param);

            DIO04_Param.DriverIP = Global.RobotParam.Mobile_System.m_WheelMotorIP;
            DIO04_Param.ControlName = CTRL_DIO.DIO_04;
            m_EzMotorInfo.Add(DIO04_Param);

            bool CheckConnect = false;
            foreach (EzMotorParam Dev in m_EzMotorInfo)
            {
                if (Dev.isbConnect == true)
                {
                    Global.ErrorList.Add(Dev.ControlName + ":" + Dev.DriverIP + "연결된 상태임.");
                    CheckConnect = true;
                }
            }

            if (CheckConnect)
                return false;

            string strmsg = string.Empty;
            foreach (EzMotorParam Dev in m_EzMotorInfo)
            {
                if (Dev.DriverIP != null)
                {
                    DeviceNumber++;
                    //if (EzDriver.Ez_Connect(Dev.DriverIP, DeviceNumber, pMain.AppInit.isbEzServoLog) == true)
                    if (Ez_Connect(Dev.DriverIP, DeviceNumber, false) == true)
                    {

                        Dev.DevNum = DeviceNumber;
                        Dev.isbConnect = true;
                        ret = Dev.isbConnect;


                    }
                    else
                    {
                        strmsg = string.Format(Dev.ControlName + ":" + Dev.DriverIP + " Not Connect");
                        Global.ErrorList.Add(strmsg);
                        ret = false;
                    }
                }

            }

            return false;
        }

        public void Close()
        {
            foreach (EzMotorParam Dev in m_EzMotorInfo)
                EziMOTIONPlusELib.FAS_Close(Dev.DevNum);
        }

        private bool Ez_Connect(string DevIp, int DevNum, bool isbLog = false)
        {
            EzMotorParam dev = new EzMotorParam();
            if (isbLog)
            {
                EziMOTIONPlusELib.FAS_SetLogPath(System.IO.Directory.GetCurrentDirectory());
                EziMOTIONPlusELib.FAS_SetLogLevel(EziMOTIONPlusELib.LOG_LEVEL.LOG_LEVEL_ALL);
            }

            if (EziMOTIONPlusELib.FAS_Connect(IPAddress.Parse(DevIp), DevNum) == false)
            {
                return false;
            }

            dev.DriverIP = DevIp;
            dev.DevNum = DevNum;
            dev.isbConnect = true;

            return true;
        }

        private int GetDIOCtrlNumber(string MotorName)
        {

            foreach (EzMotorParam Device in m_EzMotorInfo)
            {
                if (MotorName == Device.ControlName)
                {
                    return Device.DevNum;
                }
            }
            return 0;
        }

        public void SetCompressor(bool OnOff)
        {
            uint ComMaskBit = 0x00000100;
            if (OnOff)
            {
                SetBuffer_DIO_02 |= (uint)ComMaskBit;
                ResetBuffer_DIO_02 &= (uint)~ComMaskBit;
            }
            else
            {
                SetBuffer_DIO_02 &= (uint)~ComMaskBit;
                ResetBuffer_DIO_02 |= (uint)ComMaskBit;
            }

            EziMOTIONPlusELib.FAS_SetOutput(GetDIOCtrlNumber(CTRL_DIO.DIO_02), SetBuffer_DIO_02, ResetBuffer_DIO_02);
        }

        public void SetSolenoidCtrl(Solenoid_def Sol, bool OnOff)
        {
            if (OnOff)
            {
                SetBuffer_DIO_02 |= (uint)Sol;
                ResetBuffer_DIO_02 &= (uint)~Sol;
            }
            else
            {
                SetBuffer_DIO_02 &= (uint)~Sol;
                ResetBuffer_DIO_02 |= (uint)Sol;
            }


            EziMOTIONPlusELib.FAS_SetOutput(GetDIOCtrlNumber(CTRL_DIO.DIO_02), SetBuffer_DIO_02, ResetBuffer_DIO_02);
        }

        public void SetLedCtrl(LED_def Led, bool OnOff)
        {
            if (OnOff)
            {
               if( Led == LED_def.Bottom_Left || Led == LED_def.Bottom_Right || Led == LED_def.Elevatoin_Left || Led == LED_def.Elevatoin_Right)
                {
                    SetBuffer_DIO_04 |= (uint)Led;
                    ResetBuffer_DIO_04 &= (uint)~Led;
                }
                else if (Led != LED_def.Front_Center)
                {
                    SetBuffer_DIO_03 |= (uint)Led;
                    ResetBuffer_DIO_03 &= (uint)~Led;
                }
                else
                {
                    SetBuffer_DIO_02 |= (uint)Led;
                    ResetBuffer_DIO_02 &= (uint)~Led;
                }

            }
            else
            {

                if (Led == LED_def.Bottom_Left || Led == LED_def.Bottom_Right || Led == LED_def.Elevatoin_Left || Led == LED_def.Elevatoin_Right)
                {
                    SetBuffer_DIO_04 &= (uint)~Led;
                    ResetBuffer_DIO_04 |= (uint)Led;
                }
                else if (Led != LED_def.Front_Center)
                {
                    SetBuffer_DIO_03 &= (uint)~Led;
                    ResetBuffer_DIO_03 |= (uint)Led;
                }
                else
                {
                    SetBuffer_DIO_02 &= (uint)~Led;
                    ResetBuffer_DIO_02 |= (uint)Led;
                }
            }

            if (Led == LED_def.Bottom_Left || Led == LED_def.Bottom_Right || Led == LED_def.Elevatoin_Left || Led == LED_def.Elevatoin_Right)
                EziMOTIONPlusELib.FAS_SetOutput(GetDIOCtrlNumber(CTRL_DIO.DIO_04), SetBuffer_DIO_04, ResetBuffer_DIO_04);
            else if (Led != LED_def.Front_Center)
                EziMOTIONPlusELib.FAS_SetOutput(GetDIOCtrlNumber(CTRL_DIO.DIO_03), SetBuffer_DIO_03, ResetBuffer_DIO_03);
            else
                EziMOTIONPlusELib.FAS_SetOutput(GetDIOCtrlNumber(CTRL_DIO.DIO_02), SetBuffer_DIO_02, ResetBuffer_DIO_02);
        }

        public void SetCylinderCtrl(Cylinder_Def Cylinder, bool OnOff)
        {
            if (OnOff)
            {
                SetBuffer_DIO_04 |= (uint)Cylinder;
                ResetBuffer_DIO_04 &= (uint)~Cylinder;
            }
            else
            {
                SetBuffer_DIO_01 &= (uint)~Cylinder;
                ResetBuffer_DIO_01 |= (uint)Cylinder;
            }
            int ret = EziMOTIONPlusELib.FAS_SetOutput(GetDIOCtrlNumber(CTRL_DIO.DIO_01), SetBuffer_DIO_01, ResetBuffer_DIO_01);
            Console.WriteLine("Ret = " + ret);
        }
        public void SetCleaingPump(bool OnOff)
        {
            uint Pump = 0x00000100;
            if (OnOff)
            {
                SetBuffer_DIO_04 |= (uint)Pump;
                ResetBuffer_DIO_04 &= (uint)~Pump;
            }
            else
            {
                SetBuffer_DIO_04 &= (uint)~Pump;
                ResetBuffer_DIO_04 |= (uint)Pump;
            }
            int ret = EziMOTIONPlusELib.FAS_SetOutput(GetDIOCtrlNumber(CTRL_DIO.DIO_04), SetBuffer_DIO_04, ResetBuffer_DIO_04);
        }

        public void SetCylinderCM(Cylinder_Def Cylinder, bool OnOff)
        {
            if( Cylinder == Cylinder_Def.FrontLeftUp)
            {
                if (OnOff == false)
                    OnOff = true;

                SetCylinderCtrl(Cylinder_Def.FrontLeftDown, false);
                SetCylinderCtrl(Cylinder, !OnOff);
                Thread.Sleep(200);
                SetCylinderCtrl(Cylinder, OnOff);
            }
            else if (Cylinder == Cylinder_Def.FrontLeftDown)
            {
                if (OnOff == false)
                    OnOff = true;

                SetCylinderCtrl(Cylinder_Def.FrontLeftUp, false);
                SetCylinderCtrl(Cylinder, !OnOff);
                Thread.Sleep(200);
                SetCylinderCtrl(Cylinder, OnOff);
            }
            else if (Cylinder == Cylinder_Def.FrontRightUp)
            {
                if (OnOff == false)
                    OnOff = true;

                SetCylinderCtrl(Cylinder_Def.FrontRightDown, false);
                SetCylinderCtrl(Cylinder, !OnOff);
                Thread.Sleep(200);
                SetCylinderCtrl(Cylinder, OnOff);
            }
            else if (Cylinder == Cylinder_Def.FrontRightDown)
            {
                if (OnOff == false)
                    OnOff = true;

                SetCylinderCtrl(Cylinder_Def.FrontRightUp, false);
                SetCylinderCtrl(Cylinder, !OnOff);
                Thread.Sleep(200);
                SetCylinderCtrl(Cylinder, OnOff);
            }
            else if (Cylinder == Cylinder_Def.RearLeftUp)
            {
                if (OnOff == false)
                    OnOff = true;

                SetCylinderCtrl(Cylinder_Def.RearLeftDown, false);
                SetCylinderCtrl(Cylinder, !OnOff);
                Thread.Sleep(200);
                SetCylinderCtrl(Cylinder, OnOff);
            }
            else if (Cylinder == Cylinder_Def.RearLeftDown)
            {
                if (OnOff == false)
                    OnOff = true;

                SetCylinderCtrl(Cylinder_Def.RearLeftUp, false);
                SetCylinderCtrl(Cylinder, !OnOff);
                Thread.Sleep(200);
                SetCylinderCtrl(Cylinder, OnOff);
            }
            else if (Cylinder == Cylinder_Def.RearRightUp)
            {
                if (OnOff == false)
                    OnOff = true;

                SetCylinderCtrl(Cylinder_Def.RearRightDown, false);
                SetCylinderCtrl(Cylinder, !OnOff);
                Thread.Sleep(200);
                SetCylinderCtrl(Cylinder, OnOff);
            }
            else if (Cylinder == Cylinder_Def.RearRightDown)
            {
                if (OnOff == false)
                    OnOff = true;

                SetCylinderCtrl(Cylinder_Def.RearRightUp, false);
                SetCylinderCtrl(Cylinder, !OnOff);
                Thread.Sleep(200);
                SetCylinderCtrl(Cylinder, OnOff);
            }
        }

        public void SetCylinderCtrlMode(CylinderCtrlMode_def Mode, bool OnOff)
        {
            switch(Mode)
            {
                case CylinderCtrlMode_def.AllUp:
                    SetCylinderCM(Cylinder_Def.FrontLeftUp, OnOff);
                    SetCylinderCM(Cylinder_Def.FrontRightUp,OnOff);
                    SetCylinderCM(Cylinder_Def.RearLeftUp, OnOff);
                    SetCylinderCM(Cylinder_Def.RearRightUp, OnOff);
                    break;
                case CylinderCtrlMode_def.AllDown:

                    SetCylinderCM(Cylinder_Def.FrontLeftDown, OnOff);
                    SetCylinderCM(Cylinder_Def.FrontRightDown, OnOff);
                    SetCylinderCM(Cylinder_Def.RearLeftDown, OnOff);
                    SetCylinderCM(Cylinder_Def.RearRightDown, OnOff);
                    break;
                case CylinderCtrlMode_def.FrontUp:
                    SetCylinderCM(Cylinder_Def.FrontLeftUp, OnOff);
                    SetCylinderCM(Cylinder_Def.FrontRightUp, OnOff);
                    break;
                case CylinderCtrlMode_def.FrontDown:
                    SetCylinderCM(Cylinder_Def.FrontLeftDown, OnOff);
                    SetCylinderCM(Cylinder_Def.FrontRightDown, OnOff);
                    break;
                case CylinderCtrlMode_def.RearUp:
                    SetCylinderCM(Cylinder_Def.RearLeftUp, OnOff);
                    SetCylinderCM(Cylinder_Def.RearRightUp, OnOff);
                    break;
                case CylinderCtrlMode_def.RearDown:
                    SetCylinderCM(Cylinder_Def.RearLeftDown, OnOff);
                    SetCylinderCM(Cylinder_Def.RearRightDown, OnOff);
                    break;
                case CylinderCtrlMode_def.Left_DiagonalUp:
                    SetCylinderCM(Cylinder_Def.FrontLeftUp, OnOff);
                    SetCylinderCM(Cylinder_Def.RearRightUp, OnOff);
                    break;
                case CylinderCtrlMode_def.Left_DiagonalDown:
                    SetCylinderCM(Cylinder_Def.FrontLeftDown, OnOff);
                    SetCylinderCM(Cylinder_Def.RearRightDown, OnOff);
                    break;
                case CylinderCtrlMode_def.Right_DiagonalUp:
                    SetCylinderCM(Cylinder_Def.FrontRightUp, OnOff);
                    SetCylinderCM(Cylinder_Def.RearLeftUp, OnOff);
                    break;
                case CylinderCtrlMode_def.Right_DiagonalDown:
                    SetCylinderCM(Cylinder_Def.FrontRightDown, OnOff);
                    SetCylinderCM(Cylinder_Def.RearLeftDown, OnOff);
                    break;

            }
        }
    }


}
