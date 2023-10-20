using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CharacterController;


//�Z�[�u�f�[�^�m�F����
public class Title : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI tx;

    private void Start()
    {
        int num = ES3.Load<int>("BestScore", 0);

        tx.text = num.ToString();
    }


    public void StartGame()
    {
        //�Z�[�u�f�[�^�Ȃ��Ȃ�쐬
        if (!ES3.FileExists("SaveFile.es3"))
        {

            LevelManager.instance.NewSave();
        //    await UniTask.WaitUntil(()=> ES3.KeyExists("MapImfo"));
        }
        bl_SceneLoaderManager.LoadScene("MainScene");
    }

    public void DeleteData()
    {
        ES3.DeleteFile("Users/tatuk/AppData/LocalLow/DefaultCompany/My project/SaveFile.es3");
    }


}
