using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshDeformer : MonoBehaviour
{
	public float springForce = 20f;
	public float damping = 5f;

    Mesh deformingMesh;

    Vector3[] originalVertices;
	Vector3[] displacedVertices;
	Vector3[] vertexVelocities;

	float uniformScale = 1f;

	void Start()
	{
		deformingMesh = GetComponent<MeshFilter>().mesh;
		
		displacedVertices = deformingMesh.vertices;
		originalVertices = deformingMesh.vertices;
		
		vertexVelocities = new Vector3[deformingMesh.vertices.Length];
	}

	void Update()
	{
		//uniformScale = transform.localScale.x;

		for (int i = 0; i < displacedVertices.Length; i++)
		{
			Vector3 velocity = vertexVelocities[i];
            Vector3 displacement = displacedVertices[i] - originalVertices[i];
            velocity -= displacement * springForce * Time.deltaTime;
            vertexVelocities[i] = velocity;
            velocity -= displacement * springForce * Time.deltaTime;

            {
                //(Time.deltaTime / uniformScale);

                //Vector3 displacement = displacedVertices[i] - originalVertices[i];

                //displacement *= uniformScale;
                //velocity -= displacement * springForce * Time.deltaTime;
                //velocity *= 1f - damping * Time.deltaTime;

                //vertexVelocities[i] = velocity;
            }
        }

        deformingMesh.SetVertices(displacedVertices);
		deformingMesh.RecalculateNormals();
	}

	public void AddDeformingForce(Vector3 point, float force)
	{
		// Transform point from world space to local space (our vertices are in local space)
		point = transform.InverseTransformPoint(point);

		for (int i = 0; i < displacedVertices.Length; i++)
		{
			Vector3 pointToVertex = displacedVertices[i] - point;

			float mass = 1;
			//float acceleration = force / mass;
            float attenuatedForce = force / (1f + pointToVertex.sqrMagnitude);
            float deltaVelocity = attenuatedForce * Time.deltaTime;          
            
			vertexVelocities[i] += pointToVertex.normalized * deltaVelocity;

			{
				//float attenuatedForce = force / (1f + pointToVertex.sqrMagnitude);
				//pointToVertex *= uniformScale;
			}
		}
	}
}