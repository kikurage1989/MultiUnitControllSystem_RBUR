
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
    public class SyncDoorKeySW : syncSW_Base
    {
        [Header("ドア鍵自体のコライダーOnOffはMultiUnitControllSystemにて制御する")]
        [Header("↓のDoorSwColliderのみ設定すること")]
        [SerializeField] protected Collider doorSwCollider;
        protected MeshRenderer doorKeySwMesh;

        protected override void Start()
        {
            doorKeySwMesh = GetComponent<MeshRenderer>();
            if(doorKeySwMesh == null)
            {
                Debug.LogError("DoorKeySW:MeshRenderer NULL!");
                return;
            }
            if(doorKeySwMesh == null)
            {
                Debug.LogError("DoorKeySW:MeshRenderer NULL!");
                return;
            }
            doorKeySwMesh.enabled = doorSwCollider.enabled = false;
            base.Start();
        }

        protected override void SomeUpdate()
        {
            doorKeySwMesh.enabled = doorSwCollider.enabled = udonSyncedBool[0];
        }
    }
}
