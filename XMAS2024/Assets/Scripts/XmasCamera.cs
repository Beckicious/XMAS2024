using UnityEngine;

public class XmasCamera : MonoBehaviour
{
    public float speed;
    public int buttonMultiplier;
    public int scrollMultiplier;
    public XmasGrid xmasGrid;

    public static bool Locked
    {
        set
        {
            instance.enabled = !value;
        }
    }

    static XmasCamera instance;

    private void OnEnable()
    {
        instance = this;
    }

    private void Update()
    {
        float xDelta = Input.GetAxis("Horizontal");
        if (xDelta != 0)
        {
            AdjustPosition(xDelta * buttonMultiplier);
        }

        float xScrollDelta = -Input.GetAxis("Mouse ScrollWheel");
        if (xScrollDelta != 0)
        {
            AdjustPosition(xScrollDelta * scrollMultiplier);
        }
    }

    void AdjustPosition(float xDelta)
    {
        Vector3 direction = new Vector3(xDelta, 0, 0).normalized;
        float distance = speed * Mathf.Abs(xDelta) * Time.deltaTime;

        Vector3 position = transform.localPosition;
        position += direction * distance;
        transform.localPosition = ClampPosition(position);
    }

    Vector3 ClampPosition(Vector3 position)
    {
        float xMin = XmasGrid.GridHeight / 1.5f;
        float xMax = xmasGrid.GridWidth - XmasGrid.GridHeight / 1.5f;
        position.x = Mathf.Clamp(position.x, xMin, xMax);

        return position;
    }

    public static void ValidatePosition()
    {
        instance.AdjustPosition(0f);
    }
}
