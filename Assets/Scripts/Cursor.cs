using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    public enum CursorMode{
        None,
        Selector,
        PlacementMode
    }
    public CursorMode mode;
    public List<Selectable> selected = new List<Selectable>();
    public Placeable placeable;

    [SerializeField]
    private TileMap.MapController controller;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    bool startedSelection = false;
    Vector3 startingPoint = Vector3.zero;
    Vector3 endPoint = Vector3.zero;
    void Update()
    {
        switch (mode)
        {
            case CursorMode.None:
                if (Input.GetMouseButtonDown(0))
                {
                    startedSelection = true;
                    startingPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    mode = CursorMode.Selector;
                    RaycastHit2D raycastHit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                    if (raycastHit.collider != null)
                    {
                        Selectable selectable;
                        if(raycastHit.collider.gameObject.TryGetComponent<Selectable>(out selectable))
                        {
                            selectable.OnSelect();
                            selected.Add(selectable);
                        }
                    }
                }
                else
                {
                    //TODO: Handle changing cursor sprites
                }
                if (Input.GetMouseButtonDown(1))
                {
                    foreach (Selectable item in selected)
                    {
                        item.OnAction(Input.mousePosition);
                    }
                }
                break;
            case CursorMode.Selector:
                if (Input.GetMouseButtonUp(0))
                {
                    endPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    //Units take priority in selection
                    List<Unit> units = controller.GetUnitsInAreaOfFaction(startingPoint, endPoint, "Player");
                    if (units != null)
                    {
                        ClearSelected();
                        selected.AddRange(units);
                        foreach (Selectable item in units)
                        {
                            selected.Add(item);
                            item.OnSelect();
                        }
                    }
                    startedSelection = false;
                    mode = CursorMode.None;
                }
                else
                {
                    endPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                }
                break;
            case CursorMode.PlacementMode:
                break;
        }
    }
    void ClearSelected()
    {
        foreach (Selectable item in selected)
        {
            item.Unselect();
        }
        selected.Clear();
    }
}
public interface Selectable
{
    void OnSelect();
    void OnAction(Vector3 screenPostion);
    void Unselect();
}
public interface Placeable
{
    void Place(Vector3 screenPostion);
}
