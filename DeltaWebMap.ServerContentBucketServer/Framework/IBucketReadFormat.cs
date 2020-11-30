using LibDeltaSystem.WebFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DeltaWebMap.ServerContentBucketServer.Framework
{
    public interface IBucketReadFormat
    {
        string display_name { get; }
        string id { get; }
        Task Encode(Stream output, object[] data);
    }
}
