using DeltaWebMap.ContentDatabase;
using LibDeltaSystem;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeltaWebMap.ServerContentBucketServer.ServiceTemplates
{
    public abstract class IServer : DeltaWebService
    {
        public IServer(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public static readonly string[] SUPPORTED_BUCKETS = new string[]
        {
            "dinos",
            "structures",
            "inventories"
        };

        private ObjectId serverId;

        public DbServer server;

        public override Task<bool> SetArgs(Dictionary<string, string> args)
        {
            serverId = ObjectId.Parse(args["SERVER_ID"]);
            return base.SetArgs(args);
        }

        public override async Task OnRequest()
        {
            //Load server details
            server = await conn.GetServerByIdAsync(serverId);
            if(server == null)
                throw new DeltaWebException("Server was not found.", 400);

            //Make sure this belongs to this server
            if (server.game_content_server_hostname != Program.unitConfig.hostname)
                throw new DeltaWebException($"This is not the correct server for hosting this content.", 400);

            //Handle
            await HandleBucket();
        }

        public MultithreadedContentDatabaseSession RequestBucket(string bucketName)
        {
            //Make sure this is an allowed bucket
            if (!SUPPORTED_BUCKETS.Contains(bucketName))
                throw new DeltaWebException("Bucket was not found.", 400);

            //Make sure directory exists
            string dir = Program.unitConfig.content_root_path + conn.serverId + "\\" + server.id + "\\";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            //Open database
            return Program.databaseController.GetDatabaseSession(dir + bucketName + ".cdb", 512);
        }

        public abstract Task HandleBucket();
    }
}
