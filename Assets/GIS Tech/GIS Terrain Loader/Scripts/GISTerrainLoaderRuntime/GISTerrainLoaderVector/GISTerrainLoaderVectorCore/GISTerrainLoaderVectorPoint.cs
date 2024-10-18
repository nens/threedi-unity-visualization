/*     Unity GIS Tech 2020-2023      */


using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderVectorPoint : MonoBehaviour
    {
        public GISTerrainLoaderPointGeoData PointGeoData;
 
        public void UpdateGeoDataFrom(GISTerrainContainer Container,CoordinatesSource coordinatesSource= CoordinatesSource.FromGeoDataScript, bool StoreElevationValue = false, RealWorldElevation ElevationMode = RealWorldElevation.Elevation)
        {
            switch(coordinatesSource)
            {
                case CoordinatesSource.FromTerrain:
                    var RWPosition = GISTerrainLoaderGeoConversion.UnityWorldSpaceToRealWorldCoordinates(this.transform.position, Container, StoreElevationValue, ElevationMode);
                    PointGeoData.GeoPoint = RWPosition.ToDVector2();
                    
                    if (StoreElevationValue)
                        PointGeoData.Elevation = (float)RWPosition.z;
                    break;
                case CoordinatesSource.FromGeoDataScript:
                    //
                    break;
            }
        }
    }
}