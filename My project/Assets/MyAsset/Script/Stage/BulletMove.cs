using PathologicalGames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FunkyCode.Light2D;

public class BulletMove : MonoBehaviour
{

    public float Speed;
    Vector2 moveDirection;
    [SerializeField]
    Rigidbody2D rb;

    [SerializeField]
    AnimationCurve boostBulletC;

    public float LifeTime;

    SpawnPool parentPool;

    float lifeCheck;
    float nowSpeed;


    private void Start()
    {
      
        
        parentPool = transform.parent.gameObject.GetComponent<SpawnPool>();
  
    }

    // Start is called before the first frame update
    private void OnEnable()
    {
        lifeCheck = 0f;
        // moveDirection = this.transform.rotation.eulerAngles.normalized;
        //Debug.Log($"������{transform.rotation}ddd{transform.rotation.eulerAngles}ggg{transform.rotation.eulerAngles.normalized}");

        // �I�u�W�F�N�g�̊p�x���擾
        float angle = transform.rotation.eulerAngles.z;
        // ��]�p��Vector2.right�ɑ������
        moveDirection = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

   //     Debug.Log($"�A�C�C�C�C�C{moveDirection}");
      //  Speed *= 2; 
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        //�@   

        lifeCheck += Time.fixedDeltaTime;

        nowSpeed = boostBulletC.Evaluate(lifeCheck) * Speed * 1.5f;


        rb.velocity = moveDirection * nowSpeed;

 //Debug.Log($"ssddf{nowSpeed}");
        //���ʂƂ�����
        if(lifeCheck > LifeTime)
        {
            if(this.gameObject.activeSelf)
        parentPool.Despawn(this.transform);
        }

    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        //�X�e�[�W�ɓ��������������
        if (collision.tag == "stage")
        {
            if (this.gameObject.activeSelf)
                parentPool.Despawn(this.transform);
        }
    }


}
