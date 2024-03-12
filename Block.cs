using System.Collections.Generic;
using System.Numerics;

namespace WindowsFormsApp1 {
    class Block {
        public int x, y, z, type, orient;
        public IEnumerable<byte> u1, u2, u3;
        public Dictionary<string, int> materials;
        public Dictionary<string, Dictionary<string, int>> material_offsets = new Dictionary<string, Dictionary<string, int>>()
        {
            ["front"] = new Dictionary<string, int>() { ["x"] = 0, ["y"] = 0, ["r"] = 0 },
            ["left"] = new Dictionary<string, int>() { ["x"] = 0, ["y"] = 0, ["r"] = 0 },
            ["back"] = new Dictionary<string, int>() { ["x"] = 0, ["y"] = 0, ["r"] = 0 },
            ["right"] = new Dictionary<string, int>() { ["x"] = 0, ["y"] = 0, ["r"] = 0 },
            ["top"] = new Dictionary<string, int>() { ["x"] = 0, ["y"] = 0, ["r"] = 0 },
            ["bottom"] = new Dictionary<string, int>() { ["x"] = 0, ["y"] = 0, ["r"] = 0 },
        };

        public Block(
                Vector3 position,
                int type,
                IEnumerable<byte> u1,
                Dictionary<string, int> materials,
                IEnumerable<byte> u2,
                Dictionary<string, Dictionary<string, int>> material_offsets,
                int orient,
                IEnumerable<byte> u3) {
            this.x = (int)position.X;
            this.y = (int)position.Y;
            this.z = (int)position.Z;
            this.type = type;
            this.orient = orient;
            this.materials = materials;
            //this.material_offsets = material_offsets;
            this.u1 = u1 == null ? new byte[12] : u1;
            this.u2 = u2 == null ? new byte[1] : u2;
            this.u3 = u3 == null ? new byte[1] : u3;
        }
    }
}