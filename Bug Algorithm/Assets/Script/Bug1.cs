using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bug1 : MonoBehaviour
{
    public Transform goalTransform;
	public Material lineMaterial;
	public Material PlayerB;
    private Rigidbody rigid;
	private Vector3 startPos;
	private float SPEED = 5f;
	private int round = 0;
    private bool isOriginPoint = true;
    private bool isBoundaryFollowing = false;
	private Vector3 minimumPoint;
	private Vector3 prevFramePoint;
	private Vector3 nextFramePoint;
	private bool tryLeave = false;
	private GameObject path;
	private bool isStop = false;
	private float framePerDistance = 0.4f;
	private bool isFirstFrame = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
		nextFramePoint = this.transform.position;
		path = GameObject.Find("Path");
	}

    // Update is called once per frame
    void Update()
    {
		if (isStop) return;

		if (round >= 2) isStop = true;
		if (Vector3.Distance(goalTransform.position, this.transform.position) < framePerDistance * 1.05f) isStop = true;

		if (isStop) {
			this.gameObject.GetComponent<MeshRenderer>().material = PlayerB;
		}
	}

	private void FixedUpdate()
	{
		if (isStop) return;
		if (this.transform.position.y >= 0.30f) return;
		if (isBoundaryFollowing) return;

		Vector3 goalPos = goalTransform.transform.position;
		Vector3 dir = goalPos - this.transform.position;

		rigid.MovePosition(this.transform.position + dir.normalized * SPEED * Time.fixedDeltaTime);
		Draw(this.transform.position, this.transform.position + dir.normalized * SPEED * Time.fixedDeltaTime, new Color(1, 1, 0, 0.5f));

		prevFramePoint = nextFramePoint;
		nextFramePoint = this.transform.position;

		if (isFirstFrame) {
			isFirstFrame = false;
			framePerDistance = float.MinValue;
		}

		float dist = Vector3.Distance(nextFramePoint, prevFramePoint);
		if (framePerDistance < dist) framePerDistance = dist;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (isStop) return;
		if (collision.contacts[0].otherCollider.CompareTag("GROUND")) return;
		if (isBoundaryFollowing) return;

		isBoundaryFollowing = true;
		round = 0;
		isOriginPoint = true;
		startPos = this.transform.position;
		minimumPoint = float.MaxValue * new Vector3(1, 1, 1);
		tryLeave = false;
	}

	private void OnCollisionStay(Collision collision)
	{
		if (isStop) return;
		if (collision.contacts[0].otherCollider.CompareTag("GROUND")) return;
		isBoundaryFollowing = true;

		Vector3 current2goal = (this.transform.position - goalTransform.position);
		Vector3 minimum2goal = (minimumPoint - goalTransform.position);
		float error = Vector3.Distance(minimumPoint, this.transform.position);
		if (!tryLeave && round == 1 && error <= framePerDistance * 1.05f)
		{
			isBoundaryFollowing = false;
			tryLeave = true;
			return;
		}

		Vector3 dirVec = collision.contacts[0].point - this.transform.position;
		dirVec.y = 0;
		Vector3 orthogonal = new Vector3(dirVec.z, 0, -dirVec.x) + (0.2f * dirVec);
		rigid.MovePosition(this.transform.position + orthogonal.normalized * SPEED * Time.fixedDeltaTime);
		Draw(this.transform.position, this.transform.position + orthogonal.normalized * SPEED * Time.fixedDeltaTime, new Color(1, 1, 0, 0.5f));

		if (minimum2goal.magnitude > current2goal.magnitude)
		{
			minimumPoint = this.transform.position;
		}

		error = Vector3.Distance(startPos, this.transform.position);
		if (isOriginPoint == false && error < framePerDistance * 1.05f)
		{
			isOriginPoint = true;
			round++;
		}
		else if (isOriginPoint == true && error >= framePerDistance * 1.05f)
		{
			isOriginPoint = false;
		}

		prevFramePoint = nextFramePoint;
		nextFramePoint = this.transform.position;
	}

	public void Draw(Vector3 start, Vector3 end, Color color) {
		GameObject obj = new GameObject();
		LineRenderer line = obj.AddComponent<LineRenderer>();
		line.positionCount = 2;
		line.startWidth = 0.5f;
		line.endWidth = 0.5f;
		line.material = lineMaterial;
		line.startColor = color;
		line.endColor = color;
		line.transform.parent = path.transform;
		line.SetPosition(0, start);
		line.SetPosition(1, end);
	}
}
