using UnityEngine;

public class ChaseAndFall : MonoBehaviour
{
    public GameObject player;
    public float chaseRange = 5f;
    public float chaseLimit = 10f;

    /// <summary>
    /// ë¨ìx
    /// </summary>
    public float speed;
    
    /// <summary>
    /// í«ê’ÇµÇ‹Ç∑Ç©Å[
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

                //äpìxÇÃê‚ëŒílÇ™í«ê’å¿äEí¥Ç¶ÇƒÇÈÇ»ÇÁ
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