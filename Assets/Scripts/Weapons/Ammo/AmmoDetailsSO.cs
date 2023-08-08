using UnityEngine;

[CreateAssetMenu(fileName = "AmmoDetails_", menuName = "Scriptable Objects/Weapons/Ammo Details")]
public class AmmoDetailsSO : ScriptableObject
{
    #region Header BASIC AMMO DETAILS
    [Space(10)]
    [Header("BASIC AMMO DETAILS")]
    #endregion Header BASIC AMMO DETAILS
    #region Tooltip
    [Tooltip("Name for the ammo")]
    #endregion Tooltip
    public string ammoName;
    public bool isPlayerAmmo;

    #region Header AMMO SPRITE, PREFAB & MATERIALS
    [Space(10)]
    [Header("AMMO SPRITE, PREFAB & MATERIALS")]
    #endregion Header AMMO SPRITE, PREFAB & MATERIALS
    #region Tooltip
    [Tooltip("Sprite to be used for the ammo")]
    #endregion Tooltip
    public Sprite ammoSprite;
    #region Tooltip
    [Tooltip("Populate with the prefab to be used for the ammo. If multiple prefabs are specified, a random one will be selected. The prefab can be an ammo pattern (IFireable interface)")]
    #endregion Tooltip
    public GameObject[] ammoPrefabArray;
    #region Tooltip
    [Tooltip("The material to be used for the ammo")]
    #endregion Tooltip
    public Material ammoMaterial;
    #region Tooltip
    [Tooltip("If the ammo should 'charge' briefly before moving, then set the time in seconds that the ammo is held charging after firing before release")]
    #endregion Tooltip
    public float ammoChargeTime = 0.1f;
    #region Tooltip
    [Tooltip("If the ammo has a charge time, then specify what material should be used to render the ammo while charging")]
    #endregion Tooltip
    public Material ammoChargeMaterial;

    #region Header AMMO BASE PARAMETERS
    [Space(10)]
    [Header("AMMO BASE PARAMETERS")]
    #endregion Header AMMO BASE PARAMETERS
    #region Tooltip
    [Tooltip("The damage each ammo deals")]
    #endregion Tooltip
    public int ammoDammage = 1;
    #region Tooltip
    [Tooltip("The minimum speed of the ammo. The speed will be a random value between the min and max")]
    #endregion Tooltip
    public float ammoSpeedMin = 20f;
    #region Tooltip
    [Tooltip("The maximum speed of the ammo. The speed will be a random value between the min and max")]
    #endregion Tooltip
    public float ammoSpeedMax = 20f;
    #region Tooltip
    [Tooltip("The range of the ammo (or ammo pattern) in unity units")]
    #endregion Tooltip
    public float ammoRange = 20f;
    #region Tooltip
    [Tooltip("The rotation speed in degrees per second of the ammo pattern")]
    #endregion Tooltip
    public float ammoRotationSpeed = 1f;

    #region Header AMMO SPREAD DETAILS
    [Space(10)]
    [Header("AMMO SPREAD DETAILS")]
    #endregion Header AMMO SPREAD DETAILS
    #region Tooltip
    [Tooltip("Minimum spread angle of the ammo. A higher spread means less accuracy. A random spread is calculated between the min and max values")]
    #endregion Tooltip
    public float ammoSpreadMin = 0f;
    #region Tooltip
    [Tooltip("Maxium spread angle of the ammo. A higher spread means less accuracy. A random spread is calculated between the min and max values")]
    #endregion Tooltip
    public float ammoSpreadMax = 0f;

    #region Header AMMO SPAWN DETAILS
    [Space(10)]
    [Header("AMMO SPAWN DETAILS")]
    #endregion Header AMMO SPAWN DETAILS
    #region Tooltip
    [Tooltip("Minimum number of ammo that spawns per shoot. A random value is selected between the min and max values")]
    #endregion Tooltip
    public int ammoSpawnAmmoMin = 1;
    #region Tooltip
    [Tooltip("Maximum number of ammo that spawns per shoot. A random value is selected between the min and max values")]
    #endregion Tooltip
    public int ammoSpawnAmmoMax = 1;
    #region Tooltip
    [Tooltip("Minimum spawn interval time. A random value is selected between the min and max values")]
    #endregion Tooltip
    public float ammoSpawnIntervalMin = 0f;
    #region Tooltip
    [Tooltip("Maximum spawn interval time. A random value is selected between the min and max values")]
    #endregion Tooltip
    public float ammoSpawnIntervalMax = 0f;

    #region Header AMMO TRAIL DETAILS
    [Space(10)]
    [Header("AMMO TRAIL DETAILS")]
    #endregion Header AMMO TRAIL DETAILS
    #region Tooltip
    [Tooltip("Selected if an ammo trail is required, otherwise deselected. If selected, the rest of the values should be populated")]
    #endregion Tooltip
    public bool isAmmoTrail = false;
    #region Tooltip
    [Tooltip("Ammo trail lifetime in seconds")]
    #endregion Tooltip
    public float ammoTrailTime = 3f;
    #region Tooltip
    [Tooltip("Ammo trail material")]
    #endregion Tooltip
    public Material ammoTrailMaterial;
    #region Tooltip
    [Tooltip("The starting width of the ammo trail")]
    #endregion Tooltip
    [Range(0f, 1f)] public float ammoTrailStartWith;
    #region Tooltip
    [Tooltip("The ending width of the ammo trail")]
    #endregion Tooltip
    [Range(0f, 1f)] public float ammoTrailEndWith;

    #region Validation
#if UNITY_EDITOR
    public void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(ammoName), ammoName);
        HelperUtilities.ValidateCheckNullValue(this, nameof(ammoSprite), ammoSprite);
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(ammoPrefabArray), ammoPrefabArray);
        HelperUtilities.ValidateCheckNullValue(this, nameof(ammoMaterial), ammoMaterial);
        if (ammoChargeTime > 0f)
        {
            HelperUtilities.ValidateCheckNullValue(this, nameof(ammoChargeMaterial), ammoChargeMaterial);
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(ammoDammage), ammoDammage, false);
            HelperUtilities.ValidateCheckPositiveRange(this, nameof(ammoSpeedMin), ammoSpeedMin, nameof(ammoSpeedMax), ammoSpeedMax, false);
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(ammoRange), ammoRange, false);
            HelperUtilities.ValidateCheckPositiveRange(this, nameof(ammoSpreadMin), ammoSpreadMin, nameof(ammoSpreadMax), ammoSpreadMax, true);
            HelperUtilities.ValidateCheckPositiveRange(this, nameof(ammoSpawnAmmoMin), ammoSpawnAmmoMin, nameof(ammoSpawnAmmoMax), ammoSpawnAmmoMax, false);
            HelperUtilities.ValidateCheckPositiveRange(this, nameof(ammoSpawnIntervalMin), ammoSpawnIntervalMin, nameof(ammoSpawnIntervalMax), ammoSpawnIntervalMax, true);
        }
        if (isAmmoTrail)
        {
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(ammoTrailTime), ammoTrailTime, false);
            HelperUtilities.ValidateCheckNullValue(this, nameof(ammoMaterial), ammoMaterial);
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(ammoTrailStartWith), ammoTrailStartWith, false);
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(ammoTrailEndWith), ammoTrailEndWith, false);
        }
    }
#endif
    #endregion
}
