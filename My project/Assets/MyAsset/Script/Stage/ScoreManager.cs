using RenownedGames.Apex.Serialization.Collections.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

using Com.LuisPedroFonseca.ProCamera2D;
using UnityEngine.Playables;
using Cysharp.Threading.Tasks;
using DarkTonic.MasterAudio;


/// <summary>
/// スコアとタイムの管理
/// UIも制御するか
/// </summary>
public class ScoreManager : SaveMono
{

    public static ScoreManager instance = null;

    #region スコアとタイムの変数
    /// <summary>
    /// ゲーム開始してるか
    /// </summary>
    bool inGame;

    /// <summary>
    /// 遊んでる時間
    /// </summary>
    float playTime;

    /// <summary>
    /// 現在のスコア
    /// </summary>
    int Score;

    int bestScore;

    public int BestScore
    {
        get { return bestScore; }
    }

    public GameObject Player;
    #endregion

    #region 体力関連の変数

    /// <summary>
    /// 初期残機数
    /// チェックポイントでここまで回復？
    /// いやそれはいらんな。スコア支払いで回復はあり
    /// </summary>
    public int maxLife;

    /// <summary>
    /// 体力、残機
    /// </summary>
    int life;


    public bool isDie;
    #endregion


    #region チェックポイント関連



    /// <summary>
    /// マップ切り替え時にレベルマネージャーに請求する
    /// セーブポイント立ち寄った時とかでもいいけど
    /// 今回はそれね
    /// </summary>
    Vector2 checkPoint;


    #endregion


    #region UI関係

    [SerializeField]
    TextMeshProUGUI scoreViewer;

    [SerializeField]
    TextMeshProUGUI timeViewer;

    [SerializeField]
    TextMeshProUGUI lifeViewer;

    int lastTime;


    [SerializeField]
    TextMeshProUGUI finalScore;

    [SerializeField]
    TextMeshProUGUI bounus;

    [SerializeField]
    TextMeshProUGUI summury;

    /// <summary>
    /// スコア発表窓
    /// </summary>
    [SerializeField]
    GameObject ScoreDip;


    /// <summary>
    /// UIのアニメーション
    /// </summary>
    [SerializeField]
    MyAnimator _anim;


    #endregion

    [SerializeReference]
    ProCamera2D _camera;


    [SerializeField]
    GameObject home;

    [SerializeField]
    GameObject newRecord;

    int timeBounus;

    #region

    [Serializable]
    public class MyAnimator : SerializableDictionary<string, Animator>
    {
        [SerializeField]
        private List<string> keys;

        [SerializeField]
        private List<Animator> values;

        protected override List<string> GetKeys()
        {
            return keys;
        }

        protected override List<Animator> GetValues()
        {
            return values;
        }

        protected override void SetKeys(List<string> keys)
        {
            this.keys = keys;
        }

        protected override void SetValues(List<Animator> values)
        {
            this.values = values;
        }
    }
    #endregion


    public Vector2 PlayerPosi;

    public bool isGoal;

    [SerializeField]
    PlayableDirector ending;


    private void Awake()
    {
        if(instance == null)
        {
            instance = this;

        }
        else
        {
            Destroy(this.gameObject);
        }
        PlayerPosi = Player.transform.position;
    }


    // Start is called before the first frame update
    void Start()
    {
        //テスト中はね
        //本番はタイトルからとんだ時点でやるか
        inGame = true;

        life = maxLife;
        AllUpdate();
    }

    private void LateUpdate()
    {
        PlayerPosi = Player.transform.position;
    }

    // Update is called once per frame
    void Update()
    {

        if (inGame)
        {
            //プレイ中は時間を加算
            playTime += Time.deltaTime;

            //UIも管理していく
            UIUpdate();
        }


    }

    #region スコアとタイム関連


   

    /// <summary>
    /// スコア表示
    /// </summary>
    public async void ScoreDisplay()
    {
        //スコア表示
        ScoreDip.SetActive(true);

        timeBounus = (1000 - (lastTime * 2)) * 10;
        timeBounus = timeBounus < 0 ? 0:timeBounus;
        await Display(Score, finalScore,false);

        await Display(timeBounus,bounus,false);

        await Display(timeBounus+Score,summury,true);

                Debug.Log($"まえ{bestScore}"); 
        //ここからベスト記録超えてるかとか、クリックしたらもどれるようにとか
        if(Score+timeBounus > bestScore)
        {
   
            bestScore = timeBounus + Score;
            Debug.Log($"あと{bestScore}");
            newRecord.SetActive(true);
            PlaySound("NewRecord");
            
        }
           EndGame().Forget();
    }

