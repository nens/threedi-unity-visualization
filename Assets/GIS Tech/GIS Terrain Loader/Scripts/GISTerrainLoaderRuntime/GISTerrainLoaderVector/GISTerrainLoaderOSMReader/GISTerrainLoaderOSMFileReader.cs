﻿/*     Unity GIS Tech 2020-2023      */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using System.Linq;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderOSMFileReader  
    {
    }
    public class GISTerrainLoaderOSMData
    {
        [GISTerrainLoaderOSMProperty("version")]
        public string Version { get; set; }
        [GISTerrainLoaderOSMProperty("generator")]
        public string Generator { get; set; }
        [GISTerrainLoaderOSMProperty("copyright")]
        public string Copyright { get; set; }
        [GISTerrainLoaderOSMProperty("attribution")]
        public string Attribution { get; set; }
        [GISTerrainLoaderOSMProperty("license")]
        public string License { get; set; }

        public GISTerrainLoaderOSMBounds Bounds { get; set; }

        public Dictionary<long, GISTerrainLoaderOSMNode> Nodes = new Dictionary<long, GISTerrainLoaderOSMNode>();
        public List<GISTerrainLoaderOSMWay> Ways { get; set; } = new List<GISTerrainLoaderOSMWay>();
        public List<GISTerrainLoaderOSMRelation> Relations { get; set; } = new List<GISTerrainLoaderOSMRelation>();
 
        public void FillNodes()
        {
             foreach (var wayDic in Ways)
            {
                foreach (var nref in wayDic.Nds)
                {
                    var Id = long.Parse(nref.Ref);

                    GISTerrainLoaderOSMNode dicNode = new GISTerrainLoaderOSMNode();

                    if (Nodes.TryGetValue(Id, out dicNode))
                        wayDic.Nodes.Add(dicNode);
                }
            }
        }

    }
    public class GISTerrainLoaderOSMParser
    {
        private Dictionary<Type, Dictionary<string, PropertyInfo>> _metaOsmElements;
        public GISTerrainLoaderOSMParser()
        {
            _metaOsmElements = new Dictionary<Type, Dictionary<string, PropertyInfo>>();

            AddMetaOsmElement<GISTerrainLoaderOSMData>();
            AddMetaOsmElement<GISTerrainLoaderOSMBounds>();
            AddMetaOsmElement<GISTerrainLoaderNd>();
            AddMetaOsmElement<GISTerrainLoaderOSMNode>();
            AddMetaOsmElement<GISTerrainLoaderOSMTag>();
            AddMetaOsmElement<GISTerrainLoaderOSMWay>();
            AddMetaOsmElement<GISTerrainLoaderOSMRelation>();
            AddMetaOsmElement<GISTerrainLoaderRelationMember>();
        }

        private void AddMetaOsmElement<T>()
        {
            var elementType = typeof(T);
            var properties = GISTerrainLoaderParserHelper.GetOsmProperties(elementType);
            _metaOsmElements.Add(elementType, properties);
        }

        private void ApplyAttributes(XmlAttributeCollection attributes, object obj)
        {
            var osmProperties = _metaOsmElements[obj.GetType()];

            foreach (XmlAttribute rootAttribute in attributes)
            {
                var attrName = rootAttribute.Name;
                var attrValue = rootAttribute.Value;

                if (osmProperties.ContainsKey(attrName))
                {
                    var property = osmProperties[attrName];
                    GISTerrainLoaderParserHelper.SetValue(obj, property, attrValue);
                }
            }
        }

        private object ParseChild(XmlElement xmlNode, GISTerrainLoaderOSMData osm)
        {
            var name = xmlNode.Name;
            var attrs = xmlNode.Attributes;
            var result = default(object);

            if (name == "bounds")
            {
                var bounds = new GISTerrainLoaderOSMBounds();

                ApplyAttributes(attrs, bounds);
                osm.Bounds = bounds;

                result = bounds;
            }
            else if (name == "node")
            {
                var node = new GISTerrainLoaderOSMNode();
                ApplyAttributes(attrs, node);
                if (!osm.Nodes.ContainsKey(long.Parse(node.Id))) 
                osm.Nodes.Add(long.Parse(node.Id), node);
 
                foreach (XmlElement xmlNodeChildNode in xmlNode.ChildNodes)
                {
                    var childElement = ParseChild(xmlNodeChildNode, osm);

                    if (childElement.GetType() == typeof(GISTerrainLoaderOSMTag))
                    {
                        GISTerrainLoaderOSMTag tag = (GISTerrainLoaderOSMTag)childElement;
                         node.DataBase.Add(new GISTerrainLoaderGeoDataBase(tag.Attribute, tag.Value));
                    }
                }

                result = node;
            }
            else if (name == "tag")
            {
                var tag = new GISTerrainLoaderOSMTag();
                ApplyAttributes(attrs, tag);

                result = tag;
            }
            else if (name == "way")
            {
                var way = new GISTerrainLoaderOSMWay();
                ApplyAttributes(attrs, way);
                osm.Ways.Add(way);

                foreach (XmlElement xmlNodeChildNode in xmlNode.ChildNodes)
                {
                    var childElement = ParseChild(xmlNodeChildNode, osm);

                    if (childElement.GetType() == typeof(GISTerrainLoaderOSMTag))
                    {
                        GISTerrainLoaderOSMTag tag = (GISTerrainLoaderOSMTag)childElement;
                        way.DataBase.Add(new GISTerrainLoaderGeoDataBase(tag.Attribute, tag.Value));
 
                    }
                    else if (childElement.GetType() == typeof(GISTerrainLoaderNd))
                    {
                        way.Nds.Add((GISTerrainLoaderNd)childElement);
                    }
                }

                result = way;
            }
            else if (name == "relation")
            {
                var relation = new GISTerrainLoaderOSMRelation();
                ApplyAttributes(attrs, relation);
                osm.Relations.Add(relation);

                foreach (XmlElement xmlNodeChildNode in xmlNode.ChildNodes)
                {
                    var childElement = ParseChild(xmlNodeChildNode, osm);

                    if (childElement.GetType() == typeof(GISTerrainLoaderOSMTag))
                    {
                        GISTerrainLoaderOSMTag tag = (GISTerrainLoaderOSMTag)childElement;
                        relation.DataBase.Add(new GISTerrainLoaderGeoDataBase(tag.Attribute, tag.Value));
 
                     }
                    else if (childElement.GetType() == typeof(GISTerrainLoaderRelationMember))
                    {
                        relation.Members.Add((GISTerrainLoaderRelationMember)childElement);
                    }
                }

                result = relation;
            }
            else if (name == "member")
            {
                var member = new GISTerrainLoaderRelationMember();

                ApplyAttributes(attrs, member);

                result = member;
            }
            else if (name == "nd")
            {
                var nd = new GISTerrainLoaderNd();
                ApplyAttributes(attrs, nd);

                result = nd;
            }

            return result;
        }
        public GISTerrainLoaderOSMData ParseFromFile(byte[] data)
        {
            var result = default(GISTerrainLoaderOSMData);

            using (var stream = new MemoryStream(data))
            {
                result = ParseFromStream(stream);
            }

            return result;
        }

        public GISTerrainLoaderOSMData ParseFromFile(string filePath)
        {
            var result = default(GISTerrainLoaderOSMData);

            using (var stream = new FileInfo(filePath).OpenRead())
            {
                result = ParseFromStream(stream);
            }

            return result;
        }

        public async Task<GISTerrainLoaderOSMData> ParseFromFileAsync(string filePath)
        {
            var result = default(GISTerrainLoaderOSMData);

            using (var stream = new FileInfo(filePath).OpenRead())
            {
                result = await ParseFromStreamAsync(stream);
            }

            return result;
        }

        public async Task<GISTerrainLoaderOSMData> ParseFromStreamAsync(Stream stream)
        {
            var fileSource = string.Empty;

            using (var reader = new StreamReader(stream))
            {
                fileSource = await reader.ReadToEndAsync();
            }

            return Parse(fileSource);
        }

        public GISTerrainLoaderOSMData ParseFromStream(Stream stream)
        {
            var fileSource = string.Empty;

            using (var reader = new StreamReader(stream))
            {
                fileSource = reader.ReadToEnd();
            }

            return Parse(fileSource);
        }

        public GISTerrainLoaderOSMData Parse(string xmlFileSource)
        {
            var osm = new GISTerrainLoaderOSMData
            {
                Nodes = new Dictionary<long, GISTerrainLoaderOSMNode>(),
                Ways = new List<GISTerrainLoaderOSMWay>(),
                Bounds = new GISTerrainLoaderOSMBounds(),
                Relations = new List<GISTerrainLoaderOSMRelation>()
            };
            var doc = new XmlDocument();
            doc.LoadXml(xmlFileSource);
            var root = doc.DocumentElement;

            ApplyAttributes(root.Attributes, osm);

            foreach (XmlElement xmlNode in root)
            {
                ParseChild(xmlNode, osm);
            }

            return osm;
        }
    }
    public class GISTerrainLoaderOSMBounds
    {
        [GISTerrainLoaderOSMProperty("minlat")]
        public double MinLat { get; set; }
        [GISTerrainLoaderOSMProperty("minlon")]
        public double MinLon { get; set; }
        [GISTerrainLoaderOSMProperty("maxlat")]
        public double MaxLat { get; set; }
        [GISTerrainLoaderOSMProperty("maxlon")]
        public double MaxLon { get; set; }
    }
    public class GISTerrainLoaderNd
    {
        [GISTerrainLoaderOSMProperty("ref")]
        public string Ref { get; set; }


    }
    public class GISTerrainLoaderOSMNode
    {
        [GISTerrainLoaderOSMProperty("id")]
        public string Id { get; set; }
        [GISTerrainLoaderOSMProperty("visible")]
        public bool Visible { get; set; }
        [GISTerrainLoaderOSMProperty("version")]
        public int Version { get; set; }
        [GISTerrainLoaderOSMProperty("changeset")]
        public string ChangeSet { get; set; }
        [GISTerrainLoaderOSMProperty("timestamp")]
        public DateTime TimeStamp { get; set; }
        [GISTerrainLoaderOSMProperty("user")]
        public string User { get; set; }
        [GISTerrainLoaderOSMProperty("uid")]
        public string Uid { get; set; }
        [GISTerrainLoaderOSMProperty("lat")]
        public double Lat { get; set; }
        [GISTerrainLoaderOSMProperty("lon")]
        public double Lon { get; set; }
        public GISTerrainLoaderOSMTag MainTag { get; set; } = new GISTerrainLoaderOSMTag();

        public List<GISTerrainLoaderGeoDataBase> DataBase { get; set; } = new List<GISTerrainLoaderGeoDataBase>();
 
    }
    public class GISTerrainLoaderOSMTag
    {
        [GISTerrainLoaderOSMProperty("k")]
        public string Attribute { get; set; }
        [GISTerrainLoaderOSMProperty("v")]
        public string Value { get; set; }
    }
    public class GISTerrainLoaderOSMWay
    {
        [GISTerrainLoaderOSMProperty("id")]
        public string Id { get; set; }
        [GISTerrainLoaderOSMProperty("visible")]
        public bool Visible { get; set; }
        [GISTerrainLoaderOSMProperty("version")]
        public int Version { get; set; }
        [GISTerrainLoaderOSMProperty("changeset")]
        public string ChangeSet { get; set; }
        [GISTerrainLoaderOSMProperty("timestamp")]
        public DateTime TimeStamp { get; set; }
        [GISTerrainLoaderOSMProperty("user")]
        public string User { get; set; }
        [GISTerrainLoaderOSMProperty("uid")]
        public string Uid { get; set; }
        public List<GISTerrainLoaderNd> Nds { get; set; } = new List<GISTerrainLoaderNd>();
        public List<GISTerrainLoaderOSMNode> Nodes { get; set; } = new List<GISTerrainLoaderOSMNode>();
        public List<GISTerrainLoaderOSMTag> Tags { get; set; } = new List<GISTerrainLoaderOSMTag>();
        public GISTerrainLoaderOSMTag MainTag { get; set; } = new GISTerrainLoaderOSMTag();

        public List<GISTerrainLoaderGeoDataBase> DataBase { get; set; } = new List<GISTerrainLoaderGeoDataBase>();
        public List<string> GetDataBaseKeys()
        {
            List<string> Keys = new List<string>();
            foreach (var x in DataBase)
            {
                Keys.Add(x.Key);
            }
            return Keys;
        }
        public List<string> GetDataBaseValues()
        {
            List<string> Values = new List<string>();
            foreach (var x in DataBase)
            {
                Values.Add(x.Key);
            }
            return Values;
        }
        public bool TryGetValue(string attribute, out string value)
        {
            value = "";

            bool Contains = false;

            GISTerrainLoaderGeoDataBase element = DataBase.Find(x => x.Key == attribute);

            if (element != null)
            {
                value = element.Value;
                Contains = true;
            }

            return Contains;
        }
    }
    public class GISTerrainLoaderOSMRelation
    {
        [GISTerrainLoaderOSMProperty("id")]
        public string Id { get; set; }
        [GISTerrainLoaderOSMProperty("visible")]
        public bool Visible { get; set; }
        [GISTerrainLoaderOSMProperty("version")]
        public int Version { get; set; }
        [GISTerrainLoaderOSMProperty("changeset")]
        public string ChangeSet { get; set; }
        [GISTerrainLoaderOSMProperty("timestamp")]
        public DateTime TimeStamp { get; set; }
        [GISTerrainLoaderOSMProperty("user")]
        public string User { get; set; }

        [GISTerrainLoaderOSMProperty("area")]
        public string Area { get; set; }

        
        [GISTerrainLoaderOSMProperty("uid")]
        public string Uid { get; set; }
        public List<GISTerrainLoaderRelationMember> Members { get; set; } = new List<GISTerrainLoaderRelationMember>();
        public List<GISTerrainLoaderOSMTag> Tags { get; set; } = new List<GISTerrainLoaderOSMTag>();
        public GISTerrainLoaderOSMTag MainTag { get; set; } = new GISTerrainLoaderOSMTag();

        public List<GISTerrainLoaderGeoDataBase> DataBase { get; set; } = new List<GISTerrainLoaderGeoDataBase>();
        public List<string> GetDataBaseKeys()
        {
            List<string> Keys = new List<string>();
            foreach (var x in DataBase)
            {
                Keys.Add(x.Key);
            }
            return Keys;
        }
        public List<string> GetDataBaseValues()
        {
            List<string> Values = new List<string>();
            foreach (var x in DataBase)
            {
                Values.Add(x.Key);
            }
            return Values;
        }
        public bool TryGetValue(string attribute, out string value)
        {
            value = "";

            bool Contains = false;

            GISTerrainLoaderGeoDataBase element = DataBase.Find(x => x.Key == attribute);

            if (element != null)
            {
                value = element.Value;
                Contains = true;
            }

            return Contains;
        }
    }
    public class GISTerrainLoaderRelationMember
    {
        [GISTerrainLoaderOSMProperty("ref")]
        public string Ref { get; set; }

        [GISTerrainLoaderOSMProperty("role")]
        public string role { get; set; }

        [GISTerrainLoaderOSMProperty("type")]
        public string type { get; set; }


    }

    internal class GISTerrainLoaderParserHelper
    {
        public static Dictionary<string, PropertyInfo> GetOsmProperties(Type type)
        {
            var fields = type.GetProperties()
                .Select(c => new
                {
                    Property = c,
                    Attribute = (c.GetCustomAttributes().FirstOrDefault() as GISTerrainLoaderOSMPropertyAttribute)?.Name
                })
                .Where(c => c.Attribute != null)
                .ToDictionary(c => c.Attribute, f => f.Property);
            return fields;
        }
        public static void SetValue(object instance, PropertyInfo property, string value)
        {
            if (property.PropertyType == typeof(double))
            {
                if (Application.platform == RuntimePlatform.WebGLPlayer || Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
                    property.SetValue(instance, GISTerrainLoaderExtensions.GetDouble(value));
                else
                    property.SetValue(instance,GISTerrainLoaderExtensions.GetDouble(value));
            }
            else if (property.PropertyType == typeof(string))
            {
                property.SetValue(instance, value);
            }
            else if (property.PropertyType == typeof(int))
            {
                property.SetValue(instance, int.Parse(value));
            }
            else if (property.PropertyType == typeof(bool))
            {
                property.SetValue(instance, bool.Parse(value));
            }
            else if (property.PropertyType == typeof(DateTime))
            {
                var dt = DateTime.Parse(value);
                property.SetValue(instance, dt);
            }
        }
    }
    [AttributeUsage(AttributeTargets.Property)]
    internal class GISTerrainLoaderOSMPropertyAttribute : Attribute
    {
        public string Name { get; set; }

        public GISTerrainLoaderOSMPropertyAttribute(string name)
        {
            Name = name;
        }
    }
}