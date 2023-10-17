using UnityEngine;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using System.Linq;
using Beautify.Universal;
using System;
using static Dreamteck.AsyncJobSystem;
using System.Threading;
using Unity.Entities.UniversalDelegates;
using Cysharp.Threading.Tasks;

public class CharacterController : SaveMono
{
    public float jumpPower;
    public float angle;

    [SerializeField]
    float reflectForce = 45;

    private Rigidbody2D rb;

    [SerializeField]
    GameObject SpriteEffect;

    /// <summary>
    /// タッチが有効な範囲
    /// </summary>
    public float touchRange;

    /// <summary>
    /// タッチ成功になるポイント
    /// ちょっとよく飛ぶ
    /// </summary>
   public  float sweetRange;

    #region　内部ステータス



    /// <summary>
    /// 現在の数値
    /// </summary>
    float nowJumpPower;
    float nowAngle;

    float horizontal;

    Vector2 direction = new Vector2();

    /// <summary>
    /// ぶつかったイベントのデータを管理
    /// データ内のTimerで時間をはかってる
    /// </summary>
    [SerializeField]
    [ES3Serializable]
    List<EventObject.EventData> _effectData = new List<EventObject.EventData>();

    float jumpTime;

    bool isGround;

    [SerializeField]
    CharacterState nowAction;

    CharacterState lastAction;

    EffectController efCon;

    Vector2 velocityCOn;
    Vector2 ground = new Vector2(0,10);

    /// <summary>
    /// いまどの状態異常やバフがメインでグラフィックに反映されてるか
    /// </summary>
    [SerializeField]
    [ES3Serializable]
    GimickCondition _myStatus;

    [SerializeField] private LayerMask groundLayer;

    [SerializeField]
    float gCheckSpan;

    RaycastHit2D hit;

    #endregion


    #region


    /// <summary>
    /// 継続的な行動
    /// </summary>
    public enum CharacterState
    {
        jump,

        fall,
        walk,
        Float,//浮く
        idle,
        none
    }

    /// <summary>
    /// 一回限りの状態変化
    /// </summary>
    public enum CharacterCondition
    {
       
        damage,
        heal,
        addScore,
        die,
        none
    }


    /// <summary>
    /// バフデバフ
    /// </summary>
    public enum ExtraCondition
    {
        
        invincible,//無敵
        boostJump,
        none
    }




    /// <summary>
    /// ステータス用
    /// </summary>
     [Serializable]
    public struct GimickCondition
    {
        //無敵かどうか
        public bool invincible;

        public bool boostJump;

        public bool Die;

    }

    #endregion


    enum jumpStatus
    {
        weak = -1,
        normal = 0,
        add = 1
    }

    jumpStatus _status;


