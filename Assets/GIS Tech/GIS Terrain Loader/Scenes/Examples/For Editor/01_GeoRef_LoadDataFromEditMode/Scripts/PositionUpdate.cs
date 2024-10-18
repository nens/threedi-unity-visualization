using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GISTech.GISTerrainLoader;
using System;


public class PositionUpdate : MonoBehaviour
{
    public RealWorldElevation ElevationMode = RealWorldElevation.Altitude;
    public GISTerrainContainer container;
    public GISTerrainLoaderGeoPoint point;
    public WayPoints GeoWaypoints;
    public AirplaneDemo Airplane;
 
    void Start()
    {
        //Load Terrain Data Which is serialized by GTL Editor Generator (Data Located in GIS Tech\GIS Terrain Loader\Resources\HeightmapData)
        container.GetStoredHeightmap();

        //Convert a RealWorld Points List to UnitySpacePosition
        GeoWaypoints.ConvertLatLonToSpacePosition(container, false);
        if (GeoWaypoints.UnityWorldSpacePoints.Count > 0)
            Airplane.TargetPoint = GeoWaypoints.UnityWorldSpacePoints[0];
    }
    void Update()
    {
        //Update UI Text 
        var Pos = GISTerrainLoaderGeoConversion.UnityWorldSpaceToRealWorldCoordinates(Airplane.transform.position, container, true, ElevationMode);
        point.Unite_TextMesh.text ="NAD83/UTM Zone 10N" + "\n"+Pos.x +"\n"+Pos.y+"\n"+ Math.Round(Pos.z,2)+" m";
    }



}
