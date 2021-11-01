/********************************************************************
	文件名: 	\Main\Project\Client\Assets\MLDJ\Script\Obj\Obj_Character_Combat.cs
	创建时间:	2014/05/27 13:16
	全路径:	\Main\Project\Client\Assets\MLDJ\Script\Obj
	创建人:		luoy
	功能说明:	主角战斗相关
	修改记录:
*********************************************************************/

using System.Collections.Generic;
using Games.ImpactModle;
using Games.SkillModle;
using GCGame.Table;
using Module.Log;
using UnityEngine;
using System.Collections;
using Games.LogicObj;
using System;
using Games.GlobeDefine;
using GCGame;

namespace Games.LogicObj
{
	public partial class Obj_MainPlayer : Obj_OtherPlayer
	{
		//PK 模式 重载 Obj_OtherPlayer 的属性
		public override int PkModle
		{
			get { return GameManager.gameManager.PlayerDataPool.PkModle; }
			set { GameManager.gameManager.PlayerDataPool.PkModle = value; }
		}
		//隐身级别  重载 Obj 的属性
		public override int StealthLev
		{
			get { return GameManager.gameManager.PlayerDataPool.StealthLev; }
			set { GameManager.gameManager.PlayerDataPool.StealthLev = value; }
		}
		//是否可以进行合法反击
		public bool IsCanPKLegal 
		{
			get { return GameManager.gameManager.PlayerDataPool.IsCanPKLegal; }
			set { GameManager.gameManager.PlayerDataPool.IsCanPKLegal = value; }
		}
		//主角身上的技能信息
		public OwnSkillData[] OwnSkillInfo
		{
			get { return GameManager.gameManager.PlayerDataPool.OwnSkillInfo; }
			set { GameManager.gameManager.PlayerDataPool.OwnSkillInfo = value; }
		}
		//主角身上的自动挂机技能信息
		public OwnSkillData[] OwnAutoSkillInfo
		{
			get { return GameManager.gameManager.PlayerDataPool.OwnAutoSkillInfo; }
			set { GameManager.gameManager.PlayerDataPool.OwnAutoSkillInfo = value; }
		}
		
		//技能公共CD时间
		public int SkillPublicTime
		{
			get { return GameManager.gameManager.PlayerDataPool.SkillPublicTime; }
			set { GameManager.gameManager.PlayerDataPool.SkillPublicTime = value; }
		}
		//客户端 要用到BUFF信息
		//private List<ClientImpactInfo> m_ClientImpactInfo = new List<ClientImpactInfo>();
		public  List<ClientImpactInfo> ClientImpactInfo
		{
			get { return GameManager.gameManager.PlayerDataPool.ClientImpactInfo; }
			set { GameManager.gameManager.PlayerDataPool.ClientImpactInfo = value; }
		}
		//主角是否处于战斗状态
		private bool m_bInCombat = false;
		public bool InCombat
		{
			get { return m_bInCombat; }
			set { m_bInCombat = value; }
		}
		
		private int m_nCurPressSkillId = -1;//当前按下的技能ID
		public int CurPressSkillId
		{
			get { return m_nCurPressSkillId; }
			set { m_nCurPressSkillId = value; }
		}
		
		private int m_nHitPoint =0;
		public int HitPoint
		{
			get { return m_nHitPoint; }
			set { m_nHitPoint = value; }
		}
		
