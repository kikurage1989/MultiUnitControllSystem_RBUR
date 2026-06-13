
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.UdonNetworkCalling;
using VRC.SDKBase;
using TMPro; //TextMeshProを扱う際に必要
using System;//Stringに使用

//編成の要件
//Train_PrefabのFCouplerObjがついている方を1エンド側と定義する
//編成の前後でFCouplerObj、BCouplerObjが揃うこと
//1ハンドル車を作成したい場合は、brakeLever1e、brakeLever2e　をNoneにすること

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
        [System.NonSerialized] public int[] zengoSwSegment1e = new int[1];

        protected int[] notchSegment2e = new int[1];
        protected float[] notchPosition2e = new float[1];
        protected float[] notchNormPosition2e = new float[1];
        protected int[] brakeSegment2e = new int[1];
        protected float[] brakePosition2e = new float[1];
        protected float[] brakeNormPosition2e = new float[1];
        protected int[] reverserSegment2e = new int[1];
        [System.NonSerialized] public int[] zengoSwSegment2e = new int[1];

        [System.NonSerialized] public int[] transport_int = new int[5];
        //予約分
        // 0　notchPos　ATSなどの処理済みの値　セグメント
        // 1　brakeSeg　ブレーキハンドルセグメント

        [System.NonSerialized] public float[] transport_float = new float[8];
        //予約分
        // 0　powerDirection　1エンド方向を1f、中立で0f、2エンド方向で-1f　方向性
        // 1　brakePosition　ブレーキハンドルポジション
        // 2　brakeNormPosition　ブレーキハンドル正規化ポジション

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
        protected int[] zengoSwSegment_from1e = new int[1];

        protected int[] transport_int_from2e = new int[5];
        protected float[] transport_float_from2e = new float[8];
        protected bool[] transport_bool_from2e = new bool[8];
        protected bool[] transport_bool_fromFront_from2e = new bool[12];
        protected bool[] transport_bool_fromBack_from2e = new bool[12];
        protected int[] zengoSwSegment_from2e = new int[1];
        
        protected bool[] isConnectedOtherCar = new bool[2]; //通信接続がされているかのフラグ 0:1エンド側 1:2エンド側
        protected bool[] isConnectedTo2eCoupler = new bool[2]; //接続した相手車両の接続カプラが2エンド側か　Falseで1エンド側 0:自車1エンド側 1:自車2エンド側 isConnectedOtherCarと組み合わせること

        [SerializeField] protected int notchSegmentLocal; //ノッチハンドルから読み取り
        // [SerializeField] protected float notchPositionLocal; //ノッチハンドルから読み取り
        // [SerializeField] protected float notchNormPosLocal; //ノッチハンドルから読み取り
        [SerializeField] protected int brakeSegmentLocal; //ブレーキハンドルから読み取り
        [SerializeField] protected float brakePositionLocal; //ブレーキハンドルから読み取り
        [SerializeField] protected float brakeNormPosLocal; //ブレーキハンドルから読み取り
        [SerializeField] protected int notchPos; //力行処理に使用する、処理済のノッチ数
        [SerializeField] protected int brakeSeg; //制動処理に使用する、処理済のブレーキハンドルセグメント
        [SerializeField] protected float brakePos; //制動処理に使用する、処理済のブレーキハンドルポジション
        [SerializeField] protected float brakeNormPos; //制動処理に使用する、処理済のブレーキハンドル正規化ポジション
        [SerializeField] protected byte dataDirectionMode = 0; //送受信モード
        [SerializeField] protected float powerDirection = 0f;//1: 1エンド側　-1：2エンド側

        protected bool canReadFrom1e;//1エンド側から読み出し可
        protected bool canReadFrom2e;//2エンド側から読み出し可

        [SerializeField] protected TextMeshPro debugText; //デバッグ表示用TextMeshPro
        
        protected bool has2Handle = true;
        protected bool isInit = false;
        protected bool isOwnerState;
        protected float updateDeltaTime = 0.01f;//念のため

        //開発用仮実装
        [SerializeField] protected bool debug_flg;
        [SerializeField] protected frou01.RigidBodyTrain.MortorAndWheel _MotorAndWheel;
        protected float[] outputMortorForce = new float[1];
        protected float[] outputBrakeForce = new float[1];

        protected virtual void Start()
        {
            isOwnerState = Networking.IsOwner(this.gameObject);
            notchSegment1e = notchLever1e.currentSegment_Exposed;
            // notchPosition1e = notchLever1e.controllerPosition_Exposed;
            // notchNormPosition1e = notchLever1e.currentNormalizePosition_Exposed;
            brakeSegment1e = brakeLever1e.currentSegment_Exposed;
            brakePosition1e = brakeLever1e.controllerPosition_Exposed;
            brakeNormPosition1e = brakeLever1e.currentNormalizePosition_Exposed;
            reverserSegment1e = reverser1e.currentSegment_Exposed;
            zengoSwSegment1e = zengoSW1e.currentSegment_Exposed;

            notchSegment2e = notchLever2e.currentSegment_Exposed;
            // notchPosition2e = notchLever2e.controllerPosition_Exposed;
            // notchNormPosition2e = notchLever2e.currentNormalizePosition_Exposed;
            brakeSegment2e = brakeLever2e.currentSegment_Exposed;
            brakePosition2e = brakeLever2e.controllerPosition_Exposed;
            brakeNormPosition2e = brakeLever2e.currentNormalizePosition_Exposed;
            reverserSegment2e = reverser2e.currentSegment_Exposed;
            zengoSwSegment2e = zengoSW2e.currentSegment_Exposed;

            has2Handle = Utilities.IsValid(brakeLever1e) && Utilities.IsValid(brakeLever2e);

            isInit = true;
            //開発用仮実装
            outputMortorForce = _MotorAndWheel.MortorForce;
            outputBrakeForce = _MotorAndWheel.BrakeForce;
        }
        
        protected virtual void Update()
        {
            if(!isInit) return;
            updateDeltaTime = Time.deltaTime;

            //ハンドル位置読み取り
            switch(dataDirectionMode)
            {
                case 0://[前][後]
                case 1://[前][中]
                    notchSegmentLocal = notchSegment1e[0];
                    brakeSegmentLocal = brakeSegment1e[0];
                    brakePositionLocal = brakePosition1e[0];
                    brakeNormPosLocal = brakeNormPosition1e[0];
                    powerDirection = reverserSegment1e[0] - 1f;
                    DecideNotchAndBrakePos();
                    break;
                case 2://[後][前]
                case 3://[中][前]
                    notchSegmentLocal = notchSegment2e[0];
                    brakeSegmentLocal = brakeSegment2e[0];
                    brakePositionLocal = brakePosition2e[0];
                    brakeNormPosLocal = brakeNormPosition2e[0];
                    powerDirection = -1f * (reverserSegment2e[0] - 1f);
                    DecideNotchAndBrakePos();
                    break;
                case 4://[中][後]
                    ReadControllerParametersFrom1e();
                    break;
                case 5://[後][中]
                    ReadControllerParametersFrom2e();
                    break;
                case 6://[中][中]かつ送信方向が1e->2eで決定済
                    ReadControllerParametersFrom1e();
                    break;
                case 7://[中][中]かつ送信方向が2e->1eで決定済
                    ReadControllerParametersFrom2e();
                    break;
                default:
                    break;
            }

            //力行・制動処理
            if(isOwnerState) PowerAndBrakeProcess();

            //送信処理
            //操作系変数送信
            if(dataDirectionMode <= 3 || dataDirectionMode == 6 || dataDirectionMode == 7)
            {
                transport_int[0] = notchPos;
                transport_int[1] = brakeSeg;
                transport_float[1] = brakePos;
                transport_float[2] = brakeNormPos;
                //方向性信号
                if(!transport_bool[0]) //2e側へ送信
                {
                    transport_float[0] = -1f * powerDirection;
                }
                else //1e側へ送信
                {
                    transport_float[0] = powerDirection;
                }
                
            }

            //Debug表示
            if(Utilities.IsValid(debugText))
            {
                string dis_text = "";
                dis_text += "DateDirectionMode: " + dataDirectionMode + "\n";
                dis_text += "DateDirection: " + (transport_bool[1] ? (!transport_bool[0] ? "1e -> 2e" : "2e -> 1e") : "未定義") + "\n";
                dis_text += "canReadFrom1e: " + canReadFrom1e + "\n";
                dis_text += "canReadFrom2e: " + canReadFrom2e + "\n";
                dis_text += "notchSegmentLocal: " + notchSegmentLocal + "\n";
                dis_text += "brakeSegmentLocal: " + brakeSegmentLocal + "\n";
                dis_text += "brakePositionLocal: " + brakePositionLocal + "\n";
                dis_text += "brakeNormPosLocal: " + brakeNormPosLocal + "\n";
                dis_text += "notchPos: " + notchPos + "\n";
                dis_text += "brakeSeg: " + brakeSeg + "\n";
                dis_text += "brakePos: " + brakePos + "\n";
                dis_text += "brakeNormPos: " + brakeNormPos + "\n";
                dis_text += "powerDirection: " + powerDirection + "\n";
                dis_text += "FrontCheck: " + transport_bool_fromFront[0] + "\n";
                dis_text += "BackCheck: " + transport_bool_fromBack[0] + "\n";
                // dis_text += String.Format("oil:{0:000.00}", convertorOilTemperature) + "\n";
                // dis_text += String.Format("EC_Mcal:{0:000.00}", engineChargeKcal / 1000f) + "\n";
                debugText.text = dis_text;
            }
        }

        protected virtual void DecideNotchAndBrakePos()
        {
            notchPos = notchSegmentLocal;
            brakeSeg = brakeSegmentLocal;
            brakePos = brakePositionLocal;
            brakeNormPos = brakeNormPosLocal;
        }
        protected virtual void PowerAndBrakeProcess()
        {
            //Update内でオーナーのみ実行
            //Time.deltaTimeの値はupdateDeltaTimeに格納済
            //開発用仮実装
            // outputMortorForce[0] = powerDirection * 10000f * notchPos;
            // outputBrakeForce[0] = (brakeSeg == 0 ? 0f : 70000f * brakeNormPos);
        }

        //操作系変数読み取り処理
        protected void ReadControllerParametersFrom1e()
        {
            if(canReadFrom1e)
            {
                notchPos = transport_int_from1e[0];
                brakeSeg = transport_int_from1e[1];
                brakePos = transport_float_from1e[1];
                brakeNormPos = transport_float_from1e[2];
                powerDirection = -1f * transport_float_from1e[0];
            }
        }
        protected void ReadControllerParametersFrom2e()
        {
            if(canReadFrom2e)
            {
                notchPos = transport_int_from2e[0];
                brakeSeg = transport_int_from2e[1];
                brakePos = transport_float_from2e[1];
                brakeNormPos = transport_float_from2e[2];
                powerDirection = transport_float_from2e[0];
            }
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
                    zengoSwSegment_from1e = null;
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
                    zengoSwSegment_from2e = null;
                }
            }
            ChangeZengoSwEvent();
        }
        
        public void ChangeZengoSwEvent()
        {
            SendCustomEventDelayedFrames(nameof(SendDirectionUpdate), 1);
        }

        protected byte retryCount = 0;
        public void SendDirectionUpdateProcess()
        {
            if(!isInit) return;
            //読み出し可能フラグ更新 canReadFrom1e canReadFrom2e
            if(isConnectedOtherCar[0]) canReadFrom1e = (zengoSwSegment1e[0] == 1) && (zengoSwSegment_from1e[0] == 1);
            else canReadFrom1e = false;
            if(isConnectedOtherCar[1]) canReadFrom2e = (zengoSwSegment2e[0] == 1) && (zengoSwSegment_from2e[0] == 1);
            else canReadFrom2e = false;

            //前位置もしくは後位置があるなら信号方向は決定してよい
            //前後切替SW：前　確認
            transport_bool_fromFront[0] = (zengoSwSegment1e[0] == 2) || (zengoSwSegment2e[0] == 2);
            if(zengoSwSegment1e[0] == 2) transport_bool[0] = false;
            else if(zengoSwSegment2e[0] == 2) transport_bool[0] = true;

            //前後切替SW：後　確認
            transport_bool_fromBack[0] = (zengoSwSegment1e[0] == 0) || (zengoSwSegment2e[0] == 0);
            if(zengoSwSegment1e[0] == 0) transport_bool[0] = true;
            else if(zengoSwSegment2e[0] == 0) transport_bool[0] = false;
            
            //中間車信号方向決定処理　前後切替SW前後とも中位置
            if((zengoSwSegment1e[0] == 1) && (zengoSwSegment2e[0] == 1))
            {
                transport_bool_fromFront[0] = false;

                if(isConnectedOtherCar[0])
                {
                    if((zengoSwSegment_from1e[0] == 1) && transport_bool_from1e[1])
                    {
                        if(isConnectedTo2eCoupler[0] && !transport_bool_from1e[0] && transport_bool_fromFront_from1e[0])
                        {
                            transport_bool[0] = false;
                            transport_bool[1] = transport_bool_fromFront[0] = true;
                        }
                        else if(!isConnectedTo2eCoupler[0] && transport_bool_from1e[0] && transport_bool_fromFront_from1e[0])
                        {
                            transport_bool[0] = true;
                            transport_bool[1] = transport_bool_fromFront[0] = true;
                        }
                        else transport_bool[1] = transport_bool_fromFront[0] = false;
                    }
                    else transport_bool[1] = transport_bool_fromFront[0] = false;
                }
                if(!transport_bool[1] && isConnectedOtherCar[1])//1エンド側で送信方向が決定しない場合のみ2エンド側を確認
                {
                    if((zengoSwSegment_from2e[0] == 1) && transport_bool_from2e[1])
                    {
                        if(isConnectedTo2eCoupler[1] && !transport_bool_from2e[0] && transport_bool_fromFront_from2e[0])
                        {
                            transport_bool[0] = true;
                            transport_bool[1] = transport_bool_fromFront[0] = true;
                        }
                        else if(!isConnectedTo2eCoupler[1] && transport_bool_from2e[0] && transport_bool_fromFront_from2e[0])
                        {
                            transport_bool[0] = false;
                            transport_bool[1] = transport_bool_fromFront[0] =true;
                        }
                        else transport_bool[1] = transport_bool_fromFront[0] =false;
                    }
                    else transport_bool[1] = transport_bool_fromFront[0] = false;
                }
            }
            else if(((zengoSwSegment1e[0] == 2) && (zengoSwSegment2e[0] == 2)) || ((zengoSwSegment1e[0] == 0) && (zengoSwSegment2e[0] == 0))) transport_bool[1] = transport_bool_fromFront[0] = false;
            else transport_bool[1] = true;

            //送受信モード決定 dataDirectionMode
            if((zengoSwSegment1e[0] == 2) && (zengoSwSegment2e[0] == 0)) dataDirectionMode = 0;
            else if((zengoSwSegment1e[0] == 2) && (zengoSwSegment2e[0] == 1)) dataDirectionMode = 1;
            else if((zengoSwSegment1e[0] == 0) && (zengoSwSegment2e[0] == 2)) dataDirectionMode = 2;
            else if((zengoSwSegment1e[0] == 1) && (zengoSwSegment2e[0] == 2)) dataDirectionMode = 3;
            else if((zengoSwSegment1e[0] == 1) && (zengoSwSegment2e[0] == 0)) dataDirectionMode = 4;
            else if((zengoSwSegment1e[0] == 0) && (zengoSwSegment2e[0] == 1)) dataDirectionMode = 5;
            else if((zengoSwSegment1e[0] == 1) && (zengoSwSegment2e[0] == 1) && !transport_bool[0] && transport_bool[1]) dataDirectionMode = 6;
            else if((zengoSwSegment1e[0] == 1) && (zengoSwSegment2e[0] == 1) && transport_bool[0] && transport_bool[1]) dataDirectionMode = 7;
            else if(((zengoSwSegment1e[0] == 1) && (zengoSwSegment2e[0] == 1) && !transport_bool[1]) || ((zengoSwSegment1e[0] == 2) && (zengoSwSegment2e[0] == 2)) || ((zengoSwSegment1e[0] == 0) && (zengoSwSegment2e[0] == 0))) dataDirectionMode = 8;
            
        }
        public void SendDirectionUpdate()
        {
            if(isConnectedOtherCar[0])
            {
                if(end1Train.connectedTrain_F.connectedTrain_F == end1Train)
                {
                    zengoSwSegment_from1e = connectedModule_1e.zengoSwSegment1e;
                    isConnectedTo2eCoupler[0] = false;
                }
                else if(end1Train.connectedTrain_F.connectedTrain_B == end1Train)
                {
                    zengoSwSegment_from1e = connectedModule_1e.zengoSwSegment2e;
                    isConnectedTo2eCoupler[0] = true;
                }
            }
            if(isConnectedOtherCar[1])
            {
                if(end2Train.connectedTrain_B.connectedTrain_F == end2Train)
                {
                    zengoSwSegment_from2e = connectedModule_2e.zengoSwSegment1e;
                    isConnectedTo2eCoupler[1] = false;
                }
                else if(end2Train.connectedTrain_B.connectedTrain_B == end2Train)
                {
                    zengoSwSegment_from2e = connectedModule_2e.zengoSwSegment2e;
                    isConnectedTo2eCoupler[1] = true;
                }
            }

            SendDirectionUpdateProcess();

            if(isConnectedOtherCar[0]) connectedModule_1e.SendDirectionUpdateProcess();
            if(isConnectedOtherCar[1]) connectedModule_2e.SendDirectionUpdateProcess();
        }

        public override void OnOwnershipTransferred(VRC.SDKBase.VRCPlayerApi player)
        {
            isOwnerState = player == Networking.LocalPlayer;
        }
    }
}
