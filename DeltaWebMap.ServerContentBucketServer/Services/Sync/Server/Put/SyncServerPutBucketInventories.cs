using DeltaWebMap.ContentDatabase;
using DeltaWebMap.ContentDatabase.CommitBuilders.Write;
using DeltaWebMap.ServerContentBucketServer.Framework;
using DeltaWebMap.ServerContentBucketServer.NetEntities.Inventories;
using DeltaWebMap.ServerContentBucketServer.ServiceTemplates.Sync.Put;
using LibDeltaSystem;
using LibDeltaSystem.Entities.CommonNet;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DeltaWebMap.ServerContentBucketServer.Services.Sync.Server.Put
{
    public class SyncServerPutBucketInventoriesDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/{SERVER_ID}/buckets/inventories/put";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new SyncServerPutBucketInventories(conn, e);
        }
    }

    public class SyncServerPutBucketInventories : ISyncServerPut<InventorySyncPayload>
    {
        public SyncServerPutBucketInventories(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override WriteCommitObject ConvertToCommitObject(InventorySyncPayload item)
        {
            WriteCommitObject obj = new WriteCommitObject(ConversionTools.GetMultipartID(item.id1, item.id2), item.tribe);
            WriteObject[] items = new WriteObject[item.items.Length];
            for(int i = 0; i<items.Length; i++)
            {
                WriteObject o = new WriteObject();
                InventorySyncItem s = item.items[i];
                o.WriteInt64("item_id", (long)ConversionTools.GetMultipartID(s.i1, s.i2));
                o.WriteString("classname", ConversionTools.TrimArkClassname(s.c));
                o.WriteFloat("durability", s.d);
                o.WriteInt32("stack_size", s.q);
                o.WriteInt16("flags", GetFlags(s));
                items[i] = o;
            }
            obj.WriteChildObjectArray("items", items);
            obj.WriteInt64("created_time", DateTime.UtcNow.Ticks);
            return obj;
        }

        public override object GetRpcNetType(InventorySyncPayload item)
        {
            NetInventory inventory = new NetInventory
            {
                holder_id = ConversionTools.GetMultipartID(item.id1, item.id2).ToString(),
                tribe_id = item.tribe,
                items = new NetInventory.NetInventory_Item[item.items.Length]
            };
            for(int i = 0; i<item.items.Length; i++)
            {
                InventorySyncItem s = item.items[i];
                inventory.items[i] = new NetInventory.NetInventory_Item
                {
                    classname = ConversionTools.TrimArkClassname(s.c),
                    item_id = ((long)ConversionTools.GetMultipartID(s.i1, s.i2)).ToString(),
                    durability = s.d,
                    stack_size = s.q,
                    flags = (ushort)GetFlags(s)
                };
            }
            return inventory;
        }

        private short GetFlags(InventorySyncItem s)
        {
            //0: Is Tek?
            //1: Is Blueprint?
            short flags = 0;
            if (s.tek)
                flags |= 1 << 0;
            if (s.blp)
                flags |= 1 << 0;
            return flags;
        }

        public override int GetTeamId(InventorySyncPayload item)
        {
            return item.tribe;
        }
    }
}
