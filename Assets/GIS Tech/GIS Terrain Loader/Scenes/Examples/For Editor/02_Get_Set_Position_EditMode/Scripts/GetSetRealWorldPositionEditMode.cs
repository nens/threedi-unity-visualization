using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using GISTech.GISTerrainLoader;

// This Example show you how to use GTL to get and set a real world position in edit mode
#if UNITY_EDITOR
[CustomEditor(typeof(GetSetRealWorldPosition))]
public class GetSetRealWorldPositionEditMode : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox("Get and Set the position of a gameobject in edit mode \n We also be able to manuplate coordinates in play mode", MessageType.Info);

        GetSetRealWorldPosition editorGetSetPosition = (GetSetRealWorldPosition)target;
        
        GUILayout.BeginHorizontal();
        GUILayout.Label(new GUIContent(" Terrain Generated in : "));
        editorGetSetPosition.terrainSourceMode = (TerrainSourceMode)EditorGUILayout.EnumPopup("", editorGetSetPosition.terrainSourceMode);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label(new GUIContent(" Get Elevation Mode : "));
        editorGetSetPosition.GetElevationMode = (RealWorldElevation)EditorGUILayout.EnumPopup("", editorGetSetPosition.GetElevationMode);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label(new GUIContent(" Set Elevation Mode : "));
        editorGetSetPosition.SetElevationMode = (SetElevationMode)EditorGUILayout.EnumPopup("", editorGetSetPosition.SetElevationMode);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label(new GUIContent(" Elevation value : "));
        editorGetSetPosition.StartElevation = EditorGUILayout.FloatField(editorGetSetPosition.StartElevation, GUILayout.ExpandWidth(true));
        GUILayout.EndHorizontal();

        EditorGUILayout.HelpBox("Set a GameObject", MessageType.Info);

        editorGetSetPosition.Player = (GameObject)EditorGUILayout.ObjectField(editorGetSetPosition.Player, typeof(UnityEngine.GameObject), true, GUILayout.ExpandWidth(true));
 
        EditorGUILayout.HelpBox("Click On 'Get Terrain Data' button to load Terrain HeightMap + GeoRef-Data before get/set gameobject positon", MessageType.Info);

        if (GUILayout.Button("Get Terrain Data"))
        {
            editorGetSetPosition.Player = GameObject.Find("Player");
            editorGetSetPosition.GetTerrainData();
        }

        EditorGUILayout.HelpBox("Get the real world position of a gameobject in EditMode", MessageType.Info);

        if (GUILayout.Button("Get Position"))
        {
            if (editorGetSetPosition.Player)
            {
                editorGetSetPosition.GetPosition();
            }
        }

        EditorGUILayout.HelpBox("Fill Coordinates and Click on 'Set Position' Button to set real world position of a gameobject in EditMode", MessageType.Info);

        if (GUILayout.Button("Set Position"))
        {
            if (editorGetSetPosition.Player)
            {
                editorGetSetPosition.SetPosition();
            }
        }

        EditorGUILayout.HelpBox("Click On 'Get Terrain Data' button to load Terrain HeightMap", MessageType.Info);

        GUILayout.Label("Fill Coordinates", GUILayout.MaxWidth(100), GUILayout.ExpandWidth(false));


        GUILayout.Label("X (Lon) : ", GUILayout.MaxWidth(100), GUILayout.ExpandWidth(false));
        GUI.SetNextControlName("");
        editorGetSetPosition.Coordinates.x = EditorGUILayout.DoubleField(editorGetSetPosition.Coordinates.x, GUILayout.ExpandWidth(true));

        GUILayout.Label("Y (Lat) : ", GUILayout.MaxWidth(100), GUILayout.ExpandWidth(false));
        GUI.SetNextControlName("");
        editorGetSetPosition.Coordinates.y = EditorGUILayout.DoubleField(editorGetSetPosition.Coordinates.y, GUILayout.ExpandWidth(true));


    }
}
#endif