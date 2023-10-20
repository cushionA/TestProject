
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
    /// �A�j���~�߂��
    /// �����đI�����痣�ꂽ���ɉ���炷
    /// </summary>
    /// <param name="eventData"></param>
    public override void OnPointerExit(PointerEventData eventData)
    {
        if (!isEnter)
        {
            return;
        }
        isEnter = false;

        Debug.Log("��w��");
        if (_tweenAnim == null)
        {
            _tweenAnim = GetComponent<DOTweenAnimation>();
        }
        _tweenAnim.DORestartById("1");
        _tweenAnim.DOPause();

    }








    //OnClick�̓C���X�y�N�^�Őݒ�

    /// <summary>
    /// �C���X�y�N�^��������v���X���̃A�j��
    /// �����炷
    /// �I������A�j����Oncomplete�ɓ���Ă�
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
