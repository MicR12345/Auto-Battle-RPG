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
        public Tilemap borderTilemap;
        public Tilemap maskTilemap;

        public ObjectiveFactory objectiveFactory;
        public UnitFactory unitFactory;
        public GameObject bulletPrefab;

        public List<BorderTile> borderTiles = new List<BorderTile>();
        public List<MapTile> placeableTiles = new List<MapTile>();
        [HideInInspector]
        public List<Objective> objectives = new List<Objective>();
        [HideInInspector]
        public List<Unit> units = new List<Unit>();

        public GameObject MapBound;
        public GameObject MapBound2;

        public TMP_Dropdown tileDropdown;
        public TMP_Dropdown objectiveDropdown;
        public TMP_Dropdown unitDropdown;
        public TMP_Dropdown factionSelectionDropdown;
        
        public bool freezeMap = false;

        public int mapSizeX = 100;
        public int mapSizeY = 100;

        public GameObject bulletStorage;
        private void Start()
        {
            CreateAllTiles();
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
                    ForcePlaceTile(mapTile, i, j);
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
                optionData.image = tile.sprite[0];
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
        void CreateAllTiles()
        {
            CreateTilesPrefabs();
            CreateBorderTilePrefabs();
        }
        void CreateTilesPrefabs()
        {
            foreach (MapTile item in placeableTiles)
            {
                item.tileObject = CreateTile(item);
            }
        }
        void CreateBorderTilePrefabs()
        {
            foreach (BorderTile border in borderTiles)
            {
                border.GenerateTileData();
            }
        }
        public void PlaceTile(MapTile tile,int x,int y)
        {
            if (tile.isMaskLayer)
            {
                maskTilemap.SetTile(new Vector3Int(x, y), tile.tileObject);
            }
            else
            {
                tilemap.SetTile(new Vector3Int(x, y), tile.tileObject);
                maskTilemap.SetTile(new Vector3Int(x, y), null);
            }
            map.SetTile(x, y, tile.passable);
            CheckNeighboursForBorders(tile, x, y);
        }
        public void ForcePlaceTile(MapTile tile, int x, int y)
        {
            if (tile.isMaskLayer)
            {
                maskTilemap.SetTile(new Vector3Int(x, y), tile.tileObject);
            }
            else
            {
                tilemap.SetTile(new Vector3Int(x, y), tile.tileObject);
                maskTilemap.SetTile(new Vector3Int(x, y), null);
            }
            map.SetTile(x, y, tile.passable);
        }
        void CheckNeighboursForBorders(MapTile tile, int x, int y)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (x+i<0 || x+i>=mapSizeX)
                    {
                        continue;
                    }
                    if (y + j < 0 || y + j >= mapSizeY)
                    {
                        continue;
                    }
                    if (i==0 && j==0)
                    {
                        continue;
                    }
                    bool borderPlaced = false;
                    foreach (Border item in tile.applicableBorders)
                    {
                        TileBase[] tilesOnMap = GetTile(x + i, y + j);
                        if (tilesOnMap[tilesOnMap.Length - 1].name == item.neighbourName)
                        {
                            borderPlaced = true;
                            BorderTile borderTile = FindBorder(item.borderName);
                            borderTilemap.SetTile(new Vector3Int(2*x+1 + i,2*y+1 + j),borderTile.tiles[(-i+1)*3+(-j+1)]);
                        }
                        
                        MapTile other = FindTile(tilesOnMap[tilesOnMap.Length-1].name);
                        foreach (Border border in other.applicableBorders)
                        {
                            if (border.neighbourName == tile.tileName)
                            {
                                borderPlaced = true;
                                BorderTile borderTile = FindBorder(item.borderName);
                                borderTilemap.SetTile(new Vector3Int(2 * x + 1 + i, 2 * y + 1 + j), borderTile.tiles[(j + 1) * 3 + (i + 1)]);
                            }
                        }

                    }
                    if (!borderPlaced)
                    {
                        borderTilemap.SetTile(new Vector3Int(2 * x + 1 + i, 2 * y + 1 + j),null);
                    }
                }
            }
        }
        public TileBase[] GetTile(int x, int y)
        {
            TileBase tileBase = maskTilemap.GetTile(new Vector3Int(x, y));
            if (tileBase != null)
            {
                TileBase[] tiles = new TileBase[2];
                tiles[0] = tilemap.GetTile(new Vector3Int(x, y));
                tiles[1] = tileBase;
                return tiles;
            }
            else 
            {
                TileBase[] tiles = new TileBase[1];
                tiles[0] = tilemap.GetTile(new Vector3Int(x, y));
                return tiles;
            }
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
        public BorderTile FindBorder(string name)
        {
            for (int i = 0; i < placeableTiles.Count; i++)
            {
                if (borderTiles[i].borderName == name)
                {
                    return borderTiles[i];
                }
            }
            return null;
        }
        public TileBase CreateTile(MapTile mapTile)
        {
            if (mapTile.sprite.Count>1)
            {
                AnimatedTile tile = ScriptableObject.CreateInstance<AnimatedTile>();
                tile.name = mapTile.tileName;
                tile.m_AnimatedSprites = mapTile.sprite.ToArray();
                tile.m_MinSpeed = 1f;
                tile.m_MaxSpeed = 1f;
                return tile;
            }
            else
            {
                Tile tile = ScriptableObject.CreateInstance<Tile>();
                tile.name = mapTile.tileName;
                tile.sprite = mapTile.sprite[0];
                return tile;
            }
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
            ReconstructBullets(gameState.bulletData);
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
        public void ReconstructBullets(SaveManager.BulletData bulletData)
        {
            foreach (DataStorage bulletDat in bulletData.bullets)
            {
                GameObject bulletObject = GameObject.Instantiate(bulletPrefab);
                Bullet bullet = bulletObject.GetComponent<Bullet>();
                bullet.RestoreFromData(bulletDat,this);
            }
        }
        public void SaveGame()
        {
            SaveManager.MapData mapData = new SaveManager.MapData(mapSizeX, mapSizeY, this,defaultTileName);
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
            List<DataStorage> bulletData = new List<DataStorage>();
            foreach (Transform transform in bulletStorage.transform)
            {
                StoresData storesData;
                if (transform.TryGetComponent<StoresData>(out storesData))
                {
                    bulletData.Add(storesData.GetData());
                }
            }
            SaveManager.BulletData bulletsData = new SaveManager.BulletData(bulletData);
            SaveManager.GameState gameState = new SaveManager.GameState(mapData,objectivesData, unitsData,bulletsData);
            SaveManager.SaveGame(ref gameState);
        }
    }


    [System.Serializable]
    public class MapTile
    {
        public string tileName;
        public bool passable;
        public bool isMaskLayer;
        [HideInInspector]
        public TileBase tileObject;
        public List<Sprite> sprite;
        public List<Border> applicableBorders;
    }
    [System.Serializable]
    public class BorderTile
    {
        public string borderName;
        //Unity inaczej tego nie potrafi rozpoznac
        public List<Sprite> tilesTL;
        public List<Sprite> tilesT;
        public List<Sprite> tilesTR;
        public List<Sprite> tilesML;
        public List<Sprite> tilesMR;
        public List<Sprite> tilesBL;
        public List<Sprite> tilesB;
        public List<Sprite> tilesBR;
        [HideInInspector]
        public TileBase[] tiles = new TileBase[9];
        public List<Sprite> GetSpritesFromOffset(int i,int j)
        {
            if (i==-1)
            {
                if (j==-1)
                {
                    return tilesBL;
                }
                else if (j == 0)
                {
                    return tilesB;
                }
                else if (j==1)
                {
                    return tilesBR;
                }
            }
            else if(i == 0)
            {
                if (j==-1)
                {
                    return tilesML;
                }
                else if (j == 1)
                {
                    return tilesMR;
                }
            }
            else if (i==1)
            {
                if (j == -1)
                {
                    return tilesTL;
                }
                else if (j == 0)
                {
                    return tilesT;
                }
                else if (j == 1)
                {
                    return tilesTR;
                }
            }
            return null;
        }
        public void GenerateTileData()
        {
            tiles = new TileBase[9];
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0) continue;
                    List<Sprite> sprites = this.GetSpritesFromOffset(i, j);
                    if (sprites.Count > 1)
                    {
                        AnimatedTile tile = ScriptableObject.CreateInstance<AnimatedTile>();
                        tile.name = borderName;
                        tile.m_AnimatedSprites = sprites.ToArray();
                        tile.m_MinSpeed = 1f;
                        tile.m_MaxSpeed = 1f;
                        tiles[(i + 1) * 3 + (j + 1)] = tile;
                    }
                    else
                    {
                        Tile tile = ScriptableObject.CreateInstance<Tile>();
                        tile.name = borderName;
                        tile.sprite = sprites[0];
                        tiles[(i + 1) * 3 + (j + 1)] = tile;
                    }
                }
            }
        }
    }
    [System.Serializable]
    public class Border
    {
        public string neighbourName;
        public string borderName;
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