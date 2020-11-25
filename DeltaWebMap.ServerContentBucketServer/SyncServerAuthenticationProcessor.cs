using LibDeltaSystem;
using MongoDB.Bson;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DeltaWebMap.ServerContentBucketServer
{
    /// <summary>
    /// Serves as an in-memory cache for session tokens for sync servers.
    /// This serves as an authorization processor. It just checks if a sync token belongs to what server.
    /// </summary>
    public class SyncServerAuthenticationProcessor
    {
        private ConcurrentDictionary<string, ObjectId> cache;
        private DeltaConnection conn;

        public SyncServerAuthenticationProcessor(DeltaConnection conn)
        {
            this.cache = new ConcurrentDictionary<string, ObjectId>();
            this.conn = conn;
        }

        public async Task<ObjectId?> AuthorizeServer(string sessionId)
        {
            //Check if this exists in the cache
            if (cache.ContainsKey(sessionId))
                return cache[sessionId];

            //Look this up manually
            var session = await conn.AuthenticateServerSessionTokenAsync(sessionId);
            if (session == null)
                return null;
            ObjectId serverId = session.server_id;

            //Add this to the cache
            cache.TryAdd(sessionId, serverId);

            return serverId;
        }
    }
}
