using DeltaWebMap.ContentDatabase;
using DeltaWebMap.ServerContentBucketServer.ServiceTemplates.User;
using LibDeltaSystem;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DeltaWebMap.ServerContentBucketServer.Services.User.Server
{
    public class UserServerGetBucketsDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/{SERVER_ID}/bucket_list";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new UserServerGetBuckets(conn, e);
        }
    }

    public class UserServerGetBuckets : IUserServer
    {
        public UserServerGetBuckets(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task HandleUserBucket()
        {
            //Create response
            ResponseData response = new ResponseData
            {
                buckets = new List<ResponseDataBucket>(),
                superuser = admin
            };
            
            //Get info on all buckets
            foreach(var n in SUPPORTED_BUCKETS)
            {
                //Get bucket and its data
                var bucket = RequestBucket(n);
                long count;
                if (admin)
                    count = await bucket.CountAllItemsAsync();
                else
                    count = await bucket.CountTeamItemsAsync(profile.tribe_id);

                //Add bucket data
                response.buckets.Add(new ResponseDataBucket
                {
                    bucket_name = n,
                    allowed_count = count
                });
            }

            await WriteJSON(response);
        }

        class ResponseData
        {
            public List<ResponseDataBucket> buckets;
            public bool superuser;
        }

        class ResponseDataBucket
        {
            public string bucket_name;
            public long allowed_count;
        }
    }
}
