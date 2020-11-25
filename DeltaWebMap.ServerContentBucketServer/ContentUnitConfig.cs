using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.ServerContentBucketServer
{
    /// <summary>
    /// Config specifically for this instance
    /// </summary>
    public class ContentUnitConfig
    {
        public bool is_configured = false;
        public string hostname = "XXX.content-prod.deltamap.net";
        public string instance_id;
        public string content_root_path;
    }
}
