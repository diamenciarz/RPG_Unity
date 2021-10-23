using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : BasicProjectileController
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    protected override void Update()
    {
        MoveOneStep();
    }
    private void MoveOneStep()
    {
        Vector3 moveOneStepVector = StaticDataHolder.GetMoveVectorInDirection(speed, transform.rotation.eulerAngles.z);
        transform.position += moveOneStepVector * Time.deltaTime;
    }
}
