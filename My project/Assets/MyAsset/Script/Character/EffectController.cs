using DarkTonic.MasterAudio;
using PathologicalGames;
using RenownedGames.Apex.Serialization.Collections.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// 必要なのはエフェクトとサウンドを呼び出すための物
/// アニメ管理もやるか
/// </summary>
public class EffectController : MonoBehaviour
{

    #region　定義

    [Serializable]
    public class MyParticles : SerializableDictionary<string, ParticleSystem>
    {
        [SerializeField]
        private List<string> keys;

        [SerializeField]
        private List<ParticleSystem> values;

        protected override List<string> GetKeys()
        {
            return keys;
        }

        protected override List<ParticleSystem> GetValues()
        {
            return values;
        }

        protected override void SetKeys(List<string> keys)
        {
            this.keys = keys;
        }

        protected override void SetValues(List<ParticleSystem> values)
        {
            this.values = values;
        }
    }


    NowEffect nowState;

    NowEffect nowCondition;

    NowEffect nowBuff;




    struct NowEffect
    {
        /// <summary>
        /// 列挙型に対応してる
        /// </summary>
        public int num;

        /// <summary>
        /// 使用中のエフェクト
        /// </summary>
        public Transform ef;

        


    }


    #endregion


    #region シリアライズ

    [SerializeField]
    SpawnPool pool;



    /// <summary>
    /// 行動エフェクトを出す
    /// </summary>
    [SerializeField]
    Transform StateEffect;


    /// <summary>
    /// 状態エフェクトを出す
    /// </summary>
    [SerializeField]
    Transform ConditionEffect;

    /// <summary>
    /// 追加状態エフェクトを出す
    /// </summary>
    [SerializeField]
    Transform ExtraEffect;

    /// <summary>
    /// 名前でエフェクト呼び出す
    /// </summary>
    [SerializeField]
    MyParticles Particles = new MyParticles();


    [SerializeField, Header("音")]
    [Header("アクションの音")]//smallはひたひたって感じにする？
    [SoundGroup]
    public String[] StateSound;

    [SerializeField, Header("音")]
    [Header("コンディションの音")]//smallはひたひたって感じにする？
    [SoundGroup]
    public String[] ConditionSound;

    [SerializeField, Header("音")]
    [Header("バフの音")]//smallはひたひたって感じにする？
    [SoundGroup]
    public String[] ExtraSound;


    /// <summary>
    /// エフェクトを使うアクション
    /// </summary>
    [SerializeField]
    CharacterController.CharacterState[] _activeAction;


    /// <summary>
    /// エフェクトを使う状態
    /// </summary>
    [SerializeField]
    CharacterController.CharacterCondition[] _activeCondition;

    /// <summary>
    /// エフェクトを使う状態変化
    /// </summary>
    [SerializeField]
    CharacterController.ExtraCondition[] _activeEffect;

    #endregion


    #region 内部パラメータ

    Animator anim;

    bool conditionChange;

    protected const string _runningAnimationParameterName = "ActionNum";
    protected int _runningAnimationParameter;

    int animatorParam;

    int animeNum;

    string waitAnime;

    CharacterController _con;

    #endregion


    private void Start()
    {
        anim = GetComponent<Animator>();
        _con = GetComponent<CharacterController>();
        animatorParam = RegisterAnimatorParameter(_runningAnimationParameterName);
        nowCondition.num = (int)CharacterController.CharacterCondition.none;
        nowBuff.num = (int)CharacterController.ExtraCondition.none;

       
 
    }

    private void Update()
    {
        //死ぬ
        if(nowCondition.num == 3)
        {
            conditionChange = true;
            animeNum = 15;
            waitAnime = "Die";
        }
        //ダメージ
        else if (nowCondition.num == 0)
        {
            conditionChange = true;
            animeNum = 14;
            waitAnime = "Damage";
        }
        else
        {
            
            animeNum = nowState.num;
            if (nowState.num == 4)
            {

            }

        }

        UpdateAnimatorInteger(anim,animatorParam,animeNum);

        if (conditionChange)
        {
            if (CheckEnd(waitAnime))
            {
                conditionChange = false;

                //死なら処理を分ける
                if (animeNum == 15)
                {
                    ScoreManager.instance.Die();
                }
                else
                {
                    //無敵解除
                    gameObject.layer = 0;
                    //元のアクションに復帰する
                    ConditionChange(CharacterController.CharacterCondition.none);
                    nowState.num = (int)_con.JudgeAction();
                }
            }


        }





    }



