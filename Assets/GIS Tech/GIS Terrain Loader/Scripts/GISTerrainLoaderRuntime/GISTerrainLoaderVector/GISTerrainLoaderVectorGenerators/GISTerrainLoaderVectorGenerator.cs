/*     Unity GIS Tech 2020-2023      */

using System.Collections.Generic;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{

    public abstract  class GISTerrainLoaderVectorGenerator
    {
        protected GISTerrainContainer container;
        protected GISTerrainLoaderPrefs Prefs;
        protected GISTerrainLoaderGeoVectorData GeoData;
        public abstract void Generate(GISTerrainLoaderPrefs m_prefs, GISTerrainLoaderGeoVectorData m_GeoData, GISTerrainContainer m_container);
  
    }
}