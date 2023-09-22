using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Unity.Entities;
using UnityEngine;


/// <summary>
/// これはマップ起動をトリガーするオブジェクト
/// ループ処理があるけどこいつが存在してる間しか回されないから安心
/// こいつはレベルの真ん中と思しき場所に一つずつ設置する
/// 
/// ワープでトリガーすり抜ける可能性がある
/// 
/// 
/// 仕様
/// 
/// 基本的にトリガーになる座標との距離でセグメントのオンオフを切り替える
/// 右から左とか上から下とか向きの違いにも対応している
/// 向きの違いで参考にするのがX座標かｙ座標かを変える
/// 
/// アクティブにするのは次のセグメントと前のセグメント
/// 
/// </summary>
public class SegmentTrigger : MonoBehaviour
{

    //改良提案

    //WaitDistanceメソッドをアクティブ時に呼んで自分との距離が近づくまで待てば当たり判定はいらないのでは



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
    /// プレイヤーがどの方向から侵入しどこへ向かうか
    /// </summary>
    [SerializeField]
    PassDirection direction;


    [Header("前のオブジェクト")]
    public GameObject prevSegment;

   [Header("次のオブジェクト")]
    public GameObject nextSegment;


    [Header("接続するマップのセグメント番号")]
    [SerializeField]
    int nextMapSegment;

    /// <summary>
    /// 自分の座標
    /// LeftTO　みたいな横ルートならX座標入れるし
    /// 縦ならその逆
    /// </summary>
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
    [Header("何番目のマップの何番目のセグメントか。0から数える")]
    Vector2Int mapPointer;




    /// <summary>
    /// トリガーになる位置を示すオブジェクト
    /// それがなければ自分の位置をね
    /// </summary>
    [SerializeField]
    [Header("トリガーになる位置はどこ")]
    GameObject trigger;

    
    // Start is called before the first frame update
    void Awake()
    {
        //自分の他にトリガーがあるならそれを使おうね
        Transform position = trigger != null ? trigger.transform : this.transform;

        //自分の座標の上下座標を参考にするか決める。縦侵入ルートならｙ座標、横侵入ルートならｘ座標
        myPosition = direction == PassDirection.RightToLeft || direction == PassDirection.LeftToRight ? position.position.x : position.position.y;

        //プレイヤーのトランスフォーム
        Player = ScoreManager.instance.Player.transform;

    }

