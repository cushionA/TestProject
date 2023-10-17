using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventObject : MonoBehaviour
{

    #region　定義
    /// <summary>
    ///  イベントの種類を記述
    ///  ダメージなど
    /// </summary>
    public enum EventType
    {
        none,//何もない状態
        reflect,
        damage,
        recover,
        scoreGet,
        Random,

        badSight,//ここから状態異常
        
        invincible,//ここから有利効果
        boostJump
    }


    [Serializable]
    public struct EventData
    {
        /// <summary>
        /// イベントのタイプ
        /// </summary>
        public EventType type;

        /// <summary>
        /// 効果時間
        /// または加算するスコアなどの格納用
        /// </summary>
         public float effectTime;

        /// <summary>
        /// ぶつかった後の挙動を示す
        /// 真なら壊れる
        /// </summary>
        public bool _contactBreake;

        /// <summary>
        /// 時間計測用
        /// </summary>
        public float timer;

        public bool bad;
    }

    /// <summary>
    /// リスポーンというか判定復活に要する時間
    /// </summary>
    public float respornTime;

    #endregion


    /// <summary>
    /// オブジェクトとしてのID
    /// 壊れるやつだけに番号を振る
    /// </summary>
    [SerializeField]
    int myId;


    [SerializeField]
    EventData myEvent;

    [SerializeField]
    Collider2D _col;

    /// <summary>
    /// 当たり判定消えてる
    /// </summary>
    bool isDisenable;

    /// <summary>
    /// 当たり判定消えた時間d
    /// </summary>
    float disenaTime;

    /// <summary>
    /// 所属してるマップ番号
    /// </summary>
    [SerializeField]
    int mapNum;

    private void Start()
    {
     //   Debug.Log($"{myId}だ");
        //すでに存在しないなら壊す
        if (myEvent._contactBreake && LevelManager.instance.BreakCheck(myId,mapNum))
        {
            Debug.Log("あｍなあああ");
            Destroy(this.gameObject);
        }
    }



    // Update is called once per frame
    void Update()
    {
        if (isDisenable && disenaTime != 0)
        {
            if (Time.time - disenaTime > respornTime + 0.5f)
            {
                disenaTime = 0;
                isDisenable = false;

            }

        }
    }


    public EventData EventStart()
    {

        CollideEffect();
        return myEvent;
    }


    void BreakObj()
    { 
        LevelManager.instance.ObjectBreak(myId,mapNum);
        Destroy(this.gameObject);

    }


    void CollideEffect()
    {
        if (myEvent._contactBreake)
        {
            BreakObj();
        }
        else
        {
            isDisenable = true;
   
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (isDisenable)
        {
            disenaTime = Time.time;
            
        }
    }


    /// <summary>
    /// エディター上でIDを設定するやつ
    /// </summary>
    public void SetID(int ID,int mapNumber)
    {
        myId = ID;
        mapNum = mapNumber;
    }

    /// <summary>
    /// エディター上でIDを設定するやつ
    /// </summary>
    public int GetID()
    {
        return myId;
    }



    /// <summary>
    /// 真なら壊れるやつ
    /// </summary>
    /// <returns></returns>
    public bool BreakableCheck()
    {
        return myEvent._contactBreake;
    }

}
