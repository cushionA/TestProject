using UnityEngine;
using UnityEngine.SceneManagement;

public enum CheckMethod
{
    Distance,
    Trigger
}


/// <summary>
/// �ЂƂЂƂ̃V�[���Ăяo���p�̃Q�[���I�u�W�F�N�g�ɂ��Ă�����
/// �����Ă��̃I�u�W�F�N�g�̖��O�̃V�[�����Ă΂��
/// gameobject.name�ŃV�[�������[�h���Ă�̂͂�����������
/// 
/// ����Ƃ͕ʂɃ}�l�[�W���[��邩�B
/// ���ǂ̃}�b�v�ɂ��Ăǂ̃Z�O�����g�ɂ���̂����Ă��
/// ���Ƃ��̑O�A���Ǝ��̃��x�����L�^����
/// </summary>

public class LevelController : MonoBehaviour
{
    /// <summary>
    /// �v���C���[�̃|�W�V�����Ƃ��݂�Ȏg���܂���񂾂���GManager�Ƀv�[�����Ƃ��ׂ��ł́H
    /// TransformPosi�擾�͏d���񂾂���
    /// </summary>
    public Transform player;

    public CheckMethod checkMethod;
    public float loadRange;

    //Scene state
    private bool isLoaded;
    private bool shouldLoad;


    void Start()
    {
        //���̃V�[�����V�[���}�l�[�W���[�Ɋ܂܂�Ă邩�̊m�F
        if (SceneManager.sceneCount > 0)
        {
            for (int i = 0; i < SceneManager.sceneCount; ++i)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name == gameObject.name)
                {
                    isLoaded = true;
                }
            }
        }
    }

    void Update()
    {
        //�ǂ̕��@���g�����`�F�b�N
        if (checkMethod == CheckMethod.Distance)
        {
            DistanceCheck();
        }
        else if (checkMethod == CheckMethod.Trigger)
        {
            TriggerCheck();
        }
    }

    /// <summary>
    /// �����`�F�b�N�Ń}�b�v�N��
    /// </summary>
    void DistanceCheck()
    {
        //Checking if the player is within the range
        if (Vector3.Distance(player.position, transform.position) < loadRange)
        {
            LoadScene();
        }
        else
        {
            UnLoadScene();
        }
    }

    void LoadScene()
    {
        if (!isLoaded)
        {
            //���[�h����V�[�����Ɠ����ł��邽�߁Agameobject�����g���ăV�[�������[�h����B
            SceneManager.LoadSceneAsync(gameObject.name, LoadSceneMode.Additive);
            //We set it to true to avoid loading the scene twice
            isLoaded = true;
        }
    }

    void UnLoadScene()
    {
        if (isLoaded)
        {
            SceneManager.UnloadSceneAsync(gameObject.name);
            isLoaded = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            shouldLoad = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            shouldLoad = false;
        }
    }

    void TriggerCheck()
    {
        //shouldLoad is set from the Trigger methods
        if (shouldLoad)
        {
            LoadScene();
        }
        else
        {
            UnLoadScene();
        }
    }


}