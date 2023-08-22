using Cysharp.Threading.Tasks;
using RenownedGames.Apex.Serialization.Collections.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using static bl_SceneLoader;


/// <summary>
/// ���݂ǂ̃}�b�v�̂ǂ̃Z�O�����g�ɂ��邩�Ƃ���ێ�����X�N���v�g
/// 
/// �l���邱��
/// 
/// �E�N�����ɑO��Ō�ɗ����Ă����}�b�v����ǂ�����ă��[�h���邩
/// �i����:���ݗ����Ă�Z�O�����g�̋N���I�u�W�F�N�g���v�[�����Ă����B�����Ă����̖��O���烍�[�h����H�j
/// �i���ꂩ���ݗ����Ă�I�u�W�F�N�g��LevelController���烁�\�b�h�Ăяo���H�R���g���[���[�ɑO�̃V�[���Ǝ��̃V�[�����������Ă����Ă��B���ꂪ���������j
/// 
/// �E���x�����ǂ�����ăA�����[�h���邩�H
/// �i���x���R���g���[���[�Ō��݂̃Z�O�����g���ς�����炻�̎��ɑO�̃Z�O�����g�⎟�̃Z�O�����g����������悤�Ȏd�g�ݍ��H�j
/// 
/// �E���̃}�b�v�̍ŏ��̃Z�O�����g�A�ԍ��O�̃Z�O�����g�������肵����O�̃Z�O�����g�Ƃ��ǂ�����H
/// �i�O�̂����߂�Ƃ̖��O�̂Ƃ���ɒʘH�Ԃ�����ǂ����A�O�̏ꍇ�̓}�b�v�ԍ�����ʘH�𓱂������H�j
/// 
/// �E���̃}�b�v�Ƃ��O�̃}�b�v�ɂ܂�����ʘH�Ƃ����Ɠ����ɃV�[���𕡐����݂����Ȃ��ƃ_������ˁB
/// ��{�ǉ��V�[���͑S���I�u�W�F�N�g���������Ă����ĕۑ����邩
/// 
/// �O�̃��x���ɍs���ɂ͑O�̃V�[�����Ăяo���Ĉ�����L�����ŏ������ς���Ă���
/// ���̃R���g���[���[���N��������O��
/// 
/// 

/// 
/// </summary>
public class LevelManager : MonoBehaviour,SaveInterface
{
    

    [Serializable]
    public class NextSceneName : SerializableDictionary<Vector2Int, string>
    {
        [SerializeField]
        private List<Vector2Int> keys;

        [SerializeField]
        private List<string> values;

        protected override List<Vector2Int> GetKeys()
        {
            return keys;
        }

        protected override List<string> GetValues()
        {
            return values;
        }

        protected override void SetKeys(List<Vector2Int> keys)
        {
            this.keys = keys;
        }

        protected override void SetValues(List<string> values)
        {
            this.values = values;
        }
    }

        [ES3NonSerializable]
    public static LevelManager instance;

    /// <summary>
    /// ���݂ǂ̃}�b�v�̂ǂ̃Z�O�����g�ɂ��邩
    /// </summary>
    Vector2Int nowPointer;

    /// <summary>
    /// ���݂���}�b�v�̃f�[�^
    /// �ʘH�Ő؂�ւ���A����ł�����
    /// ���Ƃ�Q�[�̏ꍇ�̓`�F�b�N�|�C���g��
    /// </summary>
 //   [HideInInspector]
    public LevelData nowData;

    [Header("�}�b�v�̖��O")]
    /// <summary>
    /// ���݂̈ʒu����q���鎟�̃}�b�v�̖��O���擾
    /// </summary>
    [SerializeField]
    [Header("���ݒn���玟�̃}�b�v�̖��O���l��")]
    NextSceneName nameIndex;

    /// <summary>
    /// ���[�h�̋����Ǘ�
    /// </summary>
    AsyncOperation container;


    AsyncOperationHandle<SceneInstance> containerA;

    [SerializeField]
    [Header("Addressables���g�p���ăV�[���Ǘ������邩")]
    bool isAddressable;

    /// <summary>
    /// �Q�[�����[�h���珉��N�����ꂽ�Z�O�����g�ł��邩�ǂ���
    /// ��x����g�����
    /// �ŏ��̃Z�O�����g�͂��낢���邱�Ƃ�����
    /// �U�̎������ŏ�
    /// </summary>
    [HideInInspector]
    public bool isFirst;