    /// <summary>
    /// 最後のアニメのせっとできたかな
    /// </summary>
    int isTrigger;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        efCon = GetComponent<EffectController>();
        ChangeAction(CharacterState.idle);
        //  BeautifySettings.settings.blurIntensity.value = 1.2f;
        _status = jumpStatus.normal;
    }

    void Update()
    {
        //死んでるなら戻れ
        if (_myStatus.Die)
        {
            Debug.Log("あｄｆｇｇ");
            rb.velocity = Vector2.zero;
            return;
        }

        if (ScoreManager.instance.isGoal)
        {
            //エフェクト消そう
            if (_effectData.Any())
            {
                int count = _effectData.Count;
                for (int i = 0;i < count;i++)
                {
                    ConditionEnd(_effectData[i]);
                    _effectData.Remove(_effectData[i]);
                }
                _effectData = null;
            }

            if (isGround)
            {
                //トリガーポイントに向かう処理
                // トリガーポイントというか、X座標ゼロでいいわ
                //行きついたら最後のアニメを開始
                //いやアニメいらない、動かしながら回転させる
                //そして移動オワタら0になるまでまた回転、その後アニメ開始
                //回転処理はここに、移動処理はFixedに置くか
                //ゴールでグラウンドでトリガー到達してないなら進むよみたいに

                if (Math.Abs(isTrigger) == 1)
                {
                    //回転してアニメ開始　

                    //右に
                    if(isTrigger > 0)
                    {
                        transform.Rotate(0,0,-2);
                    }
                    //
                    else
                    {
                        transform.Rotate(0, 0, 2);
                    }

                    //止まってるなら
                    if(rb.velocity == Vector2.zero)
                    {
                        if(Math.Abs(transform.rotation.z) < 10)
                        {
                            transform.rotation = Quaternion.Euler(Vector3.zero);
                            isTrigger = 2;
                            efCon.EndAnimStart();
                        }
                    }
                }


            }

            //ゴールしたなら止まれ
            return;
        }

        if(nowAction == CharacterState.none)
        {
            return;
        }
            horizontal = Input.GetAxisRaw("Horizontal");
            _status = (jumpStatus)Input.GetAxisRaw("Vertical");
        if (nowAction == CharacterState.jump)
        {

            if (Input.GetMouseButtonDown(1))
            {
                MasterAudio.PlaySound3DAtVector3AndForget("JumpStop",transform.position);
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0;
                ChangeAction(CharacterState.fall);
            }
           

            // 上下左右の方向キーの入力を読み取る

        }

        if (_myStatus.Die)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {




            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            
            
            //クリック地点の座標獲得
            Vector2 _position = Physics2D.Raycast((Vector2)ray.origin, (Vector2)ray.direction).point;

            #region 没処理
            /*
            RaycastHit2D hit2d = Physics2D.Raycast((Vector2)ray.origin, (Vector2)ray.direction);
            if (hit2d.transform == null)
            {
                return;
            }

            if (this.gameObject == hit2d.transform.gameObject)
            {
                ChangeAction(CharacterState.jump);
                rb.velocity = Vector2.zero;
                JumpCheck();
                if(nowAngle == 0)
                {
                    direction.Set(0, nowJumpPower);
                }
                else
                {
                    nowAngle *= Mathf.Deg2Rad;
                    direction.Set(nowJumpPower * Mathf.Cos(nowAngle), nowJumpPower * Mathf.Sin(nowAngle));
                }
                
                rb.AddForce(direction, ForceMode2D.Impulse);

            }
            */
            #endregion

            //タッチしたとことキャラの距離を
            float distance = Vector2.Distance(Physics2D.Raycast((Vector2)ray.origin, (Vector2)ray.direction).point,transform.position);


           //ｗ Debug.Log($"ああああああ{distance}");

            //範囲超えてるなら失敗
            if(distance > touchRange)
            {
                if (distance > touchRange * 1.5f)
                {
                    return;
                }
                nowJumpPower = jumpPower * 0.4f;
                
            }

 


            if (distance <= sweetRange)
            {
                nowJumpPower = jumpPower * 1.15f;
            }

            ChangeAction(CharacterState.jump);
            rb.velocity = Vector2.zero;
            JumpCheck();
            if (nowAngle == 0)
            {
                direction.Set(0, nowJumpPower);
            }
            else
            {
                nowAngle *= Mathf.Deg2Rad;
                direction.Set(nowJumpPower * Mathf.Cos(nowAngle), nowJumpPower * Mathf.Sin(nowAngle));
            }

            rb.AddForce(direction, ForceMode2D.Impulse);


        }


    }

    private void OnEnable()
    {
        GroundCheck(true).Forget();
    }



    private void FixedUpdate()
    {

        //  状態変化の時間を監視
        ConditionTimer();


        if(ScoreManager.instance.isGoal && isGround)
        {
            if(isTrigger > 1)
            {
                return;
            }

            float basePosi = ScoreManager.instance.PlayerPosi.x;

            //誤差１いないなら停止
            if (Math.Abs(basePosi) < 1)
            {
                rb.velocity = Vector2.zero;
            }
            else
            {

                //右にいる時左へ
                if(basePosi > 0)
                {
                    isTrigger = -1;
                    rb.velocity = Vector2.left * 10;
                }
                else
                {
                    isTrigger = 1;
                    rb.velocity = Vector2.right * 10;
                }
            }

            return;
        }


        //ジャンプ中なら
        if (nowAction == CharacterState.jump)
        {

            //上に上がれなくなったら初期化
            //他にも障害物に当たったらジャンプ中止とか？
            if (rb.velocity.y < 0 && Time.time - jumpTime > 1.2f)
            {
                nowAngle = angle;
                nowJumpPower = jumpPower;
                ChangeAction(CharacterState.fall);
            }
        }
        else
        {

            if (isGround)
            {
                ChangeAction(CharacterState.idle);
            }
            else if (rb.velocity.y >= 0 && nowAction != CharacterState.Float)
            {
                ChangeAction(CharacterState.Float);

            }
            else if (nowAction != CharacterState.fall && nowAction != CharacterState.Float)
            {
                ChangeAction(CharacterState.fall);
            }


        }


        if (Mathf.Abs(rb.velocity.y) > 200)
        {
            float ySpeed = rb.velocity.y > 0 ? 200 : -200;

            velocityCOn.Set(rb.velocity.x,ySpeed) ;
            rb.velocity = velocityCOn;
        }
    }

    #region ギミック

    /// <summary>
    /// アイテムの効果を記す
    /// </summary>
    /// <param name="data"></param>
    void GimickAct(EventObject.EventData data, Collider2D collision)
    {
        Debug.Log($"tttttt{data.type}");
        //即時効果
        if (data.effectTime == 0 || data.type == EventObject.EventType.scoreGet)
        {
            if (data.type == EventObject.EventType.damage)
            {
                //無敵なら
                if (_myStatus.invincible)
                {
                    //無敵の音鳴らす？
                    
                    return;
                }

                //スコアも10へる
                ScoreManager.instance.ScoreChange(-100);
                bool isDie = ScoreManager.instance.LifeChange(true);

                StopAction();

                gameObject.layer = 8;

                if (isDie)
                {
                    ScoreManager.instance.isDie = true;
                    _myStatus.Die = true;
                    efCon.ActionChange(CharacterController.CharacterState.none);
                    efCon.ConditionChange(CharacterCondition.die);
                }
                else
                {
                    efCon.ConditionChange(CharacterCondition.damage);
                }
            }
            else if (data.type == EventObject.EventType.recover)
            {
                efCon.PlaySound("Recover", ScoreManager.instance.PlayerPosi);
                ScoreManager.instance.LifeChange();
                //回復エフェクト
                efCon.ConditionChange(CharacterCondition.heal);
            }

            else if (data.type == EventObject.EventType.scoreGet)
            {
                efCon.PlaySound("ScoreUp",ScoreManager.instance.PlayerPosi);
                ScoreManager.instance.ScoreChange((int)data.effectTime);
            }
            else if (data.type == EventObject.EventType.Random)
            {

            }
        }

        //時間持続効果
        else
        {
            //無敵ならバッドステータスは受け付けない
            if (_myStatus.invincible && data.bad)
            {
                //無敵の音鳴らす？
                return;
            }

            ConditionContraller(data);
        }


    }

    void ConditionContraller(EventObject.EventData data)
    {

        

        if (_effectData.Any())
        {
            for (int i = 0; i < _effectData.Count; i++)
            {
                if (data.type == _effectData[i].type)
                {
                 //   Debug.Log($"あdsasdasdwewer{data.type}{_effectData[i].type}{_effectData.Count}");
                    return;
                }
            }
        }

        if (!data.bad)
        {
          //  Debug.Log("wsdaewer");
            if (data.type == EventObject.EventType.invincible)
            {
                _myStatus.invincible = true;
                SpriteEffect.SetActive(true);
              //  Debug.Log($"あer{_myStatus.invincible}");
                //無敵の音鳴らす?
                MasterAudio.PlaySound3DAtVector3AndForget("Incivle", transform.position);
            }
            else if (data.type == EventObject.EventType.boostJump)
            {
                _myStatus.boostJump = true;
                efCon.BuffChange(ExtraCondition.boostJump);
            }


            /*
                         else if (data.type == EventObject.EventType)
            {

            }
             */
        }
        else
        {
            if (data.type == EventObject.EventType.badSight)
            {
                MasterAudio.PlaySound3DAtVector3AndForget("BadSight", transform.position);
                BeautifySettings.settings.blurIntensity.value = 1.2f;
            }
        }

        data.timer = Time.time;
      //  Debug.Log($"あああ{data.type}{data.timer}");
        _effectData.Add(data);

    }


    /// <summary>
    /// 起動時に状態を元に戻す
    /// </summary>
    void ConditionRecovery(EventObject.EventData[] dataArray)//, GimickCondition visualData)
    {


        for (int i = 0;i < dataArray.Length; i++)
        {
            if (!dataArray[i].bad)
            {
                //  Debug.Log("wsdaewer");
                if (dataArray[i].type == EventObject.EventType.invincible)
                {
                    _myStatus.invincible = true;
                    SpriteEffect.SetActive(true);
                    //  Debug.Log($"あer{_myStatus.invincible}");
                    //無敵の音鳴らす?
                    MasterAudio.PlaySound3DAtVector3AndForget("Incivle", transform.position);
                }
                else if (dataArray[i].type == EventObject.EventType.boostJump)
                {
                    _myStatus.boostJump = true;
                    efCon.BuffChange(ExtraCondition.boostJump);
                }

            }
            else
            {
                if (dataArray[i].type == EventObject.EventType.badSight)
                {
                    MasterAudio.PlaySound3DAtVector3AndForget("BadSight", transform.position);
                    BeautifySettings.settings.blurIntensity.value = 1.2f;
                }
            }
            dataArray[i].timer = Time.time;
            //  Debug.Log($"あああ{dataArray[i].type}{dataArray[i].timer}");
            _effectData.Add(dataArray[i]);
        }


    }




    /// <summary>
    /// 状態の時間をはかる
    /// </summary>
    void ConditionTimer()
    {
        if (_effectData.Any())
        {
            for (int i = 0; i < _effectData.Count; i++)
            {

                //時間超えたら消す
                //または今無敵で状態わるいやつなら
                if(Time.time - _effectData[i].timer > _effectData[i].effectTime || _myStatus.invincible && _effectData[i].bad)
                {
                 //   Debug.Log($"wwwdwd{_effectData[i].type}{Time.time - _effectData[i].timer > _effectData[i].effectTime}{_effectData[i].timer}");
                    ConditionEnd(_effectData[i]);
                    _effectData.Remove(_effectData[i]);
                }

            }
        }

    }

    /// <summary>
    /// 特殊状態を終わらせる
    /// </summary>
    void ConditionEnd(EventObject.EventData data)
    {

        if (data.bad)
        {
            if (data.type == EventObject.EventType.badSight)
            {
                BeautifySettings.settings.blurIntensity.value = 0f;
            }
        }
        else
        {
            if (data.type == EventObject.EventType.invincible)
            {
                _myStatus.invincible = false;
                SpriteEffect.SetActive(false);
                gameObject.layer = 0;

            }
            else if (data.type == EventObject.EventType.boostJump)
            {
                _myStatus.boostJump = false;

            }
        }


    }

    public async UniTask DieRecover()
    {

        if (_effectData.Any())
        {
            int count = _effectData.Count;
            for (int i =0;i<count;i++)
            {
                ConditionEnd(_effectData[i]);
            }
        }
        efCon.ConditionChange(CharacterCondition.none);
        await UniTask.Delay(TimeSpan.FromSeconds(1f));
        this.gameObject.layer = 0;
        _myStatus.Die = false;
    }


