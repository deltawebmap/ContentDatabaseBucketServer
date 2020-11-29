using DeltaWebMap.ServerContentBucketServer.Services.User;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace DeltaWebMap.ServerContentBucketServer.Framework.NetEvents
{
    public class NetEventSocketManager : NetEventRecipients
    {
        public NetEventSocketManager() : base(new List<UserEventWebsocket>())
        {
        }
    }
}
