/*     Unity GIS Tech 2020-2023      */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif
namespace GISTech.GISTerrainLoader
{

#if UNITY_EDITOR 
    [CustomEditor(typeof(GISTerrainContainer))]
    public class GISTerrainLoaderTerrainContainerInfo : Editor
    {
        private TabsBlock tabs;
        private Texture2D m_resetPrefs;

        public string[] availableHeightSrt = new string[] { "32", "64", "128", "256", "512", "1024", "2048", "4096" };
        public string[] availableHeightsResolutionPrePectSrt = new string[] { "4", "8", "16", "32" };
        public string[] availableExportFiles = new string[] { "Raw" };
        private ExportDEMType exportDEMType = ExportDEMType.Raw;

        private RawDepth depth = RawDepth.Bit16;
        private RawByteOrder order = RawByteOrder.Windows;
        private string DEMSavePath;
        private string extension;

        private ExportAs exportAs = ExportAs.Png;

        private TiffElevation tiffElevation = TiffElevation.Auto;
        private Vector2 MinMaxElevation;


        private ExportVectorType exportVectorType = ExportVectorType.Shapfile;

        private CoordinatesSource coordinatesSource = CoordinatesSource.FromTerrain;

        private OptionEnabDisab StoreElevationValue = OptionEnabDisab.Disable;

        private RealWorldElevation ElevationMode = RealWorldElevation.Elevation;

        private string VectorSavePath;

