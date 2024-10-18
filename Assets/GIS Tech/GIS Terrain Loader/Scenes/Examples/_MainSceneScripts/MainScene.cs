using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

public class MainScene : MonoBehaviour
{
    public void LoadScene(int index)
    {
        SceneManager.LoadScene(index);
    }
 
    public void AddAndLoadScene(string scenename)
    {
       
#if UNITY_EDITOR
        List<UnityEditor.EditorBuildSettingsScene> editorBuildSettingsScenes = new List<UnityEditor.EditorBuildSettingsScene>();
        List<string> SceneList = new List<string>();
        string MainFolder = "Assets/GIS Tech/GIS Terrain Loader/Scenes/Examples/";

        SceneList.Add(MainFolder + "/MainScene.unity");
        SceneList.Add(MainFolder + "/DEM +Terrains + Projection/01_Airplane Demo/AirPlaneDemo.unity");
        SceneList.Add(MainFolder + "/DEM +Terrains + Projection/02_Distance Calculation/DistanceCal.unity");
        SceneList.Add(MainFolder + "/DEM +Terrains + Projection/03_MultiBands_Tiff/MultiBandsLoader.unity");
        SceneList.Add(MainFolder + "/DEM +Terrains + Projection/04_CoordinateSystem Demo/CoordinateSystem.unity");
        SceneList.Add(MainFolder + "/DEM +Terrains + Projection/05_RuntimeProjectionSystem/RuntimeProjectionSystem.unity");
        SceneList.Add(MainFolder + "/DEM +Terrains + Projection/06_SetTerrainPosition/SnapTerrain.unity");
        SceneList.Add(MainFolder + "/DEM +Terrains + Projection/07_Airplane With Terrain Background/Terrain background.unity");


        SceneList.Add(MainFolder + "/For Editor/01_GeoRef_LoadDataFromEditMode/Editmode.unity");
        SceneList.Add(MainFolder + "/For Editor/02_Get_Set_Position_EditMode/GetSetPosition.unity");


        SceneList.Add(MainFolder + "/Textures + Shaders/01_Procedural Terrain Splatmap/Procedural Terrain Splatmap.unity");
        SceneList.Add(MainFolder + "/Textures + Shaders/02_LoadTextures/LoadTextures.unity");
        SceneList.Add(MainFolder + "/Textures + Shaders/03_TerrainShaders/TerrainShaders.unity");
        SceneList.Add(MainFolder + "/Textures + Shaders/04_SelectTextureLayer/SelectTextureLayer.unity");
        SceneList.Add(MainFolder + "/Textures + Shaders/05_BlendMultiTextures/BlendMultiLayers.unity");



        SceneList.Add(MainFolder + "/Vector Data/01_Generate_3D_Objects_From_Vector/Generate_3D_Objects.unity");
        SceneList.Add(MainFolder + "/Vector Data/02_GPX_Example/GPX_Example.unity");
        SceneList.Add(MainFolder + "/Vector Data/03_UpdateVectorData/UpdateVectorLoader.unity");
        SceneList.Add(MainFolder + "/Vector Data/04_ParseShapeFileData/LoadShapeFileData.unity");
        SceneList.Add(MainFolder + "/Vector Data/05_ParseOSMFile/ParseOSMFile.unity");
        SceneList.Add(MainFolder + "/Vector Data/06_ExportToVectorData/ExportToVectordata.unity");

 
        for (int i = 0; i < SceneList.Count; i++)
            editorBuildSettingsScenes.Add(new UnityEditor.EditorBuildSettingsScene(SceneList[i], true));

        UnityEditor.EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();

        Debug.Log("Demo Scene Added to Build Settings Scenes");
#endif
    }

}
 
#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(MainScene))]
public class customButton : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MainScene myScript = (MainScene)target;
        if (GUILayout.Button("Add Scenes to Build"))
        {
            myScript.AddAndLoadScene("");
        }
    }

}
#endif