
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KIRO
{
    public class ElevationCtrl
    {
        // 컨트롤 변수 선언
        
        private Thread th_ToolCtrl = null;
        public int DeviceNumber = 0;

        // 모터 컨트롤 전역 변수 선언
        public List<EzMotorParam> m_EzMotorInfo = new List<EzMotorParam>();
        //public event StatusLogEvent StatusEvent;
        public EzMotorParam WheelParam = new EzMotorParam();
        //public EzMotorParam RotationParam = new EzMotorParam();
        //public EzMotorParam FrontParam = new EzMotorParam();
        //public EzMotorParam LeftParam = new EzMotorParam();
        //public EzMotorParam RightParam = new EzMotorParam();
        //public EzMotorParam AdhesieParam = new EzMotorParam();

        #region // 각 모터 기구적 파라메터 정의

        public const double FASTECH_ENCODER = 10000;
        public const double FASTECH_Elevation_Reduction = 10;
        public const double FASTECH_Rotation_Reduction = 71;
        public const double FASTECH_Adhesier_Reduction = 3;
        public const double FASTECH_Tilt_Reduction = 50;
        public const double FASTECH_Elevation_Lead = 10;
        public const double FASTECH_Scanner_Lead = 2;
        public const double FASTECH_Adhesier_Lead = 5;

        public const double FASTECH_CPSTORPM_XAxis = (0.00045 / 360) * 60;  // Encdoer 20000ppr & Gear ratio 1:40 based 0.00045
        public const double FASTECH_CPSTORPM = (0.036 / 360) * 60;      // Encoder 10000ppr based 0.036
        public const double FASTECH_CPSTORPM_SCANNER = (0.036 / 360) * 60;      // Encoder 10000ppr based 0.036


        public const double ScalePulse_Elvation = (FASTECH_ENCODER / FASTECH_Elevation_Lead) * FASTECH_Elevation_Reduction;
        public const double ScalePulse_Tilt= (FASTECH_ENCODER / FASTECH_Elevation_Lead) * FASTECH_Tilt_Reduction;
        public const double ScalePulse_Rotation = (FASTECH_ENCODER * FASTECH_Elevation_Lead * FASTECH_Rotation_Reduction) / 360;
        public const double ScalePulse_Adhesie = (FASTECH_ENCODER * FASTECH_Elevation_Reduction * FASTECH_Adhesier_Reduction) / FASTECH_Adhesier_Lead;
        #endregion

        // 초기화 각 축 거리 정의
        public const double ScannerMaxStroke = 630;
        private const double HomePos_Shaving = 226.5;
        private const double HomePos_Grainding = 223.5;
        //private const double HomePoEle_Front = 200;
        private const double HomePoEle_Rotation = 30;
        private const double HomePos_FrontScanner = 317;
        private const double HomePos_RealScanner = 318;

        // 전역 변수 선언
        private bool isShavingHomeStart = false;
        public bool isGraindingHome = false;
        public bool isShavingHome = false;


        // Tool Control Mathode
        public ToolHomeSeq ToolHome = new ToolHomeSeq();

        public ElevationCtrl(object obj)
        {
            EzDriver = new EzServo_Driver(this);
        }

        #region // 각 모터 축 단위 변화 mmsec -> cps, mm/s -> cps
        public uint mmSecTocps_Elevation(double mmsec)
        {
            double rpm = (mmsec / FASTECH_Elevation_Lead) * 60;

            return (uint)(rpm / FASTECH_CPSTORPM);
        }

        public uint mmSecTocps_Rotation(double rpm)
        {
            return (uint)(rpm / FASTECH_CPSTORPM);
        }


        private int GetMotorCtrlNumber(string MotorName)
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

        private EzMotorParam GetMotorParamHandel(string MotorName)
        {

            foreach (EzMotorParam Device in m_EzMotorInfo)
            {
                if (MotorName == Device.ControlName)
                {
                    return Device;
                }
            }
            return null;
        }
        #endregion

        #region 1. DeviceOpen : 툴 제어기를 선택 및 장비를 연결 합니다.
        /// <summary>
        /// <para>툴 제어기를 선택 합니다. </para>
        /// <para>성공시 : 장비 연결, 모터 정지, 서버 온 상태를 진행을 함</para>
        /// </summary>
        /// <param name="CtrlNum">"쉐이빙" 또는 "그라인딩"제어기 번호를 입력 합니다.</param>
        /// <returns></returns>
        public bool DeviceOpen(uint CtrlNum = 0)
        {
            bool ret = false;

            WheelParam.DriverIP = Global.RobotParam.Mobile_System.m_WheelMotorIP;
            WheelParam.ControlName = CTRL_NUM.Mot_Wheel;
            WheelParam.HomePos = new ToolHomeSeq();
            m_EzMotorInfo.Add(WheelParam);

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
                    if (EzDriver.Ez_Connect(Dev.DriverIP, DeviceNumber, false) == true)
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

            EzDriver.Ez_Start();

            foreach (EzMotorParam Dev in m_EzMotorInfo)
            {
                EzDriver.EZ_SetAlarmClear(Dev.DevNum);
                Global.pUtilt.Delays(100);
            }

            th_ToolCtrl = new Thread(Thread_ToolCtrl);
            th_ToolCtrl.Start();

            SetServoOnOffAll(true);

            return ret;
        }
        #endregion

        #region 2. DeviceClose : 제어기 연결 해제(종료)
        public void DeviceClose()
        {
            try
            {
                if (th_ToolCtrl != null)
                {
                    //th_ToolCtrl.Join();
                    th_ToolCtrl.Abort();
                }

                foreach (EzMotorParam Dev in m_EzMotorInfo)
                {
                    EzDriver.Ez_SetMotorStop(Dev.DevNum);
                    EzDriver.Ez_SetServo(Dev.DevNum, false);
                }

                EzDriver.Ez_Close();
            }
            catch (Exception ex)
            {
                //Global.Log.Fatal($"An exception occurred from {MethodBase.GetCurrentMethod().Name}", ex);
            }

        }
        #endregion

        #region 3. SetMotorStop : 모터 정지
        public void SetMotorStop(String MotorName)
        {
            EzDriver.Ez_SetMotorStop(GetMotorCtrlNumber(MotorName));
        }

        public void SetMotorStop(uint Module)
        {
            //EzDriver.Ez_SetMotorStop(GetMotorCtrlNumber(CTRL_NUM.Ele_Tilt));
            //EzDriver.Ez_SetMotorStop(GetMotorCtrlNumber(CTRL_NUM.Ele_Rotation));
            //EzDriver.Ez_SetMotorStop(GetMotorCtrlNumber(CTRL_NUM.Ele_Front));
            //EzDriver.Ez_SetMotorStop(GetMotorCtrlNumber(CTRL_NUM.Ele_Left));
            //EzDriver.Ez_SetMotorStop(GetMotorCtrlNumber(CTRL_NUM.Ele_Right));
            //EzDriver.Ez_SetMotorStop(GetMotorCtrlNumber(CTRL_NUM.Ele_Adhesive));
            EzDriver.Ez_SetMotorStop(GetMotorCtrlNumber(CTRL_NUM.Mot_Wheel));
        }
        #endregion

        #region 4. SetJogMove : 조그 모드 제어

        private uint GetMotorBasedPPM(String MotorName, uint Velo = 0)
        {
            //if (MotorName == CTRL_NUM.Ele_Rotation)
            //    Velo = mmSecTocps_Rotation(Velo);
            //else if (MotorName == CTRL_NUM.Ele_Front || MotorName == CTRL_NUM.Ele_Left || MotorName == CTRL_NUM.Ele_Right)
            //    Velo = mmSecTocps_Elevation(Velo);
            //else if (MotorName == CTRL_NUM.Ele_Tilt || MotorName == CTRL_NUM.Ele_Adhesive)
            //    Velo = mmSecTocps_Elevation(Velo);

            if (MotorName == CTRL_NUM.Mot_Wheel)
                    Velo = mmSecTocps_Rotation(Velo);

            return Velo;
        }

        public void SetJogMove(String MotorName, int CCW, uint Velo = 0)
        {
            EzDriver.MT_JogMove(GetMotorCtrlNumber(MotorName), CCW, GetMotorBasedPPM(MotorName, Velo));
        }
        #endregion

        #region 5. SetAbsPositionMove : 절대 위치로 이동
        public void SetAbsPositionMove(String MotorName, bool CW_CCW, uint Velo, double Step)
        {
            EzDriver.MT_PostionAbsStepMove(GetMotorCtrlNumber(MotorName), CW_CCW, GetMotorBasedPPM(MotorName, Velo),
                ConverterToPPS(MotorName, Step));
        }

        public void SetAbsPositionMove(String MotorName, uint Velo, double Step)
        {
            bool CW_CCW = true;

            EzDriver.MT_PostionAbsStepMove(GetMotorCtrlNumber(MotorName), CW_CCW, GetMotorBasedPPM(MotorName, Velo), 
                ConverterToPPS(MotorName, Step));
        }

        public double ConverterToPPS(string MotorName, double Pos)
        {
            foreach (EzMotorParam Device in m_EzMotorInfo)
            {
                //if (MotorName == CTRL_NUM.Ele_Tilt)
                //{
                //    return (Pos * ScalePulse_Tilt);
                //}
                //else if (MotorName == CTRL_NUM.Ele_Front)
                //{
                //    return Pos * ScalePulse_Elvation;
                //}
                //else if (MotorName == CTRL_NUM.Ele_Rotation)
                //{
                //    return Pos * ScalePulse_Rotation;
                //}
                //else if (MotorName == CTRL_NUM.Ele_Left)
                //{
                //    return (Pos * ScalePulse_Elvation);
                //}
                //else if (MotorName == CTRL_NUM.Ele_Right)
                //{
                //    return (Pos * ScalePulse_Elvation);
                //}
                //else if (MotorName == CTRL_NUM.Ele_Adhesive)
                //{
                //    return (Pos * ScalePulse_Adhesie);
                //}

                if (MotorName == CTRL_NUM.Mot_Wheel)
                {
                    return (Pos * ScalePulse_Tilt);
                }
            }
            return 0;
        }

        #endregion

        #region 6. GetPosition : 현재 위치 알기

        public double GetPosition(string MotorName)
        {
            foreach (EzMotorParam Device in m_EzMotorInfo)
            {
                //if (MotorName == Device.ControlName && MotorName == CTRL_NUM.Ele_Tilt)
                //{
                //    return Device.MotionState.ActualPos / ScalePulse_Tilt;
                //}
                //else if (MotorName == Device.ControlName && MotorName == CTRL_NUM.Ele_Front)
                //{
                //    return (Device.MotionState.ActualPos / ScalePulse_Elvation);
                //}
                //else if (MotorName == Device.ControlName && MotorName == CTRL_NUM.Ele_Rotation)
                //{
                //    return Device.MotionState.ActualPos / ScalePulse_Rotation;
                //}
                //else if (MotorName == Device.ControlName && MotorName == CTRL_NUM.Ele_Left)
                //{
                //    return (Device.MotionState.ActualPos / ScalePulse_Elvation);
                //}
                //else if (MotorName == Device.ControlName && MotorName == CTRL_NUM.Ele_Right)
                //{
                //    return (Device.MotionState.ActualPos / ScalePulse_Elvation);
                //}
                //else if (MotorName == Device.ControlName && MotorName == CTRL_NUM.Ele_Adhesive)
                //{
                //    return (Device.MotionState.ActualPos / ScalePulse_Adhesie);
                //}

                if (MotorName == Device.ControlName && MotorName == CTRL_NUM.Mot_Wheel)
                {
                    return Device.MotionState.ActualPos / ScalePulse_Tilt;
                }
            }
            return 0;
        }
        #endregion

        #region 7. SetServoOnOff : 서보 On/Off 제어
        public bool SetServoOnOff(String MotorName, bool OnOff)
        {
            return EzDriver.Ez_SetServo(GetMotorCtrlNumber(MotorName), OnOff);
        }

        public void SetServoOnOffAll(bool OnOff = false)
        {
            //EzDriver.Ez_SetServo(GetMotorCtrlNumber(CTRL_NUM.Ele_Left), OnOff);
            //EzDriver.Ez_SetServo(GetMotorCtrlNumber(CTRL_NUM.Ele_Right), OnOff);
            //EzDriver.Ez_SetServo(GetMotorCtrlNumber(CTRL_NUM.Ele_Tilt), OnOff);
            //EzDriver.Ez_SetServo(GetMotorCtrlNumber(CTRL_NUM.Ele_Front), OnOff);
            //EzDriver.Ez_SetServo(GetMotorCtrlNumber(CTRL_NUM.Ele_Rotation), OnOff);
            //EzDriver.Ez_SetServo(GetMotorCtrlNumber(CTRL_NUM.Ele_Adhesive), OnOff);

            EzDriver.Ez_SetServo(GetMotorCtrlNumber(CTRL_NUM.Mot_Wheel), OnOff);


        }



        public (string[], int) GetServoOnCheckAll()
        {
            string[] ErrorList = new string[5];
            int ErrorCnt = 0;

            foreach (EzMotorParam Device in m_EzMotorInfo)
            {
                if (Device.DriverIP != null)
                {
                    if (Device.AlarmInfo.isbServoOn == false)
                    {
                        ErrorList[ErrorCnt] = Device.ControlName;
                        ErrorCnt++;
                    }
                }

            }
            return (ErrorList, ErrorCnt);
        }
        #endregion

        #region 8. SetHomePosition : 축 홈 위치로 이동
        public void SetHomePosition(String MotorName)
        {
            EzDriver.MT_HomePosition(GetMotorCtrlNumber(MotorName));
        }
        #endregion

        #region 9. SetHomePosition : 4개축을 동시헤 홈 위치로 이동

        public bool SetHomePosition(uint TargetNum)
        {
            if (isShavingHomeStart == false)
            {
                //SetHomePosition(CTRL_NUM.Ele_Tilt);
                //Thread.Sleep(100);
                //SetHomePosition(CTRL_NUM.Ele_Left);
                //Thread.Sleep(100);
                //SetHomePosition(CTRL_NUM.Ele_Rotation);

                SetHomePosition(CTRL_NUM.Mot_Wheel);

                isShavingHomeStart = true;
            }

            return false;
        }
        #endregion

        #region 10. SetVelocity : 홈 포지션 이동, 조그 모터 속도 설정
        public void SetVelocity(uint XAxis, uint YAxis, uint ZAxis, uint Scanner)
        {
            EzDriver.MoveVelocityX = XAxis;
            EzDriver.MoveVelocityY = YAxis;
            EzDriver.MoveVelocityZ = ZAxis;
            EzDriver.MoveVelocityScanner = Scanner;
        }
        #endregion

        #region 11. 모터 서버 알람 클리어
        public void SetAlarmClear(int TargetNum)
        {
            EzDriver.EZ_SetAlarmClear(TargetNum); 
        }

        public void SetAlarmClearAll()
        {
            //EzDriver.EZ_SetAlarmClear(GetMotorCtrlNumber(CTRL_NUM.Ele_Tilt));
            //EzDriver.EZ_SetAlarmClear(GetMotorCtrlNumber(CTRL_NUM.Ele_Left));
            //EzDriver.EZ_SetAlarmClear(GetMotorCtrlNumber(CTRL_NUM.Ele_Right));
            //EzDriver.EZ_SetAlarmClear(GetMotorCtrlNumber(CTRL_NUM.Ele_Tilt));
            //EzDriver.EZ_SetAlarmClear(GetMotorCtrlNumber(CTRL_NUM.Ele_Front));
            //EzDriver.EZ_SetAlarmClear(GetMotorCtrlNumber(CTRL_NUM.Ele_Rotation));
            //EzDriver.EZ_SetAlarmClear(GetMotorCtrlNumber(CTRL_NUM.Ele_Adhesive));
            EzDriver.EZ_SetAlarmClear(GetMotorCtrlNumber(CTRL_NUM.Mot_Wheel));

        }

        public void SetAlarmClear(string TargetNum)
        {
            EzDriver.EZ_SetAlarmClear(GetMotorCtrlNumber(TargetNum)); 
        }

        #endregion

        #region 12. GetPositionError : 축 이동시 포지션 에러값
        public int GetPositionError(String MotorName)
        {
            
            return EzDriver.MT_PositionError(GetMotorCtrlNumber(MotorName));
        }
        #endregion

        #region 13. SetPostionClear : 현재 위치 초기화
        public void SetPostionClear(String MotorName)
        {

            EzDriver.SetPositionClear(GetMotorCtrlNumber(MotorName));
        }

        private void Thread_ToolCtrl()
        {
            while (th_ToolCtrl.IsAlive)
            {
                Thread.Sleep(1);

                //foreach (EzMotorParam Dev in m_EzMotorInfo)
                //    HomePosition(Dev.HomePos, Dev);
            }
        }
        #endregion

        #region 14. SetUserHomePosition : 사용자 홈 포지션 메서드
        public void SetUserHomePosition(String MotorName)
        {
            EzMotorParam Home = GetMotorParamHandel(MotorName);

            Home.HomePos.isStart = true;
            Home.HomePos.nStep = 0;
        } 
        #endregion

    }
}
