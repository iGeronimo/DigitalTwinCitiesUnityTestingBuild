using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if (UNITY_EDITOR)
public enum ShapeGrid
{
    Square,
    Triangle,
    Hexagon,
}

public enum MatGrid
{
    Standard,
    HDRP,
}

/* Class: GridSpawner
 * 
 * Implements the functionalities of the tool Grid Spawner:
 * 
 *  - Implementing GUI elements
 *  - Creating a 2D grid in a 3D environment using values inputted by the user
 *  - Creating meshes to represent cells in one of three shapes (square, triangle, hexagon)
 *  - Calculating the height of cells based on average height and randomisation factor inputted by user
 *  - Placing prefabs randomly on cells based on values inputted by user
 *  - Replacing most recently created grid or creating a new grid
 * 
 */

public class GridSpawner : EditorWindow
{
    private string baseNameCell; //Every cell object will be named with this string input
    private MatGrid matShaderSelected; //Stores which option is selected for the material
    private ShapeGrid shapeSelected; //Stores which option is selected for the shape of every cell
    private float sizeCell; //Every cell will base its length and width on this value
    private float heightCell; //Every cell will take this value as its height, unless randomized (in case, this value will be the average height of all cells)
    private bool heightRandomOptions; //Controls whether height randomisation options are enabled or not
    private float heightRandomFactor; //Amount of randomisation in height (0 = no randomization, 100 = between twice the height and no height)

    private int rowsGrid; //Amount of rows present in the grid
    private int columnsGrid; //Amount of columns present in the grid
    private float gutterGrid; //Size of the gutter between the cells

    private Object prefabSource; //The prefab that will be placed randomly on the grid, unless amountRandom is set to 0
    private float sizePrefab; //Every prefab will scale its size based on this value
    private int amountRandom; //Percentage of prefabs placed relative to the amount of cells present in the grid
    private bool prefabOptions; //Controls whether prefab spawning is enabled or not

    public GameObject housePrefab;

    private GameObject parentGrid; //The parent object that holds all the cells and prefabs
    private float xPos; //x position used to place current cell
    private float zPos; //z position used to place current cell
    private float[] combinedHeight; // Height of cell combined with randomisation
    private bool triangleUp; //Decides whether a cell with a triangle shape is facing upwards or not to fit a triangle grid
    private bool rowEven; //Decides whether the current row with cells with a hexagon shape is an even number (2nd, 4th, 6th...row)

    private int numCell; //Number of cell used to decide whether current cell needs a prefab
    private int numPrefab; //Number used to name all prefabs ("Prefab" + numPrefab)

    private float cellAmount; //Determines how many cells need to have a prefab placed on them
    private int currentRandomCell; //Determines how many prefabs have already been placed and how many more need to be placed
    private float placePrefab; //Random number that decides which cell the tool will try to place a prefab
    private bool[] cellPrefab; //Stores which cells have a prefab on them already

    // Creates window
    [MenuItem("Window/Grid Spawner")]
    static void OpenWindow()
    {
        // Setting up editor window
        GridSpawner window = (GridSpawner)GetWindow(typeof(GridSpawner), false, "Grid Spawner");
        window.minSize = new Vector2(150, 150);
        window.Show();
    }

