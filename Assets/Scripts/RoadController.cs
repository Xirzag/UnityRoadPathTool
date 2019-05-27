using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadController : MonoBehaviour
{

    public RoadTool road;
    public Vehicle vehicle;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit, Mathf.Infinity)){
                road.AddPoint(hit.point);
                vehicle.ReloadPath();
            }
        }
    }

    public void DeletePath() {
        road.ClearPath();
        vehicle.ReloadAll();
    }

}