 //   SaveManager _save;

    /// <summary>
    /// ���x���f�[�^�̃��X�g
    /// �����̓}�b�v�|�C���^�[��X�ƘA������悤��
    /// </summary>
    [SerializeField]
    [Header("�}�b�v�f�[�^�̊i�[")]
    LevelData[] dataList;


    // Start is called before the first frame update
    void Awake()
    {

        if (instance == null)
        {
            instance = this;

        }
        else
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
        Load();
    }



    /// <summary>
    /// �ŏ��̃��x�������[�h����
    /// ���x���f�[�^�̃}�b�v�̖��O����V�[�������[�h
    /// ����Ń��x���f�[�^�̃Z�O�����g�̖��O����Z�O�����g�����[�h
    /// </summary>
    public void FirstLevelLoad()
    {
        //���łɃ��[�h����Ă��Ȃ��Ȃ烍�[�h
        //���̏�����Addressables������
        if (SceneManager.GetSceneByName(nowData.mapName).IsValid() == false)
        {

            if (!isAddressable)
            {
                //���[�h����V�[�����Ɠ����ł��邽�߁Agameobject�����g���ăV�[�������[�h����B
                //   container = SceneManager.LoadSceneAsync(nameIndex[nowPointer], LoadSceneMode.Additive);
               // container = SceneManager.LoadSceneAsync("Test!", LoadSceneMode.Additive);

                //���̃f�[�^�̃}�b�v�̖��O���烍�[�h
                container = SceneManager.LoadSceneAsync(nowData.mapName, LoadSceneMode.Additive);

                //We set it to true to avoid loading the scene twice

                //�����̓V�[�����[�_�[���狖�����o����\��
                container.allowSceneActivation = false;
            }
            else
            {
                //activateOnLoad�i�����ltrue�j��False�ɂ��邱�Ƃ�Activate���\�b�h�Ă΂��܂ł͎��̉����Ȃ��B
                containerA = Addressables.LoadSceneAsync(nowData.mapName, LoadSceneMode.Additive, activateOnLoad : false);
                //containerA.Result.ActivateAsync
            }

        }
    }


    /// <summary>
    /// ���x�������[�h����
    /// ���[�h����͂������ǂǂ̃Z�O�����g��L���ɂ��邩�͔Y�ނƂ���
    /// �ʘH�ɂ��������@�\���������邩�A
    /// �Q�[���I�u�W�F�N�g��o�^���Ă�
    /// </summary>
    public void LoadLevel()
    {
        //���łɃ��[�h����Ă��Ȃ��Ȃ烍�[�h
        if (SceneManager.GetSceneByName(nameIndex[nowPointer]).IsValid() == false)
        {
            if (!isAddressable)
            {
                //���[�h����V�[�����Ɠ����ł��邽�߁Agameobject�����g���ăV�[�������[�h����B
                SceneManager.LoadSceneAsync(nameIndex[nowPointer], LoadSceneMode.Additive);
                //We set it to true to avoid loading the scene twice

            }
            else
            {
                //���[�h����V�[�����Ɠ����ł��邽�߁Agameobject�����g���ăV�[�������[�h����B
                Addressables.LoadSceneAsync(nameIndex[nowPointer], LoadSceneMode.Additive);
                //We set it to true to avoid loading the scene twice
            }


        }
    }

    /// <summary>
    /// ���x�����A�����[�h����A���ƃ��[�h���Ȃ烍�[�h���~
    /// </summary>
    public void UnLoadLevel()
    {
        if (!isAddressable)
        {
            SceneManager.UnloadSceneAsync(nameIndex[nowPointer]);
        }
    }



    /// <summary>
    /// �f�[�^���A�b�v�f�[�g����
    /// �g���K�[��ʂ�߂������ɃA�b�v�f�[�g
    /// </summary>
    /// <param name="pointer"></param>
    /// <param name="isNext"></param>
    /// <param name="isFinal"></param>
    public void DataUpdate(Vector2Int pointer)
    {
        nowPointer.x = pointer.x;
        nowPointer.y = pointer.y;

    }


    /// <summary>
    /// �V�[�����[�_�[����T�u�V�[�����Ă�ł�����
    /// </summary>
    public async UniTask SubSceneCall()
    {
        if (!isAddressable)
        {
            container.allowSceneActivation = true;
           // container = null;
        }
        else
        {
            await containerA.Result.ActivateAsync();
        }
        //�����ŃZ�O�����g�̗L�����܂�

        await FirstSegmentActive();
    }

