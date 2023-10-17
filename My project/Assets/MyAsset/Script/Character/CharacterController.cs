using UnityEngine;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using System.Linq;
using Beautify.Universal;
using System;
using static Dreamteck.AsyncJobSystem;
using System.Threading;
using Unity.Entities.UniversalDelegates;
using Cysharp.Threading.Tasks;

public class CharacterController : SaveMono
{
    public float jumpPower;
    public float angle;

    [SerializeField]
    float reflectForce = 45;

    private Rigidbody2D rb;

    [SerializeField]
    GameObject SpriteEffect;

    /// <summary>
    /// �^�b�`���L���Ȕ͈�
    /// </summary>
    public float touchRange;

    /// <summary>
    /// �^�b�`�����ɂȂ�|�C���g
    /// ������Ƃ悭���
    /// </summary>
   public  float sweetRange;

    #region�@�����X�e�[�^�X



    /// <summary>
    /// ���݂̐��l
    /// </summary>
    float nowJumpPower;
    float nowAngle;

    float horizontal;

    Vector2 direction = new Vector2();

    /// <summary>
    /// �Ԃ������C�x���g�̃f�[�^���Ǘ�
    /// �f�[�^����Timer�Ŏ��Ԃ��͂����Ă�
    /// </summary>
    [SerializeField]
    [ES3Serializable]
    List<EventObject.EventData> _effectData = new List<EventObject.EventData>();

    float jumpTime;

    bool isGround;

    [SerializeField]
    CharacterState nowAction;

    CharacterState lastAction;

    EffectController efCon;

    Vector2 velocityCOn;
    Vector2 ground = new Vector2(0,10);

    /// <summary>
    /// ���܂ǂ̏�Ԉُ��o�t�����C���ŃO���t�B�b�N�ɔ��f����Ă邩
    /// </summary>
    [SerializeField]
    [ES3Serializable]
    GimickCondition _myStatus;

    [SerializeField] private LayerMask groundLayer;

    [SerializeField]
    float gCheckSpan;

    RaycastHit2D hit;

    #endregion


    #region


    /// <summary>
    /// �p���I�ȍs��
    /// </summary>
    public enum CharacterState
    {
        jump,

        fall,
        walk,
        Float,//����
        idle,
        none
    }

    /// <summary>
    /// ������̏�ԕω�
    /// </summary>
    public enum CharacterCondition
    {
       
        damage,
        heal,
        addScore,
        die,
        none
    }


    /// <summary>
    /// �o�t�f�o�t
    /// </summary>
    public enum ExtraCondition
    {
        
        invincible,//���G
        boostJump,
        none
    }




    /// <summary>
    /// �X�e�[�^�X�p
    /// </summary>
     [Serializable]
    public struct GimickCondition
    {
        //���G���ǂ���
        public bool invincible;

        public bool boostJump;

        public bool Die;

    }

    #endregion


    enum jumpStatus
    {
        weak = -1,
        normal = 0,
        add = 1
    }

    jumpStatus _status;


