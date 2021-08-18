using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;
using System.IO;
      
public class SceneGUI : EditorWindow
{
    private static DateTime experimentStartTime;
    private static List<Vector3> startingPath = null;
    private static PathSegment curPath = null;
    private static string curExperiment;
    private static Scene curScene;

    private static void DumpData()
    {

        string startingPathString = "[";
        foreach (Vector3 v in startingPath)
        {
            startingPathString += "[" + v.x + ", " + v.y + ", " + v.z + "],";
        }
        startingPathString += "]";

        string curPathString = "";
        foreach (Waypoint w in curPath.virtual_waypoints)
        {
            Vector3 v = w.transform.localPosition;
            curPathString += "[" + v.x + ", " + v.y + ", " + v.z + "],";
        }
        curPathString += "]";

        var jsonObject = new {
            Experiment = "\"" + curExperiment + "\"",
            StartingTime = ((DateTimeOffset)experimentStartTime).ToUnixTimeSeconds(),
            FinalTime = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds(),
            PathError = PathError(),
            TargetError = curPath.distanceToTarget,
            Resets = curPath.nResets,
            StartingPath = startingPathString,
            FinalPath = curPathString,
        };

        StreamWriter writer = new StreamWriter(Application.dataPath + "/../Data/" + curExperiment + ".json", false);
        writer.Write(jsonObject.ToString());
        writer.Close();
    }

    private static List<Vector3> easyPath1 = new List<Vector3>
        {
            new Vector3(5.0f, 0.0f, 0.5f),
            new Vector3(0.0f, 0.0f, 0.0f),
            new Vector3(-0.5f, 0.0f, 5.0f),
            new Vector3(-10.0f, 0.0f, 0.0f),
        };

    private static List<Vector3> easyPath2 = new List<Vector3>
        {
            new Vector3(5.0f, 0.0f, 0.5f),
            new Vector3(0.0f, 0.0f, 0.0f),
            new Vector3(-0.5f, 0.0f, 5.0f),
            new Vector3(-10.0f, 0.0f, 0.0f),
        };


    private static float PathError()
    {
        float error = 0.0f;

        for (int i=0; i<startingPath.Count; i++)
        {
            error += Vector3.Distance(startingPath[i], curPath.virtual_waypoints[i].transform.localPosition);
        }

        return error;
    }


    private static List<Waypoint> CreateVirtualPath(Transform parent, List<Vector3> waypointPositions)
    {

        List<Waypoint> virtualWaypoints = new List<Waypoint>();

        for (int i=0; i<waypointPositions.Count; i++)
        {
            GameObject tmpWaypoint = new GameObject();
            GameObject waypoint = Instantiate(tmpWaypoint, waypointPositions[i], Quaternion.identity, parent);
            GameObject.DestroyImmediate(tmpWaypoint);
            waypoint.name = "Waypoint" + (i + 1);
            virtualWaypoints.Add(new Waypoint(waypoint.transform));
        }

        return virtualWaypoints;
    }

    private static void PopulateExperiment(string experiment, List<Vector3> path, bool isAutomatic)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        scene.name = "Experiment Permutation " + experiment;
        SceneView.duringSceneGui -= OnScene;

        GameObject physicalEnvironment = new GameObject("PhysicalEnvironment");

        GameObject tmpProxyObject = new GameObject();
        GameObject proxyObject = Instantiate(tmpProxyObject, new Vector3(0.0f, 0.0f, 5.0f), Quaternion.identity, physicalEnvironment.transform);
        GameObject.DestroyImmediate(tmpProxyObject);
        proxyObject.name = "ProxyObject";

        GameObject tmpUserStart = new GameObject();
        GameObject userStart = Instantiate(tmpUserStart, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity, physicalEnvironment.transform);
        GameObject.DestroyImmediate(tmpUserStart);
        userStart.name = "UserStart";

        Selection.activeGameObject = physicalEnvironment;
        //EditorWindow.focusedWindow.SendEvent(new Event { keyCode = KeyCode.RightArrow, type = EventType.KeyDown });
        

        physicalEnvironment.AddComponent<PhysicalEnvironment>();
        physicalEnvironment.GetComponent<PhysicalEnvironment>().UserStartingTransform = userStart.transform;

        GameObject virtualPath = new GameObject("VirtualPath");
        //EditorGUIUtility.PingObject(virtualPath);

        virtualPath.AddComponent<PathSegment>();
        PathSegment pathSegment = virtualPath.GetComponent<PathSegment>();
        pathSegment.isAutomatic = isAutomatic;
        pathSegment.physicalEnvironment = physicalEnvironment.GetComponent<PhysicalEnvironment>();
        pathSegment.start = userStart.transform;
        pathSegment.target = proxyObject.transform;
        pathSegment.virtual_waypoints = CreateVirtualPath(virtualPath.transform, path);


        Selection.objects = new UnityEngine.Object[] { physicalEnvironment, virtualPath };

        
        //EditorWindow.focusedWindow.SendEvent(new Event { keyCode = KeyCode.RightArrow, type = EventType.KeyDown });
        Selection.activeGameObject = virtualPath;

        SceneView.duringSceneGui += OnScene;

        experimentStartTime = DateTime.UtcNow;
        startingPath = path;
        curPath = pathSegment;
        curExperiment = experiment;
        curScene = scene;
    }

    [MenuItem("Experiment/Start A_1_1")]
    public static void A11()
    {
        PopulateExperiment("A_1_1", easyPath1, true);      
    }

    [MenuItem("Experiment/Start A_1_2")]
    public static void A12()
    {
        PopulateExperiment("A_1_2", easyPath1, false);
    }
      
    private static void OnScene(SceneView sceneview)
    {
        

        Handles.BeginGUI();
        if (GUILayout.Button("Finish"))
        {
            DumpData();
            EditorSceneManager.SaveScene(curScene, Application.dataPath + "/../Data/" + curExperiment + ".unity");
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EditorSceneManager.CloseScene(curScene, true);
            SceneView.duringSceneGui -= OnScene;
        }

        Handles.EndGUI();
    }
}
