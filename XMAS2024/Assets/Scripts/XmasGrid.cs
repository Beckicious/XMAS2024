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

    private List<Point> RoutePoints;

    public void LoadEmptyGameGrid(int levelCount = int.MaxValue)
    {
        levelCount = Mathf.Min(levelFiles.Length, levelCount);
        RoutePoints = new List<Point>();

        // TODO: find a better solution to form the complete grid from level grids
        StringBuilder[] gameGridLines = new StringBuilder[GridHeight];
        for (int i = 0; i < GridHeight; i++)
        {
            gameGridLines[i] = new StringBuilder();
        }

        for (int l = 0; l < levelCount; l++)
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
        int levelStartCount = 0;
        int levelEndCount = 0;

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
                switch (line[x])
                {
                    case XmasCell.FIXEDOBSTACLE:
                        cell.SetCellType(CellType.FIXEDOBSTACLE);
                        break;
                    case XmasCell.LEVELSTART:
                        RoutePoints.Add(new Point(y, x));
                        levelStartCount++;
                        break;
                    case XmasCell.LEVELEND:
                        RoutePoints.Add(new Point(y, x));
                        levelEndCount++;
                        break;
                    default:
                        break;
                }
            }
        }

        // check if maps overlapped correctly ('S' should be overridden by 'E')
        Debug.Assert(levelStartCount == 1);
        Debug.Assert(levelEndCount == levelCount);
        Debug.Assert(RoutePoints.Count == levelCount + 1);

        // sort by X value
        RoutePoints.Sort((p1, p2) => p1.X.CompareTo(p2.X));
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

        foreach (Point p in GetShortestPath())
        {
            gameGrid[p.Y, p.X].SetCellType(CellType.PATH);
        }
    }

    private void RemovePath()
    {
        for (int y = 0; y < GridHeight; y++)
        {
            for (int x = 0; x < GridWidth; x++)
            {
                if (gameGrid[y, x].Type == CellType.PATH)
                {
                    gameGrid[y, x].SetCellType(CellType.FREE);
                }
            }
        }
    }

    private IList<Point> GetShortestPath()
    {
        return new AStar(gameGrid).GetPath(RoutePoints[0], RoutePoints[^1]);
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
    CellType currentDraggingType;

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
                if (gameGrid[y, x].Click())
                {
                    currentDraggingType = gameGrid[y, x].Type;
                };
                ShowShortestPath();
            }
        }
        else if (Input.GetMouseButton(0))
        {
            // Dragging
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPosition = helperGrid.WorldToCell(worldPoint);

            if (cellPosition != previousPos)
            {
                previousPos = cellPosition;

                int y = -cellPosition.y;
                int x = cellPosition.x;

                if (y >= 0 && y < GridHeight && x >= 0 && x < GridWidth)
                {
                    gameGrid[y, x].SetCellType(currentDraggingType);
                    ShowShortestPath();
                }
            }
        }
    }
}
