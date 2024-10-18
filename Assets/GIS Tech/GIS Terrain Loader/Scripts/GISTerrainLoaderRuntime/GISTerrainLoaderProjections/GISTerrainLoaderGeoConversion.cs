/*     Unity GIS Tech 2020-2023      */


using System;
using System.Text.RegularExpressions;
using UnityEngine;
#if DotSpatial
using DotSpatial.Projections;
#endif

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderGeoConversion
    {
        #region Useful API

        /// <summary>
        /// Convert Unity World space (X,Y,E) coordinates to Real World Coordinates
        /// </summary>
        /// <param name="position">SpacePosition</param>
        /// <param name="container">Terrain Container Object</param>
        /// <param name="GetElevation">True To Get Elevation value in Z</param>
        /// <param name="elevationMode"></param>
        /// <returns></returns>
        public static DVector3 UnityWorldSpaceToRealWorldCoordinates(Vector3 position, GISTerrainContainer container,bool GetElevation = false ,RealWorldElevation elevationMode = RealWorldElevation.Altitude)
        {
             DVector3 RealWorldPosition = new DVector3(0,0,0);
             DVector2 RWPos = new DVector2(0, 0);
             float elevation = 0;

            if (container.IncludeSpacePosition(position))
            {
                if (container.data.EPSG != 0)
                {
                    RWPos = UnityWorldSpaceToEPSGCoordinates(position, container);

                }
                else
                {
                    RWPos = UnityWorldSpaceToGeographicCoordinates(position, container);
                }

                if(GetElevation)
                    elevation = GetRealWorldElevation(container, position, elevationMode);
              
            }

            RealWorldPosition = new DVector3(RWPos.x, RWPos.y, elevation);

            return RealWorldPosition;
        }
        private static DVector2 UnityWorldSpaceToGeographicCoordinates(Vector3 position, GISTerrainContainer container)
        {
            var Coor_Size_X = container.data.DRPoint_LatLon.x - container.data.TLPoint_LatLon.x;
            var Coor_Size_Y = container.data.TLPoint_LatLon.y - container.data.DLPoint_LatLon.y;

            var Pos_x = (position.x * Coor_Size_X / container.ContainerSize.x) + container.data.TLPoint_LatLon.x;
            var Pos_y = (position.z * Coor_Size_Y / container.ContainerSize.z) + container.data.DLPoint_LatLon.y;

            DVector2 geoLocation = new DVector2(Pos_x, Pos_y);

            return geoLocation;
        }
        private static DVector2 UnityWorldSpaceToEPSGCoordinates(Vector3 position, GISTerrainContainer container)
        {
            var Coor_Size_X = container.data.DROriginal_Coor.x - container.data.TLOriginal_Coor.x;
            var Coor_Size_Y = container.data.TLOriginal_Coor.y - container.data.DROriginal_Coor.y;

            var Pos_x = (position.x * Coor_Size_X / container.ContainerSize.x) + container.data.TLOriginal_Coor.x;
            var Pos_y = (position.z * Coor_Size_Y / container.ContainerSize.z) + container.data.DROriginal_Coor.y;

            DVector2 geoLocation = new DVector2(Pos_x, Pos_y);

            return geoLocation;
        }




        /// <summary>
        /// Set GameObject position by converting real world coordinates to unity space position (Inputs Lat/Lon position + Elevation [m])
        /// </summary>
        /// <param name="container">Generated Terrain Container</param>
        /// <param name="RealWorldCoor"> Real World Position</param>
        /// <param name="RWElevationValue"> Elevation in m </param>
        /// <param name="elevationMode"> Check the documentation.. </param>
        /// <returns></returns>
        public static Vector3 RealWorldCoordinatesToUnityWorldSpace(GISTerrainContainer container, DVector2 RealWorldCoor, float RWElevationValue=0, SetElevationMode elevationMode = SetElevationMode.OnTheGround, float Scale = 1)
        {
            Vector3 SpacePositon = Vector3.zero;

            Vector3 SpacePos = Vector3.zero;

            bool PointInculded = false;

            if (container.data.EPSG != 0)
            {
                SpacePos = EPSGToUnityWorldSpace(RealWorldCoor, container);
 
                if (container.IncludeEPSGPoint(RealWorldCoor))
                    PointInculded = true;
            }
            else
            {

                SpacePos = LatLonToUnityWorldSpace(RealWorldCoor, container);
 
                if (container.IncludePoint(RealWorldCoor))
                    PointInculded = true;
            }

            if (PointInculded)
            {
                float RWElevation = 0;

                switch (elevationMode)
                {
                    case SetElevationMode.OnTheGround:
                        SpacePos = new Vector3(SpacePos.x, 5000, SpacePos.z);
                        RWElevation = GetHeight(SpacePos);
                        SpacePositon = new Vector3(SpacePos.x, RWElevation, SpacePos.z);
                        break;
                    case SetElevationMode.RelativeToSeaLevel:
                        RWElevation = container.data.GetElevation(RealWorldCoor);
                        SpacePos = new Vector3(SpacePos.x, 5000, SpacePos.z);
                        var Space_Y = GetHeight(SpacePos);
                        var RWE_Diff = (RWElevationValue - RWElevation) * container.Scale.y;
                        SpacePositon = new Vector3(SpacePos.x, Space_Y + RWE_Diff, SpacePos.z);
                        break;
                    case SetElevationMode.RelativeToTheGround:
                        Space_Y = SpacePos.y + (RWElevationValue * container.Scale.y * Scale);
                        SpacePositon = new Vector3(SpacePos.x, Space_Y, SpacePos.z);
                        break;
                }
            }
            return SpacePositon;
        }
        private static Vector3 EPSGToUnityWorldSpace(DVector2 Coor, GISTerrainContainer container, bool GetElevation = true)
        {
            Vector3 UnityWorldSpacePosition = Vector3.zero;

            if (container)
            {
                var Coor_Size_X = container.data.DROriginal_Coor.x - container.data.TLOriginal_Coor.x;
                var Coor_Size_Y = container.data.TLOriginal_Coor.y - container.data.DROriginal_Coor.y;

                double Pos_x = container.ContainerSize.x - (((container.data.DROriginal_Coor.x - Coor.x) * container.ContainerSize.x) / Coor_Size_X);
                double Pos_y = (container.ContainerSize.z - (((container.data.TLOriginal_Coor.y - Coor.y) * container.ContainerSize.z) / Coor_Size_Y));

                UnityWorldSpacePosition = new Vector3((float)(Pos_x), 0, (float)(Pos_y));
 
            }
            else
            {
                Debug.LogError("No Terrain Existing");

                return Vector3.zero;
            }

            return UnityWorldSpacePosition;
        }
        private static Vector3 LatLonToUnityWorldSpace(DVector2 latlon, GISTerrainContainer container, bool GetElevation = true)
        {
            if (container)
            {
                Vector3 UnityWorldSpacePosition = Vector3.zero;

                var Coor_Size_X = container.data.DRPoint_LatLon.x - container.data.TLPoint_LatLon.x;
                var Coor_Size_Y = container.data.TLPoint_LatLon.y - container.data.DLPoint_LatLon.y;
 
                var Pos_x = (container.ContainerSize.x * (latlon.x - container.data.TLPoint_LatLon.x)) / Coor_Size_X;

                var Pos_y = (container.ContainerSize.z * (latlon.y - container.data.DLPoint_LatLon.y)) / Coor_Size_Y;

                UnityWorldSpacePosition = new Vector3((float)(Pos_x), 0, (float)(Pos_y));
 
                return UnityWorldSpacePosition;

            }
            else
            {
                Debug.LogError("No Terrain Existing");

                return Vector3.zero;
            }

        }



        /// <summary>
        /// Get Real World Elevation [m] of a gameoject (in unity world space)
        /// </summary>
        /// <param name="container"Generated Terrain Container></param>
        /// <param name="ObjectPosition" GameObject Position in Unity World Space></param>
        /// <returns></returns>
        public static float GetRealWorldElevation(GISTerrainContainer container, Vector3 ObjectPosition, RealWorldElevation elevationMode = RealWorldElevation.Altitude)
        {
            float elevation = 0;

            var RealWorldCoor = GISTerrainLoaderGeoConversion.UnityWorldSpaceToRealWorldCoordinates(ObjectPosition, container).ToDVector2();

            bool PointInculded = container.IncludeRealWorldPoint(RealWorldCoor);
 
            if (PointInculded)
            {
                switch (elevationMode)
                {
                    case RealWorldElevation.Elevation:
                        elevation = container.data.GetElevation(RealWorldCoor);
                        break;
                    case RealWorldElevation.Altitude:
                        var RWElevation = container.data.GetElevation(RealWorldCoor);
                        var RWSelevation = GISTerrainLoaderGeoConversion.GetRealWorldHeight(container, ObjectPosition);
                        elevation = RWElevation + RWSelevation / 10 * container.Scale.y;
                        break;
                    case RealWorldElevation.Height:
                        elevation = GISTerrainLoaderGeoConversion.GetRealWorldHeight(container, ObjectPosition);
                        break;
                }
            }

            return elevation;
        }
        private static float GetRealWorldHeight(GISTerrainContainer container, Vector3 SpacePosition)
        {
            var PostionOnTerrain = GISTerrainLoaderGeoConversion.GetHeight(SpacePosition);

            var Diff = SpacePosition.y - PostionOnTerrain;

            var elevation = (Diff / container.Scale.y) * 10;

            return elevation;


        }



        #endregion


        #region CoordinatesConversion

        /// <summary>
        /// Convert Real World Coordinates From X_EPSG to Y_EPSG via DotSpatial lib
        /// </summary>
        /// <param name="Coor"></param>
        /// <param name="epsg"></param>
        /// <param name="UseOffset"></param>
        /// <returns></returns>
        public static DVector2 ConvertCoordinatesFromTo(DVector2 Coor, int From_epsg, int To_epsg)
        {
            DVector2 Result = new DVector2(0, 0);
            if (From_epsg == 0) From_epsg = 4326;
            if (To_epsg == 0) To_epsg = 4326;
#if DotSpatial            
            ProjectionInfo source = ProjectionInfo.FromEpsgCode(From_epsg);
            ProjectionInfo To = ProjectionInfo.FromEpsgCode(To_epsg);

            double[] xy = new double[2] { (Coor.x), (Coor.y) };
            double[] z = new double[] { 1 };

            Reproject.ReprojectPoints(xy, z, source, To, 0, 1);

            var Y_Easting = xy[0];
            var X_Northing = xy[1];

            Result = new DVector2(Y_Easting, X_Northing);
#else
            Result = Coor;
#endif
            return Result;
        }
        /// <summary>
        /// Convert Coordinates From a projection defined by EPSG to Lat Lon
        /// </summary>
        /// <param name="Coor"></param>
        /// <param name="Epsg"></param>
        /// <param name="useoffest"></param>
        /// <returns></returns>
        public static DVector2 ConvertTOLatLon(DVector2 Coor, int Epsg)
        {
#if DotSpatial

            if (Epsg != 0)
            {
                ProjectionInfo source = ProjectionInfo.FromEpsgCode(Epsg);

                if (Epsg == 4258)
                {
                    source = ProjectionInfo.FromEpsgCode(25830);
                }

                ProjectionInfo To = ProjectionInfo.FromEpsgCode(4326);

                double[] xy = new double[2] { (Coor.x), (Coor.y) };
                double[] z = new double[] { 1 };

                Reproject.ReprojectPoints(xy, z, source, To, 0, 1);

                var Lat_Easting = xy[0];
                var Lon_Northing = xy[1];
 
                return new DVector2(Lat_Easting, Lon_Northing);
            }
            else
                return new DVector2(Coor.x, Coor.y);

#else
            return new DVector2(Coor.x, Coor.y);
#endif
        }

        /// <summary>
        /// Show Geographic Lat-Lon Coordinates in Different display formats
        /// </summary>
        /// <param name="LatLon"></param>
        /// <param name="displayformat"></param>
        /// <returns></returns>
        public static string ToDisplayFormat(DVector2 LatLon,DisplayFormat displayformat)
        {
            string format = "";

            switch(displayformat)
            {
                case DisplayFormat.Decimale:
                    format = "{ " + LatLon.x.ToString() + " / " + LatLon.y.ToString() + "}";
                    break;
                case DisplayFormat.DegMinSec:
                    format = GISTerrainLoaderGeographic.DecimalToDegMinSec(LatLon);
                    break;
                case DisplayFormat.MGRS:
                    GISTerrainLoaderUTM MGRUTM = new GISTerrainLoaderUTM();
                    format = MGRUTM.LatLonToMGRUTM(LatLon);
                    break;
                case DisplayFormat.UTM:
                    GISTerrainLoaderUTM utm = new GISTerrainLoaderUTM();
                    format = utm.LatLonToUTM(LatLon);
                    break;
            }

            return format;
        }
        #endregion


        #region GTLAPI


        /// <summary>
        /// Set GameObject position by converting real world coordinates to unity space position (Inputs Lat/Lon position + Elevation [m])
        /// </summary>
        /// <param name="container">Generated Terrain Container</param>
        /// <param name="RealWorldCoor"> Real World Position</param>
        /// <param name="RWElevationValue"> Elevation in m </param>
        /// <param name="elevationMode"> Check the documentation.. </param>
        /// <returns></returns>
        public static Vector3 RealWorldCoordinatesToUnityWorldSpace(GISTerrainContainer container, DVector2 RealWorldCoor, ref GISTerrainTile terrain, float Scale = 1)
        {
            Vector3 SpacePositon = Vector3.zero;

            Vector3 SpacePos = Vector3.zero;

            bool PointInculded = false;

            if (container.data.EPSG != 0)
            {
                SpacePos = EPSGToUnityWorldSpace(RealWorldCoor, container);

                if (container.IncludeEPSGPoint(RealWorldCoor))
                    PointInculded = true;

            }
            else
            {

                SpacePos = LatLonToUnityWorldSpace(RealWorldCoor, container);
                
                if (container.IncludePoint(RealWorldCoor))
                    PointInculded = true;
            }

            if (PointInculded)
            {
                SpacePos.y = 1000;
                var elevation = GISTerrainLoaderGeoConversion.GetHeight(SpacePos, ref terrain);
                SpacePositon = new Vector3(SpacePos.x, elevation, SpacePos.z);

            }
            return SpacePositon;
        }
 
        public static double Getdistance(DVector2 P1, DVector2 P2, char ax, char unit = 'K')
        {
            double distance = 0;

            if (ax == 'X')
            {
                if (P1.x < 0 && P2.x > 0)
                {
                    var p0 = new DVector2(0, 0);
                    var p4 = new DVector2(P2.x, 0);


                    var d1 = CalDistance(new DVector2(P1.x, 0), new DVector2(0, 0));
                    var d2 = CalDistance(new DVector2(0, 0), p4);

                    distance = d1 + d2;

                }
                else
                {
                    distance = CalDistance(P1, P2);
                }

            }

            if (ax == 'Y')
            {
                if (P1.y < 0 && P2.y > 0)
                {


                    var p0 = new DVector2(0, 0);
                    var p4 = new DVector2(0, P2.y);

                    var d1 = CalDistance(new DVector2(0, P1.y), new DVector2(0, 0));
                    var d2 = CalDistance(new DVector2(0, 0), p4);

                    distance = d1 + d2;

                }
                else
                    distance = CalDistance(P1, P2);
            }
            return distance;

        }

        public static double CalDistance(DVector2 P1, DVector2 P2, char unit = 'K')
        {
            if ((P1.y == P2.y) && (P1.x == P2.x))
            {
                return 0;
            }
            else
            {
                var radlat1 = Math.PI * P1.y / 180;
                var radlat2 = Math.PI * P2.y / 180;
                var theta = P2.x - P1.x;
                var radtheta = Math.PI * theta / 180;
                var dist = Math.Sin(radlat1) * Math.Sin(radlat2) + Math.Cos(radlat1) * Math.Cos(radlat2) * Math.Cos(radtheta);

                if (dist > 1)
                {
                    dist = 1;
                }
                //if (dist > 0) dist = dist * -1;
                dist = Math.Acos(dist);
                dist = dist * 180 / Math.PI;
                dist = dist * 60 * 1.1515;
                if (unit == 'K') { dist = dist * 1.609344; }
                if (unit == 'N') { dist = dist * 0.8684; }
                return dist;
            }
        }

        public const double DEG2RAD = Math.PI / 180;
        public static DVector2 LatLongToMercat(double x, double y)
        {
            double sy = Math.Sin(y * DEG2RAD);
            var mx = (x + 180) / 360;
            var my = 0.5 - Math.Log((1 + sy) / (1 - sy)) / (Math.PI * 4);

            return new DVector2(mx, my);
        }


        //Get Terrain Height

        private static Terrain m_terrain;

        public static Terrain terrain
        {
            get { return m_terrain; }
            set
            {
                if (m_terrain != value)
                {
                    m_terrain = value;

                }
            }
        }

        private static GISTerrainTile m_terrainO;

        public static GISTerrainTile terrainO
        {
            get { return m_terrainO; }
            set
            {
                if (m_terrainO != value)
                {
                    m_terrainO = value;

                }
            }
        }
        /// <summary>
        /// Get Space World Elevation(y) of UWSPosition
        /// </summary>
        /// <param name="WSposition"></param>
        /// <returns></returns>
        public static float GetHeight(Vector3 WSposition)
        {
            float height = 0;

            terrain = GetTerrain(WSposition);

            if (terrain != null)
            {
                TerrainData t = terrain.terrainData;
                height = terrain.SampleHeight(WSposition)+ terrain.GetPosition().y;

            }


            return height;
        }
        public static float GetHeight(Vector3 WSposition, ref GISTerrainTile terrainO)
        {
            float height = 0;

            terrainO = GetTerrainObject(WSposition);

            if (terrainO != null)
            {
                height = terrainO.terrain.SampleHeight(WSposition)
                + terrainO.terrain.GetPosition().y;
            }
            return height;
        }
        public static Terrain GetTerrain(Vector3 WSposition)
        {
            var downDirection = Vector3.down;

            var Rays = Physics.RaycastAll(WSposition, downDirection);

            foreach(var ray in Rays)
            {
                if (terrain == null)
                {
                    var ter = ray.collider.transform.gameObject.GetComponent<Terrain>();

                    if (ter)
                    {
                        terrain = ter;
                    }
                       
                }


                if (terrain != null)
                {
                    if (!string.Equals(ray.collider.transform.name, terrain.name))
                    {
                        if (ray.collider.transform.gameObject.GetComponent<Terrain>())
                            terrain = ray.collider.transform.gameObject.GetComponent<Terrain>();
                    }
                }

            }

            return terrain;
        }
        public static GISTerrainTile GetTerrainObject(Vector3 WSposition)
        {
            var downDirection = Vector3.down;

            RaycastHit hitInfo;

            var ray = new Ray(WSposition, (downDirection));

            if (Physics.Raycast(ray, out hitInfo, 100000))
            {
                if (terrainO == null)
                {
                    var t = hitInfo.collider.transform.gameObject.GetComponent<GISTerrainTile>();

                    if (t)
                        terrainO = t;
                }

                if (terrainO != null)
                {
                    if (!string.Equals(hitInfo.collider.transform.name, terrainO.name))
                    {
                        if (hitInfo.collider.transform.gameObject.GetComponent<GISTerrainTile>())
                            terrainO = hitInfo.collider.transform.gameObject.GetComponent<GISTerrainTile>();
                    }
                }

            }

            return terrainO;
        }
        //Get Raster Projection

        /// <summary>
        /// Used for ShapeFile
        /// </summary>
        /// <param name="projReader"></param>
        /// <param name="point"></param>
        /// <param name="Szone"></param>
        /// <returns></returns>
        public static DVector2 ConvertTOLatLon(GISTerrainLoaderProjectionSystem projReader, DVector2 point, int epsg = 0)
        {
            DVector2 LatLon = new DVector2(0, 0);

            if (epsg != 0)
            {
#if DotSpatial
                if (epsg != 0)
                    LatLon = GISTerrainLoaderGeoConversion.ConvertTOLatLon(new DVector2(point.x, point.y), epsg);
                else
                    Debug.LogError("EPSG Not Defiened ..");
#endif
            }
            else
            {
                switch (projReader.GEOGCSProjection)
                {

                    case "Undefined":
#if DotSpatial
                        if (epsg != 0)
                            LatLon = GISTerrainLoaderGeoConversion.ConvertTOLatLon(new DVector2(point.x, point.y), epsg);
                        else
                            Debug.LogError("EPSG Not Defiened ..");
#endif
                        break;
                    // Geographic(lat/lon) coordinates
                    case "GCS_WGS_1984":
                        //if (projReader.Datum.Name == "WGS84")
                        LatLon = point;
                        break;
                    // UTM Projection
                    case "GCS_North_American_1983":
                        var utmTL = new DVector2(point.x, point.y);
                        var Fullzone = projReader.Name.Split('_')[4];
                        var ZoneNum = Regex.Match(Fullzone, @"\d+").Value;
                        var ZoneL = Regex.Replace(Fullzone, @"[\d-]", string.Empty);
                        var coor = ZoneNum + " " + ZoneL + " " + utmTL.x + " " + utmTL.y;
                        GISTerrainLoaderUTM cc = new GISTerrainLoaderUTM();
                        var str1 = cc.UTMToLatLon(coor);
                        LatLon = new DVector2(str1[1], str1[0]);
                        break;
                    case "UTM":
                        utmTL = new DVector2(point.x, point.y);
                        ZoneNum = projReader.UTMData.ZoneNum.ToString();
                        ZoneL = projReader.UTMData.ZoneLet;
                        coor = ZoneNum + " " + ZoneL + " " + utmTL.x + " " + utmTL.y;
                        cc = new GISTerrainLoaderUTM();
                        str1 = cc.UTMToLatLon(coor);
                        LatLon = new DVector2(str1[1], str1[0]);
                        break;
                    // Lumbert
                    case "GCS_RESEAU_GEODESIQUE_FRANCAIS_1993":
                        var Lmb_TL = new DVector2(point.x, point.y);
                        DVector3 LatLog_TL = GISTerrainLoaderLambert.convertToWGS84Deg(Lmb_TL.x, Lmb_TL.y, projReader.LambertData.Lambertzone);
                        LatLon = new DVector2(LatLog_TL.x, LatLog_TL.y);
                        break;
                    // Mercator
                    case "merc":
                        var merc = new DVector2(point.x, point.y);
                        LatLon = merc;
                        break;
                    case "NAD83":
#if DotSpatial
                        if (projReader.UTMData != null)
                        {
                            utmTL = new DVector2(point.x, point.y);
                            ZoneNum = projReader.UTMData.ZoneNum.ToString();
                            ZoneL = projReader.UTMData.ZoneLet;
                            coor = ZoneNum + " " + ZoneL + " " + utmTL.x + " " + utmTL.y;
                            LatLon = GISTerrainLoaderNAD.Nad83ToLatLon(utmTL);
                        }
                        else
                        {
                            if (epsg == 0)
                                LatLon = GISTerrainLoaderNAD.Nad83ToLatLon(new DVector2(point.x, point.y));
                            else
                                LatLon = GISTerrainLoaderGeoConversion.ConvertTOLatLon(new DVector2(point.x, point.y), epsg);
                        }
#else
                        //Debug.LogError("File Projected in NAD, Please add DotSpatial Lib (See Projection Section)");
#endif


                        break;
                }
            }

  
          

            return LatLon;
        }


        
        //Get Raster Projection

        /// <summary>
        /// Convert  to (Lat, Lon) coordinates 
        /// </summary>
        /// <returns>
        /// </returns>
        /// <param name='latlon'>
        /// (Lat, Lon) as Vector2
        /// </param>
        public static DVector2 ConvertTOLatLon(GISTerrainLoaderProjectionReader projReader, DVector2 point, string Szone = "S")
        {
            DVector2 LatLon = new DVector2(0, 0);

            switch (projReader.Projection)
            {
                // Case of Geographic(lat/lon) coordinates
                case "longlat":

                    LatLon = point;
                    //if (projReader.Datum == "WGS84")
                    //{
                    //    Debug.Log(projReader.Projection + "  " + point);
                    //    LatLon = point;
                    //}
                    break;
                // Case of UTM Projection
                case "utm":
                    if (projReader.Datum == "NAD83")
                    {
                        var utmTL = new DVector2(point.x, point.y);
                        LatLon = GISTerrainLoaderNAD.Nad83ToLatLon(utmTL);
                    }
                    else
                    {

                        var utmTL = new DVector2(point.x, point.y);
                        var coor = projReader.Zone.ToString() + " " + Szone + " " + utmTL.x + " " + utmTL.y;
                        GISTerrainLoaderUTM cc = new GISTerrainLoaderUTM();
                        var str1 = cc.UTMToLatLon(coor);
                        LatLon = new DVector2(str1[1], str1[0]);
                    }
                    break;

                // Lumbert
                case "lcc":
                    var Lmb_TL = new DVector2(point.x, point.y);
                    DVector3 LatLog_TL = GISTerrainLoaderLambert.convertToWGS84Deg(Lmb_TL.x, Lmb_TL.y, LambertZone.Lambert93);
                    LatLon = new DVector2(LatLog_TL.x, LatLog_TL.y);
                    break;

                // Lumbert
                case "merc":
                    var merc = new DVector2(point.x, point.y);
                    LatLon = merc;

                    break;
            }

            return LatLon;
        }


        #endregion


    }
}