    #region  エフェクト呼び出し



    public void SetActionAgle(float angle)
    {
        if (angle == 0)
        {
            angle = 270;
        }
        else
        {
            angle = angle > 0 ? 270 + (90 - angle) : 270 - (90 + angle);
        }




        // StateEffect.rotation = Quaternion.Euler(new Vector3(0,0,(angle + 180) % 360));
        StateEffect.rotation = Quaternion.Euler(angle,90,-90);
     //   Debug.Log($"ewfwefwe{StateEffect.rotation}tototo{angle}");
    }


    /// <summary>
    /// アクション変更時にエフェクトを呼び出す
    /// </summary>
    public void ActionChange(CharacterController.CharacterState state)
    {
        if (conditionChange)
        {
            return;
        }



        if (state != CharacterController.CharacterState.none)
        {
            int number = (int)state;
            

            if (nowState.ef != null)
            {
                pool.Despawn(nowState.ef);
                StopSound(StateSound[nowState.num]);
                nowState.ef = null;
            }

            bool isEffect = false;

            for (int i = 0;i < _activeAction.Length;i++)
            {
                if (state == _activeAction[i])
                {
                    isEffect = true;
                    break;
                }

            }
            if (isEffect)
            {

                //フロートか落下以外
                if (number == 1)
                {
                  //  Debug.Log($"あああ{state}");
                    nowState.ef = MySpawn(Particles[StateSound[number]], StateEffect).transform;
                    //音だけ
                    FollowSound(StateSound[number], transform);
                }
                else
                {
                    MySpawn(Particles[StateSound[number]], StateEffect);
                    PlaySound(StateSound[number], transform.position);
                    nowState.ef = null;


                }
            }
            nowState.num = number;
        }
        else
        {
            if (nowState.ef != null)
            {
                pool.Despawn(nowState.ef);
                StopSound(StateSound[nowState.num]);
                nowState.ef = null;
            }
            nowState.num = 4;
           // _con.StopAct();
        }
    }


    /// <summary>
    /// コンディション変更時にエフェクトを呼び出す
    /// ノンで状態キャンセル
    /// </summary>
    public void ConditionChange(CharacterController.CharacterCondition state)
    {

        if (state != CharacterController.CharacterCondition.none)
        {

            int number = (int)state;

            if (nowCondition.ef != null)
            {
                pool.Despawn(nowCondition.ef);
                StopSound(StateSound[nowCondition.num]);
                nowCondition.ef = null;
            }

            bool isEffect = false;

            for (int i = 0; i <= _activeCondition.Length - 1; i++)
            {
                if (state == _activeCondition[i])
                {
                    isEffect = true;
                    break;
                }

            }

            if (isEffect)
            {

                    MySpawn(Particles[StateSound[number]], StateEffect);
                    PlaySound(StateSound[number], transform.position);
                    nowCondition.ef = null;

            }
            
            nowCondition.num = number;
        }

        else
        {
            nowCondition.num = 4;
        }
    }


    /// <summary>
    /// バフ変更時にエフェクトを呼び出す
    /// 優先順でエフェクト上書きする
    /// 現在のエフェクトが消えた後は他にバフがないか調べる
    /// </summary>
    public void BuffChange(CharacterController.ExtraCondition state)
    {

        if (state != CharacterController.ExtraCondition.none)
        {

            int number = (int)state;

            bool isEffect = false;

            for (int i = 0; i < _activeEffect.Length; i++)
            {
                if (state == _activeEffect[i])
                {
                    isEffect = true;
                    break;
                }

            }

            if (nowBuff.ef != null)
            {
                //none以外で優先度上の状態変化なら
                if ( number < nowBuff.num)
                {
                    pool.Despawn(nowBuff.ef);
                    StopSound(ExtraSound[nowBuff.num]);
                    nowBuff.ef = null;
                    nowBuff.num = number;
                }

                //優先度低いならエフェクトを使わせないしナンバーも上書きしない
                else
                {
                    isEffect = false;
                }
            }





            if (isEffect)
            {

                //nowBuff.ef = MySpawn(Particles[ExtraSound[number]],  ExtraEffect).transform;
                PlaySound(ExtraSound[number], transform.position);
                

            }
        }
    }