    /// <summary>
    /// 
    /// isFirstフラグを利用し初回起動時の設定。最後のセグメントにいるならマップも呼び出すし
    /// 　このコントローラーでは今いるセグメントと次に行くセグメントだけを有効化してる
    /// 　だから本当に最初にロードした時だけ次のセグメントを起動しなきゃならない
    ///    だからマネージャーにたのもう(FirstSegmentActive)
    /// </summary>
    private void OnEnable()
    {


        if (this.gameObject.name == "SecondMapSegment0")
        {
            Debug.Log("しぬ");
        }

        //拠点間のファストトリップとかした時はその拠点、かがり火的なもんにセグメント起動させるか


        //リバース判定は距離判定より前のセグメントがあるかどうかが良くない？
        //初回起動時とそれ以外で分けるか

        //初回起動時なら
        if (!LevelManager.instance.isFirst)
        {
            Debug.Log($"hhhh{isReverse}{LevelManager.instance.isFirst}");
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
         Debug.Log($"だだだ{isReverse}{LevelManager.instance.isFirst}");
            //初回起動フラグを消す
            LevelManager.instance.isFirst = true;

            Debug.Log($"dedede{isReverse}{LevelManager.instance.isFirst}");
            //初回起動フラグ消したうえで、次か前のセグメント起動

            if (!FinalSegment)
            {

                Debug.Log($"だggg{isReverse}{LevelManager.instance.isFirst}");
                if (isReverse)
                {
                    nextSegment.SetActive(true);
                }
                else
                {
                    prevSegment.SetActive(true);
                }
            }
            else
            {
                Debug.Log("あddsdssss");

                //最初のセグメントで、リバースでなく前に進もうとしてるなら次のセグメントを
                //リバースなら前のマップを
                if (prevSegment == null)
                {
                    if (!isReverse)
                    {
                        nextSegment.SetActive(true);
                    }
                    else
                    {
                        LevelManager.instance.LoadLevel(nextMapSegment).Forget();
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
                        Debug.Log("あいいい");
                        LevelManager.instance.LoadLevel(nextMapSegment).Forget();
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
        TriggerStart().Forget();
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
    /// こいつを最初に呼ぶことで判定できる
    /// こいつを読んだら次に反復横跳び禁止のDistanceWaitを呼ぶ
    /// </summary>
    /// <returns></returns>
    async UniTaskVoid  TriggerStart()
    {
        isWait = true;
        //距離をクリアするまで待って
        await UniTask.WaitUntil(() => TriggerCheck(), cancellationToken: cts.Token);


        //レベル展開
        SetLevel();
        WaitDistance().Forget();
    }







    #region 状況判断関連

    /// <summary>
    /// トリガー起動方向にプレイヤーがいるかどうか
    /// つまりは左から右のセグメントだったら、トリガーに接触したとしても土地が―オブジェクトの右にいてくれないとダメ
    /// 
    /// 使わないかも
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
    /// 現在のプレイヤーとの距離が条件満たしてるかを確認するやつ
    /// 距離はプラスマイナス含めて判断されるので反対側に行かない限り呼ばれない
    /// </summary>
    /// <returns></returns>
    bool DistanceCheck()
    {

        //現在のセグメントじゃないのなら引っ込んでね
        if (Mathf.Abs(LevelManager.instance.GetSegment() - mapPointer.y) > 1)
        {
            return false;
        }

        bool isNext = !isPassed;

        isNext = isReverse ? !isNext : isNext;

        //directionが2以上なら縦になるんだよね
        float judgePosition = (int)direction < 2 ? Player.position.x : Player.position.y;


        //100圏内でトリガー起動するから、100圏外で起動するようにしないと連続で壊れる

        //反転してるなら色々と変わってくる
        if (!isNext)
        {
            //逆だから下にいないといけない
            if (direction == PassDirection.LeftToRight || direction == PassDirection.DownToUp)
            {
                return myPosition - judgePosition > limitDistance + 100;
            }
            else
            {
                return myPosition - judgePosition < -(limitDistance + 100);
            }

        }
        else
        {
            //上に行ってる時、右に行ってる時y
            if (direction == PassDirection.LeftToRight || direction == PassDirection.DownToUp)
            {
                return myPosition - judgePosition < -(limitDistance + 100);

            }
            else
            {
                return myPosition - judgePosition > limitDistance + 100;
            }
        }

    }

    /// <summary>
    /// 現在のプレイヤーとの距離がトリガー範囲かを確認するやつ
    /// </summary>
    /// <returns></returns>
    bool TriggerCheck()
    {
        //番号でロック欠けるのまずいな
        //この先で番号変形してるわけだし
        //番号の誤差が一ついないってことにしようか
      //  Debug.Log($"距離{(myPosition - ((int)direction < 2 ? Player.position.x : Player.position.y)) < 100} 番号{LevelManager.instance.GetSegment() == mapPointer.y}");
        //現在のセグメントじゃないのなら引っ込んでね
        if (Mathf.Abs(LevelManager.instance.GetSegment() - mapPointer.y) > 1)
        {
            return false;
        }

        //directionが2以上なら縦になるんだよね
        float judgePosition = (int)direction < 2 ? Player.position.x : Player.position.y;


        if (mapPointer.y == 1)
        {
        if (Mathf.Abs(myPosition - judgePosition) < 100)
        {
            Debug.Log($"距離到達{mapPointer.y}");
        }
        }



        //距離がちゃんとトリガー範囲内なら
        return Mathf.Abs(myPosition - judgePosition) < 100;
    }




    //思ったんだがこのWaitDistanceをセグメントがアクティブにンなった瞬間呼んでやれば当たり判定ではなく距離チェックできるのでは


    /// <summary>
    /// 制限距離のぶん離れるのを待ってからレベル展開
    /// 反復横跳び対策
    /// 
    /// トリガーを通った後、ある一定距離さらに進まないと起動しない
    /// </summary>
    /// <returns></returns>
    async UniTaskVoid WaitDistance()
    {
        isWait = true;
        //距離をクリアするまで待って
        await UniTask.WaitUntil(() => DistanceCheck(), cancellationToken: cts.Token);
        if (mapPointer.y == 1)
        {
        Debug.Log($"dis距離到達{mapPointer.y}");
        }


        //レベル展開
        SetLevel();
        TriggerStart().Forget();
    }




    #endregion



    #region マップ、セグメント起動関連


    /// <summary>
    /// レベルやセグメントを起動する
    /// そしてフラグも管理し、レべマネにも現在のマップの番号を送信する
    /// 番号は現在のマップで、通過したトリガーのセグメント番号を採用
    /// 具体的な判断（マップ起動化セグメントかなど）はSegmentActive（）に委託
    /// </summary>
    void SetLevel()
    {
        //isReverseなら通過前に前のレベルを呼び、通過後（二回目の通過）に次のレベルを置く
        //ふつうは一回目通過で次、二回目で前

        //次のを呼ぶかのフラグ
        bool isNext = !isPassed;
        //リバースなら反転させる
        isNext = !isReverse ? isNext : !isNext;

        if (mapPointer.y == 1)
        {
            Debug.Log($"逆？{isReverse}通過済み？{isPassed}");
        }
        //トリガー通ったら今のセグメントを現在位置にする
        LevelManager.instance.DataUpdate(mapPointer);

        SegmentActive(isNext);


        //通過を反転させる
        isPassed = !isPassed;
    }




    /// <summary>
    /// 移動時に次に向かうセグメントを起動する
    /// 最後のセグメントなら次のマップを呼び出す
    /// 
    /// マップ呼び出した後はセグメントを有効化しない
    /// 呼び出したマップのセグメントを有効化する役目は通路に持たせてもいいかも
    /// あと通路には見えない壁を置いておいて、マップのロードが終わったら消すようにしてもいいかも
    /// </summary>
    /// <param name="isNext">IsNextは次の場所に行こうとしてるか前にもどるか</param>
    void SegmentActive(bool isNext)
    {

        //今回の処理でアクティブにすべきセグメントがあるかどうか
        //前のマップ消したりした場合は次に向かうセグメントをつけてあげないとね
        //逆にマップ呼び出したりしたらセグメントを有効化する意味はあらず
        bool notActive = false;


        //最後のセグメントでは次のマップ呼んだりする
        if (FinalSegment)
        {

            //ここで最後のセグメントから前に行こうとしてるか次に行こうとしてるかを判断
            //マップを消すべきか呼ぶべきかを判断するの
            if (isNext)
            {
                //最初のセグメントで、前に進もうとしてるなら前のレベルを消す
                //前のセグメントがないなら最初なんだよね
                if (prevSegment == null)
                {
                    LevelManager.instance.UnLoadLevel();
                }

                //最後のセグメントで次に行こうとするなら次のマップを呼び出す
                else
                {
                    notActive = true;
                    LevelManager.instance.LoadLevel(nextMapSegment).Forget();
                }
            }
            else
            {
                //最後のセグメントで、前に戻ろうとしてるなら次のレベルを消す
                //次がないなら最初なんだ
                if (nextSegment == null)
                {
                    LevelManager.instance.UnLoadLevel();
                }
                else
                {
                    LevelManager.instance.LoadLevel(nextMapSegment).Forget();
                    notActive = true;
                }
            }
        }

        //有効化すべきセグメントがあるなら
        if(!notActive)
        {
            if (isNext)
            {
                if (nextSegment != null)
                {
                    nextSegment.SetActive(true);
                }
                if (prevSegment != null && prevSegment.activeSelf)
                {
                    prevSegment.SetActive(false);
                }

            }
            else
            {
                if (prevSegment != null)
                {
                    prevSegment.SetActive(true);
                }
                if (nextSegment != null && nextSegment.activeSelf)
                {
                    nextSegment.SetActive(false);
                }
            }

        }
    }

    #endregion


}
