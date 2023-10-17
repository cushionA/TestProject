using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    private float length;
    float player;


    /// <summary>
    /// 二枚目の画像
    /// </summary>
    public Transform secondBack;
    /// <summary>
    /// 三枚目の画像
    /// </summary>
    public Transform thirdBack;

    /// <summary>
    /// この画像の長さの何倍まで離れてもいいか
    /// </summary>
    public float multi;

    Vector3 posi;


    bool isUp;
    bool isDown;

    Vector3 nowMain;

    void Start()
    {

        // 背景画像のx軸方向の幅
        length = GetComponent<SpriteRenderer>().bounds.size.y;
        StartPosi();
    }

    private void FixedUpdate()
    {
        player = ScoreManager.instance.PlayerPosi.y;

        //まだ差が画像の長さ内なら
        if (Mathf.Abs(player - nowMain.y) < length*multi)
        {
            //プレイヤーが自分より上にいるなら残り二枚の画像を上に
            if (player > nowMain.y && !isUp)
            {

                //自分より一枚分上に置く処理
                posi.Set(nowMain.x,nowMain.y + length,nowMain.z);
                secondBack.position = posi;

                //自分より一枚分下に置く処理
                posi.Set(nowMain.x, nowMain.y + length * 2, nowMain.z);
                thirdBack.position = posi;

                //上に置いたよー
                isUp = true;
                isDown = false;
            }
            if (player < nowMain.y && !isDown)
            {
                //自分より一枚分下に置く処理
                posi.Set(nowMain.x, nowMain.y - length, nowMain.z);
                secondBack.position = posi;
                //自分より一枚分下に置く処理
                posi.Set(nowMain.x, nowMain.y - length * 2, nowMain.z);
                thirdBack.position = posi;
                isUp = false;
                isDown = true;
            }

        }
        //自分に長さのmulti倍の距離が離れ過ぎたら二枚目の位置に自分を置く
        else
        {
            //自分は二枚目がいた位置へ
            nowMain.Set(secondBack.position.x, secondBack.position.y, secondBack.position.z);
            this.transform.position = nowMain;
           


            //上に重ねるパターンなら
            if (isUp)
            {

                //自分より一枚分上に置く処理
                posi.Set(nowMain.x, nowMain.y + length, nowMain.z);
                secondBack.position = posi;

                //自分より二枚分上に置く処理
                posi.Set(nowMain.x, nowMain.y + length * 2, nowMain.z);
                thirdBack.position = posi;

                //上に置いたよー
                isUp = true;
                isDown = false;
            }
            if (isDown)
            {
                //自分より一枚分下に置く処理
                posi.Set(nowMain.x, nowMain.y - length, nowMain.z);
                secondBack.position = posi;
                //自分より二枚分下に置く処理
                posi.Set(nowMain.x, nowMain.y - length * 2, nowMain.z);
                thirdBack.position = posi;
                isUp = false;
                isDown = true;
            }
        }


    }

    void StartPosi()
    {
        player = ScoreManager.instance.PlayerPosi.y;

        //0ならなんもしない
        if (player == 0)
        {
            nowMain.Set(0, 0, 0);
            return;
        }

        //現在のプレイヤーの位置を超えない程度のLengthのn倍の位置に
        nowMain.Set(0, (player / length) * length, 0);

        //プレイヤーの高さのあまりが自分の長さの半分より上にあるなら
        //二枚目三枚目も上なのでそのままね
        if ((player % length) >= length / 2)
        {
            isUp = true;
            isDown = false;
        }
        //そうでないなら二枚目三枚目を下に
        else
        {
            //自分より一枚分下に置く処理
            posi.Set(nowMain.x, nowMain.y - length, nowMain.z);
            secondBack.position = posi;
            //自分より二枚分下に置く処理
            posi.Set(nowMain.x, nowMain.y - length * 2, nowMain.z);
            thirdBack.position = posi;

            isUp = false;
            isDown = true;
        }

    }
}
