
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.UdonNetworkCalling;
using TMPro; //TextMeshProを扱う際に必要
using System;//Stringに使用
using ragecraft.UtilsScript;
using frou01.GrabController;
//MIT Lisence 2026 XID:kikurage1989_
//編成の要件
//Train_PrefabのFCouplerObjがついている方を1エンド側と定義する
//編成の前後でFCouplerObj、BCouplerObjが揃うこと
//1ハンドル車を作成したい場合は、brakeLever1e、brakeLever2e　をNoneにすること

namespace ragecraft.MultiUnitControllSystem_RBUR
{
    public class MultiUnitControllSystem : frou01.RigidBodyTrain.TrainConnectionReciever
    {
        // [SerializeField, Tooltip("車両コントローラー ATS非常読み取り 1エンド側、2エンド側車両のみ 単車は1個のみ")] protected Animator[] controllerAnimators;//車体メッシュアニメーションコントローラ ドア開閉
        [SerializeField, Tooltip("車両メッシュコントローラー ドア開閉( isOpenLeftDoor isOpenRightDoor )、室内灯( isRoomLight )")] protected Animator[] trainMeshAnimators;//車体メッシュアニメーションコントローラ ドア開閉
        [SerializeField, Tooltip("1エンド側Train")] protected frou01.RigidBodyTrain.Train end1Train;
        [SerializeField, Tooltip("2エンド側Train 単車の場合は同じ車両を指定")] protected frou01.RigidBodyTrain.Train end2Train;
        [SerializeField, Tooltip("ノッチ切セグメント")] protected int notchOffSegment = 0;
        [SerializeField, Tooltip("ブレーキハンドル抜取セグメント")] protected int brakeHandleNukitoriSegment = 2;
        [Header("1エンド側GAC")]
        [SerializeField] protected Controller_Base notchLever1e;
        protected Collider notchLeverColider1e;
        [SerializeField] protected Controller_Base brakeLever1e;
        protected GameObject brakeHandleMesh1e;
        protected Collider brakeLeverColider1e;
        [SerializeField] protected Controller_Base reverser1e;
        protected Collider reverserColider1e;
        [SerializeField] protected Controller_Base zengoSW1e;
        protected Collider zengoSWColider1e;
        [SerializeField] protected EndChangeSW EndChangeSW1e;
        protected Collider EndChangeSWColider1e;
        protected bool[] UseEnd1 = new bool[1];
        [Header("2エンド側GAC")]
        [SerializeField] protected Controller_Base notchLever2e;
        protected Collider notchLeverColider2e;
        [SerializeField] protected Controller_Base brakeLever2e;
        protected GameObject brakeHandleMesh2e;
        protected Collider brakeLeverColider2e;
        [SerializeField] protected Controller_Base reverser2e;
        protected Collider reverserColider2e;
        [SerializeField] protected Controller_Base zengoSW2e;
        protected Collider zengoSWColider2e;
        [SerializeField] protected EndChangeSW EndChangeSW2e;
        protected Collider EndChangeSWColider2e;
        protected bool[] UseEnd2 = new bool[1];
        [Header("左側（1エンド方向基準）ドアSw")]
        [SerializeField] protected SyncDoorSW _doorSwLeft1e;
        [SerializeField] protected SyncDoorSW _doorSwLeft2e;
        [Header("左側（1エンド方向基準）ドアSw鍵")]
        [SerializeField] protected SyncDoorKeySW _keySw1eL;
        protected Collider _doorKeySwCol1eL;
        [SerializeField] protected SyncDoorKeySW _keySw2eL;
        protected Collider _doorKeySwCol2eL;
        [Header("右側（1エンド方向基準）ドアSw")]
        [SerializeField] protected SyncDoorSW _doorSwRight1e;
        [SerializeField] protected SyncDoorSW _doorSwRight2e;
        [Header("右側（1エンド方向基準）ドアSw鍵")]
        [SerializeField] protected SyncDoorKeySW _keySw1eR;
        protected Collider _doorKeySwCol1eR;
        [SerializeField] protected SyncDoorKeySW _keySw2eR;
        protected Collider _doorKeySwCol2eR;
        [Header("ブザーSw")]
        [SerializeField] protected syncSW_Base buzzerSW;
        [SerializeField] protected AudioSource[] buzzerSnd;
        protected bool isBuzzerSwPushedAnyCar;
        protected bool prevIsBuzzerSwPushed;
        [Header("室内灯Sw")]
        [SerializeField] protected syncSW_Base roomLightSW;
        protected bool isRoomLightSwPushedAnyCar;
        protected bool prevIsRoomLightSwPushed;
        [Header("ATS_Reciever コライダー")]
        [SerializeField] protected Collider atsReceiver1e;
        [SerializeField] protected Collider atsReceiver2e;

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

        protected bool[] doorSwLeft1e = new bool[1];
        protected bool[] doorSwLeft2e = new bool[1];
        protected bool[] doorSwRight1e = new bool[1];
        protected bool[] doorSwRight2e = new bool[1];

        protected bool[] keySw1eL = new bool[1];
        protected bool[] keySw2eL = new bool[1];
        protected bool[] keySw1eR = new bool[1];
        protected bool[] keySw2eR = new bool[1];

        protected bool[] isBuzzerSw = new bool[1];
        protected bool[] isRoomLightSw = new bool[1];

        [System.NonSerialized] public int[] transport_int = new int[2];
        //予約分
        // 0　notchPos　ATSなどの処理済みの値　セグメント
        // 1　brakeSeg　ブレーキハンドルセグメント

        [System.NonSerialized] public float[] transport_float = new float[3];
        //予約分
        // 0　powerDirection　1エンド方向を1f、中立で0f、2エンド方向で-1f　方向性
        // 1　brakePosition　ブレーキハンドルポジション
        // 2　brakeNormPosition　ブレーキハンドル正規化ポジション

        [System.NonSerialized] public bool[] transport_bool = new bool[8];
        //予約分 2以降は運転台からのみ送信する情報
        // 0　信号方向：　false 1e->2e　True　2e->1e　方向性
        // 1　進行方向決定済：Trueで決定済。運転台とか後端とかはこれで決定
        // 2　EnablePermission　エンジン始動とかパン上げとか
        [SerializeField] protected bool EnablePermission;

        [System.NonSerialized] public bool[] transport_bool_Doors = new bool[8];
        //ドア専用(方向性があるため)　前後送信し、「いずれかの車両で」という条件を検知するもの
        // 0　isLeftDoorOpen_forFront
        // 1　isRightDoorOpen_forFront
        // 2　isLeftDoorKey_forFront
        // 3　isRIghtDoorKey_forFront
        // 4　isLeftDoorOpen_forBack
        // 5　isRightDoorOpen_forBack
        // 6　isLeftDoorKey_forBack
        // 7　isRightDoorKey_forBack

