/********************************************************************************
 *	文件名：	SkillCore.cs
 *	全路径：	\Script\GameManager\SkillCore.cs
 *	创建人：	张旭辉
 *	创建时间：2013-11-06
 *
 *	功能说明：客户端技能逻辑核心，负责客户端技能逻辑处理
 *	修改记录：
*********************************************************************************/
using UnityEngine;

using System.Collections;
using Games.LogicObj;
using Games.GlobeDefine;
using GCGame.Table;
using System.Collections.Generic;
using UnityEngineInternal;
using Module.Log;
namespace Games.SkillModle
{
	public class SkillCore 
	{
        private class BulletData
        {
            public BulletData(int selfID, int targetID)
            {
                _selfID = selfID;
                _targetID = targetID;
            }

            public int _selfID;
            public int _targetID;
        }
        public SkillCore()
        {
        }
		protected int m_nLastSkillId =-1;
		public int LastSkillId
		{
			get { return m_nLastSkillId; }
		}
		protected int m_nLastSendId = -1;
		protected int m_nLastTargetId = -1;
		public int LastTargetId
		{
			get { return m_nLastTargetId; }
		}
		private bool m_bIsUsingSkill = false;
		public bool IsUsingSkill
		{
			get { return m_bIsUsingSkill; }
			set { m_bIsUsingSkill = value; }
		}

	    private float m_fLastUseSkillTime =0.0f;
        public float LastUseSkillTime
        {
            get { return m_fLastUseSkillTime; }
            set { m_fLastUseSkillTime = value; }
        }
	    private float m_OldCameraScan = -1.0f;
        private Tab_SkillEx m_UsingSkillExInfo = null;//!!!使用前记得判空 缓存下技能信息
        public GCGame.Table.Tab_SkillEx UsingSkillExInfo//!!!使用前记得判空 缓存下技能信息
        {
            get { return m_UsingSkillExInfo; }
            set { m_UsingSkillExInfo = value; }
        }

        private Tab_SkillBase m_UsingSkillBaseInfo = null; //!!!使用前记得判空 缓存下技能信息
        public GCGame.Table.Tab_SkillBase UsingSkillBaseInfo //!!!使用前记得判空 缓存下技能信息
        {
            get { return m_UsingSkillBaseInfo; }
            set { m_UsingSkillBaseInfo = value; }
        }

        private Obj_Character m_SkillSender = null;//!!!使用前记得判空 缓存下技能发送者信息
        public Games.LogicObj.Obj_Character SkillSender //!!!使用前记得判空 缓存下技能发送者信息
        {
            get { return m_SkillSender; }
            set { m_SkillSender = value; }
        }
	    public void CheckSkillShouldFinish()
	    {
	        if (m_bIsUsingSkill ==false)
	        {
	            return;
	        }
	        if (m_nLastSkillId ==-1)
	        {
	           return;   
            }
	        if (m_UsingSkillBaseInfo ==null || m_UsingSkillExInfo ==null)
	        {
	            return;
	        }
            //客户端检测技能是否结束
            int _skillContinuTime = m_UsingSkillExInfo.SkillContinueTime; 
			if (m_SkillSender.ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_MAIN_PLAYER) 
			{   
				Obj_MainPlayer obj=(Obj_MainPlayer)m_SkillSender;
				if(obj.AutoComabat)
				{
//				if(m_UsingSkillExInfo.CheckTime!=-1)
//					_skillContinuTime=_skillContinuTime-100;
				}
			}
	        float _fElapseTime =(Time.time - m_fLastUseSkillTime)*1000.0f;
            //做1s的延迟处理
            if (_fElapseTime-_skillContinuTime>0)
	        {
	            SkillFinsh();
            }
	    }

