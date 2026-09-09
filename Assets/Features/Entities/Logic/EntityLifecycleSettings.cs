using UnityEngine;

[CreateAssetMenu(fileName = "EntityLifecycleSettings", menuName = "Settings/Entities/EntityLifecycleSettings")]
public class EntityLifecycleSettings : ScriptableObject
{
    [SerializeField] private float _gracePeriod = 0.25f;

    public float GracePeriod => _gracePeriod;
}
