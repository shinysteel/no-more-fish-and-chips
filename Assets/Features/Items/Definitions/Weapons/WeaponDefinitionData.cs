using UnityEngine;

namespace NoMoreFishAndChips.Items
{
    public enum WeaponType
    {
        None,
        Paddle,
        Spear,
        Slingshot
    }

    [CreateAssetMenu(fileName = "WeaponDefinitionData", menuName = "Data/Items/WeaponDefinitionnData")]
    public class WeaponDefinitionData : ItemDefinitionData
    {
        [SerializeField] private WeaponType _weaponType;

        public WeaponType WeaponType => _weaponType;
    }
}