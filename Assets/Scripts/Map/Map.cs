using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

namespace PathfindMap
{
    [System.Serializable]
    public class Map
    {
        public int sizeX;
        public int sizeY;

        MapTile[,] mapTiles;

        public Map(int sizeX, int sizeY)
        {
            this.sizeX = sizeX;
            this.sizeY = sizeY;
            mapTiles = new MapTile[sizeX, sizeY];
        }
        public void SetTile(int x,int y,bool passable)
        {
            mapTiles[x,y] = new MapTile(passable);
        }
        public List<(int,int)> Astar((int,int) start, (int,int) end)
        {
            bool[,] visited = new bool[sizeX, sizeY];
            List<PathfindNode> open = new List<PathfindNode>();
            List<PathfindNode> closed = new List<PathfindNode>();

            bool targetFound = false;

            open.Add(new PathfindNode(start));
            visited[start.Item1, start.Item2] = true;

            PathfindNode closest = open[0];
            int increaseCounter = 0;
            while (open.Count > 0 && !targetFound)
            {
                open.Sort(new CompareByScore());
                PathfindNode q = open[0];
                if (q.fscore<closest.fscore)
                {
                    closest = q;
                    increaseCounter = 0;
                }
                else
                {
                    increaseCounter++;
                }
                if (increaseCounter>=50)
                {
                    return CreatePath(closest);
                }
                open.Remove(q);
                List<PathfindNode> descendants = new List<PathfindNode>();
                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        if (i == 0 && j == 0) continue;
                        if (q.x + i >= sizeX || q.x + i < 0 || q.y + j >= sizeY || q.y + j < 0) 
                            continue;
                        if (mapTiles[q.x + i, q.y + j].passable && !mapTiles[q.x + i, q.y + j].occupiedStatic && (!mapTiles[q.x + i, q.y + j].occupied || q.gscore > 5)
                            && (mapTiles[q.x + i, q.y + j].reserved==null || q.gscore>5) 
                            && !visited[q.x + i, q.y + j])
                        {
                            descendants.Add(new PathfindNode((q.x + i, q.y + j),
                                    q.gscore + mapTiles[q.x + i, q.y + j].cost,
                                    q));
                            visited[q.x + i, q.y + j] = true;
                        }
 
                    }
                }
                for (int i = 0; i < descendants.Count; i++)
                {
                    if (descendants[i].x == end.Item1 && descendants[i].y== end.Item2)
                    {
                        return CreatePath(descendants[i]);
                    }
                    else
                    {
                        descendants[i].CalculateFscoreEuclid((descendants[i].x, descendants[i].y), end);
                        open.Add(descendants[i]);
                    }
                }
                closed.Add(q);
            }
            Debug.Log("Couldn't find path from" + start.Item1 + ", " + start.Item2 + " to " + end.Item1 + ", " + end.Item2);
            //Path not found
            return CreatePath(closest);

        }
        public List<(int,int)> CreatePath(PathfindNode p)
        {
            PathfindNode pointer = p;
            List<(int,int)> path = new List<(int,int)> ();
            while (pointer.parent != null)
            {
                path.Add((pointer.x, pointer.y));
                pointer = pointer.parent;
            }
            path.Reverse();
            return path;
        }
        public static Vector3 ConvertPathNodeToV3((int, int) position)
        {
            return new Vector3(position.Item1 + 0.5f, position.Item2 + 0.5f);
        }
        public bool CheckIfReservedOrOccupied((int,int) position)
        {
            return mapTiles[position.Item1, position.Item2].reserved != null || mapTiles[position.Item1, position.Item2].occupied
                || mapTiles[position.Item1, position.Item2].occupiedStatic;
        }
        public bool CheckIfReserved((int, int) position)
        {
            return mapTiles[position.Item1, position.Item2].reserved;
        }
        public bool CheckIfOccupied((int, int) position)
        {
            return mapTiles[position.Item1, position.Item2].occupied || mapTiles[position.Item1, position.Item2].occupiedStatic;
        }
        public bool CheckIfReachableReverse((int, int) end, (int, int) start)
        {
            if (Astar(start, end) != null) return true;
            return false;
        }
        public void UpdateOccupation((int,int) tile,(int,int) previousTile)
        {
            mapTiles[previousTile.Item1, previousTile.Item2].occupied = false;
            mapTiles[previousTile.Item1, previousTile.Item2].reserved = null;
            mapTiles[tile.Item1, tile.Item2].occupied = true;
        }
        public bool ReserveTile((int,int) tile,UnitMovement unitMovement)
        {
            if (mapTiles[tile.Item1, tile.Item2].reserved==null && !mapTiles[tile.Item1, tile.Item2].occupied && !mapTiles[tile.Item1, tile.Item2].occupiedStatic)
            {
                mapTiles[tile.Item1, tile.Item2].reserved = unitMovement;
                return true;
            }
            return false;
        }
        public class PathfindNode : IComparable
        {
            public int x;
            public int y;
            public float gscore;
            public float fscore = float.PositiveInfinity;
            public PathfindNode parent = null;
            public PathfindNode((int,int) position,float gscore = 0,PathfindNode parent = null)
            {
                x = position.Item1;
                y = position.Item2;
                this.gscore = gscore;
                this.parent = parent;
            }
            public void CalculateFscoreEuclid((int,int) start,(int,int) end)
            {
                fscore = gscore +Mathf.Sqrt(Mathf.Pow(end.Item1 - start.Item1,2) + Mathf.Pow(end.Item2 - start.Item2,2));
            }
            public int CompareTo(object obj)
            {
                if (obj == null) return 1;
                PathfindNode pathfindNode = obj as PathfindNode;
                if (pathfindNode != null)
                {
                    if (pathfindNode.fscore == this.fscore)
                    {
                        return 0;
                    }
                    else return pathfindNode.fscore.CompareTo(this.fscore);
                }
                else
                {
                    throw new ArgumentException("Not pathfind node");
                }
            }
        }

        public class CompareByScore : IComparer<PathfindNode>
        {
            public int Compare(PathfindNode x, PathfindNode y)
            {
                return y.CompareTo(x);
            }
        }
    }
    [System.Serializable]
    public class MapTile
    {
        public bool passable;
        public UnitMovement reserved;
        public bool occupied;
        public bool occupiedStatic;
        public float cost;
        public MapTile(bool passable = true,float cost = 0.5f)
        {
            this.cost = cost;
            reserved = null;
            occupied = false;
            occupiedStatic = false;
            this.passable = passable;
        }
    }
}
