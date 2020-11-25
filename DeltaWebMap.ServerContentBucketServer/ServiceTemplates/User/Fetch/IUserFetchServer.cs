using DeltaWebMap.ContentDatabase;
using DeltaWebMap.ContentDatabase.CommitBuilders.Find;
using LibDeltaSystem;
using LibDeltaSystem.Tools.DeltaWebFormat;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DeltaWebMap.ServerContentBucketServer.ServiceTemplates.User.Fetch
{
    public abstract class IUserFetchServer<T> : IUserServer
    {
        public IUserFetchServer(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public string format;
        public int limit;
        public int skip;
        public MultithreadedContentDatabaseSession bucket;

        public override async Task HandleUserBucket()
        {
            //Read parameters
            limit = GetIntFromQuery("limit", 100, 0, 1000);
            skip = GetIntFromQuery("skip", 0, 0, int.MaxValue);

            //Get format parameter
            if (!e.Request.Query.ContainsKey("format"))
                throw new DeltaWebException("No format specified. Please set the \"format\" query parameter to one of \"json\" or \"dwf_v1\".", 400);
            format = e.Request.Query["format"];
            if(format != "json" && format != "dwf_v1")
                throw new DeltaWebException("Format invalid. Please set the \"format\" query parameter to one of \"json\" or \"dwf_v1\".", 400);

            //Open the requested bucket
            bucket = RequestBucket(GetBucketName());

            //Load results
            FindCommitResults results;
            if (admin)
                results = await bucket.FindAllItemsAsync(skip, limit);
            else
                results = await bucket.FindTeamItemsAsync(profile.tribe_id, skip, limit);

            //Deserialize all results and convert them into a web type
            object[] items = new object[results.results.Count];
            for(int i = 0; i<results.results.Count; i++)
            {
                //Deserialize blob
                var r = results.results[i];
                var deser = r.Deserialize();

                //Convert
                T item = ConvertObjectToWebType(deser, r.object_id, r.commit_id, r.group_id, r.commit_type);
                items[i] = item;
            }

            //We'll now send the results based on the format requested
            if(format == "json")
            {
                await WriteJSON(new ResponseJsonData
                {
                    data = items
                });
            } else if (format == "dwf_v1")
            {
                using(MemoryStream ms = new MemoryStream())
                {
                    //Encode
                    DeltaWebFormatEncoder encoder = new DeltaWebFormatEncoder(ms, typeof(T));
                    encoder.Encode(items, new Dictionary<byte, byte[]>());

                    //Rewind and copy
                    ms.Position = 0;
                    await ms.CopyToAsync(e.Response.Body);
                }
            } else
            {
                throw new Exception("Unknown format, but permitted by entry to the function.");
            }
        }

        public abstract string GetBucketName();

        public abstract T ConvertObjectToWebType(DatabaseObject o, ulong object_id, ulong commit_id, int group_id, byte commit_type);

        class ResponseJsonData
        {
            public object[] data;
        }
    }
}
