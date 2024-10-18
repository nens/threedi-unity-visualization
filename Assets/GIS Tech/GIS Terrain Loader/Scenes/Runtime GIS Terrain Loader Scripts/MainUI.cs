/*     Unity GIS Tech 2020-2023      */

using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using GISTech.GISTerrainLoader;

#if FileBrowser
using SimpleFileBrowser;
#endif


public class MainUI : MonoBehaviour
{
    public Button LoadTerrain;

#if FileBrowser
    public FileBrowser fileBrowserDiag;
#endif
 
    public Text TerrainPathText;

    public Scrollbar Terrain_Exaggeration;

    public Dropdown ProjectionMode_DropDown;
    public InputField EPSGCode_Input;


    public Dropdown ElevationMode;
    public Text Terrain_Exaggeration_value;
    public Dropdown DimensionMode;
    public InputField TerrainLenght;
    public InputField TerrainWidth;

    public Dropdown UnderWaterMode;
    public Dropdown AutoFixMode;
    public InputField MinElevation;
    public InputField MaxElevation;

    public InputField TerrainScale_x;
    public InputField TerrainScale_y;
    public InputField TerrainScale_z;

    public Dropdown HeightMapResolution;

    public Dropdown TexturingMode;
    public Dropdown ShaderType;

    public Dropdown VectorType;
    public Dropdown GenerateTrees;
    public Dropdown GenerateGrass;
    public Dropdown GenerateRoads;
    public Dropdown GenerateBuildings;


    public Button GenerateTerrainBtn;

    private RuntimeTerrainGenerator RuntimeGenerator;

    private GISTerrainLoaderPrefs Prefs;

    private GISTerrainLoaderRuntimePrefs RuntimePrefs;

    private Camera3D camera3d;

    public Scrollbar GenerationProgress;

    public Text Phasename;

    public Text progressValue;


    void Start()
    {
        RuntimeTerrainGenerator.OnProgress += OnGeneratingTerrainProg;

#if FileBrowser
        FileBrowser.SetFilters(true, new FileBrowser.Filter("DEM File", ".tif", ".tiff", ".flt", ".hgt", ".bil", ".asc", ".bin", ".las", ".ter", ".png", ".raw"));
        FileBrowser.SetDefaultFilter(".tif");
        FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe");
        FileBrowser.AddQuickLink("Data Path", Application.dataPath, null);
#else
        Debug.Log("Runtime File Browser Not Installed, to install it From the main menu click on Tools/GIS Tech/GIS Terrain Loader/Install Libs/FileBrowser/Import_FileBrowser");
#endif



        if (camera3d)
        {
            camera3d = Camera.main.GetComponent<Camera3D>();

            camera3d.enabled = false;
        }



        LoadTerrain.onClick.AddListener(OnLoadBtnClicked);

        GenerateTerrainBtn.onClick.AddListener(OnGenerateTerrainbtnClicked);

        RuntimePrefs = GISTerrainLoaderRuntimePrefs.Get;

        Prefs = RuntimePrefs.Prefs;

        Prefs.LoadSettings();

        RuntimeGenerator = RuntimeTerrainGenerator.Get;

        ProjectionMode_DropDown.onValueChanged.AddListener(OnProjectionModeChanged);
        ElevationMode.onValueChanged.AddListener(OnElevationModeChanged);
        DimensionMode.onValueChanged.AddListener(OnDimensionModeChanged);
        Terrain_Exaggeration.onValueChanged.AddListener(OnTerrainExaggerationChanged);
        AutoFixMode.onValueChanged.AddListener(OnFixModeChanged);
        TexturingMode.onValueChanged.AddListener(OnTexturingModeChanged);
        ShaderType.onValueChanged.AddListener(OnShaderTypeChanged);
        VectorType.onValueChanged.AddListener(OnVectorTypeChanged);
        GenerateTrees.onValueChanged.AddListener(OnGenerateTreesChanged);
        GenerateGrass.onValueChanged.AddListener(OnGenerateGrassChanged);
        GenerateRoads.onValueChanged.AddListener(OnGenerateRoadsChanged);
        GenerateBuildings.onValueChanged.AddListener(OnGenerateBuildingsChanged);

    }


