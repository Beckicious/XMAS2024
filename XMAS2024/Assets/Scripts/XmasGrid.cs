using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class XmasGrid : MonoBehaviour
{
    public const int GridHeight = 32;
    private const string PLAYERPREFSGRIDKEY = "xmas-2024-grid";
    public int GridWidth { get; private set; } = 0; // Set GridWidth dependable on loaded levels
    [SerializeField] private GameObject cellGO;
    [SerializeField] private GameObject board;
    [SerializeField] private TextAsset[] levelFiles;
    public TextMeshProUGUI lengthText;

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
                        cell.isLevelStart = true;
                        levelStartCount++;
                        break;
                    case XmasCell.LEVELEND:
                        RoutePoints.Add(new Point(y, x));
                        cell.isLevelEnd = true;
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

    public void ResetGrid()
    {
        ClearGameGrid();

        LoadEmptyGameGrid();

        ShowShortestPath();
    }

    private void ShowShortestPath()
    {
        RemovePath();

        var path = GetShortestPath();
        foreach (Point p in path)
        {
            gameGrid[p.Y, p.X].SetCellType(CellType.PATH);
        }
        lengthText.text = $"Length: {path.Count}";
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

    private void SaveGameToPlayerPrefs()
    {
        string gridString = GetGridAsString();
        PlayerPrefs.SetString(PLAYERPREFSGRIDKEY, gridString);
        PlayerPrefs.Save();
    }

    private void Start()
    {
        helperGrid = GetComponent<Grid>();

        // Load empty grid (levels) and set GridWidth from there
        LoadEmptyGameGrid();

        if (PlayerPrefs.HasKey(PLAYERPREFSGRIDKEY))
        {
            var boardString = PlayerPrefs.GetString(PLAYERPREFSGRIDKEY);
            LoadSolution(boardString);
        }

        ShowShortestPath();

        // Set camera position
        Vector3 camPos = new Vector3(GridHeight / 2, -GridHeight / 2, Camera.main.transform.position.z);
        Camera.main.transform.position = camPos;
        XmasCamera.ValidatePosition();

        InvokeRepeating("SaveGameToPlayerPrefs", 5, 5);
    }

    Vector3Int previousPos;
    CellType currentDraggingType;
    Vector3? lastPanPos = null;
    float? lastDist = null;

    void Update()
    {
        if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            if (touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Moved)
            {
                if (lastDist is null)
                {
                    lastDist = Vector2.Distance(touch1.position, touch2.position);
                }
                else
                {
                    float newDist = Vector2.Distance(touch1.position, touch2.position);
                    float touchDist = lastDist.Value - newDist;
                    lastDist = newDist;
                    Camera.main.orthographicSize += touchDist * 0.1f;
                }

                if (lastPanPos is null)
                {
                    lastPanPos = Vector3.Lerp(touch1.position, touch2.position, 0.5f);
                }
                else
                {
                    Vector3 panPos = Vector3.Lerp(touch1.position, touch2.position, 0.5f);
                    Vector3 panDiff = lastPanPos.Value - panPos;

                    Camera.main.transform.position += panDiff * 0.01f;
                }

                XmasCamera.ValidatePosition();
            }
            else if (touch1.phase == TouchPhase.Ended || touch2.phase == TouchPhase.Ended)
            {
                lastPanPos = null;
                lastDist = null;
            }
        }
        else if (Input.GetMouseButtonDown(0))
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
