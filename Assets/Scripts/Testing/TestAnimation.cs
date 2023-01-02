using System;
using UnityEngine;

public class TestAnimation : MonoBehaviour
{
    private Animator anim;
    private static readonly int IsWalking = Animator.StringToHash("isWalking");

    private bool isMoving;

    [SerializeField] private float multiplier = 1.5f;
    private static readonly int SpeedMultiplier = Animator.StringToHash("SpeedMultiplier");

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        anim.SetBool(IsWalking, Input.GetKey(KeyCode.Space));
        if (Input.GetKey(KeyCode.LeftShift)) anim.SetFloat(SpeedMultiplier, 1);
        else anim.SetFloat(SpeedMultiplier, multiplier);
    }


    private void OnAnimatorIK(int layerIndex)
    {
        Debug.Log("dwa");


    }

    void testEvent(MovementState state)
    {
        
    }
} 