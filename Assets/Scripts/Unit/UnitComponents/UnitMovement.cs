using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitMovement : MonoBehaviour,StoresData
{
    public Unit unit;
    public (int, int) currentTile;
    [SerializeField]
    List<(int, int)> pathfindPath;
    [SerializeField]
    int pathEnumerator = 0;
    [SerializeField]
    (int, int) pathfindTarget;
    [SerializeField]
    bool pathfindTargetReached = true;

    Vector3 V3PathfindStep;

    bool tileReserved = false;
    bool pathfindTargetChanged = false;
    bool moving = false;

    public (int,int)? ReservedTile
    {
        get
        {
            if (tileReserved) return pathfindPath[pathEnumerator];
            else return null;
        }
    }
    public void OnEnable()
    {
        unit.componentSerializableData.Add(this);
    }
    private void FixedUpdate()
    {
        if (unit.freezeLogic || unit.controller.freezeMap)
        {
            return;
        }
        TickUnitMovementLogic();
    }
    void TickUnitMovementLogic()
    {
        if (pathfindPath == null && !pathfindTargetChanged)
        {
            pathfindTargetReached = true;
            //TODO Reattempt to pathfind when locked inside units
        }
        if (!pathfindTargetReached)
        {
            if (!pathfindTargetChanged && pathEnumerator > pathfindPath.Count)
            {
                Pathfind(pathfindTarget);
            }
            if (!tileReserved)
            {
                if (pathfindTargetChanged)
                {
                    pathfindTargetChanged = false;
                    Pathfind(pathfindTarget);
                }
                else
                {
                    tileReserved = TryReservingTile(pathfindPath[pathEnumerator]);
                    if (!tileReserved)
                    {
                        PartialPathfind();
                    }
                }

            }
            else
            {
                moving = true;
                V3PathfindStep = PathfindMap.Map.ConvertPathNodeToV3(pathfindPath[pathEnumerator]);
            }
            if (moving)
            {
                if (Vector3.Distance(V3PathfindStep, transform.parent.position) < 0.01f)
                {
                    pathEnumerator++;
                    if (pathEnumerator >= pathfindPath.Count)
                    {
                        pathfindTargetReached = true;
                    }
                    (int, int) prevTile = currentTile;
                    currentTile = pathfindPath[pathEnumerator - 1];
                    unit.controller.map.UpdateOccupation(currentTile, prevTile);
                    transform.parent.position = V3PathfindStep;
                    moving = false;
                    tileReserved = false;
                }
                else
                {
                    transform.parent.position = Vector3.Lerp(transform.parent.position, V3PathfindStep,
                        unit.speed * Time.deltaTime);
                }
            }
        }
    }
    public void Pathfind((int,int) destination)
    {
        pathfindPath = unit.controller.map.Astar(currentTile, destination);
        if (pathfindPath==null)
        {
            pathfindTargetReached = true;
            return;
        }
        if (pathfindPath.Count>0)
        {
            if (pathfindTarget != pathfindPath[pathfindPath.Count-1])
            {
                pathfindTarget = pathfindPath[pathfindPath.Count-1];
            }
            else
            {
                pathfindTarget = destination;
            }
            pathEnumerator = 0;
            pathfindTargetReached = false;
        }
        if (pathfindPath.Count==0)
        {
            pathfindTargetReached = true;
        }

    }
    public void PartialPathfind()
    {
        int newEnumerator = FindNextUnoccupiedTile();
        if (newEnumerator == pathfindPath.Count)
        {
            Pathfind(pathfindTarget);
        }
        else
        {
            List<(int, int)> partialPath = unit.controller.map.Astar(currentTile, pathfindPath[newEnumerator]);
            if (partialPath == null || partialPath.Count == 0)
            {
                pathfindTargetReached = true;
                return;
            }
            if (partialPath[partialPath.Count-1] != pathfindPath[newEnumerator])
            {
                pathfindTarget = partialPath[partialPath.Count - 1];
            }
            pathfindPath.RemoveRange(pathEnumerator, newEnumerator - pathEnumerator);
            pathfindPath.InsertRange(pathEnumerator, partialPath);
        }

    }
    public bool TryReservingTile((int,int) tile)
    {
        return unit.controller.map.ReserveTile(tile,this);
    }
    int FindNextUnoccupiedTile()
    {
        for (int i = pathEnumerator; i < pathfindPath.Count; i++)
        {
            if(!unit.controller.map.CheckIfReservedOrOccupied(pathfindPath[i]))
            {
                return i;
            }
        }
        return pathfindPath.Count;
    }
    public bool IsFinalTileOccupied()
    {
        return true;
    }
    private void OnDrawGizmos()
    {
        if (pathfindPath != null && !pathfindTargetReached)
        {
            for (int i = pathEnumerator + 1; i < pathfindPath.Count; i++)
            {
                Gizmos.DrawLine(PathfindMap.Map.ConvertPathNodeToV3(pathfindPath[i]), PathfindMap.Map.ConvertPathNodeToV3(pathfindPath[i - 1]));
            }
        }

    }
    public void BeginPathfind((int,int) tile)
    {
        pathfindTargetReached = false;
        pathfindTarget = tile;
        pathfindTargetChanged = true;
    }
    public DataStorage GenerateData()
    {
        DataStorage dataStorage = new DataStorage("UnitMovement");
        dataStorage.RegisterNewParam("currentTileX", currentTile.Item1.ToString());
        dataStorage.RegisterNewParam("currentTileY", currentTile.Item2.ToString());
        dataStorage.RegisterNewParam("pathfindTargetReached",pathfindTargetReached.ToString());
        if (!pathfindTargetReached)
        {
            dataStorage.RegisterNewParam("pathfindTargetX", pathfindTarget.Item1.ToString());
            dataStorage.RegisterNewParam("pathfindTargetY", pathfindTarget.Item2.ToString());
            dataStorage.RegisterNewParam("tileReserved", tileReserved.ToString());
            dataStorage.RegisterNewParam("reservedTileX", pathfindPath[pathEnumerator].Item1.ToString());
            dataStorage.RegisterNewParam("reservedTileY", pathfindPath[pathEnumerator].Item2.ToString());
        }
        return dataStorage;
    }

    DataStorage StoresData.GetData()
    {
        return GenerateData();
    }
}
