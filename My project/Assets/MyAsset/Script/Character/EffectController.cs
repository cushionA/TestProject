using DarkTonic.MasterAudio;
using PathologicalGames;
using RenownedGames.Apex.Serialization.Collections.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// �K�v�Ȃ̂̓G�t�F�N�g�ƃT�E���h���Ăяo�����߂̕�
/// �A�j���Ǘ�����邩
/// </summary>
public class EffectController : MonoBehaviour
{

    #region�@��`

    [Serializable]
    public class MyParticles : SerializableDictionary<string, ParticleSystem>
    {
        [SerializeField]
        private List<string> keys;

        [SerializeField]
        private List<ParticleSystem> values;

        protected override List<string> GetKeys()
        {
            return keys;
        }

        protected override List<ParticleSystem> GetValues()
        {
            return values;
        }

        protected override void SetKeys(List<string> keys)
        {
            this.keys = keys;
        }

        protected override void SetValues(List<ParticleSystem> values)
        {
            this.values = values;
        }
    }


    NowEffect nowState;

    NowEffect nowCondition;

    NowEffect nowBuff;




    struct NowEffect
    {
        /// <summary>
        /// �񋓌^�ɑΉ����Ă�
        /// </summary>
        public int num;

        /// <summary>
        /// �g�p���̃G�t�F�N�g
        /// </summary>
        public Transform ef;

        


    }


    #endregion


    #region �V���A���C�Y

    [SerializeField]
    SpawnPool pool;



    /// <summary>
    /// �s���G�t�F�N�g���o��
    /// </summary>
    [SerializeField]
    Transform StateEffect;


    /// <summary>
    /// ��ԃG�t�F�N�g���o��
    /// </summary>
    [SerializeField]
    Transform ConditionEffect;

    /// <summary>
    /// �ǉ���ԃG�t�F�N�g���o��
    /// </summary>
    [SerializeField]
    Transform ExtraEffect;

    /// <summary>
    /// ���O�ŃG�t�F�N�g�Ăяo��
    /// </summary>
    [SerializeField]
    MyParticles Particles = new MyParticles();


    [SerializeField, Header("��")]
    [Header("�A�N�V�����̉�")]//small�͂Ђ��Ђ����Ċ����ɂ���H
    [SoundGroup]
    public String[] StateSound;

    [SerializeField, Header("��")]
    [Header("�R���f�B�V�����̉�")]//small�͂Ђ��Ђ����Ċ����ɂ���H
    [SoundGroup]
    public String[] ConditionSound;

    [SerializeField, Header("��")]
    [Header("�o�t�̉�")]//small�͂Ђ��Ђ����Ċ����ɂ���H
    [SoundGroup]
    public String[] ExtraSound;


    /// <summary>
    /// �G�t�F�N�g���g���A�N�V����
    /// </summary>
    [SerializeField]
    CharacterController.CharacterState[] _activeAction;


    /// <summary>
    /// �G�t�F�N�g���g�����
    /// </summary>
    [SerializeField]
    CharacterController.CharacterCondition[] _activeCondition;

    /// <summary>
    /// �G�t�F�N�g���g����ԕω�
    /// </summary>
    [SerializeField]
    CharacterController.ExtraCondition[] _activeEffect;

    #endregion


    #region �����p�����[�^

    Animator anim;

    bool conditionChange;

    protected const string _runningAnimationParameterName = "ActionNum";
    protected int _runningAnimationParameter;

    int animatorParam;

    int animeNum;

    string waitAnime;

    CharacterController _con;

    #endregion


    private void Start()
    {
        anim = GetComponent<Animator>();
        _con = GetComponent<CharacterController>();
        animatorParam = RegisterAnimatorParameter(_runningAnimationParameterName);
        nowCondition.num = (int)CharacterController.CharacterCondition.none;
        nowBuff.num = (int)CharacterController.ExtraCondition.none;

       
 
    }