        [System.NonSerialized] public bool[] transport_bool_fromFront = new bool[8];
        // 予約分
        // 0　FrontCheck
        // 1　BuzzerPushed_fromFront
        // 2　isSitsunaitou_fromFront
        // 3  1e側の隣車へFrontを渡せる
        // 4  2e側の隣車へFrontを渡せる

        [System.NonSerialized] public bool[] transport_bool_fromBack = new bool[8];
        // 予約分
        // 0　BackCheck
        // 1　BuzzerPushed_fromBack
        // 2　isSitsunaitou_fromBack
        // 3  1e側の隣車へBackを渡せる
        // 4  2e側の隣車へBackを渡せる

        protected int[] transport_int_from1e = new int[2];
        protected float[] transport_float_from1e = new float[3];
        protected bool[] transport_bool_from1e = new bool[8];
        protected bool[] transport_bool_fromFront_from1e = new bool[6];
        protected bool[] transport_bool_fromBack_from1e = new bool[6];
        protected int[] zengoSwSegment_from1e = new int[1];
        protected bool[] transport_bool_Doors_from1e = new bool[8];

        protected int[] transport_int_from2e = new int[2];
        protected float[] transport_float_from2e = new float[3];
        protected bool[] transport_bool_from2e = new bool[8];
        protected bool[] transport_bool_fromFront_from2e = new bool[6];
        protected bool[] transport_bool_fromBack_from2e = new bool[6];
        protected int[] zengoSwSegment_from2e = new int[1];
        protected bool[] transport_bool_Doors_from2e = new bool[8];
        
        protected bool[] isConnectedOtherCar = new bool[2]; //通信接続がされているかのフラグ 0:1エンド側 1:2エンド側
        protected bool[] isConnectedTo2eCoupler = new bool[2]; //接続した相手車両の接続カプラが2エンド側か　Falseで1エンド側 0:自車1エンド側 1:自車2エンド側 isConnectedOtherCarと組み合わせること

        [Header("デバッグ表示")]
        [SerializeField] protected int notchSegmentLocal; //ノッチハンドルから読み取り
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

        //ドア開閉状態キャッシュ
        protected bool isOpenLeftDoor;
        protected bool prevIsOpenLeftDoor;
        protected bool isOpenRightDoor;
        protected bool prevIsOpenRightDoor;
        //ドア鍵状態キャッシュ
        protected bool isEnableKey1eL = true;
        protected bool isEnableKey2eL = true;
        protected bool isEnableKey1eR = true;
        protected bool isEnableKey2eR = true;
        
        protected bool prevIsEnableKey1eL = true;
        protected bool prevIsEnableKey2eL = true;
        protected bool prevIsEnableKey1eR = true;
        protected bool prevIsEnableKey2eR = true;
        protected bool isEnabledKeyOtherL = true;
        protected bool isEnabledKeyOtherR = true;
        //アニメーションハッシュ
        protected int isOpenLeftDoorParameterID;//isOpenLeftDoor
        protected int isOpenRightDoorParameterID;//isOpenRightDoor
        protected int isRoomLightParameterID;//isRoomLight

        //開発用仮実装
        [Header("起動Sw　パンタグラフ等")]
        [SerializeField] protected syncSW_Base powerEnableSW;
        protected bool[] isPowerEnableSw = new bool[1];

        [SerializeField] protected bool debug_flg;
        [SerializeField] protected frou01.RigidBodyTrain.MortorAndWheel _MotorAndWheel;
        protected float[] outputMortorForce = new float[1];
        protected float[] outputBrakeForce = new float[1];

        protected virtual void Start()
        {
            has2Handle = Utilities.IsValid(brakeLever1e) && Utilities.IsValid(brakeLever2e);
            if(has2Handle)
            {
                brakeSegment1e = brakeLever1e.currentSegment_Exposed;
                brakePosition1e = brakeLever1e.controllerPosition_Exposed;
                brakeNormPosition1e = brakeLever1e.currentNormalizePosition_Exposed;
                brakeLeverColider1e = brakeLever1e.GetComponent<Collider>();
                brakeSegment2e = brakeLever2e.currentSegment_Exposed;
                brakePosition2e = brakeLever2e.controllerPosition_Exposed;
                brakeNormPosition2e = brakeLever2e.currentNormalizePosition_Exposed;
                brakeLeverColider2e = brakeLever2e.GetComponent<Collider>();
            }

            isOwnerState = Networking.IsOwner(this.gameObject);
            notchSegment1e = notchLever1e.currentSegment_Exposed;
            reverserSegment1e = reverser1e.currentSegment_Exposed;
            zengoSwSegment1e = zengoSW1e.currentSegment_Exposed;
            zengoSWColider1e = zengoSW1e.GetComponent<Collider>();
            UseEnd1 = EndChangeSW1e.udonSyncedBool;
            EndChangeSWColider1e = EndChangeSW1e.GetComponent<Collider>();
            notchLeverColider1e = notchLever1e.GetComponent<Collider>();
            reverserColider1e = reverser1e.GetComponent<Collider>();

            notchSegment2e = notchLever2e.currentSegment_Exposed;
            reverserSegment2e = reverser2e.currentSegment_Exposed;
            zengoSwSegment2e = zengoSW2e.currentSegment_Exposed;
            zengoSWColider2e = zengoSW2e.GetComponent<Collider>();
            UseEnd2 = EndChangeSW2e.udonSyncedBool;
            EndChangeSWColider2e = EndChangeSW2e.GetComponent<Collider>();
            notchLeverColider2e = notchLever2e.GetComponent<Collider>();
            reverserColider2e = reverser2e.GetComponent<Collider>();

            Transform tmpTransform = brakeLever1e.transform.parent.Find("BrakeHandleMesh");
            if(tmpTransform != null) brakeHandleMesh1e = tmpTransform.gameObject;
            tmpTransform = brakeLever2e.transform.parent.Find("BrakeHandleMesh");
            if(tmpTransform != null) brakeHandleMesh2e = tmpTransform.gameObject;

            doorSwLeft1e = _doorSwLeft1e.udonSyncedBool;
            doorSwLeft2e = _doorSwLeft2e.udonSyncedBool;
            doorSwRight1e = _doorSwRight1e.udonSyncedBool;
            doorSwRight2e = _doorSwRight2e.udonSyncedBool;

            keySw1eL = _keySw1eL.udonSyncedBool;
            keySw2eL = _keySw2eL.udonSyncedBool;
            keySw1eR = _keySw1eR.udonSyncedBool;
            keySw2eR = _keySw2eR.udonSyncedBool;
            
            _doorKeySwCol1eL = _keySw1eL.GetComponent<Collider>();
            _doorKeySwCol2eL = _keySw2eL.GetComponent<Collider>();
            _doorKeySwCol1eR = _keySw1eR.GetComponent<Collider>();
            _doorKeySwCol2eR = _keySw2eR.GetComponent<Collider>();

            isOpenLeftDoorParameterID = Animator.StringToHash("isOpenLeftDoor");
            isOpenRightDoorParameterID = Animator.StringToHash("isOpenRightDoor");
            isRoomLightParameterID = Animator.StringToHash("isRoomLight");

            isBuzzerSw = buzzerSW.udonSyncedBool;
            isRoomLightSw = roomLightSW.udonSyncedBool;

            isInit = true;
            //開発用仮実装
            isPowerEnableSw = powerEnableSW.udonSyncedBool;
            outputMortorForce = _MotorAndWheel.MortorForce;
            outputBrakeForce = _MotorAndWheel.BrakeForce;
        }
        