		private float m_fLastHitPointTime = 0;//上次连击点 更新时间
		private float m_fLastClickAttackBtTime = 0;//上次点击普攻按钮的时间 
		public float LastClickAttackBtTime
		{
			get { return m_fLastClickAttackBtTime; }
			set { m_fLastClickAttackBtTime = value; }
		}
		public int GetSkillIndexById(int nSkillId)
		{
			for (int nIndex = 0; nIndex < OwnSkillInfo.Length; nIndex++)
			{
				if (OwnSkillInfo[nIndex].SkillId == nSkillId)
				{
					return nIndex;
				}
			}
			return -1;
		}
		public bool IsHaveSkill(int nSkillId)
		{
			if (nSkillId ==-1)
			{
				return false;
			}
			for (int i = 0; i < OwnSkillInfo.Length; i++)
			{
				if (OwnSkillInfo[i].SkillId == nSkillId)
				{
					return true;
				}
			}
			return false;
		}
		public bool IsHaveSkill(int nSkillId,ref int nSkillIndex)
		{
			nSkillIndex = -1;
			if (nSkillId == -1)
			{
				return false;
			}
			for (int i = 0; i < OwnSkillInfo.Length; i++)
			{
				if (OwnSkillInfo[i].SkillId == nSkillId)
				{
					nSkillIndex = i;
					return true;
				}
			}
			return false;
		}
		public bool IsStudySkill(int nSkillBaseId)
		{
			for (int i = 0; i < OwnSkillInfo.Length; i++)
			{
				if (OwnSkillInfo[i].IsValid())
				{
					Tab_SkillEx _SkillEx = TableManager.GetSkillExByID(OwnSkillInfo[i].SkillId, 0);
					if (_SkillEx != null && _SkillEx.BaseId == nSkillBaseId)
					{
						return true;
					}
				}
			}
			
			return false;
		}
		public bool IsStudySkill(int nSkillBaseId,ref int nSkillInex)
		{
			nSkillInex = -1;
			for (int i = 0; i < OwnSkillInfo.Length; i++)
			{
				if (OwnSkillInfo[i].IsValid())
				{
					Tab_SkillEx _SkillEx = TableManager.GetSkillExByID(OwnSkillInfo[i].SkillId, 0);
					if (_SkillEx != null && _SkillEx.BaseId == nSkillBaseId)
					{
						nSkillInex = i;
						return true;
					}
				}
			}
			return false;
		}
		public int HaveSkillNum()
		{
			int num = 0;
			for (int i = 0; i < OwnSkillInfo.Length; i++)
			{
				if (OwnSkillInfo[i].SkillId != -1)
				{
					Tab_SkillEx _skillEx = TableManager.GetSkillExByID(OwnSkillInfo[i].SkillId, 0);
					if (_skillEx!=null)
					{
						Tab_SkillBase _skillBase = TableManager.GetSkillBaseByID(_skillEx.BaseId, 0);
						//不是普攻 不是XP 不是被动技能
						if (_skillBase !=null)
						{
							if ((_skillBase.SkillClass & (int)SKILLCLASS.AUTOREPEAT) == 0 &&
							    (_skillBase.SkillClass & (int)SKILLCLASS.XP) == 0 &&
							    (_skillBase.SkillClass & (int)SKILLCLASS.PASSIVITY) == 0)
							{
								num++;
							}
						}
					}
					// num++;
				}
			}
			return num;
		}
		public int NeedSkillBarNum() //需要开放的技能栏数
		{
			int num = 0;
			for (int i = 0; i < OwnSkillInfo.Length; i++)
			{
				if (OwnSkillInfo[i].SkillId != -1)
				{
					Tab_SkillEx _skillEx = TableManager.GetSkillExByID(OwnSkillInfo[i].SkillId, 0);
					if (_skillEx!=null)
					{
						Tab_SkillBase _skillBase = TableManager.GetSkillBaseByID(_skillEx.BaseId, 0);
						//不是普攻 不是XP 不是被动技能
						if (_skillBase !=null)
						{
							if ((_skillBase.SkillClass & (int)SKILLCLASS.AUTOREPEAT) == 0 &&
							    (_skillBase.SkillClass & (int)SKILLCLASS.XP) == 0 &&
							    (_skillBase.SkillClass & (int)SKILLCLASS.PASSIVITY) == 0)
							{
								num++;
							}
						}
					}
				}
			}
			return num;
		}
		//初始化主角身上的技能信息
		public void InitSkillInfo()
		{
			
			UpdateSkillInfo();
		}
		//更新技能CD
		void UpdateSkillCDTime()
		{
			//更新冷却时间
			if (SkillPublicTime > 0) //技能公共CD
			{
				SkillPublicTime -= (int)(Time.fixedDeltaTime * 1000);
				if (SkillPublicTime < 0)
				{
					SkillPublicTime = 0;
				}
			}
			for (int i = 0; i < OwnSkillInfo.Length; i++)
			{
				if (OwnSkillInfo[i].CDTime > 0)
				{
					OwnSkillInfo[i].CDTime -= (int)(Time.fixedDeltaTime * 1000);
					//LogModule.DebugLog(OwnSkillInfo[i].CDTime.ToString());
					if (OwnSkillInfo[i].CDTime < 0)
					{
						OwnSkillInfo[i].CDTime = 0;
					}
				}
			}
		}
		public void UpdateSkillInfo()
		{
			//更新技能栏图标
			if (SkillBarLogic.Instance() != null)
			{
				SkillBarLogic.Instance().UpdateSkillBarInfo();
			}
			for (int nIndex = 0; nIndex < OwnSkillInfo.Length; nIndex++)
			{
				//挂机时技能使用优先级
				if (OwnSkillInfo[nIndex].SkillId != -1)
				{
					Tab_SkillEx SkillExinfo = TableManager.GetSkillExByID(OwnSkillInfo[nIndex].SkillId, 0);
					if (SkillExinfo != null)
					{
						Tab_SkillBase SkillBase = TableManager.GetSkillBaseByID(SkillExinfo.BaseId, 0);
						if (SkillBase != null)
						{
							
							OwnSkillInfo[nIndex].PriorityAutoCombat = SkillBase.PriorityAutoFight;
						}
					}
				}
			}
		}
		public void StudySkillOpt(int nSkillId, int nSkillIndex)
		{
			if (nSkillId == -1)
			{
				return;
			}
			if (nSkillIndex >= 0 && nSkillIndex < OwnSkillInfo.Length)
			{
				Tab_SkillEx SkillExinfo = TableManager.GetSkillExByID(nSkillId, 0);
				if (SkillExinfo == null)
				{
					LogModule.DebugLog("SkillExinfo is Null: " + nSkillId);
					return;
				}
				Tab_SkillBase SkillBase = TableManager.GetSkillBaseByID(SkillExinfo.BaseId, 0);
				if (SkillBase == null)
				{
					LogModule.DebugLog("SkillBase is Null: " + nSkillId);
					return;
				}
				//设置图标
				OwnSkillInfo[nSkillIndex].SkillId = nSkillId;
				//设置冷却时间
				OwnSkillInfo[nSkillIndex].CDTime = 0;
				//挂机时使用的优先级
				OwnSkillInfo[nSkillIndex].PriorityAutoCombat = SkillBase.PriorityAutoFight;
				//更新技能栏图标
				if (SkillBarLogic.Instance() != null)
				{
					//不是普攻和XP技 被动技 设置技能栏位置
					if ((SkillBase.SkillClass & (int)SKILLCLASS.AUTOREPEAT) == 0 &&
					    (SkillBase.SkillClass & (int)SKILLCLASS.XP) == 0 &&
					    (SkillBase.SkillClass & (int)SKILLCLASS.PASSIVITY) == 0)
					{
						for (int _skillBarIndex = 0; _skillBarIndex < (int)SKILLBAR.MAXSKILLBARNUM; _skillBarIndex++)
						{
							//找到空位了
							if (SkillBarLogic.Instance().MySkillBarInfo[_skillBarIndex].SkillIndex == -1)
							{
								SkillBarLogic.Instance().MySkillBarInfo[_skillBarIndex].SkillIndex = nSkillIndex;
								
								//保存配置
								UserConfigData.AddSkillBarSetInfo(GUID.ToString(), SkillBarLogic.Instance().MySkillBarInfo);
								
								if (GameManager.gameManager.RunningScene == (int)GameDefine_Globe.SCENE_DEFINE.SCENE_YANMENGUANWAI)
								{
									SkillBarLogic.Instance().UpdateSkillBarInfo(); 
								}
								else
								{
									if (_skillBarIndex== 3)
									{
										GameManager.gameManager.PlayerDataPool.ForthSkillFlag = true;
									}
                                    if (GameManager.gameManager.RunningScene != (int)GameDefine_Globe.SCENE_DEFINE.SCENE_YANMENGUANWAI)
                                    {
                                        NewItemGetLogic.InitItemInfo(SkillBase.Icon,
                                                                     SkillBarLogic.Instance().MySkillBarInfo[_skillBarIndex].buttonInfo,
                                                                     NewItemGetLogic.NEWITEMTYPE.TYPE_SKILL,
                                                                     nSkillId);
                                    }
								}
								break;
							}
						}
					}
                    else if ((SkillBase.SkillClass & (int)SKILLCLASS.XP) != 0 && GameManager.gameManager.RunningScene != (int)GameDefine_Globe.SCENE_DEFINE.SCENE_YANMENGUANWAI) //XP教学
					{
						NewItemGetLogic.InitItemInfo(SkillBase.Icon,
						                             SkillBarLogic.Instance().m_SkillXPBt,
						                             NewItemGetLogic.NEWITEMTYPE.TYPE_SKILL,
						                             nSkillId);
					}
				}
			}
		}
		
		bool CheckBeforUseSkill(int nSkillId)
		{
			if (m_SkillCore == null)
			{
				return false;
			}
			if (IsDie())
			{
				return false;
			}

			//轻功中不能使用技能
			if (QingGongState)
			{
				return false;
			}
			
			if (AcceleratedMotion != null && AcceleratedMotion.Going == true)
			{
				return false;
			}
			
			if (m_bIsInModelStory)
			{
				return false;
			}
			int nSkillIndex = GetSkillIndexById(nSkillId);
			if (nSkillIndex >= 0 && nSkillIndex < OwnSkillInfo.Length)
			{
				//增加 技能公共CD
				if (OwnSkillInfo[nSkillIndex].CDTime > 0 || SkillPublicTime > 0)
				{
					//技能正在冷却中 请稍后使用
					//   SendNoticMsg("#{1245}");
					return false;
				}
			}
			Tab_SkillEx _skillEx = TableManager.GetSkillExByID(nSkillId, 0);
			if (_skillEx == null)
			{
				return false;
			}
			Tab_SkillBase _skillBase = TableManager.GetSkillBaseByID(_skillEx.BaseId, 0);
			if (_skillBase == null)
			{
				return false;
			}
			if (((_skillBase.SkillClass&(int)SKILLCLASS.AUTOREPEAT)!=0)&&(GameManager.gameManager.PlayerDataPool.Usingskill == 1)) 
			{
				return false;
			}
			//检测消耗类型
			SKILLDELANDGAINTYPE nNeedType_1 = (SKILLDELANDGAINTYPE)_skillEx.GetDelTypebyIndex(0);
			int nNeedValue1 = _skillEx.GetDelNumbyIndex(0);
			if (CheckForUseSkillNeed(nNeedType_1, nNeedValue1) == false)
			{
				return false;
			}
			SKILLDELANDGAINTYPE nNeedType_2 = (SKILLDELANDGAINTYPE)_skillEx.GetDelTypebyIndex(1);
			int nNeedValue2 = _skillEx.GetDelNumbyIndex(1);
			if (CheckForUseSkillNeed(nNeedType_2, nNeedValue2) == false)
			{
				return false;
			}
			return true;
		}
		
