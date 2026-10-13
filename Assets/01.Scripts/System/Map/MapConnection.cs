using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class MapConnection : UIBase
{
    [SerializeField] private float thickness = 6f;

    private RectTransform rectTransform;
    private Image lineImage;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        lineImage = GetComponent<Image>();

        lineImage.raycastTarget = false;
    }

    public void Initialized(Vector2 startPos, Vector2 endPos)
    {
        Vector2 dir = endPos - startPos;

        rectTransform.anchoredPosition = (startPos + endPos) * 0.5f;
        rectTransform.sizeDelta = new Vector2(dir.magnitude, thickness);
        rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
    }
}