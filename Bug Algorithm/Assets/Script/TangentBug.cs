using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using UnityEngine;

public class TangentBug : MonoBehaviour
{
	public Transform goalTransform;
	public Material lineMaterial;
	public Material PlayerB;
	private GameObject path;
	private GameObject sensor;
	public static int BLOCK = 5;
	private RaycastHit[] raycast;
	private LineRenderer[] line;
	private Rigidbody rigid;
	public float SPEED = 5f;
	public float lineWeight = 0.025f;
	public float lineLength = 2f;
	private bool isBoundaryFollowing = false;
	private bool isStuckWall = false;
	public float STUCK_TIME = 1.5f;
	private float accTime = 0f;
	private Vector3 localMinimumPoint;
	private Vector3 prevBlockPos;
	private bool localMinimumCheck = false;
	private Vector3 prevFramePoint;
	private Vector3 nextFramePoint;
	private bool isStop = false;
	private Vector3 enterPos = new Vector3();
	private int round = -1;
	private bool isChecked = false;
	private float framePerDistance = 0.4f;
	private bool isFirstFrame = true;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
		path = GameObject.Find("Path");
		raycast = new RaycastHit[360/BLOCK];
		line = new LineRenderer[360/BLOCK];
		rigid = GetComponent<Rigidbody>();
		sensor = GameObject.Find("Sensor");
		localMinimumPoint = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		prevBlockPos = this.transform.position;
		nextFramePoint = this.transform.position;

