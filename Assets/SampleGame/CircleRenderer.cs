using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class CircleRenderer : MonoBehaviour {

    public float radius = 0.5f;
    public Vector3 center;
    public int numSegments;
    public float timeToGrow = 1.5f;//measured in seconds
    [SerializeField] float currentRadius = 0.5f;
    public float targetRadius = 8f;//how big the circle should be in timeToGrow seconds

    private LineRenderer line;
    private CircleCollider2D col;

    private Vector3[] points;
    [SerializeField]
    private Vector3[] normalizedPoints;

    [SerializeField]
    private float radiusRate;
    private bool isExpanding = false;

	void Start () {
        line = GetComponent<LineRenderer>();
        col = GetComponent<CircleCollider2D>();

        line.positionCount = numSegments + 1;
        normalizedPoints = new Vector3[line.positionCount];
        points = new Vector3[line.positionCount];
        currentRadius = col.radius = radius;
        col.isTrigger = true;

        //line.enabled = false;
        col.enabled = false;

        calculateNormalizedCircle();
        drawCircle();
	}

    //Unit circle! radius = 1
    void calculateNormalizedCircle()
    {
        float angle = 0f;
        float deltaTheta = 360f / numSegments;
        for(int i = 0;i < line.positionCount;++i)
        {
            float radians = Mathf.Deg2Rad * angle;
            float x = Mathf.Cos(radians);
            float y = Mathf.Sin(radians);
            normalizedPoints[i] = new Vector3(x, y, 0f);
            points[i] = new Vector3(x, y, 0f) * radius;
            angle += deltaTheta;
        }
    }

    void drawCircle()
    {
        for(int i = 0;i < line.positionCount;++i)
            points[i].Set(normalizedPoints[i].x * currentRadius + center.x, normalizedPoints[i].y * currentRadius + center.y, 0f);

        line.SetPositions(points);
    }

    IEnumerator m_expandCircle()
    {
        isExpanding = true;
        float radiusDiff = targetRadius - radius;
        radiusRate = (radiusDiff / timeToGrow) * Time.deltaTime;
        while (currentRadius < targetRadius)
        {
            col.radius += radiusRate;
            currentRadius += radiusRate;
            drawCircle();
            yield return null;
        }

        //floor to match targetRadius exactly
        currentRadius = Mathf.Floor(currentRadius);
        col.radius = currentRadius;
        drawCircle();
        isExpanding = false;
        resetSignal();
        GetComponent<InputMap>().SetKey(InputMap.EMIT_DASH, false);
        GetComponent<InputMap>().SetKey(InputMap.EMIT_JUMP, false);
        yield return null;
    }

    public void broadcastSignal()
    {
        if(!isExpanding)
        {
            line.enabled = true;
            col.enabled = true;
            StartCoroutine(m_expandCircle());
        }
    }

    public void resetSignal()
    {
        currentRadius = radius;
        col.radius = radius;
        //line.enabled = false;
        drawCircle();
        col.enabled = false;

    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(!collision.gameObject.Equals(this.gameObject) && collision.tag.Equals("Player"))
        {
            Debug.Log(gameObject.name + "==" + collision.gameObject.name);

            PlayerController receiver = collision.GetComponent<PlayerController>();
            receiver.processSignal(this.gameObject);
        }
    }
}
