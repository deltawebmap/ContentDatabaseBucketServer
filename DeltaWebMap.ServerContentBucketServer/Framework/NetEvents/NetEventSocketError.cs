using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.ServerContentBucketServer.Framework.NetEvents
{
    public enum NetEventSocketError
    {
        LOGIN_FAILED = 1,
        LOGIN_REQUIRED = 2,
        SERVER_NOT_FOUND = 3,
        SERVER_NOT_PERMITTED = 4,
        SERVER_ALREADY_REGISTERED = 5,
        INVALID_COMMAND = 6
    }
}
