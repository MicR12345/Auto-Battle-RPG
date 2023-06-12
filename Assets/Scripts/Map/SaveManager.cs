using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
public static class SaveManager
{
    public static void SaveGame(ref GameState gameState)
    {
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(GameState));
        TextWriter writer = new StreamWriter(Application.dataPath + "/save.MAP");
        xmlSerializer.Serialize(writer, gameState);
        writer.Close();
    }
    public static GameState LoadGame(string filename)
    {
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(GameState));
        TextReader reader = new StreamReader(Application.dataPath + "/save.MAP");
        GameState gameState = (GameState)xmlSerializer.Deserialize(reader);
        reader.Close();
        return gameState;
    }
    [System.Serializable,XmlRoot("GameState")]
    public class GameState
    {
        public MapData mapData;
        public ObjectivesData objectivesData;
        public UnitsData unitsData;
        public GameState()
        {
            this.mapData = new MapData();
            this.objectivesData = new ObjectivesData();
        }
        public GameState(MapData mapData,ObjectivesData objectivesData,UnitsData unitsData)
        {
            this.mapData = mapData;
            this.objectivesData = objectivesData;
            this.unitsData = unitsData;
        }
    }
    public class MapData
    {
        public int mapSizeX;
        public int mapSizeY;
        [XmlArray("Map"), XmlArrayItem("Tile")]
        public List<TileMap.MapSaveTileData> mapTiles;
        public MapData(int mapSizeX,int mapSizeY,ref Tilemap tilemap,string defaultTile)
        {
            this.mapSizeX = mapSizeX;
            this.mapSizeY = mapSizeY;
            mapTiles = new List<TileMap.MapSaveTileData>();
            for (int i = 0; i < mapSizeX; i++)
            {
                for (int j = 0; j < mapSizeY; j++)
                {
                    TileBase tile = tilemap.GetTile(new Vector3Int(i, j));
                    if (tile.name != defaultTile)
                    {
                        mapTiles.Add(new TileMap.MapSaveTileData(tile.name, i, j));
                    }
                }
            }
        }
        public MapData()
        {
            mapSizeX = 0;
            mapSizeY = 0;
            mapTiles = new List<TileMap.MapSaveTileData>();
        }
    }
    public class ObjectivesData
    {
        public List<DataStorage> objectives;
        public ObjectivesData()
        {
            objectives = null;
        }
        public ObjectivesData(List<DataStorage> data)
        {
            objectives = data;
        }
    }
    public class UnitsData
    {
        public List<DataStorage> units;
        public UnitsData()
        {
            units = null;
        }
        public UnitsData(List<DataStorage> data)
        {
            units = data;
        }
    }
}
[System.Serializable]
public class DataStorage
{
    public string name;
    public List<Parameter> parameters;
    public List<DataStorage> subcomponents;
    public DataStorage(string name)
    {
        this.name = name;
        parameters = new List<Parameter>();
        subcomponents = null;
    }
    public DataStorage()
    {
        name = "";
    }
    public void RegisterNewParam(string name, string value)
    {
        if (parameters==null)
        {
            parameters = new List<Parameter>();
        }
        parameters.Add(new Parameter(name, value));
    }
    public void AddSubcomponent(DataStorage subcomponent)
    {
        if (subcomponents==null)
        {
            subcomponents = new List<DataStorage>();
        }
        subcomponents.Add(subcomponent);
    }
}
[System.Serializable]
public class Parameter
{
    public string name;
    public string value;
    public Parameter(string name,string value)
    {
        this.name=name;
        this.value=value;
    }
    public Parameter()
    {
        name = "";
        value = "";
    }
}
public interface StoresData
{
    public DataStorage GetData();
}
