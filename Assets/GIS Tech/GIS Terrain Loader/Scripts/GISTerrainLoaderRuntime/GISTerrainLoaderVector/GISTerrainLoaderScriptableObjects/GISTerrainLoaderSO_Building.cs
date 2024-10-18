/*     Unity GIS Tech 2020-2023      */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
	public class GISTerrainLoaderSO_Building : ScriptableObject
    {
		[HideInInspector]
		public string buildingTag;
		//Custom Building
		[HideInInspector]
		public int FloorCount = 4;
		[HideInInspector]
		public int ColumnCount = 2;
		[HideInInspector]
		public float TopBorderRatio = 0.05f;
		[HideInInspector]
		public float BasementRatio = 0.3f;
		[HideInInspector]
        public BuildingUVTextureResolution textureResolution = BuildingUVTextureResolution.R_1024;

        [HideInInspector]
        public Texture2D m_texture;
		[HideInInspector]
		public Texture2D p_texture;

		[HideInInspector]
		public Texture2D WallTexture;
		[HideInInspector]
		public Texture2D WallTextureNormalMap;
		[HideInInspector]
		public Texture2D RoofTexture;
		[HideInInspector]
		public Texture2D RoofTextureNormalMap;
		[HideInInspector]
		public Texture2D BasementTexture;
		[HideInInspector]
		public Texture2D BasementTextureNormalMap;

		[HideInInspector]
		public float Defaultheight = 3;
		[HideInInspector]
		public Rect TextureRect =  new Rect(new Vector2(0, 0), new Vector2(1, 1));

		public void LoadDefaultValues()
        {
			Defaultheight = 3;
			FloorCount = 2;
			ColumnCount = 4;
			TopBorderRatio = 0.05f;
			BasementRatio = 0.3f;

			WallTexture = Resources.Load(GISTerrainLoaderConstants.DefaultWallTexture, typeof(Texture2D)) as Texture2D;
			RoofTexture = Resources.Load(GISTerrainLoaderConstants.DefaultRoofTexture, typeof(Texture2D)) as Texture2D;
		}
		public void PreviewBuildingMap(BuildingUVTextureResolution m_textureResolution)
		{
			GenerateTexture(m_textureResolution);
			p_texture = m_texture;
			p_texture.Apply();
		}

		public void GenerateBuildingMap(BuildingUVTextureResolution m_textureResolution)
		{
			GenerateTexture(m_textureResolution);


			string Dir = Application.dataPath + GISTerrainLoaderConstants.MainGISFolder + GISTerrainLoaderConstants.UVBuildingTextureFolder;

			if (!Directory.Exists(Dir))
				Directory.CreateDirectory(Dir);

			string FilePath = Path.Combine(Dir, "UV_" + buildingTag + ".png");

			GISTerrainLoaderTextureGenerator.WriteTexture(FilePath,m_texture,TextureFormatExt.PNG);

			//string NormalMapFilePath = Path.Combine(Dir, "UVNormalMap_" + buildingTag + ".png");

			//GISTerrainLoaderTextureGenerator.WriteNormalMap(m_texture, NormalMapFilePath);
		}

		[HideInInspector]
		public float  WallLength = 1;
		[HideInInspector]
		public float FloorHeight;
		[HideInInspector]
		public float FirstFloorHeight;
		[HideInInspector]
		public float TopFloorHeight;

		[HideInInspector]
		public float bottomOfTopUv;
		[HideInInspector]
		public float topOfMidUv;
		[HideInInspector]
		public float topOfBottomUv;
		[HideInInspector]
		public float midUvHeight;
		[HideInInspector]
		public float WallToFloorRatio;


		private int _drawCount;
		private const float _cellRatioMargin = 0.01f;

		public void CalculateParameters()
        {
			_drawCount = 0;
			TextureRect = new Rect(new Vector2(0, 0), new Vector2(1, 1));
			WallLength = 45;
			

			FirstFloorHeight = (WallLength * ((TextureRect.height * BasementRatio) / TextureRect.width));
			TopFloorHeight = (WallLength * ((TextureRect.height * TopBorderRatio)  / TextureRect.width)) ;
			FloorHeight = (WallLength * ((1 - TopBorderRatio - BasementRatio)  * (TextureRect.height / TextureRect.width))) ;

			bottomOfTopUv = (TextureRect.yMax - (TextureRect.size.y * TopBorderRatio)) ;
			topOfMidUv =(TextureRect.yMax - (TextureRect.height * TopBorderRatio)) ;
			topOfBottomUv = (TextureRect.yMin + (TextureRect.size.y * BasementRatio )) ;
			midUvHeight = (TextureRect.height * (1 - TopBorderRatio - BasementRatio )) ;
			WallToFloorRatio = ((1 - TopBorderRatio - BasementRatio)  * (TextureRect.height / TextureRect.width)) ;
  
		}
		public void GenerateTexture(BuildingUVTextureResolution m_textureResolution)
        {

			m_texture = new Texture2D((int)m_textureResolution, (int)m_textureResolution);
			Rect baseRect = TextureRect;

			float topRatio = (TopBorderRatio * baseRect.height) ;
			float bottomRatio = (BasementRatio * baseRect.height) ;
			float middleRatio = (baseRect.height - (topRatio + bottomRatio));

			Rect groundFloorRect = new Rect(baseRect.x, baseRect.y, baseRect.width, bottomRatio);
			Rect topFloorRect = new Rect(baseRect.x, baseRect.y + baseRect.height - topRatio, baseRect.width, topRatio);

			PixelRect basePixelRect = ConvertUVRectToPixel(m_textureResolution,baseRect);
			PixelRect groundFloorPixelRect = ConvertUVRectToPixel(m_textureResolution, groundFloorRect);
			PixelRect topFloorPixelRect = ConvertUVRectToPixel(m_textureResolution, topFloorRect);

			Color color = Color.gray;
			Color colorLight = (color + Color.white) / 2;
			Color colorDark = (color + Color.black) / 2;

			DrawRect(basePixelRect, color);
			DrawRect(groundFloorPixelRect, colorLight);
			DrawRect(topFloorPixelRect, colorDark);

			DrawDebugCross(groundFloorPixelRect);
			DrawDebugCross(topFloorPixelRect);

			int numColumns = ColumnCount;
			int numMidFloors = FloorCount;

			float colWidth = baseRect.width / numColumns;
			float floorHeight = middleRatio / numMidFloors;

			float midFloorBase = baseRect.y + bottomRatio;

			float mrgn = _cellRatioMargin;
			float halfMrgn = mrgn / 2;

			for (int j = 0; j < numMidFloors; j++)
			{
				float floorStart = midFloorBase + (floorHeight * j);

				for (int k = 0; k < numColumns; k++)
				{
					float columnStart = baseRect.x + (colWidth * k);

					Rect cellRect = new Rect(columnStart + halfMrgn, floorStart + halfMrgn, colWidth - mrgn, floorHeight - mrgn);
					PixelRect cellPixelRect = ConvertUVRectToPixel(m_textureResolution,cellRect);

					DrawRect(cellPixelRect, Color.white);
					DrawDebugCross(cellPixelRect);
				}
			}

			DrawCornerWatermarks(groundFloorPixelRect);
			DrawCornerWatermarks(topFloorPixelRect);
			_drawCount++;

		}
		private PixelRect ConvertUVRectToPixel(BuildingUVTextureResolution m_textureResolution,Rect atlasRect)
		{
			PixelRect pixelRect = new PixelRect();
			pixelRect.x = GetPixelCoorRatio(m_textureResolution, atlasRect.x);
			pixelRect.y = GetPixelCoorRatio(m_textureResolution, atlasRect.y);
			pixelRect.xx = GetPixelCoorRatio(m_textureResolution, atlasRect.x + atlasRect.width);
			pixelRect.yy = GetPixelCoorRatio(m_textureResolution, atlasRect.y + atlasRect.height);
			return pixelRect;
		}
		private int GetPixelCoorRatio(BuildingUVTextureResolution m_textureResolution,float ratio)
		{
			return (int)((int)m_textureResolution * ratio);
		}
		private void DrawRect(PixelRect pr, Color color)
		{
			for (int i = pr.x; i < pr.xx; i++)
			{
				for (int j = pr.y; j < pr.yy; j++)
				{
					m_texture.SetPixel(i, j, color);
				}
			}
		}
		private void DrawDebugCross(PixelRect pr)
		{
			int centerX = (pr.x + pr.xx) / 2;
			int centerY = (pr.y + pr.yy) / 2;

			m_texture.SetPixel(centerX, centerY, Color.black);

			for (int x = pr.x; x < pr.xx; x++)
			{
				m_texture.SetPixel(x, centerY, Color.black);
				m_texture.SetPixel(x, centerY - 1, Color.black);
				m_texture.SetPixel(x, centerY + 1, Color.black);
			}

			for (int y = pr.y; y < pr.yy; y++)
			{
				m_texture.SetPixel(centerX, y, Color.black);
				m_texture.SetPixel(centerX - 1, y, Color.black);
				m_texture.SetPixel(centerX + 1, y, Color.black);
			}
		}
		private void DrawWatermark(int x, int y)
		{
			m_texture.SetPixel(x, y, Color.black);
			m_texture.SetPixel(x + 3, y, Color.black);
			m_texture.SetPixel(x, y + 3, Color.black);
			m_texture.SetPixel(x + 3, y + 3, Color.black);

			m_texture.SetPixel(x + 1, y + 1, Color.black);
			m_texture.SetPixel(x + 2, y + 1, Color.black);
			m_texture.SetPixel(x + 1, y + 2, Color.black);
			m_texture.SetPixel(x + 2, y + 2, Color.black);
		}
		private void DrawCornerWatermarks(PixelRect pr)
		{
			DrawWatermark(pr.x, pr.y);
			DrawWatermark(pr.xx - 4, pr.y);
			DrawWatermark(pr.x, pr.yy - 4);
			DrawWatermark(pr.xx - 4, pr.yy - 4);
		}


	}
	public class PixelRect
	{
		public int x;
		public int y;

		public int xx;
		public int yy;
	}
}