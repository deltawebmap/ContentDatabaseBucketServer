using DeltaWebMap.ContentDatabase.CommitBuilders.Write;
using LibDeltaSystem;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DeltaWebMap.ServerContentBucketServer.ServiceTemplates.Sync.Put
{
    public abstract class ISyncServerPut<T> : ISyncServer
    {
        public ISyncServerPut(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task HandleSyncServerBucket()
        {
            //Deserialize incoming data
            RequestData payload = await DecodePOSTBody<RequestData>();

            //Get the working commit. This sits in memory until we finsh
            var workingCommit = GetCreateWorkingCommit(payload.working_commit_id, payload.commit_type);

            //Convert each type
            foreach (var o in payload.items)
                workingCommit.AddObject(ConvertToCommitObject(o));

            //Dispatch net events
            var recipients = Program.netEventManager.GetRecipientsByServer(server._id);
            foreach(var o in payload.items)
            {
                //Get team ID
                int teamId = GetTeamId(o);
                
                //Check
                bool shouldSend = recipients.HasRecipients(x => x.isSuperuser || x.teamId == teamId);

                //Send
                if (shouldSend)
                    await recipients.SendContentChunkEvent(workingCommit.id, workingCommit.commitType, GetRpcNetType(o), x => x.isSuperuser || x.teamId == teamId);
            }

            //Success
            await WriteString("OK", "text/plain", 200);
        }

        public abstract WriteCommitObject ConvertToCommitObject(T item);
        public abstract int GetTeamId(T item);
        public abstract object GetRpcNetType(T item);

        class RequestData
        {
            public int working_commit_id; //An ID just used for this current session. Client generated
            public byte commit_type;
            public int chunk_index;
            public List<T> items;
        }
    }
}
