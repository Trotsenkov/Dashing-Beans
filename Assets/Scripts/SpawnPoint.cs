using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    static List<Vector3> points = new List<Vector3>();
    static List<Vector3> Usedpoints = new List<Vector3>();

    void Awake()
    {
        points.Add(transform.position);
        Destroy(gameObject);
    }

    public static Vector3 Get()
    {
        Vector3 point = points[Random.Range(0, points.Count)];
        Usedpoints.Add(point);
        points.Remove(point);
        return point;
    }
    public static void Restart()
    {
        points.AddRange(Usedpoints);
        Usedpoints = new List<Vector3>();
    }
}