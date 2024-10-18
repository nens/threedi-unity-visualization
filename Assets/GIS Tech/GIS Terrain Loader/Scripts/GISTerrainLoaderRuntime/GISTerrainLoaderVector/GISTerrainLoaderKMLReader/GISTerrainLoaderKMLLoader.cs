using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderKMLLoader : GISTerrainLoaderGeoDataHolder
    {
        private static GISTerrainLoaderVectorParameters_SO VectorParameters;

        public static GISTerrainLoaderGeoVectorData fileData = new GISTerrainLoaderGeoVectorData("");
        public GISTerrainLoaderKMLLoader(string kmlfile)
        {
            if (VectorParameters == null)
                VectorParameters = GISTerrainLoaderVectorParameters_SO.LoadParameters();

            LoadKMLFile(kmlfile);
        }
         public static void LoadKMLFile(string kmlfile)
        {
            fileData = new GISTerrainLoaderGeoVectorData("");

            GISTerrainLoaderKMLReader KmlReader = new GISTerrainLoaderKMLReader(kmlfile);
            fileData = KmlReader.fileData;
        }
         public override GISTerrainLoaderGeoVectorData GetGeoFiltredData(string name, GISTerrainContainer container = null)
        {
            if (!VectorParameters.LoadDataOutOfContainerBounds && container)
            {
                return fileData.GetDataInContainerBounds(container);
            }
            else
            return fileData;
        }
    }
}