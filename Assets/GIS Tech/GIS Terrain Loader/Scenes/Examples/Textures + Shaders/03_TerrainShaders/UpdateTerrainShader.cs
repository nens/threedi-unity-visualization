using GISTech.GISTerrainLoader;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UpdateTerrainShader : MonoBehaviour
{
    public GISTerrainContainer container;
    public Dropdown ShadersInput;
    public Slider ContourInterval;
    public Slider ContourLineWidth;
    public Text IntervalValue;
    public Text LineWidthValue;
    // Start is called before the first frame update
    void Start()
    {
        ShadersInput.onValueChanged.AddListener(OnTerrainShaderChanged);
        ContourInterval.onValueChanged.AddListener(OnTerrainContourIntervalChanged);
        ContourLineWidth.onValueChanged.AddListener(OnTerrainContourLineWidthChanged);
    }
    private void OnTerrainShaderChanged(int index)
    {
        container.UpdateTerrainMaterial((TerrainMaterialMode)index);

        if((TerrainMaterialMode)index == TerrainMaterialMode.HeightmapColorRampContourLines)
        {
            ContourInterval.gameObject.SetActive(true);
            ContourLineWidth.gameObject.SetActive(true);
        }else
        {
            ContourInterval.gameObject.SetActive(false);
            ContourLineWidth.gameObject.SetActive(false);
        }
    }
    private float ContourIntervalValue = 10;
    private float ContourLineWidthValue = 0.017f;
    private void OnTerrainContourIntervalChanged(float value)
    {
        ContourIntervalValue = value;
        container.UpdateTerrainMaterial((TerrainMaterialMode)ShadersInput.value, ContourIntervalValue);
        IntervalValue.text = Mathf.Round(ContourIntervalValue).ToString() + " [m]";
    }
    private void OnTerrainContourLineWidthChanged(float value)
    {
        ContourLineWidthValue = value;
        container.UpdateTerrainMaterial((TerrainMaterialMode)ShadersInput.value, ContourIntervalValue, ContourLineWidthValue);
        LineWidthValue.text = Mathf.Round(ContourLineWidthValue).ToString();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