        private GUIStyle TileStyle = new GUIStyle();
        private GISTerrainContainer ContainerObjectInfo { get { return target as GISTerrainContainer; } }
        private void OnEnable()
        {
            LoadPrefs();

            tabs = new TabsBlock(new Dictionary<string, System.Action>()
            {
                {"Terrain Metadata", TerrainMetadata},
                {"Terrain Parameters", TerrainParameters},
                   {"Export Terrain Data", Export}
            });

            if (ContainerObjectInfo)
                tabs.SetCurrentMethod(ContainerObjectInfo.lastTab);


            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.grey);
            texture.Apply();
            TileStyle.normal.background = texture;
            TileStyle.normal.textColor = Color.black;
            TileStyle.fontSize = 13;
            TileStyle.fontStyle = FontStyle.Bold;
            TileStyle.alignment = TextAnchor.MiddleLeft;
        }
        private void OnDisable()
        {
            SavePrefs();
        }
        private void TerrainMetadata()
        {
            using (new HorizontalBlock())
            {
                CoordinatesBarGUI();
            }

        }
        private void TerrainParameters()
        {
            using (new HorizontalBlock())
            {
                TerrainParametersGUI();
            }

        }
        private void Export()
        {
            using (new HorizontalBlock())
            {
                ExportToDEMGUI();
            }
            using (new HorizontalBlock())
            {
                ExportToVectorGUI();
            }
        }
        public override void OnInspectorGUI()
        {
            Undo.RecordObject(ContainerObjectInfo, "GTL_TerrainContainerInfo");
            tabs.Draw();
            if (GUI.changed)
                ContainerObjectInfo.lastTab = tabs.curMethodIndex;
            EditorUtility.SetDirty(ContainerObjectInfo);
        }
        private void CoordinatesBarGUI()
        {
            using (new VerticalBlock(GUI.skin.box))
            {
                GUILayout.Label("DEM File Projection / Bounds ", TileStyle);

                if (ContainerObjectInfo.data.EPSG != 0)
                {
                    using (new HorizontalBlock(GUI.skin.button))
                    {

                        GUILayout.Label(" Projection Name : " + GISTerrainLoaderEPSG.GetEPSGName(ContainerObjectInfo.data.EPSG));
                    }

                    using (new HorizontalBlock(GUI.skin.button))
                    {
                        GUILayout.Label(" EPSG Code :  " + ContainerObjectInfo.data.EPSG);
                    }
                    using (new HorizontalBlock(GUI.skin.button))
                    {
                        GUILayout.Label(" Upper Left :    ");
                        GUILayout.Label("");

                        GUILayout.Label("  X : ");
                        GUI.SetNextControlName("UpperLeftCoordianteLon");
                        GUILayout.Label(Math.Round(ContainerObjectInfo.data.TLOriginal_Coor.x, 10).ToString());
                        GUI.SetNextControlName("UpperLeftCoordianteLon");

                        GUILayout.Label("  Y : ");
                        GUI.SetNextControlName("UpperLeftCoordianteLat");
                        GUILayout.Label(Math.Round(ContainerObjectInfo.data.TLOriginal_Coor.y, 10).ToString());
                        GUI.SetNextControlName("UpperLeftCoordianteLon");

                    }

                    using (new HorizontalBlock(GUI.skin.button))
                    {
                        GUILayout.Label(" Bottom Right : ");
                        GUILayout.Label("", GUILayout.ExpandWidth(true));

                        GUILayout.Label("  X : ");
                        GUI.SetNextControlName("");
                        GUILayout.Label(Math.Round(ContainerObjectInfo.data.DROriginal_Coor.x, 10).ToString());
                        GUI.SetNextControlName("");

                        GUILayout.Label("  Y : ");
                        GUI.SetNextControlName("");
                        GUILayout.Label(Math.Round(ContainerObjectInfo.data.DROriginal_Coor.y, 10).ToString());
                        GUI.SetNextControlName("");



                    }
                }
                else
                {
                    using (new HorizontalBlock(GUI.skin.button))
                    {
                        GUILayout.Label(" Projection Name : Geographic Lat/Lon");
                    }

                    using (new HorizontalBlock(GUI.skin.button))
                    {
                        GUILayout.Label(" EPSG Code :    " + "4326");
                    }
                }
                GUILayout.Label("Terrain Bounds in Geographic Coordinates  ", TileStyle);

                using (new HorizontalBlock(GUI.skin.button))
                {
                    GUILayout.Label(" Upper Left :    ");
                    GUILayout.Label("");
                    GUILayout.Label("  Latitude : ");
                    GUI.SetNextControlName("");
                    GUILayout.Label(Math.Round(ContainerObjectInfo.data.TLPoint_LatLon.y, 10).ToString());
                    GUI.SetNextControlName("");

                    GUILayout.Label("  Longitude : ");
                    GUI.SetNextControlName("");
                    GUILayout.Label(Math.Round(ContainerObjectInfo.data.TLPoint_LatLon.x, 10).ToString());
                    GUI.SetNextControlName("");

                }

                using (new HorizontalBlock(GUI.skin.button))
                {
                    GUILayout.Label(" Bottom Right : ");
                    GUILayout.Label("", GUILayout.ExpandWidth(true));
                    GUILayout.Label("  Latitude : ");
                    GUI.SetNextControlName("");
                    GUILayout.Label(Math.Round(ContainerObjectInfo.data.DRPoint_LatLon.y, 10).ToString());
                    GUI.SetNextControlName("");

                    GUILayout.Label("  Longitude : ");
                    GUI.SetNextControlName("");
                    GUILayout.Label(Math.Round(ContainerObjectInfo.data.DRPoint_LatLon.x, 10).ToString());
                    GUI.SetNextControlName("");

                }

                GUILayout.Label("Terrain Dimension [Km] ", TileStyle);

                using (new HorizontalBlock(GUI.skin.button))
                {
                    GUILayout.Label("  Width :  ");
                    GUILayout.Label(Math.Round(ContainerObjectInfo.data.Dimensions.x, 2).ToString());

                    GUILayout.Label("  Lenght : ");
                    GUILayout.Label(Math.Round(ContainerObjectInfo.data.Dimensions.y, 2).ToString());

                }
                GUILayout.Label("Min Max Elevation [m] ", TileStyle);

                using (new HorizontalBlock(GUI.skin.button))
                {
                    GUILayout.Label("  Min :  ");
                    GUILayout.Label(Math.Round(ContainerObjectInfo.data.MinMaxElevation.x, 2).ToString());

                    GUILayout.Label("  Max :  ");
                    GUILayout.Label(Math.Round(ContainerObjectInfo.data.MinMaxElevation.y, 2).ToString());

                }

                GUILayout.Label("Terrain Scale Factor ", TileStyle);

                using (new HorizontalBlock(GUI.skin.button))
                {
                    GUILayout.Label("  X :  ");
                    GUILayout.Label(ContainerObjectInfo.Scale.x.ToString());

                    GUILayout.Label("  Y : ");
                    GUILayout.Label(ContainerObjectInfo.Scale.y.ToString());

                    GUILayout.Label("  Z : ");
                    GUILayout.Label(ContainerObjectInfo.Scale.z.ToString());

                }

                GUILayout.Label("Terrain Total Size [Terrain Unite] ", TileStyle);

                using (new HorizontalBlock(GUI.skin.button))
                {
                    GUILayout.Label("  X :  ");
                    GUILayout.Label(ContainerObjectInfo.ContainerSize.x.ToString());

                    GUILayout.Label("  Y : ");
                    GUILayout.Label(ContainerObjectInfo.ContainerSize.y.ToString());

                    GUILayout.Label("  Z : ");
                    GUILayout.Label(ContainerObjectInfo.ContainerSize.z.ToString());

                }

                GUILayout.Label("Terrains Count ", TileStyle);

                using (new HorizontalBlock(GUI.skin.button))
                {
                    GUILayout.Label("  X :  ");
                    GUILayout.Label(ContainerObjectInfo.TerrainCount.x.ToString());

                    GUILayout.Label("  Y : ");
                    GUILayout.Label(ContainerObjectInfo.TerrainCount.y.ToString());

                    GUILayout.Label("      ");
                    GUILayout.Label(" ");

                }
            }
        }
        private void TerrainParametersGUI()
        {
            using (new VerticalBlock(GUI.skin.box))
            {
                using (new VerticalBlock(GUI.skin.box))
                {
                    GUILayout.Label("Terrain Base prefs ", TileStyle);

                    using (new VerticalBlock(GUI.skin.box))
                    {
                        using (new HorizontalBlock(GUI.skin.button))
                        {
                            GUILayout.Label(new GUIContent(" Pixel Error ", " The accuracy of the mapping between Terrain maps (such as heightmaps and Textures) and generated Terrain. Higher values indicate lower accuracy, but with lower rendering overhead. "), GUILayout.MaxWidth(200));
                            ContainerObjectInfo.PixelErro = EditorGUILayout.Slider(ContainerObjectInfo.PixelErro, 1, 200, GUILayout.ExpandWidth(true));
                        }

                        using (new HorizontalBlock(GUI.skin.button))
                        {
                            GUILayout.Label(new GUIContent(" Base Map Dis ", " The maximum distance at which Unity displays Terrain Textures at full resolution. Beyond this distance, the system uses a lower resolution composite image for efficiency "), GUILayout.MaxWidth(200));
                            ContainerObjectInfo.BaseMapDistance = EditorGUILayout.Slider(ContainerObjectInfo.BaseMapDistance, 1, 20000, GUILayout.ExpandWidth(true));
                        }

                    }
                }

                using (new VerticalBlock(GUI.skin.box))
                {
                    GUILayout.Label("Tree & Details objects ", TileStyle);

                    using (new VerticalBlock(GUI.skin.box))
                    {
                        using (new HorizontalBlock(GUI.skin.button))
                        {
                            GUILayout.Label(new GUIContent("  Detail Distance ", " The distance from the camera beyond which details are culled "), GUILayout.MaxWidth(200));
                            ContainerObjectInfo.DetailDistance = EditorGUILayout.Slider(ContainerObjectInfo.DetailDistance, 10f, 400, GUILayout.ExpandWidth(true));
                        }

                        using (new HorizontalBlock(GUI.skin.button))
                        {
                            GUILayout.Label(new GUIContent("  Detail Density ", " The number of detail/grass objects in a given unit of area. Set this value lower to reduce rendering overhead "), GUILayout.MaxWidth(200));
                            ContainerObjectInfo.DetailDensity = EditorGUILayout.Slider(ContainerObjectInfo.DetailDensity, 0, 1, GUILayout.ExpandWidth(true));
                        }
                        using (new HorizontalBlock(GUI.skin.button))
                        {
                            GUILayout.Label(new GUIContent("  Tree Distance ", " The distance from the camera beyond which trees are culled "), GUILayout.MaxWidth(200));
                            ContainerObjectInfo.TreeDistance = EditorGUILayout.Slider(ContainerObjectInfo.TreeDistance, 1, 5000, GUILayout.ExpandWidth(true));
                        }
                        using (new HorizontalBlock(GUI.skin.button))
                        {
                            GUILayout.Label(new GUIContent("  Tree BillBoard Start Distance ", "The distance from the camera at which Billboard images replace 3D Tree objects"), GUILayout.MaxWidth(200));
                            ContainerObjectInfo.BillBoardStartDistance = EditorGUILayout.Slider(ContainerObjectInfo.BillBoardStartDistance, 1, 2000, GUILayout.ExpandWidth(true));
                        }
                        using (new HorizontalBlock(GUI.skin.button))
                        {
                            GUILayout.Label(new GUIContent("  Fade Length ", "The distance over which Trees transition between 3D objects and Billboards."), GUILayout.MaxWidth(200));
                            ContainerObjectInfo.FadeLength = EditorGUILayout.Slider(ContainerObjectInfo.FadeLength, 1, 200, GUILayout.ExpandWidth(true));
                        }
                        using (new HorizontalBlock(GUI.skin.button))
                        {
                            GUILayout.Label(new GUIContent(" Detail Resolution ", "The number of cells available for placing details onto the Terrain tile used to controls grass and detail meshes. Lower you set this number performance will be better"), GUILayout.MaxWidth(200));
                            ContainerObjectInfo.DetailResolution_index = EditorGUILayout.Popup(ContainerObjectInfo.DetailResolution_index, availableHeightSrt, GUILayout.ExpandWidth(true));
                        }

                        using (new HorizontalBlock(GUI.skin.button))
                        {
                            GUILayout.Label(new GUIContent(" Resolution Per Patch ", "The number of cells in a single patch (mesh), recommended value is 16 for very large detail object distance "), GUILayout.MaxWidth(200));
                            ContainerObjectInfo.ResolutionPerPatch_index = EditorGUILayout.Popup(ContainerObjectInfo.ResolutionPerPatch_index, availableHeightsResolutionPrePectSrt, GUILayout.ExpandWidth(true));
                        }

                        using (new HorizontalBlock(GUI.skin.button))
                        {
                            GUILayout.Label(new GUIContent(" Base Map Resolution ", "Resolution of the composite texture used on the terrain when viewed from a distance greater than the Basemap Distance"), GUILayout.MaxWidth(200));
                            ContainerObjectInfo.BaseMapResolution_index = EditorGUILayout.Popup(ContainerObjectInfo.BaseMapResolution_index, availableHeightSrt, GUILayout.ExpandWidth(true));
                        }

                    }
                }
            }
        }
        private void ExportToDEMGUI()
        {
            using (new VerticalBlock(GUI.skin.box))
            {
                using (new VerticalBlock(GUI.skin.box))
                {
                    GUILayout.Label(" Export Terrain To DEM File ", TileStyle);

                    using (new VerticalBlock(GUI.skin.box))
                    {
                        using (new HorizontalBlock(GUI.skin.button))
                        {
                            GUILayout.Label(new GUIContent(" Export To ", " Output file type "), GUILayout.MaxWidth(200));
                            exportDEMType = (ExportDEMType)EditorGUILayout.EnumPopup("", exportDEMType);
                        }

                        if (exportDEMType == ExportDEMType.Raw)
                        {
                            extension = "raw";
                            using (new HorizontalBlock(GUI.skin.button))
                            {
                                GUILayout.Label(new GUIContent(" Depth ", "  "), GUILayout.MaxWidth(200));
                                depth = (RawDepth)EditorGUILayout.EnumPopup("", depth);
                            }
                            using (new HorizontalBlock(GUI.skin.button))
                            {
                                GUILayout.Label(new GUIContent(" ByteOrder ", " Output file type "), GUILayout.MaxWidth(200));
                                order = (RawByteOrder)EditorGUILayout.EnumPopup("", order);
                            }
                        }

                        if (exportDEMType == ExportDEMType.Png)
                        {
                            extension = "png";

                            using (new HorizontalBlock(GUI.skin.button))
                            {
                                GUILayout.Label(new GUIContent(" Save File As ", " PNG/JPG "), GUILayout.MaxWidth(200));
                                exportAs = (ExportAs)EditorGUILayout.EnumPopup("", exportAs);
                            }

                        }
                        if (exportDEMType == ExportDEMType.Tiff)
                        {
                            extension = "tif";

                            using (new HorizontalBlock(GUI.skin.button))
                            {
                                GUILayout.Label(new GUIContent(" Tiff Elevation Mode ", "In auto mode the range (Min-Max) Elevation will be the same as the default DEM Real world data, you can also customize the min and max elevation in Custom Mode "), GUILayout.MaxWidth(200));
                                tiffElevation = (TiffElevation)EditorGUILayout.EnumPopup("", tiffElevation);


                            }
                            if (tiffElevation == TiffElevation.Custom)
                            {
                                using (new HorizontalBlock(GUI.skin.button))
                                {
                                    GUILayout.Label("Min Elevation : ", GUILayout.MaxWidth(100), GUILayout.ExpandWidth(false));
                                    GUI.SetNextControlName("UpperLeftCoordianteLat");
                                    MinMaxElevation.x = EditorGUILayout.FloatField(MinMaxElevation.x, GUILayout.ExpandWidth(true));

                                    GUILayout.Label("Max Elevation : ", GUILayout.MaxWidth(100), GUILayout.ExpandWidth(false));
                                    GUI.SetNextControlName("UpperLeftCoordianteLat");
                                    MinMaxElevation.y = EditorGUILayout.FloatField(MinMaxElevation.y, GUILayout.ExpandWidth(true));
                                }
                            }



                        }
                    }
                    using (new VerticalBlock(GUI.skin.box))
                    {
                        using (new HorizontalBlock(GUI.skin.button))
                        {
                            GUILayout.Label(new GUIContent(" Path ", " Selete a location to save the exported file "), GUILayout.MaxWidth(200));
                            DEMSavePath = EditorGUILayout.TextField("", DEMSavePath);

                            if (GUILayout.Button(new GUIContent(" Select Location ", "Save heightmap to specific file by opening dialoge"), GUILayout.ExpandWidth(false)))
                            {
                                if (string.IsNullOrEmpty(DEMSavePath)) DEMSavePath = Application.dataPath;
                                DEMSavePath = EditorUtility.SaveFilePanel("Export Heightmap", DEMSavePath, "heightmap." + extension, extension);
                                if (string.IsNullOrEmpty(DEMSavePath)) return;
                            }
                        }
                        using (new HorizontalBlock(GUI.skin.box))
                        {
                            if (GUILayout.Button(new GUIContent(" Export ", "Click to export file"), GUILayout.ExpandWidth(true)))
                            {
                                if (string.IsNullOrEmpty(DEMSavePath)) return;

                                switch (extension)
                                {
                                    case "raw":

                                        GISTerrainLoaderRawExporter RawExporter = new GISTerrainLoaderRawExporter(DEMSavePath, depth, order, ContainerObjectInfo);
                                        RawExporter.ExportToRaw();
                                        break;

                                    case "png":

                                        GISTerrainLoaderPngExporter PngExporter = new GISTerrainLoaderPngExporter(DEMSavePath, ContainerObjectInfo, exportAs);
                                        PngExporter.ExportToPng();
                                        break;

                                    case "tif":

                                        GISTerrainLoaderTiffExporter TiffExporter = new GISTerrainLoaderTiffExporter(DEMSavePath, ContainerObjectInfo, tiffElevation, MinMaxElevation);
                                        TiffExporter.ExportToTiff();
                                        break;
                                }

                            }

                        }
                    }

                }

            }
        }
        private void ExportToVectorGUI()
        {
            using (new VerticalBlock(GUI.skin.box))
            {
                using (new VerticalBlock(GUI.skin.box))
                {
                    GUILayout.Label(" Export To Vector Data ", TileStyle);

                    using (new VerticalBlock(GUI.skin.box))
                    {
                        using (new HorizontalBlock(GUI.skin.button))
                        {
                            GUILayout.Label(new GUIContent(" Export To ", " Output file type "), GUILayout.MaxWidth(200));
                            exportVectorType = (ExportVectorType)EditorGUILayout.EnumPopup("", exportVectorType);
                        }

                        if (exportVectorType == ExportVectorType.Shapfile)
                        {
                            extension = "shp";

                            
                             using (new HorizontalBlock(GUI.skin.button))
                            {
                                GUILayout.Label(new GUIContent(" Coordinates Source ", "Get all gameobjects of type 'GISTerrainLoaderVectorPoint' / 'GISTerrainLoaderVectorLine' / 'GISTerrainLoaderVectorPolygon', From Terrain means coordinates and Z values which will be exported to as vector data will be updated according to the gameobject position, but 'From Geodata' the export will use directly data inside of one of mentioned scripts "), GUILayout.MaxWidth(200));
                                coordinatesSource = (CoordinatesSource)EditorGUILayout.EnumPopup("", coordinatesSource);
                            }
                            using (new HorizontalBlock(GUI.skin.button))
                            {
                                GUILayout.Label(new GUIContent(" Store Z value ", "Enable this option to Get and Store Elevation from scene and save it to ShapeFile"), GUILayout.MaxWidth(200));
                                StoreElevationValue = (OptionEnabDisab)EditorGUILayout.EnumPopup("", StoreElevationValue);
                            }


                            if(StoreElevationValue == OptionEnabDisab.Enable && coordinatesSource == CoordinatesSource.FromTerrain)
                            {
                                using (new HorizontalBlock(GUI.skin.button))
                                {
                                    GUILayout.Label(new GUIContent(" Elevation Mode ", "Source to obtain Elevation of each point/Gameobject inside of the container"), GUILayout.MaxWidth(200));
                                    ElevationMode = (RealWorldElevation)EditorGUILayout.EnumPopup("", ElevationMode);
                                }
                            }

                        }
                    }
                    using (new VerticalBlock(GUI.skin.box))
                    {
                        using (new HorizontalBlock(GUI.skin.button))
                        {
                            GUILayout.Label(new GUIContent(" Path ", " Selete a location to save the exported file "), GUILayout.MaxWidth(200));
                            VectorSavePath = EditorGUILayout.TextField("", VectorSavePath);

                            if (GUILayout.Button(new GUIContent(" Select Location ", "Save VectorData to a specific file by opening dialoge"), GUILayout.ExpandWidth(false)))
                            {
                                if (string.IsNullOrEmpty(VectorSavePath)) VectorSavePath = Application.dataPath;

                                VectorSavePath = EditorUtility.SaveFilePanel("Export Vector File", Application.dataPath, "VectorFile." + extension, extension);
                                if (string.IsNullOrEmpty(DEMSavePath)) return;
                            }
                        }

                        using (new HorizontalBlock(GUI.skin.box))
                        {
                            if (GUILayout.Button(new GUIContent(" Export ", "Click to export file"), GUILayout.ExpandWidth(true)))
                            {
                                if (string.IsNullOrEmpty(VectorSavePath)) return;

                                bool m_StoreElevationValue = false;
                                if (StoreElevationValue == OptionEnabDisab.Enable) m_StoreElevationValue = true; else m_StoreElevationValue = false;
                                
                                ContainerObjectInfo.GetStoredHeightmap();

                                GISTerrainLoaderGeoVectorData m_GeoData = GISTerrainLoaderGeoVectorData.GetGeoDataFromScene(ContainerObjectInfo,coordinatesSource, m_StoreElevationValue, ElevationMode);

                                ContainerObjectInfo.ExportVectorData(exportVectorType, VectorSavePath, m_GeoData, m_StoreElevationValue);


                            }

                        }
                    }

                }

            }
        }
        private void SavePrefs()
        {
            GISTerrainLoaderSaveLoadPrefs.SavePref("VectorSavePath", VectorSavePath);
            GISTerrainLoaderSaveLoadPrefs.SavePref("StoreElevationValue", (int)StoreElevationValue);
        }
        private void LoadPrefs()
        {
            VectorSavePath = GISTerrainLoaderSaveLoadPrefs.LoadPref("VectorSavePath", Application.dataPath);
            StoreElevationValue = (OptionEnabDisab) GISTerrainLoaderSaveLoadPrefs.LoadPref("StoreElevationValue", 0);
        }
    }

#endif

}