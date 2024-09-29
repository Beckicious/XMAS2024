using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Tilemaps;

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
                    cell.SetDebugUnclickable();
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
        var lines = Regex.Split(solution, "\r\n|\n|\r");
        for (int y = 0; y < GridHeight; y++)
        {
            for (int x = 0; x < GridWidth; x++)
            {
                if (lines[y][x] == XmasCell.SETOBSTACLE)
                {
                    gameGrid[y, x].SetSelected();
                }
            }
        }
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
                }
            }
        }
    }
}