		public bool IsSkillNeedSelectTar(Tab_SkillBase _skillBase, Tab_SkillEx _skillEx)
		{
			if (_skillBase==null || _skillEx ==null)
			{
				return false; 
			}
			//目标类型为自己 则不选择目标 
			if ((_skillBase.TargetType & (int)CharacterDefine.TARGETTYPE.SELF) != 0)
			{
				return false;
			}
			//表里配置不选择目标的非单攻技能 则不选择目标
			if (_skillBase.SelLogic != (int)SKILLSELLOGIC.SELLOGIC_SINGLE &&
			    _skillBase.IsAutoSelectTar != 1)
			{
				return false;
			}
			return true;
		}
		//释放技能接口
		public void UseSkillOpt(int nSkillId, GameObject skillBtn)
		{
			
			if (CheckBeforUseSkill(nSkillId) == false)
			{
				return;
			}
			
			Obj_Character TargetObjChar = null;
			Tab_SkillEx _skillEx = TableManager.GetSkillExByID(nSkillId, 0);
			if (_skillEx == null)
			{
				return;
			}
			Tab_SkillBase _skillBase = TableManager.GetSkillBaseByID(_skillEx.BaseId, 0);
			if (_skillBase == null)
			{
				return;
			}
//			CurUseSkillId = nSkillId;
			if (m_selectTarget != null && m_selectTarget.ServerID != ServerID)// 当前有选择目标且不是自己 就不重新选择了
			{
				TargetObjChar = m_selectTarget;
				
				#if UNITY_ANDROID && !UNITY_EDITOR
				
				if (!m_selectTarget.gameObject.activeSelf)
				{
					TargetObjChar = null;
				}
				#endif
			}
			//          
			//需要重新选择目标 
			if (TargetObjChar == null|| TargetObjChar.IsDie()/* ||TargetObjChar.CurObjAnimState==GameDefine_Globe.OBJ_ANIMSTATE.STATE_ATTACKFLY */)
			{
				//如果目标死亡或消失 需要等技能使用结束才允许重新选择
				if(m_SkillCore.IsUsingSkill&&(_skillBase.IsSkillBreak==0)&&(_skillBase.SelLogic!=1))
				{
					return;
				}
				//if (IsSkillNeedSelectTar(_skillBase,_skillEx))
				{
					TargetObjChar = SelectSkillTarget(nSkillId);
					if (TargetObjChar != null)
					{
						OnSelectTarget(TargetObjChar.gameObject, false);
					}
					else
					{
						OnSelectTarget(null, false);
					}
				}
			}
			//
			//            //新修改，无法自动选择NPC并且移动过去，只可能发起攻击
			//           
			//			
			CurUseSkillId = nSkillId;
			OnEnterCombat(TargetObjChar);
			
		}
		public Obj_Character SelectQKSkillTarget(int nSkillId)
		{
			Tab_SkillEx _skillEx = TableManager.GetSkillExByID(nSkillId,0);
			if (_skillEx == null)
			{
				return null;
			}
			Tab_SkillBase _skillBase = TableManager.GetSkillBaseByID(_skillEx.BaseId,0);
			if (_skillBase == null)
			{
				return null;
			}
			
			//会在一次遍历中分别记录前后最近目标，前方有目标则优先选择前方，否则选择后方最近目标
			Obj_Character frontTargetObjChar = null;
			Obj_Character backTargetObjChar = null;
			float frontMinDistance = 8.0f;
			float backMinDistance = 8.0f;
			//if (Profession == (int)CharacterDefine.PROFESSION.XIAOYAO ||
			//   Profession == (int)CharacterDefine.PROFESSION.DALI)
			//{
			//    frontMinDistance = 8.0f;
			//    backMinDistance = 8.0f;
			//}
			//else
			//{
			//    frontMinDistance = 4.0f;
			//    backMinDistance = 4.0f;
			//}
			
			Dictionary<string, Obj> targets = Singleton<ObjManager>.GetInstance().ObjPools;
			foreach (Obj targetObj in targets.Values)
			{
				if (targetObj != null)
				{
					
					#if UNITY_ANDROID && !UNITY_EDITOR
					if (!targetObj.gameObject.activeSelf)
					{
						continue;
					}
					#endif
					
					Obj_Character targeObjChar = targetObj.gameObject.GetComponent<Obj_Character>();
					//	_objChar.CurObjAnimState = (GameDefine_Globe.OBJ_ANIMSTATE)packet.AnimationState;
					if (targeObjChar == null/* || targeObjChar.IsDie()||targeObjChar.CurObjAnimState==GameDefine_Globe.OBJ_ANIMSTATE.STATE_ATTACKFLY*/)
					{
						continue;
					}
					//自己排除在外
					if (targeObjChar.ServerID == ServerID)
					{
						continue;
					}
					//伙伴为不可选中目标
					if (targeObjChar.ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_FELLOW)
					{
						continue;
					}
					//不符合技能选择类型的排除
					CharacterDefine.REPUTATION_TYPE nType = Reputation.GetObjReputionType(targeObjChar);
					//技能针对敌对目标 过滤掉非敌对的目标
					if ((_skillBase.TargetType & (int)CharacterDefine.TARGETTYPE.ENEMY) != 0 &&
					    nType != CharacterDefine.REPUTATION_TYPE.REPUTATION_NEUTRAL &&
					    nType != CharacterDefine.REPUTATION_TYPE.REPUTATION_HOSTILE
					    )
					{
						continue;
					}
					//技能 针对为友好目标 过滤掉非友好目标
					if ((_skillBase.TargetType & (int)CharacterDefine.TARGETTYPE.FRIEND) != 0)
					{
						if (nType != (int)CharacterDefine.REPUTATION_TYPE.REPUTATION_FRIEND)
						{
							continue;
						}
						//可以对话的NPC 不是技能目标
						if (targeObjChar.ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_NPC)
						{
							Tab_RoleBaseAttr _roleBase = TableManager.GetRoleBaseAttrByID(targeObjChar.BaseAttr.RoleBaseID, 0);
							if (_roleBase == null || _roleBase.DialogID != -1)
							{
								continue;
							}
						}
					}
					
					//分别取得前方和后方的最近Obj
					float distance = Vector3.Distance(Position, targeObjChar.Position);
					if (IsInFront(targeObjChar))
					{
						if (distance < frontMinDistance)
						{
							frontMinDistance = distance;
							frontTargetObjChar = targeObjChar;
						}
					}
					else
					{
						if (distance < backMinDistance)
						{
							backMinDistance = distance;
							backTargetObjChar = targeObjChar;
						}
					}
				}
			}
			
			//如果前方有目标则返回前方目标，否则才返回身后目标
			if (null != frontTargetObjChar)
			{
				return frontTargetObjChar;
			}
			
			return backTargetObjChar;
		}
		public Obj_Character SelectSkillTarget(int nSkillId)
		{
			Tab_SkillEx _skillEx = TableManager.GetSkillExByID(nSkillId,0);
			if (_skillEx == null)
			{
				return null;
			}
			Tab_SkillBase _skillBase = TableManager.GetSkillBaseByID(_skillEx.BaseId,0);
			if (_skillBase == null)
			{
				return null;
			}
			if (IsSkillNeedSelectTar(_skillBase,_skillEx) ==false)
			{
				return null;
			}
			
			//会在一次遍历中分别记录前后最近目标，前方有目标则优先选择前方，否则选择后方最近目标
			Obj_Character frontTargetObjChar = null;
			Obj_Character backTargetObjChar = null;
			float frontMinDistance = 8.0f;
			float backMinDistance = 8.0f;
			//if (Profession == (int)CharacterDefine.PROFESSION.XIAOYAO ||
			//   Profession == (int)CharacterDefine.PROFESSION.DALI)
			//{
			//    frontMinDistance = 8.0f;
			//    backMinDistance = 8.0f;
			//}
			//else
			//{
			//    frontMinDistance = 4.0f;
			//    backMinDistance = 4.0f;
			//}
			
			Dictionary<string, Obj> targets = Singleton<ObjManager>.GetInstance().ObjPools;
			foreach (Obj targetObj in targets.Values)
			{
				if (targetObj != null)
				{
					
					#if UNITY_ANDROID && !UNITY_EDITOR
					if (!targetObj.gameObject.activeSelf)
					{
						continue;
					}
					#endif
					
					Obj_Character targeObjChar = targetObj.gameObject.GetComponent<Obj_Character>();
					if (targeObjChar == null || targeObjChar.IsDie())
					{
						continue;
					}
					//自己排除在外
					if (targeObjChar.ServerID == ServerID)
					{
						continue;
					}
					//伙伴为不可选中目标
					if (targeObjChar.ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_FELLOW)
					{
						continue;
					}
					//不符合技能选择类型的排除
					CharacterDefine.REPUTATION_TYPE nType = Reputation.GetObjReputionType(targeObjChar);
					//技能针对敌对目标 过滤掉非敌对的目标
					if ((_skillBase.TargetType & (int)CharacterDefine.TARGETTYPE.ENEMY) != 0 &&
					    nType != CharacterDefine.REPUTATION_TYPE.REPUTATION_NEUTRAL &&
					    nType != CharacterDefine.REPUTATION_TYPE.REPUTATION_HOSTILE
					    )
					{
						continue;
					}
					//技能 针对为友好目标 过滤掉非友好目标
					if ((_skillBase.TargetType & (int)CharacterDefine.TARGETTYPE.FRIEND) != 0)
					{
						if (nType != (int)CharacterDefine.REPUTATION_TYPE.REPUTATION_FRIEND)
						{
							continue;
						}
						//可以对话的NPC 不是技能目标
						if (targeObjChar.ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_NPC)
						{
							Tab_RoleBaseAttr _roleBase = TableManager.GetRoleBaseAttrByID(targeObjChar.BaseAttr.RoleBaseID, 0);
							if (_roleBase == null || _roleBase.DialogID != -1)
							{
								continue;
							}
						}
					}
					
					//分别取得前方和后方的最近Obj
					float distance = Vector3.Distance(Position, targeObjChar.Position);
					if (IsInFront(targeObjChar))
					{
						if (distance < frontMinDistance)
						{
							frontMinDistance = distance;
							frontTargetObjChar = targeObjChar;
						}
					}
					else
					{
						if (distance < backMinDistance)
						{
							backMinDistance = distance;
							backTargetObjChar = targeObjChar;
						}
					}
				}
			}
			
			//如果前方有目标则返回前方目标，否则才返回身后目标
			if (null != frontTargetObjChar)
			{
				return frontTargetObjChar;
			}
			
			return backTargetObjChar;
		}
		public bool  UpdateTarget()
		{

			//===自动选择目标需排除以下操作 1自动寻路中 2正在普攻连击中 3目标不为空且没有在移动中 4如果有手动选择的目标 5自动战斗中
			if(GameManager.gameManager.AutoSearch.IsAutoSearching)
				return false;
			if(SkillBarLogic.Instance()!=null )//&& SkillBarLogic.Instance().inAttTemp
				return false;
			if (m_selectTarget != null&&IsMoving==false)
				return false;
			if(m_onSelectForClick)
				return false;
			if (AutoComabat == true)
			{
				return false;
			}
			//会在一次遍历中分别记录前后最近目标，前方有目标则优先选择前方，否则选择后方最近目标
			Obj_Character frontTargetObjChar = null;
			Obj_Character backTargetObjChar = null;
			float frontMinDistance = 8.0f;
			float backMinDistance = 8.0f;
			//if (Profession == (int)CharacterDefine.PROFESSION.XIAOYAO ||
			//   Profession == (int)CharacterDefine.PROFESSION.DALI)
			//{
			//    frontMinDistance = 8.0f;
			//    backMinDistance = 8.0f;
			//}
			//else
			//{
			//    frontMinDistance = 4.0f;
			//    backMinDistance = 4.0f;
			//}
			
			Dictionary<string, Obj> targets = Singleton<ObjManager>.GetInstance().ObjPools;
			foreach (Obj targetObj in targets.Values)
			{
				if (targetObj != null)
				{
					
					#if UNITY_ANDROID && !UNITY_EDITOR
					if (!targetObj.gameObject.activeSelf)
					{
						continue;
					}
					#endif
					
					Obj_Character targeObjChar = targetObj.gameObject.GetComponent<Obj_Character>();
					if (targeObjChar == null || targeObjChar.IsDie())
					{
						continue;
					}
					//自己排除在外
					if (targeObjChar.ServerID == ServerID)
					{
						continue;
					}
					//伙伴为不可选中目标
					if (targeObjChar.ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_FELLOW)
					{
						continue;
					}
					//不符合技能选择类型的排除
					CharacterDefine.REPUTATION_TYPE nType = Reputation.GetObjReputionType(targeObjChar);
					
					//技能 针对为友好目标 过滤掉非友好目标
					
					if (nType == (int)CharacterDefine.REPUTATION_TYPE.REPUTATION_FRIEND)
					{
						continue;
					}
					//可以对话的NPC 不是技能目标
					if (targeObjChar.ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_NPC)
					{
						Tab_RoleBaseAttr _roleBase = TableManager.GetRoleBaseAttrByID(targeObjChar.BaseAttr.RoleBaseID, 0);
						if (_roleBase == null || _roleBase.DialogID != -1)
						{
							continue;
						}
					}
					
					
					//分别取得前方和后方的最近Obj
					float distance = Vector3.Distance(Position, targeObjChar.Position);
					
					if (distance < frontMinDistance)
					{
						frontMinDistance = distance;
						frontTargetObjChar = targeObjChar;
					}
					
				}
			}
			
			//如果前方有目标则返回前方目标，否则才返回身后目标
			if (null != frontTargetObjChar)
			{
				OnSelectTarget(frontTargetObjChar.gameObject);
				SelectTarget=frontTargetObjChar;
				//GameManager.gameManager.ActiveScene.ActiveSelectCircle(SelectTarget.gameObject,SelectTarget);
				return true;
			}
			
			
			return false;
		}
		//切换当前怪物
		private bool checkTargetObj(Obj targetObj)
		{
			if (null == targetObj)
			{
				return true;
			}
			if (targetObj.ObjType != GameDefine_Globe.OBJ_TYPE.OBJ_NPC)
			{
				return true;
			}
			//自己排除在外
			if (targetObj.ServerID == ServerID)
			{
				return true;
			}
			Obj_NPC targeObjChar = targetObj.gameObject.GetComponent<Obj_NPC>();
			if (targeObjChar == null || targeObjChar.IsDie())
			{
				return true;
			}
			CharacterDefine.REPUTATION_TYPE nType = Reputation.GetObjReputionType(targeObjChar);
			if (nType != CharacterDefine.REPUTATION_TYPE.REPUTATION_HOSTILE &&
			    nType != CharacterDefine.REPUTATION_TYPE.REPUTATION_NEUTRAL)
			{
				return true;
			}
			
			return false;
		}
		
