using UnityEngine;

public class RegisterAsSun : MonoBehaviour
{
    [SerializeField] private Light _light;

    public void Start()
    {
        RenderSettings.sun = _light;
    }
}
