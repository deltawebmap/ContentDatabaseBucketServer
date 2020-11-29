using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.ServerContentBucketServer.Framework.NetEvents
{
    public enum NetEventOpcode
    {
        COMMIT_PUT_CONTENT,
        COMMIT_FINALIZE,
        COMMIT_CREATE
    }
}
