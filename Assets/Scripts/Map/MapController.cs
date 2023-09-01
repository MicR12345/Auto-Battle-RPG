using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Globalization;

namespace TileMap
{
    public class MapController : MonoBehaviour
    {
        [Header("Map stuff")]
        public const string defaultTileName = "Grass";
        

        public PathfindMap.Map map;
        public Tilemap tilemap;
        public Tilemap borderTilemap;
        public Tilemap maskTilemap;

        [Header("Factories")]
        public ObjectiveFactory objectiveFactory;
        public UnitFactory unitFactory;
        public TurretFactory turretFactory;
        public GameObject bulletPrefab;
        [Header("AI")]
        public AIController aiController;
        [Header("Tiles")]
        public List<MapTile> placeableTiles = new List<MapTile>();
        public List<TileSpriteSheet> tileSpriteSheets = new List<TileSpriteSheet>();
        [HideInInspector]
        public List<Objective> objectives = new List<Objective>();
        [HideInInspector]
        public List<Unit> units = new List<Unit>();
        [HideInInspector]
        public List<Turret> turrets = new List<Turret>();
        [Header("Boundries")]
        public GameObject MapBound;
        public GameObject MapBound2;
        [Header("Map editor stuff")]
        public TMP_Dropdown tileDropdown;
        public TMP_Dropdown objectiveDropdown;
        public TMP_Dropdown unitDropdown;
        public TMP_Dropdown turretDropdown;
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
            CreateSheetTiles();
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
            FillMapEditorTurretOptions();
        }
        void FillMapEditorTileOptions()
        {
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            /*foreach (MapTile tile in placeableTiles)
            {
                TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
                optionData.text = tile.tileName;
                optionData.image = tile.sprite[0];
                options.Add(optionData);
            }*/
            foreach (TileSpriteSheet tileSpriteSheet in tileSpriteSheets)
            {
                TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
                optionData.text = tileSpriteSheet.sheetData.name;
                optionData.image = tileSpriteSheet.mapTiles[0].sprite[0];
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
        void FillMapEditorTurretOptions()
        {
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            foreach (TurretType turretType in turretFactory.turretTypes)
            {
                TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
                optionData.text = turretType.type;
                options.Add(optionData);
            }
            turretDropdown.AddOptions(options);
        }
        void CreateSheetTiles()
        {
            foreach (TileSpriteSheet sheet in tileSpriteSheets)
            {
                sheet.CreateMapTiles();
            }
        }
        void CreateAllTiles()
        {
            CreateTilesPrefabs();
        }
        void CreateTilesPrefabs()
        {
            foreach (MapTile item in placeableTiles)
            {
                item.CreateTile();
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
        public void RegisterTurret(Turret turret)
        {
            turrets.Add(turret);
        }
        public void UnregisterTurret(Turret turret)
        {
            turrets.Remove(turret);
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
            ReconstructTurrets(gameState.turretsData);
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
        public void ReconstructTurrets(SaveManager.TurretsData turretsData)
        {
            foreach (DataStorage turret in turretsData.turrets)
            {
                Placeable turretss = turretFactory.ReconstructTurretFromData(turret);
                turretss.Place(new Vector3(
                    float.Parse(turret.FindParam("x").value, CultureInfo.InvariantCulture.NumberFormat),
                    float.Parse(turret.FindParam("y").value, CultureInfo.InvariantCulture.NumberFormat)
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
            List<DataStorage> turretData = new List<DataStorage>();
            foreach (StoresData turret in turrets)
            {
                turretData.Add(turret.GetData());
            }
            SaveManager.TurretsData turretsData = new SaveManager.TurretsData(turretData);
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
            SaveManager.GameState gameState = new SaveManager.GameState(mapData,objectivesData, unitsData,turretsData,bulletsData,factionResourceManager.resourcesWrapper);
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
        public void CreateTile()
        {
            if (sprite.Count > 1)
            {
                AnimatedTile tile = ScriptableObject.CreateInstance<AnimatedTile>();
                tile.name = tileName;
                tile.m_AnimatedSprites = sprite.ToArray();
                tile.m_MinSpeed = 1f;
                tile.m_MaxSpeed = 1f;
                tileObject = tile;
            }
            else
            {
                Tile tile = ScriptableObject.CreateInstance<Tile>();
                tile.name = tileName;
                tile.sprite = sprite[0];
                tileObject = tile;
            }
        }
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
    [System.Serializable]
    public class TileSpriteSheet
    {
        public SheetData sheetData;
        public Texture2D spriteSheet;
        public bool areTilesPassable = true;
        public bool areTilesMask = false;
        //[HideInInspector]
        public List<MapTile> mapTiles = new List<MapTile>();
        [System.Serializable]
        public class SheetData
        {
            public string name;
            public int pixelWidth;
            public int pixelHeight;
            bool rowsAreFrames = false;
            public SheetData()
            {
                name = "";
                pixelWidth = 16;
                pixelHeight = 16;
                rowsAreFrames = false;
            }
            public SheetData(string name,int width = 16,int height = 16,bool rowsAsFrames = false)
            {
                this.name = name;
                pixelWidth = width;
                pixelHeight = height;
                rowsAreFrames = rowsAsFrames;
            }

        }
        public void CreateMapTiles()
        {
            for (int i = 0; i < spriteSheet.width; i=i+sheetData.pixelWidth)
            {
                for (int j = 0; j < spriteSheet.height; j = j + sheetData.pixelHeight)
                {
                    Sprite tileSprite = Sprite.Create(spriteSheet, new Rect(new Vector2(i, j), 
                        new Vector2(sheetData.pixelWidth, sheetData.pixelHeight)),
                        new Vector2(sheetData.pixelWidth/2, sheetData.pixelHeight/2));
                    MapTile mapTile = new MapTile();
                    mapTile.passable = areTilesPassable;
                    mapTile.sprite = new List<Sprite>();
                    mapTile.sprite.Add(tileSprite);
                    mapTile.isMaskLayer = areTilesMask;
                    mapTile.tileName = sheetData.name + mapTiles.Count;
                    mapTile.CreateTile();
                    mapTiles.Add(mapTile);
                }
            }
        }
    }
}