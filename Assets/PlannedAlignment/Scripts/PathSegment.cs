using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[ExecuteInEditMode]
public class PathSegment : MonoBehaviour
{
    const float GAIN_ROT_LOWER = 0.8f;
    const float GAIN_ROT_UPPER = 1.4f;
    const float GAIN_TRANS_LOWER = 0.8f;
    const float GAIN_TRANS_UPPER = 1.4f;

    public bool draw = true;
    public bool resets = true;
    public PhysicalEnvironment physicalEnvironment;
    public Transform start;
    public Color virtualPathColor = Color.cyan;
    public Color physicalPathColor = Color.red;
    public Color errorColor = Color.yellow;
    public List<Waypoint> virtual_waypoints;
    public Transform target;
    public float distanceToTarget = Mathf.Infinity;

    public bool isAutomatic = false;
    public bool canChooseAutomatic = false;

    public int nResets = 0;


    public PathSegment()
    {
        virtual_waypoints = new List<Waypoint>();
    }

    public void Clone(PathSegment segment)
    {

        segment.draw = this.draw;
        segment.resets = this.resets;
        segment.physicalEnvironment = this.physicalEnvironment;
        segment.start = this.start;
        segment.virtualPathColor = this.virtualPathColor;
        segment.physicalPathColor = this.physicalPathColor;
        segment.errorColor = this.errorColor;
        segment.target = this.target;
        segment.distanceToTarget = this.distanceToTarget;

        foreach (Waypoint waypoint in this.virtual_waypoints)
        {
            segment.virtual_waypoints.Add(waypoint.Clone());
        }

    }

    public void DrawCircle(Vector3 center, float radius, Color color)
    {
        int N = 16;
        float d_theta = 2 * Mathf.PI / N;

        for (int i = 0; i < N; i++)
        {
            float theta = d_theta * i;
            Vector3 p1 = new Vector3(center.x + radius * Mathf.Cos(theta), 0.0f, center.z + radius * Mathf.Sin(theta));

            theta += d_theta;
            Vector3 p2 = new Vector3(center.x + radius * Mathf.Cos(theta), 0.0f, center.z + radius * Mathf.Sin(theta));

            Debug.DrawLine(p1, p2, color);

        }
    }

    public List<float> GainsList()
    {
        List<float> gains = new List<float>();

        foreach (Waypoint waypoint in virtual_waypoints)
        {
            gains.Add(waypoint.gain_rotation);
            gains.Add(waypoint.gain_translation);
        }

        return gains;
    }

    public void ApplyGainsList(List<float> gains)
    {
        for (int i = 0; i < gains.Count; i += 2)
        {
            virtual_waypoints[i / 2].gain_rotation = gains[i];
            virtual_waypoints[i / 2].gain_translation = gains[i + 1];
        }
    }

    public List<float> TurnAndWalkList(float initialRotation)
    {
        List<float> turnAndWalks = new List<float>();

        float rot = initialRotation;

        for (int i = 0; i < virtual_waypoints.Count - 1; i++)
        {
            float x = virtual_waypoints[i + 1].transform.position.x - virtual_waypoints[i].transform.position.x;
            float y = virtual_waypoints[i + 1].transform.position.z - virtual_waypoints[i].transform.position.z;

            float theta = Mathf.Atan2(y, x);
            float deltaTheta = theta - rot;

            while (deltaTheta <= -Mathf.PI)
                deltaTheta += Mathf.PI;
            while (deltaTheta > Mathf.PI)
                deltaTheta -= Mathf.PI;

            turnAndWalks.Add(deltaTheta);
            turnAndWalks.Add(Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(y, 2)));

            rot = theta;
        }

