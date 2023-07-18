using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using System.Threading;
using System.Threading.Tasks;

namespace PathfindMap
{
    public class Map
    {
        public int sizeX;
        public int sizeY;

        const int closestProximityReattempts = 1000;

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
        public (List<(int,int)>,bool) Astar((int,int) start, (int,int) end,float tolerance)
        {
            bool[,] visited = new bool[sizeX, sizeY];
            List<PathfindNode> open = new List<PathfindNode>();
            List<PathfindNode> closed = new List<PathfindNode>();

            bool targetFound = false;

            open.Add(new PathfindNode(start));
            visited[start.Item1, start.Item2] = true;

            PathfindNode closest = open[0];
            int increaseCounter = 0;
            closest.CalculateFscoreEuclid(start, end);
            while (open.Count > 0 && !targetFound)
            {
                open.Sort(new CompareByScore());
                PathfindNode q = open[0];
                if (q.hscore <closest.hscore)
                {
                    closest = q;
                    increaseCounter = 0;
                }
                else if(q.hscore> closest.hscore)
                {
                    increaseCounter++;
                }
                if (increaseCounter>=5 && closest.hscore<tolerance)
                {
                    return (CreatePath(closest),true);
                }
                if (increaseCounter>closestProximityReattempts)
                {
                    return (CreatePath(closest), true);
                }
                open.Remove(q);
                List<PathfindNode> descendants = new List<PathfindNode>();
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (i == 0 && j == 0) continue;
                        if (q.x + i >= sizeX || q.x + i < 0 || q.y + j >= sizeY || q.y + j < 0) 
                            continue;
                        if (mapTiles[q.x + i, q.y + j].passable && !mapTiles[q.x + i, q.y + j].occupiedStatic && (q.gscore > 20 || !CheckIfOccupied((q.x + i, q.y + j)))
                            && (mapTiles[q.x + i, q.y + j].reserved==null || q.gscore>10) 
                            && !visited[q.x + i, q.y + j])
                        {
                            if (Mathf.Abs(i)==Mathf.Abs(j))
                            {
                                descendants.Add(new PathfindNode((q.x + i, q.y + j),
                                    q.gscore + mapTiles[q.x + i, q.y + j].cost * 1.5f,
                                    q));
                            }
                            else
                            {
                                descendants.Add(new PathfindNode((q.x + i, q.y + j),
                                    q.gscore + mapTiles[q.x + i, q.y + j].cost,
                                    q));
                            }
                            visited[q.x + i, q.y + j] = true;
                        }
 
                    }
                }
                for (int i = 0; i < descendants.Count; i++)
                {
                    if (descendants[i].x == end.Item1 && descendants[i].y== end.Item2)
                    {
                        return (CreatePath(descendants[i]),true);
                    }
                    else
                    {
                        descendants[i].CalculateFscoreEuclid((descendants[i].x, descendants[i].y), end);
                        open.Add(descendants[i]);
                    }
                }
                closed.Add(q);
            }
            //Debug.Log("Couldn't find path from" + start.Item1 + ", " + start.Item2 + " to " + end.Item1 + ", " + end.Item2);
            //Path not found
            return (CreatePath(closest),false);

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
            return mapTiles[position.Item1, position.Item2].reserved != null || CheckIfOccupied(position) || !mapTiles[position.Item1, position.Item2].passable;
        }
        public bool CheckIfReserved((int, int) position)
        {
            return mapTiles[position.Item1, position.Item2].reserved != null;
        }
        public bool CheckIfOccupied((int, int) position)
        {
            if (mapTiles[position.Item1, position.Item2].occupied is not null)
            {
                if (mapTiles[position.Item1, position.Item2].occupied.isDead())
                {
                    return mapTiles[position.Item1, position.Item2].occupiedStatic;
                }
                return !mapTiles[position.Item1, position.Item2].occupied.IsMoving() || mapTiles[position.Item1, position.Item2].occupiedStatic;
            }
            else
            {
                return mapTiles[position.Item1, position.Item2].occupiedStatic;
            }
        }
        public void UpdateOccupation((int,int) tile,(int,int) previousTile,OccupiesTile o)
        {

            mapTiles[tile.Item1, tile.Item2].reserved = null;
            if (mapTiles[previousTile.Item1, previousTile.Item2].occupied == o)
            {
                mapTiles[previousTile.Item1, previousTile.Item2].occupied = null;
            }
            mapTiles[tile.Item1, tile.Item2].occupied = o;
        }
        public void Occupy((int, int) tile, OccupiesTile o)
        {
            mapTiles[tile.Item1, tile.Item2].occupied = o;
        }
        public void ForceReserve((int, int) tile, OccupiesTile o)
        {
            mapTiles[tile.Item1, tile.Item2].reserved = o;
        }
        public void OccupyStatic((int, int) tile)
        {
            mapTiles[tile.Item1, tile.Item2].occupiedStatic = true;
        }
        public bool ReserveTile((int,int) tile,OccupiesTile o)
        {
            if (!CheckIfReservedOrOccupied(tile)
                )
            {
                mapTiles[tile.Item1, tile.Item2].reserved = o;
                return true;
            }
            return false;
        }
        public Task<(List<(int, int)>, bool)> CreatePathfindThread((int,int) start, (int, int) end,float tolerance)
        {
            Task<(List<(int, int)>, bool)> t = Task<(List<(int, int)>, bool)>.Factory.StartNew(
                () =>
                {
                    return Astar(start, end, tolerance);
                }
                );
            return t;
        }
        public class CompareByScore : IComparer<PathfindNode>
        {
            public int Compare(PathfindNode x, PathfindNode y)
            {
                return y.CompareTo(x);
            }
        }
        public class MapTile
        {
            public bool passable;
            public OccupiesTile reserved;
            public OccupiesTile occupied;
            public bool occupiedStatic;
            public float cost;
            public MapTile(bool passable = true, float cost = 1f)
            {
                this.cost = cost;
                reserved = null;
                occupied = null;
                occupiedStatic = false;
                this.passable = passable;
            }
        }
        public class PathfindNode : IComparable
        {
            public int x;
            public int y;
            public float gscore;
            public float hscore = float.PositiveInfinity;
            public float fscore = float.PositiveInfinity;
            public PathfindNode parent = null;
            public PathfindNode((int, int) position, float gscore = 0, PathfindNode parent = null)
            {
                x = position.Item1;
                y = position.Item2;
                this.gscore = gscore;
                this.parent = parent;
            }
            public void CalculateFscoreEuclid((int, int) start, (int, int) end)
            {
                hscore = Mathf.Sqrt(Mathf.Pow(end.Item1 - start.Item1, 2) + Mathf.Pow(end.Item2 - start.Item2, 2));
                fscore = gscore + hscore;
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
    }

    public interface OccupiesTile
    {
        public bool IsMoving();
        public bool isDead();
    }
}
