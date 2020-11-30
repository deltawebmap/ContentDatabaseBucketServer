using DeltaWebMap.ContentDatabase.CommitBuilders.Write;
using DeltaWebMap.ServerContentBucketServer.Framework;
using DeltaWebMap.ServerContentBucketServer.NetEntities.Dinos;
using DeltaWebMap.ServerContentBucketServer.ServiceTemplates.Sync.Put;
using LibDeltaSystem;
using LibDeltaSystem.Entities.CommonNet;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.ServerContentBucketServer.Services.Sync.Server.Put
{
    public class SyncServerPutBucketDinosDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/{SERVER_ID}/buckets/dinos/put";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new SyncServerPutBucketDinos(conn, e);
        }
    }

    public class SyncServerPutBucketDinos : ISyncServerPut<DinoSyncPayload>
    {
        public SyncServerPutBucketDinos(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override WriteCommitObject ConvertToCommitObject(DinoSyncPayload item)
        {
            //Read ID
            ulong dinoId = ConversionTools.GetMultipartID(item.id_1, item.id_2);

            //Make object
            var o = new WriteCommitObject(dinoId, item.tribe_id);
            o.WriteInt32("tribe_id", item.tribe_id);
            o.WriteInt64("dino_id", (long)dinoId);
            o.WriteBool("is_female", item.is_female);
            o.WriteInt32Array("colors", item.colors);
            o.WriteString("tamed_name", item.name);
            o.WriteString("tamer_name", item.tamer);
            o.WriteString("classname", ConversionTools.TrimArkClassname(item.classname));
            o.WriteFloatArray("current_stats", item.current_stats);
            o.WriteFloatArray("max_stats", item.max_stats);
            o.WriteInt32Array("base_levelups_applied", item.points_wild);
            o.WriteInt32Array("tamed_levelups_applied", item.points_tamed);
            o.WriteInt32("base_level", item.base_level);
            o.WriteInt32("level", item.base_level + item.extra_level);
            o.WriteFloat("experience", item.experience);
            o.WriteBool("is_baby", item.baby);
            o.WriteFloat("baby_age", item.baby_age);
            o.WriteDouble("next_imprint_time", item.next_cuddle);
            o.WriteFloat("imprint_quality", item.imprint_quality);
            o.WriteChildObject("location", ConversionTools.ConvertLocationToWriteObject(item.location));
            o.WriteStringEnum("status", item.status, new string[] { "PASSIVE", "NEUTRAL", "AGGRESSIVE", "PASSIVE_FLEE", "YOUR_TARGET" });
            o.WriteFloat("taming_effectiveness", 0);
            o.WriteBool("is_cryo", false);
            o.WriteInt64("last_sync_time", DateTime.UtcNow.Ticks);
            o.WriteBool("is_alive", true);
            return o;
        }

        public override int GetTeamId(DinoSyncPayload item)
        {
            return item.tribe_id;
        }

        public override object GetRpcNetType(DinoSyncPayload item, ulong commitId, byte commitType)
        {
            return new NetDino
            {
                tribe_id = item.tribe_id,
                dino_id = ConversionTools.GetMultipartID(item.id_1, item.id_2).ToString(),
                is_female = item.is_female,
                colors = item.colors,
                colors_hex = new string[6],
                tamed_name = item.name,
                tamer_name = item.tamer,
                classname = ConversionTools.TrimArkClassname(item.classname),
                current_stats = item.current_stats,
                max_stats = item.max_stats,
                base_levelups_applied = item.points_wild,
                tamed_levelups_applied = item.points_tamed,
                base_level = item.base_level,
                level = item.base_level + item.extra_level,
                experience = item.experience,
                is_baby = item.baby,
                baby_age = item.baby_age,
                next_imprint_time = item.next_cuddle,
                imprint_quality = item.imprint_quality,
                location = item.location,
                status = item.status,
                taming_effectiveness = 0,
                is_cryo = false,
                experience_points = item.experience,
                last_sync_time = DateTime.UtcNow,
                is_alive = true,
                commit_id = commitId.ToString(),
                commit_type = commitType
            };
        }

        public static int[] ConvertToStatsInt(Dictionary<string, int> data)
        {
            int[] values = new int[15];
            foreach (var v in data)
            {
                int key = int.Parse(v.Key);
                if (key < values.Length)
                    values[key] = v.Value;
            }
            return values;
        }

        public static float[] ConvertToStatsFloat(Dictionary<string, float> data)
        {
            float[] values = new float[15];
            foreach (var v in data)
            {
                int key = int.Parse(v.Key);
                if (key < values.Length)
                    values[key] = v.Value;
            }
            return values;
        }

        public override string GetBucketName()
        {
            return "dinos";
        }
    }
}
