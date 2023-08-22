using RenownedGames.Apex.Serialization.Collections.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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



    #endregion


    #region チェックポイント関連



    /// <summary>
    /// チェックポイント到達時に使用
    /// やり直し時に使う
    /// </summary>
    int memoryLife;

    /// <summary>
    /// チェックポイント到達時に使用
    /// やり直し時に使う
    /// </summary>
    int memoryScore;

    /// <summary>
    /// チェックポイント到達時に使用
    /// やり直し時に使う
    /// </summary>
    int memoryTime;

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

    /// <summary>
    /// UIのアニメーション
    /// </summary>
    [SerializeField]
    MyAnimator _anim;


    #endregion


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

    public void ScoreChange(int change)
    {
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
        if (life > 0)
        {
            lifeViewer.text = life.ToString();
        }

        return life < 0;
    }


    /// <summary>
    /// 死亡イベント
    /// やり直し時にスコア消費
    /// </summary>
    public void Die()
    {

        Score = Score - 500 < 0 ? 0 : Score - 500;
        life = maxLife;

        AllUpdate();
    }

    public void Reset()
    {
        Score -= memoryScore;
    }

    #endregion





    #region チェックポイント関連

    /// <summary>
    /// 再挑戦
    /// </summary>
    void ReTry(bool isDie)
    {
        if (!isDie)
        {
            Score = memoryScore;
            playTime = memoryTime;
        }

        life = memoryLife;
        AllUpdate();

        //ここから暗転処理

    }


    /// <summary>
    /// チェックポイント到達
    /// </summary>
    void ReachPoint(int reach)
    {
        memoryScore = Score;
        memoryTime = lastTime;
        memoryLife = life;

    }

    #endregion



    /// <summary>
    /// セーブ機能
    /// 必ずしも変数いらんね、文字列キーで呼び出して使えばいいし
    /// </summary>
    public override void Save()
    {
        //今の状態と使用エフェクトと今の位置を格納
        ES3.Save("PlayTime", playTime);
        ES3.Save("Score", Score);
        ES3.Save("Life", life);

        //記録用、やり直しに使う
        ES3.Save("MScore", memoryScore);
        ES3.Save("MLife", memoryLife);
        ES3.Save("MTime", memoryTime);

        //ロード時に必要な処置はスコアやらのUIへの反映とメモリーの復帰だけ
    }


    public override void Load()
    {
        playTime = ES3.Load<float>("PlayTime");
        Score = ES3.Load<int>("Score");
         life = ES3.Load<int>("Life");

        //記録用、やり直しに使う
        memoryScore = ES3.Load<int>("MScore");
        memoryLife = ES3.Load<int>("MLife");
        memoryTime = ES3.Load<int>("MTime");
        AllUpdate();
    }


}
