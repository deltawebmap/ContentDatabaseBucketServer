using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.ServerContentBucketServer.Framework
{
    public struct WorkingCommitIdentity
    {
        public ObjectId server_id;
        public int working_commit_id;

        public WorkingCommitIdentity(ObjectId server_id, int working_commit_id)
        {
            this.server_id = server_id;
            this.working_commit_id = working_commit_id;
        }
    }
}
