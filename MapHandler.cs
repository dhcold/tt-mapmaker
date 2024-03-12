using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace WindowsFormsApp1 {    

    class MapHandler {        

        public MapObject NewMap()
        {
            MapObject m = ParseMap("ttt.rbe");
            //Serialize(m, "map_data.bim");
            //MapObject m = Deserialize<MapObject>("map_data.xml");
            return m;
        }

        public static void Serialize(MapObject obj, string filePath)
        {
            // Convert IEnumerable<byte> properties to List<byte>
            obj.u1 = obj.u1?.ToList();
            obj.u2 = obj.u2?.ToList();
            obj.u4 = obj.u4?.ToList();
            obj.u5 = obj.u5?.ToList();

            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);
            }
        }

        public static MapObject Deserialize(string filePath)
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                MapObject obj = (MapObject)formatter.Deserialize(stream);

                // Convert List<byte> properties back to IEnumerable<byte>
                obj.u1 = obj.u1?.AsEnumerable();
                obj.u2 = obj.u2?.AsEnumerable();
                obj.u4 = obj.u4?.AsEnumerable();
                obj.u5 = obj.u5?.AsEnumerable();

                return obj;
            }
        }

        // Parse an .rbe file into a MapObject
        public MapObject ParseMap(string path) {
            MapObject m = new MapObject();
            using (BinaryReader br_template = new BinaryReader(File.Open(path, FileMode.Open))) {

                // REBM
                m.rebm = new string(br_template.ReadChars(4));

                // VERSION
                m.ver = br_template.ReadInt32();

                // U1
                m.u1 = br_template.ReadBytes(20);

                // Save the remaining gzipped portion temporarily
                List<byte> remainder = new List<byte>();
                for (int i = 0; i < br_template.BaseStream.Length - 28; i++) {
                    remainder.Add(br_template.ReadByte());
                }
                using (FileStream fs = new FileStream("temp.x", FileMode.Create)) {
                    foreach (byte bb in remainder) {
                        fs.WriteByte(bb);
                    }
                }

                // Decompress the gzipped portion
                using (FileStream fs = new FileStream("temp.x", FileMode.Open)) {
                    using (GZipStream gs = new GZipStream(fs, CompressionMode.Decompress)) {
                        using (FileStream de = File.Create("temp.y")) {
                            gs.CopyTo(de);
                        }
                    }
                }

                // Open the decompressed remainder and parse it
                using (BinaryReader br = new BinaryReader(File.Open("temp.y", FileMode.Open))) {

                    // MATERIALS
                    int c = br.ReadByte() - 1;
                    for (int i = 0; i < c; i++) {
                        string name = new string(br.ReadChars(br.ReadInt32()));
                        m.AddMaterial(name);
                    }

                    // U2
                    m.u2 = br.ReadBytes(4);

                    // BLOCKS
                    c = br.ReadInt32();
                    int minx = int.MaxValue; int miny = int.MaxValue; int minz = int.MaxValue;
                    int maxx = int.MinValue; int maxy = int.MinValue; int maxz = int.MinValue;
                    List<Block> blocks = new List<Block>();
                    for (int i = 0; i < c; i++) {
                        Block b = new Block(
                            new Vector3 (br.ReadInt32(), br.ReadInt32(), br.ReadInt32()),
                            type: br.ReadByte(),
                            u1: br.ReadBytes(12),
                            materials: new Dictionary<string, int>() {
                                ["front"] = br.ReadByte(),
                                ["left"] = br.ReadByte(),
                                ["back"] = br.ReadByte(),
                                ["right"] = br.ReadByte(),
                                ["top"] = br.ReadByte(),
                                ["bottom"] = br.ReadByte(),
                            },
                            u2: br.ReadBytes(1),
                            material_offsets: new Dictionary<string, Dictionary<string, int>>() {
                                ["front"] = new Dictionary<string, int>() {
                                    ["x"] = br.ReadByte(),
                                    ["y"] = br.ReadByte(),
                                    ["r"] = br.ReadByte(),
                                },
                                ["left"] = new Dictionary<string, int>() {
                                    ["x"] = br.ReadByte(),
                                    ["y"] = br.ReadByte(),
                                    ["r"] = br.ReadByte(),
                                },
                                ["back"] = new Dictionary<string, int>() {
                                    ["x"] = br.ReadByte(),
                                    ["y"] = br.ReadByte(),
                                    ["r"] = br.ReadByte(),
                                },
                                ["right"] = new Dictionary<string, int>() {
                                    ["x"] = br.ReadByte(),
                                    ["y"] = br.ReadByte(),
                                    ["r"] = br.ReadByte(),
                                },
                                ["top"] = new Dictionary<string, int>() {
                                    ["x"] = br.ReadByte(),
                                    ["y"] = br.ReadByte(),
                                    ["r"] = br.ReadByte(),
                                },
                                ["bottom"] = new Dictionary<string, int>() {
                                    ["x"] = br.ReadByte(),
                                    ["y"] = br.ReadByte(),
                                    ["r"] = br.ReadByte(),
                                },
                            },
                            orient: br.ReadByte(),
                            u3: br.ReadBytes(1)
                            );
                        blocks.Add(b);
                        minx = b.x < minx ? b.x : minx;
                        miny = b.y < miny ? b.y : miny;
                        minz = b.z < minz ? b.z : minz;
                        maxx = b.x > maxx ? b.x : maxx;
                        maxy = b.y > maxy ? b.y : maxy;
                        maxz = b.z > maxz ? b.z : maxz;
                    }
                    m.bounds_minx = minx; m.bounds_maxx = maxx;
                    m.bounds_miny = miny; m.bounds_maxy = maxy;
                    m.bounds_minz = minz; m.bounds_maxz = maxz;
                    // Add blocks to the list
                    m.blocks_list = blocks;

                    // U3
                    c = br.ReadInt32();
                    IEnumerable<byte> d = null;
                    for (int i = 0; i < c; i++) {
                        IEnumerable<byte> bytes = br.ReadBytes(16);
                        d = (d == null) ? bytes : d.Concat(bytes);
                    }
                    m.u3 = BitConverter.GetBytes(c).Concat(d);

                    // ENTITIES
                    c = br.ReadInt32();
                    for (int i = 0; i < c; i++) {
                        string name = new string(br.ReadChars(br.ReadInt32()));
                        float[] n = {
                            br.ReadSingle(),
                            br.ReadSingle(),
                            br.ReadSingle(),
                            RadToDeg(br.ReadSingle()),
                            RadToDeg(br.ReadSingle()),
                            RadToDeg(br.ReadSingle()),
                            br.ReadSingle(),
                            br.ReadSingle(),
                            br.ReadSingle(),
                        };
                        List<Tuple<string, string>> props = new List<Tuple<string, string>>();
                        int c2 = br.ReadInt32();
                        for (int j = 0; j < c2; j++) {
                            string key = new string(br.ReadChars(br.ReadInt32()));
                            string val = new string(br.ReadChars(br.ReadInt32()));
                            props.Add(new Tuple<string, string>(key, val));
                        }
                        m.AddEntity(
                            name,
                            n[0], n[1], n[2], n[3], n[4], n[5], n[6], n[7], n[8],
                            props);
                    }

                    // AUDIO
                    c = br.ReadInt32();
                    d = null;
                    for (int i = 0; i < c; i++) {
                        IEnumerable<byte> bytes = br.ReadBytes(12);
                        d = (d == null) ? bytes : d.Concat(bytes);

                        int c2 = br.ReadInt32();
                        IEnumerable<byte> d2 = null;
                        for (int j = 0; j < c2; j++) {
                            IEnumerable<byte> bytes2 = br.ReadBytes(12);
                            d2 = (d2 == null) ? bytes2 : d2.Concat(bytes2);
                        }
                        d = d.Concat(BitConverter.GetBytes(c2)).Concat(d2);
                    }
                    m.audio = BitConverter.GetBytes(c).Concat(d);

                    // U4
                    m.u4 = br.ReadBytes(4);

                    // MINIMAP LAYERS
                    c = br.ReadInt32();
                    for (int i = 0; i < c; i++) {
                        int height = br.ReadInt32();

                        int c2 = br.ReadInt32();
                        List<Tuple<int, int>> points = new List<Tuple<int, int>>();
                        for (int j = 0; j < c2; j++) {
                            points.Add(new Tuple<int, int>(br.ReadInt32(), br.ReadInt32()));
                        }
                        m.AddMapLayer(height, points);
                    }

                    // U5 (+i_)
                    m.u5 = br.ReadBytes((int)br.BaseStream.Length);
                }
            }

            File.Delete("temp.x");
            File.Delete("temp.y");

            return m;
        }
        
        // Write a MapObject to an .rbe file
        public bool WriteMap(MapObject m, string path) {

            // Write header to new file
            using (FileStream fs = new FileStream(path, FileMode.Create)) {

                // REBM
                fs.Write(EncStr(m.rebm), 0, 4);

                // VERSION
                fs.Write(EncNum(m.ver), 0, 4);

                // U1
                fs.Write(m.u1.ToArray(), 0, 20);
            }

            // Write remainder to temp.x
            using (FileStream fs = new FileStream("temp.x", FileMode.Create)) {

                // MATERIALS
                fs.Write(EncNum(m.materials.Count + 1), 0, 1);
                foreach (Material mat in m.materials) {
                    fs.Write(EncNum(mat.name.Length), 0, 4);
                    fs.Write(EncStr(mat.name), 0, mat.name.Length);
                }

                // U2
                fs.Write(m.u2.ToArray(), 0, 4);

                // BLOCKS
                fs.Write(EncNum(m.blocks_list.Count()), 0, 4);
                foreach (Block b in m.blocks_list) {
                    fs.Write(EncNum(b.x), 0, 4);
                    fs.Write(EncNum(b.y), 0, 4);
                    fs.Write(EncNum(b.z), 0, 4);
                    fs.Write(EncNum(b.type), 0, 1);
                    fs.Write(b.u1.ToArray(), 0, 12);
                    fs.Write(EncNum(b.materials["front"]), 0, 1);
                    fs.Write(EncNum(b.materials["left"]), 0, 1);
                    fs.Write(EncNum(b.materials["back"]), 0, 1);
                    fs.Write(EncNum(b.materials["right"]), 0, 1);
                    fs.Write(EncNum(b.materials["top"]), 0, 1);
                    fs.Write(EncNum(b.materials["bottom"]), 0, 1);
                    fs.Write(b.u2.ToArray(), 0, 1);
                    fs.Write(EncNum(b.material_offsets["front"]["x"]), 0, 1);
                    fs.Write(EncNum(b.material_offsets["front"]["y"]), 0, 1);
                    fs.Write(EncNum(b.material_offsets["front"]["r"]), 0, 1);
                    fs.Write(EncNum(b.material_offsets["left"]["x"]), 0, 1);
                    fs.Write(EncNum(b.material_offsets["left"]["y"]), 0, 1);
                    fs.Write(EncNum(b.material_offsets["left"]["r"]), 0, 1);
                    fs.Write(EncNum(b.material_offsets["back"]["x"]), 0, 1);
                    fs.Write(EncNum(b.material_offsets["back"]["y"]), 0, 1);
                    fs.Write(EncNum(b.material_offsets["back"]["r"]), 0, 1);
                    fs.Write(EncNum(b.material_offsets["right"]["x"]), 0, 1);
                    fs.Write(EncNum(b.material_offsets["right"]["y"]), 0, 1);
                    fs.Write(EncNum(b.material_offsets["right"]["r"]), 0, 1);
                    fs.Write(EncNum(b.material_offsets["top"]["x"]), 0, 1);
                    fs.Write(EncNum(b.material_offsets["top"]["y"]), 0, 1);
                    fs.Write(EncNum(b.material_offsets["top"]["r"]), 0, 1);
                    fs.Write(EncNum(b.material_offsets["bottom"]["x"]), 0, 1);
                    fs.Write(EncNum(b.material_offsets["bottom"]["y"]), 0, 1);
                    fs.Write(EncNum(b.material_offsets["bottom"]["r"]), 0, 1);
                    fs.Write(EncNum(b.orient), 0, 1);
                    fs.Write(b.u3.ToArray(), 0, 1);
                }

                // U3
                fs.Write(m.u3.ToArray(), 0, m.u3.Count());

                // ENTITIES
                fs.Write(EncNum(m.entities.Count()), 0, 4);
                foreach (Entity e in m.entities) {
                    fs.Write(EncNum(e.name.Length), 0, 4);
                    fs.Write(EncStr(e.name), 0, e.name.Length);
                    fs.Write(EncNum(e.position.X), 0, 4);
                    fs.Write(EncNum(e.position.Y), 0, 4);
                    fs.Write(EncNum(e.position.Z), 0, 4);
                    fs.Write(EncNum(DegToRad(e.rotation.X)), 0, 4);
                    fs.Write(EncNum(DegToRad(e.rotation.Y)), 0, 4);
                    fs.Write(EncNum(DegToRad(e.rotation.Z)), 0, 4);
                    fs.Write(EncNum(e.scale.X), 0, 4);
                    fs.Write(EncNum(e.scale.Y), 0, 4);
                    fs.Write(EncNum(e.scale.Z), 0, 4);
                    fs.Write(EncNum(e.properties.Count()), 0, 4);
                    foreach (Tuple<string, string> p in e.properties) {
                        fs.Write(EncNum(p.Item1.Length), 0, 4);
                        fs.Write(EncStr(p.Item1), 0, p.Item1.Length);
                        fs.Write(EncNum(p.Item2.Length), 0, 4);
                        fs.Write(EncStr(p.Item2), 0, p.Item2.Length);
                    }
                }

                // AUDIO
                fs.Write(m.audio.ToArray(), 0, m.audio.Count());

                // U4
                fs.Write(m.u4.ToArray(), 0, m.u4.Count());

                // MINIMAP LAYERS
                fs.Write(EncNum(m.map_layers.Count()), 0, 4);
                foreach (MapLayer ml in m.map_layers) {
                    fs.Write(EncNum(ml.height), 0, 4);
                    fs.Write(EncNum(ml.points.Count()), 0, 4);
                    foreach (Tuple<int, int> p in ml.points) {
                        fs.Write(EncNum(p.Item1), 0, 4);
                        fs.Write(EncNum(p.Item2), 0, 4);
                    }
                }

                // U5 (+i_)
                fs.Write(m.u5.ToArray(), 0, m.u5.Count());
            }

            // Compress and write to temp.y
            using (FileStream fs = new FileStream("temp.y", FileMode.Create)) {
                using (GZipStream gs = new GZipStream(fs, CompressionMode.Compress)) {
                    using (FileStream co = new FileStream("temp.x", FileMode.Open)) {
                        co.CopyTo(gs);
                    }
                }
            }

            // Append temp.y to path map file
            using (FileStream fs = new FileStream(path, FileMode.Append)) {
                using (FileStream co = new FileStream("temp.y", FileMode.Open)) {
                    co.CopyTo(fs);
                }
            }

            File.Delete("temp.x");
            File.Delete("temp.y");

            return true;
        }

        // These functions encode & order bytes according to the system architecture
        private byte[] EncNum(int i) {
            byte[] b = BitConverter.GetBytes(i);
            if (!BitConverter.IsLittleEndian) Array.Reverse(b);
            return b;
        }
        private byte[] EncNum(float i) {
            byte[] b = BitConverter.GetBytes(i);
            if (!BitConverter.IsLittleEndian) Array.Reverse(b);
            return b;
        }
        private byte[] EncStr(string s) {
            byte[] b = Encoding.ASCII.GetBytes(s);
            if (!BitConverter.IsLittleEndian) Array.Reverse(b);
            return b;
        }

        // Convert to/from radians/degrees
        private float DegToRad(float deg) {
            return (float)(Math.PI / 180.0 * deg);
        }
        private float RadToDeg(float rad) {
            return (float)(rad * 180.0 / Math.PI);
        }
    }
}
