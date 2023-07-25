using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animator : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public List<Sprite> currentAnimation;
    public float playSpeed = 1f;
    public int nextSprite = 0;
    public bool loop = true;
    public void SetSpriteList(List<Sprite> sprites,float speed,bool loop=true)
    {
        currentAnimation = sprites;
        playSpeed = speed;
        this.loop = loop;
        StopAllCoroutines();
        StartCoroutine(PlayAnimation());
    }

    IEnumerator PlayAnimation()
    {
        while (nextSprite<currentAnimation.Count)
        {
            spriteRenderer.sprite = currentAnimation[nextSprite];
            nextSprite++;
            if (nextSprite>=currentAnimation.Count && loop)
            {
                nextSprite = 0;
            }
            yield return new WaitForSeconds(1/playSpeed);
        }
    }
}
