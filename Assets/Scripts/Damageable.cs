using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Damageable
{
    void ApplyDamage(int damage);
    string GetFaction();
}
public interface Captureable
{
    void TryCapturing(int value,string faction);
}