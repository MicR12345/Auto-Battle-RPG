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
        public TurretFactory turretFactory;
        public GameObject bulletPrefab;
        [Header("AI")]
        public AIController aiController;
        [Header("Tiles")]
        public List<BorderTile> borderTiles = new List<BorderTile>();
        public List<MapTile> placeableTiles = new List<MapTile>();
        public List<AutoBorder> autoBorderTiles = new List<AutoBorder>();
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
            foreach (AutoBorder border in autoBorderTiles)
            {
                border.CreateTiles();
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
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0) continue;
                    if (x + i >= 0 && x + i < mapSizeX && y + j >= 0 && y + j < mapSizeY)
                    {
                        placeBorders[i + 1, j + 1] = CheckForBorderBetweenTiles(x, y, x + i, y + j);
                    }
                }
            }
            placeBorders[1, 1] = true;
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
        bool CheckForBorderBetweenTiles(int x1,int y1, int x2,int y2)
        {
            TileBase[] tiles1 = GetTile(x1, y1);
            TileBase[] tiles2 = GetTile(x2, y2);
            MapTile mapTile1 = FindTile(tiles1[tiles1.Length - 1].name);
            MapTile mapTile2 = FindTile(tiles2[tiles2.Length - 1].name);
            int i = x2 - x1;
            int j = y2 - y1;
            if (mapTile1 != null && mapTile2 != null)
            {

                if (mapTile1.tileName==mapTile2.tileName)
                {
                    foreach (Border border in mapTile1.applicableBorders)
                    {
                        string borderVariant = GetBorderVariantSameTile(x1, y1, i, j, border.neighbourName);
                        if (borderVariant != "None")
                        {
                            BorderTile borderTile = FindBorder(border.borderName);
                            AutoBorder autoBorder = FindBorderAuto(border.borderName);
                            if (borderTile != null)
                            {
                                borderTilemap.SetTile(new Vector3Int(2 * x1 + 1 + i, 2 * y1 + 1 + j), borderTile.GetTileVariant(borderVariant));
                                return true;
                            }
                            if (autoBorder != null)
                            {
                                borderTilemap.SetTile(new Vector3Int(2 * x1 + 1 + i, 2 * y1 + 1 + j), autoBorder.GetBorderVariant(borderVariant,border.isInverted));
                                return true;
                            }
                        }
                    }
                    foreach (Border border in mapTile2.applicableBorders)
                    {
                        string borderVariant = GetBorderVariantSameTile(x2, y2, -i, -j, border.neighbourName);
                        if (borderVariant != "None")
                        {
                            BorderTile borderTile = FindBorder(border.borderName);
                            AutoBorder autoBorder = FindBorderAuto(border.borderName);
                            if (borderTile != null)
                            {
                                borderTilemap.SetTile(new Vector3Int(2 * x1 + 1 + i, 2 * y1 + 1 + j), borderTile.GetTileVariant(borderVariant));
                                return true;
                            }
                            if (autoBorder != null)
                            {
                                borderTilemap.SetTile(new Vector3Int(2 * x1 + 1 + i, 2 * y1 + 1 + j), autoBorder.GetBorderVariant(borderVariant, border.isInverted));
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    foreach (Border border in mapTile1.applicableBorders)
                    {
                        if (border.neighbourName==mapTile2.tileName)
                        {
                            string borderVariant = GetBorderVariantFromPosition(x1, y1, i, j, border.neighbourName);
                            BorderTile borderTile = FindBorder(border.borderName);
                            AutoBorder autoBorder = FindBorderAuto(border.borderName);
                            if (borderTile != null)
                            {
                                borderTilemap.SetTile(new Vector3Int(2 * x1 + 1 + i, 2 * y1 + 1 + j), borderTile.GetTileVariant(borderVariant));
                                return true;
                            }
                            if (autoBorder != null)
                            {
                                borderTilemap.SetTile(new Vector3Int(2 * x1 + 1 + i, 2 * y1 + 1 + j), autoBorder.GetBorderVariant(borderVariant, border.isInverted));
                                return true;
                            }
                        }
                    }
                    foreach (Border border in mapTile2.applicableBorders)
                    {
                        if (border.neighbourName == mapTile1.tileName)
                        {
                            string borderVariant = GetBorderVariantFromPosition(x2, y2, -i, -j, border.neighbourName);
                            BorderTile borderTile = FindBorder(border.borderName);
                            AutoBorder autoBorder = FindBorderAuto(border.borderName);
                            if (borderTile != null)
                            {
                                borderTilemap.SetTile(new Vector3Int(2 * x1 + 1 + i, 2 * y1 + 1 + j), borderTile.GetTileVariant(borderVariant));
                                return true;
                            }
                            if (autoBorder != null)
                            {
                                borderTilemap.SetTile(new Vector3Int(2 * x1 + 1 + i, 2 * y1 + 1 + j), autoBorder.GetBorderVariant(borderVariant, border.isInverted));
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
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
                        if (tileRight == neighbourName && tileAbove == neighbourName)
                        {
                            return "PL";
                        }
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
                    if (tileRight == neighbourName && tileBelow == neighbourName)
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
                        if (tileLeft == neighbourName && tileAbove == neighbourName)
                        {
                            return "PR";
                        }
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
                        if (tileLeft == neighbourName && tileBelow == neighbourName)
                        {
                            return "PL";
                        }
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
            for (int i = 0; i < borderTiles.Count; i++)
            {
                if (borderTiles[i].borderName == name)
                {
                    return borderTiles[i];
                }
            }
            return null;
        }
        public AutoBorder FindBorderAuto(string name)
        {
            for (int i = 0; i < autoBorderTiles.Count; i++)
            {
                if (autoBorderTiles[i].borderName == name)
                {
                    return autoBorderTiles[i];
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
    public class AutoBorder
    {
        public string neighbourName;
        public string borderName;
        public List<Sprite> textures;
        public List<TileBase> tileBases;
        public BorderSetup borderSetup;
        public void CreateTiles()
        {
            for (int i = 0; i < textures.Count; i++)
            {
                Tile tile = ScriptableObject.CreateInstance<Tile>();
                tile.name = borderName + "_" + i;
                tile.sprite = textures[i];
                tileBases.Add(tile);
            }
        }
        public static string[] regular = { "TL", "T", "TR", "CBR", "CBL", "ML", "MR", "CTR", "CTL", "BL", "B", "BR", "PR", "PL" };
        public static string[] inverted = { "CBR", "B", "CBL", "TL", "TR", "MR", "ML", "BL", "BR", "CTR", "T", "CTL", "PL", "PR" };
        public enum BorderSetup
        {
            regular,
            inverted
        }
        public TileBase GetBorderVariant(string variant, bool isInverted = false)
        {
            BorderSetup border = borderSetup;
            if (isInverted)
            {
                if (border==BorderSetup.regular)
                {
                    border = BorderSetup.inverted;
                }
                if (border == BorderSetup.inverted)
                {
                    border = BorderSetup.regular;
                }
            }
            switch (border)
            {
                case BorderSetup.regular:
                    for (int i = 0; i < regular.Length; i++)
                    {
                        if (regular[i]==variant)
                        {
                            return tileBases[i];
                        }
                    }
                    break;
                case BorderSetup.inverted:
                    for (int i = 0; i < inverted.Length; i++)
                    {
                        if (inverted[i] == variant)
                        {
                            return tileBases[i];
                        }
                    }
                    break;
            }
            return tileBases[0];
        }
    }
    [System.Serializable]
    public class Border
    {
        public string neighbourName;
        public string borderName;
        [SerializeField]
        public bool isInverted;
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