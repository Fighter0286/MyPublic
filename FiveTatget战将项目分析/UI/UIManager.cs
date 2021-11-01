/********************************************************************
	created:	2014/02/17
	created:	17:2:2014   9:57
	filename: 	UIManager.cs
	author:		张旭辉
	
	purpose:	UIRoot下的UI管理器，创建不同的根节点来控制不同功能的UI
 *              删除UI禁止使用Destroy 必须使用DestroyUI，否则可能引起内存无线增长
*********************************************************************/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GCGame.Table;
using Module.Log;
using System;
using Games.GlobeDefine;

public interface IUIWidget 
{
   void OnLoad(params object[] args);
}
public class UIManager : MonoBehaviour
{
    //private static int m_sCloseUICount = 0;

    public delegate void OnOpenUIDelegate(bool bSuccess, object param);
    public delegate void OnLoadUIItemDelegate(GameObject resItem, object param1);

    private Transform BaseUIRoot;      // 位于UI最底层，常驻场景，基础交互
    private Transform PopUIRoot;       // 位于UI上层，弹出式，互斥
    private Transform StoryUIRoot;     // 故事背景层
    private Transform TipUIRoot;       // 位于UI顶层，弹出重要提示信息等
    private Transform MenuPopUIRoot;
    private Transform MessageUIRoot;
    private Transform DeathUIRoot;

    private Dictionary<string, GameObject> m_dicTipUI = new Dictionary<string, GameObject>();      //弹窗
    private Dictionary<string, GameObject> m_dicBaseUI = new Dictionary<string, GameObject>();     //父类
    private Dictionary<string, GameObject> m_dicPopUI = new Dictionary<string, GameObject>();  //弹窗
    private Dictionary<string, GameObject> m_dicStoryUI = new Dictionary<string, GameObject>();  //故事面板
    private Dictionary<string, GameObject> m_dicMenuPopUI = new Dictionary<string, GameObject>();  //菜单弹出面板
    private Dictionary<string, GameObject> m_dicMessageUI = new Dictionary<string, GameObject>();  //菜单面板
    private Dictionary<string, GameObject> m_dicDeathUI = new Dictionary<string, GameObject>();  //死亡面板
    private Dictionary<string, GameObject> m_dicCacheUI = new Dictionary<string, GameObject>();  //临时缓存

    private Dictionary<string, int> m_dicWaitLoad = new Dictionary<string, int>();  //等待加载
    //private static string[] m_MenuBarPopUI = { "RoleView", "PartnerAndMountRoot", "MissionLogRoot", "RelationRoot", "BelleController", "BackPackRoot", "EquipStrengthenRoot", "SkillRoot" };

    private static UIManager m_instance;  //单例模式
    public static UIManager Instance()
    {
        return m_instance;
    }

    //private static bool m_GCTimerGo = false;
    //private static float m_GCWaitTime = GlobeVar.INVALID_ID;
    private const int GCCollectTime = 1;

    void Awake()
    { 
        //初始化清空字典
        m_dicTipUI.Clear();
        m_dicBaseUI.Clear();
        m_dicPopUI.Clear();
        m_dicStoryUI.Clear();
        m_dicMenuPopUI.Clear();
        m_dicMessageUI.Clear();
        m_dicDeathUI.Clear();
        m_dicCacheUI.Clear();
        m_instance = this;

        BaseUIRoot = gameObject.transform.FindChild("BaseUIRoot");
        if (null == BaseUIRoot)
        {
            BaseUIRoot = AddObjToRoot("BaseUIRoot").transform;
        }

        PopUIRoot = gameObject.transform.FindChild("PopUIRoot");
        if (null == PopUIRoot)
        {
            PopUIRoot = AddObjToRoot("PopUIRoot").transform;
        }

        StoryUIRoot = gameObject.transform.FindChild("StoryUIRoot");
        if (null == StoryUIRoot)
        {
            StoryUIRoot = AddObjToRoot("StoryUIRoot").transform;
        }

        TipUIRoot = gameObject.transform.FindChild("TipUIRoot");
        if (null == TipUIRoot)
        {
            TipUIRoot = AddObjToRoot("TipUIRoot").transform;
        }

        MenuPopUIRoot = gameObject.transform.FindChild("MenuPopUIRoot");
        if (null == MenuPopUIRoot)
        {
            MenuPopUIRoot = AddObjToRoot("MenuPopUIRoot").transform;
        }

        MessageUIRoot = gameObject.transform.FindChild("MessageUIRoot");
        if (null == MessageUIRoot)
        {
            MessageUIRoot = AddObjToRoot("MessageUIRoot").transform;
        }

        DeathUIRoot = gameObject.transform.FindChild("DeathUIRoot");
        if (null == DeathUIRoot)
        {
            DeathUIRoot = AddObjToRoot("DeathUIRoot").transform;
        }

        //设置所有父级显示出来
        BaseUIRoot.gameObject.SetActive(true);
        TipUIRoot.gameObject.SetActive(true);
        PopUIRoot.gameObject.SetActive(true);
        StoryUIRoot.gameObject.SetActive(true);
        MenuPopUIRoot.gameObject.SetActive(true);
        MessageUIRoot.gameObject.SetActive(true);
        DeathUIRoot.gameObject.SetActive(true);
    }

