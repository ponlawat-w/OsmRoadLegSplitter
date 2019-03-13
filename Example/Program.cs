using OsmRoadLegSplitter;
using OsmRoadLegSplitter.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Example
{
    class Program
    {
        private static Random Random = new Random();

        private static string RandomColor()
        {
            int r = Random.Next(0, 255);
            int g = Random.Next(0, 255);
            int b = Random.Next(0, 255);

            return string.Format("#{0:X2}{1:X2}{2:X2}", r, g, b);
        }

        static void Main(string[] args)
        {
            RoadLegSplitter splitter = new RoadLegSplitter("Data/thailand-latest.osm.pbf");
            List<RoadLeg> roadLegs = splitter.Split(
                98.35372924804688f,
                7.928674801364048f,
                98.43389511108398f,
                7.850798117127191f).ToList();

            List<string> coordinateStrings = new List<string>();
            foreach (RoadLeg leg in roadLegs)
            {
                coordinateStrings.Add(string.Join(',',
                    leg.RoadNodes.Select(n => $"[{n.Coordinate.X},{n.Coordinate.Y}]")));
            }

            string output = string.Join(',',
               coordinateStrings.Select(c => "{\"type\":\"Feature\",\"properties\":{\"stroke\":\"" + RandomColor() + "\",\"stroke-width\":2,\"stroke-opacity\":1},\"geometry\":{\"type\":\"LineString\",\"coordinates\":[" + c + "]}}"
            ));

            using (StreamWriter writer = new StreamWriter("output.json"))
            {
                writer.Write("{\"type\":\"FeatureCollection\",\"features\":[" + output + "]}");
                writer.Close();
            }
        }
    }
}
