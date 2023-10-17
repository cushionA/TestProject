using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using static CharacterController;
using PathologicalGames;
using Cysharp.Threading.Tasks.Triggers;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Unload : MonoBehaviour
{

    [SerializeField]
    SpawnPool pool;

    [SerializeField]
    int mapNum;

#if UNITY_EDITOR
    [ContextMenu("���g�p�I�u�W�F�N�g���A�����[�h")]
    public async UniTaskVoid UnLoadMethod()
    {
        Debug.Log("�A�����[�h");
        await Resources.UnloadUnusedAssets();
    }


    [ContextMenu("�ǂݍ��܂�Ă�V�[�����m�F")]
    public void CheckScene()
    {
        //���ݓǂݍ��܂�Ă���V�[�����������[�v
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {

            //�ǂݍ��܂�Ă���V�[�����擾���A���̖��O�����O�ɕ\��
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).name;
            Debug.Log(sceneName);

        }
    }


    [ContextMenu("scene���̃C�x���g�I�u�W�F�N�g�Ƀ}�b�v�ԍ���ID�t�^")]
    public void SetID()
    {
        EventObject[] objs = FindObjectsOfType<EventObject>();

        int count = objs.Length;

        int i = 0;

        //���ݓǂݍ��܂�Ă���V�[�����������[�v
        foreach(EventObject obj in objs)
        {
            if (obj.BreakableCheck())
            {
                obj.SetID(i,mapNum);
                
                Debug.Log($"ID�F{i}");
                i++;
            }
        }

        Debug.Log($"{i}�̔j��\�I�u�W�F�N�g");
    }
    [ContextMenu("scene���̃C�x���g�I�u�W�F�N�g��ID�m�F")]
    public void ReadID()
    {
        EventObject[] objs = FindObjectsOfType<EventObject>();

        int count = objs.Length;

        int i = 0;

        //���ݓǂݍ��܂�Ă���V�[�����������[�v
        foreach (EventObject obj in objs)
        {
            if (obj.BreakableCheck())
            {

                Debug.Log($"ID�F{obj.GetID()}");
                i++;
            }
        }
    }


    [ContextMenu("�V�����Z�[�u�f�[�^���쐬")]
    public void NewSave()
    {
        //�X�R�A�}�l�[�W���[�̏�����

        ES3.Save("PlayTime", 0);
        ES3.Save("Score", 0);
        ES3.Save("Life", 5);


        //  �L�����R���g���[���[�̏�����
        ES3.Save("NowCondition", new List<EventObject.EventData>());
        ES3.Save("NowEffect", new GimickCondition());
        ES3.Save("PositionImfo", Vector3.zero);

        LevelManager.instance.NewSave();
    }

    [ContextMenu("�v���n�u�̐���������")]
    public void DisplayPool()
    {
        //�X�R�A�}�l�[�W���[�̏�����
       int count = pool.prefabPools.Count;
        for (int i = 0;i<count;i++)
        {
         //   Debug.Log($"{i + 1}�߂�Pool��{pool.prefabPools[i].totalCount}");
        }
    }


#endif

}
