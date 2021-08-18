using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class ManualVirtualWaypoint
{
    public Transform transform;
    [Range(0.8f, 1.4f)]
    public float gain_rotation;
    [Range(0.8f, 1.4f)]
    public float gain_translation;

    public ManualVirtualWaypoint()
    {
        gain_rotation = 1.0f;
        gain_translation = 1.0f;
    }
}

[System.Serializable]
public class ManualVirtualPathSegment
{
    public bool draw = true;
    public Color virtualPathColor = Color.cyan;
    public Color physicalPathColor = Color.red;
    public Color errorColor = Color.yellow;
    public List<ManualVirtualWaypoint> virtual_waypoints;
    public Transform target;
    public float distanceToTarget = Mathf.Infinity;


    public ManualVirtualPathSegment()
    {
        virtual_waypoints = new List<ManualVirtualWaypoint>();
    }
}


[ExecuteInEditMode]
public class Manual : MonoBehaviour
{
    [SerializeField]
    private List<ManualVirtualPathSegment> virtual_path_segments;
    
    [SerializeField]
    private bool draw_virtual = true;
    
    [SerializeField]
    private float physical_range_x = 0.0f;
    
    [SerializeField]
    private float physical_range_y = 0.0f;
    
    [SerializeField]
    private Transform user_start;
    
    [SerializeField]
    private bool resets = true;
    
    [SerializeField]
    private int num_resets = 0;

     
    void Awake()
    {
        virtual_path_segments = new List<ManualVirtualPathSegment>();
    }

    void Update()
    {
        DrawVirtualPath(); 
        CalculatePhysical();
        DrawPhysicalBoundaries();
    }
    
    void CalculatePhysical()
    {

        List<Vector3> physical_waypoints = new List<Vector3>();
        	
		Vector3 pos = Vector3.zero;
	    float rot = 0.0f;
        
        for (int i=0; i<virtual_path_segments.Count; i++)
        {
            
		    physical_waypoints.Clear();
		    physical_waypoints.Add(new Vector3(pos.x, pos.y, pos.z));

            ManualVirtualPathSegment segment = virtual_path_segments[i];
            
            if (segment.virtual_waypoints.Count < 2)
                continue;
            
            int j;
            for (j=0; j<segment.virtual_waypoints.Count-1; j++)
            {
                Transform w0 = segment.virtual_waypoints[j].transform;
        	    Transform w1 = segment.virtual_waypoints[j+1].transform;

        	    // Get angle between cur and next
            	float theta = Mathf.Atan2(w1.transform.position.z - w0.transform.position.z,
            						      w1.transform.position.x - w0.transform.position.x);
         		theta *= segment.virtual_waypoints[j+1].gain_rotation;
          		
          		float distance = Vector3.Distance(w0.position, w1.position);
          		distance *= segment.virtual_waypoints[j+1].gain_translation;
          		
          		rot += theta;
          		pos = pos + new Vector3(Mathf.Cos(theta) * distance, 0.0f, Mathf.Sin(theta) * distance);

                
          		
          		physical_waypoints.Add(new Vector3(pos.x, pos.y, pos.z)); 
            }
            
            // Draw physical path
            for (j=0; j<physical_waypoints.Count-1; j++)
            {
            	
            	if (!resets)
            	{
            	    if (segment.draw)
            	    {
                	    Debug.DrawLine(physical_waypoints[j],
                		    		   physical_waypoints[j+1],
                		    		   segment.physicalPathColor);
                		continue;
                	}
                }
                
                Vector3 p = Vector3.zero;
                bool out_of_bounds = false;
                float x = physical_range_x / 2.0f;
                float y = physical_range_y / 2.0f;
                
                for (float t=0.01f; t<=1.0f; t+=0.01f)
                {
                    p = (physical_waypoints[j] * (1.0f - t)) + (physical_waypoints[j+1] * t);
                    
                    if (p.x <= -x || p.x >= x)
                    {
                        out_of_bounds = true;
                        break;
                    }
                    
                    if (p.z <= -y || p.z >= y)
                    {
                        out_of_bounds = true;
                        break;
                    }
                    
                }
                
                // If not out of bounds, draw path segment as normal
                if (!out_of_bounds && segment.draw)
                {
                    Debug.DrawLine(physical_waypoints[j],
            		    		   physical_waypoints[j+1],
            		    		   segment.physicalPathColor);
                }
                
                else
                {
                    // Draw path segment up to reset
                    if (segment.draw)
                    {
                        Debug.DrawLine(physical_waypoints[j],
                		    		   p,
                		    		   segment.physicalPathColor);
                    }
            		
            		// Calculate reset rotation
            		float theta = Vector3.Angle(physical_waypoints[j+1] - p, Vector3.zero - p) * Mathf.Deg2Rad;
            		
            		    		   
            		for (int k=j+1; k<physical_waypoints.Count; k++)
            		{
            		    Vector3 q = physical_waypoints[k] - p;
            		    
            		    float sin = Mathf.Sin(theta);
            		    float cos = Mathf.Cos(theta);
            		    
            		    q = new Vector3(q.x * cos - q.z * sin, 0.0f, q.x * sin + q.z * cos);
            		    
            		    physical_waypoints[k] = q + p;
            		}
            		
            		// Draw path segment from reset
            		if (segment.draw)
            		{
                        Debug.DrawLine(p,
                		    		   physical_waypoints[j+1],
                		    		   segment.physicalPathColor);
                	}
                }
            }
            
            pos = physical_waypoints[physical_waypoints.Count-1];
            rot = Vector3.Angle(new Vector3(1.0f, 0.0f, 0.0f), pos - physical_waypoints[physical_waypoints.Count-2]);

            if (segment.target != null)
            {
      		    if (segment.draw)
      		    {
          		    Debug.DrawLine(pos,
            		    		   segment.target.position,
            		    		   segment.errorColor);
                }
      		    segment.distanceToTarget = Vector3.Distance(pos, segment.target.position);
      		}

        }
        
        
            
        
    }
    
