using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.ServerContentBucketServer.NetEntities.Inventories
{
    public class InventorySyncItem
    {
        public int i1;
        public int i2;
        public string c; //Classname
        public float d; //Durability
        public int q; //Stack size

        public string cname; //Custom name
        public bool tek; //Is tek
        public bool blp; //Is blueprint
    }
}
