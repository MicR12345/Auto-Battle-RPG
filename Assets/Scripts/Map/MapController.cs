using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
namespace TileMap
{
    public class MapController : MonoBehaviour
    {
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

        public bool freezeMap = false;
        private void Start()
        {
            CreateTilesPrefabs();
            CreateEmptyMapWithSize(300, 300);
            FillMapEditorOptions();
        }
        void CreateEmptyMapWithSize(int x,int y)
        {
            map = new PathfindMap.Map(x, y);
            MapTile mapTile = FindTile("grass");
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
}