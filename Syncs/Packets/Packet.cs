using System;
using m2d;
using nel;
using Newtonsoft.Json.Linq;
using UnityEngine;
using XX;

namespace Kaleidoscopic.Syncs.Packets;

// no record now.
[Serializable]
public abstract class Packet {
    public string clazz => this.GetType().Name;
};

[Serializable]
public abstract class ClientBoundPacket : Packet {
    public long token => whoami;

    public static readonly long whoami = random.Next() | (long)random.Next() << 32;
}

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
    public float hp, hpmax;
    public float mp, mpmax;
    public int curMgKindInt;
    public int aimInt;
    public float curMgReduceMp;
    public float skillMpHold;
}

public class EnemyInfo {
    public int frameIndex;
    public int sequenceAim;
    public string poseTitle;
    public string characterTitle;
    public float x, y;
    public float ax, ay;
    public float vx, vy;
    public float scaleX, scaleY;
    public float rotationR;
    public string name;
    public string key;
    public float hp, hpmax;
    public float mp, mpmax;
    public int aimInt;
}

/*
 * 客户端玩家每次更新都向服务端发送这玩意。
 */
[Serializable]
public class ClientBoundPlayerSyncPacket : ClientBoundPacket {
    public PlayerInfo info;
}

[Serializable]
public class ClientBoundEnemySyncPacket : ClientBoundPacket {
    public EnemyInfo[] enemyInfos;
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

[Serializable]
public class ServerBoundOtherEnemiesSyncPacket : ServerBoundPacket {
    public EnemyInfo[] otherEnemies;
    public long[] remoteTokens;
    public override void Process() {
        SyncPatcherEnemies.enemyInfos = otherEnemies;
        // GunmuHandler.recreateGunmu(remoteTokens, otherEnemies);
    }
}

[Serializable]
public class ClientBoundDamagePacket : ClientBoundPacket {
    public long targetToken;
    public string targetKey;
    public int damage;
    public int attrInt;
    public float knockback_len;
    public float knockback_ratio_p;
    public float knockback_ratio_t;
    public bool _apply_knockback_current;
    public int mgkindInt;
    public int mghitInt;
    public bool casterIsPlayer;
}

[Serializable]
public class ServerBoundDamagePacket : ServerBoundPacket {
    public ClientBoundDamagePacket original;
    public override void Process() {
        // do something...
        info("found process on", this.original.targetToken, this.original.targetKey, this.original.damage);
        var pr = SceneGame.M2D?.PlayerNoel;
        var mp = pr?.Mp;
        if (mp == null) return;
        var mv = mp.getMoverByName(this.original.targetKey) as M2Attackable;
        if (mv == null) return;
        NelAttackInfo atk = new NelAttackInfo();
        atk.attr = (MGATTR)this.original.attrInt;
        atk.knockback_len = this.original.knockback_len;
        atk.knockback_ratio_p = this.original.knockback_ratio_p;
        atk.knockback_ratio_t = this.original.knockback_ratio_t;
        atk._apply_knockback_current = this.original._apply_knockback_current;
        atk.Caster = pr;
        if (this.original.mgkindInt != 0) {
            MagicItem mg = new MagicItem(SceneGame.M2D.MGC);
            mg.init(random.Next(), pr, (MGKIND)this.original.mgkindInt, (MGHIT)this.original.mghitInt);
            atk.PublishMagic = mg;
        }
        int good = mv.applyHpDamage(this.original.damage, true, atk);
        mp.DmgCntCon.Make(mv, -good, 0, M2DmgCounterItem.DC.NORMAL, false); // 弹伤害数字
        if (atk.PublishMagic != null) atk.PublishMagic.kill();
    }
}

[Serializable]
public class ServerBoundPingPacket : ServerBoundPacket {
    public long time;
    public override void Process() {
        Dispatcher.send(new ClientBoundPongPacket() {
            time = this.time,
        });
    }
}

[Serializable]
public class ClientBoundPongPacket : ClientBoundPacket {
    public long time;
}

[Serializable]
public class ClientBoundDmgCounterPacket : ClientBoundPacket {
    public bool isPlayer;
    public float x, y;
    public int dcInt;
    public int damage, mpDamage;
}

[Serializable]
public class ServerBoundDmgCounterPacket : ServerBoundPacket {
    public ClientBoundDmgCounterPacket original;
    public override void Process() {
        MakeHandler.Process(this);
    }
}

[Serializable]
public class ClientBoundBatchedPacket : ClientBoundPacket {
    public ClientBoundPacket[] payloads;
}

[Serializable]
public class ClientBoundProtectedModePacket : ClientBoundPacket {
    public string payload; // GZIP + Base64 后的字符串
}

[Serializable]
public class ServerBoundBatchedPacket : ServerBoundPacket {
    public JObject[] payloads; 

    public override void Process() {
        // 解开合包，塞回队列重新派发
        if (payloads == null) return;
        foreach (var payloadObj in payloads) {
            // 将子包再次丢给调度器处理
            Dispatcher.received(payloadObj.ToString());
        }
    }
}

[Serializable]
public class ServerBoundProtectedModePacket : ServerBoundPacket {
    public string payload; 

    public override void Process() {
        if (string.IsNullOrEmpty(payload)) return;

        try {
            // 1. Base64 还原
            byte[] compressedBytes = Convert.FromBase64String(payload);

            // 2. GZIP 解压
            string rawJson;
            using (var ms = new System.IO.MemoryStream(compressedBytes))
            using (var gzip = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Decompress))
            using (var reader = new System.IO.StreamReader(gzip, System.Text.Encoding.UTF8)) {
                rawJson = reader.ReadToEnd();
            }

            // 3. 解压出来的必定是一个 ServerBoundBatchedPacket 或者是 JArray (这取决于你 Java 端发的是啥)
            // 根据你在 Java 端 `flushSockets` 里的逻辑，你发的是一个 List<Packet>，
            // 也就是它解压出来是一个 JSON 数组 (JArray)。
            var jsonArray = JArray.Parse(rawJson);
            foreach (var token in jsonArray) {
                // 将原生的 JSON 字符串重新喂给接收器
                Dispatcher.received(token.ToString());
            }
            
        } catch (Exception e) {
            error($"保护模式解压失败: {e.Message}");
        }
    }
}

public class BoundingBox {
    public float width, height;
    public string key;
    public long token;
    // how can this even happen.
    public float mapX, mapY;
    public float[] x;
    public float[] y;
}

[Serializable]
public class ClientBoundBboxPacket : ClientBoundPacket {
    public BoundingBox[] bboxes;
}

[Serializable]
public class ServerBoundBboxPacket : ServerBoundPacket {
    public BoundingBox[] bboxes;
    public override void Process() {
        GunmuHandler.recreateGunmu(bboxes);
    }
}