#endregion

    #region　エフェクト装置との連携
    /// <summary>
    /// 状態異常で止まってたのをもとに
    /// </summary>
    /// <returns></returns>
    public CharacterState JudgeAction()
    {
        nowAction = lastAction;
        return nowAction;
    }

    //状態異常で止まる
    public void StopAction()
    {

        if (nowAction == CharacterState.jump)
        {
            nowAction = CharacterState.fall;
        }
        lastAction = nowAction;
        nowAction = CharacterState.none;
        efCon.ActionChange(CharacterState.none);
    }



    public void ChangeAction(CharacterState state)
    {
        if (state == CharacterState.jump)
        {
            jumpTime = Time.time;
        }

        nowAction = state;
        efCon.ActionChange(state);
    }

    #endregion


    /// <summary>
    /// ジャンプのステータスを確認
    /// </summary>
    void JumpCheck()
    {
        if (horizontal != 0)
        {
            nowJumpPower = jumpPower;

            nowAngle = angle;
          //  Debug.Log($"角度eedss{nowAngle}");
            if (_status == jumpStatus.add)
            {
                nowAngle *= 1.5f;
            }
            else if (_status == jumpStatus.weak)
            {
                nowAngle *= 0.5f;
                nowJumpPower = jumpPower * 0.6f;
            }
            nowAngle = horizontal > 0 ? nowAngle : 180 - nowAngle;

          //  Debug.Log($"角度ssss{nowAngle}");
        }
        else
        {
            nowAngle = 0;
            if (_status == jumpStatus.add)
            {
                nowJumpPower = jumpPower * 1.2f;
            }
            else if (_status == jumpStatus.weak)
            {
                nowJumpPower = jumpPower * 0.8f;
            }
        }


        efCon.SetActionAgle(nowAngle);

        if (_myStatus.boostJump)
        {
            nowJumpPower *= 1.5f;
        }
     //   Debug.Log($"角度{nowAngle}");
    }












    private void OnTriggerEnter2D(Collider2D collision)
    {
        //地形じゃないならジャンプやめる
   //     if (collision.gameObject.tag != "stage")
 //       {
  //          nowAngle = angle;
   //         nowJumpPower = jumpPower;
  //         nowAction = CharacterState.fall;
  //      }

        //バフアイテムは一回触ると消える?
        //消滅設定に従うか
        if (collision.gameObject.tag == "Gimick")
        {
            EventObject.EventData data = collision.gameObject.GetComponent<EventObject>().EventStart();


            Debug.Log("ｇｇ");
            GimickAct(data,collision);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {


        if (ScoreManager.instance.isGoal)
        {
            if(collision.gameObject.tag == "stage")
            {
                isGround = true;
            }
        }

        if (collision.gameObject.tag == "Reflect")
        {

            if (nowAction == CharacterState.jump)
            {
                ChangeAction(CharacterState.fall);
            }


            if (rb.velocity == Vector2.zero)
            {

                direction = Vector2.Reflect((collision.transform.position - transform.position).normalized, collision.contacts[0].normal);

            }
            else
            {
                // オブジェクトの進行方向を取得する
                // 衝突した壁の法線ベクトルを取得する
                // 反射する方向を求める
                direction = Vector2.Reflect(rb.velocity.normalized, collision.contacts[0].normal);
            }

            //跳ね返る音
            MasterAudio.PlaySound3DAtVector3AndForget("Reflect", transform.position);
         

//            rb.velocity = Vector2.zero;
  //          rb.angularVelocity = 0;

            // 反射のベクトルに力をかける
            rb.AddForce(direction * reflectForce, ForceMode2D.Impulse);
        }
    }


    async UniTaskVoid GroundCheck(bool isFirst = false)
    {
        if (!isFirst)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(gCheckSpan),cancellationToken:destroyCancellationToken);
        }

        if(gameObject.activeSelf == false)
        {
            return;
        }

        isGround = isGrounded();
        GroundCheck().Forget();
    }

    /// <summary>
    /// 接地判定
    /// </summary>
    /// <returns></returns>
    private bool isGrounded()
    {
        hit = Physics2D.Raycast(transform.position, Vector2.down, 5, groundLayer);
      //  if(hit.collider!= null)
       // Debug.Log($"あたたよ{hit.collider.name}");
        return hit.collider != null;
    }

    /// <summary>
    /// セーブ機能
    /// 必ずしも変数いらんね、文字列キーで呼び出して使えばいいし
    /// </summary>
    public override void Save()
    {
        //今の状態と使用エフェクトと今の位置を格納
        //ロード時必要な処理は状況の再現
        //位置設定もここでやるか

        //エフェクトデータの時間を確認する作業
        for (int i = 0; i < _effectData.Count; i++)
        {

            //時間超えてるのは消す
            //または今無敵で状態わるいやつなら
            if (Time.time - _effectData[i].timer > _effectData[i].effectTime)
            {
             //   Debug.Log($"wwwdwd{_effectData[i].type}{Time.time - _effectData[i].timer > _effectData[i].effectTime}{_effectData[i].timer}");
                ConditionEnd(_effectData[i]);
                _effectData.Remove(_effectData[i]);
            }
            else
            {
                EventObject.EventData data = _effectData[i];
                //効果時間を残り時間に変更
                data.effectTime -= (Time.time - _effectData[i].timer);
                _effectData[i] = data;
            }

        }


        ES3.Save("NowCondition", _effectData);
        ES3.Save("NowEffect",_myStatus);
        ES3.Save("PositionImfo", transform.position);
    }



    public override void Load()
    {
        ConditionRecovery(ES3.Load<List<EventObject.EventData>>("NowCondition").ToArray());
     //   ES3.Load("NowEffect");
        transform.position = ES3.Load<Vector3>("PositionImfo");
    }




}