    //void FixedUpdate()
    //{
    //    if (m_GCTimerGo)
    //    {
    //        if (Time.fixedTime - m_GCWaitTime >= GCCollectTime)
    //        {
    //            Resources.UnloadUnusedAssets();
    //            GC.Collect();

    //            m_GCTimerGo = false;
    //            m_GCWaitTime = GlobeVar.INVALID_ID;
    //        }
    //    }
    //}

    void OnDestroy()
    {
        m_instance = null;
    }
    /// <summary>
    /// 路径 回调
    /// </summary>
    /// <param name="pathData"></param>
    /// <param name="delLoadItem"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    public static bool LoadItem(UIPathData pathData, OnLoadUIItemDelegate delLoadItem, object param =  null)
    {
        if (null == m_instance)
        {
            LogModule.ErrorLog("game manager is not init");
            return false;
        }

        m_instance.LoadUIItem(pathData, delLoadItem, param);
        return true;
    }

    /// <summary>
    /// 展示UI，根据类型不同，触发不同行为
    /// </summary>
    /// <param name="pathData"></param>
    /// <param name="delOpenUI"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    public static bool ShowUI(UIPathData pathData, OnOpenUIDelegate delOpenUI = null, object param = null)
    {
        if (null == m_instance)
        {
            LogModule.ErrorLog("game manager is not init");
            return false;
        }
        
        m_instance.AddLoadDicRefCount(pathData.name);
        

//#if !UNITY_EDITOR && !UNITY_STANDALONE_WIN
//		if(pathData.uiType == UIPathData.UIType.TYPE_POP || 
//        pathData.uiType == UIPathData.UIType.TYPE_STORY || 
//        pathData.uiType == UIPathData.UIType.TYPE_TIP ||
//		pathData.uiType == UIPathData.UIType.TYPE_MENUPOP)
//		{
//			if(JoyStickLogic.Instance() != null)
//			{
//				ProcessInput.Instance().ReleaseTouch();
//				JoyStickLogic.Instance().ReleaseJoyStick();
//			}
//		}
//#endif
        Dictionary<string, GameObject> curDic = null;
        switch (pathData.uiType)
        {
            case UIPathData.UIType.TYPE_BASE:
                curDic = m_instance.m_dicBaseUI;
                break;
            case UIPathData.UIType.TYPE_POP:
                curDic = m_instance.m_dicPopUI;
                break;
            case UIPathData.UIType.TYPE_STORY:
                curDic = m_instance.m_dicStoryUI;
                break;
            case UIPathData.UIType.TYPE_TIP:
                curDic = m_instance.m_dicTipUI;
                break;
            case UIPathData.UIType.TYPE_MENUPOP:
                curDic = m_instance.m_dicMenuPopUI;
                break;
            case UIPathData.UIType.TYPE_MESSAGE:
                curDic = m_instance.m_dicMessageUI;
                break;
            case UIPathData.UIType.TYPE_DEATH:
                curDic = m_instance.m_dicDeathUI;
               
                break;
            default:
                return false;
        }

        if (null == curDic)
        {
            return false;
        }

        if (m_instance.m_dicCacheUI.ContainsKey(pathData.name))
        {
            if (!curDic.ContainsKey(pathData.name))
            {
                curDic.Add(pathData.name, m_instance.m_dicCacheUI[pathData.name]);
            }

            m_instance.m_dicCacheUI.Remove(pathData.name);
        }

        if (curDic.ContainsKey(pathData.name))
        {
            curDic[pathData.name].SetActive(true);
            m_instance.DoAddUI(pathData, curDic[pathData.name], delOpenUI, param);
            return true;
        }

        m_instance.LoadUI(pathData, delOpenUI, param);
        
        return true;
    }

