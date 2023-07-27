using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

public class TurretShoot : MonoBehaviour, StoresData
{
    public Targetable currentTarget;
    public Turret turret;
    bool ceaseFire = false;
    bool targetChanged = false;

    public float shootingTime = 5f;
    public float shootingCooldown = 0f;

    public BulletPhase currentPhase;
    public int bulletPhaseIter = 0;
    private void Start()
    {
        if (turret.isReconstructed)
        {
            DataStorage data = turret.reconstructionData.FindSubcomp("TurretShoot");
            shootingCooldown = float.Parse(data.FindParam("cooldown").value, CultureInfo.InvariantCulture.NumberFormat);
            shootingTime = float.Parse(data.FindParam("time").value, CultureInfo.InvariantCulture.NumberFormat);
            bulletPhaseIter = int.Parse(data.FindParam("bulletPhaseIter").value, CultureInfo.InvariantCulture.NumberFormat);
        }
        turret.componentSerializableData.Add(this);
        StartCoroutine(CheckForShooting());

    }
    private void FixedUpdate()
    {
        if (turret.controller.freezeMap || turret.freezeLogic) return;
        if (shootingCooldown <= 0f)
        {
            if (currentTarget != null && (currentTarget.IsTargedDeadInside() || currentTarget.GetFaction() == turret.Faction)) currentTarget = null;
            if (currentTarget != null || currentPhase.phaseName != "shoot")
            {
                //CreateBullet();
                ProgressBulletPhase();
                shootingCooldown += currentPhase.phaseCD;
            }
        }
        else
        {
            shootingCooldown -= Time.deltaTime;
        }
    }
    void ProgressBulletPhase()
    {
        switch (currentPhase.phaseName)
        {
            case "shoot":
                Bullet.CreateBullet(currentPhase,currentTarget,turret.controller,transform.position,turret.Faction);
                break;
            case "wait":
                break;
            case "shootExtra":
                Targetable target = CheckForEnemiesOpportunity(currentPhase);
                if (target != null && !target.IsTargedDeadInside())
                {
                    Bullet.CreateBullet(currentPhase, target, turret.controller, transform.position, turret.Faction);
                }
                break;
        }
        if (bulletPhaseIter >= turret.bulletPhases.Count - 1)
        {
            bulletPhaseIter = 0;
            currentPhase = turret.bulletPhases[bulletPhaseIter];
        }
        else
        {
            bulletPhaseIter++;
            currentPhase = turret.bulletPhases[bulletPhaseIter];
        }
    }
    void CheckForEnemies()
    {
        if (!ceaseFire)
        {
            if (currentTarget != null && (currentTarget.IsTargedDeadInside() || currentTarget.GetFaction() == turret.Faction))
            {
                currentTarget = null;
            }
            if (currentTarget != null && Vector3.Distance(currentTarget.GetShootPosition(), transform.position) > turret.range)
            {
                currentTarget = null;
            }

            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, turret.range);
            if (colliders != null && colliders.Length > 0)
            {
                foreach (Collider2D collider in colliders)
                {
                    Targetable targetable;
                    if (collider.TryGetComponent<Targetable>(out targetable))
                    {
                        if (targetable.GetFaction() == turret.Faction) continue;
                        if (currentTarget == null)
                        {
                            currentTarget = targetable;
                            targetChanged = true;
                        }
                        else
                        {
                            if (currentTarget.GetTargetPriority() < targetable.GetTargetPriority())
                            {
                                currentTarget = targetable;
                                targetChanged = true;
                                continue;
                            }
                            if (
                                Vector3.Distance(currentTarget.GetShootPosition(), transform.position) > Vector3.Distance(targetable.GetShootPosition(), transform.position)
                                && currentTarget.GetTargetPriority() <= targetable.GetTargetPriority()
                                )
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
    Targetable CheckForEnemiesOpportunity(BulletPhase bulletPhase)
    {
        Targetable localTarget = null;
        if (!ceaseFire)
        {

            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, bulletPhase.range);
            if (colliders != null && colliders.Length > 0)
            {
                foreach (Collider2D collider in colliders)
                {
                    Targetable targetable;
                    if (collider.TryGetComponent<Targetable>(out targetable))
                    {
                        if (targetable.GetFaction() == turret.Faction) continue;
                        if (localTarget == null)
                        {
                            localTarget = targetable;
                        }
                        else
                        {
                            if (currentTarget.GetTargetPriority() < targetable.GetTargetPriority())
                            {
                                localTarget = targetable;
                                continue;
                            }
                            if (
                                Vector3.Distance(currentTarget.GetShootPosition(), transform.position) > Vector3.Distance(targetable.GetShootPosition(), transform.position)
                                && currentTarget.GetTargetPriority() <= targetable.GetTargetPriority()
                                )
                            {
                                localTarget = targetable;
                            }
                        }
                    }

                }

            }
        }
        return localTarget;
    }
    IEnumerator CheckForShooting()
    {
        for (; ; )
        {
            CheckForEnemies();
            yield return new WaitForSeconds(.5f);
        }
    }


    DataStorage StoresData.GetData()
    {
        DataStorage dataStorage = new DataStorage("TurretShoot");
        dataStorage.RegisterNewParam("cooldown", shootingCooldown.ToString(CultureInfo.InvariantCulture.NumberFormat));
        dataStorage.RegisterNewParam("time", shootingTime.ToString(CultureInfo.InvariantCulture.NumberFormat));
        dataStorage.RegisterNewParam("bulletPhaseIter", bulletPhaseIter.ToString(CultureInfo.InvariantCulture.NumberFormat));
        return dataStorage;
    }
}
