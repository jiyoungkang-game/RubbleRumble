using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators; //바로아랫줄 상속위해
public class _Controller : Agent
{
    public float speed = 10.0f;
    public Rigidbody rb;

    private new void Awake()
    { rb = GetComponent<Rigidbody>(); }
    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.A))
            rb.AddForce(speed * Vector3.left);
        if (Input.GetKey(KeyCode.D))
            rb.AddForce(speed * Vector3.right);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int Discrete = actions.DiscreteActions[0]; //디스크립트 액션에 0번째
        //float Continuous = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f); //-1과 1사이 값을 반환

        Debug.Log("Discrete : " + Discrete);
        //Debug.Log("Continuous : " + Continuous);


    }
}