    /// <summary>
    /// 读表展示UI
    /// </summary>
    /// <param name="tableID"></param>
    /// <param name="delOpenUI"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    public static bool ShowUIByID(int tableID, OnOpenUIDelegate delOpenUI = null, object param = null)
    {
        //if (null == m_instance)
        //{
        //    LogModule.ErrorLog("game manager is not init");
        //    return false;
        //}

        //Tab_UIPath curTabPath = TableManager.GetUIPathByID(tableID, 0);
        //if (null == curTabPath)
        //{
        //    LogModule.ErrorLog("cur ui is not set in table" + tableID);
        //    return false;
        //}

        //if(!UIPathData.m_DicUIInfo.ContainsKey(curTabPath.Path))
        //{
        //    LogModule.ErrorLog("cur ui is not set in table" + curTabPath.Path);
        //    return false;
        //}

        //UIPathData curData = UIPathData.m_DicUIInfo[curTabPath.Path];
        //return UIManager.ShowUI(curData, delOpenUI, param);
       
    }

    /// <summary>
    /// 根据ID获取关闭面板
    /// </summary>
    /// <param name="tableID"></param>
    public static void CloseUIByID(int tableID)
    {
        //if (null == m_instance)
        //{
        //    LogModule.ErrorLog("game manager is not init");
        //    return;
        //}


        //Tab_UIPath curTabPath = TableManager.GetUIPathByID(tableID, 0);
        //if (null == curTabPath)
        //{
        //    LogModule.ErrorLog("cur ui is not set in table" + tableID);
        //    return;
        //}

        //if (!UIPathData.m_DicUIInfo.ContainsKey(curTabPath.Path))
        //{
        //    LogModule.ErrorLog("cur ui is not set in table " + curTabPath.Path);
        //    return;
        //}

        //UIPathData curPathData = UIPathData.m_DicUIInfo[curTabPath.Path];
        //CloseUI(curPathData);

    }

    /// <summary>
    /// 关闭UI，根据类型不同，触发不同行为
    /// </summary>
    /// <param name="pathData"></param>
    public static void CloseUI(UIPathData pathData)
    {
        if (null == m_instance)
        {
            return;
        }

        //int MaxCloseCount = PlayerPreferenceData.MaxCleanUICount;
        //if (MaxCloseCount > 6)
        //{
        //    MaxCloseCount = 6;
        //}

        //关闭MaxCloseCount次UI的时候，立即GC
        //if (++m_sCloseUICount >= MaxCloseCount)
        //{
        //    Resources.UnloadUnusedAssets();
        //    GC.Collect();
        //    m_sCloseUICount = 0;
        //   // LogModule.DebugLog("CloseUI GC 1");
        //}
        //else
        //{
        //    //活动，侠客，世界地图，PK，酒楼, 美人，背包界面，伙伴，每次打开都清理
        //    if (pathData.name == "ActivityController" ||
        //        pathData.name == "SwordsManController" ||
        //        pathData.name == "SceneMapRoot" ||
        //        pathData.name == "PKSetRoot" ||
        //        pathData.name == "Restaurant" ||
        //        pathData.name == "BelleController" ||
        //        pathData.name == "BackPackRoot" ||
        //        pathData.name == "PartnerAndMountRoot")
        //    {
        //        Resources.UnloadUnusedAssets();
        //        GC.Collect();
        //        m_sCloseUICount = 0;
        //        //LogModule.DebugLog("CloseUI GC 2 " + pathData.name);
        //    }
        //}
        //LogModule.DebugLog("m_sCloseUICount : " + m_sCloseUICount + " MaxCloseCount= " + MaxCloseCount);

        //if (!m_GCTimerGo)
        //{
        //    //关闭UI的时候，如果玩家不会进行其他操作，则顺手清理一下内存
        //    //如果关闭UI的时候玩家需要流畅的玩耍，则要排除掉
        //    //目前先增加特例，之后等特例多了之后进行统一处理
        //    if (pathData.name != "NewPlayerGuidRoot")
        //    {
        //        m_GCTimerGo = true;
        //        m_GCWaitTime = Time.fixedTime;
        //    }
        //}

//         if (pathData.name.Equals("BelleController"))
//         {
//             Resources.UnloadUnusedAssets();
//           //  GC.Collect();
//             //LogModule.DebugLog("BelleController GC " + pathData.name);
//         }
         Resources.UnloadUnusedAssets();
        m_instance.RemoveLoadDicRefCount(pathData.name);
        switch (pathData.uiType)
        {
            case UIPathData.UIType.TYPE_BASE:
                m_instance.CloseBaseUI(pathData.name);
                break;
            case UIPathData.UIType.TYPE_POP:
                m_instance.ClosePopUI(pathData.name);
                break;
            case UIPathData.UIType.TYPE_STORY:
                m_instance.CloseStoryUI(pathData.name);
                break;
            case UIPathData.UIType.TYPE_TIP:
                m_instance.CloseTipUI(pathData.name);
                break;
            case UIPathData.UIType.TYPE_MENUPOP:
                m_instance.CloseMenuPopUI(pathData.name);
                break;
            case UIPathData.UIType.TYPE_MESSAGE:
                m_instance.CloseMessageUI(pathData.name);
                break;
            case UIPathData.UIType.TYPE_DEATH:
                m_instance.CloseDeathUI(pathData.name);
                break;
            default:
                break;
        }

        if (pathData.uiGroupName != null && pathData.isMainAsset)
        {
            BundleManager.ReleaseLoginBundle();
        }
    }

