/*     Unity GIS Tech 2020-2023      */
using System.ComponentModel;

namespace GISTech.GISTerrainLoader
{
    public enum DisplayFormat
    {
        Decimale = 0,
        DegMinSec,
        UTM,
        MGRS
    }
    
    /// <summary>
    /// Get Terrain Geo-Ref from Terrain Generated in edit mode or play mode
    /// </summary>
    public enum TerrainSourceMode
    {
        FromEditMode = 0,
        FromPlayMode
    }
    /// <summary>
    /// Elevation : Real world elevation on a position
    /// Height : Height from the ground
    /// Altitude : Real world elevation + Height
    /// </summary>
    public enum RealWorldElevation
    {
        Elevation = 0, 
        Height, 
        Altitude,
    }
    /// <summary>
    /// OnTheGround: Relative to the ground 
    /// RelativeToTheGround : Relative to the ground 
    /// RelativeToSeaLevel : Relative to the sea level
    /// </summary>
    public enum SetElevationMode
    {
        OnTheGround = 0,
        RelativeToTheGround, 
        RelativeToSeaLevel
    }
    public enum ReadingMode
    {
        Full = 0,
        SubRegion
    }
    public enum ProjectionMode
    {
        Geographic = 0,
        AutoDetection = 1,
        Custom_EPSG
    }
    public enum TerrainElevation
    {
        ExaggerationTerrain = 1,
        RealWorldElevation = 0,
    }

     public enum TerrainDimensionsMode
    {
        AutoDetection = 0,
        Manual
    }
    public enum TerrainMaterialMode
    {
        Standard = 0,
        HeightmapColorRamp,
        HeightmapColorRampContourLines,
        ElevationGrayScaleGradient,
        Custom,
#if GISVirtualTexture
GISVirtualTexture
#endif
    }
    public enum OptionEnabDisab
    {
        Disable = 0,
        Enable
    }
    public enum FixOption
    {
        Disable=0,
        AutoFix ,
        ManualFix
    }
    public enum EmptyPoints
    {
        Average = 0,
        Manual,
    }
    public enum TerrainSide
    {
        Non,
        [Description("-1__0")]
        Left,
        [Description("1__0")]
        Right,
        [Description("0__1")]
        Top,
        [Description("0__-1")]
        Bottom,
        [Description("1__1")]
        TopRight,
        [Description("-1__1")]
        TopLeft,
        [Description("1__-1")]
        BottomRight,
        [Description("-1__-1")]
        BottomLeft,
    }
    public enum TextureMode
    {
        WithoutTexture,
        WithTexture,
        MultiTexture,
        MultiLayers,
        ShadedRelief,
        Splatmapping
    }
    public enum ShaderType {
        ColorRamp=0,
        ElevationGrayScale,
        ElevationInversGrayScale,
        Slop,
        SlopInvers,
        NormalMap,
        Foam,
        Water
    };

    public enum TexturesLoadingMode
    {
        AutoDetection = 1,
        Manual = 0
    }
    public enum ShadedTextureType
    {
        Foam,
        Water
    };
    public enum ShaderColor
    {
        GradientColor,
        MainGradient,
        NegativeGradient,
        BlackToWhite,
        GreyToWhite,
        GreyToBlack,
    };
 
    public enum GeneratorState
    {
        idle,
        Generating
    }

    public enum ElevationState
    {
        Wait,
        Loading,
        Loaded,
        Error
    }
    public enum TextureState
    {
        Wait,
        Loading,
        Loaded,
        Error
    }
    public enum RoadGeneratorType
    {
        Line,
        RoadCreatorPro,
        EasyRoad3D,
    }

    public enum VectorType
    {
        AllVectorFiles = 0,
        OpenStreetMap, 
        ShapeFile,
        GPX,
        KML, 
        Geojson  
    }
    public enum VectorObjectType
    {
        Point = 0,
        Line = 1,
        Polygon = 2
    }
 
    public enum BuildingUVTextureResolution
    {
        R_256 = 256,
        R_512 = 512,
        R_1024 = 1024,
        R_2048 = 2048,
    }
    public enum TiffElevationSource
    {
        DEM = 0,
        GrayScale,
        BandsData
    }
    public enum TextureFormatExt
    {
        PNG = 0,
        JPG
    }
    public enum RawDepth
    {
        Bit8 = 1,
        Bit16
    }

    public enum RawByteOrder
    {
        Mac = 1,
        Windows
    }

    public enum ExportDEMType
    {
        Raw = 1,
        Png,
        Tiff
    }

    public enum CoordinatesSource
    {
        FromTerrain = 1,
        FromGeoDataScript = 2,
    }
    public enum ExportAs
    {
        Png = 1,
        jpg,
    }
    public enum TiffElevation
    {
        Auto = 1,
        Custom
    }
    public enum ExportVectorType
    {
        Shapfile = 1,
    }
    public enum VectorDataLineSource
    {
        LineRenderer = 0,
        Custom
    }
    public enum VectorDataPolySource
    {
        MeshFilter = 0,
        Custom
    }
    public enum GeoVectorType
    {
        Point = 0, 
        Line, 
        Polygon, 
        AllTypes
    }
    public enum WaterSource { DefaultPlane, VectorData };
    public enum PdalRader { LAS, Raster };
    public enum Unit { Degree, Grad, Radian, Meter };
    public enum PointDistribution { Randomly = 1,uniformly};
    public enum VectorElevationMode { Default2DPlane, AdaptedToTerrainElevation };

}