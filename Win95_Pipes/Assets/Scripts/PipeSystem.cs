using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeSystem : MonoBehaviour
{
    public int gridSize;
    public GameObject regularPipePrefab;
    public GameObject elbowPiecePrefab;
    private bool growing = false;
    public Material[] pipeMaterials;
    private List<Material> availableMaterials;
    private int[,,] pipeNetwork;

    void Start()
    {
        InitializePipeNetwork();
        InitializeAvailableMaterials();
        DisplayGrid();
    }

    void Update()
    {
        if (!growing)
        {
            if (IsGridFull())
            {
                ResetPipeSystem();
            }
            else
            {
                StartCoroutine(GrowPipesCoroutine());
            }
        }
    }

    bool IsGridFull()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    if (pipeNetwork[x, y, z] == 0)
                    {
                        return false; // Grid is not full
                    }
                }
            }
        }

        return true; // Grid is full
    }

    void ResetPipeSystem()
    {
        // Clear the entire pipe system
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Reset the pipe network
        InitializePipeNetwork();
    }

    void InitializePipeNetwork()
    {
        pipeNetwork = new int[gridSize, gridSize, gridSize];
    }
    void InitializeAvailableMaterials()
    {
        // Ensure pipeMaterials array is not null
        if (pipeMaterials == null || pipeMaterials.Length == 0)
        {
            Debug.LogError("Pipe materials array is not set or is empty.");
            return;
        }

        // Initialize the list of available materials
        availableMaterials = new List<Material>(pipeMaterials);


    }

    IEnumerator GrowPipesCoroutine()
    {
        growing = true;

        // Choose a random starting position for a new pipe
        int startX = Random.Range(0, gridSize);
        int startY = Random.Range(0, gridSize);
        int startZ = Random.Range(0, gridSize);

        // Check if the starting position is already occupied
        if (pipeNetwork[startX, startY, startZ] != 0)
        {
            growing = false;
            yield break; // Starting position is occupied, do nothing
        }

        int currentX = startX;
        int currentY = startY;
        int currentZ = startZ;

        int pipeNumber = Random.Range(1, 2); // Different numbers for different pipe types

        // Get a unique material for each pipe type before starting to grow
        Material pipeMaterial = GetUniquePipeMaterial();

        while (ShouldContinueGrowing(currentX, currentY, currentZ))
        {
            pipeNetwork[currentX, currentY, currentZ] = pipeNumber;

            // Instantiate the pipe with the unique material
            GameObject pipePiecePrefab = GetPipePiecePrefab(pipeNumber);
            GameObject pipePiece = Instantiate(pipePiecePrefab, new Vector3(currentX, currentY, currentZ), Quaternion.identity, transform);
            pipePiece.GetComponent<Renderer>().material = pipeMaterial;

            // Get a list of valid adjacent positions
            List<Vector3Int> validAdjacentPositions = GetValidAdjacentPositions(currentX, currentY, currentZ);

            if (validAdjacentPositions.Count > 0)
            {
                // Choose a random valid adjacent position
                Vector3Int randomValidPosition = validAdjacentPositions[Random.Range(0, validAdjacentPositions.Count)];

                currentX = randomValidPosition.x;
                currentY = randomValidPosition.y;
                currentZ = randomValidPosition.z;
            }
            else
            {
                // No valid adjacent positions, stop growing
                break;
            }

            // Wait for a short duration to visualize each step
            yield return new WaitForSeconds(0.1f);
        }

        growing = false;
    }

    List<Vector3Int> GetValidAdjacentPositions(int x, int y, int z)
    {
        List<Vector3Int> validAdjacentPositions = new List<Vector3Int>();

        // Check all six possible adjacent positions
        for (int i = 0; i < 6; i++)
        {
            int newX = x + (i == 0 ? 1 : (i == 1 ? -1 : 0));
            int newY = y + (i == 2 ? 1 : (i == 3 ? -1 : 0));
            int newZ = z + (i == 4 ? 1 : (i == 5 ? -1 : 0));

            // Check if the new position is within bounds and unoccupied
            if (newX >= 0 && newX < gridSize && newY >= 0 && newY < gridSize && newZ >= 0 && newZ < gridSize
                && pipeNetwork[newX, newY, newZ] == 0)
            {
                validAdjacentPositions.Add(new Vector3Int(newX, newY, newZ));
            }
        }

        return validAdjacentPositions;
    }

    bool ShouldContinueGrowing(int x, int y, int z)
    {
        // Check if the current position is within bounds and unoccupied
        return x >= 0 && x < gridSize && y >= 0 && y < gridSize && z >= 0 && z < gridSize && pipeNetwork[x, y, z] == 0;
    }

    

    Material GetUniquePipeMaterial()
    {
        if (availableMaterials.Count == 0)
        {
            Debug.LogWarning("No available materials for pipes. Reusing materials.");
            InitializeAvailableMaterials();
        }

        // Choose a random material from the available materials
        int randomIndex = Random.Range(0, availableMaterials.Count);
        Material uniqueMaterial = availableMaterials[randomIndex];

        // Remove the chosen material from the list to ensure uniqueness
        availableMaterials.RemoveAt(randomIndex);
        return uniqueMaterial;
    }

    //TODO: Add a method to get the correct pipe prefab based on the pipe type
    GameObject GetPipePiecePrefab(int pipeType)
    {
        switch (pipeType)
        {
            case 1:
                return regularPipePrefab;
            case 2:
                return elbowPiecePrefab;
            default:
                return null;
        }
    }

    void DisplayGrid()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    Vector3 min = new Vector3(x, y, z);
                    Vector3 max = new Vector3((x + 1), (y + 1), (z + 1));
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
}
