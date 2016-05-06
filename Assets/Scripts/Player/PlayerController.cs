﻿using JetBrains.Annotations;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof (Animator))]
[RequireComponent(typeof (Rigidbody))]
[RequireComponent(typeof (CapsuleCollider))]

public class PlayerController : MonoBehaviour
{
    public enum CharacterClass
    {
        MeleeWarrior,
        RangedMage
    }

    public class AttackCombo
    {
        public enum AttackState
        {
            NotAttacking,
            FirstAttack,
            SecondAttack,
            ThirdAttack
        }

        //Trigger Based
        public AttackState currAttackState;
        public bool amComboing = false;
        public bool canStartCombo = true;
        public bool isComboOver = true;

        public AttackCombo()
        {
            currAttackState = AttackState.NotAttacking;
        }

        //Helper Funcs
        public void StartCombo()
        {
            amComboing = true;
            canStartCombo = false;
            isComboOver = false;
            currAttackState = AttackState.FirstAttack;
        }

        public void EndCombo()
        {
            amComboing = false;
            canStartCombo = true;
            isComboOver = true;
            currAttackState = AttackState.NotAttacking;
        }
    }

    //Player Info
    private Vector3 velocity;
    public CharacterClass playerClass = CharacterClass.MeleeWarrior;

    //Player Movement
    public float fMoveSpeed = 4.0f;
    public float bMoveSpeed = 3.0f;
    public float turnRate = 1.0f;
    private float animationSpeed;

    //References
    private Animator anim;
    private RuntimeAnimatorController animRC;
    private AnimatorStateInfo animInfo;
    private Rigidbody rb;
    private CapsuleCollider coll;

    //Weapons
    private GameObject paladinSword;

    //Basic Attack Chains
    public AttackCombo meleeAttackCombo;
    public AttackCombo rangedAttackCombo;

    private void Awake()
    {
        //Player
        anim = GetComponent<Animator>();
        animRC = anim.runtimeAnimatorController;
        animInfo = anim.GetCurrentAnimatorStateInfo(0);
        rb = GetComponent<Rigidbody>();
        coll = GetComponent<CapsuleCollider>();

        //Weapons
        paladinSword = transform.FindChild("Sword").gameObject;
    }

    private void Start()
    {
        InitializeAttackComboes();
        CheckWeapon();
        animationSpeed = 1.0f;
        anim.speed = animationSpeed;
    }

    private void Update()
    {
        MovePlayer();
        UpdatePlayerClass();
        UpdateAttackChains();
        UpdateAbilites();
        UpdateBuffs();
    }

    private void MovePlayer()
    {
        float hInput = Input.GetAxis("Horizontal");
        float vInput = Input.GetAxis("Vertical");

        anim.SetFloat("Speed", vInput);
        anim.SetFloat("PivotRate", hInput);

        velocity = new Vector3(0.0f, 0.0f, vInput);
        velocity = transform.TransformDirection(velocity);

        if (vInput > 0.1f)
        {
            velocity *= fMoveSpeed;
        }
        else if (vInput < -0.1f)
        {
            velocity *= bMoveSpeed;
        }
        transform.localPosition += velocity * Time.fixedDeltaTime;
        transform.Rotate(0.0f, hInput * turnRate, 0.0f);
    }

    private void UpdatePlayerClass()
    {
        if (Input.GetButtonDown("SwitchClass"))
        {
            anim.Play(Animator.StringToHash("Base Layer.ClassChange"));

            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.ClassChange"))
            {
                switch (playerClass)
                {
                    case CharacterClass.MeleeWarrior:
                        {
                            playerClass = CharacterClass.RangedMage;
                            break;
                        }
                    case CharacterClass.RangedMage:
                        {
                            playerClass = CharacterClass.MeleeWarrior;
                            break;
                        }
                    default:
                        {
                            Debug.Log("Invalid Char Class!");
                            break;
                        }
                }
            }
        }
    }

    private void UpdateAttackChains()
    {
        if (IsWarrior())
        {
            MeleeAttackChain();
        }
        else
        {
           RangedAttackChain();
        }
        
    }


    public void MeleeAttackChain()
    {
        if (meleeAttackCombo.currAttackState == AttackCombo.AttackState.NotAttacking && meleeAttackCombo.isComboOver)
        {
            ResetCombo();
        }

        if (Input.GetButtonDown("Attack"))
        {
            if (meleeAttackCombo.canStartCombo)
            {
                meleeAttackCombo.StartCombo();
                if (meleeAttackCombo.amComboing)
                {
                    UpdateMeleeCombo();
                }
                if (!meleeAttackCombo.amComboing && meleeAttackCombo.isComboOver)
                {
                    meleeAttackCombo.StartCombo();
                    if (meleeAttackCombo.amComboing)
                    {
                        UpdateMeleeCombo();
                    }
                }
            }
            else
            {
                if (meleeAttackCombo.amComboing)
                {
                    UpdateMeleeCombo();
                }
            }
        }
    }


