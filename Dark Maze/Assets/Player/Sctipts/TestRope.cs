using UnityEngine;

public class TestRope : MonoBehaviour
{
    [SerializeField] GameObject playerObject;
    [SerializeField] GameObject handObject;
    [SerializeField] GameObject ropeObject;
    [SerializeField] GameObject ropeTopObject;
    [SerializeField] GameObject target;
    float handRadius = 0.5f;
    float ropeRadius = 1f;
    float handSpeed = 370f;
    float ropeSpeed = 360f;

    float handAngle;
    float ropeAngle;

    [SerializeField] LineRenderer line;
    Vector3[] points;
    [SerializeField] int segmentCount = 2;
    [SerializeField] float followSpeed = 10f;
    void Start()
    {
        line.positionCount = segmentCount;
        points = new Vector3[segmentCount];
        for (int i = 0; i < segmentCount; i++) points[i] = playerObject.transform.position;
    }

    void Update()
    {
        // •ûŒü‘I‘ð’†‚ÌAnimation
        IdleRopeAnimation();
        RotationRoteTopObject();
        //TargetAnimation();
    }


    void LateUpdate()
    {

    }

    void IdleRopeAnimation()
    {
        handAngle += handSpeed * Time.deltaTime;

        float handRad = handAngle * Mathf.Deg2Rad;
        Vector3 handOffset = new Vector3(Mathf.Cos(handRad) * handRadius, playerObject.transform.position.y + 1f, Mathf.Sin(handRad) * handRadius);
        handObject.transform.position = handOffset;

        ropeAngle += ropeSpeed * Time.deltaTime;

        float ropeRad = ropeAngle * Mathf.Deg2Rad;
        Vector3 ropeOffset = new Vector3(Mathf.Cos(ropeRad) * ropeRadius, playerObject.transform.position.y + 2f, Mathf.Sin(ropeRad) * ropeRadius);
        ropeObject.transform.position = ropeOffset;

        line.SetPosition(0, handObject.transform.position);
        line.SetPosition(line.positionCount - 1, ropeObject.transform.position);

        for (int i = 0; i < line.positionCount; i++)
        {
            float t = i / 9f;
            Vector3 pos = Vector3.Lerp(handObject.transform.position, ropeObject.transform.position, t);
            pos.x += Mathf.Cos(t * Mathf.PI) * 0.2f;
            pos.z += Mathf.Sin(t * Mathf.PI) * 0.2f;
            line.SetPosition(i, pos);
            if (i == line.positionCount - 1)
            {

            }
        }
    }

    void RotationRoteTopObject()
    {
        ropeObject.transform.eulerAngles += new Vector3 (0f, Time.deltaTime * 200f, 0f);
    }

    void TargetAnimation()
    {
        line.positionCount = 2;

        ropeTopObject.transform.position = target.transform.position;
        line.SetPosition(0, target.transform.position);
        line.SetPosition(1, handObject.transform.position);
    }
}
