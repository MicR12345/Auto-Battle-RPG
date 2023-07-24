using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletMovement : MonoBehaviour
{
    [SerializeField]
    GameObject visualObject;
    float speed = 1f;
    public Vector3 target;
    public Vector3 origin;

    BulletType type;
    [SerializeField]
    Bullet bullet;
    private void Start()
    {
        target = bullet.target;
        origin = bullet.origin;
        speed = bullet.speed;
        ArcBullet arcBullet = new ArcBullet(target, origin, bullet.amplitude);
        type = arcBullet;
    }
    private void Update()
    {
        if (bullet.controller == null)
        {
            GameObject.Destroy(gameObject);
            return;
        }
        if (bullet.controller.freezeMap) return;
        if (Vector3.Distance(origin,transform.position)>=Vector3.Distance(origin,target))
        {
            transform.position = target;
            bullet.Explode();
        }
        transform.position = transform.position + Vector3.Normalize(target-transform.position) * speed * Time.deltaTime;
        visualObject.transform.localPosition = type.GetVisualOffset(transform.position);
    }
    public interface BulletType
    {
        Vector3 GetVisualOffset(Vector3 position);
    }
    public class ArcBullet : BulletType
    {
        Vector3 target;
        float distance = 1f;
        float amplitude = 4f;
        public ArcBullet(Vector3 target,Vector3 start,float amplitude)
        {
            this.target = target;
            distance = Vector3.Distance(target,start);
            this.amplitude = amplitude*4;
        }

        Vector3 BulletType.GetVisualOffset(Vector3 position)
        {
            float currDist = Vector3.Distance(target, position);
            float arcPosition = currDist / distance;
            float yOffset = (-(arcPosition - 1) * arcPosition) * amplitude;
            return new Vector3(0f, yOffset, 0f);
        }
    }
}