    private void Update()
    {
        //����
        if(nowCondition.num == 3)
        {
            conditionChange = true;
            animeNum = 15;
            waitAnime = "Die";
        }
        //�_���[�W
        else if (nowCondition.num == 0)
        {
            conditionChange = true;
            animeNum = 14;
            waitAnime = "Damage";
        }
        else
        {
            
            animeNum = nowState.num;
            if (nowState.num == 4)
            {

            }

        }

        UpdateAnimatorInteger(anim,animatorParam,animeNum);

        if (conditionChange)
        {
            if (CheckEnd(waitAnime))
            {
                conditionChange = false;

                //���Ȃ珈���𕪂���
                if (animeNum == 15)
                {
                    ScoreManager.instance.Die();
                }
                else
                {
                    //���G����
                    gameObject.layer = 0;
                    //���̃A�N�V�����ɕ��A����
                    ConditionChange(CharacterController.CharacterCondition.none);
                    nowState.num = (int)_con.JudgeAction();
                }
            }


        }





    }



    #region  �G�t�F�N�g�Ăяo��



    public void SetActionAgle(float angle)
    {
        if (angle == 0)
        {
            angle = 270;
        }
        else
        {
            angle = angle > 0 ? 270 + (90 - angle) : 270 - (90 + angle);
        }




        // StateEffect.rotation = Quaternion.Euler(new Vector3(0,0,(angle + 180) % 360));
        StateEffect.rotation = Quaternion.Euler(angle,90,-90);
     //   Debug.Log($"ewfwefwe{StateEffect.rotation}tototo{angle}");
    }


    /// <summary>
    /// �A�N�V�����ύX���ɃG�t�F�N�g���Ăяo��
    /// </summary>
    public void ActionChange(CharacterController.CharacterState state)
    {
        if (conditionChange)
        {
            return;
        }



        if (state != CharacterController.CharacterState.none)
        {
            int number = (int)state;
            

            if (nowState.ef != null)
            {
                pool.Despawn(nowState.ef);
                StopSound(StateSound[nowState.num]);
                nowState.ef = null;
            }

            bool isEffect = false;

            for (int i = 0;i < _activeAction.Length;i++)
            {
                if (state == _activeAction[i])
                {
                    isEffect = true;
                    break;
                }

            }
            if (isEffect)
            {

                //�t���[�g�������ȊO
                if (number == 1)
                {
                  //  Debug.Log($"������{state}");
                    nowState.ef = MySpawn(Particles[StateSound[number]], StateEffect).transform;
                    //������
                    FollowSound(StateSound[number], transform);
                }
                else
                {
                    MySpawn(Particles[StateSound[number]], StateEffect);
                    PlaySound(StateSound[number], transform.position);
                    nowState.ef = null;


                }
            }
            nowState.num = number;
        }
        else
        {
            if (nowState.ef != null)
            {
                pool.Despawn(nowState.ef);
                StopSound(StateSound[nowState.num]);
                nowState.ef = null;
            }
            nowState.num = 4;
           // _con.StopAct();
        }
    }


    /// <summary>
    /// �R���f�B�V�����ύX���ɃG�t�F�N�g���Ăяo��
    /// �m���ŏ�ԃL�����Z��
    /// </summary>
    public void ConditionChange(CharacterController.CharacterCondition state)
    {

        if (state != CharacterController.CharacterCondition.none)
        {

            int number = (int)state;

            if (nowCondition.ef != null)
            {
                pool.Despawn(nowCondition.ef);
                StopSound(StateSound[nowCondition.num]);
                nowCondition.ef = null;
            }

            bool isEffect = false;

            for (int i = 0; i <= _activeCondition.Length - 1; i++)
            {
                if (state == _activeCondition[i])
                {
                    isEffect = true;
                    break;
                }

            }

            if (isEffect)
            {

                    MySpawn(Particles[StateSound[number]], StateEffect);
                    PlaySound(StateSound[number], transform.position);
                    nowCondition.ef = null;

            }
            
            nowCondition.num = number;
        }

        else
        {
            nowCondition.num = 4;
        }
    }


