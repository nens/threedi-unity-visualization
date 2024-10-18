/*     Unity GIS Tech 2020-2022      */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR 
using UnityEditor;
#endif
namespace GISTech.GISTerrainLoader
{
#if UNITY_EDITOR
    [CustomEditor(typeof(GISTerrainLoaderRuntimePrefs))]
    public class GISTerrainLoaderRuntimeTerrainGenerator : Editor
    {
        private GISTerrainLoaderRuntimePrefs Prefs { get { return target as GISTerrainLoaderRuntimePrefs; } }

        private TabsBlock tabs;

        private Texture2D m_resetPrefs;

        private void OnEnable()
        {
            tabs = new TabsBlock(new Dictionary<string, System.Action>()
            {
                {"DEM Terrain", DEMFileTab},
                {"Elevation,Scaling..", ElevationScalingTab},
                {"Terrain Preferences", TerrainPreferencesTab},
                {"Texturing", TexturingTab},
                {"Smoothing", SmoothingTab},
                {"Vector Data", VectorDataTab},
                {"Options", OptionsTab}
            });
            tabs.SetCurrentMethod(Prefs.Prefs.lastTab);

            if (m_resetPrefs == null)
                m_resetPrefs = LoadTexture("GTL_ResetPrefs");
        }
        private void DEMFileTab()
        {
            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" File Reading Mode "," Default is full heightmap mode, used to read whole hightmap file; sub region mode used to import a sub region of the file instead, note that coordinates of sub regions is needed; this option available only for GeoRefenced files (Tiff,HGT,BIL,ASC,FLT)"), GUILayout.MaxWidth(200));
                Prefs.Prefs.readingMode = (ReadingMode)EditorGUILayout.EnumPopup("", Prefs.Prefs.readingMode);
            }

            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" Projection Mode ", " Use this option to customize the projection of tiff file by setting EPSG code, note that DotSpatial lib is required"), GUILayout.MaxWidth(200));

                Prefs.Prefs.projectionMode = (ProjectionMode)EditorGUILayout.EnumPopup("", Prefs.Prefs.projectionMode);
            }
            using (new HorizontalBlock())
            {
                if (Prefs.Prefs.projectionMode == ProjectionMode.Custom_EPSG)
                {
                    GUILayout.Label(new GUIContent("  EPSG Code ", " Set the Projection Code"), GUILayout.MaxWidth(200));
                    Prefs.Prefs.EPSGCode = EditorGUILayout.IntField("", Prefs.Prefs.EPSGCode);
                }
                 else if (Prefs.Prefs.projectionMode == ProjectionMode.Geographic)
                    Prefs.Prefs.EPSGCode = 0;
            }



            using (new VerticalBlock())
            {
                if (Prefs.Prefs.readingMode == ReadingMode.SubRegion)
                {
                    CoordinatesBarGUI();
                }

            }
        }
        private void ElevationScalingTab()
        {
            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" Terrain Elevation Mode ", "Generate Terrain By Loading Real Elevation Data or By using 'Exaggeration' value to set manualy terrain elevation factor"), GUILayout.MaxWidth(200));
                Prefs.Prefs.TerrainElevation = (TerrainElevation)EditorGUILayout.EnumPopup("", Prefs.Prefs.TerrainElevation);
            }
            using (new VerticalBlock())
            {
                if (Prefs.Prefs.TerrainElevation == TerrainElevation.RealWorldElevation)
                {

                }
                else
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent(" Exaggeration value ", "Vertical exaggeration can be used to emphasize subtle changes in a surface. This can be useful in creating visualizations of terrain where the horizontal extent of the surface is significantly greater than the amount of vertical change in the surface. A fractional vertical exaggeration can be used to flatten surfaces or features that have extreme vertical variation"), GUILayout.MaxWidth(200));
                    Prefs.Prefs.TerrainExaggeration = EditorGUILayout.Slider(Prefs.Prefs.TerrainExaggeration, 0, 1);
                    GUILayout.EndHorizontal();
                }
 

            }

            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" Terrain Dimensions Mode ", "This Option let us to load real terrain Width/Lenght for almost of supported types - We can set it to manual to make terrain small or large as we want by setting new W/L values in 'KM' "), GUILayout.MaxWidth(200));
                Prefs.Prefs.terrainDimensionMode = (TerrainDimensionsMode)EditorGUILayout.EnumPopup("", Prefs.Prefs.terrainDimensionMode);
            }
            using (new HorizontalBlock())
            {
            if (Prefs.Prefs.terrainDimensionMode == TerrainDimensionsMode.AutoDetection)
            {

                if (!Prefs.Prefs.TerrainHasDimensions)
                {
                    GUILayout.BeginHorizontal();

                    GUILayout.Label(new GUIContent(" Terrain Dimensions (Km)    ", "This Showing because DEM file not loaded yet or has no real dimensions so we have to set Manualy terrain width/lenght in KM"), GUILayout.MaxWidth(220));

                    GUILayout.Label(" Width ");
                    Prefs.Prefs.TerrainDimensions.x = EditorGUILayout.DoubleField(Prefs.Prefs.TerrainDimensions.x, GUILayout.ExpandWidth(true));

                    GUILayout.Label(" Lenght ");
                    Prefs.Prefs.TerrainDimensions.y = EditorGUILayout.DoubleField(Prefs.Prefs.TerrainDimensions.y, GUILayout.ExpandWidth(true));
                    GUILayout.EndHorizontal();

                }
            }
            else
if (Prefs.Prefs.terrainDimensionMode == TerrainDimensionsMode.Manual)
            {
                GUILayout.BeginHorizontal();

                GUILayout.Label(new GUIContent(" Terrain Dimensions (Km)     ", "Set Manualy terrain width/lenght in KM"), GUILayout.MaxWidth(220));

                GUILayout.Label(" Width ");
                Prefs.Prefs.TerrainDimensions.x = EditorGUILayout.DoubleField(Prefs.Prefs.TerrainDimensions.x, GUILayout.ExpandWidth(true));

                GUILayout.Label(" Lenght ");
                Prefs.Prefs.TerrainDimensions.y = EditorGUILayout.DoubleField(Prefs.Prefs.TerrainDimensions.y, GUILayout.ExpandWidth(true));

                GUILayout.EndHorizontal();

            }

            }
 
            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" UnderWater ", "Enable This Option to load negative values from DEM files "), GUILayout.MaxWidth(200));
                Prefs.Prefs.UnderWater = (OptionEnabDisab)EditorGUILayout.EnumPopup("", Prefs.Prefs.UnderWater);
            }
            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" Fix Terrain ", " (Only for Real World Data) Use this option to fix null terrain elevations + unkown Min/Max values  + avoid extrem elevation values in order to generate terrain without any deformation Manually or Automatically "), GUILayout.MaxWidth(200));
                Prefs.Prefs.TerrainFixOption = (FixOption)EditorGUILayout.EnumPopup("", Prefs.Prefs.TerrainFixOption);
            }

            if (Prefs.Prefs.TerrainFixOption != FixOption.Disable)
            {
                using (new HorizontalBlock())
                {
                    GUILayout.Label(new GUIContent(" Elevation For Null Points ", " (Only for Real World Data) average will set an average elevation value to null points, but manual will give the ability to set manually the elevation value "), GUILayout.MaxWidth(200));
                    Prefs.Prefs.ElevationForNullPoints = (EmptyPoints)EditorGUILayout.EnumPopup("", Prefs.Prefs.ElevationForNullPoints);
                }
         
                if (Prefs.Prefs.ElevationForNullPoints == EmptyPoints.Manual)
                {
                    using (new HorizontalBlock())
                    {
                        GUILayout.Label(new GUIContent(" Elevation Value For Null Points ", "Set Manually the Elevation Value For for each null Points in the DEM File"), GUILayout.MaxWidth(200));
                        Prefs.Prefs.ElevationValueForNullPoints = EditorGUILayout.FloatField(Prefs.Prefs.ElevationValueForNullPoints);
                    }
                }
            }

            if (Prefs.Prefs.TerrainFixOption == FixOption.ManualFix)
            {
                using (new HorizontalBlock())
                {
                    GUILayout.Label(new GUIContent(" Elevation [m] ", "Set Manually terrain Max and Min Elevation in [m]"), GUILayout.MaxWidth(200));
                    GUILayout.Label("  Min ");
                    Prefs.Prefs.TerrainMaxMinElevation.x = EditorGUILayout.FloatField(Prefs.Prefs.TerrainMaxMinElevation.x, GUILayout.ExpandWidth(true));

                    GUILayout.Label("  Max ");
                    Prefs.Prefs.TerrainMaxMinElevation.y = EditorGUILayout.FloatField(Prefs.Prefs.TerrainMaxMinElevation.y, GUILayout.ExpandWidth(true));

                }
 
            }

            using (new HorizontalBlock())
            {

                GUILayout.Label(new GUIContent(" Terrain Scale ", " Specifies the terrain scale factor in three directions (if terrain is large with 1 value you can set small float value like 0.5f - 0.1f - 0.01f"), GUILayout.MaxWidth(200));
                Prefs.Prefs.terrainScale = EditorGUILayout.Vector3Field("", Prefs.Prefs.terrainScale);
            }

            if (Prefs.Prefs.terrainScale.x == 0 || Prefs.Prefs.terrainScale.y == 0 || Prefs.Prefs.terrainScale.z == 0)
                EditorGUILayout.HelpBox("Check your Terrain Scale (Terrain Scale is null !)", MessageType.Warning);


        }
        private void TerrainPreferencesTab()
        {

            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" Heightmap Resolution ", "The pixel resolution of the Terrain’s heightmap"), GUILayout.MaxWidth(200));
                Prefs.Prefs.heightmapResolution_index = EditorGUILayout.Popup(Prefs.Prefs.heightmapResolution_index, Prefs.Prefs.heightmapResolutionsSrt, GUILayout.ExpandWidth(true));
            }

            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" Detail Resolution ", "The number of cells available for placing details onto the Terrain tile used to controls grass and detail meshes. Lower you set this number performance will be better"), GUILayout.MaxWidth(200));
                Prefs.Prefs.detailResolution_index = EditorGUILayout.Popup(Prefs.Prefs.detailResolution_index, Prefs.Prefs.availableHeightSrt, GUILayout.ExpandWidth(true));
            }


            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" Resolution Per Patch ", "The number of cells in a single patch (mesh), recommended value is 16 for very large detail object distance "), GUILayout.MaxWidth(200));
                Prefs.Prefs.resolutionPerPatch_index = EditorGUILayout.Popup(Prefs.Prefs.resolutionPerPatch_index, Prefs.Prefs.availableHeightsResolutionPrePectSrt, GUILayout.ExpandWidth(true));
            }

            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" Base Map Resolution ", "Resolution of the composite texture used on the terrain when viewed from a distance greater than the Basemap Distance"), GUILayout.MaxWidth(200));
                Prefs.Prefs.baseMapResolution_index = EditorGUILayout.Popup(Prefs.Prefs.baseMapResolution_index, Prefs.Prefs.availableHeightSrt, GUILayout.ExpandWidth(true));
            }
            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" Pixel Error ", " The accuracy of the mapping between Terrain maps (such as heightmaps and Textures) and generated Terrain. Higher values indicate lower accuracy, but with lower rendering overhead. "), GUILayout.MaxWidth(200));
                Prefs.Prefs.PixelError = EditorGUILayout.Slider(Prefs.Prefs.PixelError, 1f, 200f, GUILayout.ExpandWidth(true));
            }
            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" BaseMap Distance ", " The maximum distance at which Unity displays Terrain Textures at full resolution. Beyond this distance, the system uses a lower resolution composite image for efficiency "), GUILayout.MaxWidth(200));
                Prefs.Prefs.BaseMapDistance = EditorGUILayout.Slider(Prefs.Prefs.BaseMapDistance, 1, 20000, GUILayout.ExpandWidth(true));
            }

            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" Terrain Material Mode ", "This option used to cutomize terrain material ex : in case of using HDRP "), GUILayout.MaxWidth(200));
                Prefs.Prefs.terrainMaterialMode = (TerrainMaterialMode)EditorGUILayout.EnumPopup("", Prefs.Prefs.terrainMaterialMode, GUILayout.ExpandWidth(true));
            }
            using (new HorizontalBlock())
            {
                if (Prefs.Prefs.terrainMaterialMode == TerrainMaterialMode.HeightmapColorRampContourLines)
                {
                    GUILayout.Label(new GUIContent(" Contour Interval [m] ", " Contour Interval in meter"), GUILayout.MaxWidth(200));
                    Prefs.Prefs.ContourInterval = EditorGUILayout.Slider(Prefs.Prefs.ContourInterval, 5, 200, GUILayout.ExpandWidth(true));
                }
            }
          

            if (Prefs.Prefs.terrainMaterialMode == TerrainMaterialMode.Custom)
            {
                using (new HorizontalBlock())
                {
                    GUILayout.Label(new GUIContent("  Terrain Material ", "Materail that will be used in the generated terrains "), GUILayout.MaxWidth(200));
                    Prefs.Prefs.terrainMaterial = (Material)EditorGUILayout.ObjectField(Prefs.Prefs.terrainMaterial, typeof(UnityEngine.Material),true ,GUILayout.ExpandWidth(true));
                }
            }

            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" Terrain Baseboards ", "Enable This option to genertate a Terrain Baseboards "), GUILayout.MaxWidth(200));
                Prefs.Prefs.TerrainBaseboards = (OptionEnabDisab)EditorGUILayout.EnumPopup("", Prefs.Prefs.TerrainBaseboards);
            }

            if (Prefs.Prefs.TerrainBaseboards == OptionEnabDisab.Enable)
            {
                using (new HorizontalBlock())
                {
                    GUILayout.Label(new GUIContent("  Border High ", " Border High value according to the Terrain minimum elevation"), GUILayout.MaxWidth(200));
                    Prefs.Prefs.BorderHigh = EditorGUILayout.IntSlider(Prefs.Prefs.BorderHigh, -150, 0, GUILayout.MaxWidth(200), GUILayout.ExpandWidth(true));

                }
                using (new HorizontalBlock())
                {
                    GUILayout.Label(new GUIContent("  Terrain Border Material ", "Materail that will be used for the terrain border "), GUILayout.MaxWidth(200));
                    Prefs.Prefs.terrainBorderMaterial = (Material)EditorGUILayout.ObjectField(Prefs.Prefs.terrainBorderMaterial, typeof(UnityEngine.Material), true, GUILayout.ExpandWidth(true));

                }

            }


            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" Terrain Background ", "Enable This option to genertate a Background to your Terrain"), GUILayout.MaxWidth(200));
                Prefs.Prefs.TerrainBackground = (OptionEnabDisab)EditorGUILayout.EnumPopup("", Prefs.Prefs.TerrainBackground);
            }
 

            if (Prefs.Prefs.TerrainBackground == OptionEnabDisab.Enable)
            {
                using (new HorizontalBlock())
                {
                    GUILayout.Label(new GUIContent("  Heightmap Resolution ", "The pixel resolution of the Terrain’s heightmap"), GUILayout.MaxWidth(200));
                    Prefs.Prefs.TerrainBackgroundHeightmapResolution_index = EditorGUILayout.Popup(Prefs.Prefs.TerrainBackgroundHeightmapResolution_index, Prefs.Prefs.TerrainBackgroundHeightmapResolutionsSrt, GUILayout.ExpandWidth(true));
                }
                using (new HorizontalBlock())
                {
                    GUILayout.Label(new GUIContent("  Texture Resolution ", "The pixel resolution of the Terrain’s heightmap"), GUILayout.MaxWidth(200));
                    Prefs.Prefs.TerrainBackgroundTextureResolution_index = EditorGUILayout.Popup(Prefs.Prefs.TerrainBackgroundTextureResolution_index, Prefs.Prefs.TerrainBackgroundTextureResolutionsSrt, GUILayout.ExpandWidth(true));
                }


            }

            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" Set Terrain Layer ", "This option cutomize terrain Layer "), GUILayout.MaxWidth(200));
                Prefs.Prefs.TerrainLayerSet = (OptionEnabDisab)EditorGUILayout.EnumPopup("", Prefs.Prefs.TerrainLayerSet, GUILayout.ExpandWidth(true));
            }

            if (Prefs.Prefs.TerrainLayerSet == OptionEnabDisab.Enable)
            {

                using (new HorizontalBlock())
                {
                    GUILayout.Label(new GUIContent(" Terrain Layer ", " Set Terrain Layer"), GUILayout.MaxWidth(220));
 
                    Prefs.Prefs.TerrainLayer = EditorGUILayout.IntField(Prefs.Prefs.TerrainLayer, GUILayout.ExpandWidth(true));
                }


            }
        }
        private void TexturingTab()
        {
            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" Texturing Mode ", "Generate Terrain with or without textures (Specifies the count terrains is needed when selecting 'Without' because Texture folder will not readed "), GUILayout.MaxWidth(200));
                Prefs.Prefs.textureMode = (TextureMode)EditorGUILayout.EnumPopup("", Prefs.Prefs.textureMode, GUILayout.ExpandWidth(true));
            }
 
            switch(Prefs.Prefs.textureMode)
            {
                case TextureMode.WithTexture:

                    using (new HorizontalBlock())
                    {
                        GUILayout.Label(new GUIContent("  Textures Loading Mode ", " The Creation of terrain tiles is based on the number of texture tiles existing in the terrain texture folder, setting this parameter to Auto means that GTL will load and generate terrains by loading directly textures from texture folder /// if it set to Manually, GTL will make some operations of merging and spliting existing textures to make them simulair to terrain tiles count ' Attention' : this operation may consume memory when textures are larges '"), GUILayout.MaxWidth(200));
                       Prefs.Prefs.textureloadingMode = (TexturesLoadingMode)EditorGUILayout.EnumPopup("", Prefs.Prefs.textureloadingMode, GUILayout.ExpandWidth(true));
                    }

                    if (Prefs.Prefs.textureloadingMode == TexturesLoadingMode.Manual)
                    {
                        using (new HorizontalBlock())
                        {
                            GUILayout.Label(new GUIContent("   Count Tiles ", " Specifie the number of terrain tiles , ' Attention '  Count Tiles set is different than the number of terrain textures tiles (Located in the Terrain texture folder), some operations (Spliting/mergins) textures will excuted so becarful when textures are large"), GUILayout.MaxWidth(200));
                            Prefs.Prefs.terrainCount = EditorGUILayout.Vector2IntField("", Prefs.Prefs.terrainCount);
                        }
                        EditorGUILayout.HelpBox("' Attention Memory ' When terrain Count Tiles is different than the number of terrain textures tiles(Located in the Terrain texture folder), some operations(Spliting / Mergins) textures will excuted so becarful for large textures ", MessageType.Warning);
                    }
                    break;

                case TextureMode.WithoutTexture:

                    using (new HorizontalBlock())
                    {
                        GUILayout.Label(new GUIContent(" Count Tiles ", " Number of Terrain Tiles in X-Y, This Option is avalaible Only when Texturing mode set to 'Without' or 'Splatmapping' "), GUILayout.MaxWidth(200));
                        Prefs.Prefs.terrainCount = EditorGUILayout.Vector2IntField("", Prefs.Prefs.terrainCount);
                    }

                    using (new HorizontalBlock())
                    {
                        GUILayout.Label(new GUIContent(" Use Custom Terrain Color ", "Enable/Disable customize terrain color "), GUILayout.MaxWidth(200));
                        Prefs.Prefs.UseTerrainEmptyColor = (OptionEnabDisab)EditorGUILayout.EnumPopup("", Prefs.Prefs.UseTerrainEmptyColor, GUILayout.ExpandWidth(true));
 
                    }

                    if (Prefs.Prefs.UseTerrainEmptyColor == OptionEnabDisab.Enable)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent("  Terrain Color ", "Used to change the main terrain color"), GUILayout.MaxWidth(200));
                        Prefs.Prefs.TerrainEmptyColor = EditorGUILayout.ColorField("", Prefs.Prefs.TerrainEmptyColor, GUILayout.ExpandWidth(true));
                        GUILayout.EndHorizontal();
                    }

                    break;

                case TextureMode.ShadedRelief:

                    using (new HorizontalBlock())
                    {
                        GUILayout.Label(new GUIContent(" Count Tiles ", " Number of Terrain Tiles in X-Y, This Option is avalaible Only when Texturing mode set to 'Without' or 'Splatmapping' "), GUILayout.MaxWidth(200));
                        Prefs.Prefs.terrainCount = EditorGUILayout.Vector2IntField("", Prefs.Prefs.terrainCount);
                    }

                    using (new HorizontalBlock())
                    {
                        GUILayout.Label(new GUIContent(" Shader Type ", " Select terrain type "), GUILayout.MaxWidth(200));
                        Prefs.Prefs.TerrainShaderType = (ShaderType)EditorGUILayout.EnumPopup("", Prefs.Prefs.TerrainShaderType, GUILayout.ExpandWidth(true));
                    }

                    using (new HorizontalBlock())
                    {
                        GUILayout.Label(new GUIContent(" UnderWater ", "Enable This Option to generate shaders to underwater terrains (Used to avoid blue color) "), GUILayout.MaxWidth(200));
                        Prefs.Prefs.UnderWaterShader = (OptionEnabDisab)EditorGUILayout.EnumPopup("", Prefs.Prefs.UnderWaterShader);
                    }

                    using (new HorizontalBlock())
                    {
                        GUILayout.Label(new GUIContent(" Save Shader Texture ", "Enable This Option to save the generated shaders as textures (For Runtime in ' Source Terrain Folder'), the texture resolution equal to terrain hightmap resolution"), GUILayout.MaxWidth(200));
                        Prefs.Prefs.SaveShaderTextures = (OptionEnabDisab)EditorGUILayout.EnumPopup("", Prefs.Prefs.SaveShaderTextures);
                    }


                    break;

                case TextureMode.Splatmapping:

                    using (new HorizontalBlock())
                    {
                        GUILayout.Label(new GUIContent(" Count Tiles ", " Number of Terrain Tiles in X-Y, This Option is avalaible Only when Texturing mode set to 'Without' or 'Splatmapping' "), GUILayout.MaxWidth(200));
                        Prefs.Prefs.terrainCount = EditorGUILayout.Vector2IntField("", Prefs.Prefs.terrainCount);
                    }
 

                    if (GUILayout.Button(new GUIContent(" Distributing Values ", m_resetPrefs, "Set All Splatmapping values to default and distributing slopes values "), new GUIStyle(EditorStyles.toolbarButton), GUILayout.ExpandWidth(true)))
                    {
                        Prefs.Prefs.Slope = 0.1f;
                        Prefs.Prefs.MergeRadius = 1;
                        Prefs.Prefs.MergingFactor = 1;

                        float step = 1f / Prefs.Prefs.TerrainLayers.Count;

                        for (int i = 0; i < Prefs.Prefs.TerrainLayers.Count; i++)
                        {
                            Prefs.Prefs.TerrainLayers[i].X_Height = i * step;
                            Prefs.Prefs.TerrainLayers[i].Y_Height = (i + 1) * step;
                        }

                    }
                    using (new VerticalBlock())
                    {
                        using (new HorizontalBlock())
                        {
                            GUILayout.Label(new GUIContent("  Slope ", " Used to normalized the slope in Y dir, The default value = 0"), GUILayout.MaxWidth(200));
                            Prefs.Prefs.Slope = EditorGUILayout.Slider(Prefs.Prefs.Slope, 0.0f, 1, GUILayout.MaxWidth(200), GUILayout.ExpandWidth(true));
                        }
                    }

                    using (new HorizontalBlock())
                    {
                        GUILayout.Label(new GUIContent("  Merging Radius ", " Used to precise the radius of merging between layers, 0 value means that no merging operation will apply  "), GUILayout.MaxWidth(200));
                        Prefs.Prefs.MergeRadius = EditorGUILayout.IntSlider(Prefs.Prefs.MergeRadius, 0, 500, GUILayout.MaxWidth(200), GUILayout.ExpandWidth(true));
                    }
                    using (new HorizontalBlock())
                    {
                        GUILayout.Label(new GUIContent("  Merging Factor ", " Used to precise how many times the merging will applyed on the terrain, the default is 1 "), GUILayout.MaxWidth(200));
                        Prefs.Prefs.MergingFactor = EditorGUILayout.IntSlider(Prefs.Prefs.MergingFactor, 1, 5, GUILayout.MaxWidth(200), GUILayout.ExpandWidth(true));
                    }
                    using (new VerticalBlock())
                    {
                        GUILayout.Label(" ");
                        GUILayout.Label(" ");
                        GUILayout.Label(new GUIContent("  Base Terrain Map ", " this will be the first splatmap for slope = 0"), GUILayout.MaxWidth(200));
                        Prefs.Prefs.BaseTerrainLayers.ShowHeight = false;

                        using (new HorizontalBlock())
                        {
                            SerializedObject BaseLayerso = new SerializedObject(Prefs);
                            SerializedProperty BaseLayerProperty = BaseLayerso.FindProperty("BaseTerrainLayers");
                            EditorGUILayout.PropertyField(BaseLayerProperty, true, GUILayout.MinWidth(0), GUILayout.MaxWidth(50), GUILayout.ExpandWidth(true));
                            BaseLayerso.ApplyModifiedProperties();
                        }

                        GUILayout.Label(" ");

                        using (new VerticalBlock())
                        {
                            SerializedObject LayersSO = new SerializedObject(Prefs);
                            SerializedProperty LayersProperty = LayersSO.FindProperty("TerrainLayers");
                            EditorGUILayout.PropertyField(LayersProperty, true);
                            LayersSO.ApplyModifiedProperties();

                            foreach (var layer in Prefs.Prefs.TerrainLayers)
                                layer.ShowHeight = true;
                        }
                    }

                    break;


            }
 
  
        }
        private void SmoothingTab()
        {


            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" Terrain Height Smoother ", "Used to softens the landscape and reduces the appearance of abrupt changes"), GUILayout.MaxWidth(200));
                Prefs.Prefs.UseTerrainHeightSmoother = (OptionEnabDisab)EditorGUILayout.EnumPopup("", Prefs.Prefs.UseTerrainHeightSmoother, GUILayout.ExpandWidth(true));
            }

            if (Prefs.Prefs.UseTerrainHeightSmoother == OptionEnabDisab.Enable)
            {
                using (new HorizontalBlock())
                {
                    GUILayout.Label("  Terrain Height Smooth Factor ", GUILayout.MaxWidth(200));
                    Prefs.Prefs.TerrainHeightSmoothFactor = EditorGUILayout.Slider(Prefs.Prefs.TerrainHeightSmoothFactor, 0.0f, 0.3f, GUILayout.ExpandWidth(true));
                }

            }

            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" Terrain Surface Smoother ", " this operation is useful when for terrains with unwanted jaggies, terraces,banding and non-smoothed terrain heights. Changing the surface smoother value to higher means more smoothing on surface while 1 value means minimum smoothing"), GUILayout.MaxWidth(200));
                Prefs.Prefs.UseTerrainSurfaceSmoother = (OptionEnabDisab)EditorGUILayout.EnumPopup("", Prefs.Prefs.UseTerrainSurfaceSmoother, GUILayout.ExpandWidth(true));
            }
 


            if (Prefs.Prefs.UseTerrainSurfaceSmoother == OptionEnabDisab.Enable)
            {
                using (new HorizontalBlock())
                {
                    GUILayout.Label(new GUIContent("  Terrain Surface Smooth Factor ", ""), GUILayout.MaxWidth(200));
                    Prefs.Prefs.TerrainSurfaceSmoothFactor = EditorGUILayout.IntSlider(Prefs.Prefs.TerrainSurfaceSmoothFactor, 1, 15, GUILayout.ExpandWidth(true));
                }

            }
        }
        private void VectorDataTab()
        {
            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" Vector Type ", "Select your vector type (Data must added to VectorData folder)"), GUILayout.MaxWidth(200));
                Prefs.Prefs.vectorType = (VectorType)EditorGUILayout.EnumPopup("", Prefs.Prefs.vectorType, GUILayout.ExpandWidth(true));
            }

           


                using (new HorizontalBlock())
                {
                    GUILayout.Label(new GUIContent(" Generate Trees ", "Enable/Disable Loading and Generating Trees from Vector Files "), GUILayout.MaxWidth(200));
                    Prefs.Prefs.EnableTreeGeneration = (OptionEnabDisab)EditorGUILayout.EnumPopup("", Prefs.Prefs.EnableTreeGeneration, GUILayout.ExpandWidth(true));
                }

                if (Prefs.Prefs.EnableTreeGeneration == OptionEnabDisab.Enable)
                {

                using (new HorizontalBlock())
                {
                    GUILayout.Label(new GUIContent("  Tree Distribution  ", " Distribute Trees in Random or Regular way "), GUILayout.MaxWidth(200));
                    Prefs.Prefs.TreeDistribution = (PointDistribution)EditorGUILayout.EnumPopup("", Prefs.Prefs.TreeDistribution, GUILayout.ExpandWidth(true));
                }

                //Grass Scale Factor
                using (new HorizontalBlock())
                {
                    GUILayout.Label("  Tree Scale Factor ", GUILayout.MaxWidth(200));
                    Prefs.Prefs.TreeScaleFactor = EditorGUILayout.Slider(Prefs.Prefs.TreeScaleFactor, 1f, 10f, GUILayout.ExpandWidth(true));
                }

      
                //Tree Distance
                using (new HorizontalBlock())
                    {
                        GUILayout.Label("  Tree Distance ", GUILayout.MaxWidth(200));
                        Prefs.Prefs.TreeDistance = EditorGUILayout.Slider(Prefs.Prefs.TreeDistance, 1, 5000, GUILayout.ExpandWidth(true));

                    }

                    //Tree BillBoard Start Distance
                    using (new HorizontalBlock())
                    {
                        GUILayout.Label("  Tree BillBoard Start Distance ", GUILayout.MaxWidth(200));
                        Prefs.Prefs.BillBoardStartDistance = EditorGUILayout.Slider(Prefs.Prefs.BillBoardStartDistance, 1, 2000, GUILayout.ExpandWidth(true));

                    }
                    //Tree Prefabs List
                    using (new HorizontalBlock())
                    {
                        GUILayout.Label("  Trees ", GUILayout.MaxWidth(200));
                        SerializedObject so = new SerializedObject(Prefs);
                        SerializedProperty stringsProperty = so.FindProperty("TreePrefabs");
                        EditorGUILayout.PropertyField(stringsProperty, true);
                        so.ApplyModifiedProperties();
                    }

                    using (new HorizontalBlock())
                    {
                        GUILayout.Label(new GUIContent("                ", " "), GUILayout.MaxWidth(200));

                        if (GUILayout.Button(new GUIContent(" Load All ", "Click To Load all tree prefabs Located in 'GIS Tech/GIS Terrain Loader/Resources/Prefabs/Environment/Trees'"), GUILayout.ExpandWidth(true)))
                        {
                            Prefs.Prefs.LoadAllTreePrefabs();
                        }
                    }

                }


                using (new HorizontalBlock())
                {
                    GUILayout.Label(new GUIContent(" Generate Grass ", "Enable/Disable Loading and Generating Grass from OSM File "), GUILayout.MaxWidth(200));
                    Prefs.Prefs.EnableGrassGeneration = (OptionEnabDisab)EditorGUILayout.EnumPopup("", Prefs.Prefs.EnableGrassGeneration, GUILayout.ExpandWidth(true));
                }

                if (Prefs.Prefs.EnableGrassGeneration == OptionEnabDisab.Enable)
                {

                using (new HorizontalBlock())
                {
                    GUILayout.Label(new GUIContent("  Grass Distribution  ", " Distribute Grass in Random or Regular way "), GUILayout.MaxWidth(200));
                    Prefs.Prefs.GrassDistribution = (PointDistribution)EditorGUILayout.EnumPopup("", Prefs.Prefs.GrassDistribution, GUILayout.ExpandWidth(true));
                }
                //Grass Scale Factor
                using (new HorizontalBlock())
                    {
                        GUILayout.Label("  Grass Scale Factor ", GUILayout.MaxWidth(200));
                        Prefs.Prefs.GrassScaleFactor = EditorGUILayout.Slider(Prefs.Prefs.GrassScaleFactor, 0.1f, 100, GUILayout.ExpandWidth(true));
                    }


                    //Detail Distance
                    using (new HorizontalBlock())
                    {
                        GUILayout.Label("  Detail Distance ", GUILayout.MaxWidth(200));
                        Prefs.Prefs.DetailDistance = EditorGUILayout.Slider(Prefs.Prefs.DetailDistance, 10f, 400, GUILayout.ExpandWidth(true));

                    }


                    //Tree Prefabs List
                    using (new HorizontalBlock())
                    {
                        GUILayout.Label("  Grass ", GUILayout.MaxWidth(200));

                        SerializedObject so = new SerializedObject(Prefs);
                        SerializedProperty stringsProperty = so.FindProperty("GrassPrefabs");
 
                    if (stringsProperty != null)
                    {
                        EditorGUILayout.PropertyField(stringsProperty, true);
                        so.ApplyModifiedProperties();
                    }

                }

                }

                using (new HorizontalBlock())
                {
                    GUILayout.Label(new GUIContent(" Generate GeoPoints ", " Enable this option to generate gamebjects according to geo-points coordinates found in the vector file"), GUILayout.MaxWidth(200));
                    Prefs.Prefs.EnableGeoPointGeneration = (OptionEnabDisab)EditorGUILayout.EnumPopup("", Prefs.Prefs.EnableGeoPointGeneration, GUILayout.ExpandWidth(true));

                }
                using (new HorizontalBlock())
                {
                    GUILayout.Label(new GUIContent(" Generate Roads ", "Enable/Disable Loading and Generating Roads from OSM File "), GUILayout.MaxWidth(200));
                    Prefs.Prefs.EnableRoadGeneration = (OptionEnabDisab)EditorGUILayout.EnumPopup("", Prefs.Prefs.EnableRoadGeneration, GUILayout.ExpandWidth(true));
                }

                if (Prefs.Prefs.EnableRoadGeneration == OptionEnabDisab.Enable)
                {
                    using (new HorizontalBlock())
                    {
                        GUILayout.Label(new GUIContent("  Road Generator Type ", "Select whiche type of road will be used (Note that EasyRoad3D must be existing in the project "), GUILayout.MaxWidth(200));
                        Prefs.Prefs.RoadGenerator = (RoadGeneratorType)EditorGUILayout.EnumPopup("", Prefs.Prefs.RoadGenerator, GUILayout.ExpandWidth(true));

                    }
                    if (Prefs.Prefs.RoadGenerator == RoadGeneratorType.EasyRoad3D)
                    {
                        using (new HorizontalBlock())
                        {
                            GUILayout.Label(new GUIContent("   Raise Offset ", " The Distanc above the terrain, (offset to rise up/down the road "), GUILayout.MaxWidth(200));
                            Prefs.Prefs.RoadRaiseOffset = EditorGUILayout.Slider(Prefs.Prefs.RoadRaiseOffset, 0.1f, 2f, GUILayout.ExpandWidth(true));
                        }

                        using (new HorizontalBlock())
                        {
                            GUILayout.Label(new GUIContent("   Build Terrains ", "Enable/Disable  Update the terrains OnFinish according to the road network shape"), GUILayout.MaxWidth(200));
                            Prefs.Prefs.BuildTerrains = (OptionEnabDisab)EditorGUILayout.EnumPopup("", Prefs.Prefs.BuildTerrains);
                        }


                    }

                    using (new HorizontalBlock())
                    {
                        GUILayout.Label(new GUIContent("  Roads Lable ", "Add Roads name  "), GUILayout.MaxWidth(200));
                        Prefs.Prefs.EnableRoadName = EditorGUILayout.Toggle("", Prefs.Prefs.EnableRoadName, GUILayout.ExpandWidth(true));

                    }


                }
                using (new HorizontalBlock())
                {
                    GUILayout.Label(new GUIContent(" Generate Buildings ", "Enable/Disable Loading and Generating buildings from Vector File "), GUILayout.MaxWidth(200));
                    Prefs.Prefs.EnableBuildingGeneration = (OptionEnabDisab)EditorGUILayout.EnumPopup("", Prefs.Prefs.EnableBuildingGeneration, GUILayout.ExpandWidth(true));
                }
                using (new HorizontalBlock())
                {
                    if (Prefs.Prefs.EnableBuildingGeneration == OptionEnabDisab.Enable)
                    {
                        GUILayout.Label(new GUIContent("   Generate Building Base ", "Enable this option to generate a base to each building "), GUILayout.MaxWidth(200));
                        Prefs.Prefs.GenerateBuildingBase = (OptionEnabDisab)EditorGUILayout.EnumPopup("", Prefs.Prefs.GenerateBuildingBase);
                    }
                }

            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" Generate Water ", "Enable/Disable this option to generate water areas from vector "), GUILayout.MaxWidth(200));
                Prefs.Prefs.EnableWaterGeneration = (OptionEnabDisab)EditorGUILayout.EnumPopup("", Prefs.Prefs.EnableWaterGeneration, GUILayout.ExpandWidth(true));


            }

            if (Prefs.Prefs.EnableWaterGeneration == OptionEnabDisab.Enable)
            {
                using (new HorizontalBlock())
                {
                    GUILayout.Label(new GUIContent("   Water Data Source ", "Plane To Generate a simple plane for the entier terrain, VectorSource generate complexed geometry water data from vectordata  "), GUILayout.MaxWidth(200));
                    Prefs.Prefs.WaterDataSource = (WaterSource)EditorGUILayout.EnumPopup("", Prefs.Prefs.WaterDataSource);
                }

                using (new HorizontalBlock())
                {
                    GUILayout.Label(new GUIContent("   Water Offset Y ", " Rise up water from the terrain by y offset "), GUILayout.MaxWidth(200));
                    Prefs.Prefs.WaterOffsetY = EditorGUILayout.Slider(Prefs.Prefs.WaterOffsetY, 0.1f, 5f, GUILayout.ExpandWidth(true));
                }

            }

 

            using (new HorizontalBlock())
            {
                GUILayout.Label(new GUIContent(" Generate LandParcel ", "Enable/Disable to generate some 2D plans on terrain from Vector File "), GUILayout.MaxWidth(200));
                Prefs.Prefs.EnableLandParcelGeneration = (OptionEnabDisab)EditorGUILayout.EnumPopup("", Prefs.Prefs.EnableLandParcelGeneration, GUILayout.ExpandWidth(true));


            }

            if (Prefs.Prefs.EnableLandParcelGeneration == OptionEnabDisab.Enable)
            {
                using (new HorizontalBlock())
                {
                    GUILayout.Label(new GUIContent("   Y Offset ", " Rise up LandParcel from the terrain by y offset, Y Unite adapted to terrain container "), GUILayout.MaxWidth(200));
                    Prefs.Prefs.LandParcelOffsetY = EditorGUILayout.Slider(Prefs.Prefs.LandParcelOffsetY, 0.1f, 50f, GUILayout.ExpandWidth(true));
                }


                using (new HorizontalBlock())
                {
                    GUILayout.Label(new GUIContent("   LandParcel Elevation Source ", "Default2D: only bounds vector points will be adapted to the terrain elevation , AdaptedToTerrainElevation: Adapte the entire polygon to the terrain elevation by creating more complexed geometry plan"), GUILayout.MaxWidth(200));
                    Prefs.Prefs.LandParcelElevationMode = (VectorElevationMode)EditorGUILayout.EnumPopup("", Prefs.Prefs.LandParcelElevationMode);
                }

                if (Prefs.Prefs.LandParcelElevationMode == VectorElevationMode.AdaptedToTerrainElevation)
                {
                    using (new HorizontalBlock())
                    {
                        GUILayout.Label(new GUIContent("   Polygon Count ", " Add more subdivisions inside of Polygon to be more adapted to the terrain curves (becareful, the more Polygon Count the higher calculation number) "), GUILayout.MaxWidth(200));
                        Prefs.Prefs.LandParcelPolygonCount = EditorGUILayout.IntSlider(Prefs.Prefs.LandParcelPolygonCount, 1, 5, GUILayout.ExpandWidth(true));
                    }
                }
            }
        }
        private void OptionsTab()
        {

            GUIStyle buttonStyle = new GUIStyle(EditorStyles.toolbarButton);

            using (new HorizontalBlock())
            {
                if (GUILayout.Button(new GUIContent(" Reset", m_resetPrefs, " Reset all Prefs to default "), buttonStyle, GUILayout.ExpandWidth(false)))
                {
                    Prefs.Prefs.ResetPrefs();
                }
            }

        }
        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            Undo.RecordObject(Prefs, "GTL_Runtime");
            tabs.Draw();
            if (GUI.changed && Prefs.Prefs!=null)
                Prefs.Prefs.lastTab = tabs.curMethodIndex;
            EditorUtility.SetDirty(Prefs);
        }
        private Texture2D LoadTexture(string m_iconeName)
        {
            var tex = new Texture2D(35, 35);

            string[] guids = AssetDatabase.FindAssets(m_iconeName + " t:texture");
            if (guids != null && guids.Length > 0)
            {
                string iconPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                tex = (Texture2D)AssetDatabase.LoadAssetAtPath(iconPath, typeof(Texture2D));
            }

            return tex;
        }
        private void CoordinatesBarGUI()
        {
            using (new VerticalBlock())
            {
                using (new VerticalBlock())
                {
                    Prefs.Prefs.ShowCoordinates = EditorGUILayout.Foldout(Prefs.Prefs.ShowCoordinates, "Sub Region Coordinates");
                }

                if (Prefs.Prefs.ShowCoordinates)
                {
                    EditorGUILayout.HelpBox(" Set Sub Region Heightmap coordinates ", MessageType.Info);

                    GUILayout.Label("Upper-Left : ", GUILayout.ExpandWidth(false));

                    using (new HorizontalBlock())
                    {
                        GUILayout.Label("Latitude : ", GUILayout.MaxWidth(100), GUILayout.ExpandWidth(false));
                        GUI.SetNextControlName("UpperLeftCoordianteLat");
                        Prefs.Prefs.SubRegionUpperLeftCoordiante.y = EditorGUILayout.DoubleField(Prefs.Prefs.SubRegionUpperLeftCoordiante.y, GUILayout.ExpandWidth(true));

                        GUILayout.Label("Longitude : ", GUILayout.MaxWidth(100), GUILayout.ExpandWidth(false));
                        GUI.SetNextControlName("UpperLeftCoordianteLon");
                        Prefs.Prefs.SubRegionUpperLeftCoordiante.x = EditorGUILayout.DoubleField(Prefs.Prefs.SubRegionUpperLeftCoordiante.x, GUILayout.ExpandWidth(true));

                    }
                    GUILayout.Label("Down-Right : ", GUILayout.ExpandWidth(false));

                    using (new HorizontalBlock())
                    {
                        GUILayout.Label("Latitude : ", GUILayout.MaxWidth(100), GUILayout.ExpandWidth(false));
                        GUI.SetNextControlName("DownRightCoordianteLat");
                        Prefs.Prefs.SubRegionDownRightCoordiante.y = EditorGUILayout.DoubleField(Prefs.Prefs.SubRegionDownRightCoordiante.y, GUILayout.ExpandWidth(true));

                        GUILayout.Label("Longitude : ", GUILayout.MaxWidth(100), GUILayout.ExpandWidth(false));
                        GUI.SetNextControlName("DownRightCoordianteLon");
                        Prefs.Prefs.SubRegionDownRightCoordiante.x = EditorGUILayout.DoubleField(Prefs.Prefs.SubRegionDownRightCoordiante.x, GUILayout.ExpandWidth(true));
                    }

                    GUILayout.Label("", GUILayout.ExpandWidth(false));

                }

            }
 
        }
    }
#endif
}