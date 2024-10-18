using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public struct GISTerrainLoaderAABB2
    {
        public double minX;
        public double maxX;
        public double minY;
        public double maxY;
        public GISTerrainLoaderAABB2(float minX, float maxX, float minY, float maxY)
        {
            this.minX = minX;
            this.maxX = maxX;
            this.minY = minY;
            this.maxY = maxY;
        }
        public GISTerrainLoaderAABB2(List<DVector2> points)
        {
            DVector2 p1 = points[0];

            double minX = p1.x;
            double maxX = p1.x;
            double minY = p1.y;
            double maxY = p1.y;

            for (int i = 1; i < points.Count; i++)
            {
                DVector2 p = points[i];

                if (p.x < minX)
                {
                    minX = p.x;
                }
                else if (p.x > maxX)
                {
                    maxX = p.x;
                }

                if (p.y < minY)
                {
                    minY = p.y;
                }
                else if (p.y > maxY)
                {
                    maxY = p.y;
                }
            }

            this.minX = minX;
            this.maxX = maxX;
            this.minY = minY;
            this.maxY = maxY;
        }
    }
}
