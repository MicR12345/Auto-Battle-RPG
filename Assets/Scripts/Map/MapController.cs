using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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
        private void Start()
        {
            map = new PathfindMap.Map(300,300);
            for (int x = 0; x < 300; x++)
            {
                for (int y = 0; y < 300; y++)
                {
                    if (Random.Range(0,12)==11)
                    {
                        MapTile tile = FindTile("impass");
                        tilemap.SetTile(new Vector3Int(x, y), tile.tile);
                        map.SetTile(x, y, tile.passable);
                    }
                    else
                    {
                        MapTile tile = FindTile("grass");
                        tilemap.SetTile(new Vector3Int(x, y), tile.tile);
                        map.SetTile(x, y, tile.passable);
                    }

                }
            }
            MapBound2.transform.position = new Vector3(300, 300);
            objectives.Add(objectiveFactory.CreateObjective("Base", new Vector3(10.5f, 10.5f),"Player"));
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
        public void RegisterUnit(Unit unit)
        {
            units.Add(unit);
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
        public TileBase tile;
    }
}