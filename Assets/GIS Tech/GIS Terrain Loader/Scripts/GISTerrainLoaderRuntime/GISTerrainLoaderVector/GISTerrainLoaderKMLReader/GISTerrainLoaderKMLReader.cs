using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderKMLReader
    {
        public GISTerrainLoaderGeoVectorData fileData = new GISTerrainLoaderGeoVectorData("");
        private GISTerrainLoaderVectorParameters_SO VectorParameters;
        protected XmlDocument m_Document;
        private string path;
        public GISTerrainLoaderKMLReader(string m_path)
        {
            if (VectorParameters == null)
                VectorParameters = GISTerrainLoaderVectorParameters_SO.LoadParameters();

            path = m_path;
            fileData = new GISTerrainLoaderGeoVectorData("");
            LoadFile();
        }
        public void LoadFile()
        {
            string text = System.IO.File.ReadAllText(path, Encoding.Default);
            m_Document = new XmlDocument();
            m_Document.LoadXml(text);

            Parse();
        }
        private void Parse()
        {
            // Visits the all child nodes
            foreach (XmlNode n in m_Document.ChildNodes)
            {
                Visit(n);
            }
        }
        protected void Visit(XmlNode node)
        {
            if (node == null) return;

            string nodeName = node.Name.ToLowerInvariant();
            XmlElement elem = node as XmlElement;

            if (elem != null)
            {
                if (nodeName.Equals("point"))
                    ReadGeoPoint(elem);
                else if (nodeName.Equals("linestring"))
                    ReadGeoLine(elem);
                else if (nodeName.Equals("polygon"))
                    ReadGeoPolygon(elem);
                //linearring multigeometry placemark model region groundoverlay
                else
                {
                    // Visits the child nodes
                    foreach (XmlNode child in node.ChildNodes)
                    {
                        Visit(child);
                    }
                }
            }
        }
        protected void ReadGeoPoint(XmlElement PointNode)
        {
            if (PointNode == null) return;

            GISTerrainLoaderPointGeoData PointGeoData = new GISTerrainLoaderPointGeoData();

            XmlElement coordinates = GetFirstChild(PointNode, "coordinates");
            if (coordinates == null || !coordinates.HasChildNodes)
                return;

            IList<DVector3> points = ParseCoordinates(coordinates.ChildNodes[0].InnerText);


            if (points.Count == 1)
            {
                if (VectorParameters.AddRandom_ID_Vector_KML)
                    PointGeoData.ID = UnityEngine.Random.Range(0, 100000).ToString();

                if (points.Count >= 1)
                {
                    if (points[0] != null)
                    {
                        PointGeoData.GeoPoint = new DVector2(points[0].x, points[0].y);
                        PointGeoData.Elevation = (float)points[0].z;
                    }
                }

                PointGeoData.DataBase.Add(new GISTerrainLoaderGeoDataBase(VectorParameters.ID_Tag, PointGeoData.ID));

                XmlElement name = GetParentNode(PointNode, "name");
                if (name != null)
                    PointGeoData.Name = name.ChildNodes[0].InnerText;


                PointGeoData.Tag = VectorParameters.KML_GeoPoint_Value;
                PointGeoData.DataBase.Add(new GISTerrainLoaderGeoDataBase(VectorParameters.KML_GeoPoint_Attribute, VectorParameters.KML_GeoPoint_Value));

                fileData.GeoPoints.Add(PointGeoData);
            }

        }
        protected void ReadGeoLine(XmlElement LineNode)
        {
            if (LineNode == null) return;

            GISTerrainLoaderLineGeoData LineGeoData = new GISTerrainLoaderLineGeoData();

            XmlElement coordinates = GetFirstChild(LineNode, "coordinates");
            if (coordinates == null || !coordinates.HasChildNodes)
                return;

            IList<DVector3> points = ParseCoordinates(coordinates.ChildNodes[0].InnerText);

            foreach (var point in points)
            {
                var PointGeoData = new GISTerrainLoaderPointGeoData();
 
                PointGeoData.GeoPoint = new DVector2(point.x, point.y);
                PointGeoData.Elevation = (float)point.z;
            }

            if (VectorParameters.AddRandom_ID_Vector_KML)
                LineGeoData.ID = UnityEngine.Random.Range(0, 100000).ToString();

            XmlElement name = GetParentNode(LineNode, "name");
            if (name != null)
                LineGeoData.Name = name.ChildNodes[0].InnerText;

            LineGeoData.DataBase.Add(new GISTerrainLoaderGeoDataBase(VectorParameters.ID_Tag, LineGeoData.ID));
            LineGeoData.Tag = VectorParameters.KML_GeoLine_Value;
            LineGeoData.DataBase.Add(new GISTerrainLoaderGeoDataBase(VectorParameters.KML_GeoLine_Attribute, VectorParameters.KML_GeoLine_Value));

            fileData.GeoLines.Add(LineGeoData);

        }
        protected void ReadGeoPolygon(XmlElement PolygonNode)
        {
            if (PolygonNode == null) return;

            GISTerrainLoaderPolygonGeoData PolygonGeoData = new GISTerrainLoaderPolygonGeoData();
 
            XmlElement coordinates = GetFirstChild(PolygonNode, "coordinates");
            if (coordinates == null || !coordinates.HasChildNodes)
                return;

            IList<DVector3> points = ParseCoordinates(coordinates.ChildNodes[0].InnerText);

            List<GISTerrainLoaderPointGeoData> SubGeoPoints = new List<GISTerrainLoaderPointGeoData>();

            foreach (var point in points)
            {
                var PointGeoData = new GISTerrainLoaderPointGeoData();

                PointGeoData.GeoPoint = new DVector2(point.x, point.y);
                PointGeoData.Elevation = (float)point.z;
            }

            PolygonGeoData.GeoPoints.Add(SubGeoPoints);

            if (VectorParameters.AddRandom_ID_Vector_KML)
                PolygonGeoData.ID = UnityEngine.Random.Range(0, 100000).ToString();

            XmlElement name = GetParentNode(PolygonNode, "name");
            if (name != null)
                PolygonGeoData.Name = name.ChildNodes[0].InnerText;

            PolygonGeoData.DataBase.Add(new GISTerrainLoaderGeoDataBase(VectorParameters.ID_Tag, PolygonGeoData.ID));

            PolygonGeoData.Tag = VectorParameters.KML_GeoLine_Value;
            PolygonGeoData.DataBase.Add(new GISTerrainLoaderGeoDataBase(VectorParameters.KML_GeoLine_Attribute, VectorParameters.KML_GeoLine_Value));

            fileData.GeoPolygons.Add(PolygonGeoData);
        }
        public static XmlElement GetFirstChild(XmlElement element, string childElementName)
        {
            if (element == null || element.HasChildNodes == false || string.IsNullOrEmpty(childElementName))
                return null;

            childElementName = childElementName.ToLowerInvariant();

            foreach (XmlNode node in element.ChildNodes)
            {
                XmlElement c = node as XmlElement;
                if (c == null)
                    continue;

                if (c.Name.ToLowerInvariant().Equals(childElementName))
                    return c;
            }

            return null;
        }
        public static XmlElement GetParentNode(XmlElement element, string ParentElementName)
        {
            if (element == null || string.IsNullOrEmpty(ParentElementName))
                return null;

            ParentElementName = ParentElementName.ToLowerInvariant();

            foreach (XmlNode node in element.ParentNode)
            {
                XmlElement c = node as XmlElement;
                if (c == null)
                    continue;
                if (c.Name.ToLowerInvariant().Equals(ParentElementName))
                    return c;
            }

            return null;
        }
        public static Dictionary<string, string> ReadDataBase (string DataText)
        {

            Dictionary<string, string> DataBase = new Dictionary<string, string>();
            var Parts_0 = DataText.Split(new string[] { "<BR>" }, StringSplitOptions.None);

            foreach (var part in Parts_0)
            {
                if (part.Contains("="))
                {
                    ;
                    var parts_01 = part.Split('=');
                    string attribute = parts_01[0];
                    var value = parts_01[1];
                    //string attribute = UnicodeToUTF8(parts_01[0]);
                    //Debug.Log(Regex.Replace(attribute, @"\u00A0", " "));
                    //var value = UnicodeToUTF8(parts_01[1]);
                    Debug.Log(attribute + "  " + value);
                    DataBase.Add(attribute, value);
                }
               
            }
            return DataBase;
        }
        private static string UnicodeToUTF8(string strFrom)
        {
            UTF32Encoding utf8 = new UTF32Encoding();
            string unicodeString = strFrom;
            byte[] encodedBytes = utf8.GetBytes(unicodeString);
            var strTo = Encoding.UTF32.GetString(encodedBytes);
            return strTo;
        }
        protected IList<DVector3> ParseCoordinates(string coordinates)
        {
            List<DVector3> points = new List<DVector3>();

            if (string.IsNullOrEmpty(coordinates) || coordinates.Trim().Length == 0)
                return points;
            
            // Removes the spaces inside the touples
            while (coordinates.Contains(", "))
            {
                coordinates = coordinates.Replace(", ", ",");
            }

            // Splits the tuples
            string[] coordinate = coordinates.Trim().Split(' ', '\t', '\n');
            if (coordinate == null)
                return points;

            // Process each tuple
            foreach (string p in coordinate)
            {
                string tp = p.Trim();
                if (tp.Length == 0)
                    continue;

                string[] cords = tp.Split(',');

                try
                {
                    double longitude = double.Parse(cords[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
                    double latitude = double.Parse(cords[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
                    double altitude = 0;

                    if (cords.Length == 3)  // the altitude is optional
                    {
                        altitude = double.Parse(cords[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
                    }

                    DVector3 point = new DVector3(0,0,0);
                    point.x = longitude;
                    point.y = latitude;
                    point.z = altitude;

                    // If point is the same as the previous point then skip it
                    if (points.Count > 0 && points[points.Count - 1].Equals(point))
                        continue;

                    points.Add(point);
                }
                catch (Exception exc)
                {
                    Debug.LogError("Coordinate is in wrong format: " + tp +  exc);
                }
            }

            return points;
        }
        public static int? GetAltitudeModeCode(XmlElement elem)
        {
            AltitudeMode? altitideMode = GetAltitudeMode(elem);

            if (altitideMode == null)
                return null;

            return (int)altitideMode;
        }
        public enum AltitudeMode
        {
            clampToGround = 0,
            relativeToGround = 1,
            absolute = 2,
            clampToSeaFloor = 3,
            relativeToSeaFloor = 4,
            NotSupported =5
        }
        private static AltitudeMode GetAltitudeMode(XmlElement elem)
        {
            AltitudeMode altitudemode = AltitudeMode.NotSupported;

            string altitudeModeVal = "clamptoground";
            XmlElement altitudeMode = GetFirstChild(elem, "altitudeMode");
            if (altitudeMode == null)
                altitudeMode = GetFirstChild(elem, "gx:altitudeMode");

            if (altitudeMode == null)
                altitudemode= AltitudeMode.absolute;

            if (altitudeMode != null && altitudeMode.HasChildNodes && altitudeMode.ChildNodes[0].Value != null)
            {
                altitudeModeVal = altitudeMode.ChildNodes[0].Value.Trim().ToLowerInvariant();
            }

            if (altitudeModeVal.Equals("clamptoground"))
                altitudemode = AltitudeMode.clampToGround;
            else if (altitudeModeVal.Equals("relativetoground"))
                altitudemode = AltitudeMode.relativeToGround;
            else if (altitudeModeVal.Equals("absolute"))
                altitudemode = AltitudeMode.absolute;
            else if (altitudeModeVal.Equals("relativetoseafloor"))
                altitudemode = AltitudeMode.relativeToSeaFloor;
            else if (altitudeModeVal.Equals("clamptoseafloor"))
                altitudemode = AltitudeMode.clampToSeaFloor;
            else if (altitudemode == AltitudeMode.NotSupported)
                Debug.LogError("Altitude mode not supported: " + altitudeModeVal);

            return altitudemode;
  
        }
        public void SetProjection(string path)
        {
            //CoordinateReferenceSystem = GISTerrainLoaderProjectionReader.ReadProjectionFile(path);
        }
        public static string GetAttribute(XmlElement elem, string attributeName)
        {
            XmlAttribute attribute = elem.Attributes[attributeName];
             
            if (attribute == null)
                return null;

            return attribute.Value;
        }
        protected void VisitGeography(XmlElement elem)
        {
            #region The id in the KML string

            // Extracts the point coordinates
            XmlElement description = GetFirstChild(elem, "description");
            if (description == null || !description.HasChildNodes)
                return;

            XmlNode coordinatesTextNode = description.ChildNodes[0];

            Debug.Log(coordinatesTextNode.InnerText);
            var s = GetAttribute(elem, "description");
            Debug.Log(s);
            #endregion

            #region The extrude flag

            XmlElement extrudeElem = GetFirstChild(elem, "extrude");
            if (extrudeElem != null && extrudeElem.HasChildNodes)
            {
                string val = extrudeElem.ChildNodes[0].InnerText;
                if (val != null && val.Trim().Equals("1"))
                {
                    //geography.Extrude = true;
                }
            }

            #endregion

            #region The tessellate flag

            XmlElement tessellateElem = GetFirstChild(elem, "tessellate");
            if (tessellateElem != null && tessellateElem.HasChildNodes)
            {
                string val = tessellateElem.ChildNodes[0].InnerText;
                if (val != null && val.Trim().Equals("1"))
                {
                    //geography.Tesselate = true;
                }
            }

            #endregion
        }

    }
    
}