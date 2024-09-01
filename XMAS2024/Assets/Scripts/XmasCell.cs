using UnityEngine;

public class XmasCell : MonoBehaviour
{
    SpriteRenderer spriteRenderer;

    public Color defaultColor;
    public Color selectedColor;
    public Color hoverColor;
    public float hoverColorDelta = 0.2f;

    private bool isSelected = false;
    private bool isHovered = false;

    public void Click()
    {
        isSelected = !isSelected;
        Debug.Log(isSelected);
        SetColor();
    }

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetColor();
    }

    private void OnMouseEnter()
    {
        isHovered = true;
        SetColor();
    }

    private void OnMouseExit()
    {
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
}
