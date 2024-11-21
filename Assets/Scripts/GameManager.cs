using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR;

public class GameManager : MonoBehaviour
{
    public GameObject[] applePrefabs; // apple1 ~ apple9 �������� ���� �迭
    public GameObject appleSelected;
    
    private int[,] grid = new int[17, 9];
    private int[] verticalAppleNum = new int[17];
    private int[] verticalRemovedAppleNum = new int[17];
    private List<GameObject> appleObjects = new List<GameObject>();
    private List<IEnumerator> appleCoroutine = new List<IEnumerator>();
    public float fallSpeed = 0.5f; // ����� �� ĭ �������µ� �ɸ��� �ð�

    public int gameMethod = 1; // 0 = �巡�� , 1 = ��ġ
    public int touchNum = 0;

    private float width;
    private float height;

    public RectTransform selectionBox; // �巡�� ���� UI ���
    private Vector2 startPosition;
    private Vector2 endPosition;
    private List<GameObject> selectedApples = new List<GameObject>();

    public TextMeshProUGUI scoreText;
    private int score = 0;
    public TextMeshProUGUI timeText;
    private float time = 0f;

    public int gameMode = 0; //0: �⺻���, 1: ���Ѹ��



    void Start()
    {
        Vector3 screenBottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane));
        Vector3 screenTopRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.nearClipPlane));

        width = screenTopRight.x - screenBottomLeft.x;
        height = screenTopRight.y - screenBottomLeft.y;

        InitializeGrid();
        UpdateScoreUI();
    }

    public void RestartGame()
    {
       
        foreach (var apple in appleObjects)
        {
            Destroy(apple);
        }
        InitializeGrid();
        score = 0;
        UpdateScoreUI();
    }

    void InitializeGrid()
    {
        verticalAppleNum = new int[17];
        verticalRemovedAppleNum = new int[17];

        for (int x = 0; x < 17; x++)
        {
            for (int y = 0; y < 9; y++)
            {
                grid[x, y] = UnityEngine.Random.Range(1, 10); // 1~9 ���� ���� ����
                verticalAppleNum[x] += 1;
                verticalRemovedAppleNum[x] += 1;
            }
        }
        DisplayGrid();
    }

    void DisplayGrid()
    {
        appleObjects.Clear();
        for (int x = 0; x < 17; x++)
        {
            for (int y = 0; y < 9; y++)
            {
                int number = grid[x, y] - 1; // 1~9�� 0~8 �ε����� ����
                GameObject apple = Instantiate(applePrefabs[number], new Vector3(x-17/2, y-9/2, 0), Quaternion.identity);
                apple.name = $"Apple_{x}_{y}"; // ������Ʈ �̸� ����

                GameObject as_img = Instantiate(appleSelected, new Vector3(0,0,0), Quaternion.identity);
                as_img.transform.SetParent(apple.transform.Find("apple_image"), false);
                as_img.gameObject.SetActive(false);
                as_img.name = "as_img";

                appleObjects.Add(apple);
                // Apple ��ũ��Ʈ�� �߰��ϰ� ���� ����
                Apple appleComponent = apple.GetComponent<Apple>();
                if (appleComponent == null)
                {
                    appleComponent = apple.AddComponent<Apple>();
                }
                appleComponent.id = y * 17 + x;
                appleComponent.number = number + 1; // ���� ���ڸ� ����
                appleComponent.coordinate = new int[] {x, y};
                appleComponent.remainingApple = y;
                
                
            }
        }
    }
   
   
        void Update()
        {
        // R Ű�� ������ �����
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }

        if (IsTouchInput())
            {
                HandleTouchInput();
            
            }
            else
            {
           //     HandleMouseInput();
           
        }
        }

        // ��ġ �Է� Ȯ��
        bool IsTouchInput()
        {
            return Input.touchCount > 0;
        }

        // ��ġ �Է� ó��
        void HandleTouchInput()
        {
            Touch touch = Input.GetTouch(0); // ù ��° ��ġ �Է�

        if (gameMethod == 0)
        {
            Debug.Log("��ġ0");
            switch (touch.phase)
            {
                case TouchPhase.Began: // ��ġ ����
                    selectedApples.Clear();
                    startPosition = touch.position;
                    endPosition = touch.position;
                    UpdateSelectionBox();
                    selectionBox.gameObject.SetActive(true);
                    break;
                case TouchPhase.Stationary: // ��ġ ������
                    UpdateSelectionBox();
                    break;


                case TouchPhase.Moved: // ��ġ �̵� ��
                    endPosition = touch.position;
                    UpdateSelectionBox();
                    break;

                case TouchPhase.Ended: // ��ġ ����
                    SelectApplesInArea();
                    selectionBox.gameObject.SetActive(false);
                    CheckAndRemoveApples();
                    break;
            }
        }
        else if (gameMethod == 1)
                {
            Debug.Log("��ġ1");

            if (touchNum == 0)
            {
                switch (touch.phase)
                {
                    case TouchPhase.Began: // ��ġ ����
                        selectedApples.Clear();
                        startPosition = touch.position;
                        endPosition = touch.position;
                        UpdateSelectionBox();
                        selectionBox.gameObject.SetActive(true);
                        break;

                    }
                touchNum = 1;
            }
            else
            {if (touchNum == 1)
                {
                    if (touch.phase == TouchPhase.Began)
                    {
                        endPosition = touch.position;
                        UpdateSelectionBox();
                        selectionBox.gameObject.SetActive(true);
                        touchNum = 2;

                    }
                }
            else if (touchNum == 2)
                {
                    if (touch.phase == TouchPhase.Ended)
                    {
                        UpdateSelectionBox();
                        SelectApplesInArea();

                        selectionBox.gameObject.SetActive(false);
                        CheckAndRemoveApples();
                        touchNum = 0;
                    }
                }
             
               
            }
        }
        }

        // ���콺 �Է� ó��
        void HandleMouseInput()
        {
            // �巡�� ����
            if (Input.GetMouseButtonDown(0))
            {
                selectedApples.Clear();
                startPosition = Input.mousePosition;
                selectionBox.gameObject.SetActive(true);
            }

            // �巡�� ��
            if (Input.GetMouseButton(0))
            {
                endPosition = Input.mousePosition;
                UpdateSelectionBox();
            }

            // �巡�� ����
            if (Input.GetMouseButtonUp(0))
            {
                SelectApplesInArea();
                selectionBox.gameObject.SetActive(false);
                CheckAndRemoveApples();
            }
        }

        // �巡�� ���簢�� ������ ����
        void UpdateSelectionBox()
    {
        Vector2 boxStart = startPosition;
        Vector2 boxSize = endPosition - startPosition;

        selectionBox.anchoredPosition = boxStart + boxSize / 2;
        selectionBox.sizeDelta = new Vector2(Mathf.Abs(boxSize.x), Mathf.Abs(boxSize.y));

        Vector2 min = Camera.main.ScreenToWorldPoint(Vector3.Min(startPosition, endPosition));
        Vector2 max = Camera.main.ScreenToWorldPoint(Vector3.Max(startPosition, endPosition));


        foreach(var apple in appleObjects)
        {
            apple.GetComponent<Apple>().isSelected = false;
            apple.GetComponent<Apple>().SetSelected();

        }

        Collider2D[] colliders = Physics2D.OverlapAreaAll(min, max);

        foreach (var collider in colliders)
        {
            if (collider.gameObject.CompareTag("Apple"))
            {
                collider.gameObject.GetComponent<Apple>().isSelected=true;
                collider.gameObject.GetComponent<Apple>().SetSelected();
            }
        }
    }

    // ���� �� ����� ����
    void SelectApplesInArea()
    {
        foreach (var apple in appleObjects)
        {
            apple.GetComponent<Apple>().isSelected = false;
            apple.GetComponent<Apple>().SetSelected();

        }

        Vector2 min = Camera.main.ScreenToWorldPoint(Vector3.Min(startPosition, endPosition));
        Vector2 max = Camera.main.ScreenToWorldPoint(Vector3.Max(startPosition, endPosition));
       

        Collider2D[] colliders = Physics2D.OverlapAreaAll(min, max);
        
        foreach (var collider in colliders)
        {
            if (collider.gameObject.CompareTag("Apple"))
            {
                selectedApples.Add(collider.gameObject);
            }
        }
    }

    // ���õ� ������� ���� 10���� Ȯ�� �� ����
    void CheckAndRemoveApples()
    {
        int sum = 0;
        foreach (var apple in selectedApples)
        {

            Apple appleComponent = apple.GetComponent<Apple>();
            
            sum += appleComponent.number; // Apple ��ũ��Ʈ�� ���� ������ �ִٰ� ����
        }

        if (sum == 10)
        {
            foreach (var apple in selectedApples)
            {
                
                Vector2 pos = apple.transform.position;
                grid[(int)pos.x + 8, (int)pos.y + 4] = 0;// ��� ���� �� 0���� ǥ��
                Apple AP = apple.GetComponent<Apple>();
                AP.ShootAndDestroy();
                appleObjects.Remove(apple);
                foreach (var ap in appleObjects.FindAll(a => a.GetComponent<Apple>().coordinate[0] == AP.coordinate[0] && a.GetComponent<Apple>().coordinate[1] > AP.coordinate[1]))
                {
                    ap.GetComponent<Apple>().remainingApple -= 1;
                }
                verticalRemovedAppleNum[AP.coordinate[0]] -= 1;
               
                CreateApple(AP.coordinate[0]);
          
                
                for (int id_num = AP.id; id_num < 17 * verticalAppleNum[AP.coordinate[0]]; id_num += 17)
                {
                    ApplyGravity(id_num);
                    PrintGrid();
                }

                
            }
            score += selectedApples.Count;
            UpdateScoreUI();
        }
    }

        void ApplyGravity(int id)
        {
            GameObject apple = FindAppleAtId(id);
            if (apple != null)
            {
                Vector3 targetPosition = new Vector3(apple.transform.position.x, apple.GetComponent<Apple>().remainingApple - 4, 0);
                apple.GetComponent<Apple>().StartFalling(targetPosition);
            }

        }
        void PrintGrid()
        {
            string gridOutput = "";

            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 17; x++)
                {
                    //gridOutput += gridAppleRemaining[x, y] + "\t"; // �� ��� ���̿� ������ ����
                }
                gridOutput += "\n"; // �� ���� ���� �� �� �ٲ�
            }
}

        GameObject FindAppleAtCoordinate(int x, int y)
        {
            return appleObjects.Find(a => a.GetComponent<Apple>().coordinate[0] == x && a.GetComponent<Apple>().coordinate[1] == y);
        }
        GameObject FindAppleAtId(int id)
        {
            return appleObjects.Find(a => a.GetComponent<Apple>().id == id);
        }
    private void UpdateScoreUI()
    {
        scoreText.text = "Score: " + score;
    }

    
    void CreateApple(int x)
    {
        verticalAppleNum[x] += 1;
        verticalRemovedAppleNum[x] += 1;
        int y = verticalAppleNum[x] - 1;
        int number = UnityEngine.Random.Range(1, 10)-1;
        float Y = appleObjects.Where(apple => apple.GetComponent<Apple>().coordinate[0] == x)
            .OrderByDescending(apple => apple.GetComponent<Apple>().coordinate[1])
            .FirstOrDefault().transform.position.y+1;
        
        Y = Y > 5 ? Y : 5;
      
        GameObject apple = Instantiate(applePrefabs[number], new Vector3(x - 17 / 2, Y, 0), Quaternion.identity);
        apple.name = $"Apple_{x}_{y}"; // ������Ʈ �̸� ����

        GameObject as_img = Instantiate(appleSelected, new Vector3(0, 0, 0), Quaternion.identity);
        as_img.transform.SetParent(apple.transform.Find("apple_image"), false);
        as_img.gameObject.SetActive(false);
        as_img.name = "as_img";

        appleObjects.Add(apple);
        // Apple ��ũ��Ʈ�� �߰��ϰ� ���� ����
        Apple appleComponent = apple.GetComponent<Apple>();
        if (appleComponent == null)
        {
            appleComponent = apple.AddComponent<Apple>();
        }
        appleComponent.id = y * 17 + x;
        appleComponent.number = number + 1; // ���� ���ڸ� ����
        appleComponent.coordinate = new int[] { x, y };
        appleComponent.remainingApple = verticalRemovedAppleNum[x] - 1;
    }

}