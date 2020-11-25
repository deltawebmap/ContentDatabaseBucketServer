using LibDeltaSystem.Db.Content;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.ServerContentBucketServer.NetEntities.Dinos
{
    public class DinoSyncPayload
    {
        public int tribe_id;
        public int id_1; //actually a uint
        public int id_2; //actually a uint
        public bool is_female;
        public int[] colors;
        public string name;
        public string tamer;
        public string classname;
        public float[] max_stats;
        public float[] current_stats;
        public int[] points_wild;
        public int[] points_tamed;
        public int extra_level;
        public int base_level;
        public float experience;
        public bool baby;
        public float baby_age;
        public double next_cuddle;
        public float imprint_quality;
        public DbLocation location;
        public string status;
    }
}