    public void UpdateMeleeCombo()
    {
        if (meleeAttackCombo.amComboing)
        {
            switch (meleeAttackCombo.currAttackState)
            {
                case AttackCombo.AttackState.NotAttacking:
                {
                    meleeAttackCombo.EndCombo();
                    break;
                }

                case AttackCombo.AttackState.FirstAttack:
                {
                    if (AnimatorIsPlaying("Idle2Running"))
                    {
                        anim.SetTrigger("MSlash1");
                        meleeAttackCombo.currAttackState = AttackCombo.AttackState.SecondAttack;
                    }
                    else
                    {
                        meleeAttackCombo.EndCombo();
                    }
                    break;
                }
                case AttackCombo.AttackState.SecondAttack:
                {
                    if (AnimatorIsPlaying("MeleeSlash1"))
                    {
                        anim.SetTrigger("MSlash2");
                        meleeAttackCombo.currAttackState = AttackCombo.AttackState.ThirdAttack;
                    }
                    else
                    {
                        meleeAttackCombo.EndCombo();
                    }
                    break;
                }
                case AttackCombo.AttackState.ThirdAttack:
                {
                    if (AnimatorIsPlaying("MeleeSlash2"))
                    {
                        anim.SetTrigger("MSlash3");
                        meleeAttackCombo.currAttackState = AttackCombo.AttackState.NotAttacking;
                    }
                    else
                    {
                        meleeAttackCombo.EndCombo();
                    }
                    break;
                }

                default:
                {
                    Debug.Log("Invalid Melee Attack State.");
                    break;
                }
            }
        }
    }

    public void RangedAttackChain()
    {
        if (rangedAttackCombo.currAttackState == AttackCombo.AttackState.NotAttacking && rangedAttackCombo.isComboOver)
        {
            ResetCombo();
        }

        if (Input.GetButtonDown("Attack"))
        {
            if (rangedAttackCombo.canStartCombo)
            {
                rangedAttackCombo.StartCombo();
                if (rangedAttackCombo.amComboing)
                {
                    UpdateRangedCombo();
                }
                if (!rangedAttackCombo.amComboing && rangedAttackCombo.isComboOver)
                {
                    rangedAttackCombo.StartCombo();
                    if (rangedAttackCombo.amComboing)
                    {
                        UpdateRangedCombo();
                    }
                }
            }
            else
            {
                if (rangedAttackCombo.amComboing)
                {
                    UpdateRangedCombo();
                }
            }
        }
    }

    public void UpdateRangedCombo()
    {
        if (rangedAttackCombo.amComboing)
        {
            switch (rangedAttackCombo.currAttackState)
            {
                case AttackCombo.AttackState.NotAttacking:
                    {
                        rangedAttackCombo.EndCombo();
                        break;
                    }

                case AttackCombo.AttackState.FirstAttack:
                    {
                        if (AnimatorIsPlaying("Idle2Running"))
                        {
                            anim.SetTrigger("RCast1");
                            rangedAttackCombo.currAttackState = AttackCombo.AttackState.SecondAttack;
                        }
                        else
                        {
                            rangedAttackCombo.EndCombo();
                        }
                        break;
                    }
                case AttackCombo.AttackState.SecondAttack:
                    {
                        if (AnimatorIsPlaying("RangedCast1"))
                        {
                            anim.SetTrigger("RCast2");
                            rangedAttackCombo.currAttackState = AttackCombo.AttackState.ThirdAttack;
                        }
                        else
                        {
                            rangedAttackCombo.EndCombo();
                        }
                        break;
                    }
                case AttackCombo.AttackState.ThirdAttack:
                    {
                        if (AnimatorIsPlaying("RangedCast2"))
                        {
                            anim.SetTrigger("RCast3");
                            rangedAttackCombo.currAttackState = AttackCombo.AttackState.NotAttacking;
                        }
                        else
                        {
                            rangedAttackCombo.EndCombo();
                        }
                        break;
                    }

                default:
                    {
                        Debug.Log("Invalid Ranged Attack State.");
                        break;
                    }
            }
        }
    }

    public void UpdateAbilites()
    {
        //Ability Code Here
    }

    public void UpdateBuffs()
    {
        //Buff Code Here
    }


    //-Helper Funcs-//
    private void InitializeAttackComboes()
    {
        meleeAttackCombo = new AttackCombo();
        rangedAttackCombo = new AttackCombo();
    }

    private bool IsWarrior()
    {
        return playerClass == CharacterClass.MeleeWarrior;
    }

    private void CheckWeapon()
    {
        switch (playerClass)
        {
            case CharacterClass.MeleeWarrior:
                {
                    paladinSword.SetActive(true);
                    break;
                }
            case CharacterClass.RangedMage:
                {
                    paladinSword.SetActive(false);
                    break;
                }
        }
    }


    //-Animation Funcs-//
    private float GetAnimationLength(string animName)
    {
        foreach (AnimationClip animClip in animRC.animationClips)
        {
            if (animClip.name == animName)
            {
                return animClip.length;
            }
        }
        Debug.Log("Could not find animation!");
        return -1.0f;
    }

    private void AnimationTester(string animationStateName)
    {
        if (Input.GetButtonDown("Ability1"))
        {
            anim.Play(Animator.StringToHash("Base Layer." + animationStateName));
        }
    }

    private bool AnimatorIsPlaying(string stateName)
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsName(stateName);
    }

    //-Animation Events-//
    private void WeaponSwap()
    {
        switch (playerClass)
        {
            case CharacterClass.MeleeWarrior:
            {
                paladinSword.SetActive(true);
                break;
            }
            case CharacterClass.RangedMage:
            {
                paladinSword.SetActive(false);
                break;
            }
        }
    }

    public void ResetCombo()
    {
        if (IsWarrior())
        {
            meleeAttackCombo.amComboing = false;
            meleeAttackCombo.canStartCombo = true;
            meleeAttackCombo.isComboOver = true;
        }
        else
        {
            rangedAttackCombo.amComboing = false;
            rangedAttackCombo.canStartCombo = true;
            rangedAttackCombo.isComboOver = true;
        }
    }
}