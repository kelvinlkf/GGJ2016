using UnityEngine;
using System.Collections;

[RequireComponent (typeof (LineRenderer))]
public class PlayerScript : MonoBehaviour {

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

    void Awake()
    {
        _transform = GetComponent<Transform>();
        _sphereCollider = GetComponent<SphereCollider>();
        _lineRenderer = GetComponent<LineRenderer>();
        _springJoint = GetComponent<SpringJoint>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _material = _meshRenderer.material;

        _lineRenderer.SetVertexCount(2);
    }

    void OnEnable()
    {
    }

	// Use this for initialization
	void Start () 
    {
        CenterHub = GameObject.FindGameObjectWithTag("Parent").GetComponent<Transform>();
        UpdateColor();
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

    public void SetLine()
    {
        _lineRenderer.SetPosition(0, _transform.position + ((_parentObj.position - _transform.position).normalized *_sphereCollider.radius));
        _lineRenderer.SetPosition(1, _parentObj.position + ((_transform.position - _parentObj.position).normalized * _parentsphereCollider.radius));
    }

    public void SetColor()
    {
        
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
        Debug.Log(_material.color.ToString());
    }
}
