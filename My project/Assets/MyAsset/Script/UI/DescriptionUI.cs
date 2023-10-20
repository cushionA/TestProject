using Cysharp.Threading.Tasks;
using DarkTonic.MasterAudio;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 説明ウィンドウ
/// </summary>
public class DescriptionUI : MonoBehaviour
{
    /// <summary>
    /// 窓のリスト
    /// </summary>
    [SerializeField]
    GameObject[] WindowList;

    [SerializeField]
    GameObject RootObj;

    int windowCount;

    /// <summary>
    /// ページめくり可能になるまでの時間
    /// </summary>
    [SerializeField]
    float pageInterval;

    int counter;

    // Start is called before the first frame update
    void Start()
    {
        windowCount = WindowList.Length;

    }



    /// <summary>
    /// まどをコントロール
    /// </summary>
    /// <returns></returns>
    async UniTaskVoid WindowController()
    {
        //時間待つ
        await UniTask.Delay(TimeSpan.FromSeconds(pageInterval), cancellationToken: destroyCancellationToken);

        //クリックしたら次へ
        await UniTask.WaitUntil(() => Input.GetMouseButtonDown(0), cancellationToken: destroyCancellationToken);

        WindowList[counter].SetActive(false);

        //カウントが要素数に追いつくなら窓を消す
        //あと音も鳴らすか？
        if (counter + 1 == windowCount)
        {
            MasterAudio.PlaySound("MenuCancel");
            counter = 0;
            RootObj.SetActive(false);
            return;
        }
        //そうでないなら次の窓を
        else
        {
            MasterAudio.PlaySound("ButoonMove");
            counter++;
            WindowList[counter].SetActive(true);
            WindowController().Forget();
        }


    }


    public void DescriptStart()
    {
        Debug.Log("Call");
        counter = 0;
        MasterAudio.PlaySound("Open");
        RootObj.SetActive(true);
        WindowList[0].SetActive(true);
        WindowController().Forget();
    }



}