        /// <summary>
        /// 释放技能
        /// </summary>
        /// <param name="skillId">技能ID</param>
        /// <param name="senderId">释放者ID</param>
        /// <param name="targetId">目标ID</param>
        /// <param name="skillname">技能名称</param>
        public void UseSkill(int skillId, int senderId, int targetId, string skillname = "")
		{
            //获取主角
			Obj_Character Sender = Singleton<ObjManager>.GetInstance().FindObjCharacterInScene(senderId);
            //判空
			if (Sender.ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_OTHER_PLAYER)
			{
				if (m_bIsUsingSkill)
				{
					return;
					
				}
			}
			if (m_bIsUsingSkill)
		    {
		        BreakCurSkill();
               
		    }
			if (Sender.m_bIsYS == true) 
			{
				Sender.m_fXSLastTime=0;
			}
            m_fLastUseSkillTime = Time.time;
			m_SkillSender = Singleton<ObjManager>.GetInstance().FindObjCharacterInScene(senderId);
            if (m_SkillSender == null)
			{
				LogModule.DebugLog("MainPlayer is Null:" + senderId);
				return ;
			}
            //读表
			//m_UsingSkillExInfo = TableManager.GetSkillExByID(skillId,0);
   //         if (m_UsingSkillExInfo == null)
			//{
			//	LogModule.DebugLog("SkillExinfo is Null: " + skillId);
			//	return ;
			//}
			//int BaseSkillId = m_UsingSkillExInfo.BaseId;
			//m_UsingSkillBaseInfo= TableManager.GetSkillBaseByID(BaseSkillId,0);
            if (m_UsingSkillBaseInfo == null)
			{
				LogModule.DebugLog("SkillBaseInfo is Null" + BaseSkillId);
				return ;
			}
			if (m_SkillSender.ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_MAIN_PLAYER)
			{
				if (m_UsingSkillBaseInfo.Id == (int)SKILLBASEID.XFZBASEID)
				{//
					if (null != GameManager.gameManager.AutoSearch &&
					    GameManager.gameManager.AutoSearch.IsAutoSearching)
					{
                        //停止自动寻路
						//GameManager.gameManager.AutoSearch.Stop();
					}
					
				}
			}
			if (m_SkillSender.IsMoving&&((m_UsingSkillBaseInfo.SkillClass&(int)SKILLCLASS.CHONGFENG)== 0))//可以攻击了且在移动 停止移动
			{
                m_SkillSender.StopMove();
			}
            
            //!!!使用的是旋风则 屏蔽旋转
            //if (m_UsingSkillBaseInfo.Id ==(int)SKILLBASEID.XFZBASEID)
            //{
            //    m_SkillSender.EnableMovingRotation(false);
            //}
            //如果有目标 朝向目标
		    //Obj_Character _targetObjCharacter = Singleton<ObjManager>.GetInstance().FindObjCharacterInScene(targetId);
		    //if (_targetObjCharacter && _targetObjCharacter.ServerID !=m_SkillSender.ServerID)
		    //{
      //          m_SkillSender.FaceTo(_targetObjCharacter.Position);
		    //}
			//===屏蔽前端冲锋动作
//			if (m_SkillSender.CheckChongFeng (skillId)&&_targetObjCharacter)
//			{
//				float dis=Vector3.Distance(Sender.Position,_targetObjCharacter.Position);
//				if(dis>3.0f)
//				{
//				m_SkillSender.EndAttckCF();
//
//				m_SkillSender.AttackCF (_targetObjCharacter);
//				}
//			
//			}

//			if(m_UsingSkillExInfo!=null)
//			{
//				if (BaseSkillId == 207) 
//				{
//					Sender.AttackYS ();
//				}
//
//
//			}
			//if(Sender.ObjType==GameDefine_Globe.OBJ_TYPE.OBJ_MAIN_PLAYER)
            //开始播放动画
		    PlayAnimation(m_UsingSkillExInfo.SatrtMotionId);
			m_bIsUsingSkill = true;

			if(m_UsingSkillExInfo!=null)
			{

				if (BaseSkillId == 204) 
				{
					//Sender.AttackXS ();
				}
				
			}
            //子弹播放
//		    int nBulletNum = m_UsingSkillExInfo.getBulletEffectIDCount();
//		    for (int i = 0; i < nBulletNum; i++)
//		    {
//		        int _nBulletId = m_UsingSkillExInfo.GetBulletEffectIDbyIndex(i);
//                if (_nBulletId!=-1)
//                {
//                    if (_nBulletId == 93 || _nBulletId == 94 || _nBulletId == 95) //大理子弹 特殊处理下
//                    {
//                        m_SkillSender.PlayEffect(_nBulletId);  
//                    }
//                    else
//                    {
//                        BulletData bulletData = new BulletData(senderId, targetId);
//                        m_SkillSender.PlayEffect(_nBulletId, OnLoadBullet, bulletData);
//                    }
//                }
//		    }
            //显示技能名字
            if (m_UsingSkillBaseInfo.IsShowSkillName == 1 && m_SkillSender.ObjType != GameDefine_Globe.OBJ_TYPE.OBJ_OTHER_PLAYER)
            {
                if (skillname == "")
                {
					if (m_SkillSender.ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_MAIN_PLAYER||m_SkillSender.ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_ZOMBIE_PLAYER)
                    {
                        m_SkillSender.ShowDamageBoard_SkillName(GameDefine_Globe.DAMAGEBOARD_TYPE.SKILL_NAME, m_UsingSkillBaseInfo.Icon);
                    }
					else if (m_SkillSender.ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_NPC||m_SkillSender.ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_FELLOW)
                    {
                        m_SkillSender.ShowDamageBoard_SkillName(GameDefine_Globe.DAMAGEBOARD_TYPE.SKILL_NAME_NPC, m_UsingSkillBaseInfo.Icon, true);
                    }
                    else
                    {
						if(GameManager.gameManager.RunningScene==17)//群雄逐鹿场景下特殊处理下
							//m_SkillSender.ShowDamageBoard_SkillName(GameDefine_Globe.DAMAGEBOARD_TYPE.SKILL_NAME, m_UsingSkillBaseInfo.Icon, true);
						else
                        //m_SkillSender.ShowDamageBoard_SkillName(GameDefine_Globe.DAMAGEBOARD_TYPE.SKILL_NAME, m_UsingSkillBaseInfo.Icon, false);
                    }
                }
                else
                {
                   // m_SkillSender.ShowDamageBoard_SkillName(GameDefine_Globe.DAMAGEBOARD_TYPE.SKILL_NAME, m_UsingSkillBaseInfo.Icon, false);
                }
            }
            //主角的一些的 特殊处理
   //         //if (m_SkillSender.ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_MAIN_PLAYER)
			//{
			//    Obj_MainPlayer _mainPlayer = Singleton<ObjManager>.GetInstance().MainPlayer;
			//    if (_mainPlayer)
			//    {
			//		if ((m_UsingSkillBaseInfo.SkillClass&(int)SKILLCLASS.AUTOREPEAT)!=0)
			//		{
			//			GameManager.gameManager.PlayerDataPool.Usingskill=2;
			//		}
   //                  // 使用XP技能，如果有新手指引就 关掉
   //                 if ((m_UsingSkillBaseInfo.SkillClass & (int)SKILLCLASS.XP) != 0
   //                     && SkillBarLogic.Instance() 
   //                     && SkillBarLogic.Instance().NewPlayerGuide_Step == 1)
   //                 {
   //                     NewPlayerGuidLogic.CloseWindow();
   //                 }
			//        int nSkillIndx = _mainPlayer.GetSkillIndexById(skillId);
   //                 if (nSkillIndx >=0 && nSkillIndx <_mainPlayer.OwnSkillInfo.Length)
			//        {
   //                     Tab_CoolDownTime _coolDownTime = TableManager.GetCoolDownTimeByID(m_UsingSkillExInfo.CDTimeId, 0);
   //                     //吟唱技不在这里加CD 吟唱技能生效后才走CD 服务器同步过来 
   //                     if (_coolDownTime != null&& m_UsingSkillBaseInfo.UseType !=(int)SKILLUSETYPE.YINCHANG)
   //                     {
   //                         _mainPlayer.OwnSkillInfo[nSkillIndx].CDTime = _coolDownTime.CDTime;
   //                     }  
   //                     //非连续技 增加公共CD
			//            int nPublicSkillCDId = (int) SKILLDEFINE.PUBLICCDID;
   //                     Tab_CoolDownTime _publicCDTime = TableManager.GetCoolDownTimeByID(nPublicSkillCDId, 0);
   //                     if (_publicCDTime != null && (m_UsingSkillBaseInfo.SkillClass&(int)SKILLCLASS.AUTOREPEAT)==0)
   //                     {
   //                        _mainPlayer.SkillPublicTime = _publicCDTime.CDTime;
			//            }
			//        }
   //                 if ( CanShowSkillProgress(skillId) )
   //                 {
   //                     //如果是吟唱技则 显示引导条
   //                     if (m_UsingSkillBaseInfo.UseType == (int)SKILLUSETYPE.YINCHANG)
   //                     {
   //                         //吟唱时间为当前动作的长度
   //                         float fYinChangTime = m_UsingSkillExInfo.YinChangTime / 1000.0f;//转换成秒
   //                         //设置持续时间
   //                         if (SkillProgressLogic.Instance() != null)
   //                         {
   //                             SkillProgressLogic.Instance().PlaySkillProgress(SkillProgressLogic.ProgressModel.ORDERMODEL, fYinChangTime);
   //                         }
   //                     }
   //                     else if ((m_UsingSkillBaseInfo.SkillClass & (int)SKILLCLASS.DUTIAO) != 0)
   //                     {
   //                         //技能长度为当前引导条的长度
   //                         float fContiueTime = m_UsingSkillExInfo.SkillContinueTime / 1000.0f;//转换成秒
   //                         //设置持续时间
   //                         if (SkillProgressLogic.Instance() != null)
   //                         {
   //                             SkillProgressLogic.Instance().PlaySkillProgress(SkillProgressLogic.ProgressModel.REVERSEDMODE, fContiueTime);
   //                         }
   //                     }
   //                 }
   //                 if (m_SkillSender.Controller != null && m_SkillSender.Controller.CombatFlag)
   //                 {
   //                     _mainPlayer.AutoComabat = true;
   //                 }
                   
			//    }
			//}
            //npc和主角自身放技能 可以自带震屏效果
		    if (m_SkillSender.ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_MAIN_PLAYER ||
                m_SkillSender.ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_NPC)
		    {
                //摄像机的 操作
                CameraOpt();
		    }
           
            //做记录
			m_nLastSkillId =skillId;
			m_nLastSendId = senderId;
			m_nLastTargetId = targetId;
		
		}

