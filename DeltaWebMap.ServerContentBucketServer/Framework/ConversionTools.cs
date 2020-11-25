using DeltaWebMap.ContentDatabase;
using LibDeltaSystem.Db.Content;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.ServerContentBucketServer.Framework
{
    public static class ConversionTools
    {
        public static string TrimArkClassname(string name)
        {
            if (name.EndsWith("_C"))
                return name.Substring(0, name.Length - 2);
            return name;
        }

        public static ulong GetMultipartID(int u1, int u2)
        {
            return DbDino.ZipperDinoId((uint)u1, (uint)u2);
        }

        public static WriteObject ConvertLocationToWriteObject(DbLocation loc)
        {
            WriteObject o = new WriteObject();
            o.WriteFloat("x", loc.x);
            o.WriteFloat("y", loc.y);
            o.WriteFloat("z", loc.z);
            o.WriteFloat("pitch", loc.pitch);
            o.WriteFloat("yaw", loc.yaw);
            o.WriteFloat("roll", loc.roll);
            return o;
        }

        public static DbLocation ConvertObjectToLocation(DatabaseObject obj)
        {
            return new DbLocation
            {
                x = (float)obj["x"],
                y = (float)obj["y"],
                z = (float)obj["z"],
                pitch = (float)obj["pitch"],
                roll = (float)obj["roll"],
                yaw = (float)obj["yaw"]
            };
        }
    }
}
