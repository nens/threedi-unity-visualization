using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderWallGenerator
    {

        GISTerrainLoaderBuildingData buildingData;
        GISTerrainLoaderSO_Building _currentFacade;
        GISTerrainContainer  container;
 
        #region ModifierOptions

        private float _scaledFirstFloorHeight = 0;

        private float _scaledTopFloorHeight = 0;

        private float _scaledPreferredWallLength;
        [SerializeField] private bool _centerSegments = true;
        [SerializeField] private bool _separateSubmesh = true;

        #endregion

        float currentWallLength = 0;
        Vector3 start = GISTerrainLoaderConstants.Vector3Zero;
        Vector3 wallDirection = GISTerrainLoaderConstants.Vector3Zero;

        Vector3 wallSegmentFirstVertex;
        Vector3 wallSegmentSecondVertex;
        Vector3 wallSegmentDirection;
        float wallSegmentLength;

        private Rect _currentTextureRect;

        private float finalFirstHeight;
        private float finalTopHeight;
        private float finalMidHeight;
        private float finalLeftOverRowHeight;
        private float _scaledFloorHeight;
        private int triIndex;
        private Vector3 wallNormal;
        private List<int> wallTriangles;
        private float columnScaleRatio;
        private float rightOfEdgeUv;

        private float currentY1;
        private float currentY2;
        private float _wallSizeEpsilon = 0.99f;
        private float _narrowWallWidthDelta = 0.01f;
        private float _shortRowHeightDelta = 0.015f;

        private int _counter = 0;
        public float height = 0.0f;
        private float _scale = 1f;
        private float _minWallLength;
        private float _singleFloorHeight;
        private float _currentMidHeight;
        private float _midUvInCurrentStep;
        private float _singleColumnLength;
        private float _leftOverColumnLength;

        private float Customheight = 0;
        public GISTerrainLoaderWallGenerator(GISTerrainLoaderBuildingData m_buildingData, GISTerrainContainer m_container, GISTerrainLoaderVectorEntity m_vectorEntity,float m_Customheight = 0)
        {
            buildingData = m_buildingData;
            this.container = m_container;
            Customheight = m_Customheight;
        }

        public void Run(GISTerrainLoaderMeshData md, GISTerrainContainer tile = null)
        {
            if (md.Vertices.Count == 0 || md == null)
                return;
 
            if (tile != null)
                _scale = tile.Scale.y;
            _scale = 1;
            _currentFacade = buildingData.building_SO;
            _currentTextureRect = _currentFacade.TextureRect;

            var TerrainScale = container.Scale.y;

            _singleFloorHeight = (TerrainScale * _currentFacade.FloorHeight) / _currentFacade.FloorCount;
            _scaledFirstFloorHeight = TerrainScale * _currentFacade.FirstFloorHeight;
            _scaledTopFloorHeight = TerrainScale * _currentFacade.TopFloorHeight;
            _scaledPreferredWallLength = TerrainScale * _currentFacade.WallLength;
            _scaledFloorHeight = _scaledPreferredWallLength * _currentFacade.WallToFloorRatio;
            _singleColumnLength = _scaledPreferredWallLength / _currentFacade.ColumnCount;


            float maxHeight = buildingData.MaxHeight;
            float minHeight = buildingData.MinHeight;

            maxHeight = maxHeight * _scale ;
            minHeight = minHeight * _scale;

            height = (maxHeight - minHeight);

            GenerateRoofMesh(md, maxHeight);
 
            finalFirstHeight = Mathf.Min(height, _scaledFirstFloorHeight);

            if (Customheight != 0)
                finalFirstHeight = Customheight * -1;

            finalTopHeight = (height - finalFirstHeight) < _scaledTopFloorHeight ? 0 : _scaledTopFloorHeight;
            finalMidHeight = Mathf.Max(0, height - (finalFirstHeight + finalTopHeight));

            wallTriangles = new List<int>();


            currentWallLength = 0;
            start = GISTerrainLoaderConstants.Vector3Zero;
            wallSegmentDirection = GISTerrainLoaderConstants.Vector3Zero;

            finalLeftOverRowHeight = 0f;

            if (finalMidHeight > 0)
            {
                finalLeftOverRowHeight = finalMidHeight;
                finalLeftOverRowHeight = finalLeftOverRowHeight % _singleFloorHeight;
                finalMidHeight -= finalLeftOverRowHeight;
            }
            else
            {
                finalLeftOverRowHeight = finalTopHeight;
            }
            for (int i = 0; i < md.Edges.Count; i += 2)
            {
                var v1 = md.Vertices[md.Edges[i]];
                var v2 = md.Vertices[md.Edges[i + 1]];

                wallDirection = v2 - v1;

                currentWallLength = Vector3.Distance(v1, v2);
                _leftOverColumnLength = currentWallLength % _singleColumnLength;
                start = v1;
                wallSegmentDirection = (v2 - v1).normalized;


                if (_centerSegments && currentWallLength > _singleColumnLength)
                {
                    wallSegmentFirstVertex = start;
                    wallSegmentLength = (_leftOverColumnLength / 2);
                    start += wallSegmentDirection * wallSegmentLength;
                    wallSegmentSecondVertex = start;

                    _leftOverColumnLength = _leftOverColumnLength / 2;
                    CreateWall(md);
                }


                while (currentWallLength > _singleColumnLength)
                {
                    wallSegmentFirstVertex = start;
                    var stepRatio = (float)Math.Min(_currentFacade.ColumnCount,
                            Math.Floor(currentWallLength / _singleColumnLength)) / _currentFacade.ColumnCount;
                    wallSegmentLength = stepRatio * _scaledPreferredWallLength;
                    start += wallSegmentDirection * wallSegmentLength;
                    wallSegmentSecondVertex = start;

                    currentWallLength -= (stepRatio * _scaledPreferredWallLength);
                    CreateWall(md);
                }

                if (_leftOverColumnLength > 0)
                {
                    wallSegmentFirstVertex = start;
                    wallSegmentSecondVertex = v2;
                    wallSegmentLength = _leftOverColumnLength;
                    CreateWall(md);
                }
            }

            if (_separateSubmesh)
            {
                md.Triangles.Add(wallTriangles);
            }
            else
            {
                md.Triangles.Capacity = md.Triangles.Count + wallTriangles.Count;
                md.Triangles[0].AddRange(wallTriangles);
            }
        }

        private void CreateWall(GISTerrainLoaderMeshData md)
        {
            triIndex = md.Vertices.Count;

            columnScaleRatio = Math.Min(1, wallSegmentLength / _scaledPreferredWallLength);
            rightOfEdgeUv =
                _currentTextureRect.xMin +
                _currentTextureRect.size.x *
                columnScaleRatio;

            _minWallLength = (_scaledPreferredWallLength / _currentFacade.ColumnCount) * _wallSizeEpsilon;

            wallNormal = new Vector3(-(wallSegmentFirstVertex.z - wallSegmentSecondVertex.z), 0,
                (wallSegmentFirstVertex.x - wallSegmentSecondVertex.x)).normalized;

   
            currentY1 = wallSegmentFirstVertex.y ;
            currentY2 = wallSegmentSecondVertex.y;

            LeftOverRow(md, finalLeftOverRowHeight);

            FirstFloor(md, height);
            TopFloor(md, finalLeftOverRowHeight );
            MidFloors(md);
        }

        private void LeftOverRow(GISTerrainLoaderMeshData md, float leftOver)
        {
            if (leftOver > 0)
            {
                md.Vertices.Add(new Vector3(wallSegmentFirstVertex.x, currentY1, wallSegmentFirstVertex.z));
                md.Vertices.Add(new Vector3(wallSegmentSecondVertex.x, currentY2, wallSegmentSecondVertex.z));

                currentY1 -= leftOver;
                currentY2 -= leftOver;

                md.Vertices.Add(new Vector3(wallSegmentFirstVertex.x, currentY1, wallSegmentFirstVertex.z));
                md.Vertices.Add(new Vector3(wallSegmentSecondVertex.x, currentY2, wallSegmentSecondVertex.z));

                if (wallSegmentLength >= _minWallLength)
                {
                    md.UV[0].Add(new Vector2(_currentTextureRect.xMin, _currentTextureRect.yMax));
                    md.UV[0].Add(new Vector2(rightOfEdgeUv, _currentTextureRect.yMax));
                    md.UV[0].Add(new Vector2(_currentTextureRect.xMin,
                        _currentTextureRect.yMax - _shortRowHeightDelta));
                    md.UV[0].Add(new Vector2(rightOfEdgeUv, _currentTextureRect.yMax - _shortRowHeightDelta));
                }
                else
                {
                    md.UV[0].Add(new Vector2(_currentTextureRect.xMin, _currentTextureRect.yMax));
                    md.UV[0].Add(
                        new Vector2(_currentTextureRect.xMin + _narrowWallWidthDelta, _currentTextureRect.yMax));
                    md.UV[0].Add(new Vector2(_currentTextureRect.xMin,
                        _currentTextureRect.yMax - _shortRowHeightDelta));
                    md.UV[0].Add(new Vector2(_currentTextureRect.xMin + _narrowWallWidthDelta,
                        _currentTextureRect.yMax - _shortRowHeightDelta));
                }

                md.Normals.Add(wallNormal);
                md.Normals.Add(wallNormal);
                md.Normals.Add(wallNormal);
                md.Normals.Add(wallNormal);

                md.Tangents.Add(wallDirection);
                md.Tangents.Add(wallDirection);
                md.Tangents.Add(wallDirection);
                md.Tangents.Add(wallDirection);

                wallTriangles.Add(triIndex);
                wallTriangles.Add(triIndex + 1);
                wallTriangles.Add(triIndex + 2);

                wallTriangles.Add(triIndex + 1);
                wallTriangles.Add(triIndex + 3);
                wallTriangles.Add(triIndex + 2);

                triIndex += 4;
            }
        }

        private void MidFloors(GISTerrainLoaderMeshData md)
        {
            _currentMidHeight = finalMidHeight;
            while (_currentMidHeight >= _singleFloorHeight - 0.01f)
            {
                _midUvInCurrentStep =
                    ((float)Math.Min(_currentFacade.FloorCount,
                        Math.Round(_currentMidHeight / _singleFloorHeight))) / _currentFacade.FloorCount;

                md.Vertices.Add(new Vector3(wallSegmentFirstVertex.x, currentY1, wallSegmentFirstVertex.z));
                md.Vertices.Add(new Vector3(wallSegmentSecondVertex.x, currentY2, wallSegmentSecondVertex.z));

                currentY1 -= (_scaledFloorHeight * _midUvInCurrentStep);
                currentY2 -= (_scaledFloorHeight * _midUvInCurrentStep);

                md.Vertices.Add(new Vector3(wallSegmentFirstVertex.x, currentY1, wallSegmentFirstVertex.z));
                md.Vertices.Add(new Vector3(wallSegmentSecondVertex.x, currentY2, wallSegmentSecondVertex.z));

                if (wallSegmentLength >= _minWallLength)
                {
                    md.UV[0].Add(new Vector2(_currentTextureRect.xMin, _currentFacade.topOfMidUv));
                    md.UV[0].Add(new Vector2(rightOfEdgeUv, _currentFacade.topOfMidUv));
                    md.UV[0].Add(new Vector2(_currentTextureRect.xMin,
                        _currentFacade.topOfMidUv - _currentFacade.midUvHeight * _midUvInCurrentStep));
                    md.UV[0].Add(new Vector2(rightOfEdgeUv,
                        _currentFacade.topOfMidUv - _currentFacade.midUvHeight * _midUvInCurrentStep));
                }
                else
                {
                    md.UV[0].Add(new Vector2(_currentTextureRect.xMin, _currentFacade.topOfMidUv));
                    md.UV[0].Add(new Vector2(_currentTextureRect.xMin + _narrowWallWidthDelta,
                        _currentFacade.topOfMidUv));
                    md.UV[0].Add(new Vector2(_currentTextureRect.xMin,
                        _currentFacade.topOfMidUv - _currentFacade.midUvHeight * _midUvInCurrentStep));
                    md.UV[0].Add(new Vector2(_currentTextureRect.xMin + _narrowWallWidthDelta,
                        _currentFacade.topOfMidUv - _currentFacade.midUvHeight * _midUvInCurrentStep));
                }

                md.Normals.Add(wallNormal);
                md.Normals.Add(wallNormal);
                md.Normals.Add(wallNormal);
                md.Normals.Add(wallNormal);

                md.Tangents.Add(wallDirection);
                md.Tangents.Add(wallDirection);
                md.Tangents.Add(wallDirection);
                md.Tangents.Add(wallDirection);

                wallTriangles.Add(triIndex);
                wallTriangles.Add(triIndex + 1);
                wallTriangles.Add(triIndex + 2);

                wallTriangles.Add(triIndex + 1);
                wallTriangles.Add(triIndex + 3);
                wallTriangles.Add(triIndex + 2);

                triIndex += 4;
                _currentMidHeight -= Math.Max(0.1f, (_scaledFloorHeight * _midUvInCurrentStep));
            }
        }

        private void TopFloor(GISTerrainLoaderMeshData md, float leftOver)
        {

            currentY1 -= finalTopHeight;
            currentY2 -= finalTopHeight;
            md.Vertices.Add(new Vector3(wallSegmentFirstVertex.x, wallSegmentFirstVertex.y - leftOver,
                wallSegmentFirstVertex.z));
            md.Vertices.Add(new Vector3(wallSegmentSecondVertex.x, wallSegmentSecondVertex.y - leftOver,
                wallSegmentSecondVertex.z));
            md.Vertices.Add(new Vector3(wallSegmentFirstVertex.x, wallSegmentFirstVertex.y - leftOver - finalTopHeight,
                wallSegmentFirstVertex.z));
            md.Vertices.Add(new Vector3(wallSegmentSecondVertex.x,
                wallSegmentSecondVertex.y - leftOver - finalTopHeight, wallSegmentSecondVertex.z));

            if (wallSegmentLength >= _minWallLength)
            {
                md.UV[0].Add(new Vector2(_currentTextureRect.xMin, _currentTextureRect.yMax));
                md.UV[0].Add(new Vector2(rightOfEdgeUv, _currentTextureRect.yMax));
                md.UV[0].Add(new Vector2(_currentTextureRect.xMin, _currentFacade.bottomOfTopUv));
                md.UV[0].Add(new Vector2(rightOfEdgeUv, _currentFacade.bottomOfTopUv));
            }
            else
            {
                md.UV[0].Add(new Vector2(_currentTextureRect.xMin, _currentTextureRect.yMax));
                md.UV[0].Add(new Vector2(_currentTextureRect.xMin + _narrowWallWidthDelta, _currentTextureRect.yMax));
                md.UV[0].Add(new Vector2(_currentTextureRect.xMin, _currentFacade.bottomOfTopUv));
                md.UV[0].Add(
                    new Vector2(_currentTextureRect.xMin + _narrowWallWidthDelta, _currentFacade.bottomOfTopUv));
            }

            md.Normals.Add(wallNormal);
            md.Normals.Add(wallNormal);
            md.Normals.Add(wallNormal);
            md.Normals.Add(wallNormal);


            md.Tangents.Add(wallDirection);
            md.Tangents.Add(wallDirection);
            md.Tangents.Add(wallDirection);
            md.Tangents.Add(wallDirection);

            wallTriangles.Add(triIndex);
            wallTriangles.Add(triIndex + 1);
            wallTriangles.Add(triIndex + 2);

            wallTriangles.Add(triIndex + 1);
            wallTriangles.Add(triIndex + 3);
            wallTriangles.Add(triIndex + 2);

            triIndex += 4;
        }

        private void FirstFloor(GISTerrainLoaderMeshData md, float hf)
        {
            md.Vertices.Add(new Vector3(wallSegmentFirstVertex.x, wallSegmentFirstVertex.y - hf + finalFirstHeight,
                wallSegmentFirstVertex.z));
            md.Vertices.Add(new Vector3(wallSegmentSecondVertex.x, wallSegmentSecondVertex.y - hf + finalFirstHeight,
                wallSegmentSecondVertex.z));
            md.Vertices.Add(new Vector3(wallSegmentFirstVertex.x, wallSegmentFirstVertex.y - hf,
                wallSegmentFirstVertex.z));
            md.Vertices.Add(new Vector3(wallSegmentSecondVertex.x, wallSegmentSecondVertex.y - hf,
                wallSegmentSecondVertex.z));

            md.Normals.Add(wallNormal);
            md.Normals.Add(wallNormal);
            md.Normals.Add(wallNormal);
            md.Normals.Add(wallNormal);
            md.Tangents.Add(wallDirection);
            md.Tangents.Add(wallDirection);
            md.Tangents.Add(wallDirection);
            md.Tangents.Add(wallDirection);

            if (wallSegmentLength >= _minWallLength)
            {
                md.UV[0].Add(new Vector2(_currentTextureRect.xMin, _currentFacade.topOfBottomUv));
                md.UV[0].Add(new Vector2(rightOfEdgeUv, _currentFacade.topOfBottomUv));
                md.UV[0].Add(new Vector2(_currentTextureRect.xMin, _currentTextureRect.yMin));
                md.UV[0].Add(new Vector2(rightOfEdgeUv, _currentTextureRect.yMin));
            }
            else
            {
                md.UV[0].Add(new Vector2(_currentTextureRect.xMin, _currentFacade.topOfBottomUv));
                md.UV[0].Add(
                    new Vector2(_currentTextureRect.xMin + _narrowWallWidthDelta, _currentFacade.topOfBottomUv));
                md.UV[0].Add(new Vector2(_currentTextureRect.xMin, _currentTextureRect.yMin));
                md.UV[0].Add(new Vector2(_currentTextureRect.xMin + _narrowWallWidthDelta, _currentTextureRect.yMin));
            }

            wallTriangles.Add(triIndex);
            wallTriangles.Add(triIndex + 1);
            wallTriangles.Add(triIndex + 2);

            wallTriangles.Add(triIndex + 1);
            wallTriangles.Add(triIndex + 3);
            wallTriangles.Add(triIndex + 2);

            triIndex += 4;
        }

        private void GenerateRoofMesh(GISTerrainLoaderMeshData md, float maxHeight)
        {
            _counter = md.Vertices.Count;
            for (int i = 0; i < _counter; i++)
            {
                md.Vertices[i] = new Vector3(md.Vertices[i].x, md.Vertices[i].y + maxHeight,
                    md.Vertices[i].z);
            }
        }

    }
 
}