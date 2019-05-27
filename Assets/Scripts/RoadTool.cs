using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class RoadTool : MonoBehaviour
{

    public List<Vector3> points = new List<Vector3>();
    public float upOffset = 0;
    public float pathWidth = 1;
    public float uvsY = 1;


    public Vector3[] GetPath() {
        Vector3[] pointsInWorld = new Vector3[points.Count];
        for(int i = 0; i < points.Count; i++) {
            pointsInWorld[i] = transform.TransformPoint(points[i]);
        }
        
        return pointsInWorld;
    }

    public void ClearPath(){
        points.Clear();
        GetComponent<MeshFilter>().mesh = new Mesh();
    }

    public void AddPoint(Vector3 position) {
        points.Add(transform.InverseTransformPoint(position));
        CreateMesh();
    }

    public void InsertPoint(Vector3 position, int index) {
        points.Insert(index, transform.InverseTransformPoint(position));
        CreateMesh();
    }

    public void MovePoint(int index, Vector3 newPosition) {
        points[index] = transform.InverseTransformPoint(newPosition);
        CreateMesh();
    }

    public void RemovePoint(int indexToRemove)
    {
        points.RemoveAt(indexToRemove);
        CreateMesh();
    }

    public void RemoveLast()
    {
        if(points.Count > 0)
            points.RemoveAt(points.Count -1);

        CreateMesh();
    }

    public void CreateMesh() {

        if(points.Count < 2) {
            GetComponent<MeshFilter>().mesh = new Mesh();  
            return;
        }

        Mesh mesh = new Mesh(); 

        Vector3[] vertices = new Vector3[points.Count * 2];
        Vector3[] normals = new Vector3[points.Count * 2];
        int[] triangles = new int[(points.Count - 1) * 2 * 3];
        Vector2[] uvs = new Vector2[points.Count * 2];

        float distance = 0;

        for(int i = 0; i < points.Count; i++) {

            Vector3 forward = Vector3.zero;
            Vector3 back = Vector3.zero;


            if(i != 0) 
                back = points[i] - points[i - 1];
            
            if(i != points.Count - 1) 
                forward = points[i] - points[i + 1];
            
            Vector3 midForwad = (forward.normalized - back.normalized).normalized;

            Vector3 widthOffset = new Vector3(-midForwad.z, 0, midForwad.x);

            vertices[i * 2] = points[i] 
                + widthOffset * pathWidth
                + Vector3.up * upOffset;
            vertices[i * 2 + 1] = points[i] 
                + -widthOffset * pathWidth
                + Vector3.up * upOffset;

            normals[i * 2] = Vector3.up;
            normals[i * 2 + 1] = Vector3.up;

            uvs[i * 2] = new Vector2(0, distance * uvsY);
            uvs[i * 2 + 1] = new Vector2(1, distance * uvsY);

            distance += forward.magnitude;

        }

        for(int i = 0; i < points.Count - 1; i++) {
            int p0 = i * 2;
            int p1 = i * 2 + 1;
            int p2 = i * 2 + 2;
            int p3 = i * 2 + 3;

            // First triangle
            triangles[i * 6 + 0] = p0;
            triangles[i * 6 + 1] = p1;
            triangles[i * 6 + 2] = p3;

            // Second triangle
            triangles[i * 6 + 3] = p0;
            triangles[i * 6 + 4] = p3;
            triangles[i * 6 + 5] = p2;
        }

        mesh.name = "Roads";
        mesh.vertices = vertices; 
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uvs;

        GetComponent<MeshFilter>().mesh = mesh; 

    }


    public bool showPath = false;
    public int selected = -1;

    #if UNITY_EDITOR
    void OnDrawGizmos() {
        if (showPath && Event.current.type == EventType.Repaint)
        {
            Gizmos.color = Color.red;
            Handles.color = Color.blue;

            Vector3[] points = GetPath();
            for(int i = 0; i < points.Length; i++) {
                if(i > 0) {
                    Gizmos.DrawLine(points[i - 1] + Vector3.up * (upOffset + 1),
                                    points[i] + Vector3.up * (upOffset + 1));
                }
                
                if(i == selected) {
                    Handles.color = Color.yellow;
                }
                Handles.DotHandleCap(0, points[i],
                                    Quaternion.identity, 1, EventType.Repaint);
                if(i == selected) {
                    Handles.color = Color.blue;
                }
            }

            Handles.color = Color.cyan;
        }

    }
    #endif

}
