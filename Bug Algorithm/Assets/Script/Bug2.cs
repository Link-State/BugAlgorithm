using UnityEngine;

public class Bug2 : MonoBehaviour
{
    public Transform goalTransform;
	public Material lineMaterial;
	public Material PlayerB;
	private Rigidbody rigid;
	private Vector3 startPosition = new Vector3();
	private Vector3 mLine = new Vector3(0, 0, 0);
	private float SPEED = 5f;
	private bool isBoundaryFollowing = false;
	private float prevSimilarity = 0.0f;
	private float nextSimilarity = 1.0f;
	private bool tryLeave = false;
	private int round = -1;
	private GameObject path;
	private bool isStop = false;
	private Vector3 prevFramePoint;
	private Vector3 nextFramePoint;
	private float framePerDistance = 0.4f;
	private float framePerSimilarity = 0.95f;
	private bool isFirstFrame = true;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
        rigid = GetComponent<Rigidbody>();
		path = GameObject.Find("Path");
		startPosition.x = this.transform.position.x;
		startPosition.y = 0.25f;
		startPosition.z = this.transform.position.z;
		nextFramePoint = this.transform.position;
		mLine = (goalTransform.position - startPosition).normalized;
    }

    // Update is called once per frame
    void Update()
    {
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
		if (isBoundaryFollowing) return;

		float error = Vector3.Distance(goalTransform.position, this.transform.position);
		if (error <= framePerDistance * 1.25f) return;

		rigid.MovePosition(this.transform.position + mLine * SPEED * Time.fixedDeltaTime);
		Draw(this.transform.position, this.transform.position + mLine * SPEED * Time.fixedDeltaTime, new Color(1, 1, 0, 0.5f));

		prevFramePoint = nextFramePoint;
		nextFramePoint = this.transform.position;

		if (isFirstFrame) {
			isFirstFrame = false;
			framePerDistance = float.MinValue;
			framePerSimilarity = float.MaxValue;
		}

		float dist = Vector3.Distance(nextFramePoint, prevFramePoint);
		Vector3 normalVec = new Vector3(prevFramePoint.z, 0, -prevFramePoint.x);
		normalVec = normalVec.normalized * dist;
		Vector3 deltaVector = prevFramePoint + normalVec;
		float simr = InnerProduct(prevFramePoint.normalized, deltaVector.normalized);
		if (framePerDistance < dist) framePerDistance = dist;
		if (framePerSimilarity > simr) framePerSimilarity = simr;
	}

	private void OnCollisionExit(Collision collision)
	{
		if (isStop) return;
		if (isBoundaryFollowing) return;

		round = -1;
	}

	private void OnCollisionStay(Collision collision)
	{
		if (isStop) return;
		if (collision.contacts[0].otherCollider.CompareTag("GROUND")) return;

		Vector3 dir = (this.transform.position - startPosition).normalized;
		prevSimilarity = nextSimilarity;
		nextSimilarity = InnerProduct(mLine, dir);
		float difference = nextSimilarity - prevSimilarity;

		if (nextSimilarity >= framePerSimilarity * 0.95f && difference <= 0.0f) {
			if (!tryLeave)
			{
				tryLeave = true;
				isBoundaryFollowing = false;
				round++;
				return;
			}
		} else {
			tryLeave = false;
		}

		isBoundaryFollowing = true;

		Vector3 dirVec = collision.contacts[0].point - this.transform.position;
		dirVec.y = 0;
		Vector3 orthogonal = new Vector3(dirVec.z, 0, -dirVec.x) + (0.5f * dirVec);
		rigid.MovePosition(this.transform.position + orthogonal.normalized * SPEED * Time.fixedDeltaTime);
		Draw(this.transform.position, this.transform.position + orthogonal.normalized * SPEED * Time.fixedDeltaTime, new Color(1, 1, 0, 0.5f));

		prevFramePoint = nextFramePoint;
		nextFramePoint = this.transform.position;
	}

	private float InnerProduct(Vector3 v1, Vector3 v2)
	{
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