    // Implements GUI elements
    void OnGUI()
    {
        // Creating fields for the grid settings
        GUILayout.Label("Grid Settings", EditorStyles.boldLabel);
        baseNameCell = EditorGUILayout.TextField("Base Cell Name", baseNameCell);
        matShaderSelected = (MatGrid)EditorGUILayout.EnumPopup("Base Material Shader", matShaderSelected);
        shapeSelected = (ShapeGrid)EditorGUILayout.EnumPopup("Shape Cell ", shapeSelected);
        sizeCell = Mathf.Max(0, EditorGUILayout.FloatField("Size Cell", sizeCell));
        heightCell = Mathf.Max(0, EditorGUILayout.FloatField("Height Cell", heightCell));
        heightRandomOptions = GUILayout.Toggle(heightRandomOptions, "Enable Height Randomisation");
        GUI.enabled = heightRandomOptions;
        heightRandomFactor = EditorGUILayout.Slider("Randomisation Factor", heightRandomFactor, 0f, 1f);
        if (!heightRandomOptions)
        {
            heightRandomFactor = 0f;
        }
        GUI.enabled = true;

        EditorGUILayout.Space();

        rowsGrid = Mathf.Max(0, EditorGUILayout.IntField("Rows", rowsGrid));
        columnsGrid = Mathf.Max(0, EditorGUILayout.IntField("Colums", columnsGrid));
        gutterGrid = Mathf.Max(0, EditorGUILayout.FloatField("Size Gutter", gutterGrid));

        EditorGUILayout.Space();

        prefabOptions = GUILayout.Toggle(prefabOptions, "Enable Prefab Spawning");
        GUI.enabled = prefabOptions;
        // Creating fields for the prefab settings
        GUILayout.Label("Prefab settings", EditorStyles.boldLabel);
        prefabSource = EditorGUILayout.ObjectField("Prefab", prefabSource, typeof(Object), true);
        housePrefab = (GameObject)EditorGUILayout.ObjectField("House Prefab", housePrefab, typeof(GameObject), true);
        sizePrefab = Mathf.Max(0, EditorGUILayout.FloatField("Size", sizePrefab));

        EditorGUILayout.Space();

        // Creating fields for the prefab spawning settings
        GUILayout.Label("Prefab Spawning settings", EditorStyles.boldLabel);
        amountRandom = EditorGUILayout.IntSlider("Amount", amountRandom, 1, 100);

        EditorGUILayout.Space();

        // Disable "Replace Grid" button if no previous grid exists
        GUI.enabled = false;

        if (parentGrid != null)
        {
            GUI.enabled = true;
        }

        // Creating replace button
        if (GUILayout.Button("Replace Grid"))
        {
            DestroyImmediate(parentGrid);
            ResetValues();
            CreateGrid();
        }

        GUI.enabled = true;

        // Creating create button
        if (GUILayout.Button("Generate New Grid"))
        {
            ResetValues();
            CreateGrid();
        }
    }

