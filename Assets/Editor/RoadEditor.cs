using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEditor;
using UnityEngine.UIElements;

[CustomEditor(typeof(RoadTool))]
public class LevelScriptEditor : Editor 
{

    private bool editMode = false; 
    private int hashCode;
    int controlID;
    
    private int selected = -1;
    private bool dragged = false;

    public bool showPath = false;

    private const float selectThreshold = 1;
    private const float lineThreshold = 2;

    RoadTool roadTool;    

    void Start()
    {
        hashCode = GetHashCode();
        
        controlID = GUIUtility.GetControlID(hashCode, FocusType.Passive);
    }


     
    public override void OnInspectorGUI()
    {
        roadTool = (RoadTool)target;

        EditorGUILayout.LabelField("Points", roadTool.points.Count.ToString());
        if(GUILayout.Button("Remove Points")) ClearPath();
        if(GUILayout.Button("Remove Last Point")) RemoveLastPoint();

        if(GUILayout.Button(!editMode? "Edit Path" : "Exit edit path mode")) ToggleEditMode();

        float upOffset = EditorGUILayout.FloatField("Up offset", roadTool.upOffset);
        float pathWidth = EditorGUILayout.FloatField("Path Width", roadTool.pathWidth);
        float uvs = EditorGUILayout.FloatField("Uv Length", roadTool.uvsY);

        if(upOffset != roadTool.upOffset 
            || pathWidth != roadTool.pathWidth
            || uvs != roadTool.uvsY) {
            roadTool.upOffset = upOffset;
            roadTool.pathWidth = pathWidth;
            roadTool.uvsY = uvs;
            roadTool.CreateMesh();
        }

    }

    public void OnSceneGUI()
    {
        roadTool = (RoadTool)target;

        Event e = Event.current;
        
        if (editMode && e.button == 0)
        {
            bool used = false;

            if(e.type == EventType.MouseDown) {
                ManageClick(e.mousePosition);
                used = true;
            }

            if(e.type == EventType.MouseDrag) {
                ManageDrag(e.mousePosition);
                used = true;
            }
            
            if(e.type == EventType.MouseUp) {
                ManageMouseUp(e.mousePosition);
                used = true;
            }

            if(used) {
                e.Use();  
                SceneView.RepaintAll();
            }

        }


        if (e.type == EventType.Layout && editMode) {
            HandleUtility.AddDefaultControl(controlID);
        }

    }

    private void ClearPath() {
        roadTool.ClearPath();
        SceneView.RepaintAll();
    }

    private void ToggleEditMode(){
        editMode = !editMode;
        roadTool.showPath = editMode;
        HandleUtility.AddDefaultControl(editMode? controlID : 0);

    }

    private void RemoveLastPoint() {
        roadTool.RemoveLast();
        SceneView.RepaintAll();
    }

    
    public void ManageDrag(Vector2 mousePosition)
    {
        if(selected == -1) return;

        Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit, Mathf.Infinity)){
            dragged = true;
            roadTool.MovePoint(selected, hit.point);
        }

    }

    
    public void ManageClick(Vector2 mousePos){
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit, Mathf.Infinity)){
            int posiblePoint;
            if(isNearPoint(hit.point, out posiblePoint)) {
                
                SelectPoint(posiblePoint);
                return;
            }

            if(IsBetweenPoints(hit.point, out posiblePoint)) {
                posiblePoint++;
                roadTool.InsertPoint(hit.point, posiblePoint);
                SelectPoint(posiblePoint);
                return;
            }

            roadTool.AddPoint(hit.point);

        }
        
    }

    public void ManageMouseUp(Vector2 mousePosition)
    {
        if(!dragged && selected != -1)
            roadTool.RemovePoint(selected);

        SelectPoint(-1);
        dragged = false;

    }
    
    public void SelectPoint(int index) {
        selected = index;
        roadTool.selected = index;
    }
    
    private bool IsBetweenPoints(Vector3 point, out int index){
        for(int i = 0; i < roadTool.points.Count -1; i++) {
            Vector3 start = roadTool.transform.TransformPoint(roadTool.points[i]);
            Vector3 direction = roadTool.transform.TransformVector(roadTool.points[i + 1] - roadTool.points[i]);
                                
            Vector3 nearestPoint;
            if(!NearestPointOnLine(start, direction, point, out nearestPoint)) 
                continue;

            float distance = Vector3.Distance(nearestPoint, point);
            

            if(distance < lineThreshold) {
                index = i;
                return true;
            }
        }
        index = -1;
        return false;
    }

    public bool NearestPointOnLine(Vector3 linePnt, Vector3 lineDir, Vector3 pnt, out Vector3 nearest)
    {
        var dir = lineDir.normalized;
        var v = pnt - linePnt;
        var d = Vector3.Dot(v, dir);
        nearest = linePnt + dir * d;
        return d >= 0 && d <= lineDir.magnitude;/*new Bounds(linePnt + lineDir/2.0f, lineDir).Contains(nearest);*/
    }


    private bool isNearPoint(Vector3 point, out int posiblePoint)
    {
        float threshold = 1;
        for(int i = 0; i < roadTool.points.Count; i++){
            if(Vector3.Distance(roadTool.transform.TransformPoint(roadTool.points[i]), point) < threshold) {
                posiblePoint = i;
                return true;
            }
        }
        posiblePoint = -1;
        return false;
    }

}