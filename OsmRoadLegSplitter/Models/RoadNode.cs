using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.Text;

namespace OsmRoadLegSplitter.Models
{
    public class RoadNode
    {
        public long Id;
        public Coordinate Coordinate;

        public RoadNode(long id, Coordinate coordinate)
        {
            Id = id;
            Coordinate = coordinate;
        }
    }
}
