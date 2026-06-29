
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3;
using VRC.Udon.Common.Interfaces;
using VRC.SDK3.UdonNetworkCalling;
using frou01.RigidBodyTrain;
using frou01.GrabController;
using ragecraft.UtilsScript;

namespace ragecraft.MultiUnitControllSystem_RBUR
{
    public class TrainsetResyncSW : UdonSharpBehaviour
    {
        [SerializeField] private Train[] targetTrains;
        [SerializeField] private Controller_Base zengoSW1e;
        [SerializeField] private EndChangeSW EndChangeSW1e;
        [SerializeField] private Controller_Base zengoSW2e;
        [SerializeField] private EndChangeSW EndChangeSW2e;
        private bool isInterval;

        public override void Interact()
        {
            if(isInterval)
            {
                Debug.Log("TrainsetResyncSW:RequestCoolTime");
                return;
            }
            isInterval = true;
            SendCustomEventDelayedSeconds(nameof(IntervalTimer), 5f);//連打防止
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(SyncEvent));
        }

        [NetworkCallable]
        public void SyncEvent() //全員で実行
        {
            foreach(Train _train in targetTrains)
            {
                if(Networking.IsOwner(_train.gameObject)) _train.resync();
            }
            if(Networking.IsOwner(EndChangeSW1e.gameObject)) EndChangeSW1e.ResyncRequest();
            if(Networking.IsOwner(EndChangeSW2e.gameObject)) EndChangeSW2e.ResyncRequest();
            if(Networking.IsOwner(zengoSW1e.gameObject)) zengoSW1e.OnDrop();
            if(Networking.IsOwner(zengoSW2e.gameObject)) zengoSW2e.OnDrop();
        }

        [NetworkCallable]
        public void IntervalTimer()
        {
            isInterval = false;
        }
    }
}
