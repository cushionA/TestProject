using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventObject : MonoBehaviour
{

    #region�@��`
    /// <summary>
    ///  �C�x���g�̎�ނ��L�q
    ///  �_���[�W�Ȃ�
    /// </summary>
    public enum EventType
    {
        none,//�����Ȃ����
        reflect,
        damage,
        recover,
        scoreGet,
        Random,

        badSight,//���������Ԉُ�
        
        invincible,//��������L������
        boostJump
    }


    [Serializable]
    public struct EventData
    {
        /// <summary>
        /// �C�x���g�̃^�C�v
        /// </summary>
        public EventType type;

        /// <summary>
        /// ���ʎ���
        /// �܂��͉��Z����X�R�A�Ȃǂ̊i�[�p
        /// </summary>
         public float effectTime;

        /// <summary>
        /// �Ԃ�������̋���������
        /// �^�Ȃ����
        /// </summary>
        public bool _contactBreake;

        /// <summary>
        /// ���Ԍv���p
        /// </summary>
        public float timer;

        public bool bad;
    }

    /// <summary>
    /// ���X�|�[���Ƃ��������蕜���ɗv���鎞��
    /// </summary>
    public float respornTime;

    #endregion


    /// <summary>
    /// �I�u�W�F�N�g�Ƃ��Ă�ID
    /// ���������ɔԍ���U��
    /// </summary>
    [SerializeField]
    int myId;


    [SerializeField]
    EventData myEvent;

    [SerializeField]
    Collider2D _col;

    /// <summary>
    /// �����蔻������Ă�
    /// </summary>
    bool isDisenable;

    /// <summary>
    /// �����蔻�����������
    /// </summary>
    float disenaTime;



    private void Start()
    {
        //���łɑ��݂��Ȃ��Ȃ��
        if (myEvent._contactBreake && LevelManager.instance.BreakCheck(myId))
        {
            GameObject.Destroy(this);
        }
    }



    // Update is called once per frame
    void Update()
    {
        if (isDisenable && disenaTime != 0)
        {
            if (Time.time - disenaTime > respornTime + 0.5f)
            {
                disenaTime = 0;
                isDisenable = false;

            }

        }
    }


    public EventData EventStart()
    {
        if (myEvent._contactBreake)
        {
            BreakObj();
        }
        else
        {
            CollideEffect();
            isDisenable = true;

        }

        return myEvent;
    }


    void BreakObj()
    {
        Destroy(this.gameObject);

    }


    void CollideEffect()
    {
        if (myEvent._contactBreake)
        {
            LevelManager.instance.ObjectBreak(myId);
            Destroy(this.gameObject);
        }
        else
        {
            isDisenable = true;
   
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (isDisenable)
        {
            disenaTime = Time.time;
            
        }
    }


}