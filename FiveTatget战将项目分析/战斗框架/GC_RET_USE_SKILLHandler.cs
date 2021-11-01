//This code create by CodeEngine

using System;
using System.Runtime.Remoting.Lifetime;
using Games.GlobeDefine;
using Games.LogicObj;
using Games.SkillModle;
using GCGame.Table;
using Module.Log;
 using Google.ProtocolBuffers;
 using System.Collections;
using UnityEngine;
namespace SPacket.SocketInstance
 {
    public class GC_RET_USE_SKILLHandler : Ipacket
    {
        public uint Execute(PacketDistributed ipacket)
        {
            //解出来pb消息 获得技能ID 释放者ID 目标ID
            //GC_RET_USE_SKILL packet = (GC_RET_USE_SKILL )ipacket;
            // if (null == packet) return (uint)PACKET_EXE.PACKET_EXE_ERROR;
            // //enter your logic
            //int nSkillId = packet.SkillId;
            //int nSenderId = packet.SenderId;
            //int nTargetID = packet.TargetId;

            //获得主角 方便调用技能核心的播放技能的方法
//            Obj_Character Sender = Singleton<ObjManager>.GetInstance().FindObjCharacterInScene(nSenderId);
//			if (Sender ==null)
//			{
//				return (uint)PACKET_EXE.PACKET_EXE_CONTINUE;
               
//            }
//            if (Sender.SkillCore ==null)
//            {
//                return (uint)PACKET_EXE.PACKET_EXE_CONTINUE;
//            }
//            int nFailType = (int) SKILLUSEFAILTYPE.INVALID;
//            if (packet.HasSkillfailType)
//            {
//                nFailType = packet.SkillfailType;
//            }
//			Tab_SkillEx  skillex=TableManager.GetSkillExByID(nSkillId,0);
//			if(skillex==null)
//			{
//				return (uint)PACKET_EXE.PACKET_EXE_CONTINUE;
//			}
////			if(skillex.CheckTime!=-1&&(Sender.ObjType ==GameDefine_Globe.OBJ_TYPE.OBJ_MAIN_PLAYER))
////			{
////				return (uint)PACKET_EXE.PACKET_EXE_CONTINUE;
////			}
//			if (GameManager.gameManager.RunningScene == 13)//新手教程地图里放完xp特效后消失
//			{
//				Tab_SkillBase skillbase=TableManager.GetSkillBaseByID(skillex.BaseId,0);
//				if((skillbase.SkillClass & (int)SKILLCLASS.XP)!= 0 )
//				{
//					SkillBarLogic.Instance().PlayXPActiveEffect(false);
//				}
//			}
//			string szSkillName = "";
//            if (packet.HasSkillname)
//            {
//                szSkillName = packet.Skillname;
//            }
//		//?????????
//            if (nFailType ==0)
//            {
//				GameManager.gameManager.PlayerDataPool.Usingskill=0;
//				return (uint)PACKET_EXE.PACKET_EXE_CONTINUE;
//            }
//            else
//            {
//				int _rangeEffectType = skillex.RangeEffectType;
//				if ((Sender.ObjType ==GameDefine_Globe.OBJ_TYPE.OBJ_NPC))
//				{
//					if(_rangeEffectType!=-1)
//					{
//						Sender.PlaySkillRangeEffect(nSkillId);
//					}
//				}
//                if (szSkillName != "")
//                {
//                    Sender.SkillCore.UseSkill(nSkillId, nSenderId, nTargetID, szSkillName);
//					Debug.Log("ffff"+nTargetID);
//                }
//                else
//                {
//                    Sender.SkillCore.UseSkill(nSkillId, nSenderId, nTargetID);
//					Debug.Log("ffff"+nTargetID);
//                }
               
//            }
//            return (uint)PACKET_EXE.PACKET_EXE_CONTINUE;
        }
    }
 }
