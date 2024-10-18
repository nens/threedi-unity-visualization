
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class WayPoints : MonoBehaviour
    {
        public bool RandomPoints = false;
        public int  PointsNumber = 0;
        void Start()
        {
       
        }
         // (Lat-Lon-Elevation(m))
        public List<DVector3> RealWorldPoints = new List<DVector3>();

        // (Space Positions)
        [HideInInspector]
        public List<Vector3> UnityWorldSpacePoints = new List<Vector3>();

        public void ConvertLatLonToSpacePosition (GISTerrainContainer Container,bool InstantiateGameObjects)
        {
            if (RandomPoints)
            {
                RealWorldPoints.Clear();

                for(int i = 0; i<=PointsNumber;i++)
                {
                    float x = Random.Range((float)Container.data.TLOriginal_Coor.x, (float)Container.data.DROriginal_Coor.x);
                    float y = Random.Range((float)Container.data.TLOriginal_Coor.y, (float)Container.data.DROriginal_Coor.y);
                    float z = Random.Range(Container.data.MinMaxElevation.y - 30, Container.data.MinMaxElevation.y + 30);
                    
                    RealWorldPoints.Add(new DVector3(x, y, z));
                }
                
                

            }

            UnityWorldSpacePoints = new List<Vector3>();

            this.transform.DestroyChildren();

            foreach (var point in RealWorldPoints)
            {
                var spaceP = GISTerrainLoaderGeoConversion.RealWorldCoordinatesToUnityWorldSpace(Container, point.ToDVector2(), (float)point.z,SetElevationMode.RelativeToSeaLevel);

                UnityWorldSpacePoints.Add(spaceP);

                if(InstantiateGameObjects)
                {
                    var p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    p.name = "Point_"+ RealWorldPoints.IndexOf(point).ToString();
                    p.transform.position = spaceP;
                    p.transform.parent = this.transform;
                }
            }

        }

    }
}