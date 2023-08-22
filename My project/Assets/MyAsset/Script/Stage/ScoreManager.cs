using RenownedGames.Apex.Serialization.Collections.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �X�R�A�ƃ^�C���̊Ǘ�
/// UI�����䂷�邩
/// </summary>
public class ScoreManager : SaveMono
{

    public static ScoreManager instance = null;

    #region �X�R�A�ƃ^�C���̕ϐ�
    /// <summary>
    /// �Q�[���J�n���Ă邩
    /// </summary>
    bool inGame;

    /// <summary>
    /// �V��ł鎞��
    /// </summary>
    float playTime;

    /// <summary>
    /// ���݂̃X�R�A
    /// </summary>
    int Score;

    public GameObject Player;
    #endregion

    #region �̗͊֘A�̕ϐ�

    /// <summary>
    /// �����c�@��
    /// �`�F�b�N�|�C���g�ł����܂ŉ񕜁H
    /// ���₻��͂����ȁB�X�R�A�x�����ŉ񕜂͂���
    /// </summary>
    public int maxLife;

    /// <summary>
    /// �̗́A�c�@
    /// </summary>
    int life;



    #endregion


    #region �`�F�b�N�|�C���g�֘A



    /// <summary>
    /// �`�F�b�N�|�C���g���B���Ɏg�p
    /// ��蒼�����Ɏg��
    /// </summary>
    int memoryLife;

    /// <summary>
    /// �`�F�b�N�|�C���g���B���Ɏg�p
    /// ��蒼�����Ɏg��
    /// </summary>
    int memoryScore;

    /// <summary>
    /// �`�F�b�N�|�C���g���B���Ɏg�p
    /// ��蒼�����Ɏg��
    /// </summary>
    int memoryTime;

    /// <summary>
    /// �}�b�v�؂�ւ����Ƀ��x���}�l�[�W���[�ɐ�������
    /// �Z�[�u�|�C���g������������Ƃ��ł���������
    /// ����͂����
    /// </summary>
    Vector2 checkPoint;


    #endregion


    #region UI�֌W

    [SerializeField]
    TextMeshProUGUI scoreViewer;

    [SerializeField]
    TextMeshProUGUI timeViewer;

    [SerializeField]
    TextMeshProUGUI lifeViewer;

    int lastTime;

    /// <summary>
    /// UI�̃A�j���[�V����
    /// </summary>
    [SerializeField]
    MyAnimator _anim;


    #endregion


    #region

    [Serializable]
    public class MyAnimator : SerializableDictionary<string, Animator>
    {
        [SerializeField]
        private List<string> keys;

        [SerializeField]
        private List<Animator> values;

        protected override List<string> GetKeys()
        {
            return keys;
        }

        protected override List<Animator> GetValues()
        {
            return values;
        }

        protected override void SetKeys(List<string> keys)
        {
            this.keys = keys;
        }

        protected override void SetValues(List<Animator> values)
        {
            this.values = values;
        }
    }
    #endregion



    private void Awake()
    {
        if(instance == null)
        {
            instance = this;

        }
        else
        {
            Destroy(this.gameObject);
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        //�e�X�g���͂�
        //�{�Ԃ̓^�C�g������Ƃ񂾎��_�ł�邩
        inGame = true;

        life = maxLife;
        AllUpdate();
    }

    // Update is called once per frame
    void Update()
    {
        if (inGame)
        {
            //�v���C���͎��Ԃ����Z
            playTime += Time.deltaTime;

            //UI���Ǘ����Ă���
            UIUpdate();
        }


    }

    #region �X�R�A�ƃ^�C���֘A

    public void ScoreChange(int change)
    {
        if (Score + change < 0)
        {
            Score = 0;
        }
        else if (Score + change > 999999999)
        {
            Score = 999999999;
        }
        else
        {
            Score += change;
        }
        scoreViewer.text = Score.ToString();
    }


    void UIUpdate()
    {
        //Debug.Log($"aaa{(int)playTime}dd{lastTime}");
        if ((int)playTime > lastTime)
        {
            lastTime = (int)playTime;
            timeViewer.text = lastTime.ToString();
        }
    }


    void AllUpdate()
    {
        timeViewer.text = lastTime.ToString();
        scoreViewer.text = Score.ToString();
        lifeViewer.text = life.ToString();
    }
    #endregion



    #region �̗͊֘A


    /// <summary>
    /// ���C�t�����炷
    /// �Ԃ�l���^�Ȃ玀�S
    /// </summary>
    public bool LifeChange(bool damage = false)
    {

        if (damage)
        {
            _anim["Life"].Play("Damage");
            life--;
        }
        else
        {
            _anim["Life"].Play("Recover");
            life++;
        }


        if (life > 99)
        {
            life = 99;
        }
        if (life > 0)
        {
            lifeViewer.text = life.ToString();
        }

        return life < 0;
    }


    /// <summary>
    /// ���S�C�x���g
    /// ��蒼�����ɃX�R�A����
    /// </summary>
    public void Die()
    {

        Score = Score - 500 < 0 ? 0 : Score - 500;
        life = maxLife;

        AllUpdate();
    }

    public void Reset()
    {
        Score -= memoryScore;
    }

    #endregion





    #region �`�F�b�N�|�C���g�֘A

    /// <summary>
    /// �Ē���
    /// </summary>
    void ReTry(bool isDie)
    {
        if (!isDie)
        {
            Score = memoryScore;
            playTime = memoryTime;
        }

        life = memoryLife;
        AllUpdate();

        //��������Ó]����

    }


    /// <summary>
    /// �`�F�b�N�|�C���g���B
    /// </summary>
    void ReachPoint(int reach)
    {
        memoryScore = Score;
        memoryTime = lastTime;
        memoryLife = life;

    }

    #endregion



    /// <summary>
    /// �Z�[�u�@�\
    /// �K�������ϐ������ˁA������L�[�ŌĂяo���Ďg���΂�����
    /// </summary>
    public override void Save()
    {
        //���̏�ԂƎg�p�G�t�F�N�g�ƍ��̈ʒu���i�[
        ES3.Save("PlayTime", playTime);
        ES3.Save("Score", Score);
        ES3.Save("Life", life);

        //�L�^�p�A��蒼���Ɏg��
        ES3.Save("MScore", memoryScore);
        ES3.Save("MLife", memoryLife);
        ES3.Save("MTime", memoryTime);

        //���[�h���ɕK�v�ȏ��u�̓X�R�A����UI�ւ̔��f�ƃ������[�̕��A����
    }


    public override void Load()
    {
        playTime = ES3.Load<float>("PlayTime");
        Score = ES3.Load<int>("Score");
         life = ES3.Load<int>("Life");

        //�L�^�p�A��蒼���Ɏg��
        memoryScore = ES3.Load<int>("MScore");
        memoryLife = ES3.Load<int>("MLife");
        memoryTime = ES3.Load<int>("MTime");
        AllUpdate();
    }


}
