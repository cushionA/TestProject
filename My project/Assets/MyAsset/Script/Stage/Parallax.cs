using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    private float length;
    float player;


    /// <summary>
    /// �񖇖ڂ̉摜
    /// </summary>
    public Transform secondBack;
    /// <summary>
    /// �O���ڂ̉摜
    /// </summary>
    public Transform thirdBack;

    /// <summary>
    /// ���̉摜�̒����̉��{�܂ŗ���Ă�������
    /// </summary>
    public float multi;

    Vector3 posi;


    bool isUp;
    bool isDown;

    Vector3 nowMain;

    void Start()
    {

        // �w�i�摜��x�������̕�
        length = GetComponent<SpriteRenderer>().bounds.size.y;
        StartPosi();
    }

    private void FixedUpdate()
    {
        player = ScoreManager.instance.PlayerPosi.y;

        //�܂������摜�̒������Ȃ�
        if (Mathf.Abs(player - nowMain.y) < length*multi)
        {
            //�v���C���[����������ɂ���Ȃ�c��񖇂̉摜�����
            if (player > nowMain.y && !isUp)
            {

                //�������ꖇ����ɒu������
                posi.Set(nowMain.x,nowMain.y + length,nowMain.z);
                secondBack.position = posi;

                //�������ꖇ�����ɒu������
                posi.Set(nowMain.x, nowMain.y + length * 2, nowMain.z);
                thirdBack.position = posi;

                //��ɒu������[
                isUp = true;
                isDown = false;
            }
            if (player < nowMain.y && !isDown)
            {
                //�������ꖇ�����ɒu������
                posi.Set(nowMain.x, nowMain.y - length, nowMain.z);
                secondBack.position = posi;
                //�������ꖇ�����ɒu������
                posi.Set(nowMain.x, nowMain.y - length * 2, nowMain.z);
                thirdBack.position = posi;
                isUp = false;
                isDown = true;
            }

        }
        //�����ɒ�����multi�{�̋���������߂�����񖇖ڂ̈ʒu�Ɏ�����u��
        else
        {
            //�����͓񖇖ڂ������ʒu��
            nowMain.Set(secondBack.position.x, secondBack.position.y, secondBack.position.z);
            this.transform.position = nowMain;
           


            //��ɏd�˂�p�^�[���Ȃ�
            if (isUp)
            {

                //�������ꖇ����ɒu������
                posi.Set(nowMain.x, nowMain.y + length, nowMain.z);
                secondBack.position = posi;

                //�������񖇕���ɒu������
                posi.Set(nowMain.x, nowMain.y + length * 2, nowMain.z);
                thirdBack.position = posi;

                //��ɒu������[
                isUp = true;
                isDown = false;
            }
            if (isDown)
            {
                //�������ꖇ�����ɒu������
                posi.Set(nowMain.x, nowMain.y - length, nowMain.z);
                secondBack.position = posi;
                //�������񖇕����ɒu������
                posi.Set(nowMain.x, nowMain.y - length * 2, nowMain.z);
                thirdBack.position = posi;
                isUp = false;
                isDown = true;
            }
        }


    }

    void StartPosi()
    {
        player = ScoreManager.instance.PlayerPosi.y;

        //0�Ȃ�Ȃ�����Ȃ�
        if (player == 0)
        {
            nowMain.Set(0, 0, 0);
            return;
        }

        //���݂̃v���C���[�̈ʒu�𒴂��Ȃ����x��Length��n�{�̈ʒu��
        nowMain.Set(0, (player / length) * length, 0);

        //�v���C���[�̍����̂��܂肪�����̒����̔�������ɂ���Ȃ�
        //�񖇖ڎO���ڂ���Ȃ̂ł��̂܂܂�
        if ((player % length) >= length / 2)
        {
            isUp = true;
            isDown = false;
        }
        //�����łȂ��Ȃ�񖇖ڎO���ڂ�����
        else
        {
            //�������ꖇ�����ɒu������
            posi.Set(nowMain.x, nowMain.y - length, nowMain.z);
            secondBack.position = posi;
            //�������񖇕����ɒu������
            posi.Set(nowMain.x, nowMain.y - length * 2, nowMain.z);
            thirdBack.position = posi;

            isUp = false;
            isDown = true;
        }

    }
}
