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
            objectiveFactory.CreateObjective("Base", new Vector3(10.5f, 10.5f),"Player");
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
    }


    [System.Serializable]
    public class MapTile
    {
        public string tileName;
        public bool passable;
        public TileBase tile;
    }
}