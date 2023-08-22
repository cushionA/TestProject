using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Entities;
using UnityEngine;


/// <summary>
/// ����̓}�b�v�N�����g���K�[����I�u�W�F�N�g
/// ���[�v���������邯�ǂ��������݂��Ă�Ԃ����񂳂�Ȃ�������S
/// �����̓��x���̐^�񒆂Ǝv�����ꏊ�Ɉ���ݒu����
/// 
/// ���[�v�Ńg���K�[���蔲����\��������
/// </summary>
public class SegmentTrigger : MonoBehaviour
{

    enum PassDirection
    {
        LeftToRight,//�E�ɒʂ蔲����
        RightToLeft,//�E���獶�ɒʂ蔲����
        DownToUp,//
        UpToDown
    }


    [Header("���������іh�~�̂��߂̋���")]
    /// <summary>
    /// ���ڃg���K�[���邽�߂ɕK�v�ȋ���
    /// ���̂����f�t�H���g�l����邩
    /// </summary>
    public float limitDistance;


    [Header("�ǂ����炫�Ăǂ��֌������̂�")]
    /// <summary>
    /// �v���C���[���ǂ�����N�����ǂ��֌�������
    /// </summary>
    [SerializeField]
     PassDirection direction;


    [Header("�O�̃I�u�W�F�N�g")]
    public GameObject prevSegment;

   [Header("���̃I�u�W�F�N�g")]
    public GameObject nextSegment;


    float myPosition;

    /// <summary>
    /// ��x�ʂ������ǂ����B
    /// ��x�ʂ����Ȃ������x�g���K�[����ɂ͎�Ԃ��������
    /// </summary>
    bool isPassed;

    /// <summary>
    /// �߂��Ă����Ƃ��Ńv���C���[�̐N���E�i�s���������]���Ă邩�ǂ���
    /// </summary>
    bool isReverse;

    /// <summary>
    /// �������҂������Ă邩�ǂ���
    /// </summary>
    bool isWait;

    CancellationTokenSource cts = new CancellationTokenSource();

    Transform Player;


    [Header("���̃}�b�v�ւ̓�������ǂ���")]
    /// <summary>
    /// ���̃}�b�v�ւ̓�����ɂȂ邩�ǂ����Ƃ�������
    /// �܂��A�ŏ��̃Z�O�����g�ɂ�����
    /// �O�̃}�b�v�ւ̓����������
    /// </summary>
    [SerializeField]
    [Header("���̃}�b�v�Ɍq����Z�O�����g�ł��邩�ǂ���")]
    bool FinalSegment;

    /// <summary>
    /// ���Ԗڂ̃}�b�v�̉��Ԗڂ̃Z�O�����g��������
    /// ���x���}�l�[�W���[���g��
    /// ����ɂ���ă}�b�v�ړ����ɂǂ̃V�[�����Ăяo���������߂���
    /// �Z�[�u�f�[�^�����[�h������x�̖ʂ���n�߂邩�Ȃǂ����߂Ă����
    /// </summary>
    [SerializeField]
    [Header("���Ԗڂ̃}�b�v�̉��Ԗڂ̃Z�O�����g��")]
    Vector2Int mapPointer;

    /// <summary>
    /// ���[�h����čŏ��ł��邩�ǂ���
    /// </summary>
    bool isFirst;

    
    // Start is called before the first frame update
    void Start()
    {
        myPosition = direction == PassDirection.RightToLeft || direction == PassDirection.LeftToRight ? transform.position.x : transform.position.y;
        Player = ScoreManager.instance.Player.transform;

    }

