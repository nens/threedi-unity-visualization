/*     Unity GIS Tech 2020-2023      */

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
#if UNITY_EDITOR
    [CustomEditor(typeof(GISTerrainLoaderSO_Building))]
    public class GISTerrainLoaderSO_BuildingInfo : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var script = (GISTerrainLoaderSO_Building)target;

            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent(" Building Tag ", ""), GUILayout.MaxWidth(200));
            script.buildingTag = EditorGUILayout.TextField("", script.buildingTag);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent(" Default Height ", ""), GUILayout.MaxWidth(200));
            script.Defaultheight = EditorGUILayout.FloatField("", script.Defaultheight);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent(" Floor Count ", ""), GUILayout.MaxWidth(200));
            script.FloorCount = EditorGUILayout.IntField("", script.FloorCount);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent(" Column Count ", ""), GUILayout.MaxWidth(200));
            script.ColumnCount = EditorGUILayout.IntField("", script.ColumnCount);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent(" Top Border Ratio ", ""), GUILayout.MaxWidth(200));
            script.TopBorderRatio = EditorGUILayout.Slider(script.TopBorderRatio, 0.01f, 0.4f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent(" Basement Ratio ", ""), GUILayout.MaxWidth(200));
            script.BasementRatio = EditorGUILayout.Slider(script.BasementRatio, 0.01f, 0.5f);
            GUILayout.EndHorizontal();


            var style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.UpperCenter;
            style.fixedWidth = 130;

            GUILayout.Label("", style);
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            GUILayout.Label("Wall Texture", style);
            script.WallTexture = (Texture2D)EditorGUILayout.ObjectField(script.WallTexture, typeof(Texture2D), false, GUILayout.Width(70), GUILayout.Height(70));
            GUILayout.Label("Wall NormalMap", style);
            script.WallTextureNormalMap = (Texture2D)EditorGUILayout.ObjectField(script.WallTextureNormalMap, typeof(Texture2D), false, GUILayout.Width(70), GUILayout.Height(70));
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label("Roof Texture", style);
            script.RoofTexture = (Texture2D)EditorGUILayout.ObjectField(script.RoofTexture, typeof(Texture2D), false, GUILayout.Width(70), GUILayout.Height(70));
            GUILayout.Label("Roof NormalMap", style);
            script.RoofTextureNormalMap = (Texture2D)EditorGUILayout.ObjectField(script.RoofTextureNormalMap, typeof(Texture2D), false, GUILayout.Width(70), GUILayout.Height(70));
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label("Basement Texture", style);
            script.BasementTexture = (Texture2D)EditorGUILayout.ObjectField(script.BasementTexture, typeof(Texture2D), false, GUILayout.Width(70), GUILayout.Height(70));
            GUILayout.Label("Basement NormalMap", style);
            script.BasementTextureNormalMap = (Texture2D)EditorGUILayout.ObjectField(script.BasementTextureNormalMap, typeof(Texture2D), false, GUILayout.Width(70), GUILayout.Height(70));
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
            GUILayout.Label("", style);

            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent(" BuildingUV Texture Resolution ", ""), GUILayout.MaxWidth(200));
            script.textureResolution = (BuildingUVTextureResolution)EditorGUILayout.EnumPopup("", script.textureResolution);
            GUILayout.EndHorizontal();

            if (script.p_texture == null)
            {
                script.p_texture = new Texture2D(256, 256);
            }

            GUIStyle boxStyle = new GUIStyle();
            boxStyle.alignment = TextAnchor.MiddleCenter;
            GUILayout.Box(script.p_texture, boxStyle, GUILayout.Width(256), GUILayout.Height(256), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));


            if (GUILayout.Button("Preview BuildigUV ", GUILayout.Height(20)))
            {
                script.PreviewBuildingMap(BuildingUVTextureResolution.R_256);
            }

            if (GUILayout.Button("Save UV Buildig", GUILayout.Height(30)))
            {
                script.GenerateBuildingMap(script.textureResolution);
            }
            EditorUtility.SetDirty(script);

        }

    
    }
#endif
}