        protected virtual void Update()
        {
            if(!isInit) return;
            updateDeltaTime = Time.deltaTime;
            isBuzzerSwPushedAnyCar = isBuzzerSw[0];
            isRoomLightSwPushedAnyCar = isRoomLightSw[0];
            SwReadProcess();

            //ハンドル位置読み取り、フロント・バックチェック
            switch(dataDirectionMode)
            {
                case 0://[前][後]
                    ControllProcess1e();
                    break;
                case 1://[前][中]
                    if(transport_bool_fromFront[0] && transport_bool_fromBack[0])
                    {
                        transport_bool_fromFront[1] = isBuzzerSwPushedAnyCar;
                        transport_bool_fromFront[2] = isRoomLightSwPushedAnyCar;
                        isBuzzerSwPushedAnyCar = (isBuzzerSwPushedAnyCar || transport_bool_fromBack_from2e[1]);
                        isRoomLightSwPushedAnyCar = (isRoomLightSwPushedAnyCar || transport_bool_fromBack_from2e[2]);
                    }
                    else
                    {
                        isBuzzerSwPushedAnyCar = false;
                        transport_bool_fromBack[1] = false;
                        isRoomLightSwPushedAnyCar = false;
                        transport_bool_fromBack[2] = false;
                        EnablePermission = false;
                    }
                    ControllProcess1e();
                    break;
                case 2://[後][前]
                    ControllProcess2e();
                    break;
                case 3://[中][前]
                    if(transport_bool_fromFront[0] && transport_bool_fromBack[0])
                    {
                        transport_bool_fromFront[1] = isBuzzerSwPushedAnyCar;
                        transport_bool_fromFront[2] = isRoomLightSwPushedAnyCar;
                        isBuzzerSwPushedAnyCar = (isBuzzerSwPushedAnyCar || transport_bool_fromBack_from1e[1]);
                        isRoomLightSwPushedAnyCar = (isRoomLightSwPushedAnyCar || transport_bool_fromBack_from1e[2]);
                    }
                    else
                    {
                        isBuzzerSwPushedAnyCar = false;
                        transport_bool_fromFront[1] = false;
                        isRoomLightSwPushedAnyCar = false;
                        transport_bool_fromFront[2] = false;
                        EnablePermission = false;
                    }
                    ControllProcess2e();
                    break;
                case 4://[中][後]
                    if(transport_bool_fromFront[0] && transport_bool_fromBack[0])
                    {
                        transport_bool_fromBack[1] = isBuzzerSwPushedAnyCar;
                        transport_bool_fromBack[2] = isRoomLightSwPushedAnyCar;
                        isBuzzerSwPushedAnyCar = (isBuzzerSwPushedAnyCar || transport_bool_fromFront_from1e[1]);
                        isRoomLightSwPushedAnyCar = (isRoomLightSwPushedAnyCar || transport_bool_fromFront_from1e[2]);

                        EnablePermission = transport_bool_from1e[2];
                        Receive_transport_bool_Others(false);
                    }
                    else
                    {
                        isBuzzerSwPushedAnyCar = false;
                        transport_bool_fromFront[1] = false;
                        isRoomLightSwPushedAnyCar = false;
                        transport_bool_fromFront[2] = false;
                        EnablePermission = false;
                        Reset_transport_bool_Others();
                    }
                    ReadControllerParametersFrom1e();
                    break;
                case 7://[中][中]かつ送信方向が1e->2eで決定済
                    if(transport_bool_fromFront[0] && transport_bool_fromBack[0])
                    {
                        transport_bool_fromFront[1] = isBuzzerSwPushedAnyCar || transport_bool_fromFront_from1e[1];
                        transport_bool_fromBack[1] = isBuzzerSwPushedAnyCar || transport_bool_fromBack_from2e[1];
                        isBuzzerSwPushedAnyCar = transport_bool_fromFront[1] || transport_bool_fromBack[1];

                        transport_bool_fromFront[2] = isRoomLightSwPushedAnyCar || transport_bool_fromFront_from1e[2];
                        transport_bool_fromBack[2] = isRoomLightSwPushedAnyCar || transport_bool_fromBack_from2e[2];
                        isRoomLightSwPushedAnyCar = transport_bool_fromFront[2] || transport_bool_fromBack[2];
                        
                        EnablePermission = transport_bool_from1e[2];
                        Receive_transport_bool_Others(false);
                    }
                    else
                    {
                        isBuzzerSwPushedAnyCar = false;
                        transport_bool_fromFront[1] = false;
                        transport_bool_fromBack[1] = false;
                        isRoomLightSwPushedAnyCar = false;
                        transport_bool_fromFront[2] = false;
                        transport_bool_fromBack[2] = false;
                        EnablePermission = false;
                        Reset_transport_bool_Others();
                    }
                    ReadControllerParametersFrom1e();
                    break;
                case 5://[後][中]
                    if(transport_bool_fromFront[0] && transport_bool_fromBack[0])
                    {
                        transport_bool_fromBack[1] = isBuzzerSwPushedAnyCar;
                        transport_bool_fromBack[2] = isRoomLightSwPushedAnyCar;
                        isBuzzerSwPushedAnyCar = (isBuzzerSwPushedAnyCar || transport_bool_fromFront_from2e[1]);
                        isRoomLightSwPushedAnyCar = (isRoomLightSwPushedAnyCar || transport_bool_fromFront_from2e[2]);
                        EnablePermission = transport_bool_from2e[2];
                        Receive_transport_bool_Others(true);
                    }
                    else
                    {
                        isBuzzerSwPushedAnyCar = false;
                        transport_bool_fromBack[1] = false;
                        isRoomLightSwPushedAnyCar = false;
                        transport_bool_fromBack[2] = false;
                        EnablePermission = false;
                        Reset_transport_bool_Others();
                    }
                    ReadControllerParametersFrom2e();
                    break;
                case 8://[中][中]かつ送信方向が2e->1eで決定済
                    if(transport_bool_fromFront[0] && transport_bool_fromBack[0])
                    {
                        transport_bool_fromFront[1] = isBuzzerSwPushedAnyCar || transport_bool_fromFront_from2e[1];
                        transport_bool_fromBack[1] = isBuzzerSwPushedAnyCar || transport_bool_fromBack_from1e[1];
                        isBuzzerSwPushedAnyCar = transport_bool_fromFront[1] || transport_bool_fromBack[1];

                        transport_bool_fromFront[2] = isRoomLightSwPushedAnyCar || transport_bool_fromFront_from2e[2];
                        transport_bool_fromBack[2] = isRoomLightSwPushedAnyCar || transport_bool_fromBack_from1e[2];
                        isRoomLightSwPushedAnyCar = transport_bool_fromFront[2] || transport_bool_fromBack[2];

                        EnablePermission = transport_bool_from2e[2];
                        Receive_transport_bool_Others(true);
                    }
                    else
                    {
                        isBuzzerSwPushedAnyCar = false;
                        transport_bool_fromFront[1] = false;
                        transport_bool_fromBack[1] = false;
                        isRoomLightSwPushedAnyCar = false;
                        transport_bool_fromFront[2] = false;
                        transport_bool_fromBack[2] = false;
                        EnablePermission = false;
                        Reset_transport_bool_Others();
                    }
                    ReadControllerParametersFrom2e();
                    break;
                default:
                    if(isBuzzerSwPushedAnyCar) isBuzzerSwPushedAnyCar = false;
                    if(transport_bool_fromFront[1]) transport_bool_fromFront[1] = false;
                    if(transport_bool_fromBack[1]) transport_bool_fromBack[1] = false;
                    if(EnablePermission || transport_bool[2]) EnablePermission = transport_bool[2] = false;
                    break;
            }
            //ブザー音
            if(isBuzzerSwPushedAnyCar != prevIsBuzzerSwPushed)
            {
                if(isBuzzerSwPushedAnyCar) foreach(AudioSource _buzzerSnd in buzzerSnd) _buzzerSnd.Play();
                else foreach(AudioSource _buzzerSnd in buzzerSnd) _buzzerSnd.Stop();
            }
            prevIsBuzzerSwPushed = isBuzzerSwPushedAnyCar;
            //室内灯
            if(isRoomLightSwPushedAnyCar != prevIsRoomLightSwPushed)
            {
                foreach(Animator _meshAnimator in trainMeshAnimators) _meshAnimator.SetBool(isRoomLightParameterID, isRoomLightSwPushedAnyCar);
            }
            prevIsRoomLightSwPushed = isRoomLightSwPushedAnyCar;

            //力行・制動処理
            if(transport_bool_fromFront[0] && transport_bool_fromBack[0]) PowerAndBrakeProcess();

            //前->後 送信処理
            if(dataDirectionMode == 1 || dataDirectionMode == 3 || dataDirectionMode == 7 || dataDirectionMode == 8)
            {
                transport_int[0] = notchPos;
                transport_int[1] = brakeSeg;
                transport_float[1] = brakePos;
                transport_float[2] = brakeNormPos;
                //方向性信号
                if(!transport_bool[0]) //2e側へ送信 反転
                {
                    transport_float[0] = -1f * powerDirection;
                }
                else //1e側へ送信　順方向
                {
                    transport_float[0] = powerDirection;
                }
                transport_bool[2] = EnablePermission;
                Send_transport_bool_Others();
            }

            //Debug表示

            if(Utilities.IsValid(debugText))
            {
                string dis_text = "";
                dis_text += "DateDirectionMode: " + dataDirectionMode + "\n";
                dis_text += "DateDirection: " + (transport_bool[1] ? (!transport_bool[0] ? "1e -> 2e" : "2e -> 1e") : "None") + "\n";
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
                dis_text += "EnablePermission: " + EnablePermission + "\n";
                debugText.text = dis_text;
            }
        }
        protected virtual void SwReadProcess() //スイッチ状態読み取り
        {
            EnablePermission = isPowerEnableSw[0];
        }

