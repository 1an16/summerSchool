using UnityEngine;

public class RandomDirectionMove2D : MonoBehaviour
{
    public float moveSpeed = 2f;
    public Vector2 areaSize = new Vector2(20, 20); // 边界大小
    public float changeDirectionTime = 2f;

    private Vector2 moveDirection;
    private float timer;

    public Vector2 areaCenter; // 移动区域中心

    void Start()
    {
        // 默认以当前物体位置作为中心
        if (areaCenter == Vector2.zero)
        {
            areaCenter = transform.position;
        }

        RandomDirection();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= changeDirectionTime)
        {
            RandomDirection();
            timer = 0;
        }

        // 移动
        transform.position += (Vector3)(moveDirection * moveSpeed * Time.deltaTime);

        // 限制范围
        LimitBoundary();
    }


    void RandomDirection()
    {
        moveDirection = new Vector2(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ).normalized;
    }


    void LimitBoundary()
    {
        float minX = areaCenter.x - areaSize.x / 2;
        float maxX = areaCenter.x + areaSize.x / 2;

        float minY = areaCenter.y - areaSize.y / 2;
        float maxY = areaCenter.y + areaSize.y / 2;


        Vector3 pos = transform.position;

        // 超出边界立即拉回并改变方向
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


    // 显示边界
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireCube(
            areaCenter,
            areaSize
        );
    }
}