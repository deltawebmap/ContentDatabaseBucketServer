using LibDeltaSystem;
using LibDeltaSystem.Db.Content;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DeltaWebMap.ServerContentBucketServer.ServiceTemplates.User
{
    public abstract class IUserServer : IServer
    {
        public IUserServer(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public DbUser user;
        public DbPlayerProfile profile;
        public bool admin;

        public override async Task HandleBucket()
        {
            //Log in user
            user = await AuthenticateUser();

            //Get user profile
            profile = await server.GetUserPlayerProfile(conn, user);
            admin = server.CheckIsUserAdmin(user);
            if (profile == null && !admin)
                throw new DeltaWebException("You do not have a tribe on this server and you are not an admin on this server.", 400);

            //Handle
            await HandleUserBucket();
        }

        public abstract Task HandleUserBucket();

        private async Task<DbUser> AuthenticateUser()
        {
            //Get token string
            string tokenString = GetAuthToken();
            if (tokenString == null)
                throw new DeltaWebException("No access token provided.", 400);

            //Get token
            var token = await conn.GetTokenByTokenAsync(tokenString);
            if (token == null)
                throw new DeltaWebException("Access token is not valid.", 400);

            //TODO: Check token scope

            //Get user
            var user = await conn.GetUserByIdAsync(token.user_id);
            if (user == null)
                throw new DeltaWebException("User account not found.", 500);

            return user;
        }

        private string GetAuthToken()
        {
            if (!e.Request.Headers.ContainsKey("authorization"))
                return null;
            string h = e.Request.Headers["authorization"];
            if (!h.StartsWith("Bearer "))
                return null;
            return h.Substring("Bearer ".Length);
        }
    }
}
