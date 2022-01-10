using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CubeSphere : MonoBehaviour {


	public int gridSubdivision;
	public float cubeSize = 1.5f;
	public float sphereRadius = 1;

	[Range(0.0f, 1.0f)]
	public float sphereToCubeRatio;

	private float _oldRatio;

	private Mesh _mesh;
	private Vector3[] _sphereVertices;
	private Vector3[] _cubeVertices;
	private Vector3[] _normals;
	private Color32[] _cubeUV;

	private void Awake () {
		Generate();
	}

	private void Generate () {
		// Generate Mesh to store sphere vertices
		GetComponent<MeshFilter>().mesh = _mesh = new Mesh();
		_mesh.name = "Procedural Cube Sphere";
		CreateSphereVertices();
		CreateCubeVertices();
		CreateTriangles();
		CreateColliders();
	}

	private void Update()
	{
		// if (sphereToCubeRatio != _oldRatio)
		// {
		// 	_oldRatio = sphereToCubeRatio;

			var vertices = new Vector3[_sphereVertices.Length];
			for (var v = 0; v < _sphereVertices.Length;v++)
			{
				Vector3 newVertices = Vector3.Lerp(_sphereVertices[v], _cubeVertices[v], sphereToCubeRatio);
				vertices[v] = newVertices;
			}
			_mesh.vertices = vertices;
		//}

	}

	public void AdjustMorp(float newRatio)
	{
		sphereToCubeRatio = newRatio;
	}

	private void CreateCubeVertices()
	{
		/* The cube is created using the number subdivisions and a set length (this
		should be chanaged to be adaptable). Now that the sphere and the cube have
		equal vertices and faces, we can lerp between the two.*/
		const int cornerVertices = 8;
		var edgeVertices = (gridSubdivision * 3 - 3) * 4;
		var faceVertices = (
			(gridSubdivision - 1) * (gridSubdivision - 1) +
			(gridSubdivision - 1) * (gridSubdivision - 1) +
			(gridSubdivision - 1) * (gridSubdivision - 1)) * 2;
		_cubeVertices = new Vector3[cornerVertices + edgeVertices + faceVertices];

		var v = 0;
		float gridStep = cubeSize / (float)gridSubdivision * 2;
		Vector3 centerOffset = Vector3.one * gridStep * gridSubdivision / (float)2;

		for (var y = 0; y <= gridSubdivision; y++) {
			for (var x = 0; x <= gridSubdivision; x++) {
				_cubeVertices[v++] = new Vector3(x, y, 0) * gridStep - centerOffset;
			}
			for (var z = 1; z <= gridSubdivision; z++) {
				_cubeVertices[v++] = new Vector3(gridSubdivision, y, z) * gridStep - centerOffset;
			}
			for (var x = gridSubdivision - 1; x >= 0; x--) {
				_cubeVertices[v++] = new Vector3(x, y, gridSubdivision) * gridStep - centerOffset;
			}
			for (var z = gridSubdivision - 1; z > 0; z--) {
				_cubeVertices[v++] = new Vector3(0, y, z) * gridStep - centerOffset;
			}
		}
		for (var z = 1; z < gridSubdivision; z++) {
			for (var x = 1; x < gridSubdivision; x++) {
				_cubeVertices[v++] = new Vector3(x, gridSubdivision, z) * gridStep - centerOffset;
			}
		}
		for (var z = 1; z < gridSubdivision; z++) {
			for (var x = 1; x < gridSubdivision; x++) {
				_cubeVertices[v++] = new Vector3(x, 0, z) * gridStep - centerOffset;
			}
		}
	}
	private void CreateSphereVertices () {
		/* The size and shape of the sphere is calculated using the radius and the
		grid subdivision. SetVertex is used to normailze the values to a unit circle*/
		int cornerVertices = 8;
		int edgeVertices = (gridSubdivision + gridSubdivision + gridSubdivision - 3) * 4;
		int faceVertices = (
			(gridSubdivision - 1) * (gridSubdivision - 1) +
			(gridSubdivision - 1) * (gridSubdivision - 1) +
			(gridSubdivision - 1) * (gridSubdivision - 1)) * 2;
		_sphereVertices = new Vector3[cornerVertices + edgeVertices + faceVertices];
		_normals = new Vector3[_sphereVertices.Length];
		_cubeUV = new Color32[_sphereVertices.Length];

		int v = 0;
		for (int y = 0; y <= gridSubdivision; y++) {
			for (int x = 0; x <= gridSubdivision; x++) {
				SetVertex(v++, x, y, 0);
			}
			for (int z = 1; z <= gridSubdivision; z++) {
				SetVertex(v++, gridSubdivision, y, z);
			}
			for (int x = gridSubdivision - 1; x >= 0; x--) {
				SetVertex(v++, x, y, gridSubdivision);
			}
			for (int z = gridSubdivision - 1; z > 0; z--) {
				SetVertex(v++, 0, y, z);
			}
		}
		for (int z = 1; z < gridSubdivision; z++) {
			for (int x = 1; x < gridSubdivision; x++) {
				SetVertex(v++, x, gridSubdivision, z);
			}
		}
		for (int z = 1; z < gridSubdivision; z++) {
			for (int x = 1; x < gridSubdivision; x++) {
				SetVertex(v++, x, 0, z);
			}
		}

		_mesh.vertices = _sphereVertices;
		_mesh.normals = _normals;
		_mesh.colors32 = _cubeUV;
	}

	private void SetVertex (int i, int x, int y, int z) {
		/* SetVertex will take a given input of i, x, y, z and normalize those
		input over the radius of the CubeSphere. Without this function the vertices
		of the sphere would not be evenly spaced around the cube*/
		Vector3 v = new Vector3(x, y, z) * 2f / gridSubdivision - Vector3.one;
		float x2 = v.x * v.x;
		float y2 = v.y * v.y;
		float z2 = v.z * v.z;
		Vector3 s;
		s.x = v.x * Mathf.Sqrt(1f - y2 / 2f - z2 / 2f + y2 * z2 / 3f);
		s.y = v.y * Mathf.Sqrt(1f - x2 / 2f - z2 / 2f + x2 * z2 / 3f);
		s.z = v.z * Mathf.Sqrt(1f - x2 / 2f - y2 / 2f + x2 * y2 / 3f);

		_normals[i] = s;
		_sphereVertices[i] = _normals[i] * sphereRadius;
		_cubeUV[i] = new Color32((byte)x, (byte)y, (byte)z, 0);
	}

	private void CreateTriangles () {
		/* This creates a triangled mesh to connect the vertices of the cube and
		give it a surface. This is done it parts with top, bottom, forward, back,
		left, and right. This is done to more easily see the cube sphere during the
		lerp.*/
		int[] trianglesZ = new int[(gridSubdivision * gridSubdivision) * 12];
		int[] trianglesX = new int[(gridSubdivision * gridSubdivision) * 12];
		int[] trianglesY = new int[(gridSubdivision * gridSubdivision) * 12];
		int ring = (gridSubdivision + gridSubdivision) * 2;
		int tZ = 0, tX = 0, tY = 0, v = 0;

		for (int y = 0; y < gridSubdivision; y++, v++) {
			for (int q = 0; q < gridSubdivision; q++, v++) {
				tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
			}
			for (int q = 0; q < gridSubdivision; q++, v++) {
				tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
			}
			for (int q = 0; q < gridSubdivision; q++, v++) {
				tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
			}
			for (int q = 0; q < gridSubdivision - 1; q++, v++) {
				tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
			}
			tX = SetQuad(trianglesX, tX, v, v - ring + 1, v + ring, v + 1);
		}

		tY = CreateTopFace(trianglesY, tY, ring);
		tY = CreateBottomFace(trianglesY, tY, ring);

		_mesh.subMeshCount = 3;
		_mesh.SetTriangles(trianglesZ, 0);
		_mesh.SetTriangles(trianglesX, 1);
		_mesh.SetTriangles(trianglesY, 2);
	}

	private int CreateTopFace (int[] triangles, int t, int ring) {
		int v = ring * gridSubdivision;
		for (int x = 0; x < gridSubdivision - 1; x++, v++) {
			t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + ring);
		}
		t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + 2);

		int vMin = ring * (gridSubdivision + 1) - 1;
		int vMid = vMin + 1;
		int vMax = v + 2;

		for (int z = 1; z < gridSubdivision - 1; z++, vMin--, vMid++, vMax++) {
			t = SetQuad(triangles, t, vMin, vMid, vMin - 1, vMid + gridSubdivision - 1);
			for (int x = 1; x < gridSubdivision - 1; x++, vMid++) {
				t = SetQuad(
					triangles, t,
					vMid, vMid + 1, vMid + gridSubdivision - 1, vMid + gridSubdivision);
			}
			t = SetQuad(triangles, t, vMid, vMax, vMid + gridSubdivision - 1, vMax + 1);
		}

		int vTop = vMin - 2;
		t = SetQuad(triangles, t, vMin, vMid, vTop + 1, vTop);
		for (int x = 1; x < gridSubdivision - 1; x++, vTop--, vMid++) {
			t = SetQuad(triangles, t, vMid, vMid + 1, vTop, vTop - 1);
		}
		t = SetQuad(triangles, t, vMid, vTop - 2, vTop, vTop - 1);

		return t;
	}

	private int CreateBottomFace (int[] triangles, int t, int ring) {
		int v = 1;
		int vMid = _sphereVertices.Length - (gridSubdivision - 1) * (gridSubdivision - 1);
		t = SetQuad(triangles, t, ring - 1, vMid, 0, 1);
		for (int x = 1; x < gridSubdivision - 1; x++, v++, vMid++) {
			t = SetQuad(triangles, t, vMid, vMid + 1, v, v + 1);
		}
		t = SetQuad(triangles, t, vMid, v + 2, v, v + 1);

		int vMin = ring - 2;
		vMid -= gridSubdivision - 2;
		int vMax = v + 2;

		for (int z = 1; z < gridSubdivision - 1; z++, vMin--, vMid++, vMax++) {
			t = SetQuad(triangles, t, vMin, vMid + gridSubdivision - 1, vMin + 1, vMid);
			for (int x = 1; x < gridSubdivision - 1; x++, vMid++) {
				t = SetQuad(
					triangles, t,
					vMid + gridSubdivision - 1, vMid + gridSubdivision, vMid, vMid + 1);
			}
			t = SetQuad(triangles, t, vMid + gridSubdivision - 1, vMax + 1, vMid, vMax);
		}

		int vTop = vMin - 1;
		t = SetQuad(triangles, t, vTop + 1, vTop, vTop + 2, vMid);
		for (int x = 1; x < gridSubdivision - 1; x++, vTop--, vMid++) {
			t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vMid + 1);
		}
		t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vTop - 2);

		return t;
	}

	private static int
	SetQuad (int[] triangles, int i, int v00, int v10, int v01, int v11) {
		triangles[i] = v00;
		triangles[i + 1] = triangles[i + 4] = v01;
		triangles[i + 2] = triangles[i + 3] = v10;
		triangles[i + 5] = v11;
		return i + 6;
	}

	private void CreateColliders () {
		MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
		meshCollider.sharedMesh = _mesh;
	}

//	private void OnDrawGizmos () {
//		if (vertices == null) {
//			return;
//		}
//		for (int i = 0; i < vertices.Length; i++) {
//			Gizmos.color = Color.black;
//			Gizmos.DrawSphere(vertices[i], 0.1f);
//			Gizmos.color = Color.yellow;
//			Gizmos.DrawRay(vertices[i], normals[i]);
//		}
//	}
}
