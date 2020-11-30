using DeltaWebMap.ContentDatabase.CommitBuilders.Write;
using DeltaWebMap.ServerContentBucketServer.Framework;
using DeltaWebMap.ServerContentBucketServer.NetEntities.Structures;
using DeltaWebMap.ServerContentBucketServer.ServiceTemplates.Sync.Put;
using LibDeltaSystem;
using LibDeltaSystem.Entities.CommonNet;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DeltaWebMap.ServerContentBucketServer.Services.Sync.Server.Put
{
    public class SyncServerPutBucketStructuresDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/{SERVER_ID}/buckets/structures/put";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new SyncServerPutBucketStructures(conn, e);
        }
    }

    public class SyncServerPutBucketStructures : ISyncServerPut<StructureSyncPayload>
    {
        public SyncServerPutBucketStructures(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override WriteCommitObject ConvertToCommitObject(StructureSyncPayload item)
        {
            WriteCommitObject obj = new WriteCommitObject((ulong)item.id, item.tribe);
            obj.WriteChildObject("location", ConversionTools.ConvertLocationToWriteObject(item.location));
            obj.WriteString("classname", ConversionTools.TrimArkClassname(item.classname));
            obj.WriteBool("has_inventory", true);
            obj.WriteInt32("current_item_count", 0);
            obj.WriteInt32("max_item_count", 0);
            obj.WriteFloat("max_health", item.max_health);
            obj.WriteFloat("current_health", item.health);
            return obj;
        }

        public override object GetRpcNetType(StructureSyncPayload item)
        {
            return new NetStructure
            {
                classname = ConversionTools.TrimArkClassname(item.classname),
                has_inventory = true,
                location = item.location,
                structure_id = item.id,
                tribe_id = item.tribe
            };
        }

        public override int GetTeamId(StructureSyncPayload item)
        {
            return item.tribe;
        }

        public override string GetBucketName()
        {
            return "structures";
        }
    }
}
