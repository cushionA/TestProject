using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Unity.Entities;
using UnityEngine;


/// <summary>
/// ����̓}�b�v�N�����g���K�[����I�u�W�F�N�g
/// ���[�v���������邯�ǂ��������݂��Ă�Ԃ����񂳂�Ȃ�������S
/// �����̓��x���̐^�񒆂Ǝv�����ꏊ�Ɉ���ݒu����
/// 
/// ���[�v�Ńg���K�[���蔲����\��������
/// 
/// 
/// �d�l
/// 
/// ��{�I�Ƀg���K�[�ɂȂ���W�Ƃ̋����ŃZ�O�����g�̃I���I�t��؂�ւ���
/// �E���獶�Ƃ��ォ�牺�Ƃ������̈Ⴂ�ɂ��Ή����Ă���
/// �����̈Ⴂ�ŎQ�l�ɂ���̂�X���W�������W����ς���
/// 
/// �A�N�e�B�u�ɂ���͎̂��̃Z�O�����g�ƑO�̃Z�O�����g
/// 
/// </summary>
public class SegmentTrigger : MonoBehaviour
{

    //���ǒ��

    //WaitDistance���\�b�h���A�N�e�B�u���ɌĂ�Ŏ����Ƃ̋������߂Â��܂ő҂ĂΓ����蔻��͂���Ȃ��̂ł�



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
    /// �v���C���[���ǂ̕�������N�����ǂ��֌�������
    /// </summary>
    [SerializeField]
    PassDirection direction;


    [Header("�O�̃I�u�W�F�N�g")]
    public GameObject prevSegment;

   [Header("���̃I�u�W�F�N�g")]
    public GameObject nextSegment;


    [Header("�ڑ�����}�b�v�̃Z�O�����g�ԍ�")]
    [SerializeField]
    int nextMapSegment;

    /// <summary>
    /// �����̍��W
    /// LeftTO�@�݂����ȉ����[�g�Ȃ�X���W����邵
    /// �c�Ȃ炻�̋t
    /// </summary>
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
    [Header("���Ԗڂ̃}�b�v�̉��Ԗڂ̃Z�O�����g���B0���琔����")]
    Vector2Int mapPointer;




    /// <summary>
    /// �g���K�[�ɂȂ�ʒu�������I�u�W�F�N�g
    /// ���ꂪ�Ȃ���Ύ����̈ʒu����
    /// </summary>
    [SerializeField]
    [Header("�g���K�[�ɂȂ�ʒu�͂ǂ�")]
    GameObject trigger;

    
    // Start is called before the first frame update
    void Awake()
    {
        //�����̑��Ƀg���K�[������Ȃ炻����g������
        Transform position = trigger != null ? trigger.transform : this.transform;

        //�����̍��W�̏㉺���W���Q�l�ɂ��邩���߂�B�c�N�����[�g�Ȃ炙���W�A���N�����[�g�Ȃ炘���W
        myPosition = direction == PassDirection.RightToLeft || direction == PassDirection.LeftToRight ? position.position.x : position.position.y;

        //�v���C���[�̃g�����X�t�H�[��
        Player = ScoreManager.instance.Player.transform;

    }

