using System;

namespace Kaleidoscopic.Syncs.Packets;

// no record now.
[Serializable]
public abstract class Packet {
    public string clazz => this.GetType().Name;
};

[Serializable]
public abstract class ClientBoundPacket : Packet;

[Serializable]
public abstract class ServerBoundPacket : Packet {
    public abstract void Process();
}

/*
 * 客户端总是会向服务端不停发送这玩意，包含 vlan（当前地图的哈希），
 * 以便服务端发送玩家同步包。
 * vlan 随时可能变更。
 */
[Serializable]
public class ClientBoundVlanChangedPacket : ClientBoundPacket {
    public int vlan;
}

public class PlayerInfo {
    public string playerName;
    public float x, y;
    public float ax, ay;
    public float vx, vy;
    public int frameIndex;
    public int sequenceAim;
    public string poseTitle;
    public string characterTitle;
    public string caneName;
}

/*
 * 客户端玩家每次更新都向服务端发送这玩意。
 */
[Serializable]
public class ClientBoundPlayerSyncPacket : ClientBoundPacket {
    public PlayerInfo info;
}

/*
 * 服务端可以以 60Hz 的速度向客户端发这玩意。
 * 但是需要注意，服务端只应当发送同 vlan 下的玩家。
 * 此外，玩家下线后会被从所有 vlan 踢出。
 */
[Serializable]
public class ServerBoundOtherPlayersSyncPacket : ServerBoundPacket {
    public PlayerInfo[] otherPlayers;
    public override void Process() {
        MultiHandler.otherPlayers = otherPlayers;
    }
}