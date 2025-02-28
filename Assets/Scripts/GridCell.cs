using UnityEngine;

public class GridCell : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private GridManager gridManager;
    private int xPos, yPos;
    public bool HasX { get; private set; }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Başlangıçta hücreyi görünür yap ve rengini ayarla
        spriteRenderer.color = new Color(1f, 1f, 1f, 1f); // Tam opak beyaz
    }

    public void Initialize(int x, int y, GridManager manager)
    {
        xPos = x;
        yPos = y;
        gridManager = manager;
    }

    private void OnMouseDown()
    {
        if (!HasX)
        {
            PlaceX();
            gridManager.CheckConnectedCells(xPos, yPos);
        }
    }

    private void PlaceX()
    {
        HasX = true;
        spriteRenderer.color = new Color(1f, 0f, 0f, 1f); // Tam opak kırmızı
    }

    public void RemoveX()
    {
        HasX = false;
        spriteRenderer.color = new Color(1f, 1f, 1f, 1f); // Tam opak beyaz
    }
}
