using UnityEngine;

public class RandomSnailMove : MonoBehaviour
{
    public float moveSpeed = 2f;
    public Vector2 moveArea = new Vector2(10, 10);
    public float changeDirectionTime = 2f;

    private Vector2 moveDirection;
    private float timer;

    private Vector2 startPosition;


    void Start()
    {
        startPosition = transform.position;

        RandomDirection();
    }


    void Update()
    {
        timer += Time.deltaTime;


        // 定时随机改变方向
        if (timer >= changeDirectionTime)
        {
            RandomDirection();
            timer = 0;
        }


        // 移动
        transform.position +=
            (Vector3)(moveDirection * moveSpeed * Time.deltaTime);


        // 限制范围
        CheckBoundary();
    }



    // 随机方向
    void RandomDirection()
    {
        float x = Random.Range(-1f, 1f);
        float y = Random.Range(-1f, 1f);

        moveDirection = new Vector2(x, y).normalized;
    }



    // 边界限制
    void CheckBoundary()
    {
        float minX = startPosition.x - moveArea.x / 2;
        float maxX = startPosition.x + moveArea.x / 2;

        float minY = startPosition.y - moveArea.y / 2;
        float maxY = startPosition.y + moveArea.y / 2;


        Vector3 pos = transform.position;


        if (pos.x < minX || pos.x > maxX)
        {
            moveDirection.x *= -1;
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
        }


        if (pos.y < minY || pos.y > maxY)
        {
            moveDirection.y *= -1;
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
        }


        transform.position = pos;
    }



    // ==========================
    // 碰撞检测
    // ==========================

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Destory"))
        {
            Destroy(collision.transform.root.gameObject);
        }

        moveDirection = -moveDirection;
    }

    // 显示移动范围
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireCube(
            Application.isPlaying ? startPosition : transform.position,
            moveArea
        );
    }
}