		private List<int>  m_selectedId=new List<int>();
		private List<int>  m_canSelectedId=new List<int>();
		private Obj_Character SwitchMonsterTarget()
		{
			Obj_Character frontTargetObjChar = null;
			Obj_Character backTargetObjChar = null;
			float frontMinDistance = 8.0f;
			float backMinDistance = 8.0f;
			//if (Profession == (int)CharacterDefine.PROFESSION.XIAOYAO ||
			//  Profession == (int)CharacterDefine.PROFESSION.DALI)
			//{
			//    frontMinDistance = 8.0f;
			//    backMinDistance = 8.0f;
			//}
			//else
			//{
			//    frontMinDistance = 4.0f;
			//    backMinDistance = 4.0f;
			//}
			//=============
			m_canSelectedId.Clear ();
			Dictionary<string, Obj> targets1 = Singleton<ObjManager>.GetInstance().ObjPools;
			foreach (Obj targetObj in targets1.Values)
			{
				if(checkTargetObj(targetObj))
					continue;
				Obj_NPC targeObjChar = targetObj.gameObject.GetComponent<Obj_NPC>();
				//分别取得前方和后方的最近Obj
				float distance = Vector3.Distance(Position, targeObjChar.Position);
				
				if (distance < frontMinDistance)
				{
					frontMinDistance = distance;
					
					if(!m_canSelectedId.Contains(targetObj.ServerID))
					{
						m_canSelectedId.Add(targetObj.ServerID);
					}
				}
			}
			for(int i=0;i<m_selectedId.Count;i++)//int id in m_selectedId)
			{
				if(m_canSelectedId.IndexOf(m_selectedId[i]) == -1)
				{
					m_selectedId.Remove(m_selectedId[i]);
					i=0;
				}
			}
			//=============
			//记录当前选择目标的ServerID
			//			if (m_canSelectedId.Count == ObjManager.GetInstance ().m_MonsterList.Count) 
			//			{
			//				m_selectedId.Clear();
			//			}
			if (m_canSelectedId.Count == m_selectedId.Count) 
			{
				m_selectedId.Clear();
			}
			
			int curSelecTargetServerID = GlobeVar.INVALID_ID;
			if (null != m_selectTarget)
			{
				curSelecTargetServerID = m_selectTarget.ServerID;
			}
			
			frontMinDistance = 8.0f;
			
			Dictionary<string, Obj> targets = Singleton<ObjManager>.GetInstance().ObjPools;
			foreach (Obj targetObj in targets.Values)
			{
				if(checkTargetObj(targetObj))
					continue;
				//已经选择的目标是不会被切换的
				if (curSelecTargetServerID != GlobeVar.INVALID_ID && targetObj.ServerID == curSelecTargetServerID)
				{
					continue;
				}
				if(m_selectedId.Contains(targetObj.ServerID))
				{
					continue;
				}
				Obj_NPC targeObjChar = targetObj.gameObject.GetComponent<Obj_NPC>();
				//分别取得前方和后方的最近Obj
				float distance = Vector3.Distance(Position, targeObjChar.Position);
				
				if (distance < frontMinDistance)
				{
					frontMinDistance = distance;
					frontTargetObjChar = targeObjChar;
				}
				
			}
			
			//如果前方有目标则返回前方目标，否则才返回身后目标
			if (null != frontTargetObjChar)
			{
				if(!m_selectedId.Contains(curSelecTargetServerID))
				{
					m_selectedId.Add(curSelecTargetServerID);
				}
				return frontTargetObjChar;
			}
			
			return null;
		}
		
