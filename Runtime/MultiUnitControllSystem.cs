using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.UdonNetworkCalling;
using VRC.SDKBase;

namespace ragecraft.MultiUnitControllSystem_RBUR
{
    public class MultiUnitControllSystem : frou01.RigidBodyTrain.TrainConnectionReciever
    {
        [SerializeField, Tooltip("1エンド側Train")] protected frou01.RigidBodyTrain.Train end1Train;
        [SerializeField, Tooltip("2エンド側Train")] protected frou01.RigidBodyTrain.Train end2Train;
        [Header("1エンド側")]
        [SerializeField] protected Controller_Base notchLever1e;
        [SerializeField] protected Controller_Base brakeLever1e;
        [SerializeField] protected Controller_Base reverser1e;
        [SerializeField] protected Controller_Base zengoSW1e;
        [Header("2エンド側")]
        [SerializeField] protected Controller_Base notchLever2e;
        [SerializeField] protected Controller_Base brakeLever2e;
        [SerializeField] protected Controller_Base reverser2e;
        [SerializeField] protected Controller_Base zengoSW2e;

        protected int[] notchSegment1e = new int[1];
        protected float[] notchPosition1e = new float[1];
        protected float[] notchNormPosition1e = new float[1];
        protected int[] notchSegment2e = new int[1];
        protected int[] notchSegment1e = new int[1];
        protected float[] notchPosition1e = new float[1];
        protected float[] notchNormPosition1e = new float[1];

        protected bool isInit = false;
        protected virtual void Start()
        {
            
            isInit = true;
        }

        protected virtual void Update()
        {
            if(!isInit) return;
        }

        public override void TrainConnectionUpdate(frou01.RigidBodyTrain.Train connectedTrain, bool F_B)
        {
            if (connectedTrain != null)
            {
                //2両対策
                if((F_B && connectedTrain == end1Train) || (!F_B && connectedTrain == end2Train)) return;

                frou01.RigidBodyTrain.TrainConnectionReciever foundModule = connectedTrain.GetConnectionRecieverByTag(connectionTags[0]);
                if (foundModule)
                {
                    foundModule = (MultiUnitControllSystem) foundModule;
                    if (F_B) //1e側接続処理
                    {

                    }
                    else //2e側接続処理
                    {

                    }
                }
            }
            else
            {
                if (F_B)
                {
                }
                else
                {
                }
            }
        }
    }
}
