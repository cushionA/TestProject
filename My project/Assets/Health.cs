using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{


    /// <summary>
    /// 初期残機数
    /// チェックポイントでここまで回復？
    /// いやそれはいらんな。スコア支払いで回復はあり
    /// </summary>
    public float maxLife;

    /// <summary>
    /// 体力、残機
    /// </summary>
    float life;

    /// <summary>
    /// チェックポイント到達時に使用
    /// やり直し時に使う
    /// </summary>
    float memoryLife;

    private void Start()
    {
        life = maxLife;
    }

    public void LifeAdd()
    {
        life++;
    }

    public void LifeDamage()
    {
        life--;

        if(life < 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 死亡イベント
    /// やり直し時にスコア消費
    /// </summary>
    void Die()
    {

    }


}
