using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitShoot : MonoBehaviour
{
    public Targetable currentTarget;
    public Unit unit;
    bool ceaseFire = false;
    bool targetChanged = false;
    private void Start()
    {
        StartCoroutine(CheckForShooting());
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
            if (currentTarget != null)
            {
                CreateBullet();
            }
            yield return new WaitForSeconds(.5f);
        }
    }
    void CreateBullet()
    {
        GameObject bulletObject = GameObject.Instantiate(unit.bulletPrefab);
        Bullet bullet = bulletObject.GetComponent<Bullet>();
        bullet.transform.position = transform.position;
        bullet.origin = transform.position;
        bullet.target = currentTarget.GetShootPosition();
        bullet.amplitude = 5;
        bullet.speed = 16f;
        bullet.damage = unit.damage;
        bullet.faction = unit.Faction;
        bullet.bulletMovement.enabled = true;
    }

}
