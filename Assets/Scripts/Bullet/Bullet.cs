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
    private void Start()
    {
        
    }
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
        bulletMovement.enabled = true;
    }
}
