/********************************************************************************
 *	文件名：NetManager.cs
 *	创建人：	张旭辉
 *	创建时间：2019-9-13
 *
 *	功能说明： 网络管理器
*********************************************************************************/

using System;
using UnityEngine;
using Module.Log;
using System.IO;
using SPacket.SocketInstance;
using System.Collections.Generic;
using GCGame.Table;

public class NetManager : MonoBehaviour
{
   
    public static bool IsReconnecting = false;

    static public NetManager m_netManager;
   
    //单例模式
    private static NetManager m_instance = null;
    public static NetManager Instance()
    {
        return m_instance;
    }

    private bool m_bAskConnecting = false;      // 是否正处于询问断线状态
    void Awake()
    {
        if (m_netManager != null)
        {
            Destroy(this.gameObject);
        }

        m_netManager = this;
        DontDestroyOnLoad(this.gameObject);  //不销毁

        Application.RegisterLogCallback(LogModule.Log);
        NetWorkLogic.SetConnectLostDelegate(ConnectLost);  //设置回调
        m_instance = this;
    }

    void OnEnable()
    {
        m_bAskConnecting = false;
    }

    /// <summary>
    /// //处理消息的接收和发送
    /// </summary>
    void Update()
    {
        NetWorkLogic.GetMe().Update();
    }
    
    /// <summary>
    /// 向服务器发送ip和端口
    /// </summary>
    /// <param name="_ip">ip</param>
    /// <param name="_port">端口</param>
    /// <param name="delConnect">回调</param>
    public void ConnectToServer(string _ip, int _port, NetWorkLogic.ConnectDelegate delConnect)
    {
        //LogModule.DebugLog("Connect to Server... ip:" + _ip + " port :" + _port.ToString());  debug输出
        NetWorkLogic.SetConcnectDelegate(delConnect);
        NetWorkLogic.GetMe().ConnectToServer(_ip, _port, 100);
    }

    /// <summary>
    /// 用户登录信息
    /// </summary>
    /// <param name="retFun">方法</param>
    /// <param name="bForce">判断</param>
    /// <param name="bReconnect"></param>
    public static void SendUserLogin(LoginData.LoginRet retFun, bool bForce, bool bReconnect = false)
    {
        //if (!LoginData.accountData.m_bInit)
        //{
        //    LogModule.ErrorLog("account data is not init");
        //    return;
        //}

        ////LogModule.DebugLog("begin login");
        ////帐户和密码
        //LoginData.retLogin = retFun;
        //CG_LOGIN accountInfo = (CG_LOGIN)PacketDistributed.CreatePacket(MessageID.PACKET_CG_LOGIN);

        //if (LoginData.accountData.m_connectType == LoginData.AccountData.ConnnectType.CYOU)
        //{
        //    accountInfo.SetVtype((int)CG_LOGIN.VALIDATETYPE.CYOU);
        //    LogModule.DebugLog("begin cy login");
        //}
        //else
        //{
        //    accountInfo.SetVtype((int)CG_LOGIN.VALIDATETYPE.TEST);
        //    LogModule.DebugLog("begin test login");
        //}
        //accountInfo.SetGameversion(1/*(int)PlatformHelper.GetGameVersion()*/);
        //accountInfo.SetProgramversion(0/*(int)PlatformHelper.GetProgramVersion()*/);
        //accountInfo.SetPublicresourceversion(TableManager.GetPublicConfigByID(GameDefines.PublicResVersionKey, 0).IntValue);
        //accountInfo.SetMaxpacketid((int)MessageID.PACKET_SIZE);
        //accountInfo.SetForceenter(bForce ? 1 : 0);
        //accountInfo.SetDeviceid(SystemInfo.deviceUniqueIdentifier/*PlatformHelper.GetDeviceUDID()*/);
        //accountInfo.SetDevicetype(SystemInfo.deviceType.ToString()+"_"+SystemInfo.deviceName/*PlatformHelper.GetDeviceType()*/);
        //accountInfo.SetDeviceversion(SystemInfo.graphicsDeviceVersion/*PlatformHelper.GetDeviceVersion()*/);

        //accountInfo.SetAccount(LoginData.accountData.m_account);
        //accountInfo.SetValidateinfo(LoginData.accountData.m_validateInfo);
        //accountInfo.SetChannelid(PlatformHelper.GetChannelID());
        //accountInfo.SetMediachannel("0"/*PlatformHelper.GetMediaChannel()*/);

        //accountInfo.SetRapidvalidatecode(bReconnect ?  LoginData.accountData.m_gameServerValidateInfo : 0);
        //accountInfo.SetReservedint1(0);
        //accountInfo.SetReservedint2(0);
        //accountInfo.SetReservedint3(0);
        //accountInfo.SetReservedint4(0);
        //accountInfo.SetReservedstring1(LoginData.accountData.productCode);
        //accountInfo.SetReservedstring2(LoginData.accountData.channelCode);
        //accountInfo.SetReservedstring3(LoginData.accountData.m_userID);
        //accountInfo.SetReservedstring4("");
        //accountInfo.SendPacket();
    }

