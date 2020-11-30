using LibDeltaSystem.Tools.DeltaWebFormat;
using LibDeltaSystem.WebFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DeltaWebMap.ServerContentBucketServer.Framework.BucketReadFormat
{
    public class BucketFormatDWF : IBucketReadFormat
    {
        public string display_name => "dwf_v1";

        public string id => "dwf_v1";

        public BucketFormatDWF(Type t)
        {
            this.t = t;
        }

        private Type t;

        public async Task Encode(Stream output, object[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                //Encode
                DeltaWebFormatEncoder encoder = new DeltaWebFormatEncoder(ms, t);
                encoder.Encode(data, new Dictionary<byte, byte[]>());
                await ms.FlushAsync();

                //Rewind and copy
                ms.Position = 0;
                await ms.CopyToAsync(output);
                await output.FlushAsync();
            }
        }
    }
}
