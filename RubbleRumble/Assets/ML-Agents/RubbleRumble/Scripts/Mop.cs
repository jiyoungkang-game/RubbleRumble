using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class Mop : MonoBehaviour
{
    public GameObject player;
    public GameObject sink;
    private GameObject nearDust;
    private PlayerInteract playerInteract;

    public Material[] mat = new Material[3];

    private bool isTrigger;
    private Vector3 righthandPos;
    //private Vector3 offset = new Vector3(0.4f, 0.05f, -0.55f);
    private Vector3 offset = new Vector3(0f, 0.1f, 0f);

    private int useCount;
    //public float triggerDistance = 0.1f;
    public float triggerDistance = 5f;

    private float holdTime = 0f;

    private void Awake()
    {
        righthandPos = gameObject.GetComponentInParent<Transform>().localPosition;
        sink = GameObject.FindWithTag("Sink");
    }

    private void Start()
    {
        player = GameObject.FindWithTag("Player");
        playerInteract = player.GetComponent<PlayerInteract>();
        //isTrigger = false;
        isTrigger = true;
        useCount = 0;
    }

    private void Update()
    {
        //if (isTrigger)
        //{
        //    transform.position = player.transform.position - offset;
        //}
        transform.localPosition = righthandPos + offset;
        transform.localRotation = Quaternion.Euler(90, 0, 45);

        if (Input.GetKeyDown(KeyCode.E) && nearDust != null)
        {
            if (useCount < 2)
            {
                // Destroy(nearDust);
                Obstacle dirt = nearDust.GetComponent<Obstacle>();
                dirt.CleanObstacle();
                nearDust = null;
                useCount++;
                gameObject.GetComponent<MeshRenderer>().material = mat[useCount];
            } else
            {
                Debug.Log("Wash Your Mop!");
            }
        }

        if (useCount >= 2)
        {
            WashMopNearSink();
        }

        ReturnCurrentInteract();
    }

    //private void OnCollisionEnter(Collision collision)
    private void OnTriggerEnter(Collider other)
    {
        //if (collision.gameObject.CompareTag("Player"))
        //{
        //    isTrigger = true;
        //    transform.rotation = Quaternion.Euler(0, 0, -22f);
        //}

        //if (collision.gameObject.CompareTag("Dust") && isTrigger)
        //{
        //    nearDust = collision.gameObject;
        //    Debug.Log("Collision Detection");
        //}
        if (other.gameObject.CompareTag("Dust") && isTrigger)
        {
            nearDust = other.gameObject;
            Debug.Log("Collision Detection");
        }
    }

    //private void OnCollisionExit(Collision collision)
    private void OnTriggerExit(Collider other)
    {
        //if (collision.gameObject.CompareTag("Dust"))
        if (other.gameObject.CompareTag("Dust"))
        {
            nearDust = null;
        }
    }

    private void WashMopNearSink()
    {
        float distance = Vector3.Distance(transform.position, sink.transform.position);

        if (distance <= triggerDistance)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                holdTime = 0f;
            }

            if (Input.GetKey(KeyCode.Q))
            {
                holdTime += Time.deltaTime;

                if (holdTime >= 2f)
                {
                    useCount = 0;
                    gameObject.GetComponent<MeshRenderer>().material = mat[useCount];
                }
            }

            if (Input.GetKeyUp(KeyCode.Q))
            {
                holdTime = 0f;
            }
        }
        else
        {
            holdTime = 0f;
        }
    }

    private int ReturnCurrentInteract()
    {
        if (holdTime > 0f && holdTime < 2f) // Ȧ���ϰ� �ִ� ���¸�
        {
            playerInteract.InteractUIState = 3;    // Ȧ���� Ȱ��ȭ
            return playerInteract.InteractUIState;
        }

        float distance = Vector3.Distance(transform.position, sink.transform.position);
        if (distance <= triggerDistance)    // ������ ������ �ְ�
        {
            if (useCount >= 2)  // ����Ƚ�� �Ѿ�����
            {
                playerInteract.InteractUIState = 2;    // ��ȣ�ۿ� Q �ȳ�
                return playerInteract.InteractUIState;
            }
            else        // ��� ������ ���¸�
            {
                playerInteract.InteractUIState = 0;    // ��ȣ�ۿ� ��Ȱ��ȭ
                return playerInteract.InteractUIState;
            }
        }

        if (nearDust != null && useCount < 2)   // ����� ������ �� ������
        {
            playerInteract.InteractUIState = 1;    // ��ȣ�ۿ� E �ȳ�
            return playerInteract.InteractUIState;
        }

        // ���ǿ� ���� ������
        playerInteract.InteractUIState = 0;    // ��ȣ�ۿ� ��Ȱ��ȭ
        return playerInteract.InteractUIState;
    }

    public float GetHoldingTime()
    {
        return holdTime;
    }
}
