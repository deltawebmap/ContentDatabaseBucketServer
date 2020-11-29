using DeltaWebMap.ServerContentBucketServer.Framework;
using LibDeltaSystem;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DeltaWebMap.ServerContentBucketServer.ServiceTemplates.Sync
{
    /// <summary>
    /// This class with authenticate server buckets 
    /// </summary>
    public abstract class ISyncServer : IServer
    {
        public ISyncServer(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task HandleBucket()
        {
            //Log in the server
            await TryLogInServer();

            //Handle
            await HandleSyncServerBucket();
        }

        private async Task TryLogInServer()
        {
            //Make sure we've provided credentials
            if (!e.Request.Query.ContainsKey("session_token"))
                throw new DeltaWebException("Request missing server credentials.", 400);

            //Authorize this server by attempting to log it in
            var challengeServerId = await Program.syncServerAuthentication.AuthorizeServer(e.Request.Query["session_token"]);
            if (!challengeServerId.HasValue)
                throw new DeltaWebException("The supplied credentials are not valid.", 400);
            if(challengeServerId.Value != server._id)
                throw new DeltaWebException("The supplied credentials do not match the server you are attempting to access.", 400);
        }

        public abstract Task HandleSyncServerBucket();

        private WorkingCommitIdentity GetWorkingCommitIdentity(int workingCommitId)
        {
            return new WorkingCommitIdentity(server._id, workingCommitId);
        }

        public ActiveWorkingCommit GetCreateWorkingCommit(int workingCommitId, byte commitType)
        {
            return Program.workingCommits.GetOrAdd(GetWorkingCommitIdentity(workingCommitId), (WorkingCommitIdentity _id) =>
            {
                //Create new
                var c = new ActiveWorkingCommit(commitType);

                //Dispatch net events for this
                Program.netEventManager.GetRecipientsByServer(server._id).SendCommitCreatedEvent(server._id, c.id, c.commitType);

                return c;
            });
        }

        public ActiveWorkingCommit RemoveWorkingCommit(int workingCommitId)
        {
            Program.workingCommits.TryRemove(GetWorkingCommitIdentity(workingCommitId), out ActiveWorkingCommit commit);
            return commit;
        }
    }
}
