using System;
using System.Collections.Generic;
using System.Numerics;

namespace WindowsFormsApp1 {
    public class Entity
    {
        public string name;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public List<Tuple<string, string>> properties;

        public Entity(
                string name,
                Vector3 position,
                Vector3 rotation,
                Vector3 scale,
                List<Tuple<string, string>> properties)
        {
            this.name = name;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
            this.properties = properties == null ? new List<Tuple<string, string>>() : properties;
        }
    }
}