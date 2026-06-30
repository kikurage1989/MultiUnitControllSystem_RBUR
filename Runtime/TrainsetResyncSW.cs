
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
        [Header("編成単位でのリシンク要求を行なうスイッチ")]
        [SerializeField] private Train[] targetTrains;
        [SerializeField] private Controller_Base zengoSW1e;
        [SerializeField] private EndChangeSW EndChangeSW1e;
        [SerializeField] private Controller_Base zengoSW2e;
        [SerializeField] private EndChangeSW EndChangeSW2e;
        private bool[] isInterval = new bool[6];
        private bool isIntervalOwner;

        public override void Interact()
        {
            if(isInterval[0])
            {
                Debug.Log("TrainsetResyncSW:RequestCoolTime");
                return;
            }
            isInterval[0] = true;
            SendCustomEventDelayedSeconds(nameof(IntervalTimer), 5f);//連打防止
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(SyncEvent));
        }

        [NetworkCallable]
        public void SyncEvent() //全員で実行
        {
            if(!isInterval[1])
            {
                foreach(Train _train in targetTrains)
                {
                    if(Networking.IsOwner(_train.gameObject))
                    {
                        _train.resync();
                        isInterval[1] = true;
                    }
                }
            }
            if(!isInterval[2] && Networking.IsOwner(EndChangeSW1e.gameObject))
            {
                EndChangeSW1e.ResyncRequest();
                isInterval[2] = true;
            }
            if(!isInterval[3] && Networking.IsOwner(EndChangeSW2e.gameObject))
            {
                EndChangeSW2e.ResyncRequest();
                isInterval[3] = true;
            }
            if(!isInterval[4] && Networking.IsOwner(zengoSW1e.gameObject))
            {
                zengoSW1e.OnDrop();
                isInterval[4] = true;
            }
            if(!isInterval[5] && Networking.IsOwner(zengoSW2e.gameObject))
            {
                zengoSW2e.OnDrop();
                isInterval[5] = true;
            }

            if(isIntervalOwner)
            {
                Debug.Log("TrainsetResyncSW:OwnerRequest CoolTime");
                return;
            }
            for(int i = 1; i < isInterval.Length; i++) isIntervalOwner |= isInterval[i];
            if(isIntervalOwner) SendCustomEventDelayedSeconds(nameof(IntervalTimerOwner), 5f);//連打防止
        }

        [NetworkCallable]
        public void IntervalTimer()
        {
            isInterval[0] = false;
        }
        [NetworkCallable]
        public void IntervalTimerOwner()
        {
            isIntervalOwner = false;
            for(int i = 1; i < isInterval.Length; i++)
            {
                isInterval[i] = false;
            }
        }
    }
}
