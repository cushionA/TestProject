using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{


    /// <summary>
    /// �����c�@��
    /// �`�F�b�N�|�C���g�ł����܂ŉ񕜁H
    /// ���₻��͂����ȁB�X�R�A�x�����ŉ񕜂͂���
    /// </summary>
    public float maxLife;

    /// <summary>
    /// �̗́A�c�@
    /// </summary>
    float life;

    /// <summary>
    /// �`�F�b�N�|�C���g���B���Ɏg�p
    /// ��蒼�����Ɏg��
    /// </summary>
    float memoryLife;

    private void Start()
    {
        life = maxLife;
    }

    public void LifeAdd()
    {
        life++;
    }

    public void LifeDamage()
    {
        life--;

        if(life < 0)
        {
            Die();
        }
    }

    /// <summary>
    /// ���S�C�x���g
    /// ��蒼�����ɃX�R�A����
    /// </summary>
    void Die()
    {

    }


}
