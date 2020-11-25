using DeltaWebMap.ServerContentBucketServer.ServiceTemplates.Sync.FinalizeCommit;
using LibDeltaSystem;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.ServerContentBucketServer.Services.Sync.Server.FinalizeCommit
{
    public class SyncServerFinalizeCommitBucketInventoriesDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/{SERVER_ID}/buckets/inventories/finalize_commit";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new SyncServerFinalizeCommitBucketInventories(conn, e);
        }
    }

    public class SyncServerFinalizeCommitBucketInventories : ISyncServerFinalizeCommit
    {
        public SyncServerFinalizeCommitBucketInventories(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override string GetBucketName()
        {
            return "inventories";
        }
    }
}
