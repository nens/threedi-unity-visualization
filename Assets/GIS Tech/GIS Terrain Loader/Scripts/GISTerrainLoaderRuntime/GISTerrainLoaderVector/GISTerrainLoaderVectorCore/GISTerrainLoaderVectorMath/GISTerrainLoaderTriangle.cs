using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public struct GISTerrainLoaderTriangle3
    {
        public DVector2 p1;
        public DVector2 p2;
        public DVector2 p3;

        public GISTerrainLoaderTriangle3(DVector2 p1, DVector2 p2, DVector2 p3)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }
        public void ChangeOrientation()
        {
            DVector2 temp = this.p1;

            this.p1 = this.p2;

            this.p2 = temp;
        }
    }
    public struct GISTerrainLoaderTriangle2
    {
        public DVector2 p1;
        public DVector2 p2;
        public DVector2 p3;

        public GISTerrainLoaderTriangle2(DVector2 p1, DVector2 p2, DVector2 p3)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }
        public void ChangeOrientation()
        {
            DVector2 temp = this.p1;

            this.p1 = this.p2;

            this.p2 = temp;
        }
        public float MinX()
        {
            return Mathf.Min((float)p1.x, Mathf.Min((float)p2.x, (float)p3.x));
        }

        public float MaxX()
        {
            return Mathf.Max((float)p1.x, Mathf.Max((float)p2.x, (float)p3.x));
        }

        public float MinY()
        {
            return Mathf.Min((float)p1.y, Mathf.Min((float)p2.y, (float)p3.y));
        }

        public float MaxY()
        {
            return Mathf.Max((float)p1.y, Mathf.Max((float)p2.y, (float)p3.y));
        }
    }
}
