using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIHover : MonoBehaviour,IPointerDownHandler,IPointerUpHandler
{
    public RectTransform hoverGameObject;
    public RectTransform draggableTransform;
    public CanvasScaler scaler;
    bool grabbing = false;
    public void Update()
    {
        if (grabbing)
        {
            float x = Input.mousePosition.x * scaler.referenceResolution.x/Screen.width;
            float y = Input.mousePosition.y * scaler.referenceResolution.y/Screen.height;
            hoverGameObject.anchoredPosition = new Vector3(x,y) - new Vector3(draggableTransform.anchoredPosition.x, draggableTransform.anchoredPosition.y);
        }
    }
    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        grabbing = true;
    }
    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        grabbing = false;
    }
}
