using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
using System.Xml;
using System.Xml.Serialization;
using System.Globalization;

namespace TileMap
{
    public class MapController : MonoBehaviour
    {
        public const string defaultTileName = "grass";

        public PathfindMap.Map map;
        public Tilemap tilemap;
        public Tilemap impassableTilemap;

        public ObjectiveFactory objectiveFactory;
        public UnitFactory unitFactory;

        public List<MapTile> placeableTiles = new List<MapTile>();
        public List<Objective> objectives = new List<Objective>();
        public List<Unit> units = new List<Unit>();

        public GameObject MapBound;
        public GameObject MapBound2;

        public TMP_Dropdown tileDropdown;
        public TMP_Dropdown objectiveDropdown;
        public TMP_Dropdown unitDropdown;

        public bool freezeMap = false;

        public int mapSizeX = 300;
        public int mapSizeY = 300;

        private void Start()
        {
            CreateTilesPrefabs();
            LoadGame();
            //CreateEmptyMapWithSize(mapSizeX, mapSizeY);
            FillMapEditorOptions();
        }
        void CreateEmptyMapWithSize(int x,int y)
        {
            map = new PathfindMap.Map(x, y);
            MapTile mapTile = FindTile(defaultTileName);
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    PlaceTile(mapTile, i, j);
                }
            }
            MapBound2.transform.position = new Vector3(x, y);
        }
        void FillMapEditorOptions()
        {
            FillMapEditorTileOptions();
            FillMapEditorObjectiveOptions();
            FillMapEditorUnitOptions();
        }
        void FillMapEditorTileOptions()
        {
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            foreach (MapTile tile in placeableTiles)
            {
                TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
                optionData.text = tile.tileName;
                optionData.image = tile.sprite;
                options.Add(optionData);
            }
            tileDropdown.AddOptions(options);
        }
        void FillMapEditorObjectiveOptions()
        {
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            foreach (ObjectiveType objectiveType in objectiveFactory.objectiveTypes)
            {
                TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
                optionData.text = objectiveType.type;
                options.Add(optionData );
            }
            objectiveDropdown.AddOptions(options);
        }
        void FillMapEditorUnitOptions()
        {
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            foreach (UnitType unitType in unitFactory.unitTypes)
            {
                TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
                optionData.text = unitType.type;
                options.Add(optionData);
            }
            unitDropdown.AddOptions(options);
        }
        void CreateTilesPrefabs()
        {
            foreach (MapTile item in placeableTiles)
            {
                item.tileObject = CreateTile(item);
            }
        }
        public void PlaceTile(MapTile tile,int x,int y)
        {
            tilemap.SetTile(new Vector3Int(x, y), tile.tileObject);
            map.SetTile(x, y, tile.passable);
        }
        public MapTile FindTile(string name)
        {
            for (int i = 0; i < placeableTiles.Count; i++)
            {
                if (placeableTiles[i].tileName == name)
                {
                    return placeableTiles[i];
                }
            }
            return null;
        }
        public Tile CreateTile(MapTile mapTile)
        {
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.name = mapTile.tileName;
            tile.sprite = mapTile.sprite;
            return tile;
        }
        public (int,int) GetMapTileFromWorldPosition(Vector3 vector3)
        {
            int x = Mathf.FloorToInt(vector3.x);
            int y = Mathf.FloorToInt(vector3.y);
            return (x, y);
        }
        public void RegisterObjective(Objective objective)
        {
            objectives.Add(objective);
        }
        public void UnregisterObjective(Objective objective)
        {
            objectives.Remove(objective);
        }
        public void RegisterUnit(Unit unit)
        {
            units.Add(unit);
        }
        public void UnregisterUnit(Unit unit)
        {
            units.Remove(unit);
        }
        public List<Unit> GetUnitsInAreaOfFaction(Vector3 start,Vector3 endPoint,string faction)
        {
            List<Unit> areaUnits = new List<Unit>();

            float startX = Mathf.Min(start.x, endPoint.x);
            float startY = Mathf.Min(start.y, endPoint.y);
            float endX = Mathf.Max(start.x, endPoint.x);
            float endY = Mathf.Max(start.y, endPoint.y);

            startX = Mathf.FloorToInt(startX);
            startY = Mathf.FloorToInt(startY);
            endX = Mathf.CeilToInt(endX);
            endY = Mathf.CeilToInt(endY);

            for (int i = 0; i < units.Count; i++)
            {
                Vector3 position = units[i].gameObject.transform.position;
                if (startX <= position.x && position.x <= endX && startY <= position.y && position.y <= endY)
                {
                    if (units[i].Faction == faction)
                    {
                        areaUnits.Add(units[i]);
                    }
                }
            }
            return areaUnits;
        }
        public void LoadGame()
        {
            SaveManager.GameState gameState = SaveManager.LoadGame("");
            ReconstructTiles(gameState.mapData);
            ReconstructObjectives(gameState.objectivesData);
            ReconstructUnits(gameState.unitsData);
        }
        public void ReconstructTiles(SaveManager.MapData mapData)
        {
            mapSizeX = mapData.mapSizeX;
            mapSizeY = mapData.mapSizeY;
            CreateEmptyMapWithSize(mapData.mapSizeX, mapData.mapSizeY);
            foreach (MapSaveTileData tileData in mapData.mapTiles)
            {
                MapTile mapTile = FindTile(tileData.tileName);
                if (mapTile!=null)
                {
                    PlaceTile(mapTile, tileData.x, tileData.y);
                }
                else
                {
                    Debug.LogError("Saved tile not found: " + tileData.tileName);
                }
            }
        }
        public void ReconstructObjectives(SaveManager.ObjectivesData objectivesData)
        {
            foreach (DataStorage objective in objectivesData.objectives)
            {
                Placeable objectiv = objectiveFactory.ReconstructObjectiveFromData(objective);
                objectiv.Place(new Vector3(
                    float.Parse(objective.FindParam("x").value,CultureInfo.InvariantCulture.NumberFormat),
                    float.Parse(objective.FindParam("y").value, CultureInfo.InvariantCulture.NumberFormat)
                    ) + new Vector3(.5f, .5f)
                    );
            }
        }
        public void ReconstructUnits(SaveManager.UnitsData unitsData)
        {
            foreach (DataStorage unit in unitsData.units)
            {
                Placeable unitt = unitFactory.ReconstructUnitFromData(unit);
                unitt.Place(new Vector3(
                    float.Parse(unit.FindParam("x").value, CultureInfo.InvariantCulture.NumberFormat),
                    float.Parse(unit.FindParam("y").value, CultureInfo.InvariantCulture.NumberFormat)
                    ));
            }
        }
        public void SaveGame()
        {
            SaveManager.MapData mapData = new SaveManager.MapData(mapSizeX, mapSizeY, ref tilemap,defaultTileName);
            List<DataStorage> objectiveData = new List<DataStorage>();
            foreach (StoresData objective in objectives)
            {
                objectiveData.Add(objective.GetData());
            }
            SaveManager.ObjectivesData objectivesData = new SaveManager.ObjectivesData(objectiveData);
            List<DataStorage> unitData = new List<DataStorage>();
            foreach (StoresData unit in units)
            {
                unitData.Add(unit.GetData());
            }
            SaveManager.UnitsData unitsData = new SaveManager.UnitsData(unitData);
            SaveManager.GameState gameState = new SaveManager.GameState(mapData,objectivesData, unitsData);
            SaveManager.SaveGame(ref gameState);
        }
    }


    [System.Serializable]
    public class MapTile
    {
        public string tileName;
        public bool passable;
        [HideInInspector]
        public Tile tileObject;
        public Sprite sprite;
    }
    [System.Serializable]
    public class MapSaveTileData
    {
        public string tileName;
        public int x;
        public int y;
        public MapSaveTileData(string name,int x ,int y)
        {
            tileName = name;
            this.x = x;
            this.y = y;
        }
        public MapSaveTileData()
        {
            tileName = MapController.defaultTileName;
            x = 0;
            y = 0;
        }
    }
}