        return turnAndWalks;
    }

    public void ApplyTurnAndWalkList(float initialRotation, List<float> turnAndWalks)
    {
        float rot = initialRotation;

        for (int i = 0; i < virtual_waypoints.Count - 1; i++)
        {
            float x = virtual_waypoints[i + 1].transform.position.x - virtual_waypoints[i].transform.position.x;
            float y = virtual_waypoints[i + 1].transform.position.z - virtual_waypoints[i].transform.position.z;

            float theta = Mathf.Atan2(y, x);

            float rot_gain = (theta - rot) / turnAndWalks[2 * i];
            float trans_gain = Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(y, 2)) / turnAndWalks[2 * i + 1];

            //Debug.Log(rot_gain + ", " + trans_gain);

            rot = theta;

            virtual_waypoints[i + 1].gain_rotation *= rot_gain;
            virtual_waypoints[i + 1].gain_translation *= trans_gain;
        }
    }

    List<float> Normalize(List<float> gains)
    {
        List<float> output = new List<float>();

        float sum = 0.0f;

        foreach (float gain in gains)
        {
            sum += gain;
            output.Add(0.0f);
        }

        if (sum == 0.0)
        {
            return output;
        }

        for (int i = 0; i < gains.Count; i++)
        {
            output[i] = gains[i] / sum;
        }

        return output;

    }

    public string ListToString<T>(List<T> list)
    {
        string s = "[";

        foreach (T item in list)
        {
            s += item.ToString();
            s += ", ";
        }

        s += "]";

        return s;
    }

    public void ApplyNegativeGradient(float stepSize)
    {
        List<float> gainsOriginal = GainsList();
        List<float> gains = GainsList();

        List<float> gradient = new List<float>();

        for (int i = 0; i < gains.Count; i++)
        {
            int val = 33;
            if (i == val) Debug.Log("1: " + ListToString<float>(GainsList()));

            gains = GainsList();
            gains[i] += stepSize;
            ApplyGainsList(gains);
            float error1 = Error();

            if (i == val) Debug.Log("2: " + ListToString<float>(gains));
            if (i == val) Debug.Log("3: " + ListToString<float>(GainsList()));
            if (i == val) Debug.Log(error1);

            gains[i] -= 2 * stepSize;
            ApplyGainsList(gains);
            float error2 = Error();

            if (i == val) Debug.Log("4: " + ListToString<float>(gains));
            if (i == val) Debug.Log("5: " + ListToString<float>(GainsList()));
            if (i == val) Debug.Log(error2);

            gradient.Add(-1 * (error1 - error2) / (2 * stepSize));
            ApplyGainsList(gainsOriginal);
        }

        //gradient = Normalize(gradient);
        //Debug.Log(ListToString<float>(gradient));

        gains = GainsList();


        for (int i = 0; i < gains.Count; i++)
        {
            if (i % 2 == 0)
                gains[i] = Mathf.Min(Mathf.Max(gains[i] + (gradient[i] * stepSize), GAIN_ROT_LOWER), GAIN_ROT_UPPER);
            else
                gains[i] = Mathf.Min(Mathf.Max(gains[i] + (gradient[i] * stepSize), GAIN_TRANS_LOWER), GAIN_TRANS_UPPER);
        }

        ApplyGainsList(gains);
    }

    public void ApplyNegativeGradient2(float stepSize, float initial_rot)
    {
        List<float> tawsOriginal = TurnAndWalkList(initial_rot);
        List<float> taws = TurnAndWalkList(initial_rot);

        //Debug.Log(ListToString<float>(taws));


        //Debug.Log("1: " + ListToString<float>(GainsList()));

        List<float> gradient = new List<float>();

        for (int i = 0; i < taws.Count; i++)
        {
            int val = 0;
            //Debug.Log("1: " + ListToString<float>(GainsList()));

            taws = TurnAndWalkList(initial_rot);
            //Debug.Log("3: " + ListToString<float>(GainsList()));
            taws[i] += stepSize;
            ApplyTurnAndWalkList(initial_rot, taws);
            //Debug.Log("5: " + ListToString<float>(GainsList()));
            float error1 = Error();


            //if (i == val) Debug.Log("2: " + ListToString<float>(taws));

            //if (i == val) Debug.Log(error1);

            taws[i] -= 2 * stepSize;
            ApplyTurnAndWalkList(initial_rot, taws);
            float error2 = Error();

            //if (i == val) Debug.Log("4: " + ListToString<float>(taws));

            //if (i == val) Debug.Log(error2); 

            gradient.Add(-1 * (error1 - error2) / (2 * stepSize));
            ApplyTurnAndWalkList(initial_rot, tawsOriginal);

        }

        //gradient = Normalize(gradient);
        //Debug.Log("0.5: " + Error());
        //Debug.Log("1: " + ListToString<float>(gradient));
        //Debug.Log("2: " + ListToString<float>(GainsList()));

        taws = TurnAndWalkList(initial_rot);

        Debug.Log(ListToString<float>(taws));

        for (int i = 0; i < taws.Count; i++)
        {
            //taws[i] = taws[i] + (gradient[i] * stepSize);

            if (i % 2 == 0)
                taws[i] = Mathf.Min(Mathf.Max(taws[i] + (gradient[i] * stepSize), taws[i] * GAIN_ROT_LOWER), taws[i] * GAIN_ROT_UPPER);

            else
                taws[i] = Mathf.Min(Mathf.Max(taws[i] + (gradient[i] * stepSize), taws[i] * GAIN_TRANS_LOWER), taws[i] * GAIN_TRANS_UPPER);
        }

        ApplyTurnAndWalkList(initial_rot, taws);
        //Debug.Log("3: " + ListToString<float>(GainsList()));
        //Debug.Log("4: " + Error());
    }

    public void Inverse(int maxIters, float stepSize)
    {
        
        float error = float.MaxValue;

        for (int i = 0; i < maxIters; i++)
        {
            ApplyNegativeGradient2(stepSize, 0.0f);
            float newError = Error();

            if (newError >= error)
            {
                break;
            }

            error = newError;
        }
    }

    public List<Vector3> Solve()
    {
        Debug.Log(ListToString<float>(TurnAndWalkList(3.14f)));
        nResets = 0;
        if (isAutomatic)
        {
            foreach (Waypoint waypoint in virtual_waypoints)
            {
                waypoint.gain_rotation = 1.0f;
                waypoint.gain_translation = 1.0f;
            }

            Inverse(1000, 0.1f);
        }

        List<Vector3> physical_waypoints = new List<Vector3>();

        List<Vector3> forward = Forward();

        physical_waypoints.Add(forward[0]);

        // Iterate over each physical waypoint that Forward returned
        for (int i = 1; i < forward.Count; i++)
        {
            // If resets are turned on and the current waypoint is out of bounds, handle a reset...
            if (resets && !InBounds(forward[i]))
            {
                // Increment number of resets
                nResets++;

                // Get the vector that points from the last in bounds waypoint to the out of bounds waypoint
                Vector3 direction = Vector3.Normalize(forward[i] - forward[i - 1]);

                // Find the point where the user would intersect the boundary
                Vector3 pos = forward[i - 1];
                while (InBounds(pos))
                {
                    pos += direction * 0.1f;
                }

                // Add this point to the physical waypoints list
                physical_waypoints.Add(pos);


                // Calculate the point on the virtual path where the reset would be triggered
                float proportion_physical_walked = Vector3.Distance(pos, forward[i - 1]) / Vector3.Distance(forward[i], forward[i - 1]);
                float distance_virtual_walked = Vector3.Distance(virtual_waypoints[i].transform.position, virtual_waypoints[i - 1].transform.position) * proportion_physical_walked;
                Vector3 virtual_direction = Vector3.Normalize(virtual_waypoints[i].transform.position - virtual_waypoints[i - 1].transform.position);
                Vector3 virtual_reset_position = virtual_waypoints[i - 1].transform.position + (virtual_direction * distance_virtual_walked);

                //Debug.Log(virtual_reset_position);
                //Debug.DrawLine(virtual_waypoints[i - 1].transform.position, virtual_reset_position, Color.black);

                //Debug.DrawRay(virtual_waypoints[i - 1].transform.position, virtual_direction, Color.black, distance_virtual_walked);

                // Create the new path segment and reset key fields
                GameObject pathSegmentObject = new GameObject();
                pathSegmentObject.AddComponent<PathSegment>();
                PathSegment new_path = pathSegmentObject.GetComponent<PathSegment>();
                Clone(new_path);
                GameObject tmp_start = new GameObject();
                tmp_start.transform.position = pos;
                new_path.start = tmp_start.transform;
                new_path.virtual_waypoints = new List<Waypoint>();

                // We are going to need to create a bunch of new transforms for our virtual waypoint copies.
                // Unity does not let us create Transform directly, so we need to instantiate an empty game
                // object for each waypoint copy. This list will hold them so we can delete them later.
                List<GameObject> tmp_waypoint_gameobjects = new List<GameObject>();


                // Add the rest of the virtual waypoints after the reset as copies to the new path
                for (int j = i; j < virtual_waypoints.Count; j++)
                {
                    Waypoint tmp_waypoint = virtual_waypoints[j].Clone();
                    GameObject tmp_waypoint_go = new GameObject();
                    tmp_waypoint_go.transform.position = tmp_waypoint.transform.position;
                    tmp_waypoint.transform = tmp_waypoint_go.transform;


                    new_path.virtual_waypoints.Add(tmp_waypoint);
                    tmp_waypoint_gameobjects.Add(tmp_waypoint_go);
                }

                new_path.virtual_waypoints[0].transform.position = virtual_reset_position;

                // Once our new path is constructed and populated, solve it.
                // Note that this is a recursive method...
                List<Vector3> new_path_forward = new_path.Solve();

                //Debug.Log(ListToString<Vector3>(new_path_forward));

                // Add the waypoints from the new_path's solution to the already calculated physical waypoints
                physical_waypoints.AddRange(new_path_forward);

                // We needed to create a bunch of game objects in order to get fresh transforms
                // delete them here so they don't overwhelm the scene hierachy
                GameObject.DestroyImmediate(tmp_start);
                GameObject.DestroyImmediate(pathSegmentObject);
                foreach (GameObject go in tmp_waypoint_gameobjects)
                {
                    GameObject.DestroyImmediate(go);
                }

                if (draw)
                {
                    // Draw where the reset happened on the virtual path
                    DrawCircle(virtual_reset_position, 0.2f, Color.red);
                }

                break;
            }

            // Resets will always break, so if we are here just add the current waypoint to the physical
            // waypoints list
            physical_waypoints.Add(forward[i]);
        }

        return physical_waypoints;
    }

    public float Error()
    {
        List<Vector3> positions = Forward();
        return Vector3.Distance(target.position, positions[positions.Count - 1]);
    }

    public bool InBounds(Vector3 point)
    {
        Vector3 center = physicalEnvironment.transform.position;
        float width = physicalEnvironment.PhysicalRangeX;
        float height = physicalEnvironment.PhysicalRangeY;

        if (point.x < center.x - (width / 2.0f)) return false;
        if (point.x > center.x + (width / 2.0f)) return false;
        if (point.z < center.z - (height / 2.0f)) return false;
        if (point.z > center.z + (height / 2.0f)) return false;

        return true;
    }

    public List<Vector3> Forward()
    {
        List<Vector3> physical_waypoints = new List<Vector3>();
        Vector3 pos = start.position;
        //float rot = Mathf.Atan2(start.forward.z, start.forward.x);


        physical_waypoints.Add(new Vector3(pos.x, pos.y, pos.z));

        if (virtual_waypoints.Count < 2)
            return physical_waypoints;

        for (int j = 0; j < virtual_waypoints.Count - 1; j++)
        {

            Transform w0 = virtual_waypoints[j].transform;
            Transform w1 = virtual_waypoints[j + 1].transform;

            // Get angle between cur and next
            float theta = Mathf.Atan2(w1.transform.position.z - w0.transform.position.z,
                                        w1.transform.position.x - w0.transform.position.x);
            theta *= virtual_waypoints[j + 1].gain_rotation;

            float distance = Vector3.Distance(w0.position, w1.position);
            distance *= virtual_waypoints[j + 1].gain_translation;
            Vector3 direction = new Vector3(Mathf.Cos(theta), 0.0f, Mathf.Sin(theta));

            //rot += theta;
            pos += (direction * distance);

            physical_waypoints.Add(new Vector3(pos.x, pos.y, pos.z));
        }

        return physical_waypoints;
    }


    public void DrawVirtualPath()
    {
        if (!draw)
            return;

        for (int j = 0; j < virtual_waypoints.Count - 1; j++)
        {
            if (virtual_waypoints[j].transform != null &&
                virtual_waypoints[j + 1].transform != null)
            {
                Debug.DrawLine(virtual_waypoints[j].transform.position,
                               virtual_waypoints[j + 1].transform.position,
                               virtualPathColor);
            }
        }
    }

    public void DrawPhysicalPath()
    {
        if (!draw)
            return;

        List<Vector3> physical_waypoints;
        physical_waypoints = Solve();


        //Debug.Log(ListToString<Vector3>(physical_waypoints));

        for (int j = 0; j < physical_waypoints.Count - 1; j++)
        {

            Debug.DrawLine(physical_waypoints[j],
                            physical_waypoints[j + 1],
                            physicalPathColor);

        }

        Vector3 pos = physical_waypoints[physical_waypoints.Count - 1];
        //float rot = Vector3.Angle(new Vector3(1.0f, 0.0f, 0.0f), pos - physical_waypoints[physical_waypoints.Count - 2]);

        if (target != null)
        {
            Debug.DrawLine(pos,
                            target.position,
                            errorColor);
            distanceToTarget = Vector3.Distance(pos, target.position);
        }
    }

    public void Update()
    {
        DrawVirtualPath();
        DrawPhysicalPath();

        if (Event.current != null && Event.current.GetTypeForControl(GUIUtility.GetControlID(FocusType.Keyboard)) == EventType.MouseDown)
            Debug.Log("Mouse Clicked...");
    }
}
