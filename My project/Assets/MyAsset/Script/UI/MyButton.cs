
using DarkTonic.MasterAudio;
using DG.Tweening;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MyButton : Button,IPointerEnterHandler,IPointerExitHandler
{
    [SerializeField]
    public DOTweenAnimation _tweenAnim;

    bool isEnter;


    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (isEnter)
        {
            return;
        }
        isEnter = true;
        if (_tweenAnim == null)
        {
            _tweenAnim = GetComponent<DOTweenAnimation>();
        }

        MasterAudio.PlaySound("ButoonMove");
        _tweenAnim.DOPlayById("1");
    }



    /// <summary>
    /// アニメ止めるよ
    /// そして選択から離れた時に音を鳴らす
    /// </summary>
    /// <param name="eventData"></param>
    public override void OnPointerExit(PointerEventData eventData)
    {
        if (!isEnter)
        {
            return;
        }
        isEnter = false;

        Debug.Log("ｄwｄ");
        if (_tweenAnim == null)
        {
            _tweenAnim = GetComponent<DOTweenAnimation>();
        }
        _tweenAnim.DORestartById("1");
        _tweenAnim.DOPause();

    }








    //OnClickはインスペクタで設定

    /// <summary>
    /// インスペクタから入れるプレス時のアニメ
    /// 音も鳴らす
    /// 終わったアニメはOncompleteに入れてね
    /// </summary>
    public void ButtonPush()
    {
        if (_tweenAnim == null)
        {
            _tweenAnim = GetComponent<DOTweenAnimation>();
        }

        MasterAudio.PlaySound("Open");
        _tweenAnim.DOPlayById("2");

    }

}
