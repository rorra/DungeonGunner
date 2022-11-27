using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperUtilities
{
    /// <summary>
    /// string is empty; returns true if ther eis an error
    /// </summary>
    /// <param name="thisObject"></param>
    /// <param name="fieldName"></param>
    /// <param name="stringToCheck"></param>
    /// <returns></returns>
    public static bool ValidateCheckEmptyString(Object thisObject, string fieldName, string stringToCheck)
    {
        if (stringToCheck == "")
        {
            Debug.LogError("The " + fieldName + " field in the " + thisObject.name + " ScriptableObject is empty. Please enter a value.");
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// list is empty or contains a null value check; returns true if there is an error
    /// </summary>
    /// <param name="thisObject"></param>
    /// <param name="fieldName"></param>
    /// <param name="enumerableToCheck"></param>
    /// <returns></returns>
    public static bool ValidateCheckEnumerableValues(Object thisObject, string fieldName, IEnumerable enumerableToCheck)
    {
        bool error = false;
        int count = 0;

        foreach (var item in enumerableToCheck)
        {
            if (item == null)
            {
                Debug.LogError("The " + fieldName + " field in the " + thisObject.name + " ScriptableObject has a null value at index " + count + ". Please enter a value.");
                error = true;
            }
            else
            {
                count++;
            }
        }
        
        if (count == 0)
        {
            Debug.LogError("The " + fieldName + " field in the " + thisObject.name + " ScriptableObject is empty. Please enter a value.");
            error = true;
        }

        return error;
    }
}
