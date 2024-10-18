/*     Unity GIS Tech 2020-2023      */

using System;
using System.IO;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderDEMPngLoader
    {
        public static event ReaderEvents OnReadError;

        public static event TerrainProgression OnProgress;

        public GISTerrainLoaderFileData data;

        public bool LoadComplet;

        public byte[] WebData = new byte[0];

        private GISTerrainLoaderPrefs Prefs;

        public GISTerrainLoaderDEMPngLoader(GISTerrainLoaderPrefs prefs)
        {
            data = new GISTerrainLoaderFileData();
            Prefs = prefs;
        }
        public void LoadFile(string filepath)
        {
            try
            {
                LoadComplet = false;

                data = new GISTerrainLoaderFileData();

                if (WebData.Length > 0)
                {
                    data.floatheightData = LoadPNG(filepath, WebData);
                }
                else
                {
                    data.floatheightData = LoadPNG(filepath);
                }

                LoadComplet = true;

                WebData = new byte[0];
            }
            catch(Exception ex)
            {
                LoadComplet = true;

                if (OnReadError != null)
                    OnReadError("Error while loading PNG file " + ex);
                return;
            }

        }
 
        private float[,] LoadPNG(string filename, byte[] FileData =null)
        {
            Texture2D heightmap = new Texture2D(0, 0);

            if (FileData!=null)
                heightmap = LoadedTextureTile(FileData);
            else
                heightmap = LoadedTextureTile(filename);

            int w = heightmap.width;
            int h = heightmap.height;

            data.mapSize_col_x = w;

            data.mapSize_row_y = h;

            var floatdata = new float[w, h];

            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    var x1 = 1.0f / w * x * w;
                    var y1 = 1.0f / h * y * h;

                    var pixel = heightmap.GetPixel((int)x1, (int)y1);

                    var el = pixel.grayscale; 

                    floatdata[x, y] = el;

                    if (el < data.MinMaxElevation.x)
                        data.MinMaxElevation.x = el;

                    if (el > data.MinMaxElevation.y)
                        data.MinMaxElevation.y = el;
                }


                var prog = (y * 100 / h);

                if (prog <= 99)
                {
                    if (OnProgress != null)
                        OnProgress("Loading File ", prog);
                }
            }

            return floatdata;
        }
 
        Texture2D LoadedTextureTile(byte[] FileData)
        {
            Texture2D tex = new Texture2D(2, 2);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.LoadImage(FileData);
            tex.LoadImage(tex.EncodeToPNG());
            return tex;
        }
        Texture2D LoadedTextureTile(string TexturePath)
        {
            Texture2D tex = new Texture2D(2, 2);

            tex.wrapMode = TextureWrapMode.Clamp;
            tex.LoadImage(File.ReadAllBytes(TexturePath));
            tex.LoadImage(tex.EncodeToPNG());

            return tex;
        }
    }

}