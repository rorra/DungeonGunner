using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFireable
{
    void initializeAmmo(AmmoDetailsSO ammoDetails, float aimAngle, float weaponAimAngle, float ammoSpeed, Vector3 weaponAimDirectorVector, bool overrideAmmoMovement = false);

    GameObject GetGameObject();
}