        protected virtual void DecideNotchAndBrakePos() //速度制限やATS等の非常制動など　base.DecideNotchAndBrakePos()
        {
            notchPos = notchSegmentLocal;
            brakeSeg = brakeSegmentLocal;
            brakePos = brakePositionLocal;
            brakeNormPos = brakeNormPosLocal;
        }
        protected virtual void PowerAndBrakeProcess() //これをoverrideして各車種の開発
        {
            //Update内で実行
            //Time.deltaTimeの値はupdateDeltaTimeに格納済
            //開発用仮実装
            outputMortorForce[0] = powerDirection * 10000f * notchPos;
            outputBrakeForce[0] = (brakeSeg == 0 ? 0f : 70000f * brakeNormPos);
        }

        //制御車　コントローラー読み取り処理
        protected void ControllProcess1e()
        {
            notchSegmentLocal = notchSegment1e[0];
            brakeSegmentLocal = brakeSegment1e[0];
            brakePositionLocal = brakePosition1e[0];
            brakeNormPosLocal = brakeNormPosition1e[0];
            powerDirection = reverserSegment1e[0] - 1f;
            DecideNotchAndBrakePos();
        }
        protected void ControllProcess2e()
        {
            notchSegmentLocal = notchSegment2e[0];
            brakeSegmentLocal = brakeSegment2e[0];
            brakePositionLocal = brakePosition2e[0];
            brakeNormPosLocal = brakeNormPosition2e[0];
            powerDirection = -1f * (reverserSegment2e[0] - 1f);
            DecideNotchAndBrakePos();
        }

        //被制御車　操作系変数読み取り処理
        protected void ReadControllerParametersFrom1e()
        {
            if(canReadFrom1e && transport_bool_fromFront_from1e[0])
            {
                transport_bool_fromFront[0] = true;
                notchPos = transport_int_from1e[0];
                brakeSeg = transport_int_from1e[1];
                brakePos = transport_float_from1e[1];
                brakeNormPos = transport_float_from1e[2];
                powerDirection = -1f * transport_float_from1e[0];
            }
            else
            {
                transport_bool_fromFront[0] = false;
            }
        }
        protected void ReadControllerParametersFrom2e()
        {
            if(canReadFrom2e && transport_bool_fromFront_from2e[0])
            {
                transport_bool_fromFront[0] = true;
                notchPos = transport_int_from2e[0];
                brakeSeg = transport_int_from2e[1];
                brakePos = transport_float_from2e[1];
                brakeNormPos = transport_float_from2e[2];
                powerDirection = transport_float_from2e[0];
            }
            else
            {
                transport_bool_fromFront[0] = false;
            }
        }
        //MARK:他transport_bool処理
        //他transport_bool送信 運転台->後端方向のみ
        protected virtual void Send_transport_bool_Others()
        {
            // transport_bool[3] = OtherBoolParameter1;
            // transport_bool[4] = OtherBoolParameter2;
            // transport_bool[5] = OtherBoolParameter3;
            // transport_bool[6] = OtherBoolParameter4;
            // transport_bool[7] = OtherBoolParameter5;
        }
        //他transport_bool受信 運転台->後端方向のみ
        protected virtual void Receive_transport_bool_Others(bool readFrom2e)
        {
            // if(!readFrom2e)//1eから読み込み
            // {
            //     // OtherBoolParameter1 = transport_bool_from1e[3];
            //     // OtherBoolParameter2 = transport_bool_from1e[4];
            //     // OtherBoolParameter3 = transport_bool_from1e[5];
            //     // OtherBoolParameter4 = transport_bool_from1e[6];
            //     // OtherBoolParameter5 = transport_bool_from1e[7];
            // }
            // else
            // {
            //     // OtherBoolParameter1 = transport_bool_from2e[3];
            //     // OtherBoolParameter2 = transport_bool_from2e[4];
            //     // OtherBoolParameter3 = transport_bool_from2e[5];
            //     // OtherBoolParameter4 = transport_bool_from2e[6];
            //     // OtherBoolParameter5 = transport_bool_from2e[7];
            // }
        }
        //他transport_bool変数リセット
        protected virtual void Reset_transport_bool_Others()
        {
            // OtherBoolParameter1 = false;
            // OtherBoolParameter2 = false;
            // OtherBoolParameter3 = false;
            // OtherBoolParameter4 = false;
            // OtherBoolParameter5 = false;
        }