        bool CanShowSkillProgress(int skillId)
        {
            if (skillId == GlobeVar.MARRY_SKILL_1 ||
                 skillId == GlobeVar.MARRY_SKILL_2 ||
                 skillId == GlobeVar.MARRY_SKILL_3)
            {
                return false;
            }
            return true;
        }

        void OnLoadBullet(GameObject bulletEffect, object param)
        {
            BulletData curData = param as BulletData;
            if (bulletEffect != null && curData != null)
            {
                Bullet _bullet = bulletEffect.GetComponent<Bullet>();
                if (_bullet != null)
                {
                    _bullet.InitData(curData._selfID, curData._targetID);
                }
            }
        }
       
        /// <summary>
        /// 播放动画
        /// </summary>
        /// <param name="AnimationId"></param>
        void PlayAnimation(int AnimationId)
		{
			//if (m_SkillSender != null && m_SkillSender.AnimLogic != null && m_SkillSender.Objanimation !=null)
			//{
   //             //保证技能动作能顺利播放
   //             if (m_SkillSender.ObjType != GameDefine_Globe.OBJ_TYPE.OBJ_NPC && m_SkillSender.Objanimation.isPlaying)
			//    {
   //                 m_SkillSender.AnimLogic.Stop();
			//    }
   //             //其他玩家放陷阱技能不播发动作
   //             if (m_UsingSkillBaseInfo !=null &&
   //                 m_UsingSkillBaseInfo.Id ==(int)SKILLBASEID.HDWLID &&
   //                 m_SkillSender.ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_OTHER_PLAYER)
			//    {
   //                return;
			//    }

                m_SkillSender.AnimLogic.Play(AnimationId);
			//}
		}

