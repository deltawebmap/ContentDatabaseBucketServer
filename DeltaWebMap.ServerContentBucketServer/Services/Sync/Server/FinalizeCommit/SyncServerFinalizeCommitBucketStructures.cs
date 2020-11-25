using DeltaWebMap.ServerContentBucketServer.ServiceTemplates.Sync.FinalizeCommit;
using LibDeltaSystem;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.ServerContentBucketServer.Services.Sync.Server.FinalizeCommit
{
    public class SyncServerFinalizeCommitBucketStructuresDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/{SERVER_ID}/buckets/structures/finalize_commit";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new SyncServerFinalizeCommitBucketStructures(conn, e);
        }
    }

    public class SyncServerFinalizeCommitBucketStructures : ISyncServerFinalizeCommit
    {
        public SyncServerFinalizeCommitBucketStructures(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override string GetBucketName()
        {
            return "structures";
        }
    }
}