        //ドア状態更新
        protected bool doorStateUpdateQueued;
        [NetworkCallable]
        public void DoorStateUpdate()
        {
            if(doorStateUpdateQueued || !isInit) return;
            doorStateUpdateQueued = true;
            switch(dataDirectionMode)
            {
                case 0://[前][後]
                    DoorStateUpdate_OnlyMyCar();
                    break;
                case 1://[前][中]
                    if(canReadFrom2e && transport_bool_fromBack[0])
                    {
                        //2エンドからの、後->前のみ読み取り
                        isOpenLeftDoor = doorSwLeft1e[0] || doorSwLeft2e[0] || transport_bool_Doors_from2e[4];
                        isOpenRightDoor = doorSwRight1e[0] || doorSwRight2e[0] || transport_bool_Doors_from2e[5];
                        
                        isEnableKey1eL = keySw1eL[0] || (!keySw2eL[0] && !transport_bool_Doors_from2e[6]);
                        isEnableKey2eL = keySw2eL[0] || (!keySw1eL[0] && !transport_bool_Doors_from2e[6]);
                        isEnableKey1eR = keySw1eR[0] || (!keySw2eR[0] && !transport_bool_Doors_from2e[7]);
                        isEnableKey2eR = keySw2eR[0] || (!keySw1eR[0] && !transport_bool_Doors_from2e[7]);
                        //2エンドへ前->後送信(反転)
                        transport_bool_Doors[0] = doorSwRight1e[0] || doorSwRight2e[0];
                        transport_bool_Doors[1] = doorSwLeft1e[0] || doorSwLeft2e[0];
                        transport_bool_Doors[2] = keySw1eR[0] || keySw2eR[0];
                        transport_bool_Doors[3] = keySw1eL[0] || keySw2eL[0];
                        //後->前は送信しない
                        transport_bool_Doors[4] = false;
                        transport_bool_Doors[5] = false;
                        transport_bool_Doors[6] = false;
                        transport_bool_Doors[7] = false;
                    }
                    else DoorStateUpdate_OnlyMyCar();
                    break;
                case 2://[後][前]
                    DoorStateUpdate_OnlyMyCar();
                    break;
                case 3://[中][前]
                    if(canReadFrom1e && transport_bool_fromBack[0])
                    {
                        //1エンドからの、後->前のみ読み取り
                        isOpenLeftDoor = doorSwLeft1e[0] || doorSwLeft2e[0] || transport_bool_Doors_from1e[5];
                        isOpenRightDoor = doorSwRight1e[0] || doorSwRight2e[0] || transport_bool_Doors_from1e[4];
                        
                        isEnableKey1eL = keySw1eL[0] || (!keySw2eL[0] && !transport_bool_Doors_from1e[7]);
                        isEnableKey2eL = keySw2eL[0] || (!keySw1eL[0] && !transport_bool_Doors_from1e[7]);
                        isEnableKey1eR = keySw1eR[0] || (!keySw2eR[0] && !transport_bool_Doors_from1e[6]);
                        isEnableKey2eR = keySw2eR[0] || (!keySw1eR[0] && !transport_bool_Doors_from1e[6]);
                        //1エンドへ前->後送信
                        transport_bool_Doors[0] = doorSwLeft1e[0] || doorSwLeft2e[0];
                        transport_bool_Doors[1] = doorSwRight1e[0] || doorSwRight2e[0];
                        transport_bool_Doors[2] = keySw1eL[0] || keySw2eL[0];
                        transport_bool_Doors[3] = keySw1eR[0] || keySw2eR[0];
                        //後->前は送信しない
                        transport_bool_Doors[4] = false;
                        transport_bool_Doors[5] = false;
                        transport_bool_Doors[6] = false;
                        transport_bool_Doors[7] = false;
                    }
                    else DoorStateUpdate_OnlyMyCar();
                    break;
                case 4://[中][後]
                    if(canReadFrom1e && transport_bool_fromFront[0])
                    {
                        //1エンドからの、前->後ろのみ読み取り
                        isOpenLeftDoor = doorSwLeft1e[0] || doorSwLeft2e[0] || transport_bool_Doors_from1e[1];
                        isOpenRightDoor = doorSwRight1e[0] || doorSwRight2e[0] || transport_bool_Doors_from1e[0];
                        
                        isEnableKey1eL = keySw1eL[0] || (!keySw2eL[0] && !transport_bool_Doors_from1e[3]);
                        isEnableKey2eL = keySw2eL[0] || (!keySw1eL[0] && !transport_bool_Doors_from1e[3]);
                        isEnableKey1eR = keySw1eR[0] || (!keySw2eR[0] && !transport_bool_Doors_from1e[2]);
                        isEnableKey2eR = keySw2eR[0] || (!keySw1eR[0] && !transport_bool_Doors_from1e[2]);
                        //1エンドへ前->後送信（後端なので無し)
                        transport_bool_Doors[0] = false;
                        transport_bool_Doors[1] = false;
                        transport_bool_Doors[2] = false;
                        transport_bool_Doors[3] = false;
                        //1エンドへ後->前送信
                        transport_bool_Doors[4] = doorSwLeft1e[0] || doorSwLeft2e[0];
                        transport_bool_Doors[5] = doorSwRight1e[0] || doorSwRight2e[0];
                        transport_bool_Doors[6] = keySw1eL[0] || keySw2eL[0];
                        transport_bool_Doors[7] = keySw1eR[0] || keySw2eR[0];
                    }
                    else DoorStateUpdate_OnlyMyCar();
                    break;
                case 5://[後][中]
                    if(canReadFrom2e && transport_bool_fromFront[0])
                    {
                        //2エンドからの、前->後ろのみ読み取り
                        isOpenLeftDoor = doorSwLeft1e[0] || doorSwLeft2e[0] || transport_bool_Doors_from2e[0];
                        isOpenRightDoor = doorSwRight1e[0] || doorSwRight2e[0] || transport_bool_Doors_from2e[1];
                        
                        isEnableKey1eL = keySw1eL[0] || (!keySw2eL[0] && !transport_bool_Doors_from2e[2]);
                        isEnableKey2eL = keySw2eL[0] || (!keySw1eL[0] && !transport_bool_Doors_from2e[2]);
                        isEnableKey1eR = keySw1eR[0] || (!keySw2eR[0] && !transport_bool_Doors_from2e[3]);
                        isEnableKey2eR = keySw2eR[0] || (!keySw1eR[0] && !transport_bool_Doors_from2e[3]);
                        //2エンドへ前->後送信（後端なので無し)
                        transport_bool_Doors[0] = false;
                        transport_bool_Doors[1] = false;
                        transport_bool_Doors[2] = false;
                        transport_bool_Doors[3] = false;
                        //2エンドへ後->前送信
                        transport_bool_Doors[4] = doorSwRight1e[0] || doorSwRight2e[0];
                        transport_bool_Doors[5] = doorSwLeft1e[0] || doorSwLeft2e[0];
                        transport_bool_Doors[6] = keySw1eR[0] || keySw2eR[0];
                        transport_bool_Doors[7] = keySw1eL[0] || keySw2eL[0];
                    }
                    else DoorStateUpdate_OnlyMyCar();
                    break;
                case 6://[中][中]
                    DoorStateUpdate_OnlyMyCar();
                    break;
                case 7://[中][中]かつ送信方向が1e->2eで決定済
                    if(canReadFrom1e && canReadFrom2e && transport_bool_fromFront[0] && transport_bool_fromBack[0])
                    {
                        //1エンドから前->後、2エンドから後->前を読み取り
                        isOpenLeftDoor = doorSwLeft1e[0] || doorSwLeft2e[0] || transport_bool_Doors_from1e[1] || transport_bool_Doors_from2e[4];
                        isOpenRightDoor = doorSwRight1e[0] || doorSwRight2e[0] || transport_bool_Doors_from1e[0] || transport_bool_Doors_from2e[5];
                        
                        isEnableKey1eL = keySw1eL[0] || (!keySw2eL[0] && !transport_bool_Doors_from1e[3] && !transport_bool_Doors_from2e[6]);
                        isEnableKey2eL = keySw2eL[0] || (!keySw1eL[0] && !transport_bool_Doors_from1e[3] && !transport_bool_Doors_from2e[6]);
                        isEnableKey1eR = keySw1eR[0] || (!keySw2eR[0] && !transport_bool_Doors_from1e[2] && !transport_bool_Doors_from2e[7]);
                        isEnableKey2eR = keySw2eR[0] || (!keySw1eR[0] && !transport_bool_Doors_from1e[2] && !transport_bool_Doors_from2e[7]);
                        //2エンドへ前->後送信(反転)
                        transport_bool_Doors[0] = doorSwRight1e[0] || doorSwRight2e[0] || transport_bool_Doors_from1e[0];
                        transport_bool_Doors[1] = doorSwLeft1e[0] || doorSwLeft2e[0] || transport_bool_Doors_from1e[1];
                        transport_bool_Doors[2] = keySw1eR[0] || keySw2eR[0] || transport_bool_Doors_from1e[2];
                        transport_bool_Doors[3] = keySw1eL[0] || keySw2eL[0] || transport_bool_Doors_from1e[3];
                        //1エンドへ後->前
                        transport_bool_Doors[4] = doorSwLeft1e[0] || doorSwLeft2e[0] || transport_bool_Doors_from2e[4];
                        transport_bool_Doors[5] = doorSwRight1e[0] || doorSwRight2e[0] || transport_bool_Doors_from2e[5];
                        transport_bool_Doors[6] = keySw1eL[0] || keySw2eL[0] || transport_bool_Doors_from2e[6];
                        transport_bool_Doors[7] = keySw1eR[0] || keySw2eR[0] || transport_bool_Doors_from2e[7];
                    }
                    break;
                case 8://[中][中]かつ送信方向が2e->1eで決定済
                    if(canReadFrom1e && canReadFrom2e && transport_bool_fromFront[0] && transport_bool_fromBack[0])
                    {
                        //2エンドから前->後、1エンドから後->前を読み取り
                        isOpenLeftDoor = doorSwLeft1e[0] || doorSwLeft2e[0] || transport_bool_Doors_from2e[0] || transport_bool_Doors_from1e[5];
                        isOpenRightDoor = doorSwRight1e[0] || doorSwRight2e[0] || transport_bool_Doors_from2e[1] || transport_bool_Doors_from1e[4];

                        isEnableKey1eL = keySw1eL[0] || (!keySw2eL[0] && !transport_bool_Doors_from1e[7] && !transport_bool_Doors_from2e[2]);
                        isEnableKey2eL = keySw2eL[0] || (!keySw1eL[0] && !transport_bool_Doors_from1e[7] && !transport_bool_Doors_from2e[2]);
                        isEnableKey1eR = keySw1eR[0] || (!keySw2eR[0] && !transport_bool_Doors_from1e[6] && !transport_bool_Doors_from2e[3]);
                        isEnableKey2eR = keySw2eR[0] || (!keySw1eR[0] && !transport_bool_Doors_from1e[6] && !transport_bool_Doors_from2e[3]);
                        //1エンドへ前->後送信
                        transport_bool_Doors[0] = doorSwLeft1e[0] || doorSwLeft2e[0] || transport_bool_Doors_from2e[0];
                        transport_bool_Doors[1] = doorSwRight1e[0] || doorSwRight2e[0] || transport_bool_Doors_from2e[1];
                        transport_bool_Doors[2] = keySw1eL[0] || keySw2eL[0] || transport_bool_Doors_from2e[2];
                        transport_bool_Doors[3] = keySw1eR[0] || keySw2eR[0] || transport_bool_Doors_from2e[3];
                        //2エンドへ後->前送信(反転)
                        transport_bool_Doors[4] = doorSwRight1e[0] || doorSwRight2e[0] || transport_bool_Doors_from1e[4];
                        transport_bool_Doors[5] = doorSwLeft1e[0] || doorSwLeft2e[0] || transport_bool_Doors_from1e[5];
                        transport_bool_Doors[6] = keySw1eR[0] || keySw2eR[0] || transport_bool_Doors_from1e[6];
                        transport_bool_Doors[7] = keySw1eL[0] || keySw2eL[0] || transport_bool_Doors_from1e[7];
                    }
                    break;
                default:
                    DoorStateUpdate_OnlyMyCar();
                    break;

            }
            //左扉
            if(isOpenLeftDoor != prevIsOpenLeftDoor)
            {
                foreach(Animator _meshAnimator in trainMeshAnimators)
                {
                    _meshAnimator.SetBool(isOpenLeftDoorParameterID, isOpenLeftDoor);
                }
            }
            prevIsOpenLeftDoor = isOpenLeftDoor;
            _doorKeySwCol1eL.enabled = isEnableKey1eL;
            _doorKeySwCol2eL.enabled = isEnableKey2eL;
            //右扉
            if(isOpenRightDoor != prevIsOpenRightDoor)
            {
                foreach(Animator _meshAnimator in trainMeshAnimators) _meshAnimator.SetBool(isOpenRightDoorParameterID, isOpenRightDoor);
            }
            prevIsOpenRightDoor = isOpenRightDoor;
            _doorKeySwCol1eR.enabled = isEnableKey1eR;
            _doorKeySwCol2eR.enabled = isEnableKey2eR;

            //呼び出し
            if(transport_bool_fromFront[0] && transport_bool_fromBack[0])
            {
                if(canReadFrom1e) connectedModule_1e.DoorStateUpdate();
                if(canReadFrom2e) connectedModule_2e.DoorStateUpdate();
            }
            doorStateUpdateQueued = false;
        }
        protected void DoorStateUpdate_OnlyMyCar()//
        {
            isOpenLeftDoor = doorSwLeft1e[0] || doorSwLeft2e[0];
            isOpenRightDoor = doorSwRight1e[0] || doorSwRight2e[0];
            isEnableKey1eL = keySw1eL[0] || !keySw2eL[0];
            isEnableKey2eL = keySw2eL[0] || !keySw1eL[0];
            isEnableKey1eR = keySw1eR[0] || !keySw2eR[0];
            isEnableKey2eR = keySw2eR[0] || !keySw1eR[0];
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
                        transport_bool_Doors_from1e = connectedModule_1e.transport_bool_Doors;
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
                        transport_bool_Doors_from2e = connectedModule_2e.transport_bool_Doors;
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
                    transport_bool_Doors_from1e = null;
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
                    transport_bool_Doors_from2e = null;
                }
            }
            ChangeZengoSwEvent();
        }
        
        protected bool directionRecalcQueued;
        [NetworkCallable]
        public void ChangeZengoSwEvent()
        {
            RequestDirectionRecalc();
            DoorStateUpdate();
        }
        [NetworkCallable]
        public void RequestDirectionRecalc()
        {
            if(directionRecalcQueued) return;

            directionRecalcQueued = true;
            SendCustomEventDelayedFrames(nameof(DirectionRecalcTick), 1);
        }
        [NetworkCallable]
        public void DirectionRecalcTick()
        {
            UpdateControllerEnableProcess();
            directionRecalcQueued = false;

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
            bool changed = EvaluateDirectionState();

            if(changed)
            {
                if(isConnectedOtherCar[0])
                {
                    connectedModule_1e.RequestDirectionRecalc();
                }
                if(isConnectedOtherCar[1])
                {
                    connectedModule_2e.RequestDirectionRecalc();
                }
            }
        }
        protected bool EvaluateDirectionState()
        {
            byte oldMode = dataDirectionMode;
            bool oldDirection = transport_bool[0];
            bool oldDecided = transport_bool[1];

            bool oldFront = transport_bool_fromFront[0];
            bool oldBack = transport_bool_fromBack[0];

            bool oldFrontExport1e = transport_bool_fromFront[3];
            bool oldFrontExport2e = transport_bool_fromFront[4];
            bool oldBackExport1e = transport_bool_fromBack[3];
            bool oldBackExport2e = transport_bool_fromBack[4];

            int sw1 = zengoSwSegment1e[0];
            int sw2 = zengoSwSegment2e[0];

            //読み出し可能フラグ更新 canReadFrom1e canReadFrom2e
            if(isConnectedOtherCar[0]) canReadFrom1e = (zengoSwSegment1e[0] == 1) && (zengoSwSegment_from1e[0] == 1);
            else canReadFrom1e = false;
            if(isConnectedOtherCar[1]) canReadFrom2e = (zengoSwSegment2e[0] == 1) && (zengoSwSegment_from2e[0] == 1);
            else canReadFrom2e = false;

            bool frontFrom1e = FrontComesFrom1eSide();
            bool frontFrom2e = FrontComesFrom2eSide();
            bool backFrom1e = BackComesFrom1eSide();
            bool backFrom2e = BackComesFrom2eSide();

            // いったん全クリア
            transport_bool[0] = false;
            transport_bool[1] = false;

            transport_bool_fromFront[0] = false;
            transport_bool_fromBack[0] = false;

            transport_bool_fromFront[3] = false;
            transport_bool_fromFront[4] = false;
            transport_bool_fromBack[3] = false;
            transport_bool_fromBack[4] = false;

            dataDirectionMode = 9;

            // 矛盾系
            if((sw1 == 2 && sw2 == 2) || (sw1 == 0 && sw2 == 0))
            {
                dataDirectionMode = 9;

                atsReceiver1e.enabled = false;
                atsReceiver2e.enabled = false;
            }
            // [前][後]
            else if(sw1 == 2 && sw2 == 0)
            {
                dataDirectionMode = 0;
                transport_bool[0] = false; // 1e -> 2e
                transport_bool[1] = true;
                transport_bool_fromFront[0] = true;
                transport_bool_fromBack[0] = true;

                atsReceiver1e.enabled = true;
                atsReceiver2e.enabled = false;
            }
            // [後][前]
            else if(sw1 == 0 && sw2 == 2)
            {
                dataDirectionMode = 2;
                transport_bool[0] = true; // 2e -> 1e
                transport_bool[1] = true;
                transport_bool_fromFront[0] = true;
                transport_bool_fromBack[0] = true;

                atsReceiver1e.enabled = false;
                atsReceiver2e.enabled = true;
            }
            // [前][中]
            else if(sw1 == 2 && sw2 == 1)
            {
                dataDirectionMode = 1;
                transport_bool[0] = false; // 1e -> 2e
                transport_bool[1] = true;

                transport_bool_fromFront[0] = true;
                transport_bool_fromBack[0] = backFrom2e;

                // 2e側の中間接続へFrontを渡せる
                transport_bool_fromFront[4] = true;

                // 2e側からBackが来ていれば、編成としてBack成立
                transport_bool_fromBack[0] = backFrom2e;

                atsReceiver1e.enabled = true;
                atsReceiver2e.enabled = false;
            }
            // [中][前]
            else if(sw1 == 1 && sw2 == 2)
            {
                dataDirectionMode = 3;
                transport_bool[0] = true; // 2e -> 1e
                transport_bool[1] = true;

                transport_bool_fromFront[0] = true;
                transport_bool_fromBack[0] = backFrom1e;

                // 1e側へFrontを渡せる
                transport_bool_fromFront[3] = true;

                transport_bool_fromBack[0] = backFrom1e;

                atsReceiver1e.enabled = false;
                atsReceiver2e.enabled = true;
            }
            // [中][後]
            else if(sw1 == 1 && sw2 == 0)
            {
                dataDirectionMode = 4;
                transport_bool[0] = false; // 1e -> 2e
                transport_bool[1] = frontFrom1e;

                transport_bool_fromFront[0] = frontFrom1e;
                transport_bool_fromBack[0] = true;

                // 1e側へBackを返せる
                transport_bool_fromBack[3] = true;

                atsReceiver1e.enabled = false;
                atsReceiver2e.enabled = false;
            }
            // [後][中]
            else if(sw1 == 0 && sw2 == 1)
            {
                dataDirectionMode = 5;
                transport_bool[0] = true; // 2e -> 1e
                transport_bool[1] = frontFrom2e;

                transport_bool_fromFront[0] = frontFrom2e;
                transport_bool_fromBack[0] = true;

                // 2e側へBackを返せる
                transport_bool_fromBack[4] = true;

                atsReceiver1e.enabled = false;
                atsReceiver2e.enabled = false;
            }
            // [中][中]
            else if(sw1 == 1 && sw2 == 1)
            {
                bool conflictFront = frontFrom1e && frontFrom2e;

                if(conflictFront)
                {
                    dataDirectionMode = 9;
                    transport_bool[1] = false;
                }
                else if(frontFrom1e)
                {
                    // 1e側にFrontがあるので 1e -> 2e
                    dataDirectionMode = 7;
                    transport_bool[0] = false;
                    transport_bool[1] = true;

                    transport_bool_fromFront[0] = true;
                    transport_bool_fromBack[0] = backFrom2e;

                    // 2e側へFrontを渡す
                    transport_bool_fromFront[4] = true;

                    // 2e側からBackが来ていれば1e側へ返せる
                    if(backFrom2e)
                    {
                        transport_bool_fromBack[3] = true;
                    }
                }
                else if(frontFrom2e)
                {
                    // 2e側にFrontがあるので 2e -> 1e
                    dataDirectionMode = 8;
                    transport_bool[0] = true;
                    transport_bool[1] = true;

                    transport_bool_fromFront[0] = true;
                    transport_bool_fromBack[0] = backFrom1e;

                    // 1e側へFrontを渡す
                    transport_bool_fromFront[3] = true;

                    // 1e側からBackが来ていれば2e側へ返せる
                    if(backFrom1e)
                    {
                        transport_bool_fromBack[4] = true;
                    }
                }
                else
                {
                    dataDirectionMode = 6;
                    transport_bool[1] = false;
                }
                atsReceiver1e.enabled = false;
                atsReceiver2e.enabled = false;
            }

            return oldMode != dataDirectionMode
                || oldDirection != transport_bool[0]
                || oldDecided != transport_bool[1]
                || oldFront != transport_bool_fromFront[0]
                || oldBack != transport_bool_fromBack[0]
                || oldFrontExport1e != transport_bool_fromFront[3]
                || oldFrontExport2e != transport_bool_fromFront[4]
                || oldBackExport1e != transport_bool_fromBack[3]
                || oldBackExport2e != transport_bool_fromBack[4];
        }

        protected bool FrontComesFrom1eSide()
        {
            if(!canReadFrom1e) return false;

            // 相手2eに接続しているなら、相手の2e側Front出力を見る
            if(isConnectedTo2eCoupler[0])
            {
                return connectedModule_1e.transport_bool_fromFront[4];
            }

            // 相手1eに接続しているなら、相手の1e側Front出力を見る
            return connectedModule_1e.transport_bool_fromFront[3];
        }

        protected bool FrontComesFrom2eSide()
        {
            if(!canReadFrom2e) return false;

            if(isConnectedTo2eCoupler[1])
            {
                return connectedModule_2e.transport_bool_fromFront[4];
            }

            return connectedModule_2e.transport_bool_fromFront[3];
        }

        protected bool BackComesFrom1eSide()
        {
            if(!canReadFrom1e) return false;

            if(isConnectedTo2eCoupler[0])
            {
                return connectedModule_1e.transport_bool_fromBack[4];
            }

            return connectedModule_1e.transport_bool_fromBack[3];
        }

        protected bool BackComesFrom2eSide()
        {
            if(!canReadFrom2e) return false;

            if(isConnectedTo2eCoupler[1])
            {
                return connectedModule_2e.transport_bool_fromBack[4];
            }

            return connectedModule_2e.transport_bool_fromBack[3];
        }

        public override void OnOwnershipTransferred(VRC.SDKBase.VRCPlayerApi player)
        {
            isOwnerState = player == Networking.LocalPlayer;
        }

        //MARK:エンド交換 運転台インターロック
        public void UpdateControllerEnable()
        {
            // Debug.Log("UpdateControllerEnable");
            SendCustomEventDelayedFrames(nameof(UpdateControllerEnableProcess), 1);
        }
        public void UpdateControllerEnableProcess()
        {
            notchLeverColider1e.enabled = UseEnd1[0];
            notchLeverColider2e.enabled = UseEnd2[0];
            zengoSWColider1e.enabled = !UseEnd1[0];
            zengoSWColider2e.enabled = !UseEnd2[0];
            if(has2Handle)
            {
                brakeLeverColider1e.enabled = UseEnd1[0];
                brakeLeverColider2e.enabled = UseEnd2[0];
                brakeHandleMesh1e.SetActive(UseEnd1[0]);
                brakeHandleMesh2e.SetActive(UseEnd2[0]);
                EndChangeSWColider1e.enabled = (!UseEnd1[0] && !UseEnd2[0] && (zengoSwSegment1e[0] == 2)) || (UseEnd1[0] && (notchSegment1e[0] == notchOffSegment) && (reverserSegment1e[0] == 1) && (brakeSegment1e[0] == brakeHandleNukitoriSegment));
                EndChangeSWColider2e.enabled = (!UseEnd1[0] && !UseEnd2[0] && (zengoSwSegment2e[0] == 2)) || (UseEnd2[0] && (notchSegment2e[0] == notchOffSegment) && (reverserSegment2e[0] == 1) && (brakeSegment2e[0] == brakeHandleNukitoriSegment));
            }
            else
            {
                EndChangeSWColider1e.enabled = (!UseEnd1[0] && !UseEnd2[0] && (zengoSwSegment1e[0] == 2)) || (UseEnd1[0] && (notchSegment1e[0] == notchOffSegment) && (reverserSegment1e[0] == 1));
                EndChangeSWColider2e.enabled = (!UseEnd1[0] && !UseEnd2[0] && (zengoSwSegment2e[0] == 2)) || (UseEnd2[0] && (notchSegment2e[0] == notchOffSegment) && (reverserSegment2e[0] == 1));
            }
            reverserColider1e.enabled = UseEnd1[0] && (notchSegment1e[0] == notchOffSegment);
            reverserColider2e.enabled = UseEnd2[0] && (notchSegment2e[0] == notchOffSegment);
        }
    }
}