        /// <summary>
        /// 打断动作等等
        /// </summary>
		public void BreakCurSkill()
		{
            //打断当前技能动作
            if (m_nLastSkillId == -1 || m_bIsUsingSkill ==false)
            {
                return;
            }
		    if (m_UsingSkillBaseInfo ==null || m_UsingSkillExInfo ==null || m_SkillSender ==null)
		    {
		        return;
		    }
			if (m_UsingSkillBaseInfo != null && ((m_UsingSkillBaseInfo.SkillClass & (int)SKILLCLASS.CHONGFENG) != 0)) 
			{
				m_SkillSender.m_bIsCanCF=false;
			}
     	//	Tab_SkillEx  skillex=TableManager.GetSkillExByID(SkillSender.CurUseSkillId,0);
//			if(skillex==null)
//			{
//				return;
//			}
//			Tab_SkillBase skillbase=TableManager.GetSkillBaseByID(skillex.BaseId,0);
			if(m_UsingSkillBaseInfo!=null)//打断隐身
			{
				if (m_UsingSkillBaseInfo.Id == 207) 
				{
					m_SkillSender.m_fXSLastTime=0;
				}
			}
            if (m_SkillSender.AnimLogic != null)
		    {
                if (m_SkillSender.AnimLogic.CurAnimData != null)
                {
                    Tab_Animation _CurAnimInfo = TableManager.GetAnimationByID(m_SkillSender.AnimLogic.CurAnimData.AnimID, 0);
                    if (_CurAnimInfo != null)
                    {
                        //技能被打断了 停止当前技能动作的音效
                        if (_CurAnimInfo.SoundID >= 0 && null != GameManager.gameManager.SoundManager)
                        {
                            GameManager.gameManager.SoundManager.StopSoundEffect(_CurAnimInfo.SoundID);
                        }
                    }
                }
                m_SkillSender.AnimLogic.Stop();
				//===如果技能被打断则进入战斗站立动作

				m_SkillSender.AnimLogic.Play((int)(CharacterDefine.CharacterAnimId.ActStand));
		    }
			//if(m_UsingSkillBaseInfo)
            //打断震屏
            if (m_SkillSender.ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_MAIN_PLAYER)
		    {
                //如果 该技能还有震屏且还在震屏 则停止震屏
                if (m_UsingSkillExInfo.CameraRockId != -1)
                {
                    CameraController camController = Singleton<ObjManager>.GetInstance().MainPlayer.GetComponent<CameraController>();
                    if (camController != null && camController.IsHaveRockInfoById(m_UsingSkillExInfo.CameraRockId))
                    {
                        camController.CleanUpRockInfoById(m_UsingSkillExInfo.CameraRockId);
                    }
                }
                //如果是吟唱技能 则打断读条
                if (m_UsingSkillBaseInfo.UseType == (int)SKILLUSETYPE.YINCHANG)
                {
                    if (SkillProgressLogic.Instance() != null)
                    {
                        SkillProgressLogic.Instance().CloseWindow();
                    }
                }
                //读条技能 打断读条
                if ((m_UsingSkillBaseInfo.SkillClass & (int)SKILLCLASS.DUTIAO) != 0)
		        {
                    if (SkillProgressLogic.Instance() != null)
                    {
                        SkillProgressLogic.Instance().CloseWindow();
                    }
		        }
		    }
            //打断全屏特效
            if (m_UsingSkillExInfo.SceneEffectId != -1)
            {
                if (BackCamerControll.Instance() != null && BackCamerControll.Instance().SceneEffecLogic!=null)
                {
                    BackCamerControll.Instance().SceneEffecLogic.StopEffect(m_UsingSkillExInfo.SceneEffectId);
                }
            }
            //打断播放的子弹
            int nBulletNum = m_UsingSkillExInfo.getBulletEffectIDCount();
            for (int i = 0; i < nBulletNum; i++)
            {
                int _nBulletId = m_UsingSkillExInfo.GetBulletEffectIDbyIndex(i);
                if (_nBulletId != -1 && m_SkillSender.ObjEffectLogic !=null)
                {
                    m_SkillSender.ObjEffectLogic.StopEffect(_nBulletId);
                }
            }
              SkillFinsh();
		}