    async UniTask Display(int target,TextMeshProUGUI targetUI,bool total, int nowNum = 0)
    {
        if (nowNum == 0)
        {
            if (target > 40)
            {
                targetUI.text = "0";
            }
            else
            {
                if (total)
                {
                    PlaySound("TotalOpen");
                }
                else
                {
                    PlaySound("ScoreOpen");
                }
                targetUI.text = target.ToString();
                return;
            }
            await UniTask.Delay(TimeSpan.FromSeconds(0.8f));
            PlaySound("ScoreEffect");
        }
        else
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.05f));
        }
        nowNum = nowNum + (target / 40) <= target ? nowNum + (target / 40) : target;

        targetUI.text =  nowNum.ToString();

        if (nowNum != target)
        {
            await Display(target,targetUI,total,nowNum);
        }
        else
        {
            StopSound("ScoreEffect");
            if (total)
            {
                PlaySound("TotalOpen");
            }
            else
            {
                PlaySound("ScoreOpen");
            }
        }
    }


    async UniTaskVoid EndGame()
    {
        await UniTask.WaitUntil(() => Input.GetMouseButtonDown(0),cancellationToken: destroyCancellationToken);
        GetComponent<UIManager>().ClearSave();
    }


    public void ScoreChange(int change)
    {
        Debug.Log($"あｓｄｆｆ{change}");
        if (Score + change < 0)
        {
            Score = 0;
        }
        else if (Score + change > 999999999)
        {
            Score = 999999999;
        }
        else
        {
            Score += change;
        }
        scoreViewer.text = Score.ToString();
    }


    void UIUpdate()
    {
        //Debug.Log($"aaa{(int)playTime}dd{lastTime}");
        if ((int)playTime > lastTime)
        {
            lastTime = (int)playTime;
            timeViewer.text = lastTime.ToString();
        }
    }


    void AllUpdate()
    {
        timeViewer.text = lastTime.ToString();
        scoreViewer.text = Score.ToString();
        lifeViewer.text = life.ToString();
    }
    #endregion



    #region 体力関連


    /// <summary>
    /// ライフを減らす
    /// 返り値が真なら死亡
    /// </summary>
    public bool LifeChange(bool damage = false)
    {

        if (damage)
        {
            _anim["Life"].Play("Damage");
            life--;
        }
        else
        {
            _anim["Life"].Play("Recover");
            life++;
        }


        if (life > 99)
        {
            life = 99;
        }
        else if (life < 0)
        {
            life = 0;
        }
     lifeViewer.text = life.ToString();
        return life == 0;
    }


    /// <summary>
    /// 死亡イベント
    /// やり直し時にスコア消費
    /// 最後にセーブしたところからやり直し
    /// ダメージでスコア減ってるから死のスコア減少いらない
    /// いや、ひとつ前のセグメントに戻すか
    /// </summary>
    public async UniTaskVoid Die()
    {
        isDie = true;
        Score = Score - 1100 < 0 ? 0 : Score - 1100;
        life = maxLife;

        //ここでレベルマネージャーでひとつ前のセグメント…というか今のセグメントの一番下１０あたりに戻る
        //一瞬暗転させるか
        LevelManager.instance.Resporn();

        AllUpdate();

        await Player.GetComponent<CharacterController>().DieRecover();
        isDie = false;
        Debug.Log("ああああ");
    }



    /// <summary>
    /// カメラをエンディング仕様にしてイベントシーン開始
    /// </summary>
    public void EndingStart()
    {
        inGame = false;
        ending.Play();
        _camera.AdjustCameraTargetInfluence(_camera.GetCameraTarget(Player.transform), 0, 0, 0f);
        _camera.AdjustCameraTargetInfluence(_camera.GetCameraTarget(home.transform),0,1,0.05f);
        EndingWait().Forget();
    }

    /// <summary>
    /// タイムライン終わったらスコア表示するぞ
    /// </summary>
    /// <returns></returns>
    public async UniTaskVoid EndingWait()
    {        
        Debug.Log("エフェクト");
        await UniTask.WaitUntil(() => ending.time >= ending.duration,cancellationToken: destroyCancellationToken);
        ScoreDisplay();

    }



    #endregion






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

    public void PlaySound(string sType, float volumePercentage = 1f, float? pitch = null, float delaySoundTime = 0f, string variationName = null, double? timeToSchedulePlay = null, bool isRemember = false)
    {
        // Debug.Log("ｄｄｄ");
        if (isRemember)
        {
            MasterAudio.PlaySound3DAtVector3(sType, PlayerPosi, volumePercentage, pitch, delaySoundTime, variationName, timeToSchedulePlay);
        }
        else
        {
            MasterAudio.PlaySound3DAtVector3AndForget(sType, PlayerPosi, volumePercentage, pitch, delaySoundTime, variationName, timeToSchedulePlay);

        }

    }
    /// <summary>
    /// 最後のtrueにしないならフェードが基本
    /// </summary>
    /// <param name="soundGroupName"></param>
    /// <param name="fadeTime"></param>
    /// <param name="isStop"></param>
    public void StopSound(string soundGroupName)
    {

            MasterAudio.StopAllOfSound(soundGroupName);

    }

    /// <summary>
    /// セーブ機能
    /// 必ずしも変数いらんね、文字列キーで呼び出して使えばいいし
    /// </summary>
    public override void Save()
    {
        //今の状態と使用エフェクトと今の位置を格納
        ES3.Save<int>("PlayTime", lastTime);
        ES3.Save<int>("Score", Score);
        ES3.Save("Life", life);



        //ロード時に必要な処置はスコアやらのUIへの反映とメモリーの復帰だけ
    }


    public override void Load()
    {
        lastTime = ES3.Load<int>("PlayTime");
        playTime = lastTime;
        Score = ES3.Load<int>("Score");
         life = ES3.Load<int>("Life");


        bestScore = ES3.Load<int>("BestScore",0);

        AllUpdate();
    }


}
