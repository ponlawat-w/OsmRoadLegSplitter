using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using OsmRoadLegSplitter.Models;
using OsmSharp;
using OsmSharp.Geo;
using OsmSharp.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OsmRoadLegSplitter
{
    public class RoadLegSplitter
    {
        FileStream _stream;

        public RoadLegSplitter(FileStream stream)
        {
            _stream = stream;
        }
        public RoadLegSplitter(string filePath): this(new FileInfo(filePath).OpenRead()) { }

        public void CloseStream()
        {
            _stream.Close();
        }

        private IEnumerable<RoadLeg> ProcessSplit(OsmStreamSource source)
        {
            List<OsmGeo> osmGeos = source.ToList();

            IDictionary<long, RoadNode> nodes = osmGeos
                .Where(geo => geo.Type == OsmGeoType.Node)
                .Select(geo => (Node)geo)
                .Where(node => node.Id != null && node.Latitude != null && node.Longitude != null)
                .Select(node => new RoadNode(node.Id.Value, new Coordinate(
                    node.Longitude.Value, node.Latitude.Value)))
                .ToDictionary(roadNode => roadNode.Id, roadNode => roadNode);

            List<RoadLeg> roadLegs = new List<RoadLeg>();
            List<Way> tempWays = osmGeos.Where(geo => geo.Type == OsmGeoType.Way
                    && geo.Tags != null && geo.Tags.ContainsKey("highway"))
                .Select(geo => (Way)geo).ToList();

            IDictionary<long, long> nodeWayCount = new Dictionary<long, long>();

            List<Way> ways = new List<Way>();

            foreach (Way way in tempWays)
            {
                bool nodeIsComplete = true;

                foreach (long nodeId in way.Nodes)
                {
                    if (!nodes.ContainsKey(nodeId))
                    {
                        nodeIsComplete = false;
                        continue;
                    }

                    nodeWayCount[nodeId] = nodeWayCount.ContainsKey(nodeId) ?
                        nodeWayCount[nodeId] + 1 : 1;
                }
                
                if (nodeIsComplete)
                {
                    ways.Add(way);
                }
            }

            foreach (Way way in ways)
            {
                List<RoadNode> roadNodes = new List<RoadNode>();
                foreach (long nodeId in way.Nodes)
                {
                    long nodeInAllWayCount = nodeWayCount[nodeId];

                    if (nodeInAllWayCount > 1 && roadNodes.Count() > 0)
                    {
                        roadNodes.Add(nodes[nodeId]);
                        roadLegs.Add(new RoadLeg(roadNodes));
                        roadNodes.Clear();
                    }
                    roadNodes.Add(nodes[nodeId]);
                }

                if (roadNodes.Count() > 1)
                {
                    roadLegs.Add(new RoadLeg(roadNodes));
                }
                roadNodes.Clear();
            }

            return roadLegs;
        }

        public IEnumerable<RoadLeg> Split()
        {
            return ProcessSplit(new PBFOsmStreamSource(_stream));
        }

        public IEnumerable<RoadLeg> Split(
            float filteredLeft,
            float filteredTop,
            float filteredRight,
            float filteredBottom)
        {
            return ProcessSplit(new PBFOsmStreamSource(_stream).FilterBox(
                filteredLeft, filteredTop, filteredRight, filteredBottom));
        }

        public IEnumerable<RoadLeg> Split(Coordinate topLeft, Coordinate bottomRight)
        {
            return ProcessSplit(new PBFOsmStreamSource(_stream).FilterBox(
                (float)topLeft.X, (float)topLeft.Y, (float)bottomRight.X, (float)bottomRight.Y));
        }
    }
}