		private bool isPlayer;
	    public void SkillFinsh()
	    {
            if (m_nLastSkillId == -1 || m_bIsUsingSkill == false)
            {
                return;
            }
			if (m_SkillSender.ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_MAIN_PLAYER)
			{
				isPlayer = true;
			}else{
				isPlayer = false;
			}

			//===========
			if (m_SkillSender.ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_MAIN_PLAYER) {
								Obj_MainPlayer objmain = (Obj_MainPlayer)m_SkillSender;

					GameManager.gameManager.PlayerDataPool.Usingskill =0;
			
			
//								if (SkillBarLogic.Instance () != null && SkillBarLogic.Instance ().ispress == false && isPlayer == true && (!objmain.AutoComabat)) {
//										Tab_SkillEx se = TableManager.GetSkillExByID (SkillBarLogic.Instance ().nextSkillId, 0);
//										if (se != null && se.ReturnActID != -1)
//												PlayAnimation (se.ReturnActID);
//								}
						}
			//===========

            //清除数据
            m_bIsUsingSkill = false;
			//Debug.Log("using skil "+m_bIsUsingSkill);
            m_fLastUseSkillTime = 0;
			m_nLastSkillId = -1;
			m_SkillSender.CurUseSkillId = -1;
	        if (m_UsingSkillBaseInfo ==null || m_UsingSkillExInfo ==null || m_SkillSender ==null)
	        {
	            return;
	        }

            m_SkillSender.EnableMovingRotation(true);

            if (m_SkillSender.ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_MAIN_PLAYER ||
                m_SkillSender.ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_OTHER_PLAYER)
            {
                Obj_OtherPlayer player = m_SkillSender as Obj_OtherPlayer;
                if (null != player)
                    player.UpdateVisualAfterSkill();
            }

            //XP 阳关三叠 放完 恢复之前的视角
            if ((m_UsingSkillBaseInfo.SkillClass & (int)SKILLCLASS.XP) != 0 ||
                 m_UsingSkillBaseInfo.Id == (int)SKILLBASEID.YGSDID)
            {
                CameraController camController = Singleton<ObjManager>.GetInstance().MainPlayer.GetComponent<CameraController>();
                if (camController != null)
                {
                    if (m_OldCameraScan !=-1)
                    {
                        camController.m_Scale = m_OldCameraScan;
                    }
                }
            }
            //如果需要在技能结束的时候 停止对应的特效 则停止
            if (m_UsingSkillBaseInfo.IsNeedStopEffectId !=-1)
	        {
                m_SkillSender.StopEffect(m_UsingSkillBaseInfo.IsNeedStopEffectId);
	        }

			int _rangeEffectType = m_UsingSkillExInfo.RangeEffectType;
			if ((m_SkillSender.ObjType ==GameDefine_Globe.OBJ_TYPE.OBJ_NPC))
			{
				if(_rangeEffectType!=-1)
				{
					m_SkillSender.StopSkillRangeEffect(m_UsingSkillExInfo.SkillExID);
				}
				
				
			}
            //清除数据
	        m_UsingSkillBaseInfo = null;
	        m_UsingSkillExInfo = null;
	        m_SkillSender = null;
//			//========
//			Obj_MainPlayer _mainPlayer = Singleton<ObjManager>.GetInstance().MainPlayer;
//			if (_mainPlayer != null && isPlayer == true && !m_bIsUsingSkill && SkillBarLogic.Instance().ispress==true)
//			{
//				Tab_SkillEx se = TableManager.GetSkillExByID(SkillBarLogic.Instance().nextSkillId,0);
//				if(se!=null)
//				{
//					if(se.CheckTime != -1)
//					{
//						_mainPlayer.UseSkillOpt(se.ContinuityID, null);
//						SkillBarLogic.Instance().nextSkillId = se.ContinuityID;
//						SkillBarLogic.Instance ().LJtime = 0;
////						SkillBarLogic.Instance ().inReturnAct = false;
////						SkillBarLogic.Instance ().returnActTime = 9999f;
//					}
//				}
//			}
//			else if (_mainPlayer != null && isPlayer == true && !m_bIsUsingSkill && SkillBarLogic.Instance().ispress==false)
//			{
//				Tab_SkillEx se = TableManager.GetSkillExByID(SkillBarLogic.Instance().nextSkillId,0);
//				if(se!=null)
//				{
//					if(se.CheckTime != -1)
//					{
//						SkillBarLogic.Instance ().inReturnAct = true;
//						SkillBarLogic.Instance ().returnActTime = 0f;
//					}
//				}
//			}
			//======
	    }
		//控制摄像头 createInstance
		void CameraOpt()
		{
            if (m_UsingSkillExInfo ==null || m_UsingSkillExInfo ==null)
		    {
		        return;
		    }
            //震屏
            if (m_UsingSkillExInfo.CameraRockId != -1)
		    {
		        int nRandNum = Random.Range(0, 100);
                if (m_UsingSkillExInfo.CameraRockRate >= nRandNum)
		        {
                    CameraController camController = Singleton<ObjManager>.GetInstance().MainPlayer.GetComponent<CameraController>();
                    if ( camController!=null)
		            {
                        camController.InitCameraRock(m_UsingSkillExInfo.CameraRockId);
		            }
		        }
		    }
            //是否需要播放全屏特效
            if (m_UsingSkillExInfo.SceneEffectId != -1)
            {
                if (BackCamerControll.Instance()!=null)
                {
                    BackCamerControll.Instance().PlaySceneEffect(m_UsingSkillExInfo.SceneEffectId);
                }
            }
            //XP技能拉高摄像机视角
		    if ((m_UsingSkillBaseInfo.SkillClass&(int)SKILLCLASS.XP)!=0 ||
                m_UsingSkillBaseInfo.Id ==(int)SKILLBASEID.YGSDID) //阳关三叠 也做这个处理
		    {
                CameraController camController = Singleton<ObjManager>.GetInstance().MainPlayer.GetComponent<CameraController>();
                if (camController != null)
                {
                    m_OldCameraScan = camController.m_Scale;
                    camController.m_Scale =1.5f;
                }
		    }
		}