    /// <summary>
    /// 发送pb对象
    /// </summary>
    /// <param name="roleGUID"></param>
    /// <param name="funRet"></param>
    public static void SendChooseRole(ulong roleGUID, GC_SELECTROLE_RETHandler.SelectRoleFailRet funRet)
    {
        /*
        CG_SELECTROLE selectRolePacket = (CG_SELECTROLE)PacketDistributed.CreatePacket(MessageID.PACKET_CG_SELECTROLE);
        selectRolePacket.SetRoleGUID(roleGUID);
        selectRolePacket.SendPacket();
         * */
     
        //GC_SELECTROLE_RETHandler.retSelectRoleFail = funRet;
        //CG_SELECTROLE createRolePacket = (CG_SELECTROLE)PacketDistributed.CreatePacket(MessageID.PACKET_CG_SELECTROLE);
        //createRolePacket.SetRoleGUID(roleGUID);
        //createRolePacket.SendPacket();
    }

    /// <summary>
    /// 发送pb对象
    /// </summary>
    public static void SendUserLogout()
    {
        //CG_ASK_QUIT_GAME packet = (CG_ASK_QUIT_GAME)PacketDistributed.CreatePacket(MessageID.PACKET_CG_ASK_QUIT_GAME);
        //packet.Type = (int)CG_ASK_QUIT_GAME.GameSelecTType.GAMESELECTTYPE_ACCOUNT;
        //packet.SendPacket();
    }

    /// <summary>
    /// 判断断线重连
    /// </summary>
    public void ConnectLost()
    {
        if (!GameManager.gameManager.OnLineState)
        {
            return;
        }
        if (LoginUILogic.Instance() != null)
        {
            LoginUILogic.Instance().EnterServerChoose();
            // 连接丢失，请重新登录
            MessageBoxLogic.OpenOKBox(1292,1000);
            return;
        }
        else if (MainUILogic.Instance() != null)
        {
            if(!m_bAskConnecting || null != MessageBoxLogic.Instance())
            {
                LogModule.DebugLog("reconnecting....");
                // 连接丢失，正在重新连接。。。
                MessageBoxLogic.OpenOKBox(1293, 1000, OnReconnect);
                m_bAskConnecting = true;
                if (BackCamerControll.Instance() != null && BackCamerControll.Instance().gameObject.activeInHierarchy)
                {
                    BackCamerControll.Instance().gameObject.SetActive(false);
                }
            }
        }
        else
        {
            // 有可能在loading不处理，等UI起来后检测
        }
    }

    public static void OnWaitPacketTimeOut()
    {
        //MessageBoxLogic.OpenOKBox(1292, 1000, OnReturnServerChoose);
    }

    private static void OnReturnServerChoose()
    {
        //NetWorkLogic.GetMe().DisconnectServer();
        //LoginUILogic.Instance().EnterServerChoose();
    }

    public void OnReconnect()
    {
        //m_bAskConnecting = false;
        //NetWorkLogic.SetConcnectDelegate(Ret_Reconnect);
        //NetWorkLogic.GetMe().ReConnectToServer();
    }
    /// <summary>
    /// 重新登录
    /// </summary>
    /// <param name="bSuccess"></param>
    /// <param name="strResult"></param>
    void Ret_Reconnect(bool bSuccess, string strResult)
    {
        //if (bSuccess)
        //{
        //    // 重新登录
        //    LogModule.DebugLog("relogining....");
        //    NetManager.SendUserLogin(Ret_Login, true, true);
        //    //NetManager.SendUserLogin(PlayerPreferenceData.LastAccount, PlayerPreferenceData.LastPsw, Ret_Login);
        //}
        //else
        //{
        //    // 重连失败，点击确定重新登录
        //    MessageBoxLogic.OpenOKBox(1295,1000, EnterLoginScene);
            
        //}
    }

    /// <summary>
    /// // 重新连接失败，点击确定返回登录界面
    /// </summary>
    /// <param name="result"></param>
    /// <param name="validateResult"></param>
    void Ret_Login(GC_LOGIN_RET.LOGINRESULT result, int validateResult)
    {
        //if (result == GC_LOGIN_RET.LOGINRESULT.SUCCESS)
        //{
        //    LogModule.DebugLog("choose role....");
        //    MessageBoxLogic.CloseBox();
        //    MessageBoxLogic.OpenWaitBox(1366, 1, 0, OnChooseRole);
        //}
        //else
        //{
        //    // 重新连接失败，点击确定返回登录界面
        //    MessageBoxLogic.OpenOKBox(1295,1000, EnterLoginScene);
        //}
    }

    /// <summary>
    ///  // 正在等待进入场景
    /// </summary>
    void OnChooseRole()
    {
        //// 正在等待进入场景
        //MessageBoxLogic.OpenWaitBox(2352, GameDefines.CONNECT_TIMEOUT, 0, OnWaitPacketTimeOut);
        //NetManager.SendChooseRole(PlayerPreferenceData.LastRoleGUID, RetSelectRoleFail);
    }

    /// <summary>
    /// 加载场景
    /// </summary>
    void EnterLoginScene()
    {
        LoadingWindow.LoadScene(Games.GlobeDefine.GameDefine_Globe.SCENE_DEFINE.SCENE_LOGIN);
    }

    void EnterOffline()
    {
        GameManager.gameManager.OnLineState = false;
    }

    void RetSelectRoleFail(GC_SELECTROLE_RET.SELECTROLE_RESULT result)
    {
        // 重新连接获取角色失败，点击确定重新登录
        MessageBoxLogic.OpenOKBox(1294, 1000, OnSelectRoleFail);
    }

    void OnSelectRoleFail()
    {
        if (null != LoginUILogic.Instance())
        {
            LoginUILogic.Instance().EnterServerChoose();
        }
        else
        {
            LoadingWindow.LoadScene(Games.GlobeDefine.GameDefine_Globe.SCENE_DEFINE.SCENE_LOGIN);
        }
    }
}