    private void OnGenerateTerrainbtnClicked()
    {
        if (camera3d)
            camera3d.enabled = false;

        RuntimeGenerator.enabled = true;

        var TerrainPath = TerrainPathText.text;



        if (!string.IsNullOrEmpty(TerrainPath) && System.IO.File.Exists(TerrainPath))
        {
            Prefs.TerrainFilePath = TerrainPath;

            if (Prefs.projectionMode == ProjectionMode.Custom_EPSG)
            {
                if (!string.IsNullOrEmpty(EPSGCode_Input.text))
                {
                    var EPSG = int.Parse(EPSGCode_Input.text);
                    Prefs.EPSGCode = EPSG;
                }
                else
                {
                    Prefs.projectionMode = ProjectionMode.Geographic;
                    Prefs.EPSGCode = 0;
                    Debug.LogError("EPSG Not Correct...");
                }
            }

            Prefs.TerrainElevation = (TerrainElevation)ElevationMode.value;
            Prefs.TerrainExaggeration = Terrain_Exaggeration.value;
            Prefs.terrainDimensionMode = (TerrainDimensionsMode)DimensionMode.value;
            Prefs.UnderWater = (OptionEnabDisab)UnderWaterMode.value;
            Prefs.TerrainFixOption = (FixOption)AutoFixMode.value;

            if (Prefs.TerrainFixOption == FixOption.ManualFix)
            {
                var min = float.Parse(MinElevation.text.Replace(".", ","));
                var max = float.Parse(MinElevation.text.Replace(".", ","));
                Prefs.TerrainMaxMinElevation = new Vector2(min, max);
            }
            var scale_x = float.Parse(TerrainScale_x.text.Replace(".", ","));
            var scale_y = float.Parse(TerrainScale_y.text.Replace(".", ","));
            var scale_z = float.Parse(TerrainScale_z.text.Replace(".", ","));
            Prefs.terrainScale = new Vector3(scale_x, scale_y, scale_z);

            Prefs.heightmapResolution_index = HeightMapResolution.value;

            Prefs.heightmapResolution = Prefs.heightmapResolutions[Prefs.heightmapResolution_index];

            Prefs.RemovePrvTerrain = OptionEnabDisab.Enable;

            if (!Prefs.Settings_SO.IsGeoFile(Path.GetExtension(TerrainPath)) || Prefs.terrainDimensionMode == TerrainDimensionsMode.Manual)
            {
                if (!string.IsNullOrEmpty(TerrainLenght.text) && !string.IsNullOrEmpty(TerrainWidth.text))
                {

                    var terrainWidth = float.Parse(TerrainLenght.text.Replace(".", ","));
                    var terrainLenght = float.Parse(TerrainWidth.text.Replace(".", ","));

                    Prefs.TerrainDimensions = new DVector2(terrainWidth, terrainLenght);
                }
                else
                {
                    Debug.LogError("Reset terrain Dimensions...");
                    return;
                }
            }

            StartCoroutine(RuntimeGenerator.StartGenerating(Prefs));
        }
        else
        {
            Debug.LogError("Terrain file null or not supported.. Try againe");
            return;
        }



    }
    private void OnTerrainExaggerationChanged(float value)
    {
        Terrain_Exaggeration_value.text = value.ToString();
    }
    private void OnProjectionModeChanged(int value)
    {
        var parent = EPSGCode_Input.transform.parent.parent.parent;

        switch (value)
        {
            case (int)ProjectionMode.Geographic:
                parent.gameObject.SetActive(false);
                Prefs.projectionMode = ProjectionMode.Geographic;
                Prefs.EPSGCode = 0;
                break;
            case (int)ProjectionMode.AutoDetection:
                parent.gameObject.SetActive(false);
                Prefs.projectionMode = ProjectionMode.AutoDetection;
                break;
            case (int)ProjectionMode.Custom_EPSG:
                parent.gameObject.SetActive(true);
                Prefs.projectionMode = ProjectionMode.Custom_EPSG;
                break;
        }
    }
    private void OnElevationModeChanged(int value)
    {

        switch (value)
        {
            case (int)TerrainElevation.RealWorldElevation:
                Terrain_Exaggeration.transform.parent.parent.gameObject.SetActive(false);
                Terrain_Exaggeration.transform.parent.parent.GetComponent<Element>().ShowElement = false;

                break;
            case (int)TerrainElevation.ExaggerationTerrain:
                Terrain_Exaggeration.transform.parent.parent.gameObject.SetActive(true);
                Terrain_Exaggeration.transform.parent.parent.GetComponent<Element>().ShowElement = true;
                break;
        }

    }
    private void OnDimensionModeChanged(int value)
    {
        var parent = TerrainLenght.transform.parent.parent.parent.parent.parent.parent;
        switch (value)
        {
            case (int)TerrainDimensionsMode.AutoDetection:
                parent.gameObject.SetActive(false);
                parent.GetComponent<Element>().ShowElement = false;

                break;
            case (int)TerrainDimensionsMode.Manual:
                parent.gameObject.SetActive(true);
                parent.GetComponent<Element>().ShowElement = true;

                break;
        }

    }
    private void OnFixModeChanged(int value)
    {
        var parent = MinElevation.transform.parent.parent.parent.parent.parent.parent;

        if (value == (int)FixOption.ManualFix)
        {
            parent.gameObject.SetActive(true);
            parent.GetComponent<Element>().ShowElement = true;
        }
        else
        {
            parent.gameObject.SetActive(false);
            parent.GetComponent<Element>().ShowElement = false;
        }



    }
    private void OnTexturingModeChanged(int value)
    {
        var parent = ShaderType.transform.parent.parent.parent;

        Prefs.textureMode = (TextureMode)value;

        if ((TextureMode)value == TextureMode.ShadedRelief)
        {
            parent.gameObject.SetActive(true);
            parent.GetComponent<Element>().ShowElement = true;
        }
        else
        {
            parent.gameObject.SetActive(false);
            parent.GetComponent<Element>().ShowElement = false;
        }
    }
    private void OnVectorTypeChanged(int value)
    {
        Prefs.vectorType = (VectorType)value;
    }
    private void OnShaderTypeChanged(int value)
    {
        Prefs.TerrainShaderType = (ShaderType)value;
    }
    private void OnGenerateTreesChanged(int value)
    {
        if (value == 0)
            Prefs.EnableTreeGeneration = OptionEnabDisab.Disable;
        else Prefs.EnableTreeGeneration = OptionEnabDisab.Enable;
    }
    private void OnGenerateGrassChanged(int value)
    {
        if (value == 0)
            Prefs.EnableGrassGeneration = OptionEnabDisab.Disable;
        else Prefs.EnableGrassGeneration = OptionEnabDisab.Enable;
    }
    private void OnGenerateRoadsChanged(int value)
    {
        if (value == 0)
            Prefs.EnableRoadGeneration = OptionEnabDisab.Disable;
        else Prefs.EnableRoadGeneration = OptionEnabDisab.Enable;
    }
    private void OnGenerateBuildingsChanged(int value)
    {
        if (value == 0)
            Prefs.EnableBuildingGeneration = OptionEnabDisab.Disable;
        else Prefs.EnableBuildingGeneration = OptionEnabDisab.Enable;
    }



