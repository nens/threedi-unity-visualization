/*     Unity GIS Tech 2020-2022      */

using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderAddDefinition
    { 

        [MenuItem("Tools/GIS Tech/GIS Terrain Loader/Install Libs/DotSpatial/Import_DotSpatial", false, 4)]
        static void Import_DotSpatial()
        {
            var LibPath = Path.Combine(Application.dataPath, "GIS Tech/GIS Terrain Loader/Resources/Libs/DotNetSpatial.unitypackage") ;
            AssetDatabase.ImportPackage(LibPath, false);

            GISTerrainLoaderDefinesHelper.AddSymbolToAllTargets("DotSpatial");
            Debug.Log("NetDotSpatial Lib Added to your Project");

            AssetDatabase.Refresh();
        }
        [MenuItem("Tools/GIS Tech/GIS Terrain Loader/Install Libs/DotSpatial/Remove_DotSpatial", false, 4)]
        static void Remove_DotSpatial()
        {
            var LibPath = Path.Combine(Application.dataPath, "GIS Tech/GIS Terrain Loader/Plugins/DotSpatial");

            if (Directory.Exists(LibPath))
            {
                FileUtil.DeleteFileOrDirectory(LibPath);

                var meta = LibPath + ".meta";
                if (File.Exists(meta))
                    FileUtil.DeleteFileOrDirectory(meta);

                GISTerrainLoaderDefinesHelper.RemoveSymbolFromAllTargets("DotSpatial");
            
                AssetDatabase.Refresh();

                Debug.Log("NetDotSpatial Lib Removed from your Project");

            }





        }



        [MenuItem("Tools/GIS Tech/GIS Terrain Loader/Install Libs/Pdal/Import_Pdal", false, 4)]
        static void Import_Pdal()
        {
            var LibPath = Path.Combine(Application.dataPath, "GIS Tech/GIS Terrain Loader/Resources/Libs/PdalLib.unitypackage");
            AssetDatabase.ImportPackage(LibPath, false);

            GISTerrainLoaderDefinesHelper.AddSymbolToAllTargets("GISTerrainLoaderPdal");
            Debug.Log("Pdal Lib Added to your Project");

            AssetDatabase.Refresh();
        }

        [MenuItem("Tools/GIS Tech/GIS Terrain Loader/Install Libs/Pdal/Remove_Pdal", false, 4)]
        static void Remove_Pdal()
        {
            var LibPath = Path.Combine(Application.dataPath, "GIS Tech/GIS Terrain Loader/Plugins/Lidar");

            if (Directory.Exists(LibPath))
            {
                FileUtil.DeleteFileOrDirectory(LibPath);

                var meta = LibPath + ".meta";
                if (File.Exists(meta))
                    FileUtil.DeleteFileOrDirectory(meta);

                GISTerrainLoaderDefinesHelper.RemoveSymbolFromAllTargets("GISTerrainLoaderPdal");

                AssetDatabase.Refresh();

                Debug.Log("NetDotSpatial Lib Removed from your Project");

            }
        }



        [MenuItem("Tools/GIS Tech/GIS Terrain Loader/Install Libs/GeoJson/Import_GeoJson", false, 4)]
        static void Import_GeoJson()
        {
            var LibPath = Path.Combine(Application.dataPath, "GIS Tech/GIS Terrain Loader/Resources/Libs/GeoJson.unitypackage");
            AssetDatabase.ImportPackage(LibPath, false);

            GISTerrainLoaderDefinesHelper.AddSymbolToAllTargets("GISTerrainLoaderGeoJson");

            Debug.Log("GeoJson.Net Lib Added to your Project");

            AssetDatabase.Refresh();
        }

        [MenuItem("Tools/GIS Tech/GIS Terrain Loader/Install Libs/GeoJson/Remove_GeoJson", false, 4)]
        static void Remove_GeoJson()
        {
            var LibPath = Path.Combine(Application.dataPath, "GIS Tech/GIS Terrain Loader/Plugins/GeoJSON.Net");
 
            if (Directory.Exists(LibPath))
            {
                FileUtil.DeleteFileOrDirectory(LibPath);

                var meta = LibPath + ".meta";
                if(File.Exists(meta))
                    FileUtil.DeleteFileOrDirectory(meta);

                GISTerrainLoaderDefinesHelper.RemoveSymbolFromAllTargets("GISTerrainLoaderGeoJson");

                AssetDatabase.Refresh();

                Debug.Log("GeoJSON.Net Lib Removed from your Project");

            }
        }


        [MenuItem("Tools/GIS Tech/GIS Terrain Loader/Install Libs/FileBrowser/Import_FileBrowser", false, 4)]
        static void Import_FileBrowser()
        {
            var LibPath = Path.Combine(Application.dataPath, "GIS Tech/GIS Terrain Loader/Resources/Libs/FileBrowser.unitypackage");

            if (File.Exists(LibPath))
            {
                AssetDatabase.ImportPackage(LibPath, false);

                GISTerrainLoaderDefinesHelper.AddSymbolToAllTargets("FileBrowser");
                Debug.Log("FileBrowser Lib Added to your Project");

                AssetDatabase.Refresh();
            }
            else
                Debug.Log("FileBrowser.unitypackage package not exists in GIS Tech/GIS Terrain Loader/Resources/Libs, try to download it from" +
                    " the main asset store page or from https://drive.google.com/file/d/18ihDQjUBs0-9i2H9BZZB9w002vNd585e/view?usp=sharing");

        }

        [MenuItem("Tools/GIS Tech/GIS Terrain Loader/Install Libs/FileBrowser/Remove_FileBrowser", false, 4)]
        static void Remove_FileBrowser()
        {
            var LibPath = Path.Combine(Application.dataPath, "SimpleFileBrowser");

            if (Directory.Exists(LibPath))
            {
                FileUtil.DeleteFileOrDirectory(LibPath);

                var meta = LibPath + ".meta";
                if (File.Exists(meta))
                    FileUtil.DeleteFileOrDirectory(meta);

                GISTerrainLoaderDefinesHelper.RemoveSymbolFromAllTargets("FileBrowser");

                AssetDatabase.Refresh();

                Debug.Log("FileBrowser Lib Removed from your Project");

            }
        }
    }

    public class GISTerrainLoaderDefinesHelper
    {
        public static void AddSymbolToAllTargets(string defineSymbol)
        {
            foreach (BuildTargetGroup group in Enum.GetValues(typeof(BuildTargetGroup)))
            {
                if (!IsValidBuildTargetGroup(group)) continue;

                var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';').Select(d => d.Trim()).ToList();
                if (!defineSymbols.Contains(defineSymbol))
                {
                    defineSymbols.Add(defineSymbol);
                    try
                    {
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", defineSymbols.ToArray()));
                    }
                    catch (Exception)
                    {
                        Debug.Log("Could not set defines for build target group: " + group);
                        throw;
                    }
                }
            }
        }

        public static void RemoveSymbolFromAllTargets(string defineSymbol)
        {
            foreach (BuildTargetGroup group in Enum.GetValues(typeof(BuildTargetGroup)))
            {
                if (!IsValidBuildTargetGroup(group)) continue;

                var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';').Select(d => d.Trim()).ToList();
                if (defineSymbols.Contains(defineSymbol))
                {
                    defineSymbols.Remove(defineSymbol);
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", defineSymbols.ToArray()));
                }
            }
        }

        private static bool IsValidBuildTargetGroup(BuildTargetGroup group)
        {
            if (group == BuildTargetGroup.Unknown || IsObsolete(group)) return false;
#if UNITY_5_3_0 || UNITY_5_3 
            if ((int)(object)group == 25) return false;
#endif

#if UNITY_5_4 || UNITY_5_5 
            if ((int)(object)group == 15) return false;
            if ((int)(object)group == 16) return false;
#endif
            if (Application.unityVersion.StartsWith("5.6"))
            {
                if ((int)(object)group == 27) return false;
            }

            return true;
        }

        private static bool IsObsolete(Enum value)
        {
            var enumInt = (int)(object)value;
            if (enumInt == 4 || enumInt == 14) return false;

            var field = value.GetType().GetField(value.ToString());
            var attributes = (ObsoleteAttribute[])field.GetCustomAttributes(typeof(ObsoleteAttribute), false);
            return attributes.Length > 0;
        }

    }
}