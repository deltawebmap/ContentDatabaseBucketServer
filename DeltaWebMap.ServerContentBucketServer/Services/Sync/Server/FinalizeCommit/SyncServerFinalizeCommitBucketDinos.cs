using DeltaWebMap.ServerContentBucketServer.ServiceTemplates.Sync.FinalizeCommit;
using LibDeltaSystem;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.ServerContentBucketServer.Services.Sync.Server.FinalizeCommit
{
    public class SyncServerFinalizeCommitBucketDinosDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/{SERVER_ID}/buckets/dinos/finalize_commit";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new SyncServerFinalizeCommitBucketDinos(conn, e);
        }
    }

    public class SyncServerFinalizeCommitBucketDinos : ISyncServerFinalizeCommit
    {
        public SyncServerFinalizeCommitBucketDinos(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override string GetBucketName()
        {
            return "dinos";
        }
    }
}
