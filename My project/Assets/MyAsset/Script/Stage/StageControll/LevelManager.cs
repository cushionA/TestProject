using Cysharp.Threading.Tasks;
using RenownedGames.Apex.Serialization.Collections.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
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
public class LevelManager : SaveMono
{
    

    [Serializable]
    public class NextSceneName : SerializableDictionary<Vector2Int, int>
    {
        [SerializeField]
        private List<Vector2Int> keys;

        [SerializeField]
        private List<int> values;

        protected override List<Vector2Int> GetKeys()
        {
            return keys;
        }

        protected override List<int> GetValues()
        {
            return values;
        }

        protected override void SetKeys(List<Vector2Int> keys)
        {
            this.keys = keys;
        }

        protected override void SetValues(List<int> values)
        {
            this.values = values;
        }
    }

        [ES3NonSerializable]
    public static LevelManager instance;

    [SerializeField]
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


 //   AsyncOperationHandle<SceneInstance> containerA;

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

    /// <summary>
    /// ���[�h���Ԃ͂����
    /// </summary>
    float timeChecek;






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
        UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false;
    }





    #region �}�b�v���[�h�ƃA�����[�h




    /// <summary>
    /// ���x�������[�h����
    /// ���[�h����͂������ǂǂ̃Z�O�����g��L���ɂ��邩�͔Y�ނƂ���
    /// �ʘH�ɂ��������@�\���������邩�A
    /// �Q�[���I�u�W�F�N�g��o�^���Ă�
    /// </summary>
    public async UniTaskVoid LoadLevel(int num)
    {
        //�����ׂ��}�b�v�Ȃ��Ȃ��߂Ƃ�
        if (nameIndex.ContainsKey(nowPointer) == false || dataList[nameIndex[nowPointer]] == null)
        {
            return;
        }


        string name = dataList[nameIndex[nowPointer]].mapName;
        //���łɃ��[�h����Ă��Ȃ��Ȃ烍�[�h
        if (SceneManager.GetSceneByName(name).IsValid() == false)
        {

            //������Await���ă}�b�v�̃��[�h�҂Ԍ����Ȃ��Ǐo�����肵�Ă�������
          
            if (!isAddressable)
            {
                //���[�h����V�[�����Ɠ����ł��邽�߁Agameobject�����g���ăV�[�������[�h����B
               await SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
                //We set it to true to avoid loading the scene twice

            }
            else
            {
                //���[�h����V�[�����Ɠ����ł��邽�߁Agameobject�����g���ăV�[�������[�h����B
                await Addressables.LoadSceneAsync(name, LoadSceneMode.Additive);
                //We set it to true to avoid loading the scene twice
            }

         //    await UniTask.Delay(TimeSpan.FromSeconds(10));


            //�Z�O�����g�N��
            GameObject.Find($"{name}SceneRoot").transform.Find($"{name}Segment{num}").gameObject.SetActive(true);

 

         //  Debug.Log($"���܂��H{obj != null}�@�����Ă�H{obj.activeSelf}�@�����O��{obj.name}");
        }
    }


    /// <summary>
    /// ���x�����A�����[�h����A���ƃ��[�h���Ȃ烍�[�h���~
    /// </summary>
    public void UnLoadLevel()
    {
        if (!isAddressable)
        {
            //�����ׂ��}�b�v�Ȃ��Ȃ��߂Ƃ�
            if (nameIndex.ContainsKey(nowPointer) == false || dataList[nameIndex[nowPointer]] == null)
            {
                return;
            }

            SceneManager.UnloadSceneAsync(dataList[nameIndex[nowPointer]].mapName);
            //�g���ĂȂ��A�Z�b�g�A�����[�h
            Resources.UnloadUnusedAssets();
        }
    }

    /// <summary>
    /// �V�[�����[�_�[�ɃT�u�V�[���̃��[�h����������
    /// </summary>
    public float SubSceneProgress()
    {
        if (!isAddressable)
        {
            return container.progress + 0.1f / 2;

        }
        else
        {
            return container.progress + 0.1f / 2;
            // return containerA.PercentComplete + 0.1f / 2;
        }
    }

    /// <summary>
    /// �V�[�����[�_�[����T�u�V�[�����Ă�ł�����
    /// ���C���V�[�������[�h�ł����炻���ɃT�u�V�[�����[�h����
    /// �Z�O�����g��L����
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
            //  Debug.Log($"{containerA.DebugName}");
            //   await containerA.Result.ActivateAsync();
        }
        //�����ŃZ�O�����g�̗L�����܂�
        Debug.Log($"����");
        await FirstSegmentActive();
    }

    /// <summary>
    /// �ŏ��̃��x�������[�h����
    /// ���x���f�[�^�̃}�b�v�̖��O����V�[�������[�h
    /// ����Ń��x���f�[�^�̃Z�O�����g�̖��O����Z�O�����g�����[�h
    /// </summary>
    public void FirstLevelLoad()
    {


        //��[�ǂ͂����ł�邩��}�l�[�W���ɂ܂Ƃ߂Ȃ��ł�
        Load();



        //�����ōŏ��̃}�b�v�͑f��ɕύX���������Ă邱�Ƃ�

        nowData.isChanged = true;

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
             //   containerA = Addressables.LoadSceneAsync(nowData.mapName, LoadSceneMode.Additive, activateOnLoad : false);
                //containerA.Result.ActivateAsync

               
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

            //�V�[�������[�h�����܂ő҂�
            await UniTask.WaitUntil(() => container.isDone);

        }
        else
        {
            //�V�[�������[�h�����܂ő҂�
            //  await UniTask.WaitUntil(() => containerA.IsDone);
        }



        await ScoreManager.instance.gameObject.GetComponent<SaveManager>().LoadAsync();


        //�����ŃZ�[�u���[�h�ǂݍ��ݒ��c���ăe�L�X�g���o�����Ƃ��\
        // await _save.LoadAsync();

        //���[�h���ꂽ��Z�O�����g��L����
        //�}�b�v�f�[�^��Awake�ł��łɗp�ӂ��Ă���
        //SegmentName���K�v�ȗ��R�͕ʂ̃}�b�v�̃Z�O�����g0����Ƃ����N�����Ⴄ�\�����邩��
        //�}�b�v���ŗǂ���
        //   Debug.Log($"{nowData.mapName}Segment{nowPointer.y}");


        //��A�N�e�B�u�ȃI�u�W�F�N�g�̓��[�g�I�u�W�F�N�g�̃g�����X�t�H�[�����炽�ǂ��ĒT����
        SegmentSerch(nowPointer).SetActive(true);
            container = null;
        //�g���ĂȂ��A�Z�b�g�A�����[�h
        await Resources.UnloadUnusedAssets();

    }

    /// <summary>
    /// �}�b�v��񂩂�Z�O�����g�T���Ă���
    /// </summary>
    /// <returns></returns>
    public GameObject SegmentSerch(Vector2Int pointer)
    {
        string name = dataList[pointer.x].mapName;
        return GameObject.Find($"{name}SceneRoot").transform.Find($"{name}Segment{pointer.y}").gameObject;
    }



    #endregion



    #region �ʒu���Ǘ�

    /// <summary>
    /// �f�[�^���A�b�v�f�[�g����
    /// �g���K�[��ʂ�߂������ɃA�b�v�f�[�g
    /// </summary>
    /// <param name="pointer"></param>
    /// <param name="isNext"></param>
    /// <param name="isFinal"></param>
    public void DataUpdate(Vector2Int pointer)//,bool final)
    {
        //�}�b�v�ԍ��ς���Ă�̂Ȃ�
        if (nowPointer.x != pointer.x)
        {
            nowData = dataList[pointer.x];

            //��}�b�v���ɕω��A�����Ă��Ƃ�

            nowData.isChanged = true;
        }

        if (pointer.x == 1 && pointer.y == 0) { 
            Debug.Log($"�X�V{pointer}");
    }
        nowPointer.x = pointer.x;
        nowPointer.y = pointer.y;

    }

    /// <summary>
    /// ���݂ǂ̃Z�O�����g�ɂ��邩��������
    /// </summary>
    /// <returns></returns>
    public int GetSegment(bool isHorizon = false)
    {
        if (!isHorizon)
            return nowPointer.y;
        else
            return nowPointer.x;
    }


    #endregion


    #region�@�f�[�^�֘A

    /// <summary>
    /// �Z�[�u�@�\
    /// �K�������ϐ������ˁA������L�[�ŌĂяo���Ďg���΂�����
    /// </summary>
    public override void Save()
    {
        //���A���ɂ�鏈��
        //�܂��f�[�^���}�l�[�W���[�ɖ߂��B�v���C���[���ʒu�ύX�i�����܂�Awake�j

        //�}�b�v�̍��̃}�b�v�ƃZ�O�����g�f�[�^�ɏ]���ă}�b�v���[�h
        //�Z�O�����g�L����
        //�}�b�v�̈��̃I�u�W�F�N�g�����������L�����ǂ����������̔ԍ��Ŗ₢���킹�Đ^�U�^�ŏ�����iDestroy��disena���͎����Ō��߂�j
        //�����܂�Start

        ES3.Save( "MapImfo",dataList);
        ES3.Save("SegmentImfo", nowPointer);

    }

    /// <summary>
    /// �V�����f�[�^�����
    /// </summary>
    public void NewSave(bool isClear = false)
    {
        if (!isClear)
        {
            ES3.Save<int>("PlayTime", 0);
            ES3.Save<int>("Score", 0);
            ES3.Save<int>("Life", 5);
            ES3.Save<int>("BestScore", 0);

            //  �L�����R���g���[���[�̏�����
            ES3.Save("NowCondition", new List<EventObject.EventData>());
            // ES3.Save("NowEffect", new GimickCondition());
            ES3.Save("PositionImfo", Vector3.zero);
        }
        //���A���ɂ�鏈��
        //�܂��f�[�^���}�l�[�W���[�ɖ߂��B�v���C���[���ʒu�ύX�i�����܂�Awake�j
        ES3.Save("SegmentImfo", Vector2Int.zero);

       //�d�l�̓}�b�v�̍��̃}�b�v�ƃZ�O�����g�f�[�^�ɏ]���ă}�b�v���[�h
        //�Z�O�����g�L����
        //�}�b�v�̈��̃I�u�W�F�N�g�����������L�����ǂ����������̔ԍ��Ŗ₢���킹�Đ^�U�^�ŏ�����iDestroy��disena���͎����Ō��߂�j
        //�����܂�Start
        MapDataCleaner(); 
        //�X�R�A�}�l�[�W���[�̏�����
�@�@�@
        dataList[0].isChanged = true;
�@ES3.Save("MapImfo",  dataList);
        //ES3.Save("MapImfo", dataList);

    }







    /// <summary>
    /// �}�b�v�̃f�[�^����
    /// �Z�[�u���ăQ�[���I���O��A�f�[�^���[�h�������ɕύX���������}�b�v�Ɋւ��Ă͂܂�����ɖ߂��Ă���
    /// ��������ƃZ�[�u�������_����̕ύX�������Ă܂�����ɂł���
    /// </summary>
    void MapDataCleaner()
    {
        int count = dataList.Length;

        for (int i = 0; i < count; i++)
        {
            if (dataList[i].isChanged)
            {

                //�I�u�W�F�N�g�ƓG���ēx�L����
                int count2 = dataList[i].disenableObjects.Length;
                for (int s = 0; s < count2; s++)
                {
                    dataList[i].disenableObjects[s] = false;
                }

                count2 = dataList[i].disenableEnemy.Length;
                for (int s = 0; s < count2; s++)
                {
                    dataList[i].disenableEnemy[s] = false;
                }
            }

                dataList[i].isChanged = false;

        }

    }


    /// <summary>
    /// �Z�[�u�f�[�^�����[�h����
    /// 
    /// 
    /// </summary>
    public override void Load()
    {
        MapDataCleaner();
        // ES3.Load("MapImfo", nowData); �݂����Ƀ��[�h����΃f�[�^�����݂��Ȃ��Ƃ���nowdata�ϐ��̓��e��Ԃ��Ă����B
        //�ł��Z�[�u�f�[�^�쐬���Ďn�߂�͂������炢��Ȃ���
        nowPointer = ES3.Load<Vector2Int>("SegmentImfo");
        LevelData[] data = ES3.Load<LevelData[]>("MapImfo");


     SetData(data);
    }

    /// <summary>
    /// ���x���f�[�^�̃��[�h���̐��l����
    /// �z��ɂЂƂЂƂ���Ă���
    /// ����NowData���d����
    /// </summary>
    void SetData(LevelData[] data)
    {
        if(data == null)
        {
            return;
        }

        int dCount = data.Length;

        

        for (int i = 0; i < dCount; i++)
        {
            //�g���f�[�^
            LevelData useData = data[i];


            //�f�[�^���X�g�̒��ɃZ�[�u���Ă����f�[�^�����Ă���
            int count = useData.disenableEnemy.Length;
            if (count > 0)
            {
                for (int s = 0; s < count; s++)
                {
                    dataList[i].disenableEnemy[s] = useData.disenableEnemy[s];
                }

            }

            //�f�[�^���X�g�̒��ɃZ�[�u���Ă����f�[�^�����Ă���
            count = useData.disenableObjects.Length;
            if (count > 0)
            {
                for (int s = 0; s < count; s++)
                {
                    dataList[i].disenableObjects[s] = useData.disenableObjects[s];
                }

            }
        }

        //���̃f�[�^�̓|�C���^�[�̈ʒu�Ō��߂�
        nowData = dataList[nowPointer.x];

    }


    /// <summary>
    /// �I�u�W�F�N�g����ꂽ�炻�̎|���L�^����
    /// </summary>
    /// <param name="ID"></param>
    public void ObjectBreak(int ID,int mapNum)
    {
        if (dataList[mapNum].isChanged == false)
        {
            dataList[mapNum].isChanged = true;
        }
        // Debug.Log($"�}�CID:{ID}");
        dataList[mapNum].disenableObjects[ID] = true;
    }



    /// <summary>
    /// ���̃I�u�W�F�N�g�����Ă邩�ǂ����m�点��
    /// </summary>
    /// <param name="ID"></param>
    /// <returns></returns>
    public bool BreakCheck(int ID,int mapNum)
    {
        if (dataList[mapNum].disenableObjects.Length <= ID)
        {
            Debug.Log($"��������{dataList[mapNum]}");
        }

        return dataList[mapNum].disenableObjects[ID];
    }
    #endregion


    #region ���񂾂Ƃ��̂��߂̃��X�|�[������

    /// <summary>
    /// ���̃Z�O�����g�̈�ԉ��̈ʒu�ɍs����
    /// �Z�O�����g��Pass���������đO�̃}�b�v��Z�O�����g���N��
    /// �����̓Z�O�����g��ԍ��ƃ}�b�v�l�[���ŒT���ď��������肢����
    /// �v���C���[�̍��W���Z�O�����g�̈ʒu�{�P�O�ɏ����ς���
    /// �ł��Z�O�����g�̃`�F�b�N�|�C���g�ʂ��ĂȂ������猻�݈ʒu���O�̃Z�O�����g���ƌ������邩��
    /// �ǂ����悤
    /// </summary>
    public void Resporn()
    {
        SegmentSerch(nowPointer).GetComponent<SegmentTrigger>().RespornSegment();
    }


    #endregion

    /// <summary>
    /// �V�������x���}�l�[�W���𐶐�����
    /// </summary>
    public void NewInstance()
    {
        Destroy(instance);
        Resources.UnloadUnusedAssets();
        this.AddComponent<LevelManager>();
    }


    /// <summary>
    /// �����͂����̃��[�h���Ԍv���p�̃��\�b�h
    /// ���������Ă�����
    /// </summary>
    public void TimeStart()
    {
        timeChecek = Time.unscaledTime;
    }
    /// <summary>
    /// ��ɓ���
    /// </summary>
    /// <returns></returns>
    public float TimeEnd()
    {
        return Time.unscaledTime - timeChecek;
    }
    


}
