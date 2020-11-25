using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace DeltaWebMap.ServerContentBucketServer.Framework.NetEvents
{
    public class NetEventSocketManager
    {
        public List<NetEventSocketSubscription> subscriptions;

        public NetEventSocketManager()
        {
            subscriptions = new List<NetEventSocketSubscription>();
        }

        private NetEventRecipients FindRecipients(ObjectId serverId, Expression<Func<NetEventSocketSubscription, bool>> expression)
        {
            //Compile
            var compiled = expression.Compile();
            
            //Find
            List<NetEventSocketSubscription> filtered = new List<NetEventSocketSubscription>();
            lock(subscriptions)
            {
                foreach(var s in subscriptions)
                {
                    if (compiled(s))
                        filtered.Add(s);
                }
            }

            return new NetEventRecipients(subscriptions, serverId);
        }

        public NetEventRecipients GetRecipientsByTribe(ObjectId serverId, int tribeId, bool includeSuperusers = true)
        {
            return FindRecipients(serverId, x => x.serverId == serverId && (x.teamId == tribeId || (includeSuperusers && x.isSuperuser)));
        }

        public NetEventRecipients GetRecipientsByServer(ObjectId serverId)
        {
            return FindRecipients(serverId, x => x.serverId == serverId);
        }
    }
}
