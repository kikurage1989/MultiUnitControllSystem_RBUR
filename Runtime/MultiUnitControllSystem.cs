
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.UdonNetworkCalling;
using VRC.SDKBase;

//編成の要件
//Train_PrefabのFCouplerObjがついている方を1エンド側と定義する
//編成の前後でFCouplerObj、BCouplerObjが揃うこと

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
        protected int[] brakeSegment1e = new int[1];
        protected float[] brakePosition1e = new float[1];
        protected float[] brakeNormPosition1e = new float[1];
        protected int[] reverserSegment1e = new int[1];
        protected int[] zengoSwSegment1e = new int[1];

        protected int[] notchSegment2e = new int[1];
        protected float[] notchPosition2e = new float[1];
        protected float[] notchNormPosition2e = new float[1];
        protected int[] brakeSegment2e = new int[1];
        protected float[] brakePosition2e = new float[1];
        protected float[] brakeNormPosition2e = new float[1];
        protected int[] reverserSegment2e = new int[1];
        protected int[] zengoSwSegment2e = new int[1];

        [System.NonSerialized] public int[] transport_int = new int[5];
        //予約分
        // 0　notchPos　ATSなどの処理済みの値　セグメント
        // 1　brakePos　ブレーキハンドルセグメント

        [System.NonSerialized] public float[] transport_float = new float[8];
        //予約分
        // 0　forceDirection　1エンド方向を1f、中立で0f、2エンド方向で-1f　方向性
        // 1　notchPosition　ノッチハンドルポジション
        // 2　notchNormPosition　ノッチハンドル正規化ポジション
        // 3　brakePosition　ブレーキハンドルポジション
        // 4　brakeNormPosition　ブレーキハンドル正規化ポジション

        [System.NonSerialized] public bool[] transport_bool = new bool[8];
        //予約分
        // 0　信号方向：　false 1e->2e　True　2e->1e　方向性
        // 1　進行方向決定済：Trueで決定済。運転台とか後端とかはこれで決定
        // 2　EnablePermission　エンジン始動とかパン上げとか
        // 3　ATS_EmerStop

        [System.NonSerialized] public bool[] transport_bool_fromFront = new bool[12];
        //　前後同時送信し、(いずれかの車両で)という条件を取るためのもの
        // 予約分
        // 0　FrontCheck
        // 1　BuzzerPushedFromFront
        // 2　isSitsunaitouFromFront
        // 3　isLeftDoorOpenFromFront
        // 4　isLeftDoorKeyFromFront
        // 5　isRIghtDoorOpenFromFront
        // 6　isRIghtDoorKeyFromFront

        [System.NonSerialized] public bool[] transport_bool_fromBack = new bool[12];
        // 予約分
        // 0　BackCheck
        // 1　BuzzerPushedFromBack
        // 2　isSitsunaitouFromBack
        // 3　isLeftDoorOpenFromBack
        // 4　isLeftDoorKeyFromBack
        // 5　isRIghtDoorOpenFromBack
        // 6　isRIghtDoorKeyFromBack

        protected int[] transport_int_from1e = new int[5];
        protected float[] transport_float_from1e = new float[8];
        protected bool[] transport_bool_from1e = new bool[8];
        protected bool[] transport_bool_fromFront_from1e = new bool[12];
        protected bool[] transport_bool_fromBack_from1e = new bool[12];

        protected int[] transport_int_from2e = new int[5];
        protected float[] transport_float_from2e = new float[8];
        protected bool[] transport_bool_from2e = new bool[8];
        protected bool[] transport_bool_fromFront_from2e = new bool[12];
        protected bool[] transport_bool_fromBack_from2e = new bool[12];
        
        protected bool[] isConnectedOtherCar = new bool[2]; //通信接続がされているかのフラグ 0:1エンド側 1:2エンド側
        protected bool[] isConnectedTo2eCoupler = new bool[2]; //接続した相手車両の接続カプラが2エンド側か　Falseで1エンド側 0:自車1エンド側 1:自車2エンド側 isConnectedOtherCarと組み合わせること


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

            notchSegment2e = notchLever2e.currentSegment_Exposed;
            notchPosition2e = notchLever2e.controllerPosition_Exposed;
            notchNormPosition2e = notchLever2e.currentNormalizePosition_Exposed;
            brakeSegment2e = brakeLever2e.currentSegment_Exposed;
            brakePosition2e = brakeLever2e.controllerPosition_Exposed;
            brakeNormPosition2e = brakeLever2e.currentNormalizePosition_Exposed;
            reverserSegment2e = reverser2e.currentSegment_Exposed;
            zengoSwSegment2e = zengoSW2e.currentSegment_Exposed;

            isInit = true;
        }

        protected virtual void Update()
        {
            if(!isInit) return;
        }

        //MARK:総括制御処理
        //Train.csから増解結時に呼び出される connectedTrain:接続先Train bool F_B前カプラ側かどうか
        protected MultiUnitControllSystem connectedModule_1e = null;
        protected MultiUnitControllSystem connectedModule_2e = null;
        public override void TrainConnectionUpdate(frou01.RigidBodyTrain.Train connectedTrain, bool F_B)
        {
            if (connectedTrain != null)
            {
                //2両対策
                if((F_B && connectedTrain == end1Train) || (!F_B && connectedTrain == end2Train)) return;

                frou01.RigidBodyTrain.TrainConnectionReciever foundModule = connectedTrain.GetConnectionRecieverByTag(connectionTags[0]);
                if (foundModule)
                {
                    // foundModule = (MultiUnitControllSystem) foundModule;
                    if (F_B) //1e側接続処理
                    {
                        connectedModule_1e = (MultiUnitControllSystem) foundModule;
                        isConnectedOtherCar[0] = true;
                        
                        transport_int_from1e = connectedModule_1e.transport_int;
                        transport_float_from1e = connectedModule_1e.transport_float;
                        transport_bool_from1e = connectedModule_1e.transport_bool;
                        transport_bool_fromFront_from1e = connectedModule_1e.transport_bool_fromFront;
                        transport_bool_fromBack_from1e = connectedModule_1e.transport_bool_fromBack;
                    }
                    else //2e側接続処理
                    {
                        connectedModule_2e = (MultiUnitControllSystem) foundModule;
                        isConnectedOtherCar[1] = true;

                        transport_int_from2e = connectedModule_2e.transport_int;
                        transport_float_from2e = connectedModule_2e.transport_float;
                        transport_bool_from2e = connectedModule_2e.transport_bool;
                        transport_bool_fromFront_from2e = connectedModule_2e.transport_bool_fromFront;
                        transport_bool_fromBack_from2e = connectedModule_2e.transport_bool_fromBack;
                    }
                }
            }
            else
            {
                if (F_B)
                {
                    connectedModule_1e = null;
                    isConnectedOtherCar[0] = false;
                    transport_int_from1e = null;
                    transport_float_from1e = null;
                    transport_bool_from1e = null;
                    transport_bool_fromFront_from1e = null;
                    transport_bool_fromBack_from1e = null;
                }
                else
                {
                    connectedModule_2e = null;
                    isConnectedOtherCar[1] = false;
                    transport_int_from2e = null;
                    transport_float_from2e = null;
                    transport_bool_from2e = null;
                    transport_bool_fromFront_from2e = null;
                    transport_bool_fromBack_from2e = null;
                }
            }
            ChangeZengoSwEvent();
        }
        
        public void ChangeZengoSwEvent()
        {
            SendCustomEventDelayedFrames(nameof(SendDirectionUpdate), 1);
        }
        public void SendDirectionUpdateProcess()
        {
            //前位置もしくは後位置があるなら信号方向は決定してよい
            //前後切替SW：前　確認
            transport_bool_fromFront[0] = (zengoSwSegment1e[0] == 0) || (zengoSwSegment2e[0] == 0);
            if(zengoSwSegment1e[0] == 0) transport_bool[0] = false;
            else if(zengoSwSegment2e[0] == 0) transport_bool[0] = true;

            //前後切替SW：後　確認
            transport_bool_fromBack[0] = (zengoSwSegment1e[0] == 2) || (zengoSwSegment2e[0] == 2);
            if(zengoSwSegment1e[0] == 2) transport_bool[0] = true;
            else if(zengoSwSegment2e[0] == 2) transport_bool[0] = false;
            
            transport_bool[1] = (zengoSwSegment1e[0] == 0) || (zengoSwSegment2e[0] == 0) || (zengoSwSegment1e[0] == 2) || (zengoSwSegment2e[0] == 2);
            
            //中間車信号方向決定処理
            if((!isConnectedOtherCar[0] && (zengoSwSegment1e[0] == 1)) || (!isConnectedOtherCar[1] && (zengoSwSegment2e[1] == 1))) transport_bool[1] = false;
            else if((isConnectedOtherCar[0] && (zengoSwSegment1e[0] == 1)) && (isConnectedOtherCar[1] && (zengoSwSegment2e[1] == 1)))
            {
                //1エンド側確認
                //1エンド側の接続先が相手側1エンド側
                if(end1Train.connectedTrain_F.connectedTrain_F == end1Train)
                {
                    isConnectedTo2eCoupler[0] = false;
                    transport_bool[1] = transport_bool_from1e[0] && transport_bool_from1e[1];//相手側信号方向が2e->1eで方向決定しているなら
                }
                else if(end1Train.connectedTrain_F.connectedTrain_B == end1Train)//1エンド側の接続先が相手側2エンド側 
                {
                    isConnectedTo2eCoupler[0] = true;
                    transport_bool[1] = !transport_bool_from1e[0] && transport_bool_from1e[1];//相手側信号方向が1e->2eで方向決定しているなら
                }
                if(transport_bool[1]) transport_bool[0] = false; //信号方向1e->2e

                //2エンド側で確認
                if(end2Train.connectedTrain_B.connectedTrain_F == end1Train)//2エンド側の接続先が相手側1エンド側
                {
                    isConnectedTo2eCoupler[1] = false;
                    transport_bool[1] = transport_bool_from2e[0] && transport_bool_from2e[1];//相手側信号方向が2e->1eで方向決定しているなら
                }
                else if(end1Train.connectedTrain_B.connectedTrain_B == end1Train)//2エンド側の接続先が相手側2エンド側 
                {
                    isConnectedTo2eCoupler[1] = true;
                    transport_bool[1] = !transport_bool_from2e[0] && transport_bool_from2e[1];//相手側信号方向が1e->2eで方向決定しているなら
                }
                if(transport_bool[1]) transport_bool[0] = true; //信号方向2e->1e
                //ここよくない
            }
        }
        public void SendDirectionUpdate()
        {
            SendDirectionUpdateProcess();

            if(isConnectedOtherCar[0]) connectedModule_1e.SendDirectionUpdateProcess();
            if(isConnectedOtherCar[1]) connectedModule_2e.SendDirectionUpdateProcess();

            Debug.Log("ChangeZengoSw:1e" + zengoSwSegment1e[0] + " 2e:" + zengoSwSegment2e[0]);
        }
    }
}
