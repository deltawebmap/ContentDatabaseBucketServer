using DeltaWebMap.ContentDatabase;
using DeltaWebMap.ServerContentBucketServer.Framework;
using DeltaWebMap.ServerContentBucketServer.Framework.NetEvents;
using LibDeltaSystem;
using LibDeltaSystem.WebFramework;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace DeltaWebMap.ServerContentBucketServer
{
    class Program
    {
        public static MultithreadedContentDatabaseController databaseController;
        public static SyncServerAuthenticationProcessor syncServerAuthentication;
        public static ConcurrentDictionary<WorkingCommitIdentity, ActiveWorkingCommit> workingCommits;
        public static DeltaConnection connection;
        public static DeltaWebServer server;
        public static Random rand;
        public static NetEventSocketManager netEventManager;
        public static ContentUnitConfig unitConfig;

        public const int APP_VERISON_MAJOR = 0;
        public const int APP_VERISON_MINOR = 3;

        static void Main(string[] args)
        {
            //Init connection
            connection = DeltaConnection.InitDeltaManagedApp(args, DeltaCoreNetServerType.API_SERVER_CONTENT_BUCKET, APP_VERISON_MAJOR, APP_VERISON_MINOR);

            //Get the unit config. This is unique to us and tells us how to get ready
            connection.Log("INIT", "Getting unit config...", DeltaLogLevel.Medium);
            do
            {
                string filename = "cb_unit_" + connection.serverId + ".json";
                unitConfig = connection.GetUserConfig<ContentUnitConfig>(filename, new ContentUnitConfig
                {
                    is_configured = false,
                    instance_id = connection.serverId.ToString()
                }).GetAwaiter().GetResult();
                if(!unitConfig.is_configured)
                {
                    connection.Log("INIT", $"Unit config was not yet configured! Waiting for admin to fix this... ({filename})", DeltaLogLevel.Medium);
                    Thread.Sleep(10000);
                }
            } while (!unitConfig.is_configured);
            connection.Log("INIT", $"Unit config defined this server as having hostname {unitConfig.hostname}. Ready to go!", DeltaLogLevel.Medium);

            //Make sure directory exists
            string dir = unitConfig.content_root_path + connection.serverId + Path.DirectorySeparatorChar;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            //Open parts
            databaseController = new MultithreadedContentDatabaseController();
            syncServerAuthentication = new SyncServerAuthenticationProcessor(connection);
            workingCommits = new ConcurrentDictionary<WorkingCommitIdentity, ActiveWorkingCommit>();
            rand = new Random();
            netEventManager = new NetEventSocketManager();

            //Open server
            server = new DeltaWebServer(connection, connection.GetUserPort(0));
            server.AddService(new Services.User.UserEventWebsocketDefinition());
            server.AddService(new Services.User.Server.UserServerGetBucketsDefinition());

            server.AddService(new Services.User.Server.Fetch.UserServerFetchBucketDinosDefinition());
            server.AddService(new Services.User.Server.Fetch.UserServerFetchBucketStructuresDefinition());
            server.AddService(new Services.User.Server.Fetch.UserServerFetchBucketInventoriesDefinition());

            server.AddService(new Services.Sync.Server.Put.SyncServerPutBucketDinosDefinition());
            server.AddService(new Services.Sync.Server.Put.SyncServerPutBucketInventoriesDefinition());
            server.AddService(new Services.Sync.Server.Put.SyncServerPutBucketStructuresDefinition());

            server.AddService(new Services.Sync.Server.FinalizeCommit.SyncServerFinalizeCommitBucketDinosDefinition());
            server.AddService(new Services.Sync.Server.FinalizeCommit.SyncServerFinalizeCommitBucketStructuresDefinition());
            server.AddService(new Services.Sync.Server.FinalizeCommit.SyncServerFinalizeCommitBucketInventoriesDefinition());

            //Run
            server.RunAsync().GetAwaiter().GetResult();
        }
    }
}
