using DeltaWebMap.ServerContentBucketServer.Services.User;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.ServerContentBucketServer.Framework.NetEvents
{
    /// <summary>
    /// A subscription to a server by a user
    /// </summary>
    public class NetEventSocketSubscription
    {
        public UserEventWebsocket sock;
        public ObjectId serverId;
        public bool isSuperuser;
        public int teamId;
    }
}
