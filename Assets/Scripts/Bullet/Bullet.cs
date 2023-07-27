using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

public class Bullet : MonoBehaviour,StoresData
{
    public Vector3 target;
    public Vector3 origin;
    public int damage;
    public float speed = 1f;
    public float amplitude = 5f;
    public float explosionRange = 1f;
    public string faction;
    public BulletMovement bulletMovement;

    public TileMap.MapController controller;
    public Animator animator;
    public void Explode()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRange);
        foreach (Collider2D collider in colliders)
        {
            Damageable damageable;
            if (collider.TryGetComponent<Damageable>(out damageable))
            {
                if (damageable.GetFaction()!=faction)
                {
                    damageable.ApplyDamage(damage);
                }
            }
            Captureable captureable;
            if (collider.TryGetComponent<Captureable>(out captureable))
            {
                captureable.TryCapturing(damage, faction);
            }
        }
        GameObject.Destroy(gameObject);
    }

    DataStorage StoresData.GetData()
    {
        DataStorage dataStorage = new DataStorage("Bullet");
        dataStorage.RegisterNewParam("damage", damage.ToString());
        dataStorage.RegisterNewParam("speed", speed.ToString(CultureInfo.InvariantCulture.NumberFormat));
        dataStorage.RegisterNewParam("amplitude", amplitude.ToString(CultureInfo.InvariantCulture.NumberFormat));
        dataStorage.RegisterNewParam("explosionRange", explosionRange.ToString(CultureInfo.InvariantCulture.NumberFormat));
        dataStorage.RegisterNewParam("faction", faction);
        dataStorage.RegisterNewParam("x", transform.position.x.ToString(CultureInfo.InvariantCulture.NumberFormat));
        dataStorage.RegisterNewParam("y", transform.position.y.ToString(CultureInfo.InvariantCulture.NumberFormat));
        dataStorage.RegisterNewParam("targetX", target.x.ToString(CultureInfo.InvariantCulture.NumberFormat));
        dataStorage.RegisterNewParam("targetY", target.y.ToString(CultureInfo.InvariantCulture.NumberFormat));
        dataStorage.RegisterNewParam("originX", origin.x.ToString(CultureInfo.InvariantCulture.NumberFormat));
        dataStorage.RegisterNewParam("originY", origin.y.ToString(CultureInfo.InvariantCulture.NumberFormat));
        return dataStorage;
    }
    public void RestoreFromData(DataStorage dataStorage,TileMap.MapController controller)
    {
        damage = int.Parse(dataStorage.FindParam("damage").value);
        speed = float.Parse(dataStorage.FindParam("speed").value, CultureInfo.InvariantCulture.NumberFormat);
        amplitude = float.Parse(dataStorage.FindParam("amplitude").value, CultureInfo.InvariantCulture.NumberFormat);
        explosionRange = float.Parse(dataStorage.FindParam("explosionRange").value, CultureInfo.InvariantCulture.NumberFormat);
        faction = dataStorage.FindParam("faction").value;
        Vector3 position = new Vector3(
            float.Parse(dataStorage.FindParam("x").value, CultureInfo.InvariantCulture.NumberFormat),
            float.Parse(dataStorage.FindParam("y").value, CultureInfo.InvariantCulture.NumberFormat)
            );
        transform.position = position;
        target = new Vector3(
            float.Parse(dataStorage.FindParam("targetX").value, CultureInfo.InvariantCulture.NumberFormat),
            float.Parse(dataStorage.FindParam("targetY").value, CultureInfo.InvariantCulture.NumberFormat)
            );
        transform.position = position;
        origin = new Vector3(
            float.Parse(dataStorage.FindParam("originX").value, CultureInfo.InvariantCulture.NumberFormat),
            float.Parse(dataStorage.FindParam("originY").value, CultureInfo.InvariantCulture.NumberFormat)
            );
        transform.parent = controller.bulletStorage.transform;
        transform.position = position;
        this.controller = controller;
        bulletMovement.enabled = true;
    }
    public static void CreateBullet(BulletPhase bulletPhase ,Targetable currentTarget,TileMap.MapController controller, Vector3 origin, string faction)
    {
        GameObject bulletObject = GameObject.Instantiate(controller.bulletPrefab);
        Bullet bullet = bulletObject.GetComponent<Bullet>();
        bullet.transform.parent = controller.bulletStorage.transform;
        bullet.transform.position = origin;
        bullet.origin = origin;
        bullet.target = currentTarget.GetShootPosition();
        bullet.amplitude = bulletPhase.amplitude;
        bullet.speed = bulletPhase.speed;
        bullet.damage = bulletPhase.damage;
        bullet.faction = faction;
        bullet.controller = controller;
        bullet.animator.SetSpriteList(bulletPhase.bulletSprites, bulletPhase.animSpeed);
        bullet.bulletMovement.enabled = true;
    }
}
[System.Serializable]
public class BulletPhase
{
    public string phaseName;
    public float phaseCD;
    public float amplitude;
    public float speed;
    public int damage;
    public float range;
    public List<Sprite> bulletSprites;
    public float animSpeed;

    public List<Sprite> explosionSprites;
    public float explostionAnimSpeed;
}