		//切换当前玩家
		private bool checkTargetObjPlayer(Obj targetObj)
		{
			if (null == targetObj)
			{
				return true;
			}
			
			if (targetObj.ObjType != GameDefine_Globe.OBJ_TYPE.OBJ_OTHER_PLAYER)
			{
				return true;
			}
			
			//自己排除在外
			if (targetObj.ServerID == ServerID)
			{
				return true;
			}
			
			Obj_OtherPlayer targetOtherPlayer = targetObj as Obj_OtherPlayer;
			if (targetOtherPlayer == null || targetOtherPlayer.IsDie())
			{
				return true;
			}
			
			CharacterDefine.REPUTATION_TYPE nType = Reputation.GetObjReputionType(targetOtherPlayer);
			if (nType != CharacterDefine.REPUTATION_TYPE.REPUTATION_HOSTILE)
			{
				return true;
			}
			
			return false;
		}
		
		private List<int>  m_selectedIdPlayer=new List<int>();
		private List<int>  m_canSelectedIdPlayer=new List<int>();
		//切换当前玩家目标
		private Obj_Character SwitchPlayerTarget()
		{
			Obj_Character frontTargetObjChar = null;
			Obj_Character backTargetObjChar = null;
			float frontMinDistance = 10.0f;
			float backMinDistance = 10.0f;
			
			//==========
			m_canSelectedIdPlayer.Clear ();
			Dictionary<string, Obj> targets1 = Singleton<ObjManager>.GetInstance().ObjPools;
			foreach (Obj targetObj in targets1.Values)
			{
				
				if(checkTargetObjPlayer(targetObj))
					continue;
				
				Obj_OtherPlayer targetOtherPlayer = targetObj as Obj_OtherPlayer;
				//分别取得前方和后方的最近Obj
				float distance = Vector3.Distance(Position, targetOtherPlayer.Position);
				if (distance < frontMinDistance)
				{
					frontMinDistance = distance;
					
					if(!m_canSelectedIdPlayer.Contains(targetObj.ServerID))
					{
						m_canSelectedIdPlayer.Add(targetObj.ServerID);//m_selectedIdPlayer.Add(targetObj.ServerID);
					}
				}
			}
			for(int i=0;i<m_selectedIdPlayer.Count;i++)//int id in m_selectedId)
			{
				if(m_canSelectedIdPlayer.IndexOf(m_selectedIdPlayer[i]) == -1)
				{
					m_selectedIdPlayer.Remove(m_selectedIdPlayer[i]);
					i=0;
				}
			}
			if (m_canSelectedIdPlayer.Count == m_selectedIdPlayer.Count) 
			{
				m_selectedIdPlayer.Clear();
			}
			
			//===========
			//记录当前选择目标的ServerID
			int curSelecTargetServerID = GlobeVar.INVALID_ID;
			if (null != m_selectTarget)
			{
				curSelecTargetServerID = m_selectTarget.ServerID;
			}
			
			frontMinDistance = 10.0f;
			
			Dictionary<string, Obj> targets = Singleton<ObjManager>.GetInstance().ObjPools;
			foreach (Obj targetObj in targets.Values)
			{
				if(checkTargetObjPlayer(targetObj))
					continue;
				
				//已经选择的目标是不会被切换的
				if (curSelecTargetServerID != GlobeVar.INVALID_ID && targetObj.ServerID == curSelecTargetServerID)
				{
					continue;
				}
				
				if(m_selectedIdPlayer.Contains(targetObj.ServerID))
				{
					continue;
				}
				
				Obj_OtherPlayer targetOtherPlayer = targetObj as Obj_OtherPlayer;
				
				//分别取得前方和后方的最近Obj
				float distance = Vector3.Distance(Position, targetOtherPlayer.Position);
				
				if (distance < frontMinDistance)
				{
					frontMinDistance = distance;
					frontTargetObjChar = targetOtherPlayer;
				}
				
			}
			
			//如果前方有目标则返回前方目标，否则才返回身后目标
			if (null != frontTargetObjChar)
			{
				if(!m_selectedIdPlayer.Contains(curSelecTargetServerID))
				{
					m_selectedIdPlayer.Add(curSelecTargetServerID);
				}
				return frontTargetObjChar;
			}
			
			return null;
		}
		