    // Forms 2D grid in a 3D environment
    void CreateGrid()
    {
        // Creating parent objects
        parentGrid = new GameObject("Grid");

        GameObject parentPrefab = null;
        if (prefabOptions && prefabSource != null)
        {
            parentPrefab = new GameObject("Prefabs");
            parentPrefab.transform.parent = parentGrid.transform;
        }

        GameObject parentCell = new GameObject("Cells");
        parentCell.transform.parent = parentGrid.transform;
        parentCell.AddComponent<SetTiles>();
        parentCell.GetComponent<SetTiles>().rows = rowsGrid;
        parentCell.GetComponent<SetTiles>().cols = columnsGrid;

        // Calculates the amount of cells that need to have a prefab present
        cellAmount = Mathf.Round((float)columnsGrid * (float)rowsGrid / 100 * (float)amountRandom);

        cellPrefab = new bool[columnsGrid * rowsGrid];
        combinedHeight = new float[columnsGrid * rowsGrid];

        RandomPick();

        // Checks which base shape has been selected, and creates a grid
        int row;
        int column;
        switch (shapeSelected)
        {
            // Square selected
            case ShapeGrid.Square:
                row = 0;


                //Creating triangle information for mesh once
                int[] squareTriangles = {
                    // Bottom triangles
                    2, 1, 0,
                    3, 2, 0,
                    // Back triangles
                    6, 5, 4,
                    7, 6, 4,
                    // Right triangles
                    10, 9, 8,
                    11, 10, 8,
                    // Left triangles
                    14, 13, 12,
                    15, 14, 12,
                    // Top triangles
                    18, 17, 16,
                    19, 18, 16,
                    // Front triangles
                    22, 21, 20,
                    23, 22, 20,
                };

                while (row < rowsGrid)
                {
                    column = 0;
                    while (column < columnsGrid)
                    {
                        combinedHeight[numCell] = heightCell + heightCell * Random.Range(-heightRandomFactor, heightRandomFactor);
                        if (prefabOptions && prefabSource != null && cellPrefab[numCell])
                        {
                            numPrefab++;
                            Vector3 prefabPos = new Vector3(xPos + sizeCell * 0.5f, combinedHeight[numCell], zPos + sizeCell * 0.5f);
                            InstantiatePrefab(prefabPos, parentPrefab);
                        }

                        Square(parentCell, squareTriangles);

                        xPos += sizeCell + gutterGrid;
                        numCell++;
                        column++;
                    }
                    xPos = 0f;
                    zPos += sizeCell + gutterGrid;
                    row++;
                }
                ResetValues();
                break;

            // Triangle selected
            case ShapeGrid.Triangle:
                row = 0;

                //Creating triangle information for mesh once
                int[] triangleUpTriangles = {
                    // Bottom triangles
                    2, 1, 0,
                    // Right triangles
                    5, 4, 3,
                    6, 5, 3,
                    // Left triangles
                    9, 8, 7,
                    10, 9, 7,
                    // Top triangles
                    13, 12, 11,
                    // Front triangles
                    16, 15, 14,
                    17, 16, 14,
                };
                int[] triangleDownTriangles = {
                    // Bottom triangles
                    2, 0, 1,
                    // Back triangles
                    5, 4, 3,
                    6, 4, 5,
                    // Right triangles
                    9, 8, 7,
                    10, 9, 7,
                    // Left triangles
                    13, 12, 11,
                    14, 13, 11,
                    // Top triangles
                    17, 16, 15,
                };

                while (row < rowsGrid)
                {
                    column = 0;
                    while (column < columnsGrid)
                    {
                        combinedHeight[numCell] = heightCell + heightCell * Random.Range(-heightRandomFactor, heightRandomFactor);
                        if (triangleUp)
                        {
                            if (prefabOptions && prefabSource != null && cellPrefab[numCell])
                            {
                                numPrefab++;
                                Vector3 prefabPos = new Vector3(xPos + sizeCell * 0.5f, combinedHeight[numCell], zPos + sizeCell * 0.33f);
                                InstantiatePrefab(prefabPos, parentPrefab);
                            }
                            Triangle(parentCell, triangleUpTriangles);
                        }
                        else
                        {
                            if (prefabOptions && prefabSource != null && cellPrefab[numCell])
                            {
                                numPrefab++;
                                Vector3 prefabPos = new Vector3(xPos + sizeCell * 0.5f, combinedHeight[numCell], zPos + sizeCell * 0.67f);
                                InstantiatePrefab(prefabPos, parentPrefab);
                            }
                            Triangle(parentCell, triangleDownTriangles);
                        }

                        xPos += sizeCell / 2 + gutterGrid;
                        numCell++;
                        triangleUp = !triangleUp;
                        column++;
                    }
                    xPos = 0f;
                    zPos += sizeCell + gutterGrid;
                    if (columnsGrid % 2 == 0)
                    {
                        triangleUp = !triangleUp;
                    }
                    row++;
                }
                ResetValues();
                triangleUp = true;
                break;

            // Hexagon selected
            case ShapeGrid.Hexagon:
                xPos = -sizeCell * (0.5f - Mathf.Sqrt(3f) / 4f);
                row = 0;

                //Creating triangle information once for mesh
                int[] hexagonTriangles = {
                    // Bottom triangles
                    1, 0, 6,
                    2, 1, 6,
                    3, 2, 6,
                    4, 3, 6,
                    5, 4, 6,
                    0, 5, 6,
                    // Back right triangles
                    7, 9, 8,
                    7, 10, 9,
                    // Right triangles
                    11, 13, 12,
                    11, 14, 13,
                    // Front right triangles
                    15, 17, 16,
                    15, 18, 17,
                    // Front left triangles
                    19, 21, 20,
                    19, 22, 21,
                    // Left triangles
                    23, 25, 24,
                    23, 26, 25,
                    // Back left triangles
                    27, 29, 28,
                    27, 30, 29,
                    // Top triangles
                    37, 31, 32,
                    37, 32, 33,
                    37, 33, 34,
                    37, 34, 35,
                    37, 35, 36,
                    37, 36, 31,
                };

                while (row < rowsGrid)
                {
                    column = 0;
                    while (column < columnsGrid)
                    {
                        combinedHeight[numCell] = heightCell + heightCell * Random.Range(-heightRandomFactor, heightRandomFactor);
                        if (prefabOptions && prefabSource != null && cellPrefab[numCell])
                        {
                            Vector3 prefabPos = new Vector3(xPos + sizeCell * 0.5f, combinedHeight[numCell], zPos + sizeCell * 0.5f);
                            InstantiatePrefab(prefabPos, parentPrefab);
                            numPrefab++;
                        }

                        Hexagon(parentCell, hexagonTriangles);

                        xPos -= sizeCell * (Mathf.Sqrt(3f) / 2f) + gutterGrid;
                        numCell++;
                        column++;
                    }
                    if (rowEven)
                    {
                        xPos = -sizeCell * (0.5f - Mathf.Sqrt(3f) / 4f);
                        zPos += sizeCell * 0.75f + gutterGrid;
                        rowEven = false;
                    }
                    else
                    {
                        xPos = sizeCell * (Mathf.Sqrt(3f) / 4f) - sizeCell * (0.5f - Mathf.Sqrt(3f) / 4f) + gutterGrid / 2;
                        zPos += sizeCell * 0.75f + gutterGrid;
                        rowEven = true;
                    }
                    row++;
                }
                ResetValues();
                break;
        }
    }

