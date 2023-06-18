using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

public class UnitShoot : MonoBehaviour,StoresData
{
    public Targetable currentTarget;
    public Unit unit;
    bool ceaseFire = false;
    bool targetChanged = false;

    public float shootingTime = 5f;
    public float shootingCooldown = 0f;
    private void Start()
    {
        if (unit.isReconstructed)
        {
            DataStorage data = unit.reconstructionData.FindSubcomp("UnitShoot");
            shootingCooldown = float.Parse(data.FindParam("cooldown").value, CultureInfo.InvariantCulture.NumberFormat);
            shootingTime = float.Parse(data.FindParam("time").value, CultureInfo.InvariantCulture.NumberFormat);
        }
        unit.componentSerializableData.Add(this);
        StartCoroutine(CheckForShooting());

    }
    private void FixedUpdate()
    {
        if (unit.controller.freezeMap || unit.freezeLogic) return;
        if (shootingCooldown<=0f)
        {
            if (currentTarget!=null)
            {
                CreateBullet();
                shootingCooldown += shootingTime;
            }
        }
        else
        {
            shootingCooldown -= Time.deltaTime;
        }
    }
    void CheckForEnemies()
    {
        if (!ceaseFire)
        {
            if (currentTarget!=null && currentTarget.IsTargedDeadInside())
            {
                currentTarget = null;
            }
            if (currentTarget!=null && Vector3.Distance(currentTarget.GetShootPosition(),transform.position) > unit.range)
            {
                currentTarget = null;
            }

            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, unit.range);
            if (colliders != null && colliders.Length > 0)
            {
                foreach (Collider2D collider in colliders)
                {
                    Targetable targetable;
                    if (collider.TryGetComponent<Targetable>(out targetable))
                    {
                        if (targetable.GetFaction() == unit.Faction) continue;
                        if (currentTarget == null)
                        {
                            currentTarget = targetable;
                            targetChanged = true;
                        }
                        else
                        {
                            if (Vector3.Distance(currentTarget.GetShootPosition(),transform.position)>Vector3.Distance(targetable.GetShootPosition(),transform.position))
                            {
                                currentTarget = targetable;
                                targetChanged = true;
                            }
                        }
                    }

                }

            }
        }
    }
    IEnumerator CheckForShooting()
    {
        for(; ; )
        {
            CheckForEnemies();
            yield return new WaitForSeconds(.5f);
        }
    }
    void CreateBullet()
    {
        GameObject bulletObject = GameObject.Instantiate(unit.controller.bulletPrefab);
        Bullet bullet = bulletObject.GetComponent<Bullet>();
        bullet.transform.parent = unit.controller.bulletStorage.transform;
        bullet.transform.position = transform.position;
        bullet.origin = transform.position;
        bullet.target = currentTarget.GetShootPosition();
        bullet.amplitude = 5;
        bullet.speed = 16f;
        bullet.damage = unit.damage;
        bullet.faction = unit.Faction;
        bullet.controller = unit.controller;
        bullet.bulletMovement.enabled = true;
    }

    DataStorage StoresData.GetData()
    {
        DataStorage dataStorage = new DataStorage("UnitShoot");
        dataStorage.RegisterNewParam("cooldown", shootingCooldown.ToString(CultureInfo.InvariantCulture.NumberFormat));
        dataStorage.RegisterNewParam("time", shootingTime.ToString(CultureInfo.InvariantCulture.NumberFormat));
        return dataStorage;
    }
}