		public void SwitchTarget()
		{
			Obj_Character obj = null;
			//PK模式下，先选择敌对列表中的敌人，然后选择敌对玩家，最后是怪物
			//处于反击状态时，选择目标规则与进入杀戮模式相同
			if (PkModle == (int)CharacterDefine.PKMODLE.KILL || 
			    IsCanPKLegal ||
			    BaseAttr.Force ==(int)GameDefine_Globe.FORCETYPE.PVP1 || //PVP 势力
			    BaseAttr.Force ==(int)GameDefine_Globe.FORCETYPE.PVP2 //PVP 势力
			    )
			{
				obj = SwitchPlayerTarget();
				if (null == obj)
				{
					obj = SwitchMonsterTarget();
				}
			}
			else
			{
				//非PK模式下，先选择敌对列表中的敌人
				obj = SwitchMonsterTarget();
			}
			
			//未选中则选择怪物
			if (null != obj)
			{
				//取消选择光环
				if(m_selectTarget!=null)
					m_selectTarget.CancelOutLine();
				m_selectTarget = null;
				
				OnSelectTarget(obj.gameObject);
			}
		}
		
		public override void OnEnterCombat(Obj_Character Target)
		{
			//无技能 默认使用普攻
			if (CurUseSkillId == -1)
			{
				CurUseSkillId = OwnSkillInfo[0].SkillId;
			}
			if (CheckBeforUseSkill(CurUseSkillId) == false)
			{
				return;
			}
			Tab_SkillEx skillExInfo = TableManager.GetSkillExByID(CurUseSkillId, 0);
			if (skillExInfo == null)
			{
				return;
			}
			Tab_SkillBase skillBaseInfo = TableManager.GetSkillBaseByID(skillExInfo.BaseId, 0);
			if (skillBaseInfo == null)
			{
				return;
			}
			// 攻击状态需要退出跟随状态
//			Singleton<ObjManager>.GetInstance().MainPlayer.LeaveTeamFollow();
//			Singleton<ObjManager>.GetInstance ().MainPlayer.RideOrUnMount (-1);
//			if (Target && Reputation.GetObjReputionType (Target) == CharacterDefine.REPUTATION_TYPE.REPUTATION_FRIEND) 
//			{
//				return ;
//			}
				//如果开打的时候 当前选择的目标是友好的目标则重新选择一个可以攻击的目标
			if (Target && Reputation.GetObjReputionType(Target) == CharacterDefine.REPUTATION_TYPE.REPUTATION_FRIEND)
			{
				if ((Target.ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_NPC || Target.ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_MAIN_PLAYER)
				    && (skillBaseInfo.TargetType & (int)CharacterDefine.TARGETTYPE.SELF) == 0
				    && (skillBaseInfo.TargetType & (int)CharacterDefine.TARGETTYPE.FRIEND) == 0
				    )
				{
					Target = SelectSkillTarget(CurUseSkillId);
					if (Target)
					{
						OnSelectTarget(Target.gameObject);
					}
				}
			}
			//            //单攻和普攻需要目标
			if (skillBaseInfo.SelLogic == (int)SKILLSELLOGIC.SELLOGIC_SINGLE ||
			    (skillBaseInfo.SkillClass & (int)SKILLCLASS.AUTOREPEAT) != 0)
			{
				if (Target == null||Target.IsDie()||(Target.CurObjAnimState==GameDefine_Globe.OBJ_ANIMSTATE.STATE_ATTACKFLY))
				{
					if(!AutoComabat)//自动挂机情况下去掉这个提示
					SendNoticMsg(false,"#{1391}");
//					//===找不到目标时将正在连击标记置空
//					SkillBarLogic.Instance().inAttTemp = false;
					return;
				}
				//				else
				//				{
				//					m_selectTarget=Target;
				//				}
				//				if (Target != null&&Target.CurObjAnimState==GameDefine_Globe.OBJ_ANIMSTATE.STATE_ATTACKFLY)
				//				{
				//					// SendNoticMsg("#{1391}");
				//					return;
				//				}
				
			}
			int nTargetID = -1;
			//向选中目标移动  没在移动状态 能移动 且需要向目标移动则移动
			if (Target != null && IsSkillNeedSelectTar(skillBaseInfo,skillExInfo))
			{
				float skillRadius = skillExInfo.Radius;
//				Tab_RoleBaseAttr  role=TableManager.GetRoleBaseAttrByID(
//					Target.BaseAttr.RoleBaseID,0);
//				float roleradio=0.0f;
//				if(role!=null)
//					roleradio=role.SelectRadius;
				float dis = Vector3.Distance(Position, Target.Position);
				if (Target.BaseAttr.MoveSpeed <= 0 &&
				    IsOpenAutoCombat &&
				    AutoComabat && 
				    dis >1) //挂机状态下 对于那些不能移动目标 跑到目标点再放技能 防止出现因为服务器跟客户端位置的偏差 导致技能打不到的情况
				{

					//==========
					if (IsMoving == false && IsCanOperate_Move())
					{

						MoveTo(Target.Position, Target.gameObject, 0.5f);
					}
					return;

				}
				else
				{
					float diffDistance = dis - skillRadius;
					float  bjdis =0.0f;
					if((Target.ObjType==GameDefine_Globe.OBJ_TYPE.OBJ_LUZHANG))
					{
						bjdis=3.0f;
					}

					if (diffDistance >bjdis)
					{
						//move
						if (IsMoving == false && IsCanOperate_Move())
						{

			
							MoveTo(Target.Position, Target.gameObject, skillRadius - 1.0f);
						}
						return;
					}
				}
					//===========
				/*
					float diffDistance = dis-roleradio - skillRadius;
//					if(Profession==(int)CharacterDefine.PROFESSION.XIAOYAO||skillExInfo.CheckTime<=0)
//					{
//						if(diffDistance>=0)
//						{
//							MoveTo(Target.Position, Target.gameObject, skillRadius +roleradio- 0.5f);
//							
//							return ;
//						}
//					}
//					else
					{
						if(diffDistance>0)
						{
							MoveTo(Target.Position, Target.gameObject, skillRadius +roleradio);
							
							return ;
						}
//						if (diffDistance > 0&&diffDistance<3)
//						{
//							//move
//							if (IsMoving == false && IsCanOperate_Move())
//							{
//								this.FaceTo(Target.Position);
//								//AttackFly(10,5,1.0f);
//								AttackPGCF(Target);
//								//MoveTo(Target.Position, Target.gameObject, skillRadius +roleradio- 0.5f);
//							}
//							return;
//						}
					}
				}
				else
				{
					float diffDistance = dis-roleradio - skillRadius;
//					if(Profession==(int)CharacterDefine.PROFESSION.XIAOYAO||skillExInfo.CheckTime<=0)
//					{
//						if(diffDistance>=0)
//						{
//							MoveTo(Target.Position, Target.gameObject, skillRadius +roleradio- 0.5f);
//							
//							return ;
//						}
//					}
//					else
					{
						if(diffDistance>0)
						{
							MoveTo(Target.Position, Target.gameObject, skillRadius +roleradio);
							
							return ;
						}
					}
					
				}

						*/


				nTargetID = Target.ServerID;
			}


//			if(Target!=null)
//			{
//				if(skillBaseInfo.Id!=5009)//法师瞬移的时候不用看向目标
//				   this.gameObject.transform.LookAt(Target.Position);
//			}



			// 对方玩家低于30级不攻击
//			if((null != Target) && (Target.ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_OTHER_PLAYER))
//			{
//				Obj_OtherPlayer o = Target as Obj_OtherPlayer;
//				if(o.BaseAttr.Level < 30)
//				{
//					SendNoticMsg(false, "#{1112}");
//					return;
//				}
//			}
//			if (skillBaseInfo.SelLogic == (int)SKILLSELLOGIC.SELLOGIC_ALL)
//			{
//				nTargetID=this.ServerID;
//			}
//			if(Target!=null)
//			 {
//				if(skillBaseInfo.Id!=5009)//法师瞬移的时候不用看向目标
//				this.transform.LookAt(Target.Position);
//			}


			ActiveSkill(CurUseSkillId,nTargetID);
			CurUseSkillId = -1;
			base.OnEnterCombat(Target);
			
			//====使用技能下马
			//			int m_CurMountID = GameManager.gameManager.PlayerDataPool.m_objMountParam.MountID;
			//=====使用技能前端显示逻辑为下马 具体数据由服务器数据更新为准

			//			CG_MOUNT_UNMOUNT packet = (CG_MOUNT_UNMOUNT)PacketDistributed.CreatePacket(MessageID.PACKET_CG_MOUNT_UNMOUNT);
			//			packet.SetMountID(m_CurMountID);
			//			packet.SendPacket();
			
		}
		public void ActiveSkill(int nSkillId,int targetId)
		{
			if (m_SkillCore ==null)
			{
				return;
			}
			Tab_SkillEx SkillExinfo = TableManager.GetSkillExByID(nSkillId,0);
			if (SkillExinfo == null)
			{
				LogModule.DebugLog("SkillExinfo is Null: " + nSkillId);
				return;
			}
			int BaseSkillId = SkillExinfo.BaseId;
			Tab_SkillBase SkillBaseinfo = TableManager.GetSkillBaseByID(BaseSkillId, 0);
			if (SkillBaseinfo == null)
			{
				LogModule.DebugLog("SkillBaseInfo is Null" + BaseSkillId);
				return;
			}
			if (UseSkillCheck (targetId, SkillExinfo, SkillBaseinfo))
			{
			//====对比旧逻辑删除不必要的逻辑20160606
			//	StopMove();
//				CG_MOVE movPacket = (CG_MOVE)PacketDistributed.CreatePacket(MessageID.PACKET_CG_MOVE);
//				movPacket.Poscount = 1;
//				movPacket.AddPosx((int)(m_ObjTransform.position.x * 100));
//				movPacket.AddPosz((int)(m_ObjTransform.position.z * 100));
//				
//				movPacket.Ismoving =1;
//				movPacket.SendPacket();

				//发送技能使用包  by dsy  修改战斗发送消息时序，变为在战斗中发送
				CG_SKILL_USE usekill = (CG_SKILL_USE)PacketDistributed.CreatePacket (MessageID.PACKET_CG_SKILL_USE);
				usekill.SetSkillId (nSkillId);
			
				//Debug.LogError("nskillid+"+nSkillId);
				usekill.SetTargetId (targetId);
				float face=Utils.DirClientToServer(this.Rotation);
				int facedir=(int)(face*100);

				usekill.SetPlayerFace(facedir);
//				usekill.SetIsEnd (0);
//				usekill.SetSkillAnimationIndex (0);
//				if(targetId!=-1)
//				{
//				Vector3 posV3 = ObjManager.Instance.FindObjCharacterInScene(targetId).Position;
//				usekill.PosX =((int)(posV3.x *100f));
//				usekill.PosY =((int)(posV3.z *100f));
//				}
				usekill.SendPacket ();
				if((SkillBaseinfo.SkillClass&(int)SKILLCLASS.AUTOREPEAT)!=0)
				GameManager.gameManager.PlayerDataPool.Usingskill=1;
//				if(SkillExinfo.CheckTime!=-1)
//					SkillCore.UseSkill (nSkillId, ServerID, targetId);
			}
			
		}
		//CD检测  消耗检测（血 蓝）
		//目标是否可以攻击(距离判断)
		//
		public bool UseSkillCheck(int targetId, Tab_SkillEx SkillExinfo, Tab_SkillBase SkillBaseinfo)
		{
			Obj_Character Target = Singleton<ObjManager>.GetInstance().FindObjCharacterInScene(targetId);
			if (IsDie())
			{
				return false;
			}
			if (Target!=null&&Target.ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_OTHER_PLAYER && GameManager.gameManager.RunningScene ==(int) GameDefine_Globe.SCENE_DEFINE.SCENE_ERHAI) 
			{
				return false;
			}
			//正在使用技能 且技能无法被打断 则无法使用
			if (null != m_SkillCore && m_SkillCore.IsUsingSkill)
			{
				if (m_SkillCore.UsingSkillBaseInfo != null && m_SkillCore.UsingSkillExInfo!= null)
				{
					
//					if(SkillExinfo.CheckTime != -1)
//					{
//						return false;
//					}
					//正在使用的技能 无法被打断 则不允许使用
					if (m_SkillCore.UsingSkillBaseInfo.IsSkillBreak != 1)
					{
						return false;
					}
					
					//正在使用连续技 则不允许再次使用连续技
					if ((m_SkillCore.UsingSkillBaseInfo.SkillClass & (int)SKILLCLASS.AUTOREPEAT) != 0 &&
					    (SkillBaseinfo.SkillClass & (int)SKILLCLASS.AUTOREPEAT) != 0)
					{
						return false;
					}
				}
			}
			//CD 时间
			//检测目标合法性 群攻可以无目标释放
			if (SkillBaseinfo.SelLogic == (int)SKILLSELLOGIC.SELLOGIC_SINGLE ||
			    (SkillBaseinfo.SkillClass & (int)SKILLCLASS.AUTOREPEAT) != 0)
			{
				if (Target == null)
				{
					return false;
				}
				
				if (Target.IsDie())
				{
					//  _mainPlayer.SendNoticMsg("#{1391}");
					return false;
				}
			}
			//CD 时间检测 增加公共CD判断
			int nSkillIndex = GetSkillIndexById(SkillExinfo.SkillExID);
			if (nSkillIndex >= 0 && nSkillIndex <OwnSkillInfo.Length)
			{
				if (OwnSkillInfo[nSkillIndex].CDTime > 0 || SkillPublicTime > 0)
				{
					//   SendNoticMsg("#{1245}");
					return false;
				}
			}
			//检测消耗类型
			SKILLDELANDGAINTYPE nNeedType_1 = (SKILLDELANDGAINTYPE)SkillExinfo.GetDelTypebyIndex(0);
			int nNeedValue1 = SkillExinfo.GetDelNumbyIndex(0);
			if (CheckForUseSkillNeed(nNeedType_1, nNeedValue1) == false)
			{
				return false;
			}
			SKILLDELANDGAINTYPE nNeedType_2 = (SKILLDELANDGAINTYPE)SkillExinfo.GetDelTypebyIndex(1);
			int nNeedValue2 = SkillExinfo.GetDelNumbyIndex(1);
			if (CheckForUseSkillNeed(nNeedType_2, nNeedValue2) == false)
			{
				return false;
			}
			return true;
		}
		protected bool CheckForUseSkillNeed(SKILLDELANDGAINTYPE nType, int nNeedValue)
		{
			switch (nType)
			{
			case SKILLDELANDGAINTYPE.HPTYPE_VALUE://HP的数值
				if (BaseAttr.HP - nNeedValue <= 0)
				{
					SendNoticMsg(false, "#{1247}");
					return false;
				}
				break;
			case SKILLDELANDGAINTYPE.HPTYPE_RATE://HP的百分比
				
				if (BaseAttr.HP - BaseAttr.MaxHP * nNeedValue / 100 <= 0)
				{
					SendNoticMsg(false, "#{1247}");
					return false;
				}
				break;
			case SKILLDELANDGAINTYPE.MPTYPE_VALUE://MP数值
				if (BaseAttr.MP - nNeedValue < 0)
				{
					SendNoticMsg(false, "#{1248}");
					return false;
				}
				break;
			case SKILLDELANDGAINTYPE.MPTYPE_RATE: //MP百分比
				
				if (BaseAttr.MP - BaseAttr.MaxMP * nNeedValue / 100 < 0)
				{
					SendNoticMsg(false, "#{1248}");
					return false;
				}
				break;
			case SKILLDELANDGAINTYPE.XPTYPE_VALUE: //XP数值
				if (BaseAttr.XP - nNeedValue < 0)
				{
					SendNoticMsg(false, "#{1244}");
					return false;
				}
				break;
			case SKILLDELANDGAINTYPE.XPTYPE_RATE: //XP百分比
				if (BaseAttr.XP - BaseAttr.MaxXP * nNeedValue / 100 < 0)
				{
					SendNoticMsg(false, "#{1244}");
					return false;
				}
				break;
			default:
				return true;
			}
			return true;
		}
		public void ChangeHit(int nIncHitCount, bool isCritical)
		{
			m_nHitPoint = m_nHitPoint + nIncHitCount;
			if (m_fLastHitPointTime>0 && Time.time-m_fLastHitPointTime > 5.0f) //5s清零
			{
				m_nHitPoint = 0;
			}
			m_fLastHitPointTime = Time.time;
			if (PlayerHitsLogic.Instance() != null)
			{
				PlayerHitsLogic.Instance().AddPlayerHits(m_nHitPoint, isCritical);
			}            
		}
		
