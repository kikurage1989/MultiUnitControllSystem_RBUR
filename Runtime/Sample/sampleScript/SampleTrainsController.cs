
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.UdonNetworkCalling;
using TMPro; //TextMeshProを扱う際に必要
using System;//Stringに使用
using ragecraft.UtilsScript;
using frou01.GrabController;
using ragecraft.MultiUnitControllSystem_RBUR;

/////////////////////////////////////////////////////////////////////////////////
// このサンプルスクリプトについては改変・再配布など自由に使用していただいて大丈夫です。
// AssemblyDefinitionは各々適切に設定してください。
/////////////////////////////////////////////////////////////////////////////////

//編成の要件
//Train_PrefabのFCouplerObjがついている方を1エンド側と定義する
//編成の前後でFCouplerObj、BCouplerObjが揃うこと
//1ハンドル車を作成したい場合は、brakeLever1e、brakeLever2e　をNoneにすること

//車両メッシュアニメーターをtrainMeshAnimatorsへ設定してください。
//isRoomLight isOpenRightDoor isOpenLeftDoorへの送信は元クラスから送信しています。

//Update()以外のUpdate系イベントはMultiUnitControllSystem内で不使用です。自由に扱ってください。

namespace ragecraft.MUCS_Sample
{
    public class SampleTrainsController : MultiUnitControllSystem
    {
        //開発用仮実装
        [Header("起動Sw　パンタグラフ等")]
        [SerializeField] protected syncSW_Base powerEnableSW;
        protected bool[] isPowerEnableSw = new bool[1];
        [SerializeField] protected frou01.RigidBodyTrain.MortorAndWheel _MotorAndWheel;
        protected float[] outputMortorForce = new float[1];
        protected float[] outputBrakeForce = new float[1];

        //AddStartProcess()     Start()での追加処理
        protected override void AddStartProcess()
        {
            isPowerEnableSw = powerEnableSW.udonSyncedBool;
            outputMortorForce = _MotorAndWheel.MortorForce;
            outputBrakeForce = _MotorAndWheel.BrakeForce;
        }

        //SwReadProcess()      Update()内で実行　スイッチ状態読み取り
        protected override void SwReadProcess() 
        {
            EnablePermission = isPowerEnableSw[0];
        }

        //DecideNotchAndBrakePos()      Update()内で実行　制御車での速度制限やATS等の非常制動など
        protected override void DecideNotchAndBrakePos()
        {
            base.DecideNotchAndBrakePos();
            // base.DecideNotchAndBrakePos()　内容
            // notchPos = notchSegmentLocal;
            // brakeSeg = brakeSegmentLocal;
            // brakePos = brakePositionLocal;
            // brakeNormPos = brakeNormPosLocal;
        }

        //PowerAndBrakeProcess()        Update内で通信が確立されていたら実行される。これをoverrideして各車種の開発
        protected override void PowerAndBrakeProcess() 
        {
            //Time.deltaTimeの値はprotected float updateDeltaTimeに格納済
            if(isOwnerState)
            {
                if(EnablePermission) outputMortorForce[0] = powerDirection * 10000f * notchPos;
                else outputMortorForce[0] = 0f;
                outputBrakeForce[0] = (brakeSeg == 0 ? 0f : 70000f * brakeNormPos);
            }
        }
    }
}
