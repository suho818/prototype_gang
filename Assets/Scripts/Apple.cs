
using UnityEngine;
using System.Collections;



public class Apple : MonoBehaviour
{
    public int number; // ����� ���� ����
    public int id; // ����� ���� ��ȣ
    public int remainingApple;
    public int[] coordinate = new int[2]; // ����� ���� grid�� ��ǥ

    private Coroutine fallCoroutine;
    public float fallSpeed = 4f; // �� ĭ �������� �ӵ�

    public float launchForce = 300f; // ���� �߻��ϴ� ��
    public float horizontalOffset = 0.5f; // �¿�� ƥ �� �ִ� �ִ� �Ÿ�
    private Rigidbody2D rb;

    public bool isSelected = false;
    public GameObject selectedEffect;

    void Start()
    {
        selectedEffect = transform.Find("apple_image").Find("as_img").gameObject;
        rb = gameObject.AddComponent<Rigidbody2D>(); // Rigidbody2D ������Ʈ �߰�
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0; // �߷� ������ ����

    }
    // �Ʒ��� �̵��ϴ� �ڷ�ƾ ����
    public void StartFalling(Vector3 targetPosition)
    {
        if (fallCoroutine != null)
        {
            StopCoroutine(fallCoroutine); // �̹� ���� ���� �ڷ�ƾ�� �ִٸ� ����
        }
        fallCoroutine = StartCoroutine(FallToPosition(targetPosition));
    }

    // ������ �Ʒ��� �̵��ϴ� �ڷ�ƾ
    private IEnumerator FallToPosition(Vector3 targetPosition)
    {
        
        while (transform.position != targetPosition)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, fallSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition;
        fallCoroutine = null; // �̵��� �Ϸ�Ǹ� �ڷ�ƾ�� ����
    }

    public void ShootAndDestroy()
    {
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.bodyType = RigidbodyType2D.Dynamic;
        Destroy(gameObject.GetComponent<CircleCollider2D>());
        transform.position += new Vector3(0, 0, -1); // Z�� ���� ���� ���� ǥ�õǵ��� ����
        // ���� �߻��� �� �ణ�� �¿� ���� ������ �߰�
        float randomHorizontalForce = Random.Range(-horizontalOffset, horizontalOffset) * launchForce;
        Vector2 launchDirection = new Vector2(randomHorizontalForce, launchForce);
        rb.AddForce(launchDirection);
        rb.gravityScale = 3;
        // ���� �ð� �� ����� ����
        Destroy(gameObject, 2f); // 2�� �Ŀ� ����
    }

    public void SetSelected()
    {
        if (selectedEffect != null)
        {
            selectedEffect.SetActive(isSelected); // ���� ���ο� ���� ���� Ȱ��ȭ
        }
    }
}