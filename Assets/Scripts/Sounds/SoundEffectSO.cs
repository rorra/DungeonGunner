using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundEffect_", menuName = "Scriptable Objects/Sound/Sound Effect")]
public class SoundEffectSO : ScriptableObject
{
    #region Header SOUND EFFECT DETAILS
    [Space(10)]
    [Header("SOUND EFFECT DETAILS")]
    #endregion
    #region ToolTip
    [Tooltip("The name for the sound effect")]
    #endregion
    public string soundEffectName;
    #region ToolTip
    [Tooltip("The prefab for the sound effect")]
    #endregion
    public GameObject soundPrefab;
    #region ToolTip
    [Tooltip("The audio clip for the sound effect")]
    #endregion
    public AudioClip soundEffectClip;
    #region ToolTip
    [Tooltip("The minimum pitch variation for the sound effect. A random pitch variation will be generated between the " +
             "minimum and maximum values. A random pitch variation makes sound effects more natural.")]
    [Range(0.1f, 1.5f)]
    #endregion
    public float soundEffectPitchRandomVariationMin = 0.8f;
    #region ToolTip
    [Tooltip("The maximum pitch variation for the sound effect. A random pitch variation will be generated between the " +
             "minimum and maximum values. A random pitch variation makes sound effects more natural.")]
    [Range(0.1f, 1.5f)]
    #endregion
    public float soundEffectPitchRandomVariationMax = 1.2f;
    #region ToolTip
    [Tooltip("The sound effect volume")]
    [Range(0.0f, 1.0f)]
    #endregion
    public float soundEffectVolume = 1f;
    
    #region Validation
    #if UNITY_EDITOR

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(soundEffectName), soundEffectName);
        HelperUtilities.ValidateCheckNullValue(this, nameof(soundPrefab), soundPrefab);
        HelperUtilities.ValidateCheckNullValue(this, nameof(soundEffectClip), soundEffectClip);
        HelperUtilities.ValidateCheckPositiveRange(this, 
            nameof(soundEffectPitchRandomVariationMin), soundEffectPitchRandomVariationMin, 
            nameof(soundEffectPitchRandomVariationMax), soundEffectPitchRandomVariationMax, 
            false);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(soundEffectVolume), soundEffectVolume, true);
    }
    
    #endif
    #endregion
}
