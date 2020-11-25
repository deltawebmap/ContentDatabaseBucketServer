using LibDeltaSystem;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DeltaWebMap.ServerContentBucketServer.ServiceTemplates.Sync.FinalizeCommit
{
    public abstract class ISyncServerFinalizeCommit : ISyncServer
    {
        protected ISyncServerFinalizeCommit(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task HandleSyncServerBucket()
        {
            //Deserialize incoming data
            RequestData payload = await DecodePOSTBody<RequestData>();

            //Get the working commit
            var workingCommit = RemoveWorkingCommit(payload.working_commit_id);
            if(workingCommit == null)
            {
                await WriteString("EMPTY", "text/plain", 200);
                return;
            }

            //Open the requested bucket
            var bucket = RequestBucket(GetBucketName());

            //Push
            await workingCommit.Finalize(bucket);

            //Success
            await WriteString("OK", "text/plain", 200);
        }

        public abstract string GetBucketName();

        class RequestData
        {
            public int working_commit_id; //An ID just used for this current session. Client generated
            public int chunk_count;
        }
    }
}