		for (int i = 0; i < line.Length; i++)
		{
			GameObject obj = new GameObject();
			obj.name = "line " + (i*BLOCK) + "˚";
			line[i] = obj.AddComponent<LineRenderer>();
			line[i].positionCount = 2;
			line[i].startWidth = lineWeight;
			line[i].endWidth = lineWeight;
			line[i].material = lineMaterial;
			line[i].startColor = new Color(1, 0, 0, 0.75f);
			line[i].endColor = new Color(1, 0, 0, 0.75f);
			line[i].transform.parent = sensor.transform;
		}
    }

    // Update is called once per frame
    void Update() {
		if (isStop) return;

		if (round >= 2) isStop = true;
		if (Vector3.Distance(goalTransform.position, this.transform.position) < framePerDistance * 1.25f) isStop = true;

		if (isStop)
		{
			this.gameObject.GetComponent<MeshRenderer>().material = PlayerB;
		}
	}

	private void FixedUpdate()
	{
		if (isStop) return;
		if (this.transform.position.y >= 0.30f) return;

		Vector3 goalPos = goalTransform.transform.position;
		Vector3 dir = (goalPos - this.transform.position).normalized;

		bool firstHit = false;
		bool prevHit = false;
		bool nextHit = false;
		bool hasObstacle = false;
		Vector3 nearestWallDir = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 minimumPoint = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 collisionPoint = new Vector3();

		for (int i = 0; i < line.Length; i++)
		{
			RaycastHit hit;
			float radian = (i * BLOCK) * Mathf.PI / 180f;
			Vector3 lineDirection = new Vector3(Mathf.Cos(radian), 0, Mathf.Sin(radian));

			line[i].SetPosition(0, this.transform.position);
			line[i].SetPosition(1, this.transform.position + lineLength * lineDirection);
			Physics.Raycast(this.transform.position, lineDirection, out hit, lineLength);
			raycast[i] = hit;

			// hit 검사
			if (i == 0)
			{
				firstHit = object.ReferenceEquals(raycast[i].collider, null);
				nextHit = object.ReferenceEquals(raycast[i].collider, null);
				continue;
			}

			prevHit = nextHit;
			nextHit = object.ReferenceEquals(raycast[i].collider, null);

			// 최단거리 검사
			if (prevHit != nextHit)
			{
				if (prevHit) collisionPoint = line[i-1].GetPosition(1);
				else if (nextHit) collisionPoint = line[i].GetPosition(1);

				float distance = GetViaDistance(this.transform.position, collisionPoint, goalTransform.position);
				float minimumDistance = GetViaDistance(this.transform.position, minimumPoint, goalTransform.position);
				if (minimumDistance > distance) minimumPoint = collisionPoint;
			}

			if (!nextHit && Vector3.Distance(nearestWallDir, this.transform.position) > Vector3.Distance(raycast[i].point, this.transform.position))
			{
				nearestWallDir = raycast[i].point;
			}

			float similarity = InnerProduct(lineDirection, dir);
			if (!nextHit && similarity >= Mathf.Cos(5 * Mathf.PI / 180f) * 0.95f)
			{
				hasObstacle = true;
			}
		}
		
		// 최단거리 검사
		if (firstHit != nextHit)
		{
			if (firstHit) collisionPoint = line[0].GetPosition(1);
			else if (nextHit) collisionPoint = line[line.Length - 1].GetPosition(1);

			float distance = GetViaDistance(this.transform.position, collisionPoint, goalTransform.position);
			float minimumDistance = GetViaDistance(this.transform.position, minimumPoint, goalTransform.position);
			if (minimumDistance > distance) minimumPoint = collisionPoint;
		}

		if (!firstHit && Vector3.Distance(nearestWallDir, this.transform.position) > Vector3.Distance(raycast[0].point, this.transform.position))
		{
			nearestWallDir = raycast[0].point;
		}

		if (isBoundaryFollowing && isStuckWall) return;

		if (!isBoundaryFollowing) {
			if (accTime >= STUCK_TIME)
			{
				float movementDistance = (this.transform.position - prevBlockPos).magnitude;
				if (!localMinimumCheck && accTime >= STUCK_TIME)
				{
					prevBlockPos = this.transform.position;
					localMinimumCheck = true;

					// 지역최소 탐색
					if (movementDistance <= framePerDistance * 0.95f)
					{
						isBoundaryFollowing = true;
						localMinimumPoint = this.transform.position;
					}
				}

				accTime = 0f;
			}
			else
			{
				localMinimumCheck = false;
			}
		}

		if (hasObstacle) {
			dir = (minimumPoint - this.transform.position).normalized;
		}

		// 가장 가까운 벽에 붙히기
		if (isBoundaryFollowing && !isStuckWall && Vector3.Distance(nextFramePoint, prevFramePoint) >= framePerDistance * 0.95f) {
			dir = (nearestWallDir - this.transform.position).normalized;
		}

		if (isBoundaryFollowing && !isStuckWall && Vector3.Distance(nextFramePoint, prevFramePoint) < framePerDistance * 0.95f)
		{
			localMinimumPoint = this.transform.position;
			isStuckWall = true;
			enterPos = this.transform.position;
			return;
		}

		rigid.MovePosition(this.transform.position + dir * SPEED * Time.fixedDeltaTime);
		Draw(this.transform.position, this.transform.position + dir * SPEED * Time.fixedDeltaTime, new Color(1, 1, 0, 0.5f));
		accTime += Time.fixedDeltaTime;

		prevFramePoint = nextFramePoint;
		nextFramePoint = this.transform.position;

		if (isFirstFrame) {
			isFirstFrame = false;
			framePerDistance = float.MinValue;
		}

		float dist = Vector3.Distance(nextFramePoint, prevFramePoint);
		if (framePerDistance < dist) framePerDistance = dist;
	}

	private void OnCollisionStay(Collision collision)
	{
		if (isStop) return;
		if (collision.contacts[0].otherCollider.CompareTag("GROUND")) return;
		if (!isBoundaryFollowing) return;
		if (!isStuckWall) return;

		if (Vector3.Distance(goalTransform.position, localMinimumPoint) > Vector3.Distance(goalTransform.position, this.transform.position) + (framePerDistance * 1.05f))
		{
			isBoundaryFollowing = false;
			isStuckWall = false;
			localMinimumPoint = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			round = -1;
			return;
		}

		Vector3 dirVec = collision.contacts[0].point - this.transform.position;
		dirVec.y = 0;
		Vector3 orthogonal = new Vector3(dirVec.z, 0, -dirVec.x) + (0.5f * dirVec);
		rigid.MovePosition(this.transform.position + orthogonal.normalized * SPEED * Time.fixedDeltaTime);
		Draw(this.transform.position, this.transform.position + orthogonal.normalized * SPEED *Time.fixedDeltaTime, new Color(1, 1, 0, 0.5f));

		prevFramePoint = nextFramePoint;
		nextFramePoint = this.transform.position;

		if (Vector3.Distance(enterPos, this.transform.position) <= framePerDistance * 1.05f) {
			if (!isChecked) {
				round++;
				isChecked = true;
			}
		} else {
			isChecked = false;
		}
	}

	private float GetViaDistance(Vector3 start, Vector3 via, Vector3 end) {
		return (via - start).magnitude + (end - via).magnitude;
	}

	private float InnerProduct(Vector3 v1, Vector3 v2) {
		return (v1.x * v2.x) + (v1.y * v2.y) + (v1.z * v2.z);
	}

	public void Draw(Vector3 start, Vector3 end, Color color)
	{
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
