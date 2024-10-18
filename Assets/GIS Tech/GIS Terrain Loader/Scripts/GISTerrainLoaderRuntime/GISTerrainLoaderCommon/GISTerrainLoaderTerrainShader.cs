/*     Unity GIS Tech 2020-2023      */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderTerrainShader
    {
        private static Texture2D Main_Gradien_Pos;
        private static Texture2D Negative_Gradien_Pos;
        private static Texture2D Color_Gradien;

        private static Vector2Int TextureSize;
        private static float Terrain_Size_Y;

        private static float[,] data;
        private static Texture2D ShadedTexture;
        private static bool ClearLayers;
        private static GISTerrainLoaderPrefs Prefs;
 
        public static async Task GenerateShadedTextureEditor(GISTerrainLoaderPrefs m_Prefs, GISTerrainTile item)
        {
            Prefs = m_Prefs;
            TextureSize = new Vector2Int(Prefs.heightmapResolution - 1, Prefs.heightmapResolution - 1);
            bool color = true;
            bool invers = false;

            if (Prefs.TerrainShaderType == ShaderType.Slop || Prefs.TerrainShaderType == ShaderType.ElevationGrayScale || Prefs.TerrainShaderType == ShaderType.ElevationInversGrayScale)
                color = false;
            if (Prefs.TerrainShaderType == ShaderType.SlopInvers || Prefs.TerrainShaderType == ShaderType.ElevationInversGrayScale)
            {
                color = false; invers = true;
            }
                

             GenerateBaseShaders(color, invers);

            data = item.terrainData.GetHeights(0, 0, TextureSize.x, TextureSize.x);

            if (Prefs.UnderWater == OptionEnabDisab.Enable)
            {
                if (item.container.data.MinMaxElevation.y < 0)
                    Terrain_Size_Y = item.container.ContainerSize.y * -1;

            }
            else
                Terrain_Size_Y = item.container.ContainerSize.y;

            ShadedTexture = new Texture2D(TextureSize.x, TextureSize.y, TextureFormat.RGB24, false);

            switch (Prefs.TerrainShaderType)
            {
                case ShaderType.ColorRamp:
                    GenerateElevationShader(item, Prefs.UnderWater);
                    break;
                case ShaderType.ElevationGrayScale:
                    GenerateElevationShader(item, Prefs.UnderWater);
                    break;
                case ShaderType.ElevationInversGrayScale:
                    GenerateElevationShader(item, Prefs.UnderWater);
                    break;
                case ShaderType.Slop:
                    GenerateSlopShader(item);
                    break;
                case ShaderType.SlopInvers:
                    GenerateSlopShader(item);
                    break;
                case ShaderType.NormalMap:
                    GenerateNormalMapShader(item);
                    break;

            }
 
            if (Prefs.SaveShaderTextures == OptionEnabDisab.Enable)
                await SaveShadersAsTexturesAsync(item, Prefs.TerrainFilePath);
 
            item.TextureState = TextureState.Loaded;
        }
        public static void GenerateShadedTextureRuntime(GISTerrainLoaderPrefs Prefs, GISTerrainTile item, bool m_ClearLayers = false)
        {
            TextureSize = new Vector2Int(Prefs.heightmapResolution - 1, Prefs.heightmapResolution - 1);
           
            ClearLayers = m_ClearLayers;
           
            bool color = true;
            bool invers = false;

            if (Prefs.TerrainShaderType == ShaderType.Slop || Prefs.TerrainShaderType == ShaderType.ElevationGrayScale || Prefs.TerrainShaderType == ShaderType.ElevationInversGrayScale)
                color = false;
           
            if (Prefs.TerrainShaderType == ShaderType.SlopInvers || Prefs.TerrainShaderType == ShaderType.ElevationInversGrayScale)
            {
                color = false; invers = true;
            }

            GenerateBaseShaders(color, invers);


            data = item.terrainData.GetHeights(0, 0, TextureSize.x, TextureSize.x);

            if (Prefs.UnderWaterShader == OptionEnabDisab.Enable)
            {
                if (item.container.data.MinMaxElevation.y < 0)
                    Terrain_Size_Y = item.container.ContainerSize.y * -1;

            }
            else
                Terrain_Size_Y = item.container.ContainerSize.y;

            ShadedTexture = new Texture2D(TextureSize.x, TextureSize.y, TextureFormat.RGB24, false);

            switch (Prefs.TerrainShaderType)
            {
                case ShaderType.ColorRamp:
                    GenerateElevationShader(item, Prefs.UnderWaterShader);
                    break;
                case ShaderType.ElevationGrayScale:
                    GenerateElevationShader(item, Prefs.UnderWaterShader);
                    break;
                case ShaderType.ElevationInversGrayScale:
                    GenerateElevationShader(item, Prefs.UnderWaterShader);
                    break;
                case ShaderType.Slop:
                    GenerateSlopShader(item);
                    break;
                case ShaderType.SlopInvers:
                    GenerateSlopShader(item);
                    break;
                case ShaderType.NormalMap:
                    GenerateNormalMapShader(item);
                    break;
                   
            }

            ShadedTexture.Apply();

            if (Prefs.SaveShaderTextures == OptionEnabDisab.Enable)
                SaveShadersAsTexturesRuntime(item, Prefs.TerrainFilePath);

            AddTextureToTerrainRuntime(item, ShadedTexture);

            item.TextureState = TextureState.Loaded;
        }
        private static void GenerateBaseShaders(bool SetColor = true, bool invers = true)
        {
            if(SetColor)
            {
                Color_Gradien = GetGradientColor(ShaderColor.GradientColor);
                Main_Gradien_Pos = GetGradientColor(ShaderColor.MainGradient);
                Negative_Gradien_Pos = GetGradientColor(ShaderColor.NegativeGradient);
            }
            else
            {
                if (invers)
                {
                    Color_Gradien = GetGradientColor(ShaderColor.GreyToBlack);
                    Main_Gradien_Pos = GetGradientColor(ShaderColor.GreyToWhite);
                    Negative_Gradien_Pos = GetGradientColor(ShaderColor.BlackToWhite);
                }
                else
                {
                    Color_Gradien = GetGradientColor(ShaderColor.BlackToWhite);
                    Main_Gradien_Pos = GetGradientColor(ShaderColor.GreyToWhite);
                    Negative_Gradien_Pos = GetGradientColor(ShaderColor.GreyToBlack);
                }
            }
 
            Color_Gradien.Apply();
            Main_Gradien_Pos.Apply();
            Negative_Gradien_Pos.Apply();
        }
        private static Texture2D GetGradientColor(ShaderColor shadercolor)
        {
            var tex = new Texture2D(0, 0);

            switch (shadercolor)
            {
                case ShaderColor.GradientColor:

                    tex = new Texture2D(9, 1, TextureFormat.ARGB32, false, true);
                    tex.SetPixel(0, 0, new Color32(80, 80, 230, 255));
                    tex.SetPixel(1, 0, new Color32(80, 180, 230, 255));
                    tex.SetPixel(2, 0, new Color32(80, 230, 230, 255));
                    tex.SetPixel(3, 0, new Color32(80, 230, 180, 255));
                    tex.SetPixel(4, 0, new Color32(80, 230, 80, 255));
                    tex.SetPixel(5, 0, new Color32(180, 230, 80, 255));
                    tex.SetPixel(6, 0, new Color32(230, 230, 80, 255));
                    tex.SetPixel(7, 0, new Color32(230, 180, 80, 255));
                    tex.SetPixel(8, 0, new Color32(230, 80, 80, 255));
                    tex.wrapMode = TextureWrapMode.Clamp;

                    break;
                case ShaderColor.MainGradient:

                    tex = new Texture2D(5, 1, TextureFormat.ARGB32, false, true);
                    tex.SetPixel(0, 0, new Color32(80, 230, 80, 255));
                    tex.SetPixel(1, 0, new Color32(180, 230, 80, 255));
                    tex.SetPixel(2, 0, new Color32(230, 230, 80, 255));
                    tex.SetPixel(3, 0, new Color32(230, 180, 80, 255));
                    tex.SetPixel(4, 0, new Color32(230, 80, 80, 255));
                    tex.wrapMode = TextureWrapMode.Clamp;

                    break;
                case ShaderColor.NegativeGradient:

                    tex = new Texture2D(5, 1, TextureFormat.ARGB32, false, true);
                    tex.SetPixel(0, 0, new Color32(80, 230, 80, 255));
                    tex.SetPixel(1, 0, new Color32(80, 230, 180, 255));
                    tex.SetPixel(2, 0, new Color32(80, 230, 230, 255));
                    tex.SetPixel(3, 0, new Color32(80, 180, 230, 255));
                    tex.SetPixel(4, 0, new Color32(80, 80, 230, 255));
                    tex.wrapMode = TextureWrapMode.Clamp;

                    break;
                case ShaderColor.BlackToWhite:

                    tex = new Texture2D(5, 1, TextureFormat.ARGB32, false, true);
                    tex.SetPixel(0, 0, new Color32(0, 0, 0, 255));
                    tex.SetPixel(1, 0, new Color32(64, 64, 64, 255));
                    tex.SetPixel(2, 0, new Color32(128, 128, 128, 255));
                    tex.SetPixel(3, 0, new Color32(192, 192, 192, 255));
                    tex.SetPixel(4, 0, new Color32(255, 255, 255, 255));
                    tex.wrapMode = TextureWrapMode.Clamp;

                    break;
                case ShaderColor.GreyToWhite:

                    tex = new Texture2D(3, 1, TextureFormat.ARGB32, false, true);
                    tex.SetPixel(0, 0, new Color32(128, 128, 128, 255));
                    tex.SetPixel(1, 0, new Color32(192, 192, 192, 255));
                    tex.SetPixel(2, 0, new Color32(255, 255, 255, 255));
                    tex.wrapMode = TextureWrapMode.Clamp;

                    break;
                case ShaderColor.GreyToBlack:

                    tex = new Texture2D(3, 1, TextureFormat.ARGB32, false, true);
                    tex.SetPixel(0, 0, new Color32(128, 128, 128, 255));
                    tex.SetPixel(1, 0, new Color32(64, 64, 64, 255));
                    tex.SetPixel(2, 0, new Color32(0, 0, 0, 255));
                    tex.wrapMode = TextureWrapMode.Clamp;

                    break;

            }
            return tex;
        }
        private static void GenerateElevationShader(GISTerrainTile item, OptionEnabDisab UnderWater)
        {
            for (int m_y = 0; m_y < TextureSize.y; m_y++)
            {
                for (int m_x = 0; m_x < TextureSize.x; m_x++)
                {
                    float el = GetNormalizedHeight(m_y, m_x);

                    if (UnderWater == OptionEnabDisab.Enable)
                    {
                        if (item.container.data.MinMaxElevation.y < 0)
                            el += item.container.data.MinMaxElevation.x;
                    }

                    Color color = GetColor(el, 0, true);
                    ShadedTexture.SetPixel(m_x, m_y, color);

                }
            }
        }
        private static void GenerateSlopShader(GISTerrainTile item)
        {
            for (int m_y = 0; m_y < TextureSize.y; m_y++)
            {
                for (int m_x = 0; m_x < TextureSize.x; m_x++)
                {
                    Vector2 d1 = DerivativeCal(m_y, m_x);

                    float slope = GISTerrainLoaderMath.SlopeCal(d1.x, d1.y);

                    Color color = GetColor(slope, 0.5f, true);

                    ShadedTexture.SetPixel(m_x, m_y, color);

                }
            }
        }
        private static void GenerateNormalMapShader(GISTerrainTile item)
        {
            for (int m_y = 0; m_y < TextureSize.y; m_y++)
            {
                for (int m_x = 0; m_x < TextureSize.x; m_x++)
                {
                    Vector2 Der = DerivativeCal(m_y, m_x);
 
                    var Normal = new Vector3(Der.x * 0.7f + 0.7f, -Der.y * 0.7f + 0.7f, 1.2f);

                    Normal.Normalize();

                    ShadedTexture.SetPixel(m_x, m_y, new Color(Normal.x, Normal.y, Normal.z, 1));
                }
            }
        }
 
        public static Texture2D GetShadedTexture(GISTerrainLoaderPrefs m_Prefs, GISTerrainContainer item,float[,] data, ShadedTextureType shadedTextureType)
        {
            var TextureSize = new Vector2Int(m_Prefs.heightmapResolution* item.TerrainCount.x - 1, m_Prefs.heightmapResolution * item.TerrainCount.y - 1);
            
            var ShadedTexture = new Texture2D(TextureSize.x, TextureSize.y, TextureFormat.RGB24, false);
            
            bool color = true;
            
            bool invers = false;

            if (m_Prefs.TerrainShaderType == ShaderType.Slop || m_Prefs.TerrainShaderType == ShaderType.ElevationGrayScale || m_Prefs.TerrainShaderType == ShaderType.ElevationInversGrayScale)
                color = false;
            if (m_Prefs.TerrainShaderType == ShaderType.SlopInvers || m_Prefs.TerrainShaderType == ShaderType.ElevationInversGrayScale)
            {
                color = false; invers = true;
            }
 
            GenerateBaseShaders(color, invers);
 
            if (m_Prefs.UnderWater == OptionEnabDisab.Enable)
            {
                if (item.data.MinMaxElevation.y < 0)
                    Terrain_Size_Y = item.ContainerSize.y * -1;

            }
            else
                Terrain_Size_Y = item.ContainerSize.y;



            switch (shadedTextureType)
            {
                case ShadedTextureType.Foam:
                    ShadedTexture = GetFoamTexture(TextureSize, data); 
                    break;
                case ShadedTextureType.Water:
                    ShadedTexture = GetWaterTexture(TextureSize, data);
                    break;
            }

            if (!Application.isPlaying)
                ShadedTexture= SaveShadedTexture(m_Prefs.TerrainFilePath, ShadedTexture, shadedTextureType);

            return ShadedTexture;
        }
        public static Texture2D GetFoamTexture(Vector2Int TextureSize, float[,] datax)
        {
 
            Texture2D  ShadedTexture = new Texture2D(TextureSize.x, TextureSize.y, TextureFormat.RGB24, false);

            for (int m_y = 0; m_y < TextureSize.y; m_y++)
            {
                for (int m_x = 0; m_x < TextureSize.x; m_x++)
                {
                    float el = GetNormalizedHeight(m_y, m_x, datax, TextureSize);

                    //Color color = Color.Lerp(Color.white, Color.black, el);
                    Color color = Color.black;
                    if (el == 0.0f)
                        color = Color.white;
                    ShadedTexture.SetPixel(m_x, m_y, color); 

                }
            }
            ShadedTexture.Apply();
            return ShadedTexture;
        }
        public static Texture2D GetWaterTexture(Vector2Int TextureSize, float[,] datax)
        {

            Texture2D ShadedTexture = new Texture2D(TextureSize.x, TextureSize.y, TextureFormat.RGB24, false);

            for (int m_y = 0; m_y < TextureSize.y; m_y++)
            {
                for (int m_x = 0; m_x < TextureSize.x; m_x++)
                {
                    float el = GetNormalizedHeight(m_y, m_x, datax, TextureSize);

                    var blue = new Color32(6, 26, 97, 255);
                    var Clearblue = new Color32(75, 192, 255, 255);
                    //Color color = Color.Lerp(blue, Clearblue, el);
                    Color color = blue;
                    if (el > 0 && el < 0.05)
                    {
                        color = Color.Lerp(blue, Clearblue, el);
                        //color = Clearblue;
                    }
                       
                    ShadedTexture.SetPixel(m_x, m_y, color);

                }
            }
            ShadedTexture.Apply();
            return ShadedTexture;
        }


        private static async Task SaveShadersAsTexturesAsync(GISTerrainTile item, string terrainPath)
        {

#if UNITY_EDITOR
                if (!Application.isPlaying)
            {
                var folderPath = Path.GetDirectoryName(terrainPath);
                var TerrainFilename = Path.GetFileNameWithoutExtension(terrainPath);
                var ShaderTexturesFolder = Path.Combine(folderPath, TerrainFilename + "_ShaderTextures");

                if (!Directory.Exists(ShaderTexturesFolder))
                    Directory.CreateDirectory(ShaderTexturesFolder);

                DirectoryInfo di = new DirectoryInfo(terrainPath);

                var ResourceShaderPath = Path.GetFileNameWithoutExtension(terrainPath) + "_ShaderTextures";

                for (int i = 0; i <= 5; i++)
                {
                    di = di.Parent;
                    ResourceShaderPath = di.Name + "/" + ResourceShaderPath;

                    if (di.Name == "GIS Terrains") break;

                    if (i == 5)
                    {
                        Debug.LogError("Texture folder not found! : Please put your terrain in GIS Terrain Loader/Recources/GIS Terrains/");

                        return;

                    }

                }

                var TexturePath = ShaderTexturesFolder + "/" + "Tile__" + (item.Number.x).ToString() + "__" + item.Number.y + ".jpg";
                await WriteShaderAsync(ShadedTexture, TexturePath);
 
                AssetDatabase.Refresh();

                var ResourceTexturePath = ResourceShaderPath + "/" + "Tile__" + (item.Number.x).ToString() + "__" + item.Number.y;
                AddTextureToTerrainEditor(item, ResourceTexturePath);
            }
            else
            {
                if (!string.IsNullOrEmpty(terrainPath))
                {
                    var folderPath = Path.GetDirectoryName(terrainPath);
                    var TerrainFilename = Path.GetFileNameWithoutExtension(terrainPath);
                    var ShaderFolder = Path.Combine(folderPath, TerrainFilename + "_ShaderTextures");

                    if (!Directory.Exists(ShaderFolder))
                        Directory.CreateDirectory(ShaderFolder);

                    var TexturePath = ShaderFolder + "/" + "Tile__" + (item.Number.x).ToString() + "__" + item.Number.y + ".jpg";
                    await WriteShaderAsync(ShadedTexture, TexturePath);

                }
            }

#else
                if (!string.IsNullOrEmpty(terrainPath))
                {
                    var folderPath = Path.GetDirectoryName(terrainPath);
                    var TerrainFilename = Path.GetFileNameWithoutExtension(terrainPath);
                    var ShaderFolder = Path.Combine(folderPath, TerrainFilename + "_ShaderTextures");

                    if (!Directory.Exists(ShaderFolder))
                        Directory.CreateDirectory(ShaderFolder);

                    var TexturePath = ShaderFolder + "/" + "Tile__" + (item.Number.x).ToString() + "__" + item.Number.y + ".jpg";
                    await WriteShaderAsync(ShadedTexture, TexturePath);

                }
#endif

        }
        private static void SaveShadersAsTexturesRuntime(GISTerrainTile item, string RuntimePath = "")
        {
            var TexturePath = ""; 

            if (!string.IsNullOrEmpty(RuntimePath))
            {
                var folderPath = Path.GetDirectoryName(RuntimePath);
                var TerrainFilename = Path.GetFileNameWithoutExtension(RuntimePath);
                var ShaderFolder = Path.Combine(folderPath, TerrainFilename + "_ShaderTextures");

                if (!Directory.Exists(ShaderFolder))
                    Directory.CreateDirectory(ShaderFolder);

                TexturePath = ShaderFolder + "/" + item.name + ".jpg";
                WriteShaderRuntime(ShadedTexture, TexturePath);
  
            }


        }
        private static void AddTextureToTerrainEditor(GISTerrainTile item, string texPath)
        {
 
#if UNITY_2018_1_OR_NEWER

                TerrainLayer NewterrainLayer = new TerrainLayer();

                string path = Path.Combine(item.container.TerrainFilePath, item.name + ".terrainlayer");

#if UNITY_EDITOR
                AssetDatabase.CreateAsset(NewterrainLayer, path);
#endif
                TerrainLayer[] ExistingTerrainLayers = item.terrainData.terrainLayers;
                List<TerrainLayer> NewLayers = new List<TerrainLayer>();
                foreach (var l in ExistingTerrainLayers)
                {
                    NewLayers.Add(l);
                }
#if UNITY_EDITOR
                NewterrainLayer.diffuseTexture = (Texture2D)Resources.Load(texPath); 
#endif

                NewterrainLayer.tileSize = new Vector2(item.size.x, item.size.z);
                NewterrainLayer.tileOffset = Vector2.zero;

                NewLayers.Add(NewterrainLayer);

                item.terrainData.terrainLayers = NewLayers.ToArray();

#else

            SplatPrototype sp = new SplatPrototype
            {
                texture = tex,
                tileSize = new Vector2(item.size.x, item.size.z),
                tileOffset = Vector2.zero
            };
            item.terrainData.splatPrototypes = new[] { sp };

#endif
 

        }
        private static void AddTextureToTerrainRuntime(GISTerrainTile item, Texture2D generatedText)
        {
     
#if UNITY_2018_1_OR_NEWER

            TerrainLayer NewterrainLayer = new TerrainLayer();

                TerrainLayer[] ExistingTerrainLayers = item.terrainData.terrainLayers;
                List<TerrainLayer> NewLayers = new List<TerrainLayer>();

            if (!ClearLayers)
            {
                Debug.Log(ExistingTerrainLayers.Length);
                foreach (var l in ExistingTerrainLayers)
                {
                    if (l != null)
                        NewLayers.Add(l);
                }
            }

            NewterrainLayer.diffuseTexture = generatedText;

            NewterrainLayer.tileSize = new Vector2(item.size.x, item.size.z);
            NewterrainLayer.tileOffset = Vector2.zero;

            NewLayers.Add(NewterrainLayer);

            item.terrainData.terrainLayers = NewLayers.ToArray();

#else

            SplatPrototype sp = new SplatPrototype
            {
                texture = tex,
                tileSize = new Vector2(item.size.x, item.size.z),
                tileOffset = Vector2.zero
            };
            item.terrainData.splatPrototypes = new[] { sp };

#endif

        }
        private static void ChangeDiffuseTexture(GISTerrainTile item, Texture2D generatedText)
        {
            if(item.terrainData.terrainLayers == null || item.terrainData.terrainLayers.Length==0)
            {
#if UNITY_2018_1_OR_NEWER

                TerrainLayer NewterrainLayer = new TerrainLayer();

                TerrainLayer[] ExistingTerrainLayers = item.terrainData.terrainLayers;
                List<TerrainLayer> NewLayers = new List<TerrainLayer>();
                foreach (var l in ExistingTerrainLayers)
                {
                    NewLayers.Add(l);
                }

                NewterrainLayer.diffuseTexture = generatedText;

                NewterrainLayer.tileSize = new Vector2(item.size.x, item.size.z);
                NewterrainLayer.tileOffset = Vector2.zero;

                NewLayers.Add(NewterrainLayer);

                item.terrainData.terrainLayers = NewLayers.ToArray();

#else

            SplatPrototype sp = new SplatPrototype
            {
                texture = tex,
                tileSize = new Vector2(item.size.x, item.size.z),
                tileOffset = Vector2.zero
            };
            item.terrainData.splatPrototypes = new[] { sp };
#endif
            }
            else
            {
                item.
                    terrainData.
                    terrainLayers[0].
                    diffuseTexture = 
                    generatedText;
            }


        }
        public static async Task WriteShaderAsync(Texture2D texture, string path)
        {
            byte[] bytes = texture.EncodeToPNG();
            await GISTerrainLoaderFileAsync.WriteAllBytes(path, bytes);
        }
        public static void WriteShaderRuntime(Texture2D texture, string path)
        {
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
        }
        public static Texture2D LoadedTextureTile(string TexturePath)
        {
            Texture2D tex = new Texture2D(2, 2);

            if (File.Exists(TexturePath))
            {
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.LoadImage(File.ReadAllBytes(TexturePath));
                tex.LoadImage(tex.EncodeToJPG(100));
            }
            return tex;
        }

        private static Texture2D SaveShadedTexture(string terrainPath, Texture2D ShadedTexture, ShadedTextureType shadedTextureType)
        {

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var folderPath = Path.GetDirectoryName(terrainPath);
                var TerrainFilename = Path.GetFileNameWithoutExtension(terrainPath);
                var ShaderTexturesFolder = Path.Combine(folderPath, TerrainFilename + "_ShadedTextures");

                if (!Directory.Exists(ShaderTexturesFolder))
                    Directory.CreateDirectory(ShaderTexturesFolder);

                DirectoryInfo di = new DirectoryInfo(terrainPath);

                var ResourceShaderPath = Path.GetFileNameWithoutExtension(terrainPath) + "_ShadedTextures";

                for (int i = 0; i <= 5; i++)
                {
                    di = di.Parent;
                    ResourceShaderPath = di.Name + "/" + ResourceShaderPath;

                    if (di.Name == "GIS Terrains") break;

                    if (i == 5)
                    {
                        Debug.LogError("Texture folder not found! : Please put your terrain in GIS Terrain Loader/Recources/GIS Terrains/");

                        return null;

                    }

                }

                var TexturePath = ShaderTexturesFolder + "/" + shadedTextureType.ToString() + ".jpg";
                byte[] bytes = ShadedTexture.EncodeToJPG();
                File.WriteAllBytes(TexturePath, bytes);

                AssetDatabase.Refresh();

                var ResourceTexturePath = ResourceShaderPath + "/" + shadedTextureType;

                ShadedTexture = (Texture2D)Resources.Load(ResourceTexturePath);
            }
            else
            {
                if (!string.IsNullOrEmpty(terrainPath))
                {
                    var folderPath = Path.GetDirectoryName(terrainPath);
                    var TerrainFilename = Path.GetFileNameWithoutExtension(terrainPath);
                    var ShaderFolder = Path.Combine(folderPath, TerrainFilename + "_ShadedTextures");

                    if (!Directory.Exists(ShaderFolder))
                        Directory.CreateDirectory(ShaderFolder);

                    var TexturePath = ShaderFolder + "/" + shadedTextureType.ToString() + ".jpg";
                    byte[] bytes = ShadedTexture.EncodeToJPG();
                    File.WriteAllBytes(TexturePath, bytes);

                }
            }
#else
            if (!string.IsNullOrEmpty(terrainPath))
            {
                var folderPath = Path.GetDirectoryName(terrainPath);
                var TerrainFilename = Path.GetFileNameWithoutExtension(terrainPath);
                var ShaderFolder = Path.Combine(folderPath, TerrainFilename + "_ShadedTextures");

                if (!Directory.Exists(ShaderFolder))
                    Directory.CreateDirectory(ShaderFolder);

                var TexturePath = ShaderFolder + "/" + shadedTextureType.ToString() + ".jpg";
                byte[] bytes = ShadedTexture.EncodeToJPG();
                File.WriteAllBytes(TexturePath, bytes);

            }
#endif
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Resources.UnloadUnusedAssets();

            return ShadedTexture;
        }


        private static float GetHeight(int x, int y)
        {
            return GetNormalizedHeight(x, y) * Terrain_Size_Y;
        }
        private static float GetNormalizedHeight(int x, int y)
        {
            x = Mathf.Clamp(x, 0, TextureSize.x - 1);
            y = Mathf.Clamp(y, 0, TextureSize.y - 1);

            return data[x, y];
        }
        private static float GetNormalizedHeight(int x, int y, float[,] datax, Vector2Int TextureSize)
        {
            x = Mathf.Clamp(x, 0, TextureSize.x - 1);
            y = Mathf.Clamp(y, 0, TextureSize.y - 1);

            return datax[x, y];
        }
        public static Vector2 DerivativeCal(int x, int y)
        {
            float CellPixelSize = 10;
            float El1 = GetHeight(x - 1, y + 1);
            float El2 = GetHeight(x + 0, y + 1);
            float El3 = GetHeight(x + 1, y + 1);
            float El4 = GetHeight(x - 1, y + 0);
            float El6 = GetHeight(x + 1, y + 0);
            float El7 = GetHeight(x - 1, y - 1);
            float El8 = GetHeight(x + 0, y - 1);
            float El9 = GetHeight(x + 1, y - 1);

            float El_x = (El3 + El6 + El9 - El1 - El4 - El7) / (6.0f * CellPixelSize);
            float El_y = (El1 + El2 + El3 - El7 - El8 - El9) / (6.0f * CellPixelSize);

            return new Vector2(-El_x, -El_y);
        }
        private static Color GetColor(float v, float exponent, bool nonNegative)
        {
            if (exponent > 0)
            {
                float sign = GISTerrainLoaderMath.SignOrZero(v);
                float pow = Mathf.Pow(10, exponent);
                float log = Mathf.Log(1.0f + pow * Mathf.Abs(v));

                v = sign * log;
            }

            if (nonNegative)
                return Color_Gradien.GetPixelBilinear(v, 0);
            else
            {
                if (v > 0)
                    return Main_Gradien_Pos.GetPixelBilinear(v, 0);
                else
                    return Negative_Gradien_Pos.GetPixelBilinear(-v, 0);
            }
        }
   

    }
}