    /// <summary>
    /// 执行uI逻辑
    /// </summary>
    /// <param name="uiData"></param>
    /// <param name="curWindow"></param>
    /// <param name="fun"></param>
    /// <param name="param"></param>
    void DoAddUI(UIPathData uiData, GameObject curWindow, object fun, object param)
    {
//        if (!m_dicWaitLoad.Remove(uiData.name))
//        {
//            return;
//        }      
        if (null != curWindow)
        {
            Transform parentRoot = null;
            Dictionary<string, GameObject> relativeDic = null;
            switch (uiData.uiType)
            {
                case UIPathData.UIType.TYPE_BASE:
                    parentRoot = BaseUIRoot;
                    relativeDic = m_dicBaseUI;
                    break;
                case UIPathData.UIType.TYPE_POP:
                    parentRoot = PopUIRoot;
                    relativeDic = m_dicPopUI;
                    break;
                case UIPathData.UIType.TYPE_STORY:
                    parentRoot = StoryUIRoot;
                    relativeDic = m_dicStoryUI;
                    break;
                case UIPathData.UIType.TYPE_TIP:
                    parentRoot = TipUIRoot;
                    relativeDic = m_dicTipUI;
                    break;
                case UIPathData.UIType.TYPE_MENUPOP:
                    parentRoot = MenuPopUIRoot;
                    relativeDic = m_dicMenuPopUI;
                    break;
                case UIPathData.UIType.TYPE_MESSAGE:
                    parentRoot = MessageUIRoot;
                    relativeDic = m_dicMessageUI;
                    break;
                case UIPathData.UIType.TYPE_DEATH:
                    parentRoot = DeathUIRoot;
                    relativeDic = m_dicDeathUI;
                    break;
                default:
                    break;

            }

            if (uiData.uiType == UIPathData.UIType.TYPE_POP || uiData.uiType == UIPathData.UIType.TYPE_MENUPOP)
            {
                OnLoadNewPopUI(m_dicPopUI, uiData.name);
                OnLoadNewPopUI(m_dicMenuPopUI, uiData.name);
            }
            if (null != relativeDic && relativeDic.ContainsKey(uiData.name))
            {
                relativeDic[uiData.name].SetActive(true);
            }
              
            else if (null != parentRoot && null != relativeDic)
            {
                GameObject newWindow = GameObject.Instantiate(curWindow) as GameObject;
               
                if (null != newWindow)
                {
                    Vector3 oldScale = newWindow.transform.localScale;
                    newWindow.transform.parent = parentRoot;
                    newWindow.transform.localPosition = Vector3.zero;
                    newWindow.transform.localScale = oldScale;
                    relativeDic.Add(uiData.name, newWindow);
                    if (uiData.uiType == UIPathData.UIType.TYPE_MENUPOP)
                    {
                        LoadMenuSubUIShield(newWindow);
                    }
                }
            }

            if (uiData.uiType == UIPathData.UIType.TYPE_STORY)
            {
                BaseUIRoot.gameObject.SetActive(false);
                TipUIRoot.gameObject.SetActive(false);
                PopUIRoot.gameObject.SetActive(false);
                MenuPopUIRoot.gameObject.SetActive(false);
                MessageUIRoot.gameObject.SetActive(false);
                StoryUIRoot.gameObject.SetActive(true);
            }
            else if (uiData.uiType == UIPathData.UIType.TYPE_MENUPOP)
            {
                if (null != PlayerFrameLogic.Instance() && null != MenuBarLogic.Instance())
                {
                    //MenuBarLogic.Instance().gameObject.SetActive(false);  //新手引导脚本
                    //PlayerFrameLogic.Instance().gameObject.SetActive(false);  //玩家头像脚本

                    StartCoroutine(GCAfterOneSceond());
                }
            }
            else if (uiData.uiType == UIPathData.UIType.TYPE_DEATH)
            {
                ReliveCloseOtherSubUI();
            }
            else if (uiData.uiType == UIPathData.UIType.TYPE_POP)
            {
                //LoadPopUIShield(newWindow);
                if (PlayerFrameLogic.Instance() != null)
                {
                    //PlayerFrameLogic.Instance().SwitchAllWhenPopUIShow(false);
                    //PlayerFrameLogic.Instance().gameObject.SetActive(false);
                }
                if (MenuBarLogic.Instance() != null)
                {
                    //MenuBarLogic.Instance().gameObject.SetActive(false);
                }
				if (ItemTooltipsLogic.Instance() != null)
				{
					//ItemTooltipsLogic.Instance().gameObject.SetActive(false);//物品tips界面脚本
                }
				if (EquipTooltipsLogic.Instance() != null)
				{
					//EquipTooltipsLogic.Instance().gameObject.SetActive(false);  //装备tips界面
                }

                if (! (uiData.name.Equals("ServerChooseController") ||
                    uiData.name.Equals("RoleCreate")))
                {
                   // StartCoroutine(GCAfterOneSceond());
                } 
            }
        }

        if (null != fun)
        {
            OnOpenUIDelegate delOpenUI = fun as OnOpenUIDelegate;
            delOpenUI(curWindow != null, param);
        }
    }

