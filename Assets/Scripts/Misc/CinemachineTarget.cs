using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CinemachineTargetGroup))]

public class CinemachineTarget : MonoBehaviour
{
    private CinemachineTargetGroup cinemachineTargetGroup;

    private void Awake()
    {
        cinemachineTargetGroup = GetComponent<CinemachineTargetGroup>();        
    }

    void Start()
    {
        SetCinemachineTargetGroup();
    }

    private void SetCinemachineTargetGroup()
    {
        CinemachineTargetGroup.Target targetPlayer = new CinemachineTargetGroup.Target { weight = 1f, radius = 1f, target = GameManager.Instance.GetPlayer().transform };
        CinemachineTargetGroup.Target[] cinemachineTargetArray = new CinemachineTargetGroup.Target[] { targetPlayer };
        cinemachineTargetGroup.m_Targets = cinemachineTargetArray;
    }
}
