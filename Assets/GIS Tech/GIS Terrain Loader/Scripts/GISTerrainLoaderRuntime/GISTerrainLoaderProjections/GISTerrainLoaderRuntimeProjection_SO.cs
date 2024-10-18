/*     Unity GIS Tech 2020-2022      */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderRuntimeProjection_SO : ScriptableObject
    {
        public RuntimeProjection MainFileProjection = new RuntimeProjection();
        public List<RuntimeProjection> Projections = new List<RuntimeProjection>();

        /// <summary>
        /// Return a List of all projections exists in the RuntimeProjections ScriptableObject
        /// </summary>
        /// <returns></returns>
        public List<RuntimeProjection> GetProjection()
        {
            List < RuntimeProjection> m_Projection = new List<RuntimeProjection>();

            m_Projection.Add(MainFileProjection);

            //if (MainFileProjection.EPSG == 4326)
            //    m_Projection = Projections.Where(x => x.EPSG != 4326).ToList();


            //if (m_Projection != null)
            //{

            //    foreach (var Pro in m_Projection)
            //    {
            //        option = new UnityEngine.UI.Dropdown.OptionData(Pro.ProjectionName);

            //        options.Add(option);
            //    }
            //}

            return m_Projection;

        }
        public List<UnityEngine.UI.Dropdown.OptionData> GetOptions()
        {
            List<UnityEngine.UI.Dropdown.OptionData> options = new List<UnityEngine.UI.Dropdown.OptionData>();
           
            UnityEngine.UI.Dropdown.OptionData option = new UnityEngine.UI.Dropdown.OptionData(MainFileProjection.ProjectionName);
            
            options.Add(option);


            List<RuntimeProjection> m_Projection = Projections;

            if (MainFileProjection.EPSG == 4326)
                m_Projection = Projections.Where(x => x.EPSG != 4326).ToList();

            if (m_Projection!= null)
            {

                foreach (var Pro in m_Projection)
                {
                    option = new UnityEngine.UI.Dropdown.OptionData(Pro.ProjectionName);

                    options.Add(option);
                }
            }
 
            return options;
        }
        public int GetEPSG(string ProjectionName)
        {
            int epsg = 4326;

            if (MainFileProjection.ProjectionName.Equals(ProjectionName))
            {
                epsg = 4326;
            }else
            {
                foreach(var pro in Projections)
                {
                    if (pro.ProjectionName.Equals(ProjectionName))
                    {
                        epsg = pro.EPSG;
                        break;
                    }
                }    
            }

            return epsg;
        }
    }
    
    [Serializable]
    public class RuntimeProjection
    {
        public string ProjectionName;
        public int EPSG;

        public RuntimeProjection ()
        {
            ProjectionName = "";
            EPSG = 0;
        }
        public RuntimeProjection(string m_ProjectionName,int m_EPSG)
        {
            ProjectionName = m_ProjectionName;
            EPSG = m_EPSG;
        }
    }
}