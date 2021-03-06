﻿using DeltaWebMap.ContentDatabase.CommitBuilders.Write;
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
            var workingCommit = GetCreateWorkingCommit(GetBucketName(), payload.working_commit_id, payload.commit_type);

            //Convert each type
            foreach (var o in payload.items)
                workingCommit.AddObject(ConvertToCommitObject(o));

            //Dispatch net events
            var serverRecipients = Program.netEventManager.GetRecipientsByServer(server._id);
            if(serverRecipients.HasRecipients())
            {
                foreach (var o in payload.items)
                {
                    //Get team ID
                    int teamId = GetTeamId(o);

                    //Check
                    var tribeRecipients = serverRecipients.GetRecipientsByServerTribe(server._id, teamId);

                    //Send
                    if (tribeRecipients.HasRecipients())
                        tribeRecipients.SendCommitPutEvent(server._id, GetBucketName(), workingCommit.id, workingCommit.commitType, GetRpcNetType(o, workingCommit.id, workingCommit.commitType));
                }
            }

            //Success
            await WriteString("OK", "text/plain", 200);
        }

        public abstract WriteCommitObject ConvertToCommitObject(T item);
        public abstract int GetTeamId(T item);
        public abstract object GetRpcNetType(T item, ulong commitId, byte commitType);
        public abstract string GetBucketName();

        class RequestData
        {
            public int working_commit_id; //An ID just used for this current session. Client generated
            public byte commit_type;
            public int chunk_index;
            public List<T> items;
        }
    }
}