    /// <summary>
    /// バフ変更時にエフェクトを呼び出す
    /// 優先順でエフェクト上書きする
    /// 現在のエフェクトが消えた後は他にバフがないか調べる
    /// </summary>
    public void BuffEnd(CharacterController.ExtraCondition state,CharacterController.ExtraCondition sub = CharacterController.ExtraCondition.none)
    {

        if (state != CharacterController.ExtraCondition.none)
        {


            //使用中のエフェクトがあり、現在一番優先度高いエフェクトより優先度高いなら消す
            if (nowBuff.ef != null && sub > state)
            {
                pool.Despawn(nowBuff.ef);
                StopSound(ExtraSound[nowBuff.num]);
                nowBuff.ef = null;


            }

            if (sub != CharacterController.ExtraCondition.none && sub != state)
            {
                //ここで別に優先度低いエフェクトがあるならそっち使う
                //nowBuff.num = number;
                BuffChange(sub);
            }

        }

    }


    public ParticleSystem MySpawn(ParticleSystem prefab, Transform parent)
    {
        if(prefab == null)
        {
            return null;
        }
        return pool.Spawn(prefab, parent.position, parent.rotation, parent);
    }


    #endregion

    #region　音
    /// <summary>
    /// 音声再生。音源に追随しない
    /// 普通は再生する音声と
    /// </summary>
    /// <param name="sType">再生する音の名前。バリエーションありのやつ。
    /// <param name="sourcePosition">音を鳴らしたい位置。必須です。 </param>
    /// <param name="volumePercentage"><b>Optional</b> - 音量を下げて再生したい場合に使用します（0～1の間）。
    /// <param name="pitch"><b>Optional</b> - 特定のピッチで音を再生したい場合に使用します。</param> <param name="pitch"><b>Optional</b> - 特定の音程で再生したい場合に使用します。そうすると、バリエーションの中のpichとrandom pitchを上書きします。
    /// <param name="delaySoundTime"><b>Optional</b> - すぐにではなく、X秒後に音を鳴らしたい場合に使用します。
    /// <param name="variationName"><b>Optional</b> - 特定のバリエーション（またはクリップID）の名前で再生したい場合に使用します。それ以外の場合は、ランダムなバリエーションが再生されます。
    /// <param name="timeToSchedulePlay"><b>Optional</b> - サウンドを再生するためのDSP時間を渡すために使用します。通常はこれを使用せず、代わりにdelaySoundTimeパラメータを使用します。
    /// <param name="isRemember"><b>Optional</b> - PlaySoundResultを取得するかどうか。
    /// <returns>PlaySoundResult - このオブジェクトは、サウンドが再生されたかどうかを読み取るために使用され、使用されたVariationオブジェクトへのアクセスも可能です。

