
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3;
using VRC.SDK3.Components;
using VRC.Udon.Common.Interfaces;
using VRC.SDK3.UdonNetworkCalling;
using ragecraft.UtilsScript;

namespace ragecraft.MultiUnitControllSystem_RBUR
{
    public class SyncDoorSW : syncSW_Base
    {
        [Header("ドア開閉はMultiUnitControllSystemにて制御されるので設定不要です")]
        [Header("↓のEventReceiverMUCSのみ設定すること")]
        [SerializeField] protected MultiUnitControllSystem eventReceiverMUCS;

        protected override void Start()
        {
            if(eventReceiverMUCS == null)
            {
                Debug.LogError("DoorSW:eventReceiverMUCS NULL!");
                return;
            }
            base.Start();
        }

        protected override void SomeUpdate()
        {
            base.SomeUpdate();
            if(useAnimator)
            {
                int i;
                for (i = 0; i < setParameterAnimator.Length; i++)
                {
                    setParameterAnimator[i].SetBool(setParameterNameID, udonSyncedBool[0]);
                }
            }
            eventReceiverMUCS.DoorStateUpdate();
        }
    }
}
