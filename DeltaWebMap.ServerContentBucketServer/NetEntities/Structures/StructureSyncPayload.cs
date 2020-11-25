using LibDeltaSystem.Db.Content;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.ServerContentBucketServer.NetEntities.Structures
{
    public class StructureSyncPayload
    {
        public string classname;
        public int tribe;
        public DbLocation location;
        public float health;
        public float max_health;
        public int id;
    }
}
