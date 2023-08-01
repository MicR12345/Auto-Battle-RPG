using PathfindMap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using System.Threading.Tasks;

public class UnitMovement : MonoBehaviour,StoresData,PathfindMap.OccupiesTile
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
    public bool pathfindTargetReached = true;

    Vector3 V3PathfindStep;
    Vector3 currentTileV3;

    bool tileReserved = false;
    bool pathfindTargetChanged = false;
    bool moving = false;
    bool pathfindAfterMove = false;

    Task<(List<(int, int)>, bool)> pathfindHandle = null;
    public (int,int)? ReservedTile
    {
        get
        {
            if (tileReserved) return pathfindPath[pathEnumerator];
            else return null;
        }
    }
    void Start()
    {
        unit.componentSerializableData.Add(this);
        if (unit.isReconstructed)
        {
            DataStorage movementData = unit.reconstructionData.FindSubcomp("UnitMovement");
            (int,int) newCurrentTile = (
                int.Parse(movementData.FindParam("currentTileX").value),
                int.Parse(movementData.FindParam("currentTileY").value)
                );
            if (newCurrentTile != currentTile)
            {
                unit.controller.map.UpdateOccupation(newCurrentTile, currentTile, this);
                currentTile = newCurrentTile;
            }
            pathfindTargetReached = bool.Parse(movementData.FindParam("pathfindTargetReached").value);
            if (!pathfindTargetReached)
            {
                pathfindTarget = (
                    int.Parse(movementData.FindParam("pathfindTargetX").value),
                    int.Parse(movementData.FindParam("pathfindTargetY").value)
                    );
                tileReserved = bool.Parse(movementData.FindParam("tileReserved").value);
                if (tileReserved)
                {
                    (int, int) tileToBeReserved = (
                        int.Parse(movementData.FindParam("reservedTileX").value),
                        int.Parse(movementData.FindParam("reservedTileY").value)
                        );
                    unit.controller.map.ReserveTile(tileToBeReserved,this);
                    moving = true;
                    V3PathfindStep = PathfindMap.Map.ConvertPathNodeToV3(tileToBeReserved);
                    pathfindTargetChanged = true;
                    pathfindPath = new List<(int, int)>();
                    pathfindPath.Add(tileToBeReserved);
                    pathfindAfterMove = true;
                }
                else
                {
                    BeginPathfind(pathfindTarget);
                }
            }
        }
        unit.SetGraphics("NONE");
    }
    private void FixedUpdate()
    {
        if (unit.freezeLogic || unit.controller.freezeMap)
        {
            return;
        }
        NewTickMovementLogic();
    }
    void NewTickMovementLogic()
    {
        if (tileReserved)
        {
            if (!moving)
            {
                moving = true;
                V3PathfindStep = PathfindMap.Map.ConvertPathNodeToV3(pathfindPath[pathEnumerator]);
                (int, int) tile = pathfindPath[pathEnumerator];
                currentTileV3 = PathfindMap.Map.ConvertPathNodeToV3(currentTile);
                ResolveMovementGraphics(tile.Item1 - currentTile.Item1, tile.Item2 - currentTile.Item2);
            }
            if (moving)
            {
                if (Vector3.Distance(currentTileV3, transform.parent.position) >= Vector3.Distance(currentTileV3,V3PathfindStep))
                {
                    pathEnumerator++;
                    (int, int) prevTile = currentTile;
                    currentTile = pathfindPath[pathEnumerator - 1];
                    unit.controller.map.UpdateOccupation(currentTile, prevTile, this);
                    transform.parent.position = V3PathfindStep;
                    moving = false;
                    tileReserved = false;
                }
                else
                {
                    transform.parent.position = transform.parent.position + Vector3.Normalize(V3PathfindStep - transform.parent.position) * unit.speed * Time.deltaTime;
                }
            }
        }
        else
        {
            //ResolveMovementGraphics(0, 0);
            if (pathfindHandle!=null)
            {
                if (!pathfindHandle.IsCompleted)
                {
                    return;
                }
                else
                {
                    pathfindPath = new List<(int, int)>();
                    (List<(int, int)>, bool) output = pathfindHandle.Result;
                    pathfindPath = output.Item1;
                }
            }
            if (pathfindPath != null && pathfindPath.Count > pathEnumerator && !pathfindTargetReached && !pathfindTargetChanged)
            {
                tileReserved = TryReservingTile(pathfindPath[pathEnumerator]);
                if (!tileReserved)
                {
                    PartialPathfind();
                }
            }
            else
            {
                if (!pathfindTargetReached)
                {
                    if (pathfindPath == null)
                    {
                        Pathfind(pathfindTarget);
                    }
                    else
                    {
                        if (pathfindPath.Count <= pathEnumerator)
                        {
                            if (Mathf.Sqrt(Mathf.Pow(pathfindTarget.Item1 - currentTile.Item1,2)+ Mathf.Pow(pathfindTarget.Item2 - currentTile.Item2, 2))<=unit.range)
                            {
                                pathfindTargetReached = true;
                                ResolveMovementGraphics(0, 0);
                            }
                            else
                            {
                                Pathfind(pathfindTarget);
                            }
                        }
                    }
                    if (pathfindTargetChanged)
                    {
                        pathfindTargetChanged = false;
                        Pathfind(pathfindTarget);
                    }
                }
            }
        }
    }
    string[,] graphicsReference =
    {
        { "mSW","mW", "mNW"},
        { "mS","NONE","mN" },
        { "mSE","mE","mNE" }
    };
    string lastMovementGraphics = "NONE";
    void ResolveMovementGraphics(int xOffset,int yOffset)
    {
        if (lastMovementGraphics!= graphicsReference[xOffset + 1, yOffset + 1])
        {
            unit.SetGraphics(graphicsReference[xOffset + 1, yOffset + 1]);
            lastMovementGraphics = graphicsReference[xOffset + 1, yOffset + 1];
        }
    }
    public void Pathfind((int,int) destination)
    {
        //(List<(int,int)>, bool) path = unit.controller.map.Astar(currentTile, destination,unit.range);
        pathfindHandle = unit.controller.map.CreatePathfindThread(currentTile, destination, unit.range/2);
        pathEnumerator = 0;
    }
    public void PartialPathfind()
    {
        if (pathfindPath.Count - pathEnumerator < 10)
        {
            Pathfind(pathfindTarget);
            return;
        }
        int newEnumerator = FindNextUnoccupiedTile();
        if (newEnumerator - pathEnumerator >3 || newEnumerator == pathfindPath.Count)
        {
            Pathfind(pathfindTarget);
            return;
        }
        else
        {
            (List<(int, int)>,bool) partialPath = unit.controller.map.Astar(currentTile, pathfindPath[newEnumerator],unit.range);
            if (partialPath.Item1 == null)
            {
                Pathfind(pathfindTarget);
                return;
            }
            if (partialPath.Item2)
            {
                pathfindPath.RemoveRange(pathEnumerator, newEnumerator - pathEnumerator);
                pathfindPath.InsertRange(pathEnumerator, partialPath.Item1);
                return;
            }
            else
            {
                Pathfind(pathfindTarget);
                return;
            }
            
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
        DataStorage dataStorage = new DataStorage(transform.name);
        dataStorage.RegisterNewParam("currentTileX", currentTile.Item1.ToString());
        dataStorage.RegisterNewParam("currentTileY", currentTile.Item2.ToString());
        dataStorage.RegisterNewParam("pathfindTargetReached",pathfindTargetReached.ToString());
        if (!pathfindTargetReached)
        {
            dataStorage.RegisterNewParam("pathfindTargetX", pathfindTarget.Item1.ToString());
            dataStorage.RegisterNewParam("pathfindTargetY", pathfindTarget.Item2.ToString());
            dataStorage.RegisterNewParam("tileReserved", tileReserved.ToString());
            if (tileReserved)
            {
                dataStorage.RegisterNewParam("reservedTileX", pathfindPath[pathEnumerator].Item1.ToString());
                dataStorage.RegisterNewParam("reservedTileY", pathfindPath[pathEnumerator].Item2.ToString());
            }
        }
        return dataStorage;
    }

    DataStorage StoresData.GetData()
    {
        return GenerateData();
    }

    bool OccupiesTile.IsMoving()
    {
        return moving;
    }

    bool OccupiesTile.isDead()
    {
        return unit.isDead;
    }
}
