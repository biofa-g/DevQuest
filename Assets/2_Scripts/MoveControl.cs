using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MoveControl : MonoBehaviour
{
    [Header("Preset Fields")]
    [SerializeField] private Rigidbody rigid;
    [SerializeField] private CapsuleCollider col;
    
    [Header("Settings")]
    public float moveSpeed;
    public float jumpAmount;

    //FSM(finite state machine)에 대한 더 자세한 내용은 세션 3회차에서 배울 것입니다!

    [SerializeField] private GameObject overUI;

    public enum State 
    {
        None,
        Idle,
        Jump
    }
    
    [Header("Debug")]
    public State state = State.None;
    public State nextState = State.None;
    public bool landed = false;
    public bool moving = false;
    [SerializeField] private Image healImage;
    [SerializeField] private TextMeshProUGUI healTime;

    private float stateTime;
    private Vector3 forward, right;

    private float coolTime = 5.0f;
    float healT = 5.0f;
    private bool skillCool = true;
    private bool isPlaying = true;
    private ParticleSystem heal;
    private void Start()
    {
        heal = this.transform.GetChild(1).gameObject.GetComponent<ParticleSystem>();
        rigid = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        
        state = State.None;
        nextState = State.Idle;
        stateTime = 0f;
        forward = transform.forward;
        right = transform.right;
    }

    private void Update()
    {
        //0. 글로벌 상황 판단
        stateTime += Time.deltaTime;
        CheckLanded();
        //insert code here...

        if (skillCool)
        {
            healT = 5.0f;
            healTime.text = "";
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                StartCoroutine(SkillCoolTime(coolTime));    //스킬 쿨 판단 코루틴
            }
        }
        if (!skillCool)
        {
            healImage.fillAmount -= 1.0f / coolTime * Time.deltaTime;   //쿨타임 이미지
            healT -= Time.deltaTime;
            healTime.text = MathF.Round(healT) + "s";
        }
        else healTime.text = "";

        if (Input.GetKeyDown(KeyCode.Escape) && isPlaying)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
            Time.timeScale = 0;
            isPlaying = false;
            overUI.SetActive(true);
        }
        //1. 스테이트 전환 상황 판단
        if (nextState == State.None) 
        {
            switch (state) 
            {
                case State.Idle:
                    if (landed) 
                    {
                        if (Input.GetKey(KeyCode.Space)) 
                        {
                            nextState = State.Jump;
                        }
                    }
                    break;
                case State.Jump:
                    if (landed) 
                    {
                        nextState = State.Idle;
                    }
                    break;
                //insert code here...
            }
        }
        
        //2. 스테이트 초기화
        if (nextState != State.None)
        {
            state = nextState;
            nextState = State.None;
            switch (state) 
            {
                case State.Jump:
                    var vel = rigid.velocity;
                    vel.y = jumpAmount;
                    rigid.velocity = vel;
                    break;
                //insert code here...
            }
            stateTime = 0f;
        }
        
        //3. 글로벌 & 스테이트 업데이트
        //insert code here...
    }

    IEnumerator SkillCoolTime(float time)
    {
        heal.Play();
        healImage.fillAmount = 1.0f;
        skillCool = false;
        yield return new WaitForSeconds(time);
        skillCool = true;
    }
    private void FixedUpdate()
    {
        UpdateInput();
    }

    public void ExitBtn()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    public void CloseUI() 
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1;
        isPlaying = true;
        overUI.SetActive(false);
    }

    private void CheckLanded() {
        //발 위치에 작은 구를 하나 생성한 후, 그 구가 땅에 닿는지 검사한다.
        //1 << 3은 Ground의 레이어가 3이기 때문, << 는 비트 연산자
        var center = col.bounds.center;
        var origin = new Vector3(center.x, center.y - ((col.height - 1f) / 2 + 0.15f), center.z);
        landed = Physics.CheckSphere(origin, 0.45f, 1 << 3, QueryTriggerInteraction.Ignore);
    }
    
    private void UpdateInput()
    {
        var direction = Vector3.zero;
        
        if (Input.GetKey(KeyCode.W)) direction += forward; //Forward
        if (Input.GetKey(KeyCode.A)) direction += -right; //Left
        if (Input.GetKey(KeyCode.S)) direction += -forward; //Back
        if (Input.GetKey(KeyCode.D)) direction += right; //Right
        
        direction.Normalize(); //대각선 이동(Ex. W + A)시에도 동일한 이동속도를 위해 direction을 Normalize
        
        transform.Translate( moveSpeed * Time.deltaTime * direction); //Move
    }
}
