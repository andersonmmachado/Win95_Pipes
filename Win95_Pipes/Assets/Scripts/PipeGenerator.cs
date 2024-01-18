using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeGenerator : MonoBehaviour
{
    public int gridSizeX = 10; // Adjust as needed
    public int gridSizeY = 10; // Adjust as needed
    public int gridSizeZ = 10; // Adjust as needed
    public float pipeSegmentSize = 1.0f;
    public float revealDelay = 0.1f; // Adjust the delay between each segment reveal
    public GameObject pipePrefab; // Reference to a prefab for the pipe segment

    private int[,,] pipeGrid;

    void Start()
    {
        InitializePipeGrid();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(GenerateAndRenderPipes());
            DisplayGrid();
        }
    }

    IEnumerator GenerateAndRenderPipes()
    {
        GeneratePipes();

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int z = 0; z < gridSizeZ; z++)
                {
                    int pipeSegmentValue = pipeGrid[x, y, z];

                    if (pipeSegmentValue != 0)
                    {
                        Vector3 position = new Vector3(x * pipeSegmentSize, y * pipeSegmentSize, z * pipeSegmentSize);
                        GameObject pipeSegment = Instantiate(pipePrefab, position, Quaternion.identity);
                        // Optionally, you can set different visual properties based on pipeSegmentValue

                        yield return new WaitForSeconds(revealDelay);
                    }
                }
            }
        }
    }

    void InitializePipeGrid()
    {
        pipeGrid = new int[gridSizeX, gridSizeY, gridSizeZ];
    }

    void GeneratePipes()
    {
        int startX = Random.Range(0, gridSizeX);
        int startY = Random.Range(0, gridSizeY);
        int startZ = Random.Range(0, gridSizeZ);

        GrowPipe(startX, startY, startZ, Vector3.right, 10); // Example: Grow a pipe to the right for 10 segments
    }

    void GrowPipe(int startX, int startY, int startZ, Vector3 direction, int length)
    {
        int currentX = startX;
        int currentY = startY;
        int currentZ = startZ;

        for (int i = 0; i < length; i++)
        {
            if (IsWithinBounds(currentX, currentY, currentZ) && !IsPipeOccupied(currentX, currentY, currentZ))
            {
                pipeGrid[currentX, currentY, currentZ] = i + 1;

                Vector3 newDirection = GetRandomDirection(currentX, currentY, currentZ, direction);

                // Check if there is a valid position in any direction
                bool foundValidPosition = false;

                foreach (Vector3 possibleDirection in GetAdjacentDirections(currentX, currentY, currentZ, newDirection))
                {
                    int nextX = currentX + (int)possibleDirection.x;
                    int nextY = currentY + (int)possibleDirection.y;
                    int nextZ = currentZ + (int)possibleDirection.z;

                    if (IsWithinBounds(nextX, nextY, nextZ) && !IsPipeOccupied(nextX, nextY, nextZ))
                    {
                        foundValidPosition = true;
                        break;
                    }
                }

                if (!foundValidPosition)
                    return;

                currentX += (int)newDirection.x;
                currentY += (int)newDirection.y;
                currentZ += (int)newDirection.z;
            }
            else
            {
                // Handle the case where the new position is outside the bounds of the array or hits another pipe
                break;
            }
        }
    }

    bool IsWithinBounds(int x, int y, int z)
    {
        return x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY && z >= 0 && z < gridSizeZ;
    }

    bool IsPipeOccupied(int x, int y, int z)
    {
        return pipeGrid[x, y, z] != 0;
    }

    void DisplayGrid()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int z = 0; z < gridSizeZ; z++)
                {
                    Vector3 min = new Vector3(x * pipeSegmentSize, y * pipeSegmentSize, z * pipeSegmentSize);
                    Vector3 max = new Vector3((x + 1) * pipeSegmentSize, (y + 1) * pipeSegmentSize, (z + 1) * pipeSegmentSize);
                    Debug.DrawLine(min, new Vector3(max.x, min.y, min.z), Color.white, 1000f);
                    Debug.DrawLine(min, new Vector3(min.x, max.y, min.z), Color.white, 1000f);
                    Debug.DrawLine(min, new Vector3(min.x, min.y, max.z), Color.white, 1000f);

                    Debug.DrawLine(new Vector3(max.x, min.y, min.z), new Vector3(max.x, max.y, min.z), Color.white, 1000f);
                    Debug.DrawLine(new Vector3(min.x, max.y, min.z), new Vector3(max.x, max.y, min.z), Color.white, 1000f);

                    Debug.DrawLine(new Vector3(min.x, max.y, min.z), new Vector3(min.x, max.y, max.z), Color.white, 1000f);
                    Debug.DrawLine(new Vector3(min.x, min.y, max.z), new Vector3(min.x, max.y, max.z), Color.white, 1000f);
                    Debug.DrawLine(new Vector3(min.x, min.y, max.z), new Vector3(max.x, min.y, max.z), Color.white, 1000f);

                    Debug.DrawLine(new Vector3(max.x, max.y, min.z), new Vector3(max.x, max.y, max.z), Color.white, 1000f);
                    Debug.DrawLine(new Vector3(max.x, min.y, max.z), new Vector3(max.x, max.y, max.z), Color.white, 1000f);
                    Debug.DrawLine(new Vector3(min.x, max.y, max.z), new Vector3(max.x, max.y, max.z), Color.white, 1000f);
                }
            }
        }
    }

    Vector3 GetRandomDirection(int currentX, int currentY, int currentZ, Vector3 currentDirection)
    {
        Vector3[] possibleDirections = GetAdjacentDirections(currentX, currentY, currentZ, currentDirection);
        return possibleDirections[Random.Range(0, possibleDirections.Length)];
    }

    Vector3[] GetAdjacentDirections(int x, int y, int z, Vector3 currentDirection)
    {
        Vector3[] directions = { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };
        Vector3[] adjacentDirections = new Vector3[6];

        int index = 0;
        foreach (Vector3 direction in directions)
        {
            int nextX = x + (int)direction.x;
            int nextY = y + (int)direction.y;
            int nextZ = z + (int)direction.z;

            if (IsWithinBounds(nextX, nextY, nextZ) && !IsPipeOccupied(nextX, nextY, nextZ) && direction != -currentDirection)
            {
                adjacentDirections[index++] = direction;
            }
        }

        // Trim the array to remove any unused elements
        System.Array.Resize(ref adjacentDirections, index);

        return adjacentDirections;
    }
}