    /// <summary>
    /// 影藏所有的ui
    /// </summary>
    public void HideAllUILayer() 
    {
        BaseUIRoot.gameObject.SetActive(false);
        TipUIRoot.gameObject.SetActive(false);
        PopUIRoot.gameObject.SetActive(false);
        MenuPopUIRoot.gameObject.SetActive(false);
        MessageUIRoot.gameObject.SetActive(false);
        StoryUIRoot.gameObject.SetActive(false);
    }
     
    /// <summary>
    /// 展示所有的UI
    /// </summary>
    public void ShowAllUILayer() 
    {
        BaseUIRoot.gameObject.SetActive(true);
        TipUIRoot.gameObject.SetActive(true);
        PopUIRoot.gameObject.SetActive(true);
        MenuPopUIRoot.gameObject.SetActive(true);
        MessageUIRoot.gameObject.SetActive(true);
        StoryUIRoot.gameObject.SetActive(true);
    }
    IEnumerator GCAfterOneSceond()
    {
         yield return new WaitForSeconds(1);

        // Resources.UnloadUnusedAssets();
         //System.GC.Collect();
    }
    //路径 预制体 回调 参数
    void DoLoadUIItem(UIPathData uiData, GameObject curItem, object fun, object param)
    {
        if (null != fun)
        {
            OnLoadUIItemDelegate delLoadItem = fun as OnLoadUIItemDelegate;
            //得到回调 执行回调 传加载好的预制体和回调
            delLoadItem(curItem, param);
        }
    }
    void ClosePopUI(string name)
    {
        OnClosePopUI(m_dicPopUI, name);
    }

