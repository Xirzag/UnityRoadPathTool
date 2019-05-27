using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour {

    public RoadTool roadTool;
    public float speed = 20;
    public float rotationSpeed = 20;
    public Vector3 offset = Vector3.up;
    public int startNode = 0;
    public bool forward = true;

    private Vector3[] path;
    private int part;
    private int sense;
    private Vector3 pathPosition;
    private bool cantDrive = false;

    void Start() {
        ReloadAll();
    }

    void Update() {

        if(cantDrive)
            return;

        Vector3 pos = pathPosition;
        Vector3 actual = path[part];
        Vector3 next = path[part + sense];

        float moveAmount = speed * Time.deltaTime;
        float distanceRemaining = Vector3.Distance(pos, next);

        while(distanceRemaining <= moveAmount) {
            moveAmount -= distanceRemaining;

            part += sense;

            if(part + sense == path.Length || part + sense == -1) {
                sense *= -1;
            }

            actual = next;
            next = path[part + sense];

            pos = actual;

            distanceRemaining = Vector3.Distance(actual, next);

        }

        Vector3 direction = (next - actual).normalized;

        pathPosition = pos + direction * moveAmount;
        transform.position = pathPosition + transform.TransformVector(offset) + roadTool.upOffset * Vector3.up;
        transform.localRotation = Quaternion.Lerp(transform.localRotation,
                                        Quaternion.LookRotation(direction), 
                                        rotationSpeed * Time.deltaTime);

    }

    public void ReloadAll() {
        path = roadTool.GetPath();
        cantDrive = path.Length < 2;

        if(cantDrive)
            return;
        

        transform.position = path[startNode] + transform.TransformVector(offset) + roadTool.upOffset * Vector3.up;
        pathPosition = path[startNode];
        part = startNode;

        if(part == 0) sense = 1;
        else if(part == path.Length - 1) sense = -1;
        else sense = forward? 1 : -1;
    }

    public void ReloadPath() {
        path = roadTool.GetPath();
        if(path.Length > 1 && cantDrive) {
            ReloadAll();
        }
            
    }

}