    // Creates a square cell vertex information
    void Square(GameObject parent, int[] triangles)
    {
        // Creating arrays for mesh information
        Vector3[] vertices = {
            // Bottom vertices
            new Vector3(xPos + sizeCell * 1f, combinedHeight[numCell] * 0f, zPos + sizeCell * 0f), //0
            new Vector3(xPos + sizeCell * 0f, combinedHeight[numCell] * 0f, zPos + sizeCell * 0f), //1
            new Vector3(xPos + sizeCell * 0f, combinedHeight[numCell] * 0f, zPos + sizeCell * 1f), //2
            new Vector3(xPos + sizeCell * 1f, combinedHeight[numCell] * 0f, zPos + sizeCell * 1f), //3

            // Back vertices
            new Vector3(xPos + sizeCell * 1f, combinedHeight[numCell] * 0f, zPos + sizeCell * 1f), //4
            new Vector3(xPos + sizeCell * 0f, combinedHeight[numCell] * 0f, zPos + sizeCell * 1f), //5
            new Vector3(xPos + sizeCell * 0f, combinedHeight[numCell] * 1f, zPos + sizeCell * 1f), //6
            new Vector3(xPos + sizeCell * 1f, combinedHeight[numCell] * 1f, zPos + sizeCell * 1f), //7

            // Right vertices
            new Vector3(xPos + sizeCell * 0f, combinedHeight[numCell] * 0f, zPos + sizeCell * 1f), //8
            new Vector3(xPos + sizeCell * 0f, combinedHeight[numCell] * 0f, zPos + sizeCell * 0f), //9
            new Vector3(xPos + sizeCell * 0f, combinedHeight[numCell] * 1f, zPos + sizeCell * 0f), //10
            new Vector3(xPos + sizeCell * 0f, combinedHeight[numCell] * 1f, zPos + sizeCell * 1f), //11

            // Left vertices
            new Vector3(xPos + sizeCell * 1f, combinedHeight[numCell] * 0f, zPos + sizeCell * 0f), //12
            new Vector3(xPos + sizeCell * 1f, combinedHeight[numCell] * 0f, zPos + sizeCell * 1f), //13
            new Vector3(xPos + sizeCell * 1f, combinedHeight[numCell] * 1f, zPos + sizeCell * 1f), //14
            new Vector3(xPos + sizeCell * 1f, combinedHeight[numCell] * 1f, zPos + sizeCell * 0f), //15

            // Top vertices
            new Vector3(xPos + sizeCell * 0f, combinedHeight[numCell] * 1f, zPos + sizeCell * 0f), //16
            new Vector3(xPos + sizeCell * 1f, combinedHeight[numCell] * 1f, zPos + sizeCell * 0f), //17
            new Vector3(xPos + sizeCell * 1f, combinedHeight[numCell] * 1f, zPos + sizeCell * 1f), //18
            new Vector3(xPos + sizeCell * 0f, combinedHeight[numCell] * 1f, zPos + sizeCell * 1f), //19

            // Front vertices
            new Vector3(xPos + sizeCell * 1f, combinedHeight[numCell] * 1f, zPos + sizeCell * 0f), //20
            new Vector3(xPos + sizeCell * 0f, combinedHeight[numCell] * 1f, zPos + sizeCell * 0f), //21
            new Vector3(xPos + sizeCell * 0f, combinedHeight[numCell] * 0f, zPos + sizeCell * 0f), //22
            new Vector3(xPos + sizeCell * 1f, combinedHeight[numCell] * 0f, zPos + sizeCell * 0f), //23
        };
        CreateMesh(vertices, triangles, parent);
    }

