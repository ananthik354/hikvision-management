using HikvisionBackend1.Models;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
namespace HikvisionBackend1.Services
{
    public class HikvisionSdkService
    {
        private readonly ConcurrentDictionary<int, int> _cameraSessions = new();
        // ============================================================
        // Windows DLL directory
        // ============================================================

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);


        // ============================================================
        // Hikvision SDK Initialization
        // ============================================================

        [DllImport(
            "HCNetSDK.dll",
            CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool NET_DVR_Init();


        // ============================================================
        // Hikvision SDK Last Error
        // ============================================================

        [DllImport(
            "HCNetSDK.dll",
            CallingConvention = CallingConvention.StdCall)]
        private static extern uint NET_DVR_GetLastError();


        // ============================================================
        // NET_DVR_USER_LOGIN_INFO
        //
        // IMPORTANT:
        // Field order must match Hikvision native structure.
        // ============================================================

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private struct NET_DVR_USER_LOGIN_INFO
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 129)]
            public string sDeviceAddress;

            public byte byUseTransport;

            public ushort wPort;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string sUserName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string sPassword;

            public IntPtr cbLoginResult;

            public IntPtr pUser;

            public int bUseAsynLogin;

            public byte byProxyType;

            public byte byUseUTCTime;

            public byte byLoginMode;

            public byte byHttps;

            public int iProxyID;

