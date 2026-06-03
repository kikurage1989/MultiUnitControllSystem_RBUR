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

        [SerializeField] protected int[] notchSegment1e = new int[1];
        [SerializeField] protected float[] notchPosition1e = new float[1];
        [SerializeField] protected float[] notchNormPosition1e = new float[1];
        [SerializeField] protected int[] brakeSegment1e = new int[1];
        [SerializeField] protected float[] brakePosition1e = new float[1];
        [SerializeField] protected float[] brakeNormPosition1e = new float[1];
        [SerializeField] protected int[] reverserSegment1e = new int[1];
        [SerializeField] protected int[] zengoSwSegment1e = new int[1];

        [SerializeField] protected int[] notchSegment2e = new int[1];
        [SerializeField] protected float[] notchPosition2e = new float[1];
        [SerializeField] protected float[] notchNormPosition2e = new float[1];
        [SerializeField] protected int[] brakeSegment2e = new int[1];
        [SerializeField] protected float[] brakePosition2e = new float[1];
        [SerializeField] protected float[] brakeNormPosition2e = new float[1];
        [SerializeField] protected int[] reverserSegment2e = new int[1];
        [SerializeField] protected int[] zengoSwSegment2e = new int[1];

        protected bool isInit = false;
        protected virtual void Start()
        {
            notchSegment1e = notchLever1e.currentSegment_Exposed;
            notchPosition1e = notchLever1e.controllerPosition_Exposed;
            notchNormPosition1e = notchLever1e.currentNormalizePosition_Exposed;
            brakeSegment1e = brakeLever1e.currentSegment_Exposed;
            brakePosition1e = brakeLever1e.controllerPosition_Exposed;
            brakeNormPosition1e = brakeLever1e.currentNormalizePosition_Exposed;
            reverserSegment1e = reverser1e.currentSegment_Exposed;
            zengoSwSegment1e = zengoSW1e.currentSegment_Exposed;

            // notchSegment2e = new int[1];
            // notchPosition2e = new float[1];
            // notchNormPosition2e = new float[1];
            // brakeSegment2e = new int[1];
            // brakePosition2e = new float[1];
            // brakeNormPosition2e = new float[1];
            // reverserSegment2e = new int[1];
            // zengoSwSegment2e = new int[1];
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
