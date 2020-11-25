using DeltaWebMap.ServerContentBucketServer.Framework.NetEvents;
using LibDeltaSystem;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.WebFramework;
using LibDeltaSystem.WebFramework.WebSockets;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
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

    public class UserEventWebsocket : DeltaWebSocketService
    {
        public UserEventWebsocket(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
            manager = Program.netEventManager;
            subscriptions = new List<NetEventSocketSubscription>();
        }

        public bool authenticated;
        public DbUser user;

        public NetEventSocketManager manager;
        public List<NetEventSocketSubscription> subscriptions;

        public override async Task OnReceiveBinary(byte[] data, int length)
        {
            
        }

        public override async Task OnReceiveText(string data)
        {
            //Decode
            RequestData cmd = JsonConvert.DeserializeObject<RequestData>(data);
            switch(cmd.opcode)
            {
                case "CMD_LOGIN": await OnLoginRequest(cmd.value); break;
                case "CMD_REGISTER_SERVER": await OnRegisterServerRequest(cmd.value); break;
                default: await DisconnectAsync(NetEventSocketError.INVALID_COMMAND); break;
            }
        }

        public override async Task OnSockClosed(WebSocket sock)
        {
            
        }

        public override async Task OnSockOpened(WebSocket sock)
        {
            
        }

        private async Task DisconnectAsync(NetEventSocketError error)
        {
            await DisconnectAsync(WebSocketCloseStatus.InternalServerError, "ERR" + ((int)error).ToString().PadLeft(3, '0') + ": " + error.ToString());
        }

        private async Task OnLoginRequest(string token)
        {
            //Log in user
            user = await conn.AuthenticateUserToken(token);
            authenticated = user != null;
            if (!authenticated)
                await DisconnectAsync(NetEventSocketError.LOGIN_FAILED);
        }

        private async Task OnRegisterServerRequest(string id)
        {
            //Make sure we're logged in
            if (!authenticated)
            {
                await DisconnectAsync(NetEventSocketError.LOGIN_REQUIRED);
                return;
            }

            //Get the server by the ID
            DbServer server = await conn.GetServerByIdAsync(id);
            if (server == null)
            {
                await DisconnectAsync(NetEventSocketError.SERVER_NOT_FOUND);
                return;
            }

            //Check if we already have a subscription to this
            bool exists = false;
            lock (subscriptions)
            {
                foreach (var s in subscriptions)
                    exists = exists || s.serverId == server._id;
            }
            if(exists)
            {
                await DisconnectAsync(NetEventSocketError.SERVER_ALREADY_REGISTERED);
                return;
            }

            //Check how we'll be authenticated
            bool isAdmin = server.CheckIsUserAdmin(user);
            var profile = await server.GetUserPlayerProfile(conn, user);

            //Make sure authentication was OK
            if(!isAdmin && profile == null)
            {
                await DisconnectAsync(NetEventSocketError.SERVER_NOT_PERMITTED);
                return;
            }

            //Add subscription
            NetEventSocketSubscription subscription = new NetEventSocketSubscription
            {
                serverId = server._id,
                isSuperuser = isAdmin,
                sock = this,
                teamId = profile != null ? profile.tribe_id : 0
            };
            lock (subscriptions)
                subscriptions.Add(subscription);
            lock (manager.subscriptions)
                manager.subscriptions.Add(subscription);
        }

        class RequestData
        {
            public string opcode;
            public string value;
        }
    }
}
