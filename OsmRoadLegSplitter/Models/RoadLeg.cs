using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OsmRoadLegSplitter.Models
{
    public class RoadLeg
    {
        public IEnumerable<RoadNode> RoadNodes;

        public RoadLeg(IEnumerable<RoadNode> nodes)
        {
            RoadNodes = nodes.Select(n => n).ToList();
        }

        public LineString ToLineString()
        {
            return new LineString(RoadNodes.Select(n => n.Coordinate).ToArray());
        }
    }
}