    /// <summary>
    /// �Ō�̃A�j���̂����Ƃł�������
    /// </summary>
    int isTrigger;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        efCon = GetComponent<EffectController>();
        ChangeAction(CharacterState.idle);
        //  BeautifySettings.settings.blurIntensity.value = 1.2f;
        _status = jumpStatus.normal;
    }

    void Update()
    {
        //����ł�Ȃ�߂�
        if (_myStatus.Die)
        {
            Debug.Log("����������");
            rb.velocity = Vector2.zero;
            return;
        }

        if (ScoreManager.instance.isGoal)
        {
            //�G�t�F�N�g������
            if (_effectData.Any())
            {
                int count = _effectData.Count;
                for (int i = 0;i < count;i++)
                {
                    ConditionEnd(_effectData[i]);
                    _effectData.Remove(_effectData[i]);
                }
                _effectData = null;
            }

            if (isGround)
            {
                //�g���K�[�|�C���g�Ɍ���������
                // �g���K�[�|�C���g�Ƃ������AX���W�[���ł�����
                //�s��������Ō�̃A�j�����J�n
                //����A�j������Ȃ��A�������Ȃ����]������
                //�����Ĉړ��I���^��0�ɂȂ�܂ł܂���]�A���̌�A�j���J�n
                //��]�����͂����ɁA�ړ�������Fixed�ɒu����
                //�S�[���ŃO���E���h�Ńg���K�[���B���ĂȂ��Ȃ�i�ނ�݂�����

                if (Math.Abs(isTrigger) == 1)
                {
                    //��]���ăA�j���J�n�@

                    //�E��
                    if(isTrigger > 0)
                    {
                        transform.Rotate(0,0,-2);
                    }
                    //
                    else
                    {
                        transform.Rotate(0, 0, 2);
                    }

                    //�~�܂��Ă�Ȃ�
                    if(rb.velocity == Vector2.zero)
                    {
                        if(Math.Abs(transform.rotation.z) < 10)
                        {
                            transform.rotation = Quaternion.Euler(Vector3.zero);
                            isTrigger = 2;
                            efCon.EndAnimStart();
                        }
                    }
                }


            }

            //�S�[�������Ȃ�~�܂�
            return;
        }

        if(nowAction == CharacterState.none)
        {
            return;
        }
            horizontal = Input.GetAxisRaw("Horizontal");
            _status = (jumpStatus)Input.GetAxisRaw("Vertical");
        if (nowAction == CharacterState.jump)
        {

            if (Input.GetMouseButtonDown(1))
            {
                MasterAudio.PlaySound3DAtVector3AndForget("JumpStop",transform.position);
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0;
                ChangeAction(CharacterState.fall);
            }
           

            // �㉺���E�̕����L�[�̓��͂�ǂݎ��

        }

        if (_myStatus.Die)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {




            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            
            
            //�N���b�N�n�_�̍��W�l��
            Vector2 _position = Physics2D.Raycast((Vector2)ray.origin, (Vector2)ray.direction).point;

            #region �v����
            /*
            RaycastHit2D hit2d = Physics2D.Raycast((Vector2)ray.origin, (Vector2)ray.direction);
            if (hit2d.transform == null)
            {
                return;
            }

            if (this.gameObject == hit2d.transform.gameObject)
            {
                ChangeAction(CharacterState.jump);
                rb.velocity = Vector2.zero;
                JumpCheck();
                if(nowAngle == 0)
                {
                    direction.Set(0, nowJumpPower);
                }
                else
                {
                    nowAngle *= Mathf.Deg2Rad;
                    direction.Set(nowJumpPower * Mathf.Cos(nowAngle), nowJumpPower * Mathf.Sin(nowAngle));
                }
                
                rb.AddForce(direction, ForceMode2D.Impulse);

            }
            */
            #endregion

            //�^�b�`�����Ƃ��ƃL�����̋�����
            float distance = Vector2.Distance(Physics2D.Raycast((Vector2)ray.origin, (Vector2)ray.direction).point,transform.position);


           //�� Debug.Log($"������������{distance}");

            //�͈͒����Ă�Ȃ玸�s
            if(distance > touchRange)
            {
                if (distance > touchRange * 1.5f)
                {
                    return;
                }
                nowJumpPower = jumpPower * 0.4f;
                
            }

 


            if (distance <= sweetRange)
            {
                nowJumpPower = jumpPower * 1.15f;
            }

            ChangeAction(CharacterState.jump);
            rb.velocity = Vector2.zero;
            JumpCheck();
            if (nowAngle == 0)
            {
                direction.Set(0, nowJumpPower);
            }
            else
            {
                nowAngle *= Mathf.Deg2Rad;
                direction.Set(nowJumpPower * Mathf.Cos(nowAngle), nowJumpPower * Mathf.Sin(nowAngle));
            }

            rb.AddForce(direction, ForceMode2D.Impulse);


        }


    }

    private void OnEnable()
    {
        GroundCheck(true).Forget();
    }



    private void FixedUpdate()
    {

        //  ��ԕω��̎��Ԃ��Ď�
        ConditionTimer();


        if(ScoreManager.instance.isGoal && isGround)
        {
            if(isTrigger > 1)
            {
                return;
            }

            float basePosi = ScoreManager.instance.PlayerPosi.x;

            //�덷�P���Ȃ��Ȃ��~
            if (Math.Abs(basePosi) < 1)
            {
                rb.velocity = Vector2.zero;
            }
            else
            {

                //�E�ɂ��鎞����
                if(basePosi > 0)
                {
                    isTrigger = -1;
                    rb.velocity = Vector2.left * 10;
                }
                else
                {
                    isTrigger = 1;
                    rb.velocity = Vector2.right * 10;
                }
            }

            return;
        }


        //�W�����v���Ȃ�
        if (nowAction == CharacterState.jump)
        {

            //��ɏオ��Ȃ��Ȃ����珉����
            //���ɂ���Q���ɓ���������W�����v���~�Ƃ��H
            if (rb.velocity.y < 0 && Time.time - jumpTime > 1.2f)
            {
                nowAngle = angle;
                nowJumpPower = jumpPower;
                ChangeAction(CharacterState.fall);
            }
        }
        else
        {

            if (isGround)
            {
                ChangeAction(CharacterState.idle);
            }
            else if (rb.velocity.y >= 0 && nowAction != CharacterState.Float)
            {
                ChangeAction(CharacterState.Float);

            }
            else if (nowAction != CharacterState.fall && nowAction != CharacterState.Float)
            {
                ChangeAction(CharacterState.fall);
            }


        }


        if (Mathf.Abs(rb.velocity.y) > 200)
        {
            float ySpeed = rb.velocity.y > 0 ? 200 : -200;

            velocityCOn.Set(rb.velocity.x,ySpeed) ;
            rb.velocity = velocityCOn;
        }
    }

    #region �M�~�b�N

    /// <summary>
    /// �A�C�e���̌��ʂ��L��
    /// </summary>
    /// <param name="data"></param>
    void GimickAct(EventObject.EventData data, Collider2D collision)
    {
        Debug.Log($"tttttt{data.type}");
        //��������
        if (data.effectTime == 0 || data.type == EventObject.EventType.scoreGet)
        {
            if (data.type == EventObject.EventType.damage)
            {
                //���G�Ȃ�
                if (_myStatus.invincible)
                {
                    //���G�̉��炷�H
                    
                    return;
                }

                //�X�R�A��10�ւ�
                ScoreManager.instance.ScoreChange(-100);
                bool isDie = ScoreManager.instance.LifeChange(true);

                StopAction();

                gameObject.layer = 8;

                if (isDie)
                {
                    ScoreManager.instance.isDie = true;
                    _myStatus.Die = true;
                    efCon.ActionChange(CharacterController.CharacterState.none);
                    efCon.ConditionChange(CharacterCondition.die);
                }
                else
                {
                    efCon.ConditionChange(CharacterCondition.damage);
                }
            }
            else if (data.type == EventObject.EventType.recover)
            {
                efCon.PlaySound("Recover", ScoreManager.instance.PlayerPosi);
                ScoreManager.instance.LifeChange();
                //�񕜃G�t�F�N�g
                efCon.ConditionChange(CharacterCondition.heal);
            }

            else if (data.type == EventObject.EventType.scoreGet)
            {
                efCon.PlaySound("ScoreUp",ScoreManager.instance.PlayerPosi);
                ScoreManager.instance.ScoreChange((int)data.effectTime);
            }
            else if (data.type == EventObject.EventType.Random)
            {

            }
        }

        //���Ԏ�������
        else
        {
            //���G�Ȃ�o�b�h�X�e�[�^�X�͎󂯕t���Ȃ�
            if (_myStatus.invincible && data.bad)
            {
                //���G�̉��炷�H
                return;
            }

            ConditionContraller(data);
        }


    }

    void ConditionContraller(EventObject.EventData data)
    {

        

        if (_effectData.Any())
        {
            for (int i = 0; i < _effectData.Count; i++)
            {
                if (data.type == _effectData[i].type)
                {
                 //   Debug.Log($"��dsasdasdwewer{data.type}{_effectData[i].type}{_effectData.Count}");
                    return;
                }
            }
        }

        if (!data.bad)
        {
          //  Debug.Log("wsdaewer");
            if (data.type == EventObject.EventType.invincible)
            {
                _myStatus.invincible = true;
                SpriteEffect.SetActive(true);
              //  Debug.Log($"��er{_myStatus.invincible}");
                //���G�̉��炷?
                MasterAudio.PlaySound3DAtVector3AndForget("Incivle", transform.position);
            }
            else if (data.type == EventObject.EventType.boostJump)
            {
                _myStatus.boostJump = true;
                efCon.BuffChange(ExtraCondition.boostJump);
            }


            /*
                         else if (data.type == EventObject.EventType)
            {

            }
             */
        }
        else
        {
            if (data.type == EventObject.EventType.badSight)
            {
                MasterAudio.PlaySound3DAtVector3AndForget("BadSight", transform.position);
                BeautifySettings.settings.blurIntensity.value = 1.2f;
            }
        }

        data.timer = Time.time;
      //  Debug.Log($"������{data.type}{data.timer}");
        _effectData.Add(data);

    }


    /// <summary>
    /// �N�����ɏ�Ԃ����ɖ߂�
    /// </summary>
    void ConditionRecovery(EventObject.EventData[] dataArray)//, GimickCondition visualData)
    {


        for (int i = 0;i < dataArray.Length; i++)
        {
            if (!dataArray[i].bad)
            {
                //  Debug.Log("wsdaewer");
                if (dataArray[i].type == EventObject.EventType.invincible)
                {
                    _myStatus.invincible = true;
                    SpriteEffect.SetActive(true);
                    //  Debug.Log($"��er{_myStatus.invincible}");
                    //���G�̉��炷?
                    MasterAudio.PlaySound3DAtVector3AndForget("Incivle", transform.position);
                }
                else if (dataArray[i].type == EventObject.EventType.boostJump)
                {
                    _myStatus.boostJump = true;
                    efCon.BuffChange(ExtraCondition.boostJump);
                }

            }
            else
            {
                if (dataArray[i].type == EventObject.EventType.badSight)
                {
                    MasterAudio.PlaySound3DAtVector3AndForget("BadSight", transform.position);
                    BeautifySettings.settings.blurIntensity.value = 1.2f;
                }
            }
            dataArray[i].timer = Time.time;
            //  Debug.Log($"������{dataArray[i].type}{dataArray[i].timer}");
            _effectData.Add(dataArray[i]);
        }


    }




    /// <summary>
    /// ��Ԃ̎��Ԃ��͂���
    /// </summary>
    void ConditionTimer()
    {
        if (_effectData.Any())
        {
            for (int i = 0; i < _effectData.Count; i++)
            {

                //���Ԓ����������
                //�܂��͍����G�ŏ�Ԃ�邢��Ȃ�
                if(Time.time - _effectData[i].timer > _effectData[i].effectTime || _myStatus.invincible && _effectData[i].bad)
                {
                 //   Debug.Log($"wwwdwd{_effectData[i].type}{Time.time - _effectData[i].timer > _effectData[i].effectTime}{_effectData[i].timer}");
                    ConditionEnd(_effectData[i]);
                    _effectData.Remove(_effectData[i]);
                }

            }
        }

    }

    /// <summary>
    /// �����Ԃ��I��点��
    /// </summary>
    void ConditionEnd(EventObject.EventData data)
    {

        if (data.bad)
        {
            if (data.type == EventObject.EventType.badSight)
            {
                BeautifySettings.settings.blurIntensity.value = 0f;
            }
        }
        else
        {
            if (data.type == EventObject.EventType.invincible)
            {
                _myStatus.invincible = false;
                SpriteEffect.SetActive(false);
                gameObject.layer = 0;

            }
            else if (data.type == EventObject.EventType.boostJump)
            {
                _myStatus.boostJump = false;

            }
        }


    }

    public async UniTask DieRecover()
    {

        if (_effectData.Any())
        {
            int count = _effectData.Count;
            for (int i =0;i<count;i++)
            {
                ConditionEnd(_effectData[i]);
            }
        }
        efCon.ConditionChange(CharacterCondition.none);
        await UniTask.Delay(TimeSpan.FromSeconds(1f));
        this.gameObject.layer = 0;
        _myStatus.Die = false;
    }


