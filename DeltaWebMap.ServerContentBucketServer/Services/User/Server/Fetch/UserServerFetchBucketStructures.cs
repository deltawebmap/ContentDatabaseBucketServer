using DeltaWebMap.ContentDatabase;
using DeltaWebMap.ServerContentBucketServer.Framework;
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
    public class UserServerFetchBucketStructuresDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/{SERVER_ID}/buckets/structures";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new UserServerFetchBucketStructures(conn, e);
        }
    }

    public class UserServerFetchBucketStructures : IUserFetchServer<NetStructure>
    {
        public UserServerFetchBucketStructures(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override NetStructure ConvertObjectToWebType(DatabaseObject o, ulong object_id, ulong commit_id, int group_id, byte commit_type)
        {
            return new NetStructure
            {
                classname = (string)o["classname"],
                has_inventory = (bool)o["has_inventory"],
                location = ConversionTools.ConvertObjectToLocation((DatabaseObject)o["location"]),
                structure_id = (int)object_id,
                tribe_id = group_id
            };
        }

        public override string GetBucketName()
        {
            return "structures";
        }
    }
}
