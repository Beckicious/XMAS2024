using UnityEngine;

public class XmasCell : MonoBehaviour
{
    public const char FREE = '.';
    public const char FIXEDOBSTACLE = '#';
    public const char SETOBSTACLE = 'X';
    public const char LEVELSTART = 'S';
    public const char LEVELEND = 'E';

    SpriteRenderer spriteRenderer;

    public Color defaultColor;
    public Color selectedColor;
    public Color hoverColor;
    public float hoverColorDelta = 0.2f;

    private bool isSelected = false;
    private bool isHovered = false;

    public bool debugIsUnclickable = false;
    public Sprite debugUnclickableSprite;

    public void Click()
    {
        if (debugIsUnclickable)
        {
            return;
        }

        isSelected = !isSelected;
        SetColor();
    }

    public void SetSelected()
    {
        if (debugIsUnclickable)
        {
            return;
        }

        isSelected = true;
        SetColor();
    }

    public void SetDebugUnclickable()
    {
        spriteRenderer.color = Color.white;
        spriteRenderer.sprite = debugUnclickableSprite;
        debugIsUnclickable = true;
    }

    private void OnEnable()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (debugIsUnclickable)
        {
            return;
        }

        SetColor();
    }

    private void OnMouseEnter()
    {
        if (debugIsUnclickable)
        {
            return;
        }

        isHovered = true;
        SetColor();
    }

    private void OnMouseExit()
    {
        if (debugIsUnclickable)
        {
            return;
        }

        isHovered = false;
        SetColor();
    }

    private void SetColor()
    {
        spriteRenderer.color = isSelected ? selectedColor : defaultColor;
        if (isHovered)
        {
            spriteRenderer.color = Color.Lerp(spriteRenderer.color, hoverColor, hoverColorDelta);
        }
    }

    public char ToBoardChar()
    {
        if (debugIsUnclickable)
        {
            return FIXEDOBSTACLE;
        }
        if (isSelected)
        {
            return SETOBSTACLE;
        }
        return FREE;
    }
}
