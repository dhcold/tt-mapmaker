using System;
using System.Collections.Generic;

namespace WindowsFormsApp1 {
    class MapLayer {
        public int height;
        public List<Tuple<int, int>> points;

        public MapLayer(int height, List<Tuple<int, int>> points) {
            this.height = height;
            this.points = points;
        }
    }
}