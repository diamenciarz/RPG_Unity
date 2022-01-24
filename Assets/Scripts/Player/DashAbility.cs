using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashAbility : MonoBehaviour
{
    #region Serialization
    [Tooltip("The minimum time between dashes")]
    [SerializeField] float dashCooldown = 0.3f;
    [Tooltip("The travelled distance when dashing (in world units)")]
    [SerializeField] float dashLength = 2.5f;
    [Tooltip("The range from which this unit can dash")]
    [SerializeField] float dashRange = 2.5f;

    private float dashDuration = 0.5f;
    [HideInInspector]
    public float dashSpeed;
    #endregion

    #region Private variables
    private Vector3 dashDirection;
    private Coroutine dashCoroutine;

    private bool isDashing;
    private bool canDash = true;

    private Animator myAnimator;
    #endregion

    void Start()
    {
        myAnimator = GetComponent<Animator>();
        UpdatePlayerGameObject();
    }
    private void UpdatePlayerGameObject()
    {
        EventManager.TriggerEvent("UpdateDashSnapRange", dashRange);
    }

    private void Update()
    {
        CheckDash();
    }

    #region Dash
    private void CheckDash()
    {
        if (canDash && !isDashing)
        {
            CheckDashInput();
        }
    }
    private void CheckDashInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryToDash();
        }
    }
    private void TryToDash()
    {
        if (IsDashableObjectInRange())
        {
            canDash = false;
            EventManager.TriggerEvent("UpdateDashIcon", false);
            StartCoroutine(DashCooldownCoroutine());

            GameObject objectToDashThrough = StaticDataHolder.GetTheClosestDashableObject(transform.position, dashRange);
            DashThroughObject(objectToDashThrough);
        }
    }
    private bool IsDashableObjectInRange()
    {
        GameObject objectToDashThrough = StaticDataHolder.GetTheClosestDashableObject(transform.position, dashRange);
        return objectToDashThrough != null;
    }
    private void DashThroughObject(GameObject dashGO)
    {
        isDashing = true;
        dashDirection = HelperMethods.DeltaPosition(gameObject, dashGO).normalized;
        dashCoroutine = StartCoroutine(DashCoroutine());
    }
    private IEnumerator DashCoroutine()
    {
        myAnimator.SetBool("isDashing", true);
        yield return new WaitForSeconds(dashDuration);

        myAnimator.SetBool("isDashing", false);
        isDashing = false;
    }
    private IEnumerator DashCooldownCoroutine()
    {
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
        EventManager.TriggerEvent("UpdateDashIcon", true);
    }
    #endregion

    #region Accessor methods
    public bool GetCanDash()
    {
        return canDash;
    }
    public Vector3 GetDashVector()
    {
        const float rangeMultiplier = 2f; //Scales dash range to one map unit
        Vector3 dashVector = dashDirection * rangeMultiplier * dashSpeed * dashLength;
        return dashVector;
    }
    #endregion
}
