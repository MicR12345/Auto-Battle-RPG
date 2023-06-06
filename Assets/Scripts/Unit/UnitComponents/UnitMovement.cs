using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitMovement : MonoBehaviour
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

    const float pathfindCooldown = 1f;
    float timer = 0;

    bool tileReserved = false;
    bool moving = false;
    private void FixedUpdate()
    {
        if (!pathfindTargetReached)
        {
            if (pathfindPath==null)
            {
                pathfindTargetReached = true;
                //Pathfind(pathfindTarget);
            }
            if (!tileReserved)
            {
                tileReserved = TryReservingTile(pathfindPath[pathEnumerator]);
                if (!tileReserved)
                {
                     PartialPathfind();
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
            Debug.Log("Path not found");
            return;
        }
        if (pathfindPath.Count>0)
        {
            pathfindTarget = destination;
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
                Debug.Log("Path not found");
                return;
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
}
