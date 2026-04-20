using System;
using Kaleidoscopic.Core;
using Kaleidoscopic.Syncs.Packets;
using m2d;
using nel;
using UnityEngine;
using XX;

namespace Kaleidoscopic.Syncs;

public class M2Gunmu : M2Attackable {
    public long remoteToken;
    public string remoteEnemyKey;

    public void appear(Map2d _Mp) {
        _Mp.assignMover(this, true);
    }
    public override void runPre() {
        base.runPre();
    }
    public void setValues(long token, string key, float mapX, float mapY, float width, float height) {
        if(this.getColliderCreator() == null)
            this.SwitchColliderCreator(new M2MvColliderCreatorAtk(this));
        // this.getColliderCreator().fineRecreate();
        this.remoteToken = token;
        this.remoteEnemyKey = key;
        base.gameObject.isStatic = false;
        this.maxhp = 2000;
        this.maxmp = 2000;
        this.moveBy(mapX - this.x, mapY - this.y);
        base.Size(width, height);
    }

    public void moveTo(float x, float y) {
        this.moveBy(x - this.x, y - this.y);
    }

    public void doCollider(BoundingBox bbox) {
        if (bbox == null) return;
        var myCollider = this.getColliderCreator().Cld;
        

        // base.Size(bbox.width, bbox.height);
        // myCollider.offset = new Vector2(info.collider.offset.x, info.collider.offset.y);
        //
        // this.moveTo(bbox.mapX, bbox.mapY);
        if (bbox.x == null || bbox.y == null || bbox.width == 0 || bbox.height == 0) return;
        int n = Math.Min(bbox.x.Length, bbox.y.Length);
        Vector2[] localPts = new Vector2[n];
        for (int i = 0; i < n; i++) {
            // info("loading x y", bbox.x[i], bbox.y[i]);
            localPts[i] = new Vector2(bbox.x[i], bbox.y[i]);
        }
        myCollider.points = localPts;
        // myCollider.SetPath(0, localPts);
    }

    public override RAYHIT can_hit(M2Ray Ray) {
        return (RAYHIT)3; // 永远允许本地射线扫到它
    }

    // 【核心改造】受击逻辑：只做本地视觉反馈，拦截真实扣血，改为发包
    public override int applyHpDamage(int val, bool force = false, AttackInfo _Atk = null) {
        if (_Atk is not NelAttackInfo atk) return 0;

        // 1. 保留无敌帧过滤，防止一瞬间发几百个伤害包把 UDP 撑爆
        if (this.NoDamage.isActive(atk.ndmg)) return 0;

        // 2. 纯本地的视觉与手感反馈
        // this.Mp.DmgCntCon.Make(this, -val, 0, M2DmgCounterItem.DC.NORMAL, false); // 弹伤害数字
        if (this.TeCon != null) {
            this.TeCon.setDmgBlinkEnemy(atk.attr, 15f, 0.8f, 0.9f, 0); // 闪烁变红
        }
        if (atk.ndmg != NDMG.DEFAULT && atk.ndmg > NDMG.NORMAL) {
            this.NoDamage.Add(atk.ndmg, (float)atk.nodamage_time);
        }

        MGATTR attr = atk.attr;
        float knockback_len = atk.knockback_len;
        float knockback_ratio_p = atk.knockback_ratio_p;
        float knockback_ratio_t = atk.knockback_ratio_t;
        bool _apply_knockback_current = atk._apply_knockback_current;
        MagicItem mg = atk.PublishMagic;
        MGKIND mgkind = mg?.kind ?? MGKIND.NONE;
        bool casterIsPlayer = atk.Caster is PR;
        if (!casterIsPlayer) return 0;
        if (!GeneralConfigs.multiAllowPVP.Value && this.remoteEnemyKey.ToLower().Contains("noel")) {
            return 1;
        }
        Dispatcher.send(new ClientBoundDamagePacket {
            targetToken = this.remoteToken,
            targetKey = this.remoteEnemyKey,
            damage = val,
            attrInt = (int)atk.attr,
            knockback_len = atk.knockback_len,
            knockback_ratio_p = atk.knockback_ratio_p,
            knockback_ratio_t = atk.knockback_ratio_t,
            _apply_knockback_current = atk._apply_knockback_current,
            mgkindInt = (int)mgkind,
            mghitInt = (int)(atk.PublishMagic?.hittype ?? 0),
            casterIsPlayer = casterIsPlayer,
        });

        return 1; // 告诉本地引擎：打中了！
    }

    public override void fineHittingLayer() {
        base.gameObject.layer = 2; // 确保在敌人层，原版 CircleCast 才能扫到
    }

    public override bool isDamagingOrKo() => false;

    public override float auto_target_priority(M2Mover CalcFrom) => 4f; // 保持高仇恨，确保本地玩家的自瞄追踪法术能锁到它

    public override HITTYPE getHitType(M2Ray Ray) => HITTYPE.EN | HITTYPE.BREAK;
}