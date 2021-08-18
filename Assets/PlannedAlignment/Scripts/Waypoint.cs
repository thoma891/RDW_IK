using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public class Waypoint
{

    public bool show = true;

    public Transform transform;

    [Range(0.8f, 1.4f)]
    public float gain_rotation = 1.0f;

    [Range(0.8f, 1.4f)]
    public float gain_translation = 1.0f;

    public Waypoint()
    {
        gain_rotation = 1.0f;
        gain_translation = 1.0f;
    }

    public Waypoint(Transform transform)
    {
        this.transform = transform;
        gain_rotation = 1.0f;
        gain_translation = 1.0f;
    }

    public Waypoint Clone()
    {
        Waypoint waypoint = new Waypoint();

        waypoint.transform = this.transform;
        waypoint.gain_rotation = this.gain_rotation;
        waypoint.gain_translation = this.gain_translation;

        return waypoint;
    }
}