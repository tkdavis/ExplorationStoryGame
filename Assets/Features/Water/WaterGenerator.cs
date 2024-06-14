using System.Collections.Generic;
using UnityEngine;

public class WaterGenerator : MonoBehaviour
{
    public float horizontalSquishFactor = 1.0f;
    public int resolution = 4;
    public float waveSpeed = 1.0f;
    public int chunkSize = 10; // Define the size of each chunk

    public float trochoidalAmplitude = 1f;
    public float trochoidalFrequency = 1f;
    public float trochoidalPhase = 0f;

    public float amplitudeRandomness = 1;
    public float frequencyRandomness = 1;
    public float phaseRandomness = 1;

    public float waveSpeedRandomness = 1.0f;

    public List<WaveDisplacement> waveDisplacements = new List<WaveDisplacement>();

    private List<MeshFilter> meshFilters = new List<MeshFilter>();
    private List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
    private Vector3[] vertices;
    private int[]triangles;

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
    // Set a random seed for consistent randomness
    Random.InitState(12345);

    trochoidalPhase += Time.deltaTime * waveSpeed;
    for (int i = 0; i < meshFilters.Count; i++)
    {
        int startX = (i % (resolution / chunkSize)) * chunkSize;
        int startY = (i / (resolution / chunkSize)) * chunkSize;

        for (int y = 0; y <= chunkSize; y++)
        {
            for (int x = 0; x <= chunkSize; x++)
            {
                float totalDisplacementY = 0f;
                float totalDisplacementX = 0f;

                // Calculate displacement for each trochoidal wave with random variations
                foreach (WaveDisplacement displacement in waveDisplacements)
                {
                    // Generate random variations for amplitude, frequency, and phase
                    float randomAmplitude = displacement.amplitude * Random.Range(1f - amplitudeRandomness, 1f + amplitudeRandomness);
                    float randomFrequency = displacement.frequency * Random.Range(1f - frequencyRandomness, 1f + frequencyRandomness);
                    float randomPhase = displacement.phase + Random.Range(-phaseRandomness, phaseRandomness);

                    float waveDisplacementY = CalculateTrochoidalWaveDisplacement(x + startX, y + startY, randomAmplitude, randomFrequency, trochoidalPhase + randomPhase, displacement.direction.x, displacement.direction.y);
                    totalDisplacementY += waveDisplacementY;

                    // Calculate horizontal displacement based on vertical position (y-coordinate)
                    float waveDisplacementX = waveDisplacementY * horizontalSquishFactor; // Adjust horizontal squish factor as needed
                    totalDisplacementX += waveDisplacementX;
                }

                // Apply trochoidal wave displacement along the y-axis
                vertices[y * (chunkSize + 1) + x].y = startY + totalDisplacementY;

                // Apply trochoidal wave displacement along the x-axis
                vertices[y * (chunkSize + 1) + x].x = startX + x + totalDisplacementX;
            }
        }

        Mesh mesh = meshFilters[i].mesh;
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }
}

    private float CalculateTrochoidalWaveDisplacement(float x, float y, float amplitude, float frequency, float phase, float directionX, float directionY)
    {
        // Calculate the displacement using the trochoidal wave formula with direction
        return amplitude * Mathf.Sin(2 * Mathf.PI * (frequency * (x * directionX + y * directionY)) + phase);
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
