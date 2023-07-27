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
        transform.position = transform.position + Vector3.Normalize(target - transform.position) * speed * Time.deltaTime;
        if (Vector3.Distance(origin,transform.position)>=Vector3.Distance(origin,target))
        {
            transform.position = target;
            bullet.Explode();
        }
        visualObject.transform.localPosition = type.GetVisualOffset(transform.position);
        visualObject.transform.rotation = type.GetVisualRotation(transform.rotation);
    }
    public interface BulletType
    {
        Vector3 GetVisualOffset(Vector3 position);
        Quaternion GetVisualRotation(Quaternion rotation);
    }
    public class ArcBullet : BulletType
    {
        Vector3 target;
        float distance = 1f;
        float amplitude = 4f;
        Vector3 currentPos;
        Vector3 prevPosition;
        public ArcBullet(Vector3 target,Vector3 start,float amplitude)
        {
            this.target = target;
            distance = Vector3.Distance(target,start);
            this.amplitude = amplitude*4;
            currentPos = start;
            prevPosition = start;
        }

        Vector3 BulletType.GetVisualOffset(Vector3 position)
        {
            prevPosition = currentPos;
            float currDist = Vector3.Distance(target, position);
            float arcPosition = currDist / distance;
            float yOffset = (-(arcPosition - 1) * arcPosition) * amplitude;
            currentPos = position + new Vector3(0,yOffset,0);
            return new Vector3(0f, yOffset, 0f);
        }

        Quaternion BulletType.GetVisualRotation(Quaternion rotation)
        {
            Vector3 diff = currentPos - prevPosition;
            return Quaternion.Euler(0, 0, Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg);
        }
    }
}

