using Cysharp.Threading.Tasks;
using DarkTonic.MasterAudio;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// �����E�B���h�E
/// </summary>
public class DescriptionUI : MonoBehaviour
{
    /// <summary>
    /// ���̃��X�g
    /// </summary>
    [SerializeField]
    GameObject[] WindowList;

    [SerializeField]
    GameObject RootObj;

    int windowCount;

    /// <summary>
    /// �y�[�W�߂���\�ɂȂ�܂ł̎���
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
    /// �܂ǂ��R���g���[��
    /// </summary>
    /// <returns></returns>
    async UniTaskVoid WindowController()
    {
        //���ԑ҂�
        await UniTask.Delay(TimeSpan.FromSeconds(pageInterval), cancellationToken: destroyCancellationToken);

        //�N���b�N�����玟��
        await UniTask.WaitUntil(() => Input.GetMouseButtonDown(0), cancellationToken: destroyCancellationToken);

        WindowList[counter].SetActive(false);

        //�J�E���g���v�f���ɒǂ����Ȃ瑋������
        //���Ɖ����炷���H
        if (counter + 1 == windowCount)
        {
            MasterAudio.PlaySound("MenuCancel");
            counter = 0;
            RootObj.SetActive(false);
            return;
        }
        //�����łȂ��Ȃ玟�̑���
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
