using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Entities;
using UnityEngine;


/// <summary>
/// これはマップ起動をトリガーするオブジェクト
/// ループ処理があるけどこいつが存在してる間しか回されないから安心
/// こいつはレベルの真ん中と思しき場所に一つずつ設置する
/// 
/// ワープでトリガーすり抜ける可能性がある
/// </summary>
public class SegmentTrigger : MonoBehaviour
{

    enum PassDirection
    {
        LeftToRight,//右に通り抜ける
        RightToLeft,//右から左に通り抜ける
        DownToUp,//
        UpToDown
    }


    [Header("反復横跳び防止のための距離")]
    /// <summary>
    /// 二回目トリガーするために必要な距離
    /// そのうちデフォルト値入れるか
    /// </summary>
    public float limitDistance;


    [Header("どこからきてどこへ向かうのか")]
    /// <summary>
    /// プレイヤーがどこから侵入しどこへ向かうか
    /// </summary>
    [SerializeField]
     PassDirection direction;


    [Header("前のオブジェクト")]
    public GameObject prevSegment;

   [Header("次のオブジェクト")]
    public GameObject nextSegment;


    float myPosition;

    /// <summary>
    /// 一度通ったかどうか。
    /// 一度通ったならもう一度トリガーするには手間をかけるよ
    /// </summary>
    bool isPassed;

    /// <summary>
    /// 戻ってきたとかでプレイヤーの侵入・進行方向が反転してるかどうか
    /// </summary>
    bool isReverse;

    /// <summary>
    /// 今距離待ちをしてるかどうか
    /// </summary>
    bool isWait;

    CancellationTokenSource cts = new CancellationTokenSource();

    Transform Player;


    [Header("次のマップへの入り口かどうか")]
    /// <summary>
    /// 次のマップへの入り口になるかどうかということ
    /// また、最初のセグメントにもつける
    /// 前のマップへの入り口だから
    /// </summary>
    [SerializeField]
    [Header("他のマップに繋がるセグメントであるかどうか")]
    bool FinalSegment;

    /// <summary>
    /// 何番目のマップの何番目のセグメントかを示す
    /// レベルマネージャーが使う
    /// これによってマップ移動時にどのシーンを呼び出すかを決めたり
    /// セーブデータをロードした後度の面から始めるかなどを決めてくれる
    /// </summary>
    [SerializeField]
    [Header("何番目のマップの何番目のセグメントか")]
    Vector2Int mapPointer;

    /// <summary>
    /// ロードされて最初であるかどうか
    /// </summary>
    bool isFirst;

    
    // Start is called before the first frame update
    void Start()
    {
        myPosition = direction == PassDirection.RightToLeft || direction == PassDirection.LeftToRight ? transform.position.x : transform.position.y;
        Player = ScoreManager.instance.Player.transform;

    }

    /// <summary>
    /// 　このコントローラーでは今いるセグメントと次に行くセグメントだけを有効化してる
    /// 　だから本当に最初にロードした時だけ次のセグメントを起動しなきゃならない
    ///    なんでマネージャーに
    /// </summary>
    private void OnEnable()
    {

        //拠点間のファストトリップとかした時はその拠点、かがり火的なもんにセグメント起動させるか


        //リバース判定は距離判定より前のセグメントがあるかどうかが良くない？
        //初回起動時とそれ以外で分けるか

        //初回起動時なら
        if(LevelManager.instance.isFirst)
        {
            //directionが2以上なら縦になるんだよね
            float judgePosition = (int)direction < 2 ? Player.position.x : Player.position.y;
        if (direction == PassDirection.LeftToRight || direction == PassDirection.DownToUp)
        {
            isReverse = myPosition < judgePosition;
        }
        else
        {
            isReverse = myPosition > judgePosition;
        }

            //初回起動フラグを消す
            LevelManager.instance.isFirst = true;


            //初回起動フラグ消したうえで、次か前のセグメント起動

            if (!FinalSegment)
            {
                if (isReverse)
                {
                    prevSegment.SetActive(true);
                }
                else
                {
                    nextSegment.SetActive(true);
                }
            }
            else
            {
                //最初のセグメントで、リバースでなく前に進もうとしてるなら次のセグメントを
                //リバースなら前のマップを
                if (mapPointer.y == 0)
                {
                    if (!isReverse)
                    {
                        nextSegment.SetActive(true);
                    }
                    else
                    {
                        LevelManager.instance.LoadLevel();
                    }
                }
                //最後のセグメントで、リバースで後ろに進もうとしてるなら前のセグメントを
                //リバースじゃないなら次ののマップを
                else
                {
                    if (isReverse)
                    {
                        prevSegment.SetActive(true);
                    }
                    else
                    {
                        LevelManager.instance.LoadLevel();
                    }
                }
            }


        }

        //初回じゃないなら前のセグメントがあるかどうかとかで判断
        else
        {
            //最後のセグメントなら
            if (FinalSegment)
            {
                //最初のセグメントなら、次のセグメントがあるならリバースだよね（だって次から来たってことだもん）
                if (mapPointer.y == 0)
                {
                    isReverse = nextSegment.activeSelf;
                }

                //最後のセグメントなら、前のセグメントがないならリバースだね。次のマップから来たってことだし
                else
                {

                    isReverse = !prevSegment.activeSelf;
                }
            }
            //そうじゃないなら次のセグメントがあるならリバース。次から来たってことだし
            else if(nextSegment != null)
            {
                
                isReverse = nextSegment.activeSelf;
            }
        }




        isPassed = false;

    }

