using DeltaWebMap.ContentDatabase;
using DeltaWebMap.ServerContentBucketServer.NetEntities.Inventories;
using DeltaWebMap.ServerContentBucketServer.ServiceTemplates.User.Fetch;
using LibDeltaSystem;
using LibDeltaSystem.Entities.CommonNet;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.ServerContentBucketServer.Services.User.Server.Fetch
{
    public class UserServerFetchBucketInventoriesDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/{SERVER_ID}/buckets/inventories";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new UserServerFetchBucketInventories(conn, e);
        }
    }

    public class UserServerFetchBucketInventories : IUserFetchServer<NetInventory>
    {
        public UserServerFetchBucketInventories(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override NetInventory ConvertObjectToWebType(DatabaseObject o, ulong object_id, ulong commit_id, int group_id, byte commit_type)
        {
            //Init
            DatabaseObject[] oItems = (DatabaseObject[])o["items"];
            NetInventory n = new NetInventory
            {
                holder_id = object_id.ToString(),
                items = new NetInventory.NetInventory_Item[oItems.Length],
                tribe_id = group_id,
                commit_id = commit_id.ToString(),
                commit_type = commit_type
            };

            //Populate
            for(int i = 0; i<oItems.Length; i++)
                n.items[i] = ConvertItem(oItems[i]);

            return n;
        }

        private NetInventory.NetInventory_Item ConvertItem(DatabaseObject o)
        {
            return new NetInventory.NetInventory_Item
            {
                classname = (string)o["classname"],
                item_id = ((ulong)((long)o["item_id"])).ToString(),
                durability = (float)o["durability"],
                flags = (ushort)(short)o["flags"],
                stack_size = (int)o["stack_size"],
            };
        }

        public override string GetBucketName()
        {
            return "inventories";
        }
    }
}
