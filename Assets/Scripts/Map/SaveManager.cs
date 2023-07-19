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
        TextReader reader = new StreamReader(Application.dataPath + "/"+filename);
        GameState gameState = (GameState)xmlSerializer.Deserialize(reader);
        reader.Close();
        return gameState;
    }
    public static void SaveObjectiveData(ref DataStorage data,string name)
    {
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(DataStorage));
        TextWriter writer = new StreamWriter(Application.dataPath + "/Objectives/" + name + ".objective");
        xmlSerializer.Serialize(writer, data);
        writer.Close();
    }
    public static DataStorage LoadObjectiveData(string path)
    {
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(DataStorage));
        TextReader reader = new StreamReader(path);
        DataStorage gameState = (DataStorage)xmlSerializer.Deserialize(reader);
        reader.Close();
        return gameState;
    }
    [System.Serializable,XmlRoot("GameState")]
    public class GameState
    {
        public MapData mapData;
        public ObjectivesData objectivesData;
        public UnitsData unitsData;
        public BulletData bulletData;
        public FactionResourceManager.FactionResourcesWrapper factionResource;
        public GameState()
        {
            this.mapData = new MapData();
            this.objectivesData = new ObjectivesData();
            this.unitsData = new UnitsData();
            this.bulletData = new BulletData();
            this.factionResource = new FactionResourceManager.FactionResourcesWrapper();
        }
        public GameState(MapData mapData,ObjectivesData objectivesData,UnitsData unitsData,BulletData bulletData, FactionResourceManager.FactionResourcesWrapper factionResource)
        {
            this.mapData = mapData;
            this.objectivesData = objectivesData;
            this.unitsData = unitsData;
            this.bulletData = bulletData;
            this.factionResource = factionResource;
        }
    }
    public class MapData
    {
        public int mapSizeX;
        public int mapSizeY;
        [XmlArray("Map"), XmlArrayItem("Tile")]
        public List<TileMap.MapSaveTileData> mapTiles;
        public MapData(int mapSizeX,int mapSizeY,TileMap.MapController controller,string defaultTile)
        {
            this.mapSizeX = mapSizeX;
            this.mapSizeY = mapSizeY;
            mapTiles = new List<TileMap.MapSaveTileData>();
            for (int i = 0; i < mapSizeX; i++)
            {
                for (int j = 0; j < mapSizeY; j++)
                {
                    TileBase[] tiles = controller.GetTile(i,j);
                    if (tiles[0].name != defaultTile)
                    {
                        mapTiles.Add(new TileMap.MapSaveTileData(tiles[0].name, i, j));
                    }
                    if (tiles.Length > 1)
                    {
                        mapTiles.Add(new TileMap.MapSaveTileData(tiles[1].name, i, j));
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
    public class BulletData
    {
        public List<DataStorage> bullets;
        public BulletData()
        {
            bullets = null;
        }
        public BulletData(List<DataStorage> data)
        {
            bullets= data;
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
    public Parameter FindParam(string name)
    {
        foreach (Parameter param in parameters)
        {
            if (param.name==name)
            {
                return param;
            }
        }
        return null;
    }
    public void RemoveParam(Parameter parameter)
    {
        parameters.Remove(parameter);
    }
    public void EditParam(string name,string value)
    {
        foreach (Parameter param in parameters)
        {
            if (param.name == name)
            {
                param.value = value;
                return;
            }
        }
        RegisterNewParam(name, value);
    }
    public DataStorage FindSubcomp(string name)
    {
        if (subcomponents==null)
        {
            return null;
        }
        foreach (DataStorage comp in subcomponents)
        {
            if (comp.name == name)
            {
                return comp;
            }
        }
        return null;
    }
    public List<DataStorage> FindAllSubcomps(string name)
    {
        List<DataStorage> list = new List<DataStorage>();
        if (subcomponents == null)
        {
            return null;
        }
        foreach (DataStorage comp in subcomponents)
        {
            if (comp.name == name)
            {
                list.Add(comp);
            }
        }
        if (list.Count==0)
        {
            return null;
        }
        return list;
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