    private void OnDisable()
    {
        //待ってる時はキャンセルする
        if (isWait)
        {
            isWait = false;
            cts.Cancel();

        }
        isPassed = false;

    }



    /// <summary>
    /// レベルやセグメントを起動する
    /// </summary>
    void SetLevel()
    {
        //isReverseなら通過前に前のレベルを呼び、通過後（二回目の通過）に次のレベルを置く
        //ふつうは一回目通過で次、二回目で前

        //次のを呼ぶかのフラグ
        bool isNext = !isPassed;
        //リバースなら反転させる
        isNext = !isReverse ? isNext : !isNext;


        //トリガー通ったら今のセグメントを現在位置にする
        LevelManager.instance.DataUpdate(mapPointer);

        SegmentActive(isNext);


        //通過を反転させる
        isPassed = !isPassed; 
    }


    /// <summary>
    /// Exitにするのはトリガーに入ってから方向転換して前のセグメントに戻っていくようなのを想定して
    /// っていうかシスターさんのワープとか利用してトリガーせずに通り抜けたらどうするの？
    /// やっぱり距離でなんとかトリガーする方法考えた方がいい？
    /// いやでも距離だと不安定になるから、トリガーを奇数回くぐった後だけX座標、Y座標チェックをする感じにする？
    /// 
    /// 出た後どっち側にいるかも大事だな
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            if (!ExitCheck())
            {
                return;
            }

            //通過してないなら即座にレベル起動
            if (!isPassed)
            {
                SetLevel();
            }
            //そうじゃないなら距離待ち開始
            //あと既に距離待ち中なら解除する
            else
            {
                //距離待ち中なら終わらせる
                if (isWait)
                {
                    isWait = false;
                    cts.Cancel();
                    return;
                }
                
                WaitDistance().Forget();
            }
        }
    }

    /// <summary>
    /// 制限距離のぶん離れるのを待ってからレベル展開
    /// 反復横跳び対策
    /// </summary>
    /// <returns></returns>
    async UniTaskVoid WaitDistance()
    {
        isWait = true;
        //距離をクリアするまで待って
        await UniTask.WaitUntil(() => DistanceCheck() ,cancellationToken: cts.Token);

        //レベル展開
        SetLevel();
    }



    /// <summary>
    /// 現在のプレイヤーとの距離が条件満たしてるかを確認するやつ
    /// 距離はプラスマイナス含めて判断されるので反対側に行かない限り呼ばれない
    /// </summary>
    /// <returns></returns>
    bool DistanceCheck()
    {


        //directionが2以上なら縦になるんだよね
        float judgePosition = (int)direction < 2 ? Player.position.x : Player.position.y;



        //反転してるなら色々と変わってくる
        if (isReverse)
        {
            if (direction == PassDirection.LeftToRight || direction == PassDirection.DownToUp)
            {
                return myPosition - judgePosition > limitDistance;
            }
            else
            {
                return myPosition - judgePosition < -limitDistance;
            }

        }
        else
        {
            if (direction == PassDirection.LeftToRight || direction == PassDirection.DownToUp)
            {
                return myPosition - judgePosition < -limitDistance;

            }
            else
            {
                return myPosition - judgePosition > limitDistance;
            }
        }
    }



    /// <summary>
    /// 出た時トリガー起動方向にプレイヤーがいるかどうか
    /// </summary>
    /// <returns></returns>
    bool ExitCheck()
    {
        bool isNormal = !isPassed;

        isNormal = isReverse ? !isNormal : isNormal;

        //directionが2以上なら縦になるんだよね
        float judgePosition = (int)direction < 2 ? Player.position.x : Player.position.y;

        //反転してるなら色々と変わってくる
        if (isNormal)
        {
            if (direction == PassDirection.LeftToRight || direction == PassDirection.DownToUp)
            {
                return myPosition < judgePosition;
            }
            else
            {
                return myPosition > judgePosition;
            }
        }
        else
        {
            if (direction == PassDirection.LeftToRight || direction == PassDirection.DownToUp)
            {
                return myPosition > judgePosition;
            }
            else
            {
                return myPosition < judgePosition;
            }
        }
    }

    /// <summary>
    /// セグメントを起動する
    /// </summary>
    /// <param name="isNext"></param>
    void SegmentActive(bool isNext)
    {

        bool notActive = false;

        if (FinalSegment)
        {

            if (isNext)
            {
                //最初のセグメントで、前に進もうとしてるなら前のレベルを消す
                if (mapPointer.y == 0)
                {
                    LevelManager.instance.UnLoadLevel();
                }

                //最後のセグメントで次に行こうとするなら次のマップを
                else
                {
                    notActive = true;
                    LevelManager.instance.LoadLevel();
                }
            }
            else
            {
                //最後のセグメントで、後ろに進もうとしてるなら次のレベルを消す
                if (mapPointer.y == 0)
                {
                    LevelManager.instance.UnLoadLevel();
                }
                else
                {
                    LevelManager.instance.LoadLevel();
                    notActive = true;
                }
            }
        }

        if(!notActive)
        {
            if (isNext)
            {
                
                nextSegment.SetActive(true);
                if (prevSegment != null && prevSegment.activeSelf)
                {
                    prevSegment.SetActive(false);
                }

            }
            else
            {
                prevSegment.SetActive(true);
                if (nextSegment != null && nextSegment.activeSelf)
                {
                    nextSegment.SetActive(false);
                }
            }

        }
    }

}
