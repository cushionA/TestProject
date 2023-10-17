using Cysharp.Threading.Tasks;
using FunkyCode.Utilities;
using PathologicalGames;
using System;
using DarkTonic.MasterAudio;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Unity.Entities;
using Unity.VisualScripting;
using UnityEngine;
using Cysharp.Threading.Tasks.CompilerServices;

public class ArmyShot : MonoBehaviour
{

    //ゴールしたら撃たないように

    CancellationTokenSource cts;

    [SerializeField]
    SpawnPool myGun;

    [SerializeField]
    Transform bullet;

    [Header("銃の発射間隔")]
    [SerializeField]
    float fireInterval;

    [SerializeField]
    Transform firePoint;

 //   List<ParticleSystem> bulletList = new List<ParticleSystem>();

    bool isFirstWake;

    Vector3 EnemyPosi;
    bool isEnd;

    /// <summary>
    /// 射撃終了する高度
    /// </summary>
    float upLine;

    float downLine;

    [SerializeField]
    GameObject Up;
    [SerializeField]
    GameObject down;



    private void Start()
    { 
        upLine = Up.transform.position.y;
        downLine = down.transform.position.y;
        cts = new CancellationTokenSource();
        CheckPosi().Forget();
        isFirstWake = true;



    }
    private void OnEnable()
    {

        if (isFirstWake)
        {
            cts = new CancellationTokenSource();
            CheckPosi().Forget();
        }
    }

    private void OnDisable()
    { 
        
        cts.Cancel();
       // bulletList.Clear();

    }


    //延々と銃を撃つ
    async UniTaskVoid FireBullet(bool isFirst = false)
    {
        if (!isFirst)
        {
            //最初じゃなければ発射間隔だけ待つ
            await UniTask.Delay(TimeSpan.FromSeconds(fireInterval), cancellationToken: cts.Token);
        }

  

        //発射点をプレイヤーの方を向かせる
        EnemyPosi = ScoreManager.instance.Player.transform.position;
        MasterAudio.PlaySound("Shot1");
        firePoint.rotation = Quaternion.FromToRotation(Vector3.right, (EnemyPosi - this.transform.position).normalized);

        myGun.Spawn(bullet, firePoint.position, firePoint.rotation);

        //発射

        if (ScoreManager.instance.PlayerPosi.y < upLine && ScoreManager.instance.PlayerPosi.y > downLine)
        {
            FireBullet().Forget();
        }
        else
        {
            CheckPosi().Forget();
        }
    }

    async UniTaskVoid CheckPosi()
    {
       //範囲内にいるか
        await UniTask.WaitUntil(()=> (ScoreManager.instance.PlayerPosi.y < upLine && ScoreManager.instance.PlayerPosi.y > downLine),cancellationToken:cts.Token);
        FireBullet(true).Forget();
    }





}
