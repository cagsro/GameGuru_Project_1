using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Button resetButton;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private TextMeshProUGUI matchCountText;

    private void Start()
    {
        resetButton.onClick.AddListener(OnResetButtonClick);
        gridManager.OnMatchCountChanged += UpdateMatchCountText;
        UpdateMatchCountText(0);
    }

    private void OnDestroy()
    {
        gridManager.OnMatchCountChanged -= UpdateMatchCountText;
    }

    private void UpdateMatchCountText(int count)
    {
        matchCountText.text = $"MATCH COUNT: {count}";
        matchCountText.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 0, 0);
    }

    private void OnResetButtonClick()
    {
        ResetButtonPunchAnim();
        gridManager.ResetGrid();
    }

    private void ResetButtonPunchAnim()
    {
        DOTween.Kill(resetButton.transform);
        resetButton.transform.localScale = Vector3.one;
        resetButton.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 0, 0);
    }
}
