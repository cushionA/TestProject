using UnityEngine;

public class ChaseAndFall : MonoBehaviour
{
    public GameObject player;
    public float chaseRange = 5f;
    public float chaseLimit = 10f;

    /// <summary>
    /// 速度
    /// </summary>
    public float speed;
    
    /// <summary>
    /// 追跡しますかー
    /// </summary>
    public bool isChasing = false;

    private Vector2 fallVelocity;
    private Rigidbody2D rb;
    private bool isFalling = false;

    private float startFallTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        fallVelocity = new Vector2(0f, -speed);
    }



    void FixedUpdate()
    {
        if (!isFalling && player.transform.position.y > transform.position.y + 30f)
        {
            isFalling = true;
            startFallTime = Time.time;
        }

        if (isFalling && Time.time - startFallTime > chaseLimit)
        {
            isFalling = false;
            fallVelocity = new Vector2(0f, -speed);
        }

        if (isFalling)
        {
            if (!isChasing)
            {
                rb.velocity = fallVelocity;
            }
            else
            {
                fallVelocity = player.transform.position - transform.position;

                //角度の絶対値が追跡限界超えてるなら
                if (Mathf.Abs(Vector2.Angle(rb.velocity, fallVelocity)) > chaseRange)
                {
                    fallVelocity = Quaternion.Euler(0, 0, chaseRange) * -rb.velocity;
                    rb.velocity = fallVelocity.normalized * speed;
                }
                else
                {
                    rb.velocity = fallVelocity.normalized * speed;
                }
            }

            if (transform.position.y < Camera.main.transform.position.y - Camera.main.orthographicSize)
            {
                Destroy(gameObject);
            }
        }
    }


}