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
        public GameState()
        {
            this.mapData = new MapData();
        }
        public GameState(MapData mapData)
        {
            this.mapData = mapData;
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
}