    /// <summary>
    /// �V�[�����[�_�[����T�u�V�[�����Ă�ł�����
    /// </summary>
    public float SubSceneProgress()
    {
        if (!isAddressable)
        {
            return container.progress / 2;

        }
        else
        {
            return containerA.PercentComplete / 2;
        }
    }

        /// <summary>
        /// �Z�[�u�@�\
        /// �K�������ϐ������ˁA������L�[�ŌĂяo���Ďg���΂�����
        /// </summary>
        public void Save()
    {
        //���A���ɂ�鏈��
        //�܂��f�[�^���}�l�[�W���[�ɖ߂��B�v���C���[���ʒu�ύX�i�����܂�Awake�j

        //�}�b�v�̍��̃}�b�v�ƃZ�O�����g�f�[�^�ɏ]���ă}�b�v���[�h
        //�Z�O�����g�L����
        //�}�b�v�̈��̃I�u�W�F�N�g�����������L�����ǂ����������̔ԍ��Ŗ₢���킹�Đ^�U�^�ŏ�����iDestroy��disena���͎����Ō��߂�j
        //�����܂�Start

        ES3.Save( "MapImfo",nowData);
        ES3.Save("SegmentImfo", nowPointer);

    }

    /// <summary>
    /// �Z�[�u�f�[�^�����[�h����
    /// 
    /// 
    /// </summary>
    public void Load()
    {

        // ES3.Load("MapImfo", nowData); �݂����Ƀ��[�h����΃f�[�^�����݂��Ȃ��Ƃ���nowdata�ϐ��̓��e��Ԃ��Ă����B
        //�ł��Z�[�u�f�[�^�쐬���Ďn�߂�͂������炢��Ȃ���
       nowPointer = ES3.Load<Vector2Int>("SegmentImfo");
            nowData = dataList[nowPointer.x];

       SetData(ES3.Load<LevelData>("MapImfo"));
    }

    /// <summary>
    /// ���x���f�[�^�̐��l����
    /// </summary>
    void SetData(LevelData data)
    {
        if(data == null)
        {
            return;
        }

        int count = nowData.disenableEnemy.Length;

        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                nowData.disenableEnemy[i] = data.disenableEnemy[i];
            }

        }

        count = nowData.disenableObjects.Length;
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                nowData.disenableObjects[i] = data.disenableObjects[i];
            }

        }

    }


    /// <summary>
    /// �V�[�������[�h�����܂ő҂��ăZ�O�����g��L���ɂ���
    /// 
    /// ����Ƃ��Ă̓V�[�����[�_�[�ŃV�[�������[�h�����A���[�h���ꂽ�炱�����̒ǉ��V�[�������[�h
    /// �����ăZ�[�u�f�[�^�����[�h�A�Z�O�����g�L��������i�Z�O�����g����̃I�u�W�F�N�g���������ǂ����̖₢���킹�������j
    /// </summary>
    /// <returns></returns>
    async UniTask FirstSegmentActive()
    {
        if (!isAddressable)
        {
            Debug.Log($"{container ==null}");
            //�V�[�������[�h�����܂ő҂�
            await UniTask.WaitUntil(() => container.isDone);
            container = null;
        }
        else
        {
            //�V�[�������[�h�����܂ő҂�
            await UniTask.WaitUntil(() => containerA.IsDone);
        }

        await ScoreManager.instance.gameObject.GetComponent<SaveManager>().LoadAsync();

        //�����ŃZ�[�u���[�h�ǂݍ��ݒ��c���ăe�L�X�g���o�����Ƃ��\
       // await _save.LoadAsync();

        //���[�h���ꂽ��Z�O�����g��L����
        //�}�b�v�f�[�^��Awake�ł��łɗp�ӂ��Ă���
        GameObject.Find(nowData.segmentName[nowPointer.y]).SetActive(true);


    }

    /// <summary>
    /// �I�u�W�F�N�g����ꂽ�炻�̎|��m�点��
    /// </summary>
    /// <param name="ID"></param>
    public void ObjectBreak(int ID)
    {
        nowData.disenableObjects[ID] = true;
    }

    /// <summary>
    /// ���̃I�u�W�F�N�g�����Ă邩�ǂ����m�点��
    /// </summary>
    /// <param name="ID"></param>
    /// <returns></returns>
    public bool BreakCheck(int ID)
    {
        return nowData.disenableObjects[ID];
    }


    


}
