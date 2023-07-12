using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float speed = 3f;
    public float zoomScale = 0.2f;
    public TileMap.MapController controller;

    public GameObject MapBound;
    public GameObject MapBound2;
    private void FixedUpdate()
    {
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Vector2 scroll = Input.mouseScrollDelta * zoomScale;
        transform.position +=movement * speed * Time.deltaTime;
        float newX = 0;
        float newY = 0;
        Vector3 upperRight = new Vector3(Camera.main.pixelWidth, Camera.main.pixelHeight);
        float oldSize = Camera.main.orthographicSize;
        Camera.main.orthographicSize = Camera.main.orthographicSize + scroll.y;
        if (Camera.main.orthographicSize<5)
        {
            Camera.main.orthographicSize = oldSize;
        }
        if (Camera.main.ScreenToWorldPoint(upperRight).x - Camera.main.ScreenToWorldPoint(Vector3.zero).x >= controller.mapSizeX)
        {
            Camera.main.orthographicSize = oldSize;
        }
        if (Camera.main.ScreenToWorldPoint(upperRight).y - Camera.main.ScreenToWorldPoint(Vector3.zero).y >= controller.mapSizeY)
        {
            Camera.main.orthographicSize = oldSize;
        }
        if (Camera.main.ScreenToWorldPoint(Vector3.zero).x < MapBound.transform.position.x)
        {
            newX = MapBound.transform.position.x - Camera.main.ScreenToWorldPoint(Vector3.zero).x;
        }
        if (Camera.main.ScreenToWorldPoint(upperRight).x > MapBound2.transform.position.x)
        {
            newX = MapBound2.transform.position.x - Camera.main.ScreenToWorldPoint(upperRight).x;
        }
        if (Camera.main.ScreenToWorldPoint(Vector3.zero).y < MapBound.transform.position.y)
        {
            newY = MapBound.transform.position.y - Camera.main.ScreenToWorldPoint(Vector3.zero).y;
        }
        if (Camera.main.ScreenToWorldPoint(upperRight).y > MapBound2.transform.position.y)
        {
            newY = MapBound2.transform.position.y - Camera.main.ScreenToWorldPoint(upperRight).y;
        }
        transform.position = transform.position + new Vector3(newX,newY);
    }
}
