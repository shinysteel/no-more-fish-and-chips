using UnityEngine;
using UnityEngine.UI;

public class CellOutline : MonoBehaviour
{
    [SerializeField] private Image _topImage;
    [SerializeField] private Image _leftImage;
    [SerializeField] private Image _bottomImage;
    [SerializeField] private Image _rightImage;

    private void Awake()
    {
        Set(false, false, false, false);
    }

    public void Set(bool top, bool left, bool bottom, bool right)
    {
        _topImage.enabled = top;
        _leftImage.enabled = left;
        _bottomImage.enabled = bottom;
        _rightImage.enabled = right;
    }
}