		public bool IsHaveNoMoveBuff() //是否拥有不能移动的BUFF
		{
			for (int i = 0; i < ClientImpactInfo.Count; i++)
			{
				if (ClientImpactInfo[i].ImpactLogicId == 8 ||
				    ClientImpactInfo[i].ImpactLogicId == 11 ||
				    ClientImpactInfo[i].ImpactLogicId == 12 ||
				    ClientImpactInfo[i].ImpactLogicId == 18 
				    )
				{
					return true;
				}
			}
			return false;
		}
		
		public bool IsHaveStealthBuff() //是否拥有隐身BUFF
		{
			for (int i = 0; i < ClientImpactInfo.Count; i++)
			{
				if (ClientImpactInfo[i].ImpactLogicId == 17)
				{
					return true;
				}
			}
			return false;
		}
		
		public int GetTotalStudySkillCombatValue() //获取所有已学会技能的战斗力总值
		{
			int nTotalValue = 0;
			for (int i = 0; i < OwnSkillInfo.Length; i++)
			{
				if (OwnSkillInfo[i].IsValid())
				{
					Tab_SkillEx _skillEx = TableManager.GetSkillExByID(OwnSkillInfo[i].SkillId, 0);
					if (_skillEx !=null)
					{
						nTotalValue += _skillEx.CombatValue;
					}
				}
			}
			return nTotalValue;
		}
		void UpdateClientImpactInfo()
		{
			//             for (int i = 0; i < ClientImpactInfo.Count; i++)
			//             {
			//                 //!!!这里是为了防止出现丢包的情况 客户端的BUFF信息没清除的保险措施 做1s的延迟
			//                 if (ClientImpactInfo[i].IsForever ==false)
			//                 {
			//                     ClientImpactInfo[i].RemainTime -= Time.deltaTime;
			//                     if (ClientImpactInfo[i].RemainTime <= -1.0f)
			//                     {
			//                         ClientImpactInfo[i].CleanUp();
			//                     }
			//                 }
			//             }
		}
	}
}