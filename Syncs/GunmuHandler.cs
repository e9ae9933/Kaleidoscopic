using System;
using System.Collections.Generic;
using Kaleidoscopic.Syncs.Packets;
using UnityEngine;
using m2d;
using nel;

namespace Kaleidoscopic.Syncs {
    public static class GunmuHandler {
        private static long cnt = 0;

        public static void recreateGunmu(BoundingBox[] bboxes) {
            info("recreating gunmu with size"+bboxes.Length);
            var pr = SceneGame.M2D?.getPrNoel();
            if (pr == null || pr.destructed) return;
            var mp = pr.Mp;
            // 【新增防御：地图存活校验】
            // 如果地图底层的 Gob 已经被销毁，说明正在读档/切换场景，直接丢弃该包
            if (mp == null || mp.Gob == null || pr.destructed) return;

            int n = bboxes.Length;

            // 1. 将收到的网络数据转换为字典，使用 (long, string) 作为复合键
            // 这种方式完全避免了字符串拼接的 GC 分配开销
            Dictionary<(long, string), BoundingBox> incomingData = new();
            for (int i = 0; i < n; i++) {
                if (bboxes[i] == null) continue;
                var compositeKey = ( bboxes[i].token, bboxes[i].key);
                incomingData[compositeKey] = bboxes[i];
            }

            // 2. 遍历地图现有的 M2Gunmu，比对存活状态
            Dictionary<(long, string), M2Gunmu> activeGunmus = new();
            List<M2Gunmu> toDestroy = new();

            for (int i = 0; i < mp.mover_count; i++) {
                if (mp.AMov[i] is M2Gunmu gunmu) {
                    var gunmuKey = (gunmu.remoteToken, gunmu.remoteEnemyKey);
                    
                    // 如果现有的影子的复合键不在新数据里，或者它的 Key 为空（刚生成的脏数据），准备销毁
                    if (string.IsNullOrEmpty(gunmu.remoteEnemyKey) || !incomingData.ContainsKey(gunmuKey)) {
                        toDestroy.Add(gunmu);
                    } else {
                        // 仍在存活的影子，加入活跃名单
                        activeGunmus[gunmuKey] = gunmu;
                    }
                }
            }

            // 执行删除逻辑
            foreach (M2Gunmu deadGunmu in toDestroy) {
                deadGunmu.destruct();
            }

            // 3 & 4. 生成缺失的影子，并更新所有匹配的影子
            for (int i = 0; i < n; i++) {
                if (bboxes[i] == null) continue;
                
                long currentToken = bboxes[i].token;
                string currentKey = bboxes[i].key;
                var compositeKey = (currentToken, currentKey);
                // info("checking",currentKey, currentToken);

                // 如果活跃字典里没有这个复合键，说明是新怪，需要生成
                if (!activeGunmus.TryGetValue(compositeKey, out M2Gunmu gunmu)) {
                    // info("checking2",currentKey, currentToken);
                    GameObject go = new GameObject("Gunmu_" + (++cnt));
                    gunmu = go.AddComponent<M2Gunmu>();
                    gunmu.appear(mp); 
                    
                    // 注意：由于你拆分了 appear 和 setValues，这里我们需要设定一个初始的宽和高
                    // 假设 bbox 里能提取到一个初始的宽和高，如果不能，给个默认值 1f
                    float initialWidth = bboxes[i].width;
                    float initialHeight = bboxes[i].height;
                    
                    gunmu.setValues(currentToken, currentKey, bboxes[i].mapX, bboxes[i].mapY, initialWidth, initialHeight);
                    
                    activeGunmus[compositeKey] = gunmu;
                } else {
                     // 如果影子已经存在，我们需要更新它的坐标 (或者使用你之前讨论的 Lerp 平滑插值)
                     // 因为 setValues 会重置 maxhp 等，这里建议你可以再在 M2Gunmu 里写一个单独的方法比如 UpdatePosition
                     // info("checking3",currentKey, currentToken);
                     float initialWidth = bboxes[i].width;
                     float initialHeight = bboxes[i].height;
                     gunmu.setValues(currentToken, currentKey, bboxes[i].mapX, bboxes[i].mapY, initialWidth, initialHeight);
                }

                // 4. 对该复合键对应的影子更新碰撞箱
                gunmu.doCollider(bboxes[i]);
            }
        }
    }
}