using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHover : MonoBehaviour
{
    public GameObject hoverGameObject;
    private void OnMouseDown()
    {
        hoverGameObject.transform.position = Camera.main.ViewportToScreenPoint(Input.mousePosition);
    }
    public void MoveObject()
    {
        hoverGameObject.transform.position = Camera.main.ViewportToScreenPoint(Input.mousePosition);
    }
}
