using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TNRD;
using UnityEngine;

/// <summary>
///�@�Z�[�u����Ƃ��ɃX�N���v�g��������z���o��
///�@Awake�ł̓f�[�^���g�������͂��Ȃ�
///�@�X�^�[�g�ł�����������������A�Ƃ������[����O��
///�@
/// �K�v�ȏ��
/// �E�L�����N�^�[�R���g���[���[�����Ԉُ�ƃ��C�����
/// �E�X�R�A�}�l�[�W���[����v���C���[�̗̑́A�v���C����
/// �E���x���}�l�[�W���[���猻�݂̈ʒu
/// �E�}�b�v�f�[�^���猻�ݗL���ȃI�u�W�F�N�g���X�g�i���x���}�l�[�W���[�Ɏ�������H�j
/// �E����A�f�[�^�̓X�N���v�^�u��������ێ�����Ă�񂾁E���x���}�l�[�W���[�Ɏ������Ă������i����������Ă邩�݂����ȃt���O������ď���������Ă郌�x���f�[�^�͏������X�L�b�v�j
/// 
/// ES3Settings.defaultSettings.path = "MyNewFile.es3";�@����ŃZ�[�u�f�[�^�̕ۑ����ς���A�����f�[�^�����Ă邩

/// </summary>
public class SaveManager : MonoBehaviour
{

    [SerializeField]
    SaveMono[] _useList;
    

    /// <summary>
    /// �V���O���g����ÓI�C���X�^���X�̂��Ƃ����邵
    /// ������Awake�����͎��s���̍Ō�ɒu���������悭��
    /// </summary>
    // Start is called before the first frame update
    void Awake()
    {
        //���̕ӂ̏����̓��׃}�l����̌Ăяo���Ɉϑ�����
        //_useList = (SaveInterface[])_saveList;
        // Load();


    }



    /// <summary>
    /// UI�̃C�x���g�ł��Ăׂ�݂��
    /// </summary>
    bool Save()
    {
        int count = _useList.Length;
        for (int i = 0;i < count; i++)
        {
            _useList[i].Save();

        }

        //���x���}�l�[�W���[�̃Z�[�u��
        LevelManager.instance.Save();
        return true;
    }


    /// <summary>
    /// UI�̃C�x���g�ł��Ăׂ�݂��
    /// </summary>
    IEnumerator<bool> SaveA()
    {
        int count = _useList.Length;
        for (int i = 0; i < count; i++)
        {
            _useList[i].Save();
            yield return false;
        }
        yield return false;
        //���x���}�l�[�W���[�̃Z�[�u��
        LevelManager.instance.Save();
        yield return true;
    }


    /// <summary>
    /// ���[�h��Awake�ɂ�����ׂ�����Ȃ��́H
    /// ����͖{���ɂ���
    /// ���̃X�N���v�g��Awake�Ń��[�h���邩
    /// </summary>
    bool Load()
    {
        int count = _useList.Length;
        for (int i = 0; i < count; i++)
        {
            _useList[i].Load();
        }
        return true;
    }


    /// <summary>
    /// �񓯊��̃��[�h����
    /// </summary>
    /// <returns></returns>
    public async UniTask LoadAsync()
    {
        //����̃��[�h
        bool end = false;

        Debug.Log("df");

        end = Load();

        await UniTask.WaitUntil(() => end == true);
    }

    /// <summary>
    /// �񓯊��̃Z�[�u����
    /// </summary>
    /// <returns></returns>
    public async UniTask SaveAsync()
    {
        bool end = false;

        Debug.Log("df");

        end = Save();
        
        await UniTask.WaitUntil(() => end == true);



        //����̃Z�[�u
        //Save();
        Debug.Log("ss");
    }

}
