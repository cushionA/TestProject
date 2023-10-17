using RenownedGames.Apex.Serialization.Collections.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

using Com.LuisPedroFonseca.ProCamera2D;
using UnityEngine.Playables;
using Cysharp.Threading.Tasks;
using DarkTonic.MasterAudio;


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

    int bestScore;

    public int BestScore
    {
        get { return bestScore; }
    }

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


    public bool isDie;
    #endregion


    #region �`�F�b�N�|�C���g�֘A



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


    [SerializeField]
    TextMeshProUGUI finalScore;

    [SerializeField]
    TextMeshProUGUI bounus;

    [SerializeField]
    TextMeshProUGUI summury;

    /// <summary>
    /// �X�R�A���\��
    /// </summary>
    [SerializeField]
    GameObject ScoreDip;


    /// <summary>
    /// UI�̃A�j���[�V����
    /// </summary>
    [SerializeField]
    MyAnimator _anim;


    #endregion

    [SerializeReference]
    ProCamera2D _camera;


    [SerializeField]
    GameObject home;

    [SerializeField]
    GameObject newRecord;

    int timeBounus;

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


    public Vector2 PlayerPosi;

    public bool isGoal;

    [SerializeField]
    PlayableDirector ending;


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
        PlayerPosi = Player.transform.position;
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

    private void LateUpdate()
    {
        PlayerPosi = Player.transform.position;
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


   

    /// <summary>
    /// �X�R�A�\��
    /// </summary>
    public async void ScoreDisplay()
    {
        //�X�R�A�\��
        ScoreDip.SetActive(true);

        timeBounus = (1000 - (lastTime * 2)) * 10;
        timeBounus = timeBounus < 0 ? 0:timeBounus;
        await Display(Score, finalScore,false);

        await Display(timeBounus,bounus,false);

        await Display(timeBounus+Score,summury,true);

                Debug.Log($"�܂�{bestScore}"); 
        //��������x�X�g�L�^�����Ă邩�Ƃ��A�N���b�N��������ǂ��悤�ɂƂ�
        if(Score+timeBounus > bestScore)
        {
   
            bestScore = timeBounus + Score;
            Debug.Log($"����{bestScore}");
            newRecord.SetActive(true);
            PlaySound("NewRecord");
            
        }
           EndGame().Forget();
    }

    async UniTask Display(int target,TextMeshProUGUI targetUI,bool total, int nowNum = 0)
    {
        if (nowNum == 0)
        {
            if (target > 40)
            {
                targetUI.text = "0";
            }
            else
            {
                if (total)
                {
                    PlaySound("TotalOpen");
                }
                else
                {
                    PlaySound("ScoreOpen");
                }
                targetUI.text = target.ToString();
                return;
            }
            await UniTask.Delay(TimeSpan.FromSeconds(0.8f));
            PlaySound("ScoreEffect");
        }
        else
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.05f));
        }
        nowNum = nowNum + (target / 40) <= target ? nowNum + (target / 40) : target;

        targetUI.text =  nowNum.ToString();

        if (nowNum != target)
        {
            await Display(target,targetUI,total,nowNum);
        }
        else
        {
            StopSound("ScoreEffect");
            if (total)
            {
                PlaySound("TotalOpen");
            }
            else
            {
                PlaySound("ScoreOpen");
            }
        }
    }


    async UniTaskVoid EndGame()
    {
        await UniTask.WaitUntil(() => Input.GetMouseButtonDown(0),cancellationToken: destroyCancellationToken);
        GetComponent<UIManager>().ClearSave();
    }


    public void ScoreChange(int change)
    {
        Debug.Log($"����������{change}");
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
        else if (life < 0)
        {
            life = 0;
        }
     lifeViewer.text = life.ToString();
        return life == 0;
    }


    /// <summary>
    /// ���S�C�x���g
    /// ��蒼�����ɃX�R�A����
    /// �Ō�ɃZ�[�u�����Ƃ��납���蒼��
    /// �_���[�W�ŃX�R�A�����Ă邩�玀�̃X�R�A��������Ȃ�
    /// ����A�ЂƂO�̃Z�O�����g�ɖ߂���
    /// </summary>
    public async UniTaskVoid Die()
    {
        isDie = true;
        Score = Score - 1100 < 0 ? 0 : Score - 1100;
        life = maxLife;

        //�����Ń��x���}�l�[�W���[�łЂƂO�̃Z�O�����g�c�Ƃ��������̃Z�O�����g�̈�ԉ��P�O������ɖ߂�
        //��u�Ó]�����邩
        LevelManager.instance.Resporn();

        AllUpdate();

        await Player.GetComponent<CharacterController>().DieRecover();
        isDie = false;
        Debug.Log("��������");
    }



    /// <summary>
    /// �J�������G���f�B���O�d�l�ɂ��ăC�x���g�V�[���J�n
    /// </summary>
    public void EndingStart()
    {
        inGame = false;
        ending.Play();
        _camera.AdjustCameraTargetInfluence(_camera.GetCameraTarget(Player.transform), 0, 0, 0f);
        _camera.AdjustCameraTargetInfluence(_camera.GetCameraTarget(home.transform),0,1,0.05f);
        EndingWait().Forget();
    }

    /// <summary>
    /// �^�C�����C���I�������X�R�A�\�����邼
    /// </summary>
    /// <returns></returns>
    public async UniTaskVoid EndingWait()
    {        
        Debug.Log("�G�t�F�N�g");
        await UniTask.WaitUntil(() => ending.time >= ending.duration,cancellationToken: destroyCancellationToken);
        ScoreDisplay();

    }



    #endregion






    /// <summary>
    /// �����Đ��B�����ɒǐ����Ȃ�
    /// ���ʂ͍Đ����鉹����
    /// </summary>
    /// <param name="sType">�Đ����鉹�̖��O�B�o���G�[�V��������̂�B
    /// <param name="sourcePosition">����炵�����ʒu�B�K�{�ł��B </param>
    /// <param name="volumePercentage"><b>Optional</b> - ���ʂ������čĐ��������ꍇ�Ɏg�p���܂��i0�`1�̊ԁj�B
    /// <param name="pitch"><b>Optional</b> - ����̃s�b�`�ŉ����Đ��������ꍇ�Ɏg�p���܂��B</param> <param name="pitch"><b>Optional</b> - ����̉����ōĐ��������ꍇ�Ɏg�p���܂��B��������ƁA�o���G�[�V�����̒���pich��random pitch���㏑�����܂��B
    /// <param name="delaySoundTime"><b>Optional</b> - �����ɂł͂Ȃ��AX�b��ɉ���炵�����ꍇ�Ɏg�p���܂��B
    /// <param name="variationName"><b>Optional</b> - ����̃o���G�[�V�����i�܂��̓N���b�vID�j�̖��O�ōĐ��������ꍇ�Ɏg�p���܂��B����ȊO�̏ꍇ�́A�����_���ȃo���G�[�V�������Đ�����܂��B
    /// <param name="timeToSchedulePlay"><b>Optional</b> - �T�E���h���Đ����邽�߂�DSP���Ԃ�n�����߂Ɏg�p���܂��B�ʏ�͂�����g�p�����A�����delaySoundTime�p�����[�^���g�p���܂��B
    /// <param name="isRemember"><b>Optional</b> - PlaySoundResult���擾���邩�ǂ����B
    /// <returns>PlaySoundResult - ���̃I�u�W�F�N�g�́A�T�E���h���Đ����ꂽ���ǂ�����ǂݎ�邽�߂Ɏg�p����A�g�p���ꂽVariation�I�u�W�F�N�g�ւ̃A�N�Z�X���\�ł��B

    public void PlaySound(string sType, float volumePercentage = 1f, float? pitch = null, float delaySoundTime = 0f, string variationName = null, double? timeToSchedulePlay = null, bool isRemember = false)
    {
        // Debug.Log("������");
        if (isRemember)
        {
            MasterAudio.PlaySound3DAtVector3(sType, PlayerPosi, volumePercentage, pitch, delaySoundTime, variationName, timeToSchedulePlay);
        }
        else
        {
            MasterAudio.PlaySound3DAtVector3AndForget(sType, PlayerPosi, volumePercentage, pitch, delaySoundTime, variationName, timeToSchedulePlay);

        }

    }
    /// <summary>
    /// �Ō��true�ɂ��Ȃ��Ȃ�t�F�[�h����{
    /// </summary>
    /// <param name="soundGroupName"></param>
    /// <param name="fadeTime"></param>
    /// <param name="isStop"></param>
    public void StopSound(string soundGroupName)
    {

            MasterAudio.StopAllOfSound(soundGroupName);

    }

    /// <summary>
    /// �Z�[�u�@�\
    /// �K�������ϐ������ˁA������L�[�ŌĂяo���Ďg���΂�����
    /// </summary>
    public override void Save()
    {
        //���̏�ԂƎg�p�G�t�F�N�g�ƍ��̈ʒu���i�[
        ES3.Save<int>("PlayTime", lastTime);
        ES3.Save<int>("Score", Score);
        ES3.Save("Life", life);



        //���[�h���ɕK�v�ȏ��u�̓X�R�A����UI�ւ̔��f�ƃ������[�̕��A����
    }


    public override void Load()
    {
        lastTime = ES3.Load<int>("PlayTime");
        playTime = lastTime;
        Score = ES3.Load<int>("Score");
         life = ES3.Load<int>("Life");


        bestScore = ES3.Load<int>("BestScore",0);

        AllUpdate();
    }


}
