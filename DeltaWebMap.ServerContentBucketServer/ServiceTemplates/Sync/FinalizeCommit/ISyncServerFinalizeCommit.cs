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

            //Push and prune
            await workingCommit.Finalize(bucket);

            //Dispatch net events
            var serverRecipients = Program.netEventManager.GetRecipientsByServer(server._id);
            if (serverRecipients.HasRecipients())
                serverRecipients.SendCommitFinalizedEvent(server._id, GetBucketName(), workingCommit.id, workingCommit.commitType);

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
