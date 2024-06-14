using System.Collections.Generic;
using UnityEngine;

public class WaterGenerator : MonoBehaviour
{
    public int resolution = 4;
    public int chunkSize = 10; // Define the size of each chunk

    private List<MeshFilter> meshFilters = new List<MeshFilter>();
    private List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
    private Vector3[] vertices;
    private int[] triangles;

    // Define multiple wave components
    public List<WaveComponent> waveComponents = new List<WaveComponent>();

    private void Start()
    {
        CreateWaterChunks();
    }

    private void CreateWaterChunks()
    {
        for (int y = 0; y < resolution; y += chunkSize)
        {
            for (int x = 0; x < resolution; x += chunkSize)
            {
                GameObject chunkObject = new GameObject("WaterChunk");
                chunkObject.transform.parent = transform;
                chunkObject.transform.localScale = Vector3.one;
                MeshFilter meshFilter = chunkObject.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = chunkObject.AddComponent<MeshRenderer>();
                meshRenderer.material = GetComponent<MeshRenderer>().material;

                meshFilters.Add(meshFilter);
                meshRenderers.Add(meshRenderer);

                CreateChunkShape(x, y, meshFilter);
            }
        }
    }

    private void CreateChunkShape(int startX, int startY, MeshFilter meshFilter)
    {
        int chunkResolution = chunkSize + 1;
        vertices = new Vector3[chunkResolution * chunkResolution];
        triangles = new int[chunkSize * chunkSize * 6];

        for (int y = 0; y < chunkResolution; y++)
        {
            for (int x = 0; x < chunkResolution; x++)
            {
                vertices[y * chunkResolution + x] = new Vector3(startX + x, 0, startY + y);
            }
        }

        int tindex = 0;
        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                int vertIndex = y * chunkResolution + x;

                triangles[tindex] = vertIndex;
                triangles[tindex + 1] = vertIndex + chunkResolution;
                triangles[tindex + 2] = vertIndex + 1;

                triangles[tindex + 3] = vertIndex + 1;
                triangles[tindex + 4] = vertIndex + chunkResolution;
                triangles[tindex + 5] = vertIndex + chunkResolution + 1;

                tindex += 6;
            }
        }

        meshFilter.mesh = new Mesh();
        meshFilter.mesh.vertices = vertices;
        meshFilter.mesh.triangles = triangles;
        meshFilter.mesh.RecalculateNormals();
    }

    private void Update()
    {
        UpdateMeshes();
    }

    private void UpdateMeshes()
    {
        for (int i = 0; i < meshFilters.Count; i++)
        {
            int startX = (i % (resolution / chunkSize)) * chunkSize;
            int startY = (i / (resolution / chunkSize)) * chunkSize;

            ComputeWaveHeights(startX, startY);

            Mesh mesh = meshFilters[i].mesh;
            mesh.vertices = vertices;
            mesh.RecalculateNormals();
        }
    }

    private void ComputeWaveHeights(int startX, int startY)
    {
        int chunkResolution = chunkSize + 1;
        double[] heights = new double[chunkResolution * chunkResolution];

        // Generate wave heights for each component
        foreach (WaveComponent component in waveComponents)
        {
            for (int y = 0; y < chunkResolution; y++)
            {
                for (int x = 0; x < chunkResolution; x++)
                {
                    double xPos = (startX + x) * 0.1; // Adjust scaling as needed
                    double yPos = (startY + y) * 0.1; // Adjust scaling as needed

                    double height = ComputeGerstnerWaveHeight(xPos, yPos, component);
                    heights[y * chunkResolution + x] += height; // Accumulate heights from different components
                }
            }
        }

        // Apply computed heights to mesh vertices
        for (int y = 0; y < chunkResolution; y++)
        {
            for (int x = 0; x < chunkResolution; x++)
            {
                vertices[y * chunkResolution + x].y = (float)heights[y * chunkResolution + x];
            }
        }
    }

private double ComputeGerstnerWaveHeight(double x, double y, WaveComponent component)
{
    double height = 0.0;

    // Time-dependent offset for variation
    float offset = Mathf.PerlinNoise(Time.time * component.perlinScale, 0f);

    foreach (WaveDisplacement displacement in component.waveDisplacements)
    {
        // Randomly adjust parameters based on Perlin noise
        float randomAmplitude = displacement.amplitude * Random.Range(1f - component.amplitudeRandomness, 1f + component.amplitudeRandomness);
        float randomFrequency = displacement.frequency * Random.Range(1f - component.frequencyRandomness, 1f + component.frequencyRandomness);
        float randomPhase = displacement.phase + Random.Range(-component.phaseRandomness, component.phaseRandomness);

        // Apply Perlin noise offset to random parameters
        randomAmplitude *= (1f + offset * component.amplitudePerlinFactor);
        randomFrequency *= (1f + offset * component.frequencyPerlinFactor);
        randomPhase += offset * component.phasePerlinFactor;

        // Calculate wave height
        double dot = displacement.direction.x * x + displacement.direction.y * y;
        double waveHeight = randomAmplitude * Mathf.Sin((float)(dot * randomFrequency + Time.time * component.waveSpeed));
        height += waveHeight;
    }

    return height;
}


}

[System.Serializable]
public class WaveDisplacement
{
    public float amplitude;
    public float frequency;
    public float phase;
    public Vector2 direction;
}

[System.Serializable]
public class WaveComponent
{
    public float waveSpeed = 1;
    public float amplitudeRandomness = 1;
    public float frequencyRandomness = 1;
    public float phaseRandomness = 1;
    public float perlinScale = 1;
    public float amplitudePerlinFactor = 1;
    public float frequencyPerlinFactor = 1;
    public float phasePerlinFactor = 1;

    public List<WaveDisplacement> waveDisplacements = new List<WaveDisplacement>();
}