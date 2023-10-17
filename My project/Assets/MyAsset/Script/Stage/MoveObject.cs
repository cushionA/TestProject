using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


public class MoveObject : MonoBehaviour
{

    public enum MoveType
    {
        Vertical,
        Horizontal,
        FreeMove,
        LinerMove
    }



    public float moveLimit = 1;
    public float moveSpeed = 1;
    /// <summary>
    /// 待つ秒数
    /// </summary>
    public float waitSeconds;

    public MoveType _type;
    public Transform pointsRoot;

    public bool isConnect;


    private bool _isForward;

    [SerializeField]
    private List<Vector2> _movePoints = new List<Vector2>();

    private float firstPosi;
    private int currentTarget;
    private Vector2 moveVector;

    private Rigidbody2D rb;
    private float nowRate;
    //曲がるのに必要な時間
    //距離を速度で割った三分の一
    private float needSeconds;

    bool isStop;

    private void Start()
    {

        rb = GetComponent<Rigidbody2D>();
        if (_type == MoveType.Vertical)
        {
            moveVector.Set(0,moveSpeed);
            firstPosi = transform.position.y;
        }
        else if (_type == MoveType.Horizontal)
        {
            moveVector.Set(moveSpeed,0);
            firstPosi = transform.position.x;
        }
        else
        {
            CheckPoints();
        }



    }

    private void Update()
    {

        if (isStop)
        {
            return;
        }

        if (_type == MoveType.Vertical)
        {
            MoveVertical();
        }
        else if (_type == MoveType.Horizontal)
        {
            MoveHorizontal();
        }
        else if (_type == MoveType.FreeMove)
        {
            MoveFree();
        }
        else if (_type == MoveType.LinerMove)
        {
            MoveLiner();
        }
    }

    private async void MoveVertical()
    {
        float posi = transform.position.y;

        if (posi < firstPosi + moveLimit && !_isForward)
        {
            rb.velocity = moveVector;
        }
        else if (posi > firstPosi && _isForward)
        {
            rb.velocity = moveVector;
        }
        else
        {



            //到達タイミングで待つよ
            if (waitSeconds > 0)
            {
                try
                {
                    isStop = true;
                    rb.velocity = Vector2.zero;
                    await UniTask.Delay(TimeSpan.FromSeconds(waitSeconds), cancellationToken: destroyCancellationToken);
                    isStop = false;
                }
                catch
                {
                    return;
                }
            }

            if (_isForward)
            {
                transform.position = new Vector2(transform.position.x,firstPosi + 0.1f);
                moveVector.Set(moveSpeed, 0);
                _isForward = false;
            }
            else
            {
                transform.position = new Vector2( transform.position.x,firstPosi + moveLimit - 0.1f);
                moveVector.Set(-moveSpeed, 0);
                _isForward = true;
            }
        }
    }

    private async void MoveHorizontal()
    {
        float posi = transform.position.x;

//        Debug.Log("ssdd{}");

        if (_isForward)
        {
            if (moveVector.x > 0)
            {
  Debug.Log($"dadaaad");
            }
        }

        if (posi < firstPosi + moveLimit && !_isForward)
        {

            rb.velocity = moveVector;
        }
        else if (posi > firstPosi && _isForward)
        {
            rb.velocity = moveVector;
        }
        else
        {
            //到達タイミングで待つよ
            if (waitSeconds > 0)
            {
                try
                {
                    isStop = true;
                    rb.velocity = Vector2.zero;
                    await UniTask.Delay(TimeSpan.FromSeconds(waitSeconds), cancellationToken: destroyCancellationToken);
                    isStop = false;
                }
                catch
                {
                    return;
                }

            }
            if (_isForward)
            {
                transform.position = new Vector2(firstPosi, transform.position.y);
                moveVector.Set(moveSpeed, 0);
                _isForward = false;
            }
            else
            {
                transform.position = new Vector2(firstPosi + moveLimit, transform.position.y);
                moveVector.Set(-moveSpeed, 0);
                _isForward = true;
            }


        }
    }

    private async void MoveFree()
    {

        //次の地点についたら
        if (ReachPoint(_isForward))
        {
            bool conect = false;
            //到達タイミングで待つよ
            if (waitSeconds > 0)
            {
                try
                {
                    isStop = true;
                    await UniTask.Delay(TimeSpan.FromSeconds(waitSeconds), cancellationToken: destroyCancellationToken);
                    isStop = false;
                }
                catch
                {
                    return;
                }
            }

          

            //最後についたかコネクトしてなくて最初に戻ったか
            if (currentTarget == _movePoints.Count - 1 || (currentTarget == 0 && !isConnect))
            {
                //一番目に戻る
                if (isConnect)
                {
                    currentTarget = 0;
                    conect = true;
                }
                else
                {
                    _isForward = !_isForward;
                    //状況に応じてターゲットを入れてフォワードを入れ替える
                    currentTarget = _isForward ? currentTarget - 1: currentTarget + 1;

                }
            }
            else
            {
                currentTarget = _isForward == false ? currentTarget + 1 : currentTarget - 1;
            }

            int bPosi;

            if (conect)
            {
                bPosi = _movePoints.Count - 1;
            }
            else
            {
                bPosi = _isForward == false ? currentTarget - 1 : currentTarget + 1;
            }

            //で、ここで現在地bposiから次の場へのベクトルを

            // 移動先ベクトルを取得
            moveVector = (_movePoints[currentTarget] -_movePoints[bPosi]).normalized * moveSpeed;
        }

        // RigidBody2DのVelocityを更新
        rb.velocity = moveVector;

    }


    private async void MoveLiner()
    {
        //次の地点についたら
        if (ReachPoint(_isForward))
        {
            bool conect = false;

            //到達タイミングで待つよ
            if (waitSeconds > 0)
            {
                try
                {
                    isStop = true;
                    await UniTask.Delay(TimeSpan.FromSeconds(waitSeconds), cancellationToken: destroyCancellationToken);
                    isStop = false;
                }
                catch
                {
                    return;
                }



            }

            //最後についたかコネクトしてなくて最初に戻ったか
            if (currentTarget == _movePoints.Count - 1 || (currentTarget == 0 && !isConnect))
            {
                //一番目に戻る
                if (isConnect)
                {
                    currentTarget = 0;
                    conect = true;
                }
                else
                {
                    _isForward = !_isForward;
                    //状況に応じてターゲットを入れてフォワードを入れ替える
                    currentTarget = _isForward ? currentTarget - 1 : currentTarget + 1;

                }
            }
            else
            {
                currentTarget = _isForward == false ? currentTarget + 1 : currentTarget - 1;
            }

            int bPosi;

            if(conect)
            {
                bPosi = _movePoints.Count - 1;
            }
            else
            {
                bPosi = _isForward == false ? currentTarget - 1 : currentTarget + 1;
            }

            //ここでレートリセット
            nowRate = 0;

            //で、ここで曲がるのに必要な秒数を割り出す
            needSeconds = (Vector2.Distance(_movePoints[currentTarget], _movePoints[bPosi]) / moveSpeed) * 3;


            // 移動先ベクトルを取得
            moveVector = (_movePoints[currentTarget] - _movePoints[bPosi]).normalized * moveSpeed;
        }

        Mathf.SmoothDamp(0,1,ref nowRate,needSeconds);

        //現在地と目的地へのベクターと移動方向の中間ベクター
        //これでうまくいくか？　だんだん目的地に寄せないといけなくね
        //移動先ベクトルと進行ベクトルを毎フレームまぜる？
        //混ぜる比率、０から１が緩やかに変化するように？

        // 移動先ベクトルを取得
        moveVector = (((_movePoints[currentTarget] - (Vector2)transform.position) * nowRate) + (rb.velocity.normalized * (1-nowRate)));

        // RigidBody2DのVelocityを更新
        rb.velocity = moveVector * moveSpeed;

    }

    private void CheckPoints()
    {
        int count = pointsRoot.childCount;

        for(int i = 0;i < count;i++)
        {
            _movePoints.Add(pointsRoot.GetChild(i).position);
        }
        currentTarget = 1;
    }


    private bool ReachPoint(bool isReverse)
    {

        Vector2 posi = transform.position;

        int bPoint = isReverse == false ? currentTarget - 1 : currentTarget + 1;
        
        if(bPoint < 0)
        {
            bPoint = _movePoints.Count-1;
        }

            if (_movePoints[currentTarget].x >= _movePoints[bPoint].x)
            {
                if (posi.x < _movePoints[currentTarget].x)
                {
                    return false;
                }
            }
            else
            {
                if (posi.x > _movePoints[currentTarget].x)
                {
                    return false;
                }
            }
            if (_movePoints[currentTarget].y >= _movePoints[bPoint].y)
            {
                if (posi.y >= _movePoints[currentTarget].y)
                {
                    return true;
                }

                    return false;

            }
            else
            {
                if (posi.y <= _movePoints[currentTarget].y)
                {
                    return true;
                }

                    return false;

            }
       
    }


}