    // Creates a triangle cell vertex information
    void Triangle(GameObject parent, int[] triangles)
    {
        // Checking which way the triangle is facing
        if (triangleUp)
        {
            // Creating arrays for mesh information
            Vector3[] vertices = {
                // Bottom vertices
                new Vector3(xPos + sizeCell * 1f, combinedHeight[numCell] * 0f, zPos + sizeCell * 0f), //0
                new Vector3(xPos + sizeCell * 0f, combinedHeight[numCell] * 0f, zPos + sizeCell * 0f), //1
                new Vector3(xPos + sizeCell * 0.5f, combinedHeight[numCell] * 0f, zPos + sizeCell * 1f), //2

                // Right vertices
                new Vector3(xPos + sizeCell * 0.5f, combinedHeight[numCell] * 0f, zPos + sizeCell * 1f), //3
                new Vector3(xPos + sizeCell * 0f, combinedHeight[numCell] * 0f, zPos + sizeCell * 0f), //4
                new Vector3(xPos + sizeCell * 0f, combinedHeight[numCell] * 1f, zPos + sizeCell * 0f), //5
                new Vector3(xPos + sizeCell * 0.5f, combinedHeight[numCell] * 1f, zPos + sizeCell * 1f), //6

                // Left vertices
                new Vector3(xPos + sizeCell * 1f, combinedHeight[numCell] * 0f, zPos + sizeCell * 0f), //7
                new Vector3(xPos + sizeCell * 0.5f, combinedHeight[numCell] * 0f, zPos + sizeCell * 1f), //8
                new Vector3(xPos + sizeCell * 0.5f, combinedHeight[numCell] * 1f, zPos + sizeCell * 1f), //9
                new Vector3(xPos + sizeCell * 1f, combinedHeight[numCell] * 1f, zPos + sizeCell * 0f), //10

                // Top vertices
                new Vector3(xPos + sizeCell * 0f, combinedHeight[numCell] * 1f, zPos + sizeCell * 0f), //11
                new Vector3(xPos + sizeCell * 1f, combinedHeight[numCell] * 1f, zPos + sizeCell * 0f), //12
                new Vector3(xPos + sizeCell * 0.5f, combinedHeight[numCell] * 1f, zPos + sizeCell * 1f), //13

                // Front vertices
                new Vector3(xPos + sizeCell * 1f, combinedHeight[numCell] * 1f, zPos + sizeCell * 0f), //14
                new Vector3(xPos + sizeCell * 0f, combinedHeight[numCell] * 1f, zPos + sizeCell * 0f), //15
                new Vector3(xPos + sizeCell * 0f, combinedHeight[numCell] * 0f, zPos + sizeCell * 0f), //16
                new Vector3(xPos + sizeCell * 1f, combinedHeight[numCell] * 0f, zPos + sizeCell * 0f), //17
            };
            CreateMesh(vertices, triangles, parent);
        }
        else
        {
            // Creating arrays for mesh information
            Vector3[] vertices = {
                // Bottom vertices
                new Vector3(xPos + sizeCell * 0.5f, combinedHeight[numCell] * 0f, zPos + sizeCell * 0f), //0
                new Vector3(xPos + sizeCell * 1f, combinedHeight[numCell] * 0f, zPos + sizeCell * 1f), //1
                new Vector3(xPos + sizeCell * 0f, combinedHeight[numCell] * 0f, zPos + sizeCell * 1f), //2

                // Back vertices
                new Vector3(xPos + sizeCell * 1f, combinedHeight[numCell] * 0f, zPos + sizeCell * 1f), //3
                new Vector3(xPos + sizeCell * 0f, combinedHeight[numCell] * 0f, zPos + sizeCell * 1f), //4
                new Vector3(xPos + sizeCell * 1f, combinedHeight[numCell] * 1f, zPos + sizeCell * 1f), //5
                new Vector3(xPos + sizeCell * 0f, combinedHeight[numCell] * 1f, zPos + sizeCell * 1f), //6

                // Right vertices
                new Vector3(xPos + sizeCell * 0f, combinedHeight[numCell] * 0f, zPos + sizeCell * 1f), //7
                new Vector3(xPos + sizeCell * 0.5f, combinedHeight[numCell] * 0f, zPos + sizeCell * 0f), //8
                new Vector3(xPos + sizeCell * 0.5f, combinedHeight[numCell] * 1f, zPos + sizeCell * 0f), //9
                new Vector3(xPos + sizeCell * 0f, combinedHeight[numCell] * 1f, zPos + sizeCell * 1f), //10

                // Left vertices
                new Vector3(xPos + sizeCell * 0.5f, combinedHeight[numCell] * 0f, zPos + sizeCell * 0f), //11
                new Vector3(xPos + sizeCell * 1f, combinedHeight[numCell] * 0f, zPos + sizeCell * 1f), //12
                new Vector3(xPos + sizeCell * 1f, combinedHeight[numCell] * 1f, zPos + sizeCell * 1f), //13
                new Vector3(xPos + sizeCell * 0.5f, combinedHeight[numCell] * 1f, zPos + sizeCell * 0f), //14

                // Top vertices
                new Vector3(xPos + sizeCell * 0.5f, combinedHeight[numCell] * 1f, zPos + sizeCell * 0f), //15
                new Vector3(xPos + sizeCell * 1f, combinedHeight[numCell] * 1f, zPos + sizeCell * 1f), //16
                new Vector3(xPos + sizeCell * 0f, combinedHeight[numCell] * 1f, zPos + sizeCell * 1f), //17
            };
            CreateMesh(vertices, triangles, parent);
        }
    }

    // Creates a hexagon vertex information
    void Hexagon(GameObject parent, int[] triangles)
    {
        // Creating arrays for mesh information
        Vector3[] vertices = {
        // Bottom vertices
        new Vector3(xPos + sizeCell * 0.5f, combinedHeight[numCell] * 0f, zPos + sizeCell * 1f), //0
        new Vector3(xPos + sizeCell * (0.5f + Mathf.Sqrt(3f) / 4f), combinedHeight[numCell] * 0f, zPos + sizeCell * 0.75f), //1
        new Vector3(xPos + sizeCell * (0.5f + Mathf.Sqrt(3f) / 4f), combinedHeight[numCell] * 0f, zPos + sizeCell * 0.25f), //2
        new Vector3(xPos + sizeCell * 0.5f, combinedHeight[numCell] * 0f, zPos + sizeCell * 0f), //3
        new Vector3(xPos + sizeCell * (0.5f - Mathf.Sqrt(3f) / 4f), combinedHeight[numCell] * 0f, zPos + sizeCell * 0.25f), //4
        new Vector3(xPos + sizeCell * (0.5f - Mathf.Sqrt(3f) / 4f), combinedHeight[numCell] * 0f, zPos + sizeCell * 0.75f), //5
        new Vector3(xPos + sizeCell * 0.5f, combinedHeight[numCell] * 0f, zPos + sizeCell * 0.5f), //6
        
        // Back right vertices
        new Vector3(xPos + sizeCell * 0.5f, combinedHeight[numCell] * 0f, zPos + sizeCell * 1f), //7
        new Vector3(xPos + sizeCell * (0.5f - Mathf.Sqrt(3f) / 4f), combinedHeight[numCell] * 0f, zPos + sizeCell * 0.75f), //8
        new Vector3(xPos + sizeCell * (0.5f - Mathf.Sqrt(3f) / 4f), combinedHeight[numCell] * 1f, zPos + sizeCell * 0.75f), //9
        new Vector3(xPos + sizeCell * 0.5f, combinedHeight[numCell] * 1f, zPos + sizeCell * 1f), //10

        // Right vertices
        new Vector3(xPos + sizeCell * (0.5f - Mathf.Sqrt(3f) / 4f), combinedHeight[numCell] * 0f, zPos + sizeCell * 0.75f), //11
        new Vector3(xPos + sizeCell * (0.5f - Mathf.Sqrt(3f) / 4f), combinedHeight[numCell] * 0f, zPos + sizeCell * 0.25f), //12
        new Vector3(xPos + sizeCell * (0.5f - Mathf.Sqrt(3f) / 4f), combinedHeight[numCell] * 1f, zPos + sizeCell * 0.25f), //13
        new Vector3(xPos + sizeCell * (0.5f - Mathf.Sqrt(3f) / 4f), combinedHeight[numCell] * 1f, zPos + sizeCell * 0.75f), //14

        // Front right vertices
        new Vector3(xPos + sizeCell * (0.5f - Mathf.Sqrt(3f) / 4f), combinedHeight[numCell] * 0f, zPos + sizeCell * 0.25f), //15
        new Vector3(xPos + sizeCell * 0.5f, combinedHeight[numCell] * 0f, zPos + sizeCell * 0f), //16
        new Vector3(xPos + sizeCell * 0.5f, combinedHeight[numCell] * 1f, zPos + sizeCell * 0f), //17
        new Vector3(xPos + sizeCell * (0.5f - Mathf.Sqrt(3f) / 4f), combinedHeight[numCell] * 1f, zPos + sizeCell * 0.25f), //18

        // Front left vertices
        new Vector3(xPos + sizeCell * 0.5f, combinedHeight[numCell] * 0f, zPos + sizeCell * 0f), //19
        new Vector3(xPos + sizeCell * (0.5f + Mathf.Sqrt(3f) / 4f), combinedHeight[numCell] * 0f, zPos + sizeCell * 0.25f), //20
        new Vector3(xPos + sizeCell * (0.5f + Mathf.Sqrt(3f) / 4f), combinedHeight[numCell] * 1f, zPos + sizeCell * 0.25f), //21
        new Vector3(xPos + sizeCell * 0.5f, combinedHeight[numCell] * 1f, zPos + sizeCell * 0f), //22

        // Left vertices
        new Vector3(xPos + sizeCell * (0.5f + Mathf.Sqrt(3f) / 4f), combinedHeight[numCell] * 0f, zPos + sizeCell * 0.25f), //23
        new Vector3(xPos + sizeCell * (0.5f + Mathf.Sqrt(3f) / 4f), combinedHeight[numCell] * 0f, zPos + sizeCell * 0.75f), //24
        new Vector3(xPos + sizeCell * (0.5f + Mathf.Sqrt(3f) / 4f), combinedHeight[numCell] * 1f, zPos + sizeCell * 0.75f), //25
        new Vector3(xPos + sizeCell * (0.5f + Mathf.Sqrt(3f) / 4f), combinedHeight[numCell] * 1f, zPos + sizeCell * 0.25f), //26

        // Back left vertices
        new Vector3(xPos + sizeCell * (0.5f + Mathf.Sqrt(3f) / 4f), combinedHeight[numCell] * 0f, zPos + sizeCell * 0.75f), //27
        new Vector3(xPos + sizeCell * 0.5f, combinedHeight[numCell] * 0f, zPos + sizeCell * 1f), //28
        new Vector3(xPos + sizeCell * 0.5f, combinedHeight[numCell] * 1f, zPos + sizeCell * 1f), //29
        new Vector3(xPos + sizeCell * (0.5f + Mathf.Sqrt(3f) / 4f), combinedHeight[numCell] * 1f, zPos + sizeCell * 0.75f), //30

        // Bottom vertices
        new Vector3(xPos + sizeCell * 0.5f, combinedHeight[numCell] * 1f, zPos + sizeCell * 1f), //31
        new Vector3(xPos + sizeCell * (0.5f + Mathf.Sqrt(3f) / 4f), combinedHeight[numCell] * 1f, zPos + sizeCell * 0.75f), //32
        new Vector3(xPos + sizeCell * (0.5f + Mathf.Sqrt(3f) / 4f), combinedHeight[numCell] * 1f, zPos + sizeCell * 0.25f), //33
        new Vector3(xPos + sizeCell * 0.5f, combinedHeight[numCell] * 1f, zPos + sizeCell * 0f), //34
        new Vector3(xPos + sizeCell * (0.5f - Mathf.Sqrt(3f) / 4f), combinedHeight[numCell] * 1f, zPos + sizeCell * 0.25f), //35
        new Vector3(xPos + sizeCell * (0.5f - Mathf.Sqrt(3f) / 4f), combinedHeight[numCell] * 1f, zPos + sizeCell * 0.75f), //36
        new Vector3(xPos + sizeCell * 0.5f, combinedHeight[numCell] * 1f, zPos + sizeCell * 0.5f), //37
        };
        CreateMesh(vertices, triangles, parent);
    }

    // Creates cell mesh using vertex and triangle information, then gives it a empty parent object (for grouping purposes)
    void CreateMesh(Vector3[] vertices, int[] triangles, GameObject parent)
    {
        // Creating mesh
        Mesh cell = new Mesh();

        // Appointing mesh information to mesh
        cell.vertices = vertices;
        cell.triangles = triangles;
        cell.RecalculateNormals();
        cell.name = baseNameCell;

        // Creating game object
        GameObject cellObject = new GameObject(baseNameCell + numCell, typeof(MeshFilter), typeof(MeshRenderer));

        cellObject.GetComponent<MeshFilter>().mesh = cell;

        CreateMaterial(cellObject);
        GameObject newPrefab = Instantiate(housePrefab, cellObject.transform);
        newPrefab.transform.position = new Vector3(xPos + sizeCell * 0.5f, combinedHeight[numCell], zPos + sizeCell * 0.5f);
        cellObject.transform.parent = parent.transform;
        cellObject.AddComponent<TileParameters>();
        cellObject.AddComponent<BoxCollider>();
        cellObject.AddComponent<NeighbourTileDetection>();
        cellObject.tag = "Tile";
        
        
    }

    // Creates a material for the cell meshes to use
    void CreateMaterial(GameObject cellObject)
    {
        // Checks which shader is selected, and then creates a material
        switch (matShaderSelected)
        {
            case MatGrid.Standard:
                cellObject.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
                break;

            case MatGrid.HDRP:
                cellObject.GetComponent<MeshRenderer>().material = new Material(Shader.Find("HDRP/Lit"));
                break;
        }
    }

