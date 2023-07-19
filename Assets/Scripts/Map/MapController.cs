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
        [Header("Map stuff")]
        public const string defaultTileName = "grass";
        

        public PathfindMap.Map map;
        public Tilemap tilemap;
        public Tilemap borderTilemap;
        public Tilemap maskTilemap;

        [Header("Factories")]
        public ObjectiveFactory objectiveFactory;
        public UnitFactory unitFactory;
        public GameObject bulletPrefab;
        [Header("AI")]
        public AIController aiController;
        [Header("Tiles")]
        public List<BorderTile> borderTiles = new List<BorderTile>();
        public List<MapTile> placeableTiles = new List<MapTile>();
        [HideInInspector]
        public List<Objective> objectives = new List<Objective>();
        [HideInInspector]
        public List<Unit> units = new List<Unit>();
        [Header("Boundries")]
        public GameObject MapBound;
        public GameObject MapBound2;
        [Header("Map editor stuff")]
        public TMP_Dropdown tileDropdown;
        public TMP_Dropdown objectiveDropdown;
        public TMP_Dropdown unitDropdown;
        public TMP_Dropdown factionSelectionDropdown;
        [Header("Map properties")]
        public bool freezeMap = false;

        public int mapSizeX = 100;
        public int mapSizeY = 100;

        public bool loadMap = true;
        public string mapPath = "";
        public bool mapEditorMode = true;

        public GameObject bulletStorage;

        List<DataStorage> objectivePrefabs;
        [Header("Game Status")]
        public GameStatusManager gameStatusManager;
        public FactionResourceManager factionResourceManager;
        private void Start()
        {
            mapEditorMode = true;
            CreateAllTiles();
            LoadObjectives();
            if (loadMap)
            {
                LoadGame();
            }
            else
            {
                CreateEmptyMapWithSize(mapSizeX, mapSizeY);
            }
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
        public void LoadObjectives()
        {
            string[] files = System.IO.Directory.GetFiles(Application.dataPath + "/Objectives/", "*.objective");
            objectivePrefabs = new List<DataStorage>();
            foreach (string file in files)
            {
                objectivePrefabs.Add(SaveManager.LoadObjectiveData(file));
            }
            FillMapEditorObjectiveOptions();
        }
        public DataStorage FindObjectivePrefab(string name)
        {
            foreach (DataStorage objective in objectivePrefabs)
            {
                if (objective.FindParam("name").value==name)
                {
                    return objective;
                }
            }
            return null;
        }
        void FillMapEditorOptions()
        {
            FillMapEditorTileOptions();
            
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
            foreach (DataStorage objectiveData in objectivePrefabs)
            {
                TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
                optionData.text = objectiveData.FindParam("name").value;
                options.Add(optionData );
            }
            objectiveDropdown.ClearOptions();
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
            bool[,] placeBorders = new bool[3,3];
            foreach (Border border in tile.applicableBorders)
            {
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (i == 0 && j == 0) continue;
                        if (x+i>=0 && x+i <mapSizeX && y+j>=0 && y+j<mapSizeY)
                        {
                            TileBase[] tiles = GetTile(x + i, y + j);
                            if (tiles[tiles.Length-1].name==border.neighbourName)
                            {
                                string borderVariant = GetBorderVariantFromPosition(x, y, i, j, border.neighbourName);
                                BorderTile borderTile = FindBorder(border.borderName);
                                borderTilemap.SetTile(new Vector3Int(2 * x + 1 + i, 2 * y + 1 + j), borderTile.GetTileVariant(borderVariant));
                                placeBorders[i + 1, j + 1] = true;
                            }
                            if(tiles[tiles.Length - 1].name == tile.tileName)
                            {
                                string borderVariant = GetBorderVariantSameTile(x, y, i, j, border.neighbourName);
                                if (borderVariant!="None")
                                {
                                    BorderTile borderTile = FindBorder(border.borderName);
                                    borderTilemap.SetTile(new Vector3Int(2 * x + 1 + i, 2 * y + 1 + j), borderTile.GetTileVariant(borderVariant));
                                    placeBorders[i + 1, j + 1] = true;
                                }
                            }
                        }
                    }
                }
            }
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (!placeBorders[i+1,j+1])
                    {
                        borderTilemap.SetTile(new Vector3Int(2 * x + 1 + i, 2 * y + 1 + j), null);
                    }
                }
            }
        }
        string GetBorderVariantSameTile(int x, int y, int i, int j, string neighbourName)
        {
            string tileName = GetTileName(x + i, y + j);
            if (i<0)
            {
                if (j<0)
                {
                    string tileAbove = GetTileName(x + i, y + j + 1);
                    string tileRight = GetTileName(x + i + 1, y + j);
                    if (tileAbove==tileName && tileRight==tileName)
                    {
                        return "None";
                    }
                    else if (tileAbove==tileName)
                    {
                        if (tileRight == neighbourName)
                        {
                            return "CBR";
                        }
                        else return "None";
                    }
                    else if (tileRight == tileName)
                    {
                        if (tileAbove == neighbourName)
                        {
                            return "CTL";
                        }
                        else return "None";
                    }
                    else
                    {
                        return "PL";
                    }
                }
                else if (j > 0)
                {
                    string tileBelow = GetTileName(x + i, y + j - 1);
                    string tileRight = GetTileName(x + i + 1, y + j);
                    if (tileBelow == tileName && tileRight == tileName)
                    {
                        return "None";
                    }
                    else if (tileBelow == tileName)
                    {
                        if (tileRight == neighbourName)
                        {
                            return "CTR";
                        }
                        else return "None";
                    }
                    else if (tileRight == tileName)
                    {
                        if (tileBelow == neighbourName)
                        {
                            return "CBL";
                        }
                        else return "None";
                    }
                    else
                    {
                        return "PR";
                    }
                }
            }
            else if (i > 0)
            {
                if (j < 0)
                {
                    string tileAbove = GetTileName(x + i, y + j + 1);
                    string tileLeft = GetTileName(x + i - 1, y + j);
                    if (tileAbove == tileName && tileLeft == tileName)
                    {
                        return "None";
                    }
                    else if (tileAbove == tileName)
                    {
                        if (tileLeft == neighbourName)
                        {
                            return "CBL";
                        }
                        else return "None";
                    }
                    else if (tileLeft == tileName)
                    {
                        if (tileAbove == neighbourName)
                        {
                            return "CTR";
                        }
                        else return "None";
                    }
                    else
                    {
                        return "PR";
                    }
                }
                else if (j > 0)
                {
                    string tileBelow = GetTileName(x + i, y + j - 1);
                    string tileLeft = GetTileName(x + i - 1, y + j);
                    if (tileBelow == tileName && tileLeft == tileName)
                    {
                        return "None";
                    }
                    else if (tileBelow == tileName)
                    {
                        if (tileLeft == neighbourName)
                        {
                            return "CTL";
                        }
                        else return "None";
                    }
                    else if (tileLeft == tileName)
                    {
                        if (tileBelow == neighbourName)
                        {
                            return "CBR";
                        }
                        else return "None";
                    }
                    else
                    {
                        return "PL";
                    }
                }
            }
            return "None";
        }
        string GetBorderVariantFromPosition(int x,int y,int i,int j,string neighbourName)
        {
            if (i<0)
            {
                if (j<0)
                {
                    bool tileAbove = false, tileRight = false;
                    if (GetTileName(x + i, y + j + 1) == neighbourName) tileAbove = true;
                    if (GetTileName(x + i + 1, y + j) == neighbourName) tileRight = true;
                    if (tileAbove && tileRight)
                    {
                        return "BL";
                    }
                    else if(tileAbove)
                    {
                        return "ML";
                    }
                    else if (tileRight)
                    {
                        return "B";
                    }
                    else
                    {
                        return "CBL";
                    }
                }
                else if (j==0)
                {
                    return "ML";
                }
                else if (j > 0)
                {
                    bool tileBelow = false, tileRight = false;
                    if (GetTileName(x + i, y + j - 1) == neighbourName) tileBelow = true;
                    if (GetTileName(x + i + 1, y + j) == neighbourName) tileRight = true;
                    if (tileBelow && tileRight)
                    {
                        return "TL";
                    }
                    else if (tileBelow)
                    {
                        return "ML";
                    }
                    else if (tileRight)
                    {
                        return "T";
                    }
                    else
                    {
                        return "CTL";
                    }
                }
            }
            else if (i == 0)
            {
                if (j < 0)
                {
                    return "B";
                }
                else
                {
                    return "T";
                }
            }
            else if (i>0)
            {
                if (j < 0)
                {
                    bool tileAbove = false, tileLeft = false;
                    if (GetTileName(x + i, y + j + 1) == neighbourName) tileAbove = true;
                    if (GetTileName(x + i - 1, y + j) == neighbourName) tileLeft = true;
                    if (tileAbove && tileLeft)
                    {
                        return "BR";
                    }
                    else if (tileAbove)
                    {
                        return "MR";
                    }
                    else if (tileLeft)
                    {
                        return "B";
                    }
                    else
                    {
                        return "CBR";
                    }
                }
                else if (j == 0) return "MR";
                else if (j>0)
                {
                    bool tileBelow = false, tileLeft = false;
                    if (GetTileName(x + i, y + j - 1) == neighbourName) tileBelow = true;
                    if (GetTileName(x + i - 1, y + j) == neighbourName) tileLeft = true;
                    if (tileBelow && tileLeft)
                    {
                        return "TR";
                    }
                    else if (tileBelow)
                    {
                        return "MR";
                    }
                    else if (tileLeft)
                    {
                        return "T";
                    }
                    else
                    {
                        return "CTR";
                    }
                }
            }
            return "unknown";
        }
        public string GetTileName(int x,int y)
        {
            TileBase[] tile = GetTile(x, y);
            string name = tile[tile.Length - 1].name;
            return name;
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
            SaveManager.GameState gameState = SaveManager.LoadGame(mapPath);
            ReconstructTiles(gameState.mapData);
            ReconstructObjectives(gameState.objectivesData);
            ReconstructUnits(gameState.unitsData);
            ReconstructBullets(gameState.bulletData);
            RestoreFactionData(gameState);
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
        public void RestoreFactionData(SaveManager.GameState gameState)
        {
            foreach (FactionResourceManager.FactionResourcesWrapper.Faction faction in gameState.factionResource.factions)
            {
                foreach (FactionResourceManager.Resource resource in faction.resources)
                {
                    factionResourceManager.resourcesWrapper.AddFactionResource(faction.name, resource.name, resource.value);
                }
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
            SaveManager.GameState gameState = new SaveManager.GameState(mapData,objectivesData, unitsData,bulletsData,factionResourceManager.resourcesWrapper);
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
        public List<BorderVariant> borderVariants = new List<BorderVariant>();
        public TileBase GetTileVariant(string variant)
        {
            foreach (BorderVariant borderVariant in borderVariants)
            {
                if (borderVariant.variant==variant)
                {
                    return borderVariant.tile;
                }
            }
            return null;
        }
        public void GenerateTileData()
        {
            foreach (BorderVariant variant in borderVariants)
            {
                variant.CreateTile(this);
            }
        }
    }
    [System.Serializable]
    public class BorderVariant
    {
        public string variant;
        public List<Sprite> textures;
        [HideInInspector]
        public TileBase tile;
        public void CreateTile(BorderTile borderTile)
        {
            if (textures.Count > 1)
            {
                AnimatedTile tile = ScriptableObject.CreateInstance<AnimatedTile>();
                tile.name = borderTile.borderName + "_" + variant;
                tile.m_AnimatedSprites = textures.ToArray();
                tile.m_MinSpeed = 1f;
                tile.m_MaxSpeed = 1f;
                this.tile = tile;
            }
            else
            {
                Tile tile = ScriptableObject.CreateInstance<Tile>();
                tile.name = borderTile.borderName + "_" + variant;
                tile.sprite = textures[0];
                this.tile = tile;
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