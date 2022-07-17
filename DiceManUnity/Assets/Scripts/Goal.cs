using Cinemachine;
using UnityEngine;

public class Goal : MonoBehaviour
{
    public CinemachineVirtualCamera GoalCam;

    void Start()
    {
        
    }

    private void Update()
    {
        GoalCam.LookAt.Rotate(Vector3.up, 40f * Time.deltaTime);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {

           GoalCam.Priority = 100;
           FindObjectOfType<PlayerManager>().EnterGoal();
        }
    }
}
