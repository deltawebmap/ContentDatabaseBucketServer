using DeltaWebMap.ContentDatabase;
using DeltaWebMap.ServerContentBucketServer.Framework;
using DeltaWebMap.ServerContentBucketServer.ServiceTemplates.User.Fetch;
using LibDeltaSystem;
using LibDeltaSystem.Entities.CommonNet;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DeltaWebMap.ServerContentBucketServer.Services.User.Server.Fetch
{
    public class UserServerFetchBucketStructuresDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/{SERVER_ID}/buckets/structures";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new UserServerFetchBucketStructures(conn, e);
        }
    }

    public class UserServerFetchBucketStructures : IUserFetchServer<NetStructure>
    {
        public UserServerFetchBucketStructures(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
            AddFormat(new BucketFormatBinaryStructures());
        }

        public override NetStructure ConvertObjectToWebType(DatabaseObject o, ulong object_id, ulong commit_id, int group_id, byte commit_type)
        {
            return new NetStructure
            {
                classname = (string)o["classname"],
                has_inventory = (bool)o["has_inventory"],
                location = ConversionTools.ConvertObjectToLocation((DatabaseObject)o["location"]),
                structure_id = (int)object_id,
                tribe_id = group_id
            };
        }

        public override string GetBucketName()
        {
            return "structures";
        }
    }

    class BucketFormatBinaryStructures : IBucketReadFormat
    {
        public string display_name => "Binary Structures";

        public string id => "structures_bin";

        public async Task Encode(Stream output, object[] data)
        {
            //First, create the name table
            List<string> nameTable = new List<string>();
            foreach(var d in data)
            {
                string name = ((NetStructure)d).classname;
                if (!nameTable.Contains(name))
                    nameTable.Add(name);
            }

            //Open buffer
            byte[] buffer = new byte[24];

            //Create header
            BitConverter.GetBytes((int)1397577540).CopyTo(buffer, 0);
            BitConverter.GetBytes((ushort)2).CopyTo(buffer, 4);
            BitConverter.GetBytes((ushort)nameTable.Count).CopyTo(buffer, 6);
            output.Write(buffer, 0, 8);

            //Write name table
            foreach(var n in nameTable)
            {
                byte[] b = Encoding.ASCII.GetBytes(n);
                output.Write(b, 0, b.Length);
                output.WriteByte(0x00);
            }

            //Write entries count
            BitConverter.GetBytes((ushort)data.Length).CopyTo(buffer, 0);
            output.Write(buffer, 0, 2);

            //Write entries
            foreach(var e in data)
            {
                NetStructure s = (NetStructure)e;
                byte flags = 0;
                if(s.has_inventory)
                    flags |= 1 << 0;
                BitConverter.GetBytes((ushort)nameTable.IndexOf(s.classname)).CopyTo(buffer, 0);
                buffer[2] = flags;
                buffer[3] = (byte)(s.location.yaw * 0.708333333f);
                BitConverter.GetBytes((int)s.structure_id).CopyTo(buffer, 4);
                BitConverter.GetBytes((int)s.tribe_id).CopyTo(buffer, 8);
                BitConverter.GetBytes((float)s.location.x).CopyTo(buffer, 12);
                BitConverter.GetBytes((float)s.location.y).CopyTo(buffer, 16);
                BitConverter.GetBytes((float)s.location.z).CopyTo(buffer, 20);
                output.Write(buffer, 0, 24);
            }
        }
    }
}
