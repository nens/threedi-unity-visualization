/*     Unity GIS Tech 2020-2023      */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public delegate void RuntimeTerrainGeneratorEvents(GISTerrainContainer Container);
 
    public delegate void RuntimeTerrainGeneratorOrigine(DVector2 _origine, float minEle, float maxEle);
}