using DeltaWebMap.ServerContentBucketServer.Framework.NetEvents;
using LibDeltaSystem;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.WebFramework;
using LibDeltaSystem.WebFramework.WebSockets;
using LibDeltaSystem.WebFramework.WebSockets.OpcodeSock;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace DeltaWebMap.ServerContentBucketServer.Services.User
{
    public class UserEventWebsocketDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/events";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new UserEventWebsocket(conn, e);
        }
    }

    public class UserEventWebsocket : DeltaOpcodeUserWebSocketService
    {
        public UserEventWebsocket(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
            manager = Program.netEventManager;
            subscriptions = new ConcurrentDictionary<ObjectId, NetEventSocketSubscription>();
            RegisterCommandHandler("REGISTER_GUILD", OnRegisterServerRequest);
        }

        public NetEventSocketManager manager;
        public ConcurrentDictionary<ObjectId, NetEventSocketSubscription> subscriptions; //Key is a guild ID

        public bool IsPrivilegedServer(ObjectId serverId)
        {
            return subscriptions.ContainsKey(serverId);
        }

        public bool IsPrivilegedServerTribe(ObjectId serverId, int tribeId)
        {
            //Check if we have this server registered
            if (!subscriptions.TryGetValue(serverId, out NetEventSocketSubscription sub))
                return false;

            return sub.isSuperuser || sub.teamId == tribeId;
        }

        public bool TryQueuePrivilegedServerTribeCommand(ObjectId serverId, int tribeId, ISockCommand cmd)
        {
            if(IsPrivilegedServerTribe(serverId, tribeId))
            {
                EnqueueMessage(cmd);
                return true;
            }
            return false;
        }

        public override async Task OnSockOpened()
        {
            lock (manager.clients)
                manager.clients.Add(this);
        }

        public override async Task OnSockClosed()
        {
            lock (manager.clients)
                manager.clients.Remove(this);
        }

        public override async Task OnUserLoginSuccess()
        {
            
        }

        private async Task OnRegisterServerRequest(JObject cmd)
        {
            //Make sure we're logged in
            if (user == null)
            {
                await SendRegisterStatus(false, "User has not logged in yet.");
                return;
            }

            //Validate message
            if(!UtilValidateJObject(cmd, out string validateError, new JObjectValidationParameter("guild_id", JTokenType.String)))
            {
                await SendRegisterStatus(false, validateError);
                return;
            }

            //Get the server by the ID
            DbServer server = await conn.GetServerByIdAsync((string)cmd["guild_id"]);
            if (server == null)
            {
                await SendRegisterStatus(false, "Guild ID not found.");
                return;
            }

            //Check how we'll be authenticated
            bool isAdmin = server.CheckIsUserAdmin(user);
            var profile = await server.GetUserPlayerProfile(conn, user);

            //Make sure authentication was OK
            if(!isAdmin && profile == null)
            {
                await SendRegisterStatus(false, "Guild access not permitted to this user.");
                return;
            }

            //Create subscription
            NetEventSocketSubscription subscription = new NetEventSocketSubscription
            {
                isSuperuser = isAdmin,
                teamId = profile != null ? profile.tribe_id : 0
            };

            //Add or update
            subscriptions.AddOrUpdate(server._id, subscription, (ObjectId _id, NetEventSocketSubscription _old) => {
                return subscription;
            });

            //Write response
            if(isAdmin)
                await SendRegisterStatus(true, $"Registered as superuser.");
            else
                await SendRegisterStatus(true, $"Registered as tribe ID {profile.tribe_id}.");
        }

        private async Task SendRegisterStatus(bool success, string message)
        {
            JObject msg = new JObject();
            msg["success"] = success;
            msg["message"] = message;
            await SendMessage("GUILD_REGISTER_STATUS", msg);
        }
    }
}
