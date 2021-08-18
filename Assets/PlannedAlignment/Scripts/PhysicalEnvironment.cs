using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class PhysicalEnvironment : MonoBehaviour
{
    [SerializeField]
    private float physicalRangeX = 10.0f;
    public float PhysicalRangeX { get { return physicalRangeX; } set { physicalRangeX = value; } }

    [SerializeField]
    private float physicalRangeY = 10.0f;
    public float PhysicalRangeY { get { return physicalRangeY; } set { physicalRangeY = value; } }

    [SerializeField]
    private Transform userStartingTransform;
    public Transform UserStartingTransform { get; set; }

    [SerializeField]
    private Color physicalEnvironmentColor = Color.green;
    public Color PhysicalEnvironmentColor { get { return physicalEnvironmentColor; } }


    // Update is called once per frame
    void Update()
    {
        float x = physicalRangeX / 2.0f;
        float y = physicalRangeY / 2.0f;

        Debug.DrawLine(transform.position - new Vector3(-x, 0.0f, y),
                       transform.position - new Vector3(x, 0.0f, y),
                       physicalEnvironmentColor);
        Debug.DrawLine(transform.position - new Vector3(x, 0.0f, y),
                       transform.position - new Vector3(x, 0.0f, -y),
                       physicalEnvironmentColor);
        Debug.DrawLine(transform.position - new Vector3(x, 0.0f, -y),
                       transform.position - new Vector3(-x, 0.0f, -y),
                       physicalEnvironmentColor);
        Debug.DrawLine(transform.position - new Vector3(-x, 0.0f, -y),
                       transform.position - new Vector3(-x, 0.0f, y),
                       physicalEnvironmentColor);
    }
}