#endregion

    #region�@�G�t�F�N�g���u�Ƃ̘A�g
    /// <summary>
    /// ��Ԉُ�Ŏ~�܂��Ă��̂����Ƃ�
    /// </summary>
    /// <returns></returns>
    public CharacterState JudgeAction()
    {
        nowAction = lastAction;
        return nowAction;
    }

    //��Ԉُ�Ŏ~�܂�
    public void StopAction()
    {

        if (nowAction == CharacterState.jump)
        {
            nowAction = CharacterState.fall;
        }
        lastAction = nowAction;
        nowAction = CharacterState.none;
        efCon.ActionChange(CharacterState.none);
    }



    public void ChangeAction(CharacterState state)
    {
        if (state == CharacterState.jump)
        {
            jumpTime = Time.time;
        }

        nowAction = state;
        efCon.ActionChange(state);
    }

    #endregion


    /// <summary>
    /// �W�����v�̃X�e�[�^�X���m�F
    /// </summary>
    void JumpCheck()
    {
        if (horizontal != 0)
        {
            nowJumpPower = jumpPower;

            nowAngle = angle;
          //  Debug.Log($"�p�xeedss{nowAngle}");
            if (_status == jumpStatus.add)
            {
                nowAngle *= 1.5f;
            }
            else if (_status == jumpStatus.weak)
            {
                nowAngle *= 0.5f;
                nowJumpPower = jumpPower * 0.6f;
            }
            nowAngle = horizontal > 0 ? nowAngle : 180 - nowAngle;

          //  Debug.Log($"�p�xssss{nowAngle}");
        }
        else
        {
            nowAngle = 0;
            if (_status == jumpStatus.add)
            {
                nowJumpPower = jumpPower * 1.2f;
            }
            else if (_status == jumpStatus.weak)
            {
                nowJumpPower = jumpPower * 0.8f;
            }
        }


        efCon.SetActionAgle(nowAngle);

        if (_myStatus.boostJump)
        {
            nowJumpPower *= 1.5f;
        }
     //   Debug.Log($"�p�x{nowAngle}");
    }












    private void OnTriggerEnter2D(Collider2D collision)
    {
        //�n�`����Ȃ��Ȃ�W�����v��߂�
   //     if (collision.gameObject.tag != "stage")
 //       {
  //          nowAngle = angle;
   //         nowJumpPower = jumpPower;
  //         nowAction = CharacterState.fall;
  //      }

        //�o�t�A�C�e���͈��G��Ə�����?
        //���Őݒ�ɏ]����
        if (collision.gameObject.tag == "Gimick")
        {
            EventObject.EventData data = collision.gameObject.GetComponent<EventObject>().EventStart();


            Debug.Log("����");
            GimickAct(data,collision);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {


        if (ScoreManager.instance.isGoal)
        {
            if(collision.gameObject.tag == "stage")
            {
                isGround = true;
            }
        }

        if (collision.gameObject.tag == "Reflect")
        {

            if (nowAction == CharacterState.jump)
            {
                ChangeAction(CharacterState.fall);
            }


            if (rb.velocity == Vector2.zero)
            {

                direction = Vector2.Reflect((collision.transform.position - transform.position).normalized, collision.contacts[0].normal);

            }
            else
            {
                // �I�u�W�F�N�g�̐i�s�������擾����
                // �Փ˂����ǂ̖@���x�N�g�����擾����
                // ���˂�����������߂�
                direction = Vector2.Reflect(rb.velocity.normalized, collision.contacts[0].normal);
            }

            //���˕Ԃ鉹
            MasterAudio.PlaySound3DAtVector3AndForget("Reflect", transform.position);
         

//            rb.velocity = Vector2.zero;
  //          rb.angularVelocity = 0;

            // ���˂̃x�N�g���ɗ͂�������
            rb.AddForce(direction * reflectForce, ForceMode2D.Impulse);
        }
    }


    async UniTaskVoid GroundCheck(bool isFirst = false)
    {
        if (!isFirst)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(gCheckSpan),cancellationToken:destroyCancellationToken);
        }

        if(gameObject.activeSelf == false)
        {
            return;
        }

        isGround = isGrounded();
        GroundCheck().Forget();
    }

    /// <summary>
    /// �ڒn����
    /// </summary>
    /// <returns></returns>
    private bool isGrounded()
    {
        hit = Physics2D.Raycast(transform.position, Vector2.down, 5, groundLayer);
      //  if(hit.collider!= null)
       // Debug.Log($"��������{hit.collider.name}");
        return hit.collider != null;
    }

    /// <summary>
    /// �Z�[�u�@�\
    /// �K�������ϐ������ˁA������L�[�ŌĂяo���Ďg���΂�����
    /// </summary>
    public override void Save()
    {
        //���̏�ԂƎg�p�G�t�F�N�g�ƍ��̈ʒu���i�[
        //���[�h���K�v�ȏ����͏󋵂̍Č�
        //�ʒu�ݒ�������ł�邩

        //�G�t�F�N�g�f�[�^�̎��Ԃ��m�F������
        for (int i = 0; i < _effectData.Count; i++)
        {

            //���Ԓ����Ă�̂͏���
            //�܂��͍����G�ŏ�Ԃ�邢��Ȃ�
            if (Time.time - _effectData[i].timer > _effectData[i].effectTime)
            {
             //   Debug.Log($"wwwdwd{_effectData[i].type}{Time.time - _effectData[i].timer > _effectData[i].effectTime}{_effectData[i].timer}");
                ConditionEnd(_effectData[i]);
                _effectData.Remove(_effectData[i]);
            }
            else
            {
                EventObject.EventData data = _effectData[i];
                //���ʎ��Ԃ��c�莞�ԂɕύX
                data.effectTime -= (Time.time - _effectData[i].timer);
                _effectData[i] = data;
            }

        }


        ES3.Save("NowCondition", _effectData);
        ES3.Save("NowEffect",_myStatus);
        ES3.Save("PositionImfo", transform.position);
    }



    public override void Load()
    {
        ConditionRecovery(ES3.Load<List<EventObject.EventData>>("NowCondition").ToArray());
     //   ES3.Load("NowEffect");
        transform.position = ES3.Load<Vector3>("PositionImfo");
    }




}