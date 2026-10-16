using UnityEngine;

[CreateAssetMenu(fileName = "EntityLifecycleSharedSettings", menuName = "Settings/Entities/EntityLifecycleSharedSettings")]
public class EntityLifecycleSharedSettings : ScriptableObject
{
    [SerializeField] private float _gracePeriod = 0.25f;

    public float GracePeriod => _gracePeriod;
}
