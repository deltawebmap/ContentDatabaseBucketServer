using DeltaWebMap.ServerContentBucketServer.Services.User;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DeltaWebMap.ServerContentBucketServer.Framework.NetEvents
{
    public class NetEventRecipients
    {
        public NetEventRecipients(List<UserEventWebsocket> clients)
        {
            this.clients = clients;
        }

        public List<UserEventWebsocket> clients;

        public void DispatchEventToAll(NetEventOpcode opcode, object payload)
        {
            //Send
            lock(clients)
            {
                foreach (var c in clients)
                    c.QueueSendMessage(opcode.ToString(), JObject.FromObject(payload));
            }
        }

        public NetEventRecipients GetRecipientsByServerTribe(ObjectId serverId, int tribeId)
        {
            List<UserEventWebsocket> found = new List<UserEventWebsocket>();
            lock (clients)
            {
                foreach (var s in clients)
                {
                    if (s.IsPrivilegedServerTribe(serverId, tribeId))
                    {
                        found.Add(s);
                    }
                }
            }
            return new NetEventRecipients(found);
        }

        public NetEventRecipients GetRecipientsByServer(ObjectId serverId)
        {
            List<UserEventWebsocket> found = new List<UserEventWebsocket>();
            lock (clients)
            {
                foreach (var s in clients)
                {
                    if (s.IsPrivilegedServer(serverId))
                    {
                        found.Add(s);
                    }
                }
            }
            return new NetEventRecipients(found);
        }

        private static NetCommitId GetNetCommitId(ulong id)
        {
            return new NetCommitId
            {
                id_string = id.ToString(),
                id_1 = (uint)(id & 0x00000000FFFFFFFF),
                id_2 = (uint)((id & 0xFFFFFFFF00000000) >> 32)
            };
        }

        public bool HasRecipients()
        {
            return clients.Count > 0;
        }

        public void SendCommitPutEvent(ObjectId serverId, string bucketName, ulong commitId, byte commitType, object entity)
        {
            DispatchEventToAll(NetEventOpcode.COMMIT_PUT_CONTENT, new EventContentCommitPutContent
            {
                commit_id = GetNetCommitId(commitId),
                commit_type = commitType,
                entity = entity,
                server_id = serverId.ToString(),
                bucket_name = bucketName
            });
        }

        public void SendCommitFinalizedEvent(ObjectId serverId, string bucketName, ulong commitId, byte commitType)
        {
            DispatchEventToAll(NetEventOpcode.COMMIT_FINALIZE, new EventContentCommitFinalize
            {
                commit_id = GetNetCommitId(commitId),
                commit_type = commitType,
                server_id = serverId.ToString(),
                bucket_name = bucketName
            });
        }

        public void SendCommitCreatedEvent(ObjectId serverId, string bucketName, ulong commitId, byte commitType)
        {
            DispatchEventToAll(NetEventOpcode.COMMIT_CREATE, new EventContentCommitCreate
            {
                commit_id = GetNetCommitId(commitId),
                commit_type = commitType,
                server_id = serverId.ToString(),
                bucket_name = bucketName
            });
        }

        class EventContentCommitBase
        {
            public NetCommitId commit_id;
            public string bucket_name;
            public string server_id;
            public byte commit_type;
        }

        class EventContentCommitPutContent : EventContentCommitBase
        {
            public object entity;
        }

        class EventContentCommitFinalize : EventContentCommitBase
        {
        }

        class EventContentCommitCreate : EventContentCommitBase
        {
        }

        class NetCommitId
        {
            public string id_string;
            public uint id_1;
            public uint id_2;
        }
    }
}
