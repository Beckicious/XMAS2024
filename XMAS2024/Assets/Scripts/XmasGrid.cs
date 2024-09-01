using UnityEngine;
using UnityEngine.Tilemaps;

public class XmasGrid : MonoBehaviour
{
    public const int GridHeight = 30;
    public const int GridWidth = 100;
    [SerializeField] private GameObject cellGO;
    [SerializeField] private GameObject board;
    private Tilemap baseTilemap;

    private Grid grid;
    private XmasCell[] cells;

    private void Start()
    {
        grid = GetComponent<Grid>();
        cells = new XmasCell[GridHeight * GridWidth];

        for (int y = 0; y < GridHeight; y++)
        {
            for (int x = 0; x < GridWidth; x++)
            {
                Vector3 pos = grid.CellToLocalInterpolated(new Vector3(x, -y));
                pos += grid.cellSize / 2;
                var cellGO = Instantiate(this.cellGO, board.transform);
                cellGO.transform.position = pos;
                cells[y * GridWidth + x] = cellGO.GetComponent<XmasCell>();
            }
        }

        Vector3 camPos = new Vector3(GridHeight / 2, -GridHeight / 2, Camera.main.transform.position.z);
        Camera.main.transform.position = camPos;
        XmasCamera.ValidatePosition();
    }

    Vector3Int previousPos;
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("clicked");

            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPosition = grid.WorldToCell(worldPoint);

            previousPos = cellPosition;

            int cellIndex = -cellPosition.y * GridWidth + cellPosition.x;
            if (0 <= cellIndex && cellIndex < cells.Length)
            {
                cells[cellIndex].Click();
            }
        }
        else if (Input.GetMouseButton(0))
        {
            Debug.Log("dragged");

            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPosition = grid.WorldToCell(worldPoint);

            if (cellPosition != previousPos)
            {
                previousPos = cellPosition;

                int cellIndex = -cellPosition.y * GridWidth + cellPosition.x;
                if (0 <= cellIndex && cellIndex < cells.Length)
                {
                    cells[cellIndex].Click();
                }
            }
        }
    }
}