    /// <summary>
    /// 
    /// isFirst�t���O�𗘗p������N�����̐ݒ�B�Ō�̃Z�O�����g�ɂ���Ȃ�}�b�v���Ăяo����
    /// �@���̃R���g���[���[�ł͍�����Z�O�����g�Ǝ��ɍs���Z�O�����g������L�������Ă�
    /// �@������{���ɍŏ��Ƀ��[�h�������������̃Z�O�����g���N�����Ȃ���Ȃ�Ȃ�
    ///    ������}�l�[�W���[�ɂ��̂���(FirstSegmentActive)
    /// </summary>
    private void OnEnable()
    {


        if (this.gameObject.name == "SecondMapSegment0")
        {
            Debug.Log("����");
        }

        //���_�Ԃ̃t�@�X�g�g���b�v�Ƃ��������͂��̋��_�A������ΓI�Ȃ���ɃZ�O�����g�N�������邩


        //���o�[�X����͋���������O�̃Z�O�����g�����邩�ǂ������ǂ��Ȃ��H
        //����N�����Ƃ���ȊO�ŕ����邩

        //����N�����Ȃ�
        if (!LevelManager.instance.isFirst)
        {
            Debug.Log($"hhhh{isReverse}{LevelManager.instance.isFirst}");
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
         Debug.Log($"������{isReverse}{LevelManager.instance.isFirst}");
            //����N���t���O������
            LevelManager.instance.isFirst = true;

            Debug.Log($"dedede{isReverse}{LevelManager.instance.isFirst}");
            //����N���t���O�����������ŁA�����O�̃Z�O�����g�N��

            if (!FinalSegment)
            {

                Debug.Log($"��ggg{isReverse}{LevelManager.instance.isFirst}");
                if (isReverse)
                {
                    nextSegment.SetActive(true);
                }
                else
                {
                    prevSegment.SetActive(true);
                }
            }
            else
            {
                Debug.Log("��ddsdssss");

                //�ŏ��̃Z�O�����g�ŁA���o�[�X�łȂ��O�ɐi�����Ƃ��Ă�Ȃ玟�̃Z�O�����g��
                //���o�[�X�Ȃ�O�̃}�b�v��
                if (prevSegment == null)
                {
                    if (!isReverse)
                    {
                        nextSegment.SetActive(true);
                    }
                    else
                    {
                        LevelManager.instance.LoadLevel(nextMapSegment).Forget();
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
                        Debug.Log("��������");
                        LevelManager.instance.LoadLevel(nextMapSegment).Forget();
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
        TriggerStart().Forget();
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
    /// �������ŏ��ɌĂԂ��ƂŔ���ł���
    /// ������ǂ񂾂玟�ɔ��������ы֎~��DistanceWait���Ă�
    /// </summary>
    /// <returns></returns>
    async UniTaskVoid  TriggerStart()
    {
        isWait = true;
        //�������N���A����܂ő҂���
        await UniTask.WaitUntil(() => TriggerCheck(), cancellationToken: cts.Token);


        //���x���W�J
        SetLevel();
        WaitDistance().Forget();
    }







    #region �󋵔��f�֘A

    /// <summary>
    /// �g���K�[�N�������Ƀv���C���[�����邩�ǂ���
    /// �܂�͍�����E�̃Z�O�����g��������A�g���K�[�ɐڐG�����Ƃ��Ă��y�n���\�I�u�W�F�N�g�̉E�ɂ��Ă���Ȃ��ƃ_��
    /// 
    /// �g��Ȃ�����
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
    /// ���݂̃v���C���[�Ƃ̋����������������Ă邩���m�F������
    /// �����̓v���X�}�C�i�X�܂߂Ĕ��f�����̂Ŕ��Α��ɍs���Ȃ�����Ă΂�Ȃ�
    /// </summary>
    /// <returns></returns>
    bool DistanceCheck()
    {

        //���݂̃Z�O�����g����Ȃ��̂Ȃ��������ł�
        if (Mathf.Abs(LevelManager.instance.GetSegment() - mapPointer.y) > 1)
        {
            return false;
        }

        bool isNext = !isPassed;

        isNext = isReverse ? !isNext : isNext;

        //direction��2�ȏ�Ȃ�c�ɂȂ�񂾂��
        float judgePosition = (int)direction < 2 ? Player.position.x : Player.position.y;


        //100�����Ńg���K�[�N�����邩��A100���O�ŋN������悤�ɂ��Ȃ��ƘA���ŉ���

        //���]���Ă�Ȃ�F�X�ƕς���Ă���
        if (!isNext)
        {
            //�t�����牺�ɂ��Ȃ��Ƃ����Ȃ�
            if (direction == PassDirection.LeftToRight || direction == PassDirection.DownToUp)
            {
                return myPosition - judgePosition > limitDistance + 100;
            }
            else
            {
                return myPosition - judgePosition < -(limitDistance + 100);
            }

        }
        else
        {
            //��ɍs���Ă鎞�A�E�ɍs���Ă鎞y
            if (direction == PassDirection.LeftToRight || direction == PassDirection.DownToUp)
            {
                return myPosition - judgePosition < -(limitDistance + 100);

            }
            else
            {
                return myPosition - judgePosition > limitDistance + 100;
            }
        }

    }

    /// <summary>
    /// ���݂̃v���C���[�Ƃ̋������g���K�[�͈͂����m�F������
    /// </summary>
    /// <returns></returns>
    bool TriggerCheck()
    {
        //�ԍ��Ń��b�N������̂܂�����
        //���̐�Ŕԍ��ό`���Ă�킯����
        //�ԍ��̌덷������Ȃ����Ă��Ƃɂ��悤��
      //  Debug.Log($"����{(myPosition - ((int)direction < 2 ? Player.position.x : Player.position.y)) < 100} �ԍ�{LevelManager.instance.GetSegment() == mapPointer.y}");
        //���݂̃Z�O�����g����Ȃ��̂Ȃ��������ł�
        if (Mathf.Abs(LevelManager.instance.GetSegment() - mapPointer.y) > 1)
        {
            return false;
        }

        //direction��2�ȏ�Ȃ�c�ɂȂ�񂾂��
        float judgePosition = (int)direction < 2 ? Player.position.x : Player.position.y;


        if (mapPointer.y == 1)
        {
        if (Mathf.Abs(myPosition - judgePosition) < 100)
        {
            Debug.Log($"�������B{mapPointer.y}");
        }
        }



        //�����������ƃg���K�[�͈͓��Ȃ�
        return Mathf.Abs(myPosition - judgePosition) < 100;
    }




    //�v�����񂾂�����WaitDistance���Z�O�����g���A�N�e�B�u�Ƀ��Ȃ����u�ԌĂ�ł��Γ����蔻��ł͂Ȃ������`�F�b�N�ł���̂ł�


    /// <summary>
    /// ���������̂Ԃ񗣂��̂�҂��Ă��烌�x���W�J
    /// ���������ё΍�
    /// 
    /// �g���K�[��ʂ�����A�����苗������ɐi�܂Ȃ��ƋN�����Ȃ�
    /// </summary>
    /// <returns></returns>
    async UniTaskVoid WaitDistance()
    {
        isWait = true;
        //�������N���A����܂ő҂���
        await UniTask.WaitUntil(() => DistanceCheck(), cancellationToken: cts.Token);
        if (mapPointer.y == 1)
        {
        Debug.Log($"dis�������B{mapPointer.y}");
        }


        //���x���W�J
        SetLevel();
        TriggerStart().Forget();
    }




    #endregion



    #region �}�b�v�A�Z�O�����g�N���֘A


    /// <summary>
    /// ���x����Z�O�����g���N������
    /// �����ăt���O���Ǘ����A���׃}�l�ɂ����݂̃}�b�v�̔ԍ��𑗐M����
    /// �ԍ��͌��݂̃}�b�v�ŁA�ʉ߂����g���K�[�̃Z�O�����g�ԍ����̗p
    /// ��̓I�Ȕ��f�i�}�b�v�N�����Z�O�����g���Ȃǁj��SegmentActive�i�j�Ɉϑ�
    /// </summary>
    void SetLevel()
    {
        //isReverse�Ȃ�ʉߑO�ɑO�̃��x�����ĂсA�ʉߌ�i���ڂ̒ʉ߁j�Ɏ��̃��x����u��
        //�ӂ��͈��ڒʉ߂Ŏ��A���ڂőO

        //���̂��ĂԂ��̃t���O
        bool isNext = !isPassed;
        //���o�[�X�Ȃ甽�]������
        isNext = !isReverse ? isNext : !isNext;

        if (mapPointer.y == 1)
        {
            Debug.Log($"�t�H{isReverse}�ʉߍς݁H{isPassed}");
        }
        //�g���K�[�ʂ����獡�̃Z�O�����g�����݈ʒu�ɂ���
        LevelManager.instance.DataUpdate(mapPointer);

        SegmentActive(isNext);


        //�ʉ߂𔽓]������
        isPassed = !isPassed;
    }




    /// <summary>
    /// �ړ����Ɏ��Ɍ������Z�O�����g���N������
    /// �Ō�̃Z�O�����g�Ȃ玟�̃}�b�v���Ăяo��
    /// 
    /// �}�b�v�Ăяo������̓Z�O�����g��L�������Ȃ�
    /// �Ăяo�����}�b�v�̃Z�O�����g��L���������ڂ͒ʘH�Ɏ������Ă���������
    /// ���ƒʘH�ɂ͌����Ȃ��ǂ�u���Ă����āA�}�b�v�̃��[�h���I�����������悤�ɂ��Ă���������
    /// </summary>
    /// <param name="isNext">IsNext�͎��̏ꏊ�ɍs�����Ƃ��Ă邩�O�ɂ��ǂ邩</param>
    void SegmentActive(bool isNext)
    {

        //����̏����ŃA�N�e�B�u�ɂ��ׂ��Z�O�����g�����邩�ǂ���
        //�O�̃}�b�v�������肵���ꍇ�͎��Ɍ������Z�O�����g�����Ă����Ȃ��Ƃ�
        //�t�Ƀ}�b�v�Ăяo�����肵����Z�O�����g��L��������Ӗ��͂��炸
        bool notActive = false;


        //�Ō�̃Z�O�����g�ł͎��̃}�b�v�Ă񂾂肷��
        if (FinalSegment)
        {

            //�����ōŌ�̃Z�O�����g����O�ɍs�����Ƃ��Ă邩���ɍs�����Ƃ��Ă邩�𔻒f
            //�}�b�v�������ׂ����ĂԂׂ����𔻒f�����
            if (isNext)
            {
                //�ŏ��̃Z�O�����g�ŁA�O�ɐi�����Ƃ��Ă�Ȃ�O�̃��x��������
                //�O�̃Z�O�����g���Ȃ��Ȃ�ŏ��Ȃ񂾂��
                if (prevSegment == null)
                {
                    LevelManager.instance.UnLoadLevel();
                }

                //�Ō�̃Z�O�����g�Ŏ��ɍs�����Ƃ���Ȃ玟�̃}�b�v���Ăяo��
                else
                {
                    notActive = true;
                    LevelManager.instance.LoadLevel(nextMapSegment).Forget();
                }
            }
            else
            {
                //�Ō�̃Z�O�����g�ŁA�O�ɖ߂낤�Ƃ��Ă�Ȃ玟�̃��x��������
                //�����Ȃ��Ȃ�ŏ��Ȃ�
                if (nextSegment == null)
                {
                    LevelManager.instance.UnLoadLevel();
                }
                else
                {
                    LevelManager.instance.LoadLevel(nextMapSegment).Forget();
                    notActive = true;
                }
            }
        }

        //�L�������ׂ��Z�O�����g������Ȃ�
        if(!notActive)
        {
            if (isNext)
            {
                if (nextSegment != null)
                {
                    nextSegment.SetActive(true);
                }
                if (prevSegment != null && prevSegment.activeSelf)
                {
                    prevSegment.SetActive(false);
                }

            }
            else
            {
                if (prevSegment != null)
                {
                    prevSegment.SetActive(true);
                }
                if (nextSegment != null && nextSegment.activeSelf)
                {
                    nextSegment.SetActive(false);
                }
            }

        }
    }

    #endregion


}