    public void PlaySound(string sType, Vector3 sourceTrans, float volumePercentage = 1f, float? pitch = null, float delaySoundTime = 0f, string variationName = null, double? timeToSchedulePlay = null, bool isRemember = false)
    {
        // Debug.Log("ｄｄｄ");
        if (isRemember)
        {
            MasterAudio.PlaySound3DAtVector3(sType, sourceTrans, volumePercentage, pitch, delaySoundTime, variationName, timeToSchedulePlay);
        }
        else
        {
            MasterAudio.PlaySound3DAtVector3AndForget(sType, sourceTrans, volumePercentage, pitch, delaySoundTime, variationName, timeToSchedulePlay);
            
        }

    }
    /// <summary>
    /// 音声再生。音源に追随する
    /// </summary>
    /// <param name="sType">再生する音の名前。バリエーションありのやつ。
    /// <param name="sourcePosition">音を鳴らしたい位置。必須です。 </param>
    /// <param name="volumePercentage"><b>Optional</b> - 音量を下げて再生したい場合に使用します（0～1の間）。
    /// <param name="pitch"><b>Optional</b> - 特定のピッチで音を再生したい場合に使用します。</param> <param name="pitch"><b>Optional</b> - 特定の音程で再生したい場合に使用します。そうすると、バリエーションの中のpichとrandom pitchを上書きします。
    /// <param name="delaySoundTime"><b>Optional</b> - すぐにではなく、X秒後に音を鳴らしたい場合に使用します。
    /// <param name="variationName"><b>Optional</b> - 特定のバリエーション（またはクリップID）の名前で再生したい場合に使用します。それ以外の場合は、ランダムなバリエーションが再生されます。
    /// <param name="timeToSchedulePlay"><b>Optional</b> - サウンドを再生するためのDSP時間を渡すために使用します。通常はこれを使用せず、代わりにdelaySoundTimeパラメータを使用します。
    ///     /// <param name="isRemember"><b>Optional</b> - PlaySoundResultを取得するかどうか。
    /// <returns>PlaySoundResult - このオブジェクトは、サウンドが再生されたかどうかを読み取るために使用され、使用されたVariationオブジェクトへのアクセスも可能です。
    public void FollowSound(string sType, Transform sourceTrans, float volumePercentage = 1f, float? pitch = null, float delaySoundTime = 0f, string variationName = null, bool isRemember = false)
    {

        if (isRemember)
        {
            MasterAudio.PlaySound3DFollowTransform(sType, sourceTrans, volumePercentage, pitch, delaySoundTime, variationName);
        }
        else
        {
            MasterAudio.PlaySound3DFollowTransformAndForget(sType, sourceTrans, volumePercentage, pitch, delaySoundTime, variationName);
        }
    }


    /// <summary>
    /// 最後のtrueにしないならフェードが基本
    /// </summary>
    /// <param name="soundGroupName"></param>
    /// <param name="fadeTime"></param>
    /// <param name="isStop"></param>
    public void StopSound(string soundGroupName, float fadeTime = 1, bool isStop = false)
    {
        if (!isStop)
        {
            MasterAudio.FadeOutAllOfSound(soundGroupName, fadeTime);
        }
        else
        {
            MasterAudio.StopAllOfSound(soundGroupName);
        }
    }



    public void AnimSound(int index = 0)
    {
        //ダメージなら
        if (nowCondition.num == 0)
        {
            PlaySound("Damage",transform.position);
        }
        else if (nowCondition.num == 3)
        {
            if (index == 0)
            {
                PlaySound("Damage",transform.position);
            }
            else
            {
                PlaySound("Die", transform.position);
            }

           
        }

    }

    #endregion





    #region　アニメ管理

    /// <summary>
    /// Updates the animator integer.
    /// </summary>
    /// <param name="animator">Animator.</param>
    /// <param name="parameter">Parameter name.</param>
    /// <param name="value">Value.</param>
    public static bool UpdateAnimatorInteger(Animator animator, int parameter, int value)
    {
      //  Debug.Log($"ああ{value}");
        animator.SetInteger(parameter, value);
       // Debug.Log($"eqww{animator.GetInteger(parameter)}");
        return true;
    }

    /// <summary>
    /// Registers a new animator parameter to the list
    /// MyCharacterに適合させるオーバーライド
    /// </summary>
    /// <param name="parameterName">Parameter name.</param>
    /// <param name="parameterType">Parameter type.</param>
    public int RegisterAnimatorParameter(string parameterName)
    {
        return Animator.StringToHash(parameterName);
    }

    bool CheckEnd(string Name)
    {

        if (!anim.GetCurrentAnimatorStateInfo(0).IsName(Name))// || sAni.GetCurrentAnimatorStateInfo(0).IsName("OStand"))
        {   // ここに到達直後はnormalizedTimeが"Default"の経過時間を拾ってしまうので、Resultに遷移完了するまではreturnする。
            return false;
        }
        if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {  

                return false;
        }

        return true;


    }

    #endregion



}