        //显示技能名字
        public static void ShowSkillName(int skillId, int senderId, string skillName = "")
        {
            Obj_Character skillSender = Singleton<ObjManager>.GetInstance().FindObjCharacterInScene(senderId);
            if (skillSender == null)
            {
                LogModule.DebugLog("MainPlayer is Null:" + senderId);
                return;
            }
            Tab_SkillEx skillExInfo = TableManager.GetSkillExByID(skillId, 0);
            if (skillExInfo == null)
            {
                LogModule.DebugLog("SkillExinfo is Null: " + skillId);
                return;
            }
            int BaseSkillId = skillExInfo.BaseId;
            //读表
            //Tab_SkillBase skillBase = TableManager.GetSkillBaseByID(BaseSkillId, 0);
            if (skillBase == null)
            {
                LogModule.DebugLog("SkillBaseInfo is Null" + BaseSkillId);
                return;
            }

            //显示技能名字
            if (skillName == "")
            {
                if (skillSender.ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_MAIN_PLAYER ||
                    skillSender.ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_OTHER_PLAYER)
                {
                    skillSender.ShowDamageBoard_SkillName(GameDefine_Globe.DAMAGEBOARD_TYPE.SKILL_NAME, skillBase.Icon);
                }
                else if (skillSender.ObjType == GameDefine_Globe.OBJ_TYPE.OBJ_NPC)
                {
                    skillSender.ShowDamageBoard_SkillName(GameDefine_Globe.DAMAGEBOARD_TYPE.SKILL_NAME_NPC, skillBase.Icon, false);
                }
                else
                {
                    skillSender.ShowDamageBoard_SkillName(GameDefine_Globe.DAMAGEBOARD_TYPE.SKILL_NAME, skillBase.Icon, false);
                }
            }
            else
            {
                skillSender.ShowDamageBoard_SkillName(GameDefine_Globe.DAMAGEBOARD_TYPE.SKILL_NAME, skillBase.Icon, false);
            }
        }
	}
}
