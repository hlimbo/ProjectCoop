using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class CircleRenderer : MonoBehaviour {

    public float radius = 2f;
    public Vector3 center;
    public int numSegments;
    public float timeToGrow = 1.5f;//measured in seconds
    public float targetRadius = 8f;//how big the circle should be in timeToGrow seconds

    private LineRenderer line;
    private CircleCollider2D col;

    private Vector3[] points;
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
        col.radius = radius;
        col.isTrigger = true;

        calculateNormalizedCircle();
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
            points[i].Set(normalizedPoints[i].x * radius + center.x, normalizedPoints[i].y * radius + center.y, 0f);

        line.SetPositions(points);
    }

    IEnumerator m_expandCircle()
    {
        isExpanding = true;
        float radiusDiff = targetRadius - radius;
        radiusRate = (radiusDiff / timeToGrow) * Time.deltaTime;
        while (radius < targetRadius)
        {
            col.radius += radiusRate;
            radius += radiusRate;
            drawCircle();
            yield return null;
        }

        //floor to match targetRadius exactly
        radius = Mathf.Floor(radius);
        col.radius = radius;
        drawCircle();
        isExpanding = false;
        yield return null;
    }

    public void expandCircle()
    {
        if(!isExpanding)
        {
            StartCoroutine(m_expandCircle());
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag.Equals("Player"))
        {
            //TODO...send player a message to do action once player receives this signal
        }
    }
}
