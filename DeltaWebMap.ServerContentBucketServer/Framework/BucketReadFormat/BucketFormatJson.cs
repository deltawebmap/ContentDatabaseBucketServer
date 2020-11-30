using LibDeltaSystem.WebFramework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DeltaWebMap.ServerContentBucketServer.Framework.BucketReadFormat
{
    public class BucketFormatJson : IBucketReadFormat
    {
        public string display_name => "JSON";

        public string id => "json";

        public Task Encode(Stream output, object[] data)
        {
            byte[] d = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new ResponseJsonData
            {
                data = data
            }));
            return output.WriteAsync(d, 0, d.Length);
        }

        class ResponseJsonData
        {
            public object[] data;
        }
    }
}
