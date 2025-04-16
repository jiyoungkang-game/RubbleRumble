using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolManager : SingletonBase<ToolManager>
{
    private PlayerHand playerScript;
    private Transform rightHand;  // ĳ���� ������ Transform (������ ���� ��ġ)

    public GameObject[] toolPrefabs;  // 1, 2, 3�� Ű�� ������ ���� ������ �迭 (Inspector���� ����)
    private GameObject[] tools;  // ���� ������ ���� �ν��Ͻ��� �����ϴ� �迭

    public int currentTool = -1;  // ���� ������ ������ �ε��� (-1�� �ƹ� ������ �������� ������ �ǹ�)

    void Start()
    {
        playerScript = GameObject.Find("Player").GetComponent<PlayerHand>();
        rightHand = playerScript.rightHand;

        currentTool = -1;
        tools = new GameObject[toolPrefabs.Length];

        for (int i = 0; i < toolPrefabs.Length; i++)
        {
            if (toolPrefabs[i] != null) 
            {
                tools[i] = Instantiate(toolPrefabs[i], rightHand.position, rightHand.rotation, rightHand);
                tools[i].transform.localRotation = Quaternion.Euler(30, 20, -60);
                tools[i].SetActive(false);
            }
        }
        // Mop3 ������ ��ġ ����
        tools[2].transform.localPosition += Vector3.up * 0.1f;
        tools[2].transform.localPosition += Vector3.forward * 0.1f;
        tools[2].transform.localRotation = Quaternion.Euler(90, 0, 45);
    }

    public void EquipTool(int index)
    {
        if (index < 0 || index >= tools.Length || tools[index] == null) return;

        if (currentTool != -1) tools[currentTool].SetActive(false);

        tools[index].SetActive(true);
        currentTool = index;
    }
}