    void UpdateWaypoints()
    {
        //for (int i=0; i<virtual_waypoints.Count-1; i++)
        //{
            //Debug.DrawLine(virtual_waypoints[i].position,
            //               virtual_waypoints[i+1].position);
                          
        //}
    }
    
    public void DrawVirtualPath()
    {
            
        for (int i=0; i<virtual_path_segments.Count; i++)
        {
            ManualVirtualPathSegment segment = virtual_path_segments[i];
            
            if (!segment.draw)
                continue;
            
            //for (int j=0; j<segment.virtual_waypoints.Count; j++)
            //{
            //    VirtualWaypoint waypoint = segment.virtual_waypoints[j];
            //    if (waypoint.transform != null)
            //    {
            //        Handles.Label(waypoint.transform.position, string.Format("{0}-{1}", i, j));
            //    }
            //}
            
            for (int j=0; j<segment.virtual_waypoints.Count-1; j++)
            {
                if (segment.virtual_waypoints[j].transform != null &&
                    segment.virtual_waypoints[j+1].transform != null)
                {
                    Debug.DrawLine(segment.virtual_waypoints[j].transform.position,
                                   segment.virtual_waypoints[j+1].transform.position,
                                   segment.virtualPathColor);
                }
            }
        }
        
        if (virtual_path_segments.Count >= 2)
        {
		    for (int i=0; i<virtual_path_segments.Count-1; i++)
		    {
		        List<ManualVirtualWaypoint> waypoints0 = virtual_path_segments[i].virtual_waypoints;
		        List<ManualVirtualWaypoint> waypoints1 = virtual_path_segments[i+1].virtual_waypoints;
		        
		        if (waypoints0.Count > 0 && waypoints1.Count > 0 &&
		            waypoints0[waypoints0.Count-1].transform != null &&
		            waypoints1[0].transform != null)
		        {
		            Debug.DrawLine(waypoints0[waypoints0.Count-1].transform.position,
		                           waypoints1[0].transform.position,
		                           virtual_path_segments[i].virtualPathColor);
		        }
		    }
		}
    }
    
    void DrawPhysicalBoundaries()
    {
        float x = physical_range_x / 2.0f;
        float y = physical_range_y / 2.0f;
        
        Debug.DrawLine(new Vector3(-x, 0.0f, y),
                       new Vector3(x, 0.0f, y),
                       Color.green);
        Debug.DrawLine(new Vector3(x, 0.0f, y),
                       new Vector3(x, 0.0f, -y),
                       Color.green);
        Debug.DrawLine(new Vector3(x, 0.0f, -y),
                       new Vector3(-x, 0.0f, -y),
                       Color.green);
        Debug.DrawLine(new Vector3(-x, 0.0f, -y),
                       new Vector3(-x, 0.0f, y),
                       Color.green);
    }
    
    public void AddPathSegment(int index)
    {
        virtual_path_segments.Insert(index, new ManualVirtualPathSegment());
    }
    
    public void RemovePathSegment(int index)
    {
        virtual_path_segments.RemoveAt(index);
    }
    
    public void SwapPathSegments(int i, int j)
    {
        ManualVirtualPathSegment tmp = virtual_path_segments[i];
        virtual_path_segments[i] = virtual_path_segments[j];
        virtual_path_segments[j] = tmp;
    }
}
