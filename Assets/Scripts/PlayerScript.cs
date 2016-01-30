using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[RequireComponent (typeof (LineRenderer))]
public class PlayerScript : NetworkBehaviour {

    private LineRenderer _lineRenderer;
    private SpringJoint _springJoint;
    private MeshRenderer _meshRenderer;
    private Material _material;
    public Color mColor;

    private Transform _transform;
    private SphereCollider _sphereCollider;
    private Transform _parentObj;
    private SphereCollider _parentsphereCollider;

    public float Hvalue;
    private int mPlayerId;
    public Vector3 posRange;

    void Awake()
    {
        _transform = GetComponent<Transform>();
        SetPosition();
        _sphereCollider = GetComponent<SphereCollider>();
        _lineRenderer = GetComponent<LineRenderer>();
        _springJoint = GetComponent<SpringJoint>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _material = _meshRenderer.material;

        _lineRenderer.SetVertexCount(2);
    }

	[Command]
    void CmdDoSomething ()
    {
    	Debug.Log("do something");
    }

    void OnEnable()
    {
    }

	// Use this for initialization
	void Start () 
    {
        CenterHub = GameObject.FindGameObjectWithTag("MainGlobe").GetComponent<Transform>();
        UpdateColor();

		CmdDoSomething();
	}
	
	// Update is called once per frame
	void Update () 
    {
        SetLine();
	}
        
    //! Get, Set Parent
    public Transform CenterHub
    {
        get
        {
            return _parentObj;
        }
        set
        {
            _parentObj = value;
            _parentsphereCollider = _parentObj.GetComponent<SphereCollider>();
            _springJoint.connectedBody = _parentObj.GetComponent<Rigidbody>();
        }
    }

    public void SetPosition()
    {
        _transform.position = Random.insideUnitSphere * Random.Range(6f, 11f);
    }

    public void SetLine()
    {
        _lineRenderer.SetPosition(0, _transform.position + ((_parentObj.position - _transform.position).normalized *_sphereCollider.radius));
        _lineRenderer.SetPosition(1, _parentObj.position + ((_transform.position - _parentObj.position).normalized * _parentsphereCollider.radius)* _parentObj.localScale.x);
    }

    public void SetID(int _value)
    {
        mPlayerId = _value;
    }

    public void UpdateColor()
    {
        Vector3 direction = _parentObj.position - _transform.position;
        float angle = Vector3.Angle(Vector3.up, _transform.position);
        //Quaternion qua = Quaternion.Euler(new Vector3(0f,0f,direction));
        //float angle = Vector3.Angle(_parentObj.position, _transform.position);
        //Debug.Log(angle);
        HSBColor newColor = new HSBColor(Mathf.Sin (angle + Mathf.PI / 2 ), 1f, 1f);
        _material.color = newColor.ToColor();
    }

    void OnStartClient ()
    {
		Debug.Log("OnStartClient");
    }

	void OnServerConnect (NetworkConnection conn)
    {
		Debug.Log("OnServerConnect");
    }

	void OnServerDisconnect ()
    {
		Debug.Log("OnServerDisconnect");
    }

	void OnConnectedToServer ()
    {
		Debug.Log("OnConnectedToServer");
    }

	void OnPlayerConnected(NetworkPlayer player)
	{
		Debug.Log("Player connect");
	}

	void OnPlayerDisconnected(NetworkPlayer player)
	{
		Debug.Log("Player dc");

		// Cleanup stuff, from http://docs.unity3d.com/ScriptReference/MonoBehaviour.OnPlayerDisconnected.html
		Network.RemoveRPCs(player);
        Network.DestroyPlayerObjects(player);
	}
}
