using UnityEngine;

public enum CellType
{
    FREE,
    FIXEDOBSTACLE,
    SETOBSTACLE,
    PATH
}

public class XmasCell : MonoBehaviour
{
    public const char FREE = '.';
    public const char FIXEDOBSTACLE = '#';
    public const char SETOBSTACLE = 'X';
    public const char LEVELSTART = 'S';
    public const char LEVELEND = 'E';

    SpriteRenderer spriteRenderer;

    public Color freeColor;
    public Color setObstacleColor;
    public Color hoverColor;
    public float hoverColorDelta = 0.2f;

    public CellType Type { get; private set; } = CellType.FREE;

    public Sprite freeSprite;
    public Sprite setObstacleSprite;
    public Sprite fixedObstacleSprite;
    public Sprite pathSprite;

    private bool isHovered = false;

    public bool Click()
    {
        switch (Type)
        {
            case CellType.FIXEDOBSTACLE:
                return false;
            case CellType.SETOBSTACLE:
                SetCellType(CellType.FREE);
                return true;
            case CellType.PATH:
            case CellType.FREE:
                SetCellType(CellType.SETOBSTACLE);
                return true;

            default:
                return false;
        }
    }

    public void SetCellType(CellType newType)
    {
        if (Type == CellType.FIXEDOBSTACLE)
        {
            // no changing that
            return;
        }

        if (Type == CellType.SETOBSTACLE && newType == CellType.PATH)
        {
            // cannot set path on a set obstacle
            return;
        }

        switch (newType)
        {
            case CellType.FIXEDOBSTACLE:
                spriteRenderer.sprite = fixedObstacleSprite;
                break;
            case CellType.SETOBSTACLE:
                spriteRenderer.sprite = setObstacleSprite;
                break;
            case CellType.PATH:
                spriteRenderer.sprite = pathSprite;
                break;
            case CellType.FREE:
                spriteRenderer.sprite = freeSprite;
                break;
        }

        Type = newType;
        SetColor();
    }

    private void OnEnable()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        SetColor();
    }

    private void OnMouseEnter()
    {
        if (Type == CellType.FIXEDOBSTACLE)
        {
            return;
        }

        isHovered = true;
        SetColor();
    }

    private void OnMouseExit()
    {
        if (Type == CellType.FIXEDOBSTACLE)
        {
            return;
        }

        isHovered = false;
        SetColor();
    }

    private void SetColor()
    {
        switch (Type)
        {
            case CellType.FIXEDOBSTACLE:
                spriteRenderer.color = new Color(0, 0, 0, 0);
                return; // no hover for you
            case CellType.SETOBSTACLE:
                spriteRenderer.color = setObstacleColor;
                break;
            case CellType.PATH:
                spriteRenderer.color = Color.white;
                break;
            case CellType.FREE:
                spriteRenderer.color = freeColor;
                break;
        }

        if (isHovered)
        {
            spriteRenderer.color = Color.Lerp(spriteRenderer.color, hoverColor, hoverColorDelta);
        }
    }

    public char ToBoardChar()
    {
        switch (Type)
        {
            case CellType.FIXEDOBSTACLE:
                return FIXEDOBSTACLE;
            case CellType.SETOBSTACLE:
                return SETOBSTACLE;
            case CellType.PATH:
            case CellType.FREE:
            default:
                return FREE;
        }
    }
}