            public byte byVerifyMode;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 119)]
            public byte[] byRes3;
        }


        // ============================================================
        // NET_DVR_DEVICEINFO_V30
        // ============================================================
        [StructLayout(LayoutKind.Sequential)]
        private struct NET_DVR_DEVICEINFO_V30
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
            public byte[] sSerialNumber;

            public byte byAlarmInPortNum;
            public byte byAlarmOutPortNum;
            public byte byDiskNum;
            public byte byDVRType;
            public byte byChanNum;
            public byte byStartChan;
            public byte byAudioChanNum;
            public byte byIPChanNum;
            public byte byZeroChanNum;
            public byte byMainProto;
            public byte bySubProto;
            public byte bySupport;
            public byte bySupport1;
            public byte bySupport2;

            public ushort wDevType;

            public byte bySupport3;
            public byte byMultiStreamProto;
            public byte byStartDChan;
            public byte byStartDTalkChan;
            public byte byHighDChanNum;
            public byte bySupport4;
            public byte byLanguageType;
            public byte byVoiceInChanNum;
            public byte byStartVoiceInChanNo;
            public byte bySupport5;
            public byte bySupport6;
            public byte byMirrorChanNum;

            public ushort wStartMirrorChanNo;

            public byte bySupport7;
            public byte byRes2;
        }


        // ============================================================
        // NET_DVR_DEVICEINFO_V40
        //
        // This matches the current documented V40 definition.
        // ============================================================

        [StructLayout(LayoutKind.Sequential)]
        private struct NET_DVR_DEVICEINFO_V40
        {
            public NET_DVR_DEVICEINFO_V30 struDeviceV30;

            public byte bySupportLock;
            public byte byRetryLoginTime;
            public byte byPasswordLevel;
            public byte byProxyType;

            public uint dwSurplusLockTime;

            public byte byCharEncodeType;
            public byte bySupportDev5;
            public byte bySupport;
            public byte byLoginMode;

            public uint dwOEMCode;

            public int iResidualValidity;

            public byte byResidualValidity;
            public byte bySingleStartDTalkChan;
            public byte bySingleDTalkChanNums;
            public byte byPassWordResetLevel;
            public byte bySupportStreamEncrypt;
            public byte byMarketType;
            public byte byTLSCap;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 237)]
            public byte[] byRes2;
        }


        // ============================================================
        // NET_DVR_Login_V40
        // ============================================================

        [DllImport(
            "HCNetSDK.dll",
            CallingConvention = CallingConvention.StdCall,
            CharSet = CharSet.Ansi)]
        private static extern int NET_DVR_Login_V40(
            ref NET_DVR_USER_LOGIN_INFO pLoginInfo,
            ref NET_DVR_DEVICEINFO_V40 lpDeviceInfo);


        // ============================================================
        // NET_DVR_Logout
        // ============================================================

        [DllImport(
            "HCNetSDK.dll",
            CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool NET_DVR_Logout(int lUserID);


        // ============================================================
        // Initialize SDK
        // ============================================================
        private string GetHikvisionError(uint error)
        {
            return error switch
            {
                0 => "NET_DVR_NOERROR",
                1 => "NET_DVR_PASSWORD_ERROR",
                2 => "NET_DVR_NOENOUGHPRI",
                3 => "NET_DVR_NOINIT",
                4 => "NET_DVR_CHANNEL_ERROR",
                5 => "NET_DVR_OVER_MAXLINK",
                6 => "NET_DVR_VERSIONNOMATCH",
                7 => "NET_DVR_NETWORK_FAIL_CONNECT",
                8 => "NET_DVR_NETWORK_SEND_ERROR",
                9 => "NET_DVR_NETWORK_RECV_ERROR",
                10 => "NET_DVR_NETWORK_RECV_TIMEOUT",
                11 => "NET_DVR_NETWORK_ERRORDATA",
                12 => "NET_DVR_ORDER_ERROR",
                13 => "NET_DVR_OPERNOPERMIT",
                14 => "NET_DVR_COMMANDTIMEOUT",
                17 => "NET_DVR_PARAMETER_ERROR",
                23 => "NET_DVR_NOSUPPORT",
                47 => "NET_DVR_USERNOTEXIST",
                _ => "UNKNOWN_ERROR"
            };
        }
        public bool TestSdk()
        {
            try
            {
                string sdkPath = Path.Combine(
                    AppContext.BaseDirectory,
                    "HikvisionSDK");

                Console.WriteLine(
                    $"Hikvision SDK path: {sdkPath}");

                // Check SDK folder
                if (!Directory.Exists(sdkPath))
                {
                    Console.WriteLine(
                        $"ERROR: SDK folder does not exist: {sdkPath}");

                    return false;
                }

                // Set native DLL directory
                if (!SetDllDirectory(sdkPath))
                {
                    Console.WriteLine(
                        "ERROR: Could not set native DLL directory.");

                    return false;
                }

                // Initialize Hikvision SDK
                bool result = NET_DVR_Init();

                if (!result)
                {
                    uint error = NET_DVR_GetLastError();

                    Console.WriteLine(
                        $"Hikvision SDK initialization failed. Error: {error}");

                    return false;
                }

                Console.WriteLine(
                    "Hikvision SDK initialized successfully.");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Hikvision SDK error: {ex}");

                return false;
            }
        }


        // ============================================================
        // Login to Hikvision Camera
        // ============================================================

        public bool Login(
     Camera camera)
        {
            try
            {
                if (camera.Id <= 0)
                {
                    Console.WriteLine("Invalid Camera ID.");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(camera.IpAddress))
                {
                    Console.WriteLine(
                        $"Camera {camera.Id}: IP address is empty.");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(camera.HikvisionUsername))
                {
                    Console.WriteLine(
                        $"Camera {camera.Id}: username is empty.");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(camera.HikvisionPassword))
                {
                    Console.WriteLine(
                        $"Camera {camera.Id}: password is empty.");
                    return false;
                }

                // ----------------------------------------------------
                // Prevent duplicate login
                // ----------------------------------------------------

                if (_cameraSessions.ContainsKey(camera.Id))
                {
                    Console.WriteLine(
                        $"Camera {camera.Id} is already logged in.");

                    return true;
                }

                var loginInfo = new NET_DVR_USER_LOGIN_INFO
                {
                    sDeviceAddress = camera.IpAddress,

                    byUseTransport = 0,

                    wPort = (ushort)camera.SdkPort,

                    sUserName = camera.HikvisionUsername,

                    sPassword = camera.HikvisionPassword,

                    cbLoginResult = IntPtr.Zero,

                    pUser = IntPtr.Zero,

                    bUseAsynLogin = 0,

                    byProxyType = 0,

                    byUseUTCTime = 0,

                    // Private SDK login
                    byLoginMode = 0,

                    // TCP
                    byHttps = 0,

                    iProxyID = 0,

                    byVerifyMode = 0,

                    byRes3 = new byte[119]
                };

                var deviceInfo = new NET_DVR_DEVICEINFO_V40
                {
                    struDeviceV30 = new NET_DVR_DEVICEINFO_V30
                    {
                        sSerialNumber = new byte[48]
                    },

                    byRes2 = new byte[237]
                };

                Console.WriteLine(
                    $"[Camera {camera.Id}] " +
                    $"Connecting to {camera.IpAddress}:{camera.SdkPort}...");

                int userId = NET_DVR_Login_V40(
                    ref loginInfo,
                    ref deviceInfo);

                if (userId < 0)
                {
                    uint error = NET_DVR_GetLastError();

                    Console.WriteLine(
                        $"[Camera {camera.Id}] SDK login failed. " +
                        $"Error: {error} - {GetHikvisionError(error)}");

                    return false;
                }

                // ----------------------------------------------------
                // Store session against CameraId
                // ----------------------------------------------------

                _cameraSessions[camera.Id] = userId;

                Console.WriteLine(
                    $"[Camera {camera.Id}] SDK login successful. " +
                    $"User ID: {userId}");

                Console.WriteLine(
                    $"[Camera {camera.Id}] Channel Count: " +
                    $"{deviceInfo.struDeviceV30.byChanNum}");

                Console.WriteLine(
                    $"[Camera {camera.Id}] Alarm Output Count: " +
                    $"{deviceInfo.struDeviceV30.byAlarmOutPortNum}");

                Console.WriteLine(
                    $"[Camera {camera.Id}] Login Mode: " +
                    $"{deviceInfo.byLoginMode}");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[Camera {camera.Id}] SDK login error: {ex}");

                return false;
            }
        }


        // ============================================================
        // Logout
        // ============================================================

        public bool Logout(int cameraId)
        {
            if (!_cameraSessions.TryRemove(
                cameraId,
                out int userId))
            {
                Console.WriteLine(
                    $"[Camera {cameraId}] No SDK session found.");

                return true;
            }

            bool result = NET_DVR_Logout(userId);

            if (result)
            {
                Console.WriteLine(
                    $"[Camera {cameraId}] SDK logout successful.");

                return true;
            }

            uint error = NET_DVR_GetLastError();

            Console.WriteLine(
                $"[Camera {cameraId}] SDK logout failed. " +
                $"Error: {error} - {GetHikvisionError(error)}");

            return false;
        }
        public bool TryGetUserId(
    int cameraId,
    out int userId)
        {
            return _cameraSessions.TryGetValue(
                cameraId,
                out userId);
        }
    }
}