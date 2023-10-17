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

    //�S�[�������猂���Ȃ��悤��

    CancellationTokenSource cts;

    [SerializeField]
    SpawnPool myGun;

    [SerializeField]
    Transform bullet;

    [Header("�e�̔��ˊԊu")]
    [SerializeField]
    float fireInterval;

    [SerializeField]
    Transform firePoint;

 //   List<ParticleSystem> bulletList = new List<ParticleSystem>();

    bool isFirstWake;

    Vector3 EnemyPosi;
    bool isEnd;

    /// <summary>
    /// �ˌ��I�����鍂�x
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


    //���X�Əe������
    async UniTaskVoid FireBullet(bool isFirst = false)
    {
        if (!isFirst)
        {
            //�ŏ�����Ȃ���Δ��ˊԊu�����҂�
            await UniTask.Delay(TimeSpan.FromSeconds(fireInterval), cancellationToken: cts.Token);
        }

  

        //���˓_���v���C���[�̕�����������
        EnemyPosi = ScoreManager.instance.Player.transform.position;
        MasterAudio.PlaySound("Shot1");
        firePoint.rotation = Quaternion.FromToRotation(Vector3.right, (EnemyPosi - this.transform.position).normalized);

        myGun.Spawn(bullet, firePoint.position, firePoint.rotation);

        //����

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
       //�͈͓��ɂ��邩
        await UniTask.WaitUntil(()=> (ScoreManager.instance.PlayerPosi.y < upLine && ScoreManager.instance.PlayerPosi.y > downLine),cancellationToken:cts.Token);
        FireBullet(true).Forget();
    }





}
