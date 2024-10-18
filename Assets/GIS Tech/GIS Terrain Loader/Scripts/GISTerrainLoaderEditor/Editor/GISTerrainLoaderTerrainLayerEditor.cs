using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    [CustomPropertyDrawer(typeof(GISTerrainLoaderTerrainLayer))]
    public class GISTerrainLoaderTerrainLayerEditor : Editor
    {
       public SerializedProperty diffuseProperty;
        SerializedProperty normalMapProperty;
        SerializedProperty x_height;
        SerializedProperty y_height;
        SerializedProperty textureSizeProperty;
        SerializedProperty Show_height;

        private void CacheFields()
        {
            diffuseProperty = serializedObject.FindProperty("Diffuse");
            normalMapProperty = serializedObject.FindProperty("NormalMap");
            x_height = serializedObject.FindProperty("X_Height");
            y_height = serializedObject.FindProperty("Y_Height");
            textureSizeProperty = serializedObject.FindProperty("ShowHeight");
            Show_height = serializedObject.FindProperty("TextureSize");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox("To replace GameObject drag prefab here!!!", MessageType.Info);

            EditorGUILayout.PropertyField(diffuseProperty);
            EditorGUILayout.PropertyField(normalMapProperty);
            EditorGUILayout.PropertyField(x_height);
            EditorGUILayout.PropertyField(y_height);
            EditorGUILayout.PropertyField(textureSizeProperty);
            EditorGUILayout.PropertyField(Show_height);

            serializedObject.ApplyModifiedProperties();
        }
        private static GUIStyle s_TempStyle = new GUIStyle();
 
    }
}
 