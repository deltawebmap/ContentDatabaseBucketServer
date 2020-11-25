using DeltaWebMap.ContentDatabase;
using DeltaWebMap.ContentDatabase.CommitBuilders.Write;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DeltaWebMap.ServerContentBucketServer.Framework
{
    /// <summary>
    /// An in-progress commit that has not yet been pushed to disk
    /// </summary>
    public class ActiveWorkingCommit
    {
        public readonly ulong id;
        public readonly byte commitType;
        private WriteCommit commit;

        public ActiveWorkingCommit(byte commitType)
        {
            //Generate random ID
            byte[] randBytes = new byte[8];
            Program.rand.NextBytes(randBytes);
            id = BitConverter.ToUInt64(randBytes);

            //Set up
            commit = new WriteCommit(id, commitType);
            this.commitType = commitType;
        }

        public void AddObject(WriteCommitObject o)
        {
            commit.CommitObject(o);
        }

        public Task Finalize(MultithreadedContentDatabaseSession s)
        {
            return s.WriteCommitAsync(commit);
        }
    }
}
