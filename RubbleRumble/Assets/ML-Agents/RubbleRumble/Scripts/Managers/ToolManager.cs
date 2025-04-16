using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolManager : SingletonBase<ToolManager>
{
    private PlayerHand playerScript;
    private Transform rightHand;  // 캐릭터 오른손 Transform (도구를 붙일 위치)

    public GameObject[] toolPrefabs;  // 1, 2, 3번 키로 장착할 도구 프리팹 배열 (Inspector에서 설정)
    private GameObject[] tools;  // 실제 생성된 도구 인스턴스를 저장하는 배열

    public int currentTool = -1;  // 현재 장착된 도구의 인덱스 (-1은 아무 도구도 장착되지 않음을 의미)

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
        // Mop3 프리팹 위치 조정
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

