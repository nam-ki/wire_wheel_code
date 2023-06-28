
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace KIRO
{
    public class Global
    {
        
        public static RobotProperty RobotParam = new RobotProperty();
        
        
        public static List<string> ErrorList = new List<string>();

        #region 파라메터 XML 형태 읽고/쓰기
        public class RobotProperty
        {
            // 주요 파라메터 컨트롤 선언 
            public Mobile_System_def Mobile_System = new Mobile_System_def();
            public Mobile_Setting Mobile_Setting = new Mobile_Setting();
            public void GetSystemParam()
            {
                string strSystemParam = "SystemParameter";
                string strLocalFolder = Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\"));
                string strXMLFile = String.Empty;

                try
                {
                    strXMLFile = "\\Config\\" + strSystemParam + ".xml";
                    var serializer = new XmlSerializer(typeof(Mobile_System_def));
                    using (var reader = XmlReader.Create(strLocalFolder + strXMLFile))
                    {
                        Global.RobotParam.Mobile_System = (Mobile_System_def)serializer.Deserialize(reader);
                    }
                }
                catch (Exception ex)
                {
                    //Global.Log.Fatal($"An exception occurred from {MethodBase.GetCurrentMethod().Name}", ex);
                }
            }

            public void GetSettingParam()
            {
                string strSystemParam = "SettingParameter";
                string strLocalFolder = Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\"));
                string strXMLFile = String.Empty;

                try
                {
                    strXMLFile = "\\Config\\" + strSystemParam + ".xml";
                    var serializer = new XmlSerializer(typeof(Mobile_Setting));
                    using (var reader = XmlReader.Create(strLocalFolder + strXMLFile))
                    {
                        Global.RobotParam.Mobile_Setting = (Mobile_Setting)serializer.Deserialize(reader);
                    }
                }
                catch (Exception ex)
                {
                    //Global.Log.Fatal($"An exception occurred from {MethodBase.GetCurrentMethod().Name}", ex);
                }
            }


            public void SetSystemParam(object obj, Type type)
            {
                string strSystemParam = "SystemParameter";
                string strLocalFolder = Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\"));
                string strXMLFile = "\\Config\\";

                if (!Directory.Exists(strLocalFolder + strXMLFile))
                    Directory.CreateDirectory(strLocalFolder + strXMLFile);

                if (obj.GetType() == Global.RobotParam.Mobile_System.GetType())
                    strXMLFile = "\\Config\\" + strSystemParam + ".xml";

                using (StreamWriter wr = new StreamWriter(strLocalFolder + strXMLFile))
                {
                    var ns = new XmlSerializerNamespaces();
                    ns.Add(string.Empty, string.Empty);

                    XmlSerializer xs = new XmlSerializer(type);
                    xs.Serialize(wr, obj, ns);
                }
            }

            public void SetSettingParam(object obj, Type type)
            {
                string strSystemParam = "SettingParameter";
                string strLocalFolder = Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf("\\"));
                string strXMLFile = "\\Config\\";

                if (!Directory.Exists(strLocalFolder + strXMLFile))
                    Directory.CreateDirectory(strLocalFolder + strXMLFile);

                if (obj.GetType() == Global.RobotParam.Mobile_Setting.GetType())
                    strXMLFile = "\\Config\\" + strSystemParam + ".xml";

                using (StreamWriter wr = new StreamWriter(strLocalFolder + strXMLFile))
                {
                    var ns = new XmlSerializerNamespaces();
                    ns.Add(string.Empty, string.Empty);

                    XmlSerializer xs = new XmlSerializer(type);
                    xs.Serialize(wr, obj, ns);
                }
            }
        }
        #endregion

        
    }

    #region 모바일 로봇 시스템 정보 정의
    public class Mobile_System_def
    {
        //public string m_WheelMotorIP;
        //[CategoryAttribute("IP 설정"),
        //DisplayName("Wheel Motor IP 설정"),
        //Description("삭륜 회전 모터 연결을 위한 IP 주소를 설정합니다.")]
        //public string WheelMotorIP
        //{
        //    get { return m_WheelMotorIP; }
        //    set { m_WheelMotorIP = value; }
        //}

        //public string m_ScannerIP;
        //[CategoryAttribute("IP 설정"),
        //DisplayName("Scanner IP 설정"),
        //Description("삭륜 검사 레이저 스캐너 연결을 위한 IP 주소를 설정합니다.")]
        //public string ScannerIP
        //{
        //    get { return m_ScannerIP; }
        //    set { m_ScannerIP = value; }
        //}

        //public double m_RotationTrial;
        //[CategoryAttribute("삭륜 검사 조건"),
        //DisplayName("검사 회전 횟수 설정"),
        //Description("삭륜 검사 시 회전 횟수를 설정합니다.")]
        //public double RotationTrial
        //{
        //    get { return m_RotationTrial; }
        //    set { m_RotationTrial = value; }
        //}

        //public uint m_RotationVelocity;
        //[CategoryAttribute("삭륜 검사 조건"),
        //DisplayName("검사 회전 속도 설정"),
        //Description("삭륜 검사 시 회전 속도를 설정합니다.")]
        //public uint RotationVelocity
        //{
        //    get { return m_RotationVelocity; }
        //    set { m_RotationVelocity = value; }
        //}

        //public uint m_DataLoggingInterval;
        //[CategoryAttribute("삭륜 검사 조건"),
        //DisplayName("로깅 주기 설정"),
        //Description("데이터 로깅 시 로그 주기[ms]를 설정합니다. ")]
        //public uint DataLoggingInterval
        //{
        //    get { return m_DataLoggingInterval; }
        //    set { m_DataLoggingInterval = value; }
        //}

        public int m_ChartXMin;
        [CategoryAttribute("차트 설정"),
        DisplayName("차트 X축 Min값"),
        Description("차트 X축 Min 값 설정")]
        public int ChartXMin
        {
            get { return m_ChartXMin; }
            set { m_ChartXMin = value; }
        }

        public int m_ChartXMax;
        [CategoryAttribute("차트 설정"),
        DisplayName("차트 X축 Max값"),
        Description("차트 X축 Max 값 설정")]
        public int ChartXMax
        {
            get { return m_ChartXMax; }
            set { m_ChartXMax = value; }
        }

        public int m_ChartYMax;
        [CategoryAttribute("차트 설정"),
        DisplayName("차트 Y축 Max값"),
        Description("차트 Y축 Max값 설정")]
        public int ChartYMax
        {
            get { return m_ChartYMax; }
            set { m_ChartYMax = value; }
        }

        public int m_ChartYMin;
        [CategoryAttribute("차트 설정"),
        DisplayName("차트 Y축 Min값"),
        Description("차트 Y축 Min값 설정")]
        public int ChartYMin
        {
            get { return m_ChartYMin; }
            set { m_ChartYMin = value; }
        }

        //public string m_ElevationTiltIP;
        //[CategoryAttribute("모터 드라이버 IP"),
        //DisplayName("Elevation Tilt IP설정")]
        //public string ElevationTiltIP
        //{
        //    get { return m_ElevationTiltIP; }
        //    set { m_ElevationTiltIP = value; }
        //}

        //public string m_ElevationRotationIP;
        //[CategoryAttribute("모터 드라이버 IP"),
        //DisplayName("Elevation Rotation IP설정")]
        //public string ElevationRotationIP
        //{
        //    get { return m_ElevationRotationIP; }
        //    set { m_ElevationRotationIP = value; }
        //}

        //public string m_ElevationFrontIP;
        //[CategoryAttribute("모터 드라이버 IP"),
        //DisplayName("Elevation Front IP설정")]
        //public string ElevationFrontIP
        //{
        //    get { return m_ElevationFrontIP; }
        //    set { m_ElevationFrontIP = value; }
        //}

        //public string m_ElevationLeftIP;
        //[CategoryAttribute("모터 드라이버 IP"),
        //DisplayName("Elevation Left IP설정")]
        //public string ElevationLeftIP
        //{
        //    get { return m_ElevationLeftIP; }
        //    set { m_ElevationLeftIP = value; }
        //}

        //public string m_ElevationRightIP;
        //[CategoryAttribute("모터 드라이버 IP"),
        //DisplayName("Elevation Right IP설정")]
        //public string ElevationRightIP
        //{
        //    get { return m_ElevationRightIP; }
        //    set { m_ElevationRightIP = value; }
        //}

        //public string m_AdhesiveIP;
        //[CategoryAttribute("모터 드라이버 IP"),
        //DisplayName("접착제 모터 IP 설정")]
        //public string AdhesiveIP
        //{
        //    get { return m_AdhesiveIP; }
        //    set { m_AdhesiveIP = value; }
        //}

        //public string m_SolenoidIP;
        //[CategoryAttribute("디지털 출력모듈 IP"),
        //DisplayName("Solenoid Valve IP 설정")]
        //public string SolenoidIP
        //{
        //    get { return m_SolenoidIP; }
        //    set { m_SolenoidIP = value; }
        //}

        //public string m_LED_Ctrl01_IP;
        //[CategoryAttribute("디지털 출력모듈 IP"),
        //DisplayName("LED Control 01 IP 설정")]
        //public string LED_Ctrl01_IP
        //{
        //    get { return m_LED_Ctrl01_IP; }
        //    set { m_LED_Ctrl01_IP = value; }
        //}

        //public string m_LED_Ctrl02_IP;
        //[CategoryAttribute("디지털 출력모듈 IP"),
        //DisplayName("LED Control 02 IP 설정")]
        //public string LED_Ctrl02_IP
        //{
        //    get { return m_LED_Ctrl02_IP; }
        //    set { m_LED_Ctrl02_IP = value; }
        //}

        //public string m_LED_Ctrl03_IP;
        //[CategoryAttribute("디지털 출력모듈 IP"),
        //DisplayName("LED Control 03 IP 설정")]
        //public string LED_Ctrl03_IP
        //{
        //    get { return m_LED_Ctrl03_IP; }
        //    set { m_LED_Ctrl03_IP = value; }
        //}

        //public string m_LED_Ctrl04_IP;
        //[CategoryAttribute("디지털 출력모듈 IP"),
        //DisplayName("LED Control 04 IP 설정")]
        //public string LED_Ctrl04_IP
        //{
        //    get { return m_LED_Ctrl04_IP; }
        //    set { m_LED_Ctrl04_IP = value; }
        //}

        //public string m_BatteryCom_01;
        //[CategoryAttribute("배터리 확인용 포터 설정"),
        //DisplayName("배터리 1번 Comport")]
        //public string BatteryCom_01
        //{
        //    get { return m_BatteryCom_01; }
        //    set { m_BatteryCom_01 = value; }
        //}

        //public string m_BatteryCom_02;
        //[CategoryAttribute("배터리 확인용 포터 설정"),
        //DisplayName("배터리 2번 Comport")]
        //public string BatteryCom_02
        //{
        //    get { return m_BatteryCom_02; }
        //    set { m_BatteryCom_02 = value; }
        //}

        //public string m_IMU_Port;
        //[CategoryAttribute("기타 제어 항목 설정"),
        //DisplayName("IMU Port 설정")]
        //public string IMU_Port
        //{
        //    get { return m_IMU_Port; }
        //    set { m_IMU_Port = value; }
        //}

        //public string m_Dynamixel_Port;
        //[CategoryAttribute("기타 제어 항목 설정"),
        //DisplayName("Dynamixel Comport 설정")]
        //public string Dynamixel_Port
        //{
        //    get { return m_Dynamixel_Port; }
        //    set { m_Dynamixel_Port = value; }
        //}

        //public string m_LneScanner_Port;
        //[CategoryAttribute("기타 제어 항목 설정"),
        //DisplayName("Line Scanner 설정")]
        //public string LneScanner_Port
        //{
        //    get { return m_LneScanner_Port; }
        //    set { m_LneScanner_Port = value; }
        //}

        //public string m_ServerIP;
        //[CategoryAttribute("기타 제어 항목 설정"),
        //DisplayName("로컬 서버 IP 설정")]
        //public string ServerIP
        //{
        //    get { return m_ServerIP; }
        //    set { m_ServerIP = value; }
        //}

        //public string m_IMUPort;
        //[CategoryAttribute("기타 제어 항목 설정"),
        //DisplayName("IMU Serial Port 설정")]
        //public string IMUPort
        //{
        //    get { return m_IMUPort; }
        //    set { m_IMUPort = value; }
        //}

    }
    #endregion

    #region 모바일 로봇 구동 파라메터
    public class Mobile_Setting
    {
        //public string m_MobileVelocity;
        //[CategoryAttribute("모바일 로봇 주행 파라메터"),
        //DisplayName("기본 주행 속도 (mm/s)")]
        //public string MobileVelocity
        //{
        //    get { return m_MobileVelocity; }
        //    set { m_MobileVelocity = value; }
        //}

        //public string m_MobileRotationVelo;
        //[CategoryAttribute("모바일 로봇 주행 파라메터"),
        //DisplayName("회전 기본 속도")]
        //public string MobileRotationVelo
        //{
        //    get { return m_MobileRotationVelo; }
        //    set { m_MobileRotationVelo = value; }
        //}

        //public string m_MobileRotationHome;
        //[CategoryAttribute("모바일 로봇 주행 파라메터"),
        //DisplayName("홈포지션 기본 속도")]
        //public string MobileRotationHome
        //{
        //    get { return m_MobileRotationHome; }
        //    set { m_MobileRotationHome = value; }
        //}

        //public string m_JoystickDrivingMax;
        //[CategoryAttribute("모바일 로봇 주행 파라메터"),
        //DisplayName("조이스틱 주행 최고 속도")]
        //public string JoystickDrivingMax
        //{
        //    get { return m_JoystickDrivingMax; }
        //    set { m_JoystickDrivingMax = value; }
        //}


        //public string m_ElevationVelociy;
        //[CategoryAttribute("엘리베이션 축 기본 파라메터"),
        //DisplayName("기본 동작 속도 (mm/s)")]
        //public string ElevationVelociy
        //{
        //    get { return m_ElevationVelociy; }
        //    set { m_ElevationVelociy = value; }
        //}

        //public string m_HomePositionVelociy;
        //[CategoryAttribute("엘리베이션 홈 포지션"),
        //DisplayName("홈포지션 기본 속도 (mm/s)")]
        //public string HomePositionVelociy
        //{
        //    get { return m_HomePositionVelociy; }
        //    set { m_HomePositionVelociy = value; }
        //}

        //public string m_HomeRotationVelociy;
        //[CategoryAttribute("엘리베이션 홈 포지션"),
        //DisplayName("홈포지션 회전 속도 (rpm)")]
        //public string HomeRotationVelociy
        //{
        //    get { return m_HomeRotationVelociy; }
        //    set { m_HomeRotationVelociy = value; }
        //}

        //public string m_HomeOffset_Rotation;
        //[CategoryAttribute("엘리베이션 홈 포지션"),
        //DisplayName("Offset - Rotation 축 (Degree)")]
        //public string HomeOffset_Rotation
        //{
        //    get { return m_HomeOffset_Rotation; }
        //    set { m_HomeOffset_Rotation = value; }
        //}

        //public string m_HomeOffset_Front;
        //[CategoryAttribute("엘리베이션 홈 포지션"),
        //DisplayName("Offset - Front 축 (mm)")]
        //public string HomeOffset_Front
        //{
        //    get { return m_HomeOffset_Front; }
        //    set { m_HomeOffset_Front = value; }
        //}

        //public string m_HomeOffset_Left;
        //[CategoryAttribute("엘리베이션 홈 포지션"),
        //DisplayName("Offset - Left 축 (mm)")]
        //public string HomeOffset_Left
        //{
        //    get { return m_HomeOffset_Left; }
        //    set { m_HomeOffset_Left = value; }
        //}

        //public string m_HomeOffset_Right;
        //[CategoryAttribute("엘리베이션 홈 포지션"),
        //DisplayName("Offset - Right 축 (mm)")]
        //public string HomeOffset_Right
        //{
        //    get { return m_HomeOffset_Right; }
        //    set { m_HomeOffset_Right = value; }
        //}
    }
    #endregion

    #region 모바일 로봇 주행 모터 제어 명령 정의
    public class MobileDriverCommand
    {
        public uint OBJ_MAIN_MOT1_SET_CONTROLWORD = 0x100;
        public uint OBJ_MAIN_MOT2_SET_CONTROLWORD = 0x110;
        public uint OBJ_MAIN_MOT1_SET_MODE = 0x120;
        public uint OBJ_MAIN_MOT2_SET_MODE = 0x130;
        public uint OBJ_MAIN_MOT1_SET_TARGET_VEL = 0x140;
        public uint OBJ_MAIN_MOT2_SET_TARGET_VEL = 0x150;
        public uint OBJ_MAIN_MOT1_SET_TARGET_POS = 0x160;
        public uint OBJ_MAIN_MOT2_SET_TARGET_POS = 0x170;
        public uint OBJ_MAIN_MOT1_CLEAING_OFFSET = 0x190;
        public uint STATUS_DRIVING = 0x200;
        public uint STATUS_STEERING = 0x210;
        public uint STATUS_DRIVING_VELPOS = 0x220;
        public uint STATUS_STEERING_VELPOS = 0x230;
        public uint STATUS_DRIVING_CURRENT = 0x240;
        public uint STATUS_STEERING_CURRENT = 0x250;
        public uint STATUS_CLEAING_ENCODER = 0x260;

    }
    #endregion

    #region 모바일 로봇 주행 모터 정보 구조체 정의
    public class DriverInput
    {
        public float act_pos;
        public int act_vel;
        public int act_cur;          // unit : bit per 0.1%
        public float act_offset_pos;
        public UInt16 ready_fault;
        public UInt16 error_code;
        public uint mode_of_operation_display;
        public uint Fault;
        public uint Remote;
        public UInt16 Statusword;
        public uint mode_Init_state;
        public uint HeartBitState;
        public Int32 ClearingEncoder;
    }
    #endregion

    #region 모바일 로봇 주행 모터 모드 설정 정의
    public enum EPOS_MODE
    {
        Profile_Position_Mode = 1,
        Profile_Velocity_Mode = 3,
        Homming_Mode = 6,
        byteCyclic_Synchronous_Torque_Mode = 10
    }
    #endregion

    #region 모바일 로봇 주행 Controlword 명령 정의
    public enum MotorControlWord_def
    {
        SET_Shutdown = 0x0006,
        SET_Disable_Voltage = 0x0000,
        SET_Quick_Stop = 0x0002,
        SET_Enable = 0x000F,
        SET_Disable = 0x0007,
        SET_Fault_Reset = 0x0080,
        SET_Halt = 0x010F,
        SET_AbsolutePos = 0x001F,
        SET_AbsolutePos_Immediately = 0x003F,
        SET_RelativePos_Immediately = 0x007F,
        SET_RelativePos = 0x005F,
    } 
    #endregion

    #region 모바일 로봇 주행 피드백 정보 정의
    public enum MobileInfo_def
    {
        Driving_Speed = 0,
        Driving_Current,
        Steering_Angle,
        Driving_Status,
        Steering_Status,
        Driving_Error,
        Steering_Error,
    }
    #endregion

    #region 모바일 로봇 Status 정보 정의
    public enum EPOS_STATUS
    {
        GET_ReadyToSwitchOn = 0x0001,
        GET_Switched_On = 0x0002,
        GET_Operation_Enable = 0x0004,
        GET_Fault = 0x0008,
        GET_Voltage_Enable = 0x0010,
        GET_Quick_Stop = 0x0020,
        GET_Switch_On_Disable = 0x0040,
        GET_Waming = 0x0080,
        GET_Remote = 0x0200,
        GET_Operating_Mode_Spectific1 = 0x0400,
        GET_IntermalLimitActive = 0x0800,
        GET_Operating_Mode_Spectific2 = 0x1000,
        GET_Operating_Mode_Spectific3 = 0x2000,
        GET_Operating_Mode_Spectific4 = 0x8000
    }
    #endregion

    #region 모바일 로봇 모터 번호 정의
    public enum MobileDriver_def
    {
        Front_Left = 1,
        Fornt_Right,
        Rear_Left,
        Rear_Right
    }


    public enum MobileMotor_def
    {
        Front_Left_Driving = 0,
        Front_Left_Steering,
        Front_Right_Driving,
        Front_Right_Steering,
        Rear_Left_Driving,
        Rear_Left_Steering,
        Rear_Right_Driving,
        Rear_Right_Steering,
    }
    #endregion

    #region 모바일 로봇 주행 방법 정의
    public enum MobileMoveMode_def
    {
        AllMove = 0,
        AllStop,
        AllRotation,
        Front_Steering_Drving,
        Rear_Steering_Drving,
        Corresponding_Phase_Driving,
        Opposite_Phase_Driving
    }
    #endregion

    #region 엘리베이션 모터 번호 정의
    public enum Elevation_Motor_def
    {
        Tilt_IP = 0,
        Rotation_IP,
        Front_IP,
        LeftIP,
        Right_IP,
        Adhesie_IP
    }
    #endregion

    #region Ez-Sevo 모터 Feed-back 파라메터
    /// <summary>
    /// Ez-Servo 모터 상태 확인 구조체
    /// </summary>
    public struct EzServo_Alarm
    {
        public bool m_isbErrorAll;
        [CategoryAttribute("EzServoII Motor 상태"),
        DisplayName("여러 에러 중 하나 이상의 에러가 발생한 경우")]
        public bool isbErrorAll
        {
            get { return m_isbErrorAll; }
            set { m_isbErrorAll = value; }
        }

        public bool m_isbLimitOverPositive;
        [CategoryAttribute("EzServoII Motor 상태"),
        DisplayName("+방향 Limit 센서가 ON 이 된 경우")]
        public bool isbLimitOverPositive
        {
            get { return m_isbLimitOverPositive; }
            set { m_isbLimitOverPositive = value; }
        }

        public bool m_isbLimitOverNegative;
        [CategoryAttribute("EzServoII Motor 상태"),
        DisplayName("-방향 Limit 센서가 ON 이 된 경우")]
        public bool isbLimitOverNegative
        {
            get { return m_isbLimitOverNegative; }
            set { m_isbLimitOverNegative = value; }
        }

        public bool m_isbOverCurrent;
        [CategoryAttribute("EzServoII Motor 상태"),
        DisplayName("모터 구동 소자에 과전류 이상이 발생한 경우")]
        public bool isbOverCurrent
        {
            get { return m_isbOverCurrent; }
            set { m_isbOverCurrent = value; }
        }

        public bool m_isbOverHeat;
        [CategoryAttribute("EzServoII Motor 상태"),
        DisplayName("드라이브의 내부 온도가 85°C 를 초과하는 경우")]
        public bool isbOverHeat
        {
            get { return m_isbOverHeat; }
            set { m_isbOverHeat = value; }
        }

        public bool m_isbEmergencyStop;
        [CategoryAttribute("EzServoII Motor 상태"),
        DisplayName("모터가 비상 정지 상태일 경우")]
        public bool isbEmergencyStop
        {
            get { return m_isbEmergencyStop; }
            set { m_isbEmergencyStop = value; }
        }

        public bool m_isbOriginReturn;
        [CategoryAttribute("EzServoII Motor 상태"),
        DisplayName("원점 복귀 운전이 완료 된 상황일 경우.")]
        public bool isbOriginReturn
        {
            get { return m_isbOriginReturn; }
            set { m_isbOriginReturn = value; }
        }

        public bool m_isbServoOn;
        [CategoryAttribute("EzServoII Motor 상태"),
        DisplayName("모터가 Servo ON 상태일 경우.")]
        public bool isbServoOn
        {
            get { return m_isbServoOn; }
            set { m_isbServoOn = value; }
        }

        public bool m_isbAlarmReset;
        [CategoryAttribute("EzServoII Motor 상태"),
        DisplayName("Alarm Reset 명령이 실시된 상태일 경우.")]
        public bool isbAlarmReset
        {
            get { return m_isbAlarmReset; }
            set { m_isbAlarmReset = value; }
        }

        public bool m_isbPositionTableEnd;
        [CategoryAttribute("EzServoII Motor 상태"),
        DisplayName("포지션 테이블 운전이 종료된 상태일 경우.")]
        public bool isbPositionTableEnd
        {
            get { return m_isbPositionTableEnd; }
            set { m_isbPositionTableEnd = value; }
        }

        public bool m_isbMotionMoving;
        [CategoryAttribute("EzServoII Motor 상태"),
        DisplayName("모터가 현재 운전중일 경우.")]
        public bool isbMotionMoving
        {
            get { return m_isbMotionMoving; }
            set { m_isbMotionMoving = value; }
        }

        public bool m_isbMotionPause;
        [CategoryAttribute("EzServoII Motor 상태"),
        DisplayName("운전중 Pause 명령으로 정지 상태일 경우.")]
        public bool isbMotionPause
        {
            get { return m_isbMotionPause; }
            set { m_isbMotionPause = value; }
        }

        public bool m_isbOverLoad;
        [CategoryAttribute("EzServoII Motor 상태"),
        DisplayName("모터의 최대 토크를 초과하는 부하 경우.")]
        public bool isbOverLoad
        {
            get { return m_isbOverLoad; }
            set { m_isbOverLoad = value; }
        }

        public bool m_isbMotorStop;
        [CategoryAttribute("EzServoII Motor 상태"),
        DisplayName("모터가 일반 정지 상태일 경우")]
        public bool isMotorStop
        {
            get { return m_isbMotorStop; }
            set { m_isbMotorStop = value; }
        }
    }
    #endregion

    #region Fastech 모터 정보 클래스 구조
    public class EzMotorParam
    {
        public string ControlName { get; set; }
        public string DriverIP { get; set; }
        public int DevNum { get; set; }
        public bool isbConnect { get; set; }
        public bool Velocity { get; set; }

        public ToolHomeSeq HomePos;

        public EzServo_Alarm AlarmInfo { get; set; }
        public MT_MotionState MotionState { get; set; }
    }
    #endregion

    #region Fastech 피드팩 정보 구조
    public struct MT_MotionState
    {
        public int CommandPos;
        public int ActualPos;
        public int PosDifference;
        public int Velocity;
        public ushort RunningPT;
        public bool SetHomePosition;
    }
    #endregion

    #region 엘리베이션 사용자 홈포지션 구조체 
    public class ToolHomeSeq
    {
        public double nStep = -1;
        public bool isStart;
        public bool isOriginCheck;
        public bool isHomeComplete;
    } 
    #endregion

    #region 엘리베이션 모터 정의
    public class CTRL_NUM
    {
        //public const string Ele_Tilt = "Tilt_IP";
        //public const string Ele_Rotation = "Rotation_IP";
        //public const string Ele_Front = "Front_IP";
        //public const string Ele_Left = "LeftIP";
        //public const string Ele_Right = "Right_IP";
        //public const string Ele_Adhesive = "Adhesie_IP";
        public const string Mot_Wheel = "Wheel";

    }
    #endregion

    #region Fastech 모터 방향 정보 정의
    enum MOTDIR
    {
        MOTOR_CCW = 0,
        MOTOR_CW,
        MOTOR_STOP
    }
    #endregion

    #region DIO 모듈 정의
    public class CTRL_DIO
    {
        public const string DIO_01 = "DIO01_IP";
        public const string DIO_02 = "DIO02_IP";
        public const string DIO_03 = "DIO03_IP";
        public const string DIO_04 = "DIO04_IP";
    }
    #endregion

    #region 솔레노이드 제어 번호 정의

    public enum Solenoid_def
    {
        Valve_01 = 0x00000200,
        Valve_02 = 0x00000400,
        Valve_03 = 0x00000800,
        Valve_04 = 0x00001000,
        Valve_05 = 0x00002000,
        Valve_06 = 0x00004000
    }
    #endregion

    #region LED 순서 및 명칭 정의
    public enum LED_def
    {
        Front_Right = 0x00000100, //Front_Left
        Rear_Left = 0x00000200, //Rear_Right
        Rear_Right = 0x00000400, //Rear_Left
        Front_Left = 0x00000800, //Front_Right
        Top_Indicator = 0x00001000, //Top_Indicator
        Top_Siren = 0x00002000, //Top_Siren
        Left_HighBem = 0x00004000, //Left_HighBem
        Right_HighBem = 0x00008000,
        Front_Center = 0x00008001,
        Elevatoin_Left = 0x10000200,
        Elevatoin_Right = 0x20000400,
        Bottom_Left = 0x30000800,
        Bottom_Right = 0x40001000
    }
    #endregion

    #region Linear cylinder Up/Down 및 제어 모드 정의
    public enum Cylinder_Def
    {
        FrontLeftDown = 0x00000100,
        FrontLeftUp = 0x00000200,
        FrontRightDown = 0x00000400,
        FrontRightUp = 0x00000800,
        RearLeftDown = 0x00001000,
        RearLeftUp = 0x00002000,
        RearRightDown = 0x00004000,
        RearRightUp = 0x00008000
    }

    public enum CylinderCtrlMode_def
    {
        AllDown = 0,
        AllUp,
        AllStop,
        FrontUp,
        FrontDown,
        RearUp,
        RearDown,
        Left_DiagonalUp,
        Left_DiagonalDown,
        Right_DiagonalUp,
        Right_DiagonalDown
    }
    #endregion

    #region 배터리 상태 확인 변수 정의
    public class BatteryStatus
    {
        public double Voltage;
        [CategoryAttribute("배터리 상태"),
        DisplayName("전압(V)")]
        public double _Voltage
        {
            get { return Voltage; }
            set { Voltage = value; }
        }

        public double Current;
        [CategoryAttribute("배터리 상태"),
        DisplayName("전류(A)")]
        public double _Current
        {
            get { return Current; }
            set { Current = value; }
        }

        public double SOC;
        [CategoryAttribute("배터리 상태"),
        DisplayName("배터리 잔량(%)")]
        public double _SOC
        {
            get { return SOC; }
            set { SOC = value; }
        }

        public TimeSpan ChargingCompletionTime;
        [CategoryAttribute("배터리 상태"),
        DisplayName("잔량(일, 시:분:초)")]
        public TimeSpan _ChargingCompletionTime
        {
            get { return ChargingCompletionTime; }
            set { ChargingCompletionTime = value; }
        }

        public TimeSpan DischargeCompletionTime;
        [CategoryAttribute("배터리 상태"),
        DisplayName("소요시간(일, 시:분:초)")]
        public TimeSpan _DischargeCompletionTime
        {
            get { return DischargeCompletionTime; }
            set { DischargeCompletionTime = value; }
        }

        public double Temperature;
        [CategoryAttribute("배터리 상태"),
        DisplayName("배터리 온도(도)")]
        public double _Temperature
        {
            get { return Temperature; }
            set { Temperature = value; }
        }

        public double SOH;
        [CategoryAttribute("배터리 상태"),
        DisplayName("배터리 수명(%)")]
        public double _SOH
        {
            get { return SOH; }
            set { SOH = value; }
        }

        public double BatteryCapacity;
        [CategoryAttribute("배터리 상태"),
        DisplayName("배터리 용량(Ah)")]
        public double _BatteryCapacity
        {
            get { return BatteryCapacity; }
            set { BatteryCapacity = value; }
        }

    }
    #endregion

    public class IMU_Data
    {
        public float Pitch;
        public float Yaw;
        public float Roll;
        public float AccPitch;
        public float AccYaw;
        public float AccRoll;
        public float Battery;
    }

}