    /// <summary>
    /// �o�t�ύX���ɃG�t�F�N�g���Ăяo��
    /// �D�揇�ŃG�t�F�N�g�㏑������
    /// ���݂̃G�t�F�N�g����������͑��Ƀo�t���Ȃ������ׂ�
    /// </summary>
    public void BuffChange(CharacterController.ExtraCondition state)
    {

        if (state != CharacterController.ExtraCondition.none)
        {

            int number = (int)state;

            bool isEffect = false;

            for (int i = 0; i < _activeEffect.Length; i++)
            {
                if (state == _activeEffect[i])
                {
                    isEffect = true;
                    break;
                }

            }

            if (nowBuff.ef != null)
            {
                //none�ȊO�ŗD��x��̏�ԕω��Ȃ�
                if ( number < nowBuff.num)
                {
                    pool.Despawn(nowBuff.ef);
                    StopSound(ExtraSound[nowBuff.num]);
                    nowBuff.ef = null;
                    nowBuff.num = number;
                }

                //�D��x�Ⴂ�Ȃ�G�t�F�N�g���g�킹�Ȃ����i���o�[���㏑�����Ȃ�
                else
                {
                    isEffect = false;
                }
            }





            if (isEffect)
            {

                //nowBuff.ef = MySpawn(Particles[ExtraSound[number]],  ExtraEffect).transform;
                PlaySound(ExtraSound[number], transform.position);
                

            }
        }
    }



    /// <summary>
    /// �o�t�ύX���ɃG�t�F�N�g���Ăяo��
    /// �D�揇�ŃG�t�F�N�g�㏑������
    /// ���݂̃G�t�F�N�g����������͑��Ƀo�t���Ȃ������ׂ�
    /// </summary>
    public void BuffEnd(CharacterController.ExtraCondition state,CharacterController.ExtraCondition sub = CharacterController.ExtraCondition.none)
    {

        if (state != CharacterController.ExtraCondition.none)
        {


            //�g�p���̃G�t�F�N�g������A���݈�ԗD��x�����G�t�F�N�g���D��x�����Ȃ����
            if (nowBuff.ef != null && sub > state)
            {
                pool.Despawn(nowBuff.ef);
                StopSound(ExtraSound[nowBuff.num]);
                nowBuff.ef = null;


            }

            if (sub != CharacterController.ExtraCondition.none && sub != state)
            {
                //�����ŕʂɗD��x�Ⴂ�G�t�F�N�g������Ȃ炻�����g��
                //nowBuff.num = number;
                BuffChange(sub);
            }

        }

    }


    public ParticleSystem MySpawn(ParticleSystem prefab, Transform parent)
    {
        if(prefab == null)
        {
            return null;
        }
        return pool.Spawn(prefab, parent.position, parent.rotation, parent);
    }


    #endregion

    #region�@��
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

    public void PlaySound(string sType, Vector3 sourceTrans, float volumePercentage = 1f, float? pitch = null, float delaySoundTime = 0f, string variationName = null, double? timeToSchedulePlay = null, bool isRemember = false)
    {
        // Debug.Log("������");
        if (isRemember)
        {
            MasterAudio.PlaySound3DAtVector3(sType, sourceTrans, volumePercentage, pitch, delaySoundTime, variationName, timeToSchedulePlay);
        }
        else
        {
            MasterAudio.PlaySound3DAtVector3AndForget(sType, sourceTrans, volumePercentage, pitch, delaySoundTime, variationName, timeToSchedulePlay);
            
        }

    }
    /// <summary>
    /// �����Đ��B�����ɒǐ�����
    /// </summary>
    /// <param name="sType">�Đ����鉹�̖��O�B�o���G�[�V��������̂�B
    /// <param name="sourcePosition">����炵�����ʒu�B�K�{�ł��B </param>
    /// <param name="volumePercentage"><b>Optional</b> - ���ʂ������čĐ��������ꍇ�Ɏg�p���܂��i0�`1�̊ԁj�B
    /// <param name="pitch"><b>Optional</b> - ����̃s�b�`�ŉ����Đ��������ꍇ�Ɏg�p���܂��B</param> <param name="pitch"><b>Optional</b> - ����̉����ōĐ��������ꍇ�Ɏg�p���܂��B��������ƁA�o���G�[�V�����̒���pich��random pitch���㏑�����܂��B
    /// <param name="delaySoundTime"><b>Optional</b> - �����ɂł͂Ȃ��AX�b��ɉ���炵�����ꍇ�Ɏg�p���܂��B
    /// <param name="variationName"><b>Optional</b> - ����̃o���G�[�V�����i�܂��̓N���b�vID�j�̖��O�ōĐ��������ꍇ�Ɏg�p���܂��B����ȊO�̏ꍇ�́A�����_���ȃo���G�[�V�������Đ�����܂��B
    /// <param name="timeToSchedulePlay"><b>Optional</b> - �T�E���h���Đ����邽�߂�DSP���Ԃ�n�����߂Ɏg�p���܂��B�ʏ�͂�����g�p�����A�����delaySoundTime�p�����[�^���g�p���܂��B
    ///     /// <param name="isRemember"><b>Optional</b> - PlaySoundResult���擾���邩�ǂ����B
    /// <returns>PlaySoundResult - ���̃I�u�W�F�N�g�́A�T�E���h���Đ����ꂽ���ǂ�����ǂݎ�邽�߂Ɏg�p����A�g�p���ꂽVariation�I�u�W�F�N�g�ւ̃A�N�Z�X���\�ł��B
    public void FollowSound(string sType, Transform sourceTrans, float volumePercentage = 1f, float? pitch = null, float delaySoundTime = 0f, string variationName = null, bool isRemember = false)
    {

        if (isRemember)
        {
            MasterAudio.PlaySound3DFollowTransform(sType, sourceTrans, volumePercentage, pitch, delaySoundTime, variationName);
        }
        else
        {
            MasterAudio.PlaySound3DFollowTransformAndForget(sType, sourceTrans, volumePercentage, pitch, delaySoundTime, variationName);
        }
    }


    /// <summary>
    /// �Ō��true�ɂ��Ȃ��Ȃ�t�F�[�h����{
    /// </summary>
    /// <param name="soundGroupName"></param>
    /// <param name="fadeTime"></param>
    /// <param name="isStop"></param>
    public void StopSound(string soundGroupName, float fadeTime = 1, bool isStop = false)
    {
        if (!isStop)
        {
            MasterAudio.FadeOutAllOfSound(soundGroupName, fadeTime);
        }
        else
        {
            MasterAudio.StopAllOfSound(soundGroupName);
        }
    }



    public void AnimSound(int index = 0)
    {
        //�_���[�W�Ȃ�
        if (nowCondition.num == 0)
        {
            PlaySound("Damage",transform.position);
        }
        else if (nowCondition.num == 3)
        {
            if (index == 0)
            {
                PlaySound("Damage",transform.position);
            }
            else
            {
                PlaySound("Die", transform.position);
            }

           
        }

    }

    #endregion





    #region�@�A�j���Ǘ�

    /// <summary>
    /// Updates the animator integer.
    /// </summary>
    /// <param name="animator">Animator.</param>
    /// <param name="parameter">Parameter name.</param>
    /// <param name="value">Value.</param>
    public static bool UpdateAnimatorInteger(Animator animator, int parameter, int value)
    {
      //  Debug.Log($"����{value}");
        animator.SetInteger(parameter, value);
       // Debug.Log($"eqww{animator.GetInteger(parameter)}");
        return true;
    }

    /// <summary>
    /// Registers a new animator parameter to the list
    /// MyCharacter�ɓK��������I�[�o�[���C�h
    /// </summary>
    /// <param name="parameterName">Parameter name.</param>
    /// <param name="parameterType">Parameter type.</param>
    public int RegisterAnimatorParameter(string parameterName)
    {
        return Animator.StringToHash(parameterName);
    }

    bool CheckEnd(string Name)
    {

        if (!anim.GetCurrentAnimatorStateInfo(0).IsName(Name))// || sAni.GetCurrentAnimatorStateInfo(0).IsName("OStand"))
        {   // �����ɓ��B�����normalizedTime��"Default"�̌o�ߎ��Ԃ��E���Ă��܂��̂ŁAResult�ɑJ�ڊ�������܂ł�return����B
            return false;
        }
        if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {  

                return false;
        }

        return true;


    }

    #endregion



}
