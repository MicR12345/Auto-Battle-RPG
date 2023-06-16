using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Vector3 target;
    public Vector3 origin;
    public int damage;
    public float speed = 1f;
    public float amplitude = 5f;
    public float explostionRange = 1f;
    public string faction;
    public BulletMovement bulletMovement;
    private void Start()
    {
        
    }
    public void Explode()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explostionRange);
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
}