    /// <summary>
    /// 关闭故事面板
    /// </summary>
    /// <param name="name"></param>
    void CloseStoryUI(string name)
    {
        if (TryDestroyUI(m_dicStoryUI, name))
        {
            BaseUIRoot.gameObject.SetActive(true);
            TipUIRoot.gameObject.SetActive(true);
            PopUIRoot.gameObject.SetActive(true);
            MenuPopUIRoot.gameObject.SetActive(true);
            MessageUIRoot.gameObject.SetActive(true);
            StoryUIRoot.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 关闭父类面板
    /// </summary>
    /// <param name="name"></param>
    void CloseBaseUI(string name)
    {
        if (m_dicBaseUI.ContainsKey(name))
        {
            m_dicBaseUI[name].SetActive(false);
        }
    }

    /// <summary>
    /// 查询是否包含弹窗
    /// </summary>
    /// <param name="name"></param>
    void CloseTipUI(string name)
    {
        TryDestroyUI(m_dicTipUI, name);
    }

    /// <summary>
    /// 查询是否包含主菜单
    /// </summary>
    /// <param name="name"></param>
    void CloseMenuPopUI(string name)
    {
        OnClosePopUI(m_dicMenuPopUI, name);
    }

    /// <summary>
    /// 查询是否包含主菜单
    /// </summary>
    /// <param name="name"></param>
    void CloseMessageUI(string name)
    {
        TryDestroyUI(m_dicMessageUI, name);
    }

    void CloseDeathUI(string name)
    {
        if (TryDestroyUI(m_dicDeathUI, name))
        {
            // 关闭复活界面时 恢复节点的显示
            m_instance.PopUIRoot.gameObject.SetActive(true);
            m_instance.MenuPopUIRoot.gameObject.SetActive(true);
            m_instance.TipUIRoot.gameObject.SetActive(true);
            m_instance.BaseUIRoot.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 加载UI预制体 执行委托
    /// </summary>
    /// <param name="uiData"></param>
    /// <param name="delOpenUI"></param>
    /// <param name="param1"></param>
    void LoadUI(UIPathData uiData, OnOpenUIDelegate delOpenUI = null, object param1 = null)
    {
        GameObject curWindow = ResourceManager.LoadResource("Prefab/UI/" + uiData.name) as GameObject;
        if (null != curWindow)
        {
            DoAddUI(uiData, curWindow, delOpenUI, param1);
            //LogModule.ErrorLog("can not open controller path not found:" + path);
            return;
        }

        if (uiData.uiGroupName != null)
        {
            GameObject objCacheBundle = BundleManager.GetGroupUIItem(uiData);
            if (null != objCacheBundle)
            {
                DoAddUI(uiData, objCacheBundle, delOpenUI, param1);
                return;
            }
        }
        StartCoroutine(BundleManager.LoadUI(uiData, DoAddUI, delOpenUI, param1));
    }

    /// <summary>
    ///  //路径和回调  加载预制体
    /// </summary>
    /// <param name="uiData"></param>
    /// <param name="delLoadItem"></param>
    /// <param name="param"></param>
    void LoadUIItem(UIPathData uiData, OnLoadUIItemDelegate delLoadItem, object param = null)
    {
        GameObject curWindow = ResourceManager.LoadResource("Prefab/UI/" + uiData.name) as GameObject;
        if(null != curWindow)
        {
            //执行方法 路径  加载完成的预制体 回调 不定参数
            DoLoadUIItem(uiData, curWindow, delLoadItem, param);
            return;
        }

        if (uiData.uiGroupName != null)
        {
            GameObject objCacheBundle = BundleManager.GetGroupUIItem(uiData);
            if (null != objCacheBundle)
            {
                DoLoadUIItem(uiData, objCacheBundle, delLoadItem, param);
                return;
            }
        }

        StartCoroutine(BundleManager.LoadUI(uiData, DoLoadUIItem, delLoadItem, param));
    }

    /// <summary>
    /// 加载菜单预制体，初始化
    /// </summary>
    /// <param name="newWindow"></param>
    static void LoadMenuSubUIShield(GameObject newWindow)
    {
        GameObject MenuSubUIShield = ResourceManager.InstantiateResource("Prefab/UI/MenuSubUIShield") as GameObject;
        if (MenuSubUIShield == null)
        {
            LogModule.ErrorLog("can not open MenuSubUIShield path not found");
            return;
        }
        MenuSubUIShield.transform.parent = newWindow.transform;
        MenuSubUIShield.transform.localPosition = Vector3.zero;
        MenuSubUIShield.transform.localScale = Vector3.one;
    }

    /// <summary>
    /// 加载弹窗预制体，初始化
    /// </summary>
    /// <param name="newWindow"></param>
    static void LoadPopUIShield(GameObject newWindow)
    {
        if (GameManager.gameManager.RunningScene == (int)GameDefine_Globe.SCENE_DEFINE.SCENE_LOGIN ||
            GameManager.gameManager.RunningScene == (int)GameDefine_Globe.SCENE_DEFINE.SCENE_LOADINGSCENE)
        {
            return;
        }

        GameObject PopUIBlack = ResourceManager.InstantiateResource("Prefab/UI/PopUIBlack") as GameObject;
        if (PopUIBlack == null)
        {
            LogModule.ErrorLog("can not open PopUIBlack path not found");
            return;
        }
        PopUIBlack.transform.parent = newWindow.transform;
        PopUIBlack.transform.localPosition = Vector3.zero;
        PopUIBlack.transform.localScale = Vector3.one;
    }

    /// <summary>
    /// 查找场景中的物体
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    GameObject AddObjToRoot(string name)
    {
        GameObject obj = new GameObject();
        obj.transform.parent = transform;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.one;
        obj.name = name;
        return obj;
    }

    /// <summary>
    /// 判断问题
    /// </summary>
    /// <returns></returns>
    bool SubUIShow()
    {
		/**
		 * 防止下面问题出现直接返回false
		 * 任务寻路到NPC的过程中，点击小聊天框内装备链接，弹出装备Tips，任务寻路结束后，弹出NPC对话面板，结束对话后，方向操控按钮失效
		 */
        //if (m_dicPopUI.Count + m_dicStoryUI.Count + m_dicTipUI.Count + m_dicMenuPopUI.Count > 0)
        //{
        //    return true;
        //}
        //else
        //{
            return false;
        //}
    }

    /// <summary>
    /// 是否出现问题
    /// </summary>
    /// <returns></returns>
    public static bool IsSubUIShow()
    {
        if (m_instance != null)
        {
            return m_instance.SubUIShow();
        }
        return false;
    }

    /// <summary>
    /// 关闭所有UI父级
    /// </summary>
    static void ReliveCloseOtherSubUI()
    {
        // 关闭所有PopUI
        List<string> uiKeyList = new List<string>();
        foreach (KeyValuePair<string, GameObject> pair in m_instance.m_dicPopUI)
        {
            uiKeyList.Add(pair.Key);
        }
        for (int i = 0; i < uiKeyList.Count; i++ )
        {
            m_instance.ClosePopUI([i])uiKeyList;
        }
        uiKeyList.Clear();
        // 关闭所有MenuPopUI
        foreach (KeyValuePair<string, GameObject> pair in m_instance.m_dicMenuPopUI)
        {
            uiKeyList.Add(pair.Key);
        }
        for (int i = 0; i < uiKeyList.Count; i++)
        {
            m_instance.CloseMenuPopUI(uiKeyList[i]);
        }
        uiKeyList.Clear();
        // 关闭所有TipUI
        foreach (KeyValuePair<string, GameObject> pair in m_instance.m_dicTipUI)
        {
            uiKeyList.Add(pair.Key);
        }
        for (int i = 0; i < uiKeyList.Count; i++)
        {
            m_instance.CloseTipUI(uiKeyList[i]);
        }
        uiKeyList.Clear();
        // 关闭所有除CentreNotice以外的MessageUI MessageUIRoot节点保留不隐藏
        foreach (KeyValuePair<string, GameObject> pair in m_instance.m_dicMessageUI)
        {
            if (!pair.Key.Contains("CentreNotice"))
            {
                uiKeyList.Add(pair.Key);
            }
        }
        for (int i = 0; i < uiKeyList.Count; i++)
        {
            m_instance.CloseMessageUI(uiKeyList[i]);
        }
        uiKeyList.Clear();

        //// 中断剧情对话
        //if (StoryDialogLogic.Instance() != null)
        //{
        //    CloseUI(UIInfo.StoryDialogRoot);
        //}
        //// 中断诗词对话
        //if (ShiCiLogic.Instance() != null)
        //{
        //    CloseUI(UIInfo.ShiCiRoot);
        //}
        //// 中断剑谱对话
        //if (JianPuLogic.Instance() != null)
        //{
        //    CloseUI(UIInfo.JianPuRoot);
        //}

        // 隐藏二级UI节点
        m_instance.PopUIRoot.gameObject.SetActive(false);
        m_instance.MenuPopUIRoot.gameObject.SetActive(false);
        m_instance.TipUIRoot.gameObject.SetActive(false);
        m_instance.BaseUIRoot.gameObject.SetActive(false);
    }
    /// <summary>
    /// 关闭所有
    /// </summary>
    static public void NewPlayerGuideCloseSubUI()
    {
        // 关闭所有PopUI
        foreach (KeyValuePair<string, GameObject> pair in m_instance.m_dicPopUI)
        {
            m_instance.ClosePopUI(pair.Key);
            break;
        }
        // 关闭所有MenuPopUI
        foreach (KeyValuePair<string, GameObject> pair in m_instance.m_dicMenuPopUI)
        {
            m_instance.CloseMenuPopUI(pair.Key);
            break;
        }
        // 关闭所有TipUI
        foreach (KeyValuePair<string, GameObject> pair in m_instance.m_dicTipUI)
        {
            m_instance.CloseTipUI(pair.Key);
            break;
        }
        // 关闭所有MessageUI
        //         foreach (KeyValuePair<string, GameObject> pair in m_instance.m_dicMessageUI)
        //         {
        //             m_instance.CloseMessageUI(pair.Key);
        //             break;
        //         }
    }

    /// <summary>
    /// 加入等待缓存区域
    /// </summary>
    /// <param name="pathName"></param>
    void AddLoadDicRefCount(string pathName)
    {
        if (m_dicWaitLoad.ContainsKey(pathName))
        {
            m_dicWaitLoad[pathName]++;
        }
        else
        {
            m_dicWaitLoad.Add(pathName, 1);
        }
    }

    bool RemoveLoadDicRefCount(string pathName)
    {
        if (!m_dicWaitLoad.ContainsKey(pathName))
        {
            return false;
        }

        m_dicWaitLoad[pathName]--;
        if (m_dicWaitLoad[pathName] <= 0)
        {
            m_dicWaitLoad.Remove(pathName);
        }

        return true;
    }
    /// <summary>
    /// 删除UI
    /// </summary>
    void DestroyUI(string name, GameObject obj)
    {
        Destroy(obj);
        BundleManager.OnUIDestroy(name);  //销毁后调用
    }
    
    private void OnLoadNewPopUI(Dictionary<string, GameObject> curList, string curName)
    {
        if (curList == null)
        {
            return;
        }

        List<string> objToRemove = new List<string>();

        if (curList.Count > 0)
        {
            objToRemove.Clear();
            foreach (KeyValuePair<string, GameObject> objs in curList)
            {
                if (curName == objs.Key)
                {
                    continue;
                }
                objs.Value.SetActive(false);
                objToRemove.Add(objs.Key);
                if (UIPathData.m_DicUIName.ContainsKey(objs.Key) && UIPathData.m_DicUIName[objs.Key].isDestroyOnUnload)
                {
                    DestroyUI(objs.Key, objs.Value);
                }
                else
                {
                    m_dicCacheUI.Add(objs.Key, objs.Value);
                }
            }

            for (int i = 0; i < objToRemove.Count; i++)
            {
                if (curList.ContainsKey(objToRemove[i]))
                {
                    curList.Remove(objToRemove[i]);
                }
            }
        }
    }
    private void OnClosePopUI(Dictionary<string, GameObject> curList, string curName)
    {
        if (TryDestroyUI(curList, curName))
        {
            // 关闭导航栏打开的二级UI时 收回导航栏
            if (null != PlayerFrameLogic.Instance())
            {
                PlayerFrameLogic.Instance().gameObject.SetActive(true);
                if (PlayerFrameLogic.Instance().Fold)
                {
                    PlayerFrameLogic.Instance().SwitchAllWhenPopUIShow(true);
                }
            }
            if (MenuBarLogic.Instance() != null)
            {
                if (MenuBarLogic.Instance().Fold)
                {
                    MenuBarLogic.Instance().gameObject.SetActive(true);
                }
            }
        }
    }

/// <summary>
/// 查询是否包含弹窗
/// </summary>
private bool TryDestroyUI(Dictionary<string, GameObject> curList, string curName)
    {
        if (curList == null)
        {
            return false;
        }

        if (!curList.ContainsKey(curName))
        {
            return false;
        }

//#if UNITY_ANDROID

        // < 768M UI不进行缓存
        if (SystemInfo.systemMemorySize < 768)
        {
            DestroyUI(curName, curList[curName]);
            curList.Remove(curName);

            Resources.UnloadUnusedAssets();
            GC.Collect();
            return true;
        }

//#endif

        if (UIPathData.m_DicUIName.ContainsKey(curName) && !UIPathData.m_DicUIName[curName].isDestroyOnUnload)
        {
            curList[curName].SetActive(false);
            m_dicCacheUI.Add(curName, curList[curName]);
        }
        else
        {
            DestroyUI(curName, curList[curName]);
        }

        curList.Remove(curName);

        return true;
    }


#if UNITY_ANDROID 
    void Update() 
    {
        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            PlatformHelper.ClickEsc();
        }
    }

    
#endif
}
