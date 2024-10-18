// This Example show you how to Get a Real World Position of a gameobject at real time from a terrain generated/Edited in Unity editmode.

Steps : 
01 : From GTL Pro Editor Windows Enable Serialze Heightmap To Store Terrain Data into "GIS Tech\GIS Terrain Loader\Resources\HeightmapData" Folder.
02 : Generate your terrain in edit mode.
03 : Inside Start void Call "container.GetStoredHeightmap();" to Read Terrain Data Stored in Edit Mode
04 : You Can now use any API for Geo-refenecing your terrain terrain