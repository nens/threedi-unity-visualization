/*     Unity GIS Tech 2020-2023      */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
 
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace GISTech.GISTerrainLoader
{

    public class GISTerrainLoaderTextureGenerator
    {
        static Texture2D terrainTexture;
        #region Editor

#if UNITY_EDITOR
        public static async Task EditorAddTextureToTerrain(string[] Tiles, string TextureFolder, string ResFolderpath, GISTerrainLoaderSettings_SO Settings_SO , TextureSourceFormat TextureSourceSoft, GISTerrainTile terrainItem)
        {
#if UNITY_EDITOR

                    ResourceRequest resourcesRequest;

                    string TileName = GetTileName(TextureSourceSoft, terrainItem);


                    string TextureTilePath = TextureFolder + "/" + TileName;

                    bool TextureExist = false;

                    string TexturePathInResource = "";

                    foreach (var ext in Settings_SO.SupportedTextures)
                    {
                        var FullPath = TextureTilePath + ext;

                        TexturePathInResource = ResFolderpath + "/" + TileName;
 
                if (File.Exists(FullPath))
                        {
                            TextureExist = true;
                            break;
                        }
                    }
 

            if (TextureExist)
            {
 
                resourcesRequest = Resources.LoadAsync<Texture2D>(TexturePathInResource);

                while (!resourcesRequest.isDone)
                            await Task.Delay(TimeSpan.FromSeconds(0.01));

                terrainTexture = resourcesRequest.asset as Texture2D;

            }
            else
                    {
                        resourcesRequest = Resources.LoadAsync("Textures/NullTexture", typeof(Texture2D));

                        while (!resourcesRequest.isDone)
                            await Task.Delay(TimeSpan.FromSeconds(0.01));

                        terrainTexture = resourcesRequest.asset as Texture2D;
 

                        Debug.Log("Texture not found : " + TexturePathInResource);
                    }

#if UNITY_2018_1_OR_NEWER
                    TerrainLayer NewterrainLayer = new TerrainLayer();

                    string path = Path.Combine(terrainItem.container.TerrainFilePath, terrainItem.name + ".terrainlayer");
                    AssetDatabase.CreateAsset(NewterrainLayer, path);

                    TerrainLayer[] ExistingTerrainLayers = terrainItem.terrainData.terrainLayers;
                    List<TerrainLayer> NewLayers = new List<TerrainLayer>();

                    foreach (var l in ExistingTerrainLayers)
                    {
                        NewLayers.Add(l);
                    }


                    NewterrainLayer.diffuseTexture = terrainTexture;

                    NewterrainLayer.tileSize = new Vector2(terrainItem.size.x, terrainItem.size.z);
                    NewterrainLayer.tileOffset = Vector2.zero;

                    NewLayers.Add(NewterrainLayer);
                    terrainItem.terrainData.terrainLayers = NewLayers.ToArray();

#else

            SplatPrototype sp = new SplatPrototype
            {
                texture = terrainTexture,
                tileSize = new Vector2(terrainItem.size.x, terrainItem.size.z),
                tileOffset = Vector2.zero
            };
            terrain.terrainData.splatPrototypes = new[] { sp };
#endif
 
#endif
        }
        public static async Task EditorAddMultiTextureToTerrain(int FolderIndex,string[] Tiles, string TextureFolder, string[] ResFolderpath, GISTerrainLoaderSettings_SO Settings_SO, TextureSourceFormat TextureSourceSoft, GISTerrainTile terrainItem)
        {
#if UNITY_EDITOR

            ResourceRequest resourcesRequest;

            string TileName = GetTileName(TextureSourceSoft, terrainItem);


            string TextureTilePath = TextureFolder + "/" + TileName;

            bool TextureExist = false;

            string TexturePathInResource = "";

            foreach (var ext in Settings_SO.SupportedTextures)
            {
                var FullPath = TextureTilePath + ext;
  
                TexturePathInResource = ResFolderpath[FolderIndex] + "/" + TileName;
 
                if (File.Exists(FullPath))
                {
                    TextureExist = true;
                    break;
                }
            }
 
            if (TextureExist)
            {
                resourcesRequest = Resources.LoadAsync<Texture2D>(TexturePathInResource);

                while (!resourcesRequest.isDone)
                    await Task.Delay(TimeSpan.FromSeconds(0.01));

                terrainTexture = resourcesRequest.asset as Texture2D;
            }
            else
            {
                resourcesRequest = Resources.LoadAsync("Textures/NullTexture", typeof(Texture2D));

                while (!resourcesRequest.isDone)
                    await Task.Delay(TimeSpan.FromSeconds(0.01));

                terrainTexture = resourcesRequest.asset as Texture2D;

                Debug.Log("Texture not found : " + TexturePathInResource);
            }




#if UNITY_2018_1_OR_NEWER

            TerrainLayer NewterrainLayer = new TerrainLayer();

            string path = Path.Combine(terrainItem.container.TerrainFilePath, terrainItem.name+"_"+ FolderIndex + ".terrainlayer");
            AssetDatabase.CreateAsset(NewterrainLayer, path);

            Debug.Log(" terrainTexture :  " + TexturePathInResource + "  " + terrainTexture.width +" " + path);

            TerrainLayer[] ExistingTerrainLayers = terrainItem.terrainData.terrainLayers;

            List<TerrainLayer> NewLayers = new List<TerrainLayer>();

            foreach (var l in ExistingTerrainLayers)
            {
                NewLayers.Add(l);
            }


            NewterrainLayer.diffuseTexture = terrainTexture;
            NewterrainLayer.tileSize = new Vector2(terrainItem.size.x, terrainItem.size.z);
            NewterrainLayer.tileOffset = Vector2.zero;

            NewLayers.Add(NewterrainLayer);
            terrainItem.terrainData.terrainLayers = NewLayers.ToArray();

#else

            SplatPrototype sp = new SplatPrototype
            {
                texture = terrainTexture,
                tileSize = new Vector2(terrainItem.size.x, terrainItem.size.z),
                tileOffset = Vector2.zero
            };
            terrain.terrainData.splatPrototypes = new[] { sp };
#endif

#endif
        }

        public static string GetTextureFolder(string TerrainFilePath)
        {
            string TerrainFileName = Path.GetFileNameWithoutExtension(TerrainFilePath);
           
            var Settings_SO = (GISTerrainLoaderSettings_SO)Resources.Load("Settings/GISTerrainLoaderSettings", typeof(GISTerrainLoaderSettings_SO));

            string TextureFolder = "";
 
            DirectoryInfo di = new DirectoryInfo(TerrainFilePath);

            TextureFolder = TerrainFileName + Settings_SO.TextureFolderName;

            for (int i = 0; i <= 5; i++)
            {
                di = di.Parent;
                TextureFolder = di.Name + "/" + TextureFolder;

                if (di.Name == "GIS Terrains")
                {
                   break;
                }
            }
            return TextureFolder;
        }
        public static List<string> GetResTextureFolders(string TerrainFilePath)
        {
            List<string> TextureFolders = new List<string>();

            string TerrainFileName = Path.GetFileNameWithoutExtension(TerrainFilePath);

            var Settings_SO = (GISTerrainLoaderSettings_SO)Resources.Load("Settings/GISTerrainLoaderSettings", typeof(GISTerrainLoaderSettings_SO));

            string TextureFolder = "";

            DirectoryInfo di = new DirectoryInfo(TerrainFilePath);

            var MainTextureFolderPath = Path.Combine(Path.GetDirectoryName(TerrainFilePath), Path.GetFileNameWithoutExtension(TerrainFilePath) + Settings_SO.TextureFolderName);

            //if (Directory.Exists(MainTextureFolderPath))
            //{
            //    TextureFolder = TerrainFileName + Settings_SO.TextureFolderName;
            //    TextureFolders.Add(TextureFolder);
            //}

            for (int i = 0; i <= 10; i++)
            {
                if(i==0)
                    TextureFolder = TerrainFileName + Settings_SO.TextureFolderName;
                else
                TextureFolder = TerrainFileName + Settings_SO.TextureFolderName + "_" + i;

               

                for (int j = 0; j <= 5; j++)
                {
                    di = di.Parent;

                    TextureFolder = di.Name + "/" + TextureFolder;

                    if (di.Name == "GIS Terrains")
                    {
                        string fullTextureFolder = "";

                        if (i == 0)
                            fullTextureFolder = Path.Combine(Path.GetDirectoryName(TerrainFilePath), Path.GetFileNameWithoutExtension(TerrainFilePath) + Settings_SO.TextureFolderName);
                        else
                            fullTextureFolder = Path.Combine(Path.GetDirectoryName(TerrainFilePath), Path.GetFileNameWithoutExtension(TerrainFilePath) + Settings_SO.TextureFolderName + "_" + i);
 
                        if (Directory.Exists(fullTextureFolder))
                        {
                            TextureFolders.Add(TextureFolder);
                        }

                        di = new DirectoryInfo(TerrainFilePath);

                        break;
                    }
                }

            }

            return TextureFolders;


        }
#endif
        public static List<string> GetFullTextureFolders(string TerrainFilePath)
        {
            List<string> TextureFolders = new List<string>();

            string TerrainFileName = Path.GetFileNameWithoutExtension(TerrainFilePath);

            var Settings_SO = (GISTerrainLoaderSettings_SO)Resources.Load("Settings/GISTerrainLoaderSettings", typeof(GISTerrainLoaderSettings_SO));

            string TextureFolder = "";

            DirectoryInfo di = new DirectoryInfo(TerrainFilePath);

            var MainTextureFolderPath = Path.Combine(Path.GetDirectoryName(TerrainFilePath), Path.GetFileNameWithoutExtension(TerrainFilePath) + Settings_SO.TextureFolderName);

            if (Directory.Exists(MainTextureFolderPath))
            {
                TextureFolders.Add(MainTextureFolderPath);
            }

            for (int i = 1; i <= 10; i++)
            {

                TextureFolder = TerrainFileName + Settings_SO.TextureFolderName + "_" + i;

                for (int j = 0; j <= 5; j++)
                {
                    di = di.Parent;

                    TextureFolder = di.Name + "/" + TextureFolder;

                    if (di.Name == "GIS Terrains")
                    {
                        TextureFolder = Path.Combine(Path.GetDirectoryName(TerrainFilePath), Path.GetFileNameWithoutExtension(TerrainFilePath) + Settings_SO.TextureFolderName + "_" + i);

                        if (Directory.Exists(TextureFolder))
                        {
                            TextureFolders.Add(TextureFolder);
                        }

                        di = new DirectoryInfo(TerrainFilePath);

                        break;
                    }
                }

            }

            return TextureFolders;


        }
        public static string[] GetTextureTiles(string TextureFolder, GISTerrainLoaderSettings_SO Settings_SO)
        {
            string[] tiles = null;
            tiles = Directory.GetFiles(TextureFolder, "*.*", SearchOption.AllDirectories).Where(f => Settings_SO.SupportedTextures.Contains(new FileInfo(f).Extension, StringComparer.OrdinalIgnoreCase)).ToArray();
            return tiles;
        }
        public static Texture2D[] GetTextureInFolder_Editor(string terrainPath)
        {
            var Settings_SO = (GISTerrainLoaderSettings_SO)Resources.Load("Settings/GISTerrainLoaderSettings", typeof(GISTerrainLoaderSettings_SO));

            var folderPath = Path.GetDirectoryName(terrainPath);
            var TerrainFilename = Path.GetFileNameWithoutExtension(terrainPath);
            var TextureFolder = Path.Combine(folderPath, TerrainFilename + Settings_SO.TextureFolderName);

            string[] tiles = null;

            List<Texture2D> TextureTiles = new List<Texture2D>();

            if (Directory.Exists(TextureFolder))
            {
                tiles = Directory.GetFiles(TextureFolder, "*.*", SearchOption.AllDirectories).Where(f => Settings_SO.SupportedTextures.Contains(new FileInfo(f).Extension, StringComparer.OrdinalIgnoreCase)).ToArray();

                foreach (var tile in tiles)
                {

                    TextureTiles.Add(LoadedTextureTile(tile));

                }
            }
            return TextureTiles.ToArray();
        }
        #endregion
        #region Runtime

        public static void RuntimeAddTexturesToTerrain(string TextureFolder, GISTerrainLoaderSettings_SO Settings_SO, TextureSourceFormat TextureSourceSoft, GISTerrainTile terrainItem, bool ClearLayers = false)
        {
            string TileName = GetTileName(TextureSourceSoft, terrainItem);

            string TextureTilePath = TextureFolder + "/" + TileName;

            bool TextureExist = false;

            string TexturePath = "";

            foreach (var ext in Settings_SO.SupportedTextures)
            {
                var FullPath = TextureTilePath + ext;

                TexturePath = TextureFolder + "/" + TileName;

                if (File.Exists(FullPath))
                {
                    TexturePath = FullPath;

                    TextureExist = true;
                    break;
                }
            }

            if (TextureExist)
            {

                terrainTexture = new Texture2D(0, 0);
                terrainTexture = LoadedTextureTile(TexturePath);
                terrainItem.TextureState = TextureState.Loaded;

            }
            else
            {
                terrainTexture = (Texture2D)Resources.Load("Textures/NullTexture");
                Debug.Log("Texture not found : " + TexturePath);
                terrainItem.TextureState = TextureState.Loaded;

            }

#if UNITY_2018_1_OR_NEWER
            TerrainLayer NewterrainLayer = new TerrainLayer();

            TerrainLayer[] ExistingTerrainLayers = terrainItem.terrainData.terrainLayers;

            List<TerrainLayer> NewLayers = new List<TerrainLayer>();
            
            if (!ClearLayers)
            {
                foreach (var l in ExistingTerrainLayers)
                {
                    if (l != null)
                        NewLayers.Add(l);
                }
            }



            NewterrainLayer.diffuseTexture = terrainTexture;

            NewterrainLayer.tileSize = new Vector2(terrainItem.size.x, terrainItem.size.z);
            NewterrainLayer.tileOffset = Vector2.zero;

            NewLayers.Add(NewterrainLayer);
            terrainItem.terrainData.terrainLayers = NewLayers.ToArray();


#else
            SplatPrototype sp = new SplatPrototype
            {
                texture = tex,
                tileSize = new Vector2(terrainItem.size.x, terrainItem.size.z),
                tileOffset = Vector2.zero
            };
            terrain.terrainData.splatPrototypes = new[] { sp };
#endif

        }
        public static void RuntimeAddMultiTextureToTerrain(string TextureFolder, GISTerrainLoaderSettings_SO Settings_SO, TextureSourceFormat TextureSourceSoft, GISTerrainTile terrainItem, bool ClearLayers = false)
        {
            string TileName = GetTileName(TextureSourceSoft, terrainItem);

            string TextureTilePath = TextureFolder + "/" + TileName;

            bool TextureExist = false;

            string TexturePath = "";

            foreach (var ext in Settings_SO.SupportedTextures)
            {
                var FullPath = TextureTilePath + ext;

                TexturePath = TextureFolder + "/" + TileName;

                if (File.Exists(FullPath))
                {
                    TexturePath = FullPath;

                    TextureExist = true;
                    break;
                }
            }

            if (TextureExist)
            {

                terrainTexture = new Texture2D(0, 0);
                terrainTexture = LoadedTextureTile(TexturePath);
                terrainItem.TextureState = TextureState.Loaded;

            }
            else
            {
                terrainTexture = (Texture2D)Resources.Load("Textures/NullTexture");
                Debug.Log("Texture not found : " + TexturePath);
                terrainItem.TextureState = TextureState.Loaded;

            }

#if UNITY_2018_1_OR_NEWER

            TerrainLayer NewterrainLayer = new TerrainLayer();

            TerrainLayer[] ExistingTerrainLayers = terrainItem.terrainData.terrainLayers;

            List<TerrainLayer> NewLayers = new List<TerrainLayer>();

            if (!ClearLayers)
            {
                foreach (var l in ExistingTerrainLayers)
                {
                    if (l != null)
                        NewLayers.Add(l);
                }
            }



            NewterrainLayer.diffuseTexture = terrainTexture;

            NewterrainLayer.tileSize = new Vector2(terrainItem.size.x, terrainItem.size.z);
            NewterrainLayer.tileOffset = Vector2.zero;

            NewLayers.Add(NewterrainLayer);
            terrainItem.terrainData.terrainLayers = NewLayers.ToArray();


#else
            SplatPrototype sp = new SplatPrototype
            {
                texture = tex,
                tileSize = new Vector2(terrainItem.size.x, terrainItem.size.z),
                tileOffset = Vector2.zero
            };
            terrain.terrainData.splatPrototypes = new[] { sp };
#endif

        }

        public static void RuntimePlatformeAddTexturesToTerrain(Texture2D texturePath, GISTerrainTile terrainItem,bool ClearLayers=false)
        {

#if UNITY_2018_1_OR_NEWER
            TerrainLayer NewterrainLayer = new TerrainLayer();

            TerrainLayer[] ExistingTerrainLayers = terrainItem.terrainData.terrainLayers;

            List<TerrainLayer> NewLayers = new List<TerrainLayer>();

            if (!ClearLayers)
            {
                foreach (var l in ExistingTerrainLayers)
                {
                    if (l != null)
                        NewLayers.Add(l);
                }
            }



            NewterrainLayer.diffuseTexture = texturePath;

            NewterrainLayer.tileSize = new Vector2(terrainItem.size.x, terrainItem.size.z);
            NewterrainLayer.tileOffset = Vector2.zero;

            NewLayers.Add(NewterrainLayer);
            terrainItem.terrainData.terrainLayers = NewLayers.ToArray();


#else
            SplatPrototype sp = new SplatPrototype
            {
                texture = tex,
                tileSize = new Vector2(terrainItem.size.x, terrainItem.size.z),
                tileOffset = Vector2.zero
            };
            terrain.terrainData.splatPrototypes = new[] { sp };
#endif

        }
        public static Texture2D LoadedTextureTile(string TexturePath, TextureWrapMode wrapMode = TextureWrapMode.Clamp)
        {
            Texture2D tex = new Texture2D(2, 2);

            if (File.Exists(TexturePath))
            {

                if (Path.GetExtension(TexturePath) == ".tif")
                {
                    tex.wrapMode = wrapMode;
                    tex = GISTerrainLoaderTIFFLoader.TiffToTexture2D(TexturePath);
                }
                else
                {
                    tex.wrapMode = wrapMode;
                    tex.LoadImage(File.ReadAllBytes(TexturePath));
                    tex.LoadImage(tex.EncodeToJPG(100));
                }

            }
            return tex;
        }
        #endregion
        private static string[] GetTextureTiles(string terrainPath)
        {
            var Settings_SO = (GISTerrainLoaderSettings_SO)Resources.Load("Settings/GISTerrainLoaderSettings", typeof(GISTerrainLoaderSettings_SO));

            var folderPath = Path.GetDirectoryName(terrainPath);
            var TerrainFilename = Path.GetFileNameWithoutExtension(terrainPath);
            var TextureFolder = Path.Combine(folderPath, TerrainFilename + Settings_SO .TextureFolderName);
            string[] tiles = null;

            if (Directory.Exists(TextureFolder))
            {
                tiles = Directory.GetFiles(TextureFolder, "*.*", SearchOption.AllDirectories).Where(f => Settings_SO.SupportedTextures.Contains(new FileInfo(f).Extension, StringComparer.OrdinalIgnoreCase)).ToArray();
            }

            return tiles;
        }
        public static bool CheckForTerrainTextureFolder(string terrainPath, out TextureSourceFormat TextureSourceSoft)
        {
            bool TextureFolderExist = false;

            TextureSourceSoft = null;

            var Settings_SO = (GISTerrainLoaderSettings_SO)Resources.Load("Settings/GISTerrainLoaderSettings", typeof(GISTerrainLoaderSettings_SO));

            var folderPath = Path.GetDirectoryName(terrainPath);
            var TerrainFilename = Path.GetFileNameWithoutExtension(terrainPath);
            var TextureFolder = Path.Combine(folderPath, TerrainFilename + Settings_SO.TextureFolderName);

            if (Directory.Exists(TextureFolder))
            {
                TextureFolderExist = true;
                var Tiles = GetTextureTiles(terrainPath);

                bool IsCorrectFormat = false;

                if (Tiles.Length > 0)
                 IsCorrectFormat = TextureSourceName(Tiles, Settings_SO, out TextureSourceSoft);
                else
                    Debug.Log("No textures found in the texturefolder ..");

            }

            return TextureFolderExist;
        }
        private static string GetTileName(TextureSourceFormat TextureSourceSoft, GISTerrainTile terrainItem)
        {
            string TileName = "";

            string Format = TextureSourceSoft.Format.Replace("{x}", "{0}").Replace("{y}", "{1}");

            if (TextureSourceSoft.TilesOrder == TextureOrder.Col_Row)
            {
                int x = terrainItem.Number.x + TextureSourceSoft.StartTileNumber.x;
                int y = terrainItem.Number.y + TextureSourceSoft.StartTileNumber.y;
                TileName = string.Format(Format, x, y);
            }

            if (TextureSourceSoft.TilesOrder == TextureOrder.Col_Row_Reversed)
            {
                int x = terrainItem.Number.x + TextureSourceSoft.StartTileNumber.x;
                int y = (terrainItem.container.TerrainCount.y - 1 - terrainItem.Number.y) + +TextureSourceSoft.StartTileNumber.y;
                TileName = string.Format(Format, x, y);
            }

            if (TextureSourceSoft.TilesOrder == TextureOrder.Row_Col)
            {
                int x = terrainItem.Number.x + TextureSourceSoft.StartTileNumber.x;
                int y = (terrainItem.container.TerrainCount.y - 1) - terrainItem.Number.y + TextureSourceSoft.StartTileNumber.y;
                TileName = string.Format(Format, y, x);
            }

            if (TextureSourceSoft.TilesOrder == TextureOrder.Row_Col_Reversed)
            {
                int x = terrainItem.Number.y + TextureSourceSoft.StartTileNumber.x;
                int y = terrainItem.Number.x + TextureSourceSoft.StartTileNumber.y;
                TileName = string.Format(Format, x, y);
            }

            return TileName;
        }
        public static bool TextureSourceName(string[] Tiles, GISTerrainLoaderSettings_SO Settings_SO, out TextureSourceFormat TextureSource)
        {
            bool Correct = false;

            TextureSource = new TextureSourceFormat();
         
            var FirstTileName = Path.GetFileNameWithoutExtension(Tiles[0]);
            var LastTileName = Path.GetFileNameWithoutExtension(Tiles[Tiles.Length - 1]);

            foreach (var source in Settings_SO.TextureFormats)
            {
                var format = ("^" + source.Format.Replace("{x}", @"\d*").Replace("{y}", @"\d*"));

                if (Regex.IsMatch(FirstTileName, format) && Regex.IsMatch(LastTileName, format))
                {
                    Correct = true;
                    TextureSource = source;
                    break;
                }
            }
            return Correct;
        }
        public static Vector2Int GetTilesNumberInTextureFolder(string terrainPath)
        {
            var Settings_SO = (GISTerrainLoaderSettings_SO)Resources.Load("Settings/GISTerrainLoaderSettings", typeof(GISTerrainLoaderSettings_SO));

            Vector2Int Tiles = Vector2Int.zero;

            var folderPath = Path.GetDirectoryName(terrainPath);

            var TerrainFilename = Path.GetFileNameWithoutExtension(terrainPath);

            var TextureFolder = Path.Combine(folderPath, TerrainFilename + Settings_SO.TextureFolderName);

            if (Directory.Exists(TextureFolder))
            {
                var tiles = Directory.GetFiles(TextureFolder, "*.*", SearchOption.AllDirectories).Where(f => Settings_SO.SupportedTextures.Contains(new FileInfo(f).Extension, StringComparer.OrdinalIgnoreCase)).ToArray();
                TextureSourceFormat TextureSourceSoft = null;

                bool IsCorrectFormat = false;

                if (tiles.Length > 0)
                    IsCorrectFormat = TextureSourceName(tiles, Settings_SO, out TextureSourceSoft);
                else
                    Debug.Log("No textures found in the texturefolder ..");

                Vector2Int First_Tile_Index = new Vector2Int(0, 0);
                Vector2Int Last_Tile_Index = new Vector2Int(0, 0);

                if (IsCorrectFormat)
                {
                    List<int> X_index = new List<int>(0);
                    List<int> Y_index = new List<int>(0);

                    foreach (string value in tiles)
                    {
                        var withoutfull = Path.GetFileNameWithoutExtension(value);

                        var parts = Regex.Split(withoutfull, @"\D+");
                       
                        int count = 0;
                        int R_Count = 0;

                        foreach (string value0 in parts)
                        {
                            if (!string.IsNullOrEmpty(value0))
                            {
                                int i = int.Parse(value0);

                                if(i>=0)
                                {
                                    count++;
                                    if(R_Count==0)
                                    R_Count = count;
                                }
      
                            }

                        }

                        int x = int.Parse(parts[R_Count]);
                        int y = int.Parse(parts[R_Count+1]);

                        X_index.Add(x);
                        Y_index.Add(y);
 
                        R_Count = 0;
                        count = 0;

                    }

                    X_index.Sort();
                    Y_index.Sort();

                    First_Tile_Index = new Vector2Int(X_index[0], Y_index[0]);
                    int X_Count = X_index[X_index.Count - 1];
                    int Y_Count = Y_index[Y_index.Count - 1];
                    Last_Tile_Index = new Vector2Int(X_Count, Y_index[Y_index.Count - 1]);


                    if (X_index.Count > 0 && Y_index.Count > 0)
                    {
                        if(TextureSourceSoft.TilesOrder == TextureOrder.Col_Row || TextureSourceSoft.TilesOrder == TextureOrder.Col_Row_Reversed)
                        {
                            Last_Tile_Index = new Vector2Int(X_Count, Y_Count);
                       
                        }else
                        {
                            Last_Tile_Index = new Vector2Int(Y_Count, X_Count);
                        }
                    }

                    if (First_Tile_Index.x == 0)
                        Last_Tile_Index.x++;

                    if (First_Tile_Index.y == 0)
                        Last_Tile_Index.y++;

                    Tiles = Last_Tile_Index;
                }
            }
            return Tiles;

        }

        /// <summary>
        /// Rotate Texture according to terrain side
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="terrainSide"></param>
        /// <returns></returns>
        public static Texture2D OrientedTexture(Color32[] SourcePix, TerrainSide terrainSide, int TerrainBackgroundTextureResolution = 512, string TextureSavePath ="")
        {
            int m_x = TerrainBackgroundTextureResolution;
            int m_y =  TerrainBackgroundTextureResolution;

            Color32[] RotatedPix = new Color32[SourcePix.Length];

            int iOriginal;

            for (int j = 0; j < m_x; ++j)
            {
                for (int i = 0; i < m_y; ++i)
                {
                    switch (terrainSide)
                    {
                        case TerrainSide.Non:
                            //tdataHeightmap[y, x] = el;
                            break;
                        case TerrainSide.Bottom:
                            iOriginal = SourcePix.Length - 1 - (i * m_y + j);
                            RotatedPix[(i + 1) * m_x - j - 1] = SourcePix[iOriginal]; break;
                        case TerrainSide.Top:
                            iOriginal = SourcePix.Length - 1 - (i * m_y + j);
                            RotatedPix[(i + 1) * m_x - j - 1] = SourcePix[iOriginal]; break;
                        case TerrainSide.Right:
                            iOriginal = (i * m_y + j);
                            RotatedPix[(i + 1) * m_x - j - 1] = SourcePix[iOriginal]; break;

                        case TerrainSide.Left:
                            iOriginal = (i * m_y + j);
                            RotatedPix[(i + 1) * m_x - j - 1] = SourcePix[iOriginal]; break;

                        case TerrainSide.TopRight:
                            iOriginal = (i * m_y + j);
                            RotatedPix[(SourcePix.Length - 1) - ((i * m_y + j))] = SourcePix[iOriginal]; break;

                        case TerrainSide.TopLeft:
                            iOriginal = (i * m_y + j);
                            RotatedPix[(SourcePix.Length - 1) - ((i * m_y + j))] = SourcePix[iOriginal]; break;


                        case TerrainSide.BottomRight:
                            iOriginal = (i * m_y + j);
                            RotatedPix[(SourcePix.Length - 1) - ((i * m_y + j))] = SourcePix[iOriginal]; break;

                        case TerrainSide.BottomLeft:
                            iOriginal = (i * m_y + j);
                            RotatedPix[(SourcePix.Length - 1) - ((i * m_y + j))] = SourcePix[iOriginal]; break;
                    }

                }
            }


            Texture2D rotatedTexture = new Texture2D(m_y, m_x);
            rotatedTexture.SetPixels32(RotatedPix);
            rotatedTexture.Apply();

#if UNITY_EDITOR
            if(!Application.isPlaying)
            {
                string textureFilename = Path.Combine(TextureSavePath, "BGTile__" + terrainSide.GetDescription().ToString() + ".jpg");
                File.WriteAllBytes(textureFilename, rotatedTexture.EncodeToJPG());
                AssetDatabase.Refresh();

                TextureImporter importer = AssetImporter.GetAtPath(textureFilename) as TextureImporter;

                if (importer != null)
                {
                    importer.maxTextureSize = Mathf.Max(TerrainBackgroundTextureResolution, TerrainBackgroundTextureResolution);
                    importer.wrapMode = TextureWrapMode.Clamp;
                    importer.SaveAndReimport();
                }

                rotatedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(textureFilename);
            }

#endif
            return rotatedTexture;
        }



        
        /// <summary>
        /// Get a Texture Capture to terrain container
        /// </summary>
        /// <param name="container"></param>
        /// <param name="Prefs"></param>
        /// <returns></returns>
        public static Texture2D CaptureContainerTexture(GISTerrainContainer container, GISTerrainLoaderPrefs Prefs)
        {
            Texture2D texture = new Texture2D(Prefs.TerrainBackgroundTextureResolution, Prefs.TerrainBackgroundTextureResolution, TextureFormat.ARGB32, false, false);

            int mLayer = 1 << Prefs.TerrainLayer;

            float CaptureDistance = container.ContainerSize.y + 10;

            var ContainerBoundsRange = container.GlobalTerrainBounds.max - container.GlobalTerrainBounds.min;


            Vector3 CaptureSize = container.ContainerSize;
            CaptureSize.x = CaptureSize.x / Prefs.TerrainBackgroundTextureResolution;
            CaptureSize.z = CaptureSize.z / Prefs.TerrainBackgroundTextureResolution;
            CaptureSize.y = container.ContainerSize.y * 1.5f;


            Vector3 CaptureStartPosition = container.GlobalTerrainBounds.min;
            CaptureStartPosition.y += CaptureDistance;

            Vector3 CapturePoint = CaptureStartPosition + new Vector3(Prefs.TerrainBackgroundTextureResolution / 2 * CaptureSize.x, CaptureStartPosition.y, Prefs.TerrainBackgroundTextureResolution / 2 * CaptureSize.z);
            CapturePoint.x = (CapturePoint.x - container.GlobalTerrainBounds.min.x) * ContainerBoundsRange.x / container.ContainerSize.x + container.GlobalTerrainBounds.min.x;
            CapturePoint.z = (CapturePoint.z - container.GlobalTerrainBounds.min.z) * ContainerBoundsRange.z / container.ContainerSize.z + container.GlobalTerrainBounds.min.z;

            GameObject CaptureGO = new GameObject();
            Camera camera = CaptureGO.AddComponent<Camera>();

            CaptureGO.transform.position = CapturePoint;
            CaptureGO.transform.rotation = Quaternion.Euler(90, 0, 0);
            camera.orthographic = true;
            camera.orthographicSize = ContainerBoundsRange.x / 2;
            camera.clearFlags = CameraClearFlags.Color;
            camera.backgroundColor = Color.black;
            camera.cullingMask = mLayer;

            camera.targetTexture = new RenderTexture(Prefs.TerrainBackgroundTextureResolution, Prefs.TerrainBackgroundTextureResolution, 16);
            camera.nearClipPlane = 0.00001f;
            camera.farClipPlane = CaptureDistance * 2;

            float height = 2f * camera.orthographicSize;
            float width = height * camera.aspect;

            var width_Diff = width - container.ContainerSize.x;
            var height_Diff = height - container.ContainerSize.z;
            CaptureGO.transform.position += new Vector3(width_Diff / 2, 0, height_Diff / 2);

            var prevouisScale = container.ContainerSize;

            container.RescaleTerrain(new Vector3(height, 53.6f, width));

            RenderTexture currentRT = RenderTexture.active;
            RenderTexture.active = camera.targetTexture;
            camera.Render();


            texture.wrapMode = TextureWrapMode.Clamp;
            texture.ReadPixels(new Rect(0, 0, Prefs.TerrainBackgroundTextureResolution, Prefs.TerrainBackgroundTextureResolution), 0, 0);
            texture.Apply();

            RenderTexture.active = currentRT;

#if UNITY_EDITOR
            if(!Application.isPlaying)
            UnityEngine.Object.DestroyImmediate(CaptureGO);
            else UnityEngine.Object.Destroy(CaptureGO);
#else
                        UnityEngine.Object.Destroy(CaptureGO);
#endif
            container.RescaleTerrain(prevouisScale);

            return texture;
        }

        /// <summary>
        /// Adjust Texture Brightness , Contrast, gamma
        /// </summary>
        /// <param name="source"></param>
        /// <param name="brightness"></param>
        /// <param name="contrast"></param>
        /// <param name="gamma"></param>
        /// <returns></returns>
        public static Color32[] AdjustBrightnessContrast(Color32[] source,
                   float brightness = 1f, float contrast = 1f, float gamma = 1f)
        {
            float adjustedBrightness = brightness - 1.0f;

            Color32[] pixels = new Color32[source.Length];

            for (int i = 0; i < pixels.Length; i++)
            {
                var p = (Color)source[i];
                p.r = AdjustChannel(p.r, adjustedBrightness, contrast, gamma);
                p.g = AdjustChannel(p.g, adjustedBrightness, contrast, gamma);
                p.b = AdjustChannel(p.b, adjustedBrightness, contrast, gamma);
                pixels[i] = p;
            }
            return pixels;
        }
        private static float AdjustChannel(float colour,
           float brightness, float contrast, float gamma)
        {
            return Mathf.Pow(colour, gamma) * contrast + brightness;
        }

        /// <summary>
        /// Add Loaded Texture to a Terrain Tile
        /// </summary>
        /// <param name="terrainTexture"></param>
        /// <param name="terrainItem"></param>
        /// <param name="ClearLayers"></param>
        public static void AddTextureToTerrain(Texture2D terrainTexture, GISTerrainTile terrainItem, bool ClearLayers = false)
        {
            //Set This as Task for Common Editor / Runtime
            TerrainLayer NewterrainLayer = new TerrainLayer();
#if UNITY_EDITOR

            if(!Application.isPlaying)
            {
                string path = Path.Combine(terrainItem.container.TerrainFilePath, terrainItem.name + ".terrainlayer");
                AssetDatabase.CreateAsset(NewterrainLayer, path);
            }
#endif            
            TerrainLayer[] ExistingTerrainLayers = terrainItem.terrainData.terrainLayers;

            List<TerrainLayer> NewLayers = new List<TerrainLayer>();


            if (!ClearLayers)
            {
                foreach (var l in ExistingTerrainLayers)
                {
                    if (l != null)
                        NewLayers.Add(l);
                }
            }

            foreach (var l in ExistingTerrainLayers)
            {
                NewLayers.Add(l);
            }

            NewterrainLayer.diffuseTexture = terrainTexture;

            NewterrainLayer.tileSize = new Vector2(terrainItem.size.x, terrainItem.size.z);
            NewterrainLayer.tileOffset = Vector2.zero;

            NewLayers.Add(NewterrainLayer);

            terrainItem.terrainData.terrainLayers = NewLayers.ToArray();
        }

        #region SplitOperation
        public static void CombienTerrainTextures(string TerrainPath)
        {
            var Settings_SO = (GISTerrainLoaderSettings_SO)Resources.Load("Settings/GISTerrainLoaderSettings", typeof(GISTerrainLoaderSettings_SO));

            var TerrainFileName = Path.GetFileName(TerrainPath).Split('.')[0];

            var TextureFolder = Path.Combine(Path.GetDirectoryName(TerrainPath), Path.GetFileNameWithoutExtension(TerrainPath) + Settings_SO .TextureFolderName);

            Vector2Int MaxTextureSize = new Vector2Int(0, 0);

            Vector2Int TerrainsCount = GetTilesNumberInTextureFolder(TerrainPath);

            var listAccount = TerrainsCount.x * TerrainsCount.y;

            var texturesPath = GetTextureTiles(TerrainPath);

            var textures = GetTextureInFolder_Editor(TerrainPath);

            List<Vector2Int> offests = new List<Vector2Int>();
            List<Vector2Int> RealSizes = new List<Vector2Int>();

            int cx = -1;
            int cy = -1;
            for (int i = 0; i < listAccount; i = i + (int)TerrainsCount.x)
            {
                cx++;
                for (int j = i; j < i + TerrainsCount.y; j++)
                {
                    cy++;
                    var tileNumber = new Vector2Int(cx, cy);

                    var Texture = textures[j];

                    var TexturePath = "Assets" + texturesPath[j].Replace(Application.dataPath.Replace('/', '\\'), "");

                    var RealSize = GetTextureRealWidthAndHeightEditor(Texture, TexturePath);

                    if (tileNumber.x == 0)
                    {
                        MaxTextureSize.x += RealSize.x;
                    }
                    if (tileNumber.y == 0)
                    {
                        MaxTextureSize.y += RealSize.y;
                    }

                    var offest = new Vector2Int(RealSize.x * tileNumber.x, RealSize.y * tileNumber.y);
                    offests.Add(offest);

                    RealSizes.Add(RealSize);

                }
                cy = -1;
            }

            if (Directory.Exists(TextureFolder))
                Directory.Move(TextureFolder, TextureFolder + "_Original_" + UnityEngine.Random.Range(501, 1000));

            Directory.CreateDirectory(TextureFolder);

            if (!Application.isPlaying && !Application.isEditor)
            {
#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif
            }


            Texture2D Fileout = new Texture2D(MaxTextureSize.x, MaxTextureSize.y, TextureFormat.RGBA32, true);

            for (int s = 0; s < textures.Length; s++)
            {
                var tex = textures[s];
                var Rsize = RealSizes[s];
                var off = offests[s];
                var width = Rsize.x;
                var height = Rsize.y;
                RenderTexture tmp = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
                Graphics.Blit(tex, tmp);
                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = tmp;
                Texture2D newFile = new Texture2D(width, height);
                newFile.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                newFile.Apply();
                Fileout.SetPixels(off.x, off.y, width, height, newFile.GetPixels());
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(tmp);
            }
            File.WriteAllBytes(TextureFolder + "/Tile__0__0.jpg", Fileout.EncodeToJPG());
        }
        private static async void SaveToFile(string savepath, byte[] result)
        {
            using (FileStream SourceStream = File.Open(savepath, FileMode.OpenOrCreate))
            {
                SourceStream.Seek(0, SeekOrigin.End);
                await SourceStream.WriteAsync(result, 0, result.Length);
            }
        }
        public static async Task SplitTex(string TerrainPath, Vector2 SplitCount)
        {

            int TileSize_w = 0; int TileSize_h = 0;

            Vector2Int MaxTextureSize = new Vector2Int(0, 0);

            Vector2 TerrainsCount = GetTilesNumberInTextureFolder(TerrainPath);

            var listAccount = TerrainsCount.x * TerrainsCount.y;

            var texturesPath = GetTextureTiles(TerrainPath);

            var textures = GetTextureInFolder_Editor(TerrainPath);

            List<Vector2Int> offests = new List<Vector2Int>();
            List<Vector2Int> RealSizes = new List<Vector2Int>();

            int cx = -1;
            int cy = -1;


            for (int i = 0; i < listAccount; i = i + (int)TerrainsCount.x)
            {
                cx++;
                for (int j = i; j < i + TerrainsCount.y; j++)
                {
                    cy++;
                    var tileNumber = new Vector2Int(cx, cy);

                    var Texture = textures[j];

                    var TexturePath = "Assets" + texturesPath[j].Replace(Application.dataPath.Replace('/', '\\'), "");

                    var RealSize = GetTextureRealWidthAndHeightEditor(Texture, TexturePath);

                    if (tileNumber.x == 0)
                    {
                        MaxTextureSize.x += RealSize.x;
                    }
                    if (tileNumber.y == 0)
                    {
                        MaxTextureSize.y += RealSize.y;
                    }

                    var offest = new Vector2Int(RealSize.x * tileNumber.x, RealSize.y * tileNumber.y);
                    offests.Add(offest);

                    RealSizes.Add(RealSize);

                }
                cy = -1;
            }

            TileSize_w = (int)(MaxTextureSize.x / SplitCount.x);
            TileSize_h = (int)(MaxTextureSize.y / SplitCount.y);

            //Case of one texture ----> Split Directly

            if (textures.Length == 1)
            {
                await Split(TerrainPath, textures[0], MaxTextureSize, new Vector2(TileSize_w, TileSize_h), SplitCount);
            }

        }
        private static async Task Split(string SavePath, Texture2D Maintex, Vector2 mainTexSize, Vector2 tileSize, Vector2 TerrainsCount)
        {
            var Settings_SO = (GISTerrainLoaderSettings_SO)Resources.Load("Settings/GISTerrainLoaderSettings", typeof(GISTerrainLoaderSettings_SO));
 
            var TerrainFileName = Path.GetFileName(SavePath).Split('.')[0];
            var TextureFolder = Path.Combine(Path.GetDirectoryName(SavePath), Path.GetFileNameWithoutExtension(SavePath) + Settings_SO .TextureFolderName);

            if (Directory.Exists(TextureFolder))
                Directory.Move(TextureFolder, TextureFolder + "_Original_" + UnityEngine.Random.Range(0, 500));

            Directory.CreateDirectory(TextureFolder);

            if (!Application.isPlaying && !Application.isEditor)
            {
#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif
            }

            int w = (int)tileSize.x;
            int h = (int)tileSize.y;

            var pixels = Maintex.GetPixels();

            int cx = -1;
            int cy = -1;

            var listAccount = TerrainsCount.x * TerrainsCount.y;

            for (int i = 0; i < listAccount; i = i + (int)TerrainsCount.x)
            {
                cx++;
                for (int j = i; j < i + TerrainsCount.y; j++)
                {
                    cy++;

                    var tileNumber = new Vector2Int(cx, cy);

                    var offest = new Vector2Int(w * tileNumber.x, h * tileNumber.y);

                    Texture2D tmp = new Texture2D(w, h, TextureFormat.RGBA32, false);

                    for (int iw = offest.x; iw < offest.x + w; iw++)
                    {
                        for (int ih = offest.y; ih < offest.y + h; ih++)
                        {
                            var pix = Maintex.GetPixel(iw, ih);
                            tmp.SetPixel(iw, ih, pix);
                        }
                    }

                    string filePath = TextureFolder + "/Tile__" + cx + "__" + cy + ".jpg";

                    FileStream SourceStream = null;
                    var buffer = tmp.EncodeToJPG();

                    try

                    {
                        using (SourceStream = File.Open(filePath, FileMode.OpenOrCreate))
                        {
                            var t = tmp.EncodeToJPG();
                            SourceStream.Seek(0, SeekOrigin.End);

                            SourceStream.WriteAsync(t, 0, t.Length).Wait();
                        }
                    }
                    catch
                    {
                        Debug.Log("Error");
                    }
                    finally
                    {

                        if (SourceStream != null)

                            SourceStream.Dispose();

                    }
                }
                cy = -1;
            }

            await Task.Delay(1);
        }
        public static Vector2Int GetTextureRealWidthAndHeightEditor(Texture2D tex, string texturepath)
        {
            int width = 0; int height = 0;

            if (Application.isPlaying)
            {
                width = tex.width;
                height = tex.height;
            }
            else
            {

#if UNITY_EDITOR
                TextureImporter textureImporter = AssetImporter.GetAtPath(texturepath) as TextureImporter;
                System.Type type = typeof(TextureImporter);
                System.Reflection.MethodInfo method = type.GetMethod("GetWidthAndHeight", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                var args = new object[] { width, height };
                method.Invoke(textureImporter, args);
                width = (int)args[0];
                height = (int)args[1];
#endif
            }


            return new Vector2Int(width, height);
        }

        #endregion



        public static Texture2D WriteNormalMap(Texture2D source,string OutPath, float strength=0.5f, TextureFormatExt format = TextureFormatExt.PNG)
        {
            strength = Mathf.Clamp(strength, 0.0F, 1.0F);

            Texture2D normalTexture;
            float xLeft;
            float xRight;
            float yUp;
            float yDown;
            float yDelta;
            float xDelta;

            normalTexture = new Texture2D(source.width, source.height, TextureFormat.ARGB32, true);

            for (int y = 0; y < normalTexture.height; y++)
            {
                for (int x = 0; x < normalTexture.width; x++)
                {
                    xLeft = source.GetPixel(x - 1, y).grayscale * strength;
                    xRight = source.GetPixel(x + 1, y).grayscale * strength;
                    yUp = source.GetPixel(x, y - 1).grayscale * strength;
                    yDown = source.GetPixel(x, y + 1).grayscale * strength;
                    xDelta = ((xLeft - xRight) + 1) * 0.5f;
                    yDelta = ((yUp - yDown) + 1) * 0.5f;
                    normalTexture.SetPixel(x, y, new Color(xDelta, yDelta, 1.0f, yDelta));
                }
            }
            normalTexture.Apply();

            byte[] Data = new byte[0];

            if (format == TextureFormatExt.PNG)
                Data = normalTexture.EncodeToPNG();
            if (format == TextureFormatExt.JPG)
                Data = normalTexture.EncodeToJPG();
 
            System.IO.File.WriteAllBytes(OutPath, Data);

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
# endif
            return normalTexture;
        }
        public static void WriteTexture(string FilePath, Texture2D source, TextureFormatExt format = TextureFormatExt.PNG)
        {
            if (FilePath.Length == 0)
            {
                return;
            }
            byte[] Data = new byte[0];

            if(format == TextureFormatExt.PNG)
                Data = source.EncodeToPNG();
            if (format == TextureFormatExt.JPG)
                Data = source.EncodeToJPG();

            if (Data != null)
            {
                File.WriteAllBytes(FilePath, Data);
                Debug.Log("Image Created Successfully " + FilePath);
#if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
# endif
            }
        }
    }


}