    /// <summary>
    /// �@���̃R���g���[���[�ł͍�����Z�O�����g�Ǝ��ɍs���Z�O�����g������L�������Ă�
    /// �@������{���ɍŏ��Ƀ��[�h�������������̃Z�O�����g���N�����Ȃ���Ȃ�Ȃ�
    ///    �Ȃ�Ń}�l�[�W���[��
    /// </summary>
    private void OnEnable()
    {

        //���_�Ԃ̃t�@�X�g�g���b�v�Ƃ��������͂��̋��_�A������ΓI�Ȃ���ɃZ�O�����g�N�������邩


        //���o�[�X����͋���������O�̃Z�O�����g�����邩�ǂ������ǂ��Ȃ��H
        //����N�����Ƃ���ȊO�ŕ����邩

        //����N�����Ȃ�
        if(LevelManager.instance.isFirst)
        {
            //direction��2�ȏ�Ȃ�c�ɂȂ�񂾂��
            float judgePosition = (int)direction < 2 ? Player.position.x : Player.position.y;
        if (direction == PassDirection.LeftToRight || direction == PassDirection.DownToUp)
        {
            isReverse = myPosition < judgePosition;
        }
        else
        {
            isReverse = myPosition > judgePosition;
        }

            //����N���t���O������
            LevelManager.instance.isFirst = true;


            //����N���t���O�����������ŁA�����O�̃Z�O�����g�N��

            if (!FinalSegment)
            {
                if (isReverse)
                {
                    prevSegment.SetActive(true);
                }
                else
                {
                    nextSegment.SetActive(true);
                }
            }
            else
            {
                //�ŏ��̃Z�O�����g�ŁA���o�[�X�łȂ��O�ɐi�����Ƃ��Ă�Ȃ玟�̃Z�O�����g��
                //���o�[�X�Ȃ�O�̃}�b�v��
                if (mapPointer.y == 0)
                {
                    if (!isReverse)
                    {
                        nextSegment.SetActive(true);
                    }
                    else
                    {
                        LevelManager.instance.LoadLevel();
                    }
                }
                //�Ō�̃Z�O�����g�ŁA���o�[�X�Ō��ɐi�����Ƃ��Ă�Ȃ�O�̃Z�O�����g��
                //���o�[�X����Ȃ��Ȃ玟�̂̃}�b�v��
                else
                {
                    if (isReverse)
                    {
                        prevSegment.SetActive(true);
                    }
                    else
                    {
                        LevelManager.instance.LoadLevel();
                    }
                }
            }


        }

        //���񂶂�Ȃ��Ȃ�O�̃Z�O�����g�����邩�ǂ����Ƃ��Ŕ��f
        else
        {
            //�Ō�̃Z�O�����g�Ȃ�
            if (FinalSegment)
            {
                //�ŏ��̃Z�O�����g�Ȃ�A���̃Z�O�����g������Ȃ烊�o�[�X����ˁi�����Ď����痈�����Ă��Ƃ�����j
                if (mapPointer.y == 0)
                {
                    isReverse = nextSegment.activeSelf;
                }

                //�Ō�̃Z�O�����g�Ȃ�A�O�̃Z�O�����g���Ȃ��Ȃ烊�o�[�X���ˁB���̃}�b�v���痈�����Ă��Ƃ���
                else
                {

                    isReverse = !prevSegment.activeSelf;
                }
            }
            //��������Ȃ��Ȃ玟�̃Z�O�����g������Ȃ烊�o�[�X�B�����痈�����Ă��Ƃ���
            else if(nextSegment != null)
            {
                
                isReverse = nextSegment.activeSelf;
            }
        }




        isPassed = false;

    }

    private void OnDisable()
    {
        //�҂��Ă鎞�̓L�����Z������
        if (isWait)
        {
            isWait = false;
            cts.Cancel();

        }
        isPassed = false;

    }



    /// <summary>
    /// ���x����Z�O�����g���N������
    /// </summary>
    void SetLevel()
    {
        //isReverse�Ȃ�ʉߑO�ɑO�̃��x�����ĂсA�ʉߌ�i���ڂ̒ʉ߁j�Ɏ��̃��x����u��
        //�ӂ��͈��ڒʉ߂Ŏ��A���ڂőO

        //���̂��ĂԂ��̃t���O
        bool isNext = !isPassed;
        //���o�[�X�Ȃ甽�]������
        isNext = !isReverse ? isNext : !isNext;


        //�g���K�[�ʂ����獡�̃Z�O�����g�����݈ʒu�ɂ���
        LevelManager.instance.DataUpdate(mapPointer);

        SegmentActive(isNext);


        //�ʉ߂𔽓]������
        isPassed = !isPassed; 
    }


    /// <summary>
    /// Exit�ɂ���̂̓g���K�[�ɓ����Ă�������]�����đO�̃Z�O�����g�ɖ߂��Ă����悤�Ȃ̂�z�肵��
    /// ���Ă������V�X�^�[����̃��[�v�Ƃ����p���ăg���K�[�����ɒʂ蔲������ǂ�����́H
    /// ����ς苗���łȂ�Ƃ��g���K�[������@�l�������������H
    /// ����ł��������ƕs����ɂȂ邩��A�g���K�[����񂭂������ゾ��X���W�AY���W�`�F�b�N�����銴���ɂ���H
    /// 
    /// �o����ǂ������ɂ��邩���厖����
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            if (!ExitCheck())
            {
                return;
            }

