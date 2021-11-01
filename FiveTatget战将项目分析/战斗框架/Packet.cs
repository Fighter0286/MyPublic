/********************************************************************************
 *	文件名：Packet.cs
 *	全路径：	\NetWork\SocketAPI\Packet.cs
 *	创建人：	张旭辉
 *	创建时间：2013-11-29
 *
 *	功能说明： 消息包接口类
 *	       
 *	修改记录：
*********************************************************************************/

using System;

namespace SPacket.SocketInstance
{
    public enum PACKET_EXE //数据包程序
    {
        PACKET_EXE_ERROR = 0,  //错误
        PACKET_EXE_BREAK,  //终止
        PACKET_EXE_CONTINUE,  //继续
        PACKET_EXE_NOTREMOVE,  //我的的移动
        PACKET_EXE_NOTREMOVE_ERROR,  //我的的移动错误
    }

    public interface Ipacket  //接口 用于继承
    {
	    uint	Execute( PacketDistributed packet )  ;      
    }
}
