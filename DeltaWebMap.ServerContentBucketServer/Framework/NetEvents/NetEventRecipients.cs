using MongoDB.Bson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DeltaWebMap.ServerContentBucketServer.Framework.NetEvents
{
    public class NetEventRecipients
    {
        public NetEventRecipients(List<NetEventSocketSubscription> subscriptions, ObjectId targetServerId)
        {
            this.subscriptions = subscriptions;
            this.targetServerId = targetServerId;
        }

        private List<NetEventSocketSubscription> subscriptions;
        private ObjectId targetServerId;

        private async Task DispatchEventAsync(NetEventOpcode opcode, object payload, Expression<Func<NetEventSocketSubscription, bool>> expression)
        {
            //Compile
            var compiled = expression.Compile();
            
            //Create
            string msg = JsonConvert.SerializeObject(new OutgoingMessage
            {
                opcode = opcode.ToString(),
                payload = payload,
                target_server_id = targetServerId.ToString()
            });

            //Send
            foreach(var s in subscriptions)
            {
                try
                {
                    if(compiled(s))
                        await s.sock.SendData(msg);
                }
                catch { }
            }
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
            return subscriptions.Count > 0;
        }

        public bool HasRecipients(Expression<Func<NetEventSocketSubscription, bool>> expression)
        {
            //Basic check
            if (subscriptions.Count < 0)
                return false;

            //Compile and check
            var compiled = expression.Compile();
            foreach(var s in subscriptions)
            {
                if (compiled(s))
                    return true;
            }
            return false;
        }

        public async Task SendContentChunkEvent(ulong commitId, byte commitType, object entity, Expression<Func<NetEventSocketSubscription, bool>> expression)
        {
            await DispatchEventAsync(NetEventOpcode.CONTENT_CHUNK, new PayloadSendContent
            {
                commit_id = GetNetCommitId(commitId),
                commit_type = commitType,
                entity = entity
            }, expression);
        }

        class PayloadSendContent
        {
            public NetCommitId commit_id;
            public byte commit_type;
            public object entity;
        }

        class NetCommitId
        {
            public string id_string;
            public uint id_1;
            public uint id_2;
        }

        class OutgoingMessage
        {
            public string opcode;
            public string target_server_id;
            public object payload;
        }
    }
}
