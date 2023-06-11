using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
    public TMPro.TMP_Dropdown tileDropdown;
    public TileMap.MapTile tileHandle;

    [SerializeField]
    private TileMap.MapController controller;

    Vector3 startingPoint = Vector3.zero;
    Vector3 endPoint = Vector3.zero;

    bool startPainting = false;
    void Update()
    {
        switch (mode)
        {
            case CursorMode.None:
                if (Input.GetMouseButtonDown(0))
                {
                    ClearSelected();
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
                    if (units.Count>0)
                    {
                        ClearSelected();
                        selected.AddRange(units);
                        foreach (Selectable item in units)
                        {
                            selected.Add(item);
                            item.OnSelect();
                        }
                    }
                    mode = CursorMode.None;
                }
                else
                {
                    endPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                }
                break;
            case CursorMode.PlacementMode:
                if (Input.GetMouseButtonDown(0))
                {
                    startPainting = true;
                }
                if (Input.GetMouseButtonUp(0))
                {
                    startPainting = false;
                }
                if (Input.GetMouseButtonDown(1))
                {
                    mode = CursorMode.None;
                }
                if (startPainting)
                {
                    Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    (int, int) coords = controller.GetMapTileFromWorldPosition(point);
                    controller.PlaceTile(tileHandle, coords.Item1, coords.Item2);
                }
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
    public void BeginTilePlacementMode()
    {
        tileHandle = controller.FindTile(tileDropdown.options[tileDropdown.value].text);
        mode = CursorMode.PlacementMode;

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
