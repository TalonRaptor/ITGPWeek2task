using UnityEngine;
using System.Collections;

public class PhysControllerFP : MonoBehaviour 
{
	private Rigidbody move;
	private Vector3 movInputs;
	public Vector3 movePos;
	private Vector3 lookInputs;
	private Quaternion headRot;
	public Transform head;
	public float sinceLastGrounded = 0;
	public float moveSpeed = 10f;
	public float jumpHeight = 15f;
	public float gravity = 15f;
	public enum viewType {FPS, Orthographic, StaticCamera};
	public viewType _ViewType;
	public float orthographicSize = 10;
	private GameObject orthoDirection;
	public GameObject Shield;
	public bool keyboardOnly = false;
	public float keyboardSensitivity = 3f;
	// Use this for initialization - dont drink cheap gin and code
	void Awake () 
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		move = this.GetComponents<Rigidbody>()[0];
		if(_ViewType == viewType.Orthographic  || _ViewType == viewType.StaticCamera)
		{
			orthoDirection = new GameObject();
			if(Shield != null)
			{
				Shield.transform.parent = this.gameObject.transform;
				Shield.transform.position = this.gameObject.transform.position + new Vector3 (0, 1, 2);
			}
			else
				Debug.LogError("Your shield GameObject slot is unnassigned on your PhysicsPlayer");
			head.GetComponent<Camera>().orthographic = true;
			head.GetComponent<Camera>().orthographicSize = orthographicSize;
			head.transform.rotation = Quaternion.Euler(90, 0,0);
			head.transform.localPosition = new Vector3(0,orthographicSize,0);


			orthoDirection.transform.position = head.position;
			orthoDirection.transform.rotation = Quaternion.Euler(0, 0,0);
			orthoDirection.transform.parent = head.transform;

		}
		if(_ViewType == viewType.StaticCamera)
		{
			head.GetComponent<Camera>().enabled = false;
			head.transform.parent = null;
			head.GetComponent<AudioListener>().enabled = false;
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		//pooling inputs
		movInputs.x = Input.GetAxis("Horizontal");
		movInputs.z = Input.GetAxis("Vertical");
		if(_ViewType == viewType.FPS)
		{
			if(!keyboardOnly)
			{
				lookInputs.y += Input.GetAxis("Mouse X");
				lookInputs.x -= Input.GetAxis("Mouse Y");
			}
			else
			{
				lookInputs.y += Input.GetAxis("HorizontalTwo") * keyboardSensitivity;
				lookInputs.x -= Input.GetAxis("VerticalTwo") * keyboardSensitivity;
			}
		}
		else
		{
			if(!keyboardOnly)
				lookInputs.y += Input.GetAxis("Mouse X");
		
			else
				lookInputs.y += Input.GetAxis("HorizontalTwo") * keyboardSensitivity;
		}

		//detects input for jump, runs jump function
		if(Input.GetButtonDown("Jump") && sinceLastGrounded < 0.1f)
			Jump();

		if(Input.GetButtonDown("Fire1"))
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}

		Vector3 newDir = Vector3.zero;

		if(_ViewType == viewType.FPS)
			newDir = head.transform.TransformDirection(movInputs);
		
		else if(_ViewType != viewType.StaticCamera)
			newDir = orthoDirection.transform.TransformDirection(movInputs);

		else
			newDir = transform.TransformDirection(movInputs);
		//converting inputs into player Direction
		movePos = new Vector3(newDir.x  * moveSpeed , movePos.y, newDir.z * moveSpeed);

		//applying gravity
		if(sinceLastGrounded <= 0.1f)
		{
			movePos.y -= 0.2f * Time.deltaTime;
			movePos.y = Mathf.Clamp(movePos.y, -0.1f, 50f);
		}
		else
			movePos.y -= gravity * Time.deltaTime;


		//clamping camera y rotation
		lookInputs.x = Mathf.Clamp(lookInputs.x, -89, 89);

		//converting vector to Quaternion Euler
		headRot = Quaternion.Euler(lookInputs);

		//rotating the camera
			if(_ViewType == viewType.FPS)
				head.rotation = headRot;
			else if(_ViewType == viewType.Orthographic || _ViewType == viewType.StaticCamera)
				transform.rotation = headRot;
		sinceLastGrounded += Time.deltaTime;
	}

	void Jump()
	{
		movePos.y += jumpHeight;
	}

	void FixedUpdate()
	{
		move.velocity = movePos;
	}

	void OnCollisionStay(Collision col)
	{
		foreach (var point in col.contacts)
		{
			if (point.normal.y > 0.2f && point.point.y < GetComponent<Collider>().bounds.center.y)
				sinceLastGrounded = 0;
		}
		//do stuff
	}

}
