using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Targetable
{
    Vector3 GetShootPosition();
    string GetFaction();
    bool IsTargedDeadInside();
    int GetTargetPriority();
}
