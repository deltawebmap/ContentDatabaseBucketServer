using DeltaWebMap.ContentDatabase;
using DeltaWebMap.ServerContentBucketServer.Framework;
using DeltaWebMap.ServerContentBucketServer.ServiceTemplates.User.Fetch;
using LibDeltaSystem;
using LibDeltaSystem.Db.Content;
using LibDeltaSystem.Entities.CommonNet;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.ServerContentBucketServer.Services.User.Server.Fetch
{
    public class UserServerFetchBucketDinosDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/{SERVER_ID}/buckets/dinos";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new UserServerFetchBucketDinos(conn, e);
        }
    }

    public class UserServerFetchBucketDinos : IUserFetchServer<NetDino>
    {
        public UserServerFetchBucketDinos(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override NetDino ConvertObjectToWebType(DatabaseObject o, ulong object_id, ulong commit_id, int group_id, byte commit_type)
        {
            NetDino d = new NetDino
            {
                tribe_id = (int)o["tribe_id"],
                dino_id = ((ulong)((long)o["dino_id"])).ToString(),
                is_female = (bool)o["is_female"],
                colors = new int[6],
                colors_hex = new string[6],
                tamed_name = (string)o["tamed_name"],
                tamer_name = (string)o["tamer_name"],
                classname = (string)o["classname"],
                current_stats = (float[])o["current_stats"],
                max_stats = (float[])o["max_stats"],
                base_levelups_applied = (int[])o["base_levelups_applied"],
                tamed_levelups_applied = (int[])o["tamed_levelups_applied"],
                base_level = (int)o["base_level"],
                level = (int)o["level"],
                experience = (float)o["experience"],
                is_baby = (bool)o["is_baby"],
                baby_age = (float)o["baby_age"],
                next_imprint_time = (double)o["next_imprint_time"],
                imprint_quality = (float)o["imprint_quality"],
                location = ConversionTools.ConvertObjectToLocation((DatabaseObject)o["location"]),
                status = (string)o["status"],
                taming_effectiveness = (float)o["taming_effectiveness"],
                is_cryo = (bool)o["is_cryo"],
                last_sync_time = new DateTime((long)o["last_sync_time"]),
                is_alive = true
            };
            return d;
        }

        public override string GetBucketName()
        {
            return "dinos";
        }
    }
}
