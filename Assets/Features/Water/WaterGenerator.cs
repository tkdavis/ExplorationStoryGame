using System;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(MeshFilter))]
public class WaterGenerator : MonoBehaviour
{
    public int resolution = 4;
    Mesh mesh;
    Vector3[] vertices;

    int[] triangles;
    private void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateShape();
        UpdateMesh();
    }

    private void CreateShape()
    {
        vertices = new Vector3[(resolution+1) * (resolution+1)];

        for (int y = 0; y <= resolution; y++)
        {
            for (int x = 0; x <= resolution; x++)
            {
                vertices[y * (resolution + 1) + x] = new Vector3(x, 0, y);
            }
        }

        triangles = new int[resolution * resolution * 6];

        int tindex = 0;
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int vertIndex = y * (resolution + 1) + x;

                triangles[tindex] = vertIndex;
                triangles[tindex + 1] = vertIndex + resolution + 1;
                triangles[tindex + 2] = vertIndex + 1;

                triangles[tindex + 3] = vertIndex + 1;
                triangles[tindex + 4] = vertIndex + resolution + 1;
                triangles[tindex + 5] = vertIndex + resolution + 2;

                tindex += 6;
            }
        }
    }

    private void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
    }
}
