
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
    public class EndChangeSW : syncSW_Base
    {
        [SerializeField, Tooltip("UseでeventReceiverMUCSのvoid UpdateControllerEnableProcess()を呼び出します")] protected MultiUnitControllSystem eventReceiverMUCS;

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
            //なにかUdonSynced変数更新に付随した処理
            eventReceiverMUCS.UpdateControllerEnableProcess();
        }

        [NetworkCallable]
        public void ResyncRequest()
        {
            RequestSerialization();//これが通るのはオーナのプレイヤーだけ
            //新規joinプレイヤーにも同期変数が受信される
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            //プレイヤーがjoinするとこのメソッドが全プレイヤーで実行される
            SendCustomEventDelayedSeconds(nameof(ResyncRequest), UnityEngine.Random.Range(3f, 5f));
        }
    }
}