            //�ʉ߂��ĂȂ��Ȃ瑦���Ƀ��x���N��
            if (!isPassed)
            {
                SetLevel();
            }
            //��������Ȃ��Ȃ狗���҂��J�n
            //���Ɗ��ɋ����҂����Ȃ��������
            else
            {
                //�����҂����Ȃ�I��点��
                if (isWait)
                {
                    isWait = false;
                    cts.Cancel();
                    return;
                }
                
                WaitDistance().Forget();
            }
        }
    }

    /// <summary>
    /// ���������̂Ԃ񗣂��̂�҂��Ă��烌�x���W�J
    /// ���������ё΍�
    /// </summary>
    /// <returns></returns>
    async UniTaskVoid WaitDistance()
    {
        isWait = true;
        //�������N���A����܂ő҂���
        await UniTask.WaitUntil(() => DistanceCheck() ,cancellationToken: cts.Token);

        //���x���W�J
        SetLevel();
    }



    /// <summary>
    /// ���݂̃v���C���[�Ƃ̋����������������Ă邩���m�F������
    /// �����̓v���X�}�C�i�X�܂߂Ĕ��f�����̂Ŕ��Α��ɍs���Ȃ�����Ă΂�Ȃ�
    /// </summary>
    /// <returns></returns>
    bool DistanceCheck()
    {


        //direction��2�ȏ�Ȃ�c�ɂȂ�񂾂��
        float judgePosition = (int)direction < 2 ? Player.position.x : Player.position.y;



        //���]���Ă�Ȃ�F�X�ƕς���Ă���
        if (isReverse)
        {
            if (direction == PassDirection.LeftToRight || direction == PassDirection.DownToUp)
            {
                return myPosition - judgePosition > limitDistance;
            }
            else
            {
                return myPosition - judgePosition < -limitDistance;
            }

        }
        else
        {
            if (direction == PassDirection.LeftToRight || direction == PassDirection.DownToUp)
            {
                return myPosition - judgePosition < -limitDistance;

            }
            else
            {
                return myPosition - judgePosition > limitDistance;
            }
        }
    }



    /// <summary>
    /// �o�����g���K�[�N�������Ƀv���C���[�����邩�ǂ���
    /// </summary>
    /// <returns></returns>
    bool ExitCheck()
    {
        bool isNormal = !isPassed;

        isNormal = isReverse ? !isNormal : isNormal;

        //direction��2�ȏ�Ȃ�c�ɂȂ�񂾂��
        float judgePosition = (int)direction < 2 ? Player.position.x : Player.position.y;

        //���]���Ă�Ȃ�F�X�ƕς���Ă���
        if (isNormal)
        {
            if (direction == PassDirection.LeftToRight || direction == PassDirection.DownToUp)
            {
                return myPosition < judgePosition;
            }
            else
            {
                return myPosition > judgePosition;
            }
        }
        else
        {
            if (direction == PassDirection.LeftToRight || direction == PassDirection.DownToUp)
            {
                return myPosition > judgePosition;
            }
            else
            {
                return myPosition < judgePosition;
            }
        }
    }

    /// <summary>
    /// �Z�O�����g���N������
    /// </summary>
    /// <param name="isNext"></param>
    void SegmentActive(bool isNext)
    {

        bool notActive = false;

        if (FinalSegment)
        {

            if (isNext)
            {
                //�ŏ��̃Z�O�����g�ŁA�O�ɐi�����Ƃ��Ă�Ȃ�O�̃��x��������
                if (mapPointer.y == 0)
                {
                    LevelManager.instance.UnLoadLevel();
                }

                //�Ō�̃Z�O�����g�Ŏ��ɍs�����Ƃ���Ȃ玟�̃}�b�v��
                else
                {
                    notActive = true;
                    LevelManager.instance.LoadLevel();
                }
            }
            else
            {
                //�Ō�̃Z�O�����g�ŁA���ɐi�����Ƃ��Ă�Ȃ玟�̃��x��������
                if (mapPointer.y == 0)
                {
                    LevelManager.instance.UnLoadLevel();
                }
                else
                {
                    LevelManager.instance.LoadLevel();
                    notActive = true;
                }
            }
        }

        if(!notActive)
        {
            if (isNext)
            {
                
                nextSegment.SetActive(true);
                if (prevSegment != null && prevSegment.activeSelf)
                {
                    prevSegment.SetActive(false);
                }

            }
            else
            {
                prevSegment.SetActive(true);
                if (nextSegment != null && nextSegment.activeSelf)
                {
                    nextSegment.SetActive(false);
                }
            }

        }
    }

}