    private void OnLoadBtnClicked()
    {
        StartCoroutine(ShowLoadDialogCoroutine());
    }
    IEnumerator ShowLoadDialogCoroutine()
    {
#if FileBrowser
        yield return FileBrowser.WaitForLoadDialog(false, null, "Load Terrain File", "Load");

        TerrainPathText.text = FileBrowser.Result;

#else
        yield return null;
#endif

       OnTerrainFileChanged(TerrainPathText.text);
    }
    private void OnGeneratingTerrainProg(string phase, float progress)
    {
        if (!phase.Equals("Finalization"))
        {
            GenerationProgress.transform.parent.gameObject.SetActive(true);

            Phasename.text = phase.ToString();

            GenerationProgress.value = progress / 100;

            progressValue.text = (progress).ToString() + "%";
        }
        else
        {
            if (camera3d)
                camera3d.enabled = true;
            GenerationProgress.transform.parent.gameObject.SetActive(false);
        }
    }
    private void OnTerrainFileChanged(string TerrainFilePath)
    {
        Prefs.TerrainHasDimensions = Prefs.Settings_SO.IsGeoFile(Path.GetExtension(TerrainFilePath));

        var parent = TerrainLenght.transform.parent.parent.parent.parent.parent.parent;

        if (!Prefs.TerrainHasDimensions)
        {
            parent.gameObject.SetActive(true);
            parent.gameObject.SetActive(true);
        }
        else
        {
            parent.gameObject.SetActive(false);
            parent.gameObject.SetActive(false);
        }
    }

}
 