    // Resets calculation values to avoid issues such as out of bounds arrays, improper placement of cells, or improper randomisation of prefabs and cell height
    void ResetValues()
    {
        xPos = 0f;
        zPos = 0f;
        numCell = 0;
        numPrefab = 0;
        cellAmount = 0f;
        currentRandomCell = 0;
        triangleUp = false;
        rowEven = false;

        int cell = 0;
        while (cell < cellAmount)
        {
            cellPrefab[cell] = false;
            cell++;
        }
    }

    // Instantiates prefab on given location, and then gives it an empty parent object (for grouping purposes)
    void InstantiatePrefab(Vector3 prefabPos, GameObject parentPrefab)
    {
        GameObject prefabSpawned = (GameObject)Instantiate(prefabSource, prefabPos, Quaternion.identity);
        prefabSpawned.name = "Prefab" + numPrefab;
        prefabSpawned.transform.parent = parentPrefab.transform;
        prefabSpawned.transform.localScale = new Vector3(sizePrefab, sizePrefab, sizePrefab);
    }

    // Randomly picks cells for prefabs to be placed on
    void RandomPick()
    {
        int randomAmount = 0;
        while (randomAmount < columnsGrid * rowsGrid)
        {
            if (currentRandomCell < cellAmount)
            {
                placePrefab = Mathf.Min(Mathf.Round(Random.value * ((float)columnsGrid * (float)rowsGrid)), columnsGrid * rowsGrid - 1);
                if (!cellPrefab[(int)placePrefab])
                {
                    cellPrefab[(int)placePrefab] = true;
                    currentRandomCell++;
                }
                else
                {
                    randomAmount--;
                }
            }
            randomAmount++;
        }
    }
}
#endif