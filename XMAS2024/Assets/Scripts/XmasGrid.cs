using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class XmasGrid : MonoBehaviour
{
    public const int GridHeight = 32;
    public int GridWidth = 0; // Set GridWidth dependable on loaded levels
    [SerializeField] private GameObject cellGO;
    [SerializeField] private GameObject board;
    [SerializeField] private TextAsset[] levelFiles;

    private Grid helperGrid;
    private XmasCell[,] gameGrid;

    public void LoadEmptyGameGrid()
    {
        StringBuilder[] gameGridLines = new StringBuilder[GridHeight];
        for (int i = 0; i < GridHeight; i++)
        {
            gameGridLines[i] = new StringBuilder();
        }

        for (int l = 0; l < levelFiles.Length; l++)
        {
            string level = levelFiles[l].text;

            var lines = Regex.Split(level, "\r\n|\n|\r");
            Debug.Assert(lines.Length == GridHeight);

            int levelWidth = lines[0].Length;

            for (int i = 0; i < GridHeight; i++)
            {
                Debug.Assert(lines[i].Length == levelWidth);
                if (l > 0)
                {
                    gameGridLines[i].Append(lines[i].Substring(1));
                }
                else
                {
                    gameGridLines[i].Append(lines[i]);
                }
            }
        }

        GridWidth = gameGridLines[0].Length;

        // Setup Grid
        gameGrid = new XmasCell[GridHeight, GridWidth];
        for (int y = 0; y < GridHeight; y++)
        {
            string line = gameGridLines[y].ToString();

            for (int x = 0; x < GridWidth; x++)
            {
                Vector3 pos = helperGrid.CellToLocalInterpolated(new Vector3(x, -y));
                pos += helperGrid.cellSize / 2;
                var cellGO = Instantiate(this.cellGO, board.transform);
                cellGO.transform.position = pos;
                XmasCell cell = cellGO.GetComponent<XmasCell>();
                gameGrid[y, x] = cell;
                if (line[x] == XmasCell.FIXEDOBSTACLE)
                {
                    cell.SetCellType(CellType.FIXEDOBSTACLE);
                }
            }
        }
    }

    public void ClearGameGrid()
    {
        for (int y = 0; y < GridHeight; y++)
        {
            for (int x = 0; x < GridWidth; x++)
            {
                Destroy(gameGrid[y, x].gameObject);
            }
        }
        gameGrid = null;
    }

    public void LoadSolution(string solution)
    {
        ClearGameGrid();

        // reset grid
        LoadEmptyGameGrid();

        // load set obstacles from string
        string[] lines = Regex.Split(solution, "\r\n|\n|\r");
        for (int y = 0; y < GridHeight && y < lines.Length; y++)
        {
            for (int x = 0; x < GridWidth && x < lines[y].Length; x++)
            {
                if (lines[y][x] == XmasCell.SETOBSTACLE)
                {
                    gameGrid[y, x].SetCellType(CellType.SETOBSTACLE);
                }
            }
        }

        ShowShortestPath();
    }

    private void ShowShortestPath()
    {
        RemovePath();

        foreach (var point in GetShortestPath())
        {
            gameGrid[point.y, point.x].SetCellType(CellType.PATH);
        }
    }

    private void RemovePath()
    {
        for (int y = 0; y < GridHeight; y++)
        {
            for (int x = 0; x < GridWidth; x++)
            {
                if (gameGrid[y, x].cellType == CellType.PATH)
                {
                    gameGrid[y, x].SetCellType(CellType.FREE);
                }
            }
        }
    }

    private IList<(int y, int x)> GetShortestPath()
    {
        // TODO: set start and end when reading the empty board
        (int, int) start = (0, 0);
        (int, int) end = (0, GridWidth - 1);
        for (int y = 0; y < GridHeight; y++)
        {
            if (gameGrid[y, 0].cellType == CellType.FREE)
            {
                start = (y, 0);
            }
            if (gameGrid[y, GridWidth - 1].cellType == CellType.FREE)
            {
                end = (y, GridWidth - 1);
            }
        }

        // TODO: implement pathfinding

        return new List<(int, int)> { start, end };
    }

    public string GetGridAsString()
    {
        StringBuilder sb = new StringBuilder();
        for (int y = 0; y < GridHeight; y++)
        {
            for (int x = 0; x < GridWidth; x++)
            {
                sb.Append(gameGrid[y, x].ToBoardChar());
            }
            sb.Append("\n");
        }
        return sb.ToString();
    }

    private void Start()
    {
        helperGrid = GetComponent<Grid>();

        // Load empty grid (levels) and set GridWidth from there
        LoadEmptyGameGrid();

        ShowShortestPath();

        // Set camera position
        Vector3 camPos = new Vector3(GridHeight / 2, -GridHeight / 2, Camera.main.transform.position.z);
        Camera.main.transform.position = camPos;
        XmasCamera.ValidatePosition();
    }

    Vector3Int previousPos;
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPosition = helperGrid.WorldToCell(worldPoint);

            previousPos = cellPosition;

            int y = -cellPosition.y;
            int x = cellPosition.x;

            if (y >= 0 && y < GridHeight && x >= 0 && x < GridWidth)
            {
                gameGrid[y, x].Click();
                ShowShortestPath();
            }
        }
        else if (Input.GetMouseButton(0))
        {
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPosition = helperGrid.WorldToCell(worldPoint);

            if (cellPosition != previousPos)
            {
                previousPos = cellPosition;

                int y = -cellPosition.y;
                int x = cellPosition.x;

                if (y >= 0 && y < GridHeight && x >= 0 && x < GridWidth)
                {
                    gameGrid[y, x].Click();
                    ShowShortestPath();
                }
            }
        }
    }
}
