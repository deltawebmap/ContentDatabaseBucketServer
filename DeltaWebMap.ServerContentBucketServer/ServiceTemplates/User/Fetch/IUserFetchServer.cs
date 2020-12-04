using DeltaWebMap.ContentDatabase;
using DeltaWebMap.ContentDatabase.CommitBuilders.Find;
using DeltaWebMap.ServerContentBucketServer.Framework;
using DeltaWebMap.ServerContentBucketServer.Framework.BucketReadFormat;
using LibDeltaSystem;
using LibDeltaSystem.Tools.DeltaWebFormat;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace DeltaWebMap.ServerContentBucketServer.ServiceTemplates.User.Fetch
{
    public abstract class IUserFetchServer<T> : IUserServer
    {
        public IUserFetchServer(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
            formats = new Dictionary<string, IBucketReadFormat>();
            AddFormat(new BucketFormatJson());
            AddFormat(new BucketFormatDWF(typeof(T)));
        }

        public string format;
        public int limit;
        public int skip;
        public MultithreadedContentDatabaseSession bucket;
        private Dictionary<string, IBucketReadFormat> formats;

        public void AddFormat(IBucketReadFormat fmt)
        {
            formats.Add(fmt.id, fmt);
        }

        public override async Task HandleUserBucket()
        {
            //Read parameters
            limit = GetIntFromQuery("limit", 100, 0, 4000);
            skip = GetIntFromQuery("skip", 0, 0, int.MaxValue);

            //Get format parameter
            if (!e.Request.Query.ContainsKey("format"))
                throw new DeltaWebException("No format specified.", 400);
            format = e.Request.Query["format"];
            if(!formats.ContainsKey(format))
                throw new DeltaWebException("Format invalid.", 400);

            //Get the compression
            CompressionType compression = GetEnumStringFromQuery("compression", CompressionType.NONE);

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
            if(compression == CompressionType.NONE)
            {
                //Normal
                await formats[format].Encode(e.Response.Body, items);
            } else
            {
                //Compressed
                using(GZipStream gz = new GZipStream(e.Response.Body, CompressionLevel.Fastest, true))
                    await formats[format].Encode(gz, items);
            }
        }

        public abstract string GetBucketName();

        public abstract T ConvertObjectToWebType(DatabaseObject o, ulong object_id, ulong commit_id, int group_id, byte commit_type);

        class ResponseJsonData
        {
            public object[] data;
        }

        enum CompressionType
        {
            NONE,
            GZIP
        }
    }
}
