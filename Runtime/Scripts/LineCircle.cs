using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace LineCircles
{
	public class LineCircle : MonoBehaviour
	{
		/// <summary>
		/// Compute Buffer containing time snapshots in GPU
		/// </summary>
		private ComputeBuffer _snapshotBuffer;

		/// <summary>

		/// <summary>
		/// Buffer containing all the things necessary to build the mesh (for now, vertex position + color)
		/// </summary>
		private ComputeBuffer _vertexBuffer;

		private ComputeBuffer _fillArgsBuffer;

		private ComputeBuffer _lineArgsBuffer;
		
		private ComputeBuffer _latestVerticesBuffer;

		private Mesh _fillMesh;
		private Mesh _lineMesh;
		private int _setLineCount;

		private ComputeShader _snapshotCompute;
		private ComputeShader _vertexCompute;
		private Material _lineMat;
		private Material _fillMat;

		/// <summary>
		/// Parameters controlling simulation visuals
		/// </summary>
		[Tooltip("Parameters controlling simulation visuals")]
		public LineCirclePattern Pattern;

		private Bounds _bounds;
		private Bounds _maxPossibleBounds;
		private bool _dirtyBounds;
		private float _boundSize;
		private float _minX;
		private float _maxX;
		private float _minY;
		private float _maxY;
		private float _minZ;
		private float _maxZ;
		private int _setCount;

		[HideInInspector] public float AlphaMultiplier;

		public EventHandler OnPatternChanged;
		
		private void Awake()
		{
			//make sure we clear everything in case some other component already set up buffers and stuff
			ClearEverything();
			SetupEverything();
		}

		/// <summary>
		/// Does everything necessary to setup the simulation
		/// </summary>
		private void SetupEverything()
		{
			//this allows us to have more than one LineCircle in the same scene
			_snapshotCompute = Instantiate(Resources.Load<ComputeShader>("ComputeShaders/SnapshotCompute"));
			_vertexCompute = Instantiate(Resources.Load<ComputeShader>("ComputeShaders/VertexCompute"));
			_lineMat = Instantiate(Resources.Load<Material>("Materials/LineCircles_Lines"));
			_fillMat = Instantiate(Resources.Load<Material>("Materials/LineCircles_Fill"));

			_vertexBuffer = new ComputeBuffer(Pattern.Count * Pattern.LineCount * 6, sizeof(float) * 4 * 2);
			_vertexBuffer.SetData(new LineCircleVertex[Pattern.Count * Pattern.LineCount * 6]);
			_vertexCompute.SetBuffer(0, "_VertexBuffer", _vertexBuffer);
			_lineMat.SetBuffer("_VertexBuffer", _vertexBuffer);
			_fillMat.SetBuffer("_VertexBuffer", _vertexBuffer);

			_snapshotBuffer = new ComputeBuffer(Pattern.Count, sizeof(float) * 4 * 4);
			_snapshotBuffer.SetData(new LineCircleSnapshot[Pattern.Count]);
			_snapshotCompute.SetBuffer(0, "_SnapshotBuffer", _snapshotBuffer);
			_vertexCompute.SetBuffer(0, "_SnapshotBuffer", _snapshotBuffer);
			_lineMat.SetBuffer("_SnapshotBuffer", _snapshotBuffer);
			_fillMat.SetBuffer("_SnapshotBuffer", _snapshotBuffer);

			_setCount = Pattern.Count;

			//72 = 6 x 12, where 12 is the maximum line count for a pattern
			//multiplied by two to get the two bounding edges
			_latestVerticesBuffer = new ComputeBuffer(144, sizeof(float) * 4 * 2);
			_latestVerticesBuffer.SetData(new LineCircleVertex[144]);
			_vertexCompute.SetBuffer(0, "_LatestVerticesBuffer", _latestVerticesBuffer);

			_fillArgsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
			_lineArgsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
			
			ResetBounds();
		}

		public void ResetBounds()
		{
			_minX = _minY = _minZ = float.MaxValue;
			_maxX = _maxY = _maxZ = -float.MaxValue;
			transform.localScale *= 0.5f;
		}

		private void ClearEverything()
		{
			_snapshotBuffer?.Release();
			_vertexBuffer?.Release();
			_fillArgsBuffer?.Release();
			_lineArgsBuffer?.Release();
			_latestVerticesBuffer?.Release();

			if (_lineMat != null) Destroy(_lineMat);
			if (_fillMat != null) Destroy(_fillMat);

			if (_snapshotCompute != null) Destroy(_snapshotCompute);
			if (_vertexCompute != null) Destroy(_vertexCompute);
		}

		//make sure we dispose safely
		private void OnDestroy()
		{
			ClearEverything();
		}

		private void Update()
		{
			Pattern.MaxTimePossible = Pattern.Count * Pattern.TimeStep;
			DoComputeLoop();
		}

		/// <summary>
		/// Sets the number of simulation steps and recomputes everything
		/// </summary>
		/// <param name="newCount"></param>
		public void SetCount(int newCount)
		{
			if (newCount <= 0) {
				throw new System.ArgumentOutOfRangeException(nameof(newCount), newCount,
					"New pattern count must be greater than zero");
			}

			if (newCount <= _setCount) return;
			
			ClearEverything();
			SetupEverything();
		}

		public Bounds GetBounds()
		{
			if (_dirtyBounds) {
				_bounds = new Bounds(
					new Vector3(
						_minX + (_maxX - _minX) * 0.5f,
						_minY + (_maxY - _minY) * 0.5f,
						_minZ + (_maxZ - _minZ) * 0.5f),
					new Vector3(
						Mathf.Abs(_maxX - _minX),
						Mathf.Abs(_maxY - _minY),
						Mathf.Abs(_maxZ - _minZ))
				);
				_dirtyBounds = false;
			}

			return _bounds;
		}

		public Bounds GetMaxPossibleBounds()
		{
			return _maxPossibleBounds;
		}

		private void DoComputeLoop()
		{
			SetComputeValues(Pattern);
			if (Pattern.LineCount != _setLineCount) UpdateMeshes(Pattern.LineCount);

			_snapshotCompute.Dispatch(0, Mathf.CeilToInt(Pattern.Count / 256f), 1, 1);
			_vertexCompute.Dispatch(0, Pattern.Count, 1, 1);

			//Calculate dynamic bounds as the pattern evolves
			if (Pattern.TimeSpan > 0.2f) {
				AsyncGPUReadback.Request(_latestVerticesBuffer, (callback) =>
				{
					var output = callback.GetData<LineCircleVertex>();
					for (var i = 0; i < output.Length; i++) {
						if (output[i].Position == Vector4.zero) continue;

						if (output[i].Position.x < _minX) {
							_minX = output[i].Position.x;
							_dirtyBounds = true;
						}
						if (output[i].Position.x > _maxX) {
							_maxX = output[i].Position.x;
							_dirtyBounds = true;
						}

						if (output[i].Position.y < _minY) {
							_minY = output[i].Position.y;
							_dirtyBounds = true;
						}
						if (output[i].Position.y > _maxY) {
							_maxY = output[i].Position.y;
							_dirtyBounds = true;
						}

						if (output[i].Position.z < _minZ) {
							_minZ = output[i].Position.z;
							_dirtyBounds = true;
						}
						if (output[i].Position.z > _maxZ) {
							_maxZ = output[i].Position.z;
							_dirtyBounds = true;
						}
					}
				});
			}

			//multiplied by cos(45°) x sphere diameter...multiplied by 0.8 just in case!
			

			if(Pattern.DrawFill)
				Graphics.DrawMeshInstancedIndirect(_fillMesh, 0, _fillMat, new Bounds(Vector3.zero, Vector3.one * 100f),
					_fillArgsBuffer);
			if(Pattern.DrawLines)
				Graphics.DrawMeshInstancedIndirect(_lineMesh, 0, _lineMat, new Bounds(Vector3.zero, Vector3.one * 100f),
					_lineArgsBuffer);
		}

		/// <summary>
		/// Assign all Pattern values to the GPU to update its simulation
		/// </summary>
		/// <param name="pattern">The LineCirclePattern to push to the GPU</param>
		private void SetComputeValues(LineCirclePattern pattern)
		{
			//each of these has _type, _center, _amplitude, _period and _phase appended
			//just saves me writing tons of code!
			var prefixes = new[]
			{
				"circlePos_x", "circlePos_y", "circlePos_z",
				"circleRot_x", "circleRot_y", "circleRot_z",
				"circleRad",
				"lineRot_x", "lineRot_y", "lineRot_z",
				"lineLength",
				"colorRange", "colorOffset"
			};

			for (var i = 0; i < pattern.Oscillators.Length; i++) {
				_snapshotCompute.SetInt(prefixes[i] + "_type", (int) pattern.Oscillators[i].Type);
				_snapshotCompute.SetFloat(prefixes[i] + "_center", pattern.Oscillators[i].Center);
				_snapshotCompute.SetFloat(prefixes[i] + "_amplitude", pattern.Oscillators[i].Amplitude);
				_snapshotCompute.SetFloat(prefixes[i] + "_period", pattern.Oscillators[i].Period);
				_snapshotCompute.SetFloat(prefixes[i] + "_phase", pattern.Oscillators[i].Phase);
			}

			_snapshotCompute.SetInt("_BufferCount", pattern.Count);
			_snapshotCompute.SetFloat("_TimeStep", pattern.TimeStep);
			_snapshotCompute.SetFloat("_TimeOffset", pattern.TimeOffset);
			_snapshotCompute.SetInt("_TimeSpan", (int) (pattern.TimeSpan / pattern.TimeStep));
			_snapshotCompute.SetInt("_AutoScaleLines", pattern.AutoScaleLines ? 1 : 0);
			_snapshotCompute.SetInt("_IntervalOffset", (int) (pattern.TimeOffset / pattern.TimeStep));
			_snapshotCompute.SetInt("_LineInterval", pattern.LineInterval);

			_vertexCompute.SetInt("_LineCount", pattern.LineCount);
			_vertexCompute.SetFloat("_TimeSpan", pattern.TimeSpan);
			_vertexCompute.SetInt("_Spherical", pattern.SphericalCoordinates ? 1 : 0);
			_vertexCompute.SetInt("_FirstIndex", (int) (pattern.TimeSpan / pattern.TimeStep));

			_lineMat.SetInt("_LineCount", pattern.LineCount);
			_lineMat.SetFloat("_Alpha", pattern.LineColor.a * AlphaMultiplier);
			_lineMat.SetMatrix("_LocalToWorld", transform.localToWorldMatrix);
			_lineMat.SetMatrix("_WorldToLocal", transform.worldToLocalMatrix);

			_fillMat.SetInt("_LineCount", pattern.LineCount);
			_fillMat.SetFloat("_Alpha", pattern.FillColor.a * AlphaMultiplier);
			_fillMat.SetMatrix("_LocalToWorld", transform.localToWorldMatrix);
			_fillMat.SetMatrix("_WorldToLocal", transform.worldToLocalMatrix);


//		_lineMat.SetInt("_Spherical", pattern.SphericalCoordinates ? 1 : 0);
//		_lineMat.SetInt("_LineCount", pattern.LineCount);
//		_lineMat.SetColor("_Color", pattern.LineColor);
//        _lineMat.SetInt("_LineInterval", pattern.LineInterval);
//        _lineMat.SetInt("_IntervalOffset", (int)(pattern.TimeOffset / pattern.TimeStep));
//        _lineMat.SetInt("_TimeSpan", (int)(pattern.TimeSpan / pattern.TimeStep));
//
//		_lineMat.SetVector("_Position", new Vector4(transform.position.x, transform.position.y, transform.position.z, 0));
//		_lineMat.SetVector("_Rotation", new Vector4(transform.eulerAngles.x * Mathf.Deg2Rad, transform.eulerAngles.y * Mathf.Deg2Rad, transform.eulerAngles.z * Mathf.Deg2Rad, 0));
//		_lineMat.SetVector("_Scale", new Vector4(transform.localScale.x, transform.localScale.y, transform.localScale.z, 0));
//
//		_lineMat.SetInt("_SrcMode", (int)pattern.SrcMode);
//		_lineMat.SetInt("_DstMode", (int)pattern.DstMode);
//
//		_fillMat.SetInt("_Spherical", pattern.SphericalCoordinates ? 1 : 0);
//		_fillMat.SetInt("_LineCount", pattern.LineCount);
//		_fillMat.SetColor("_Color", pattern.FillColor);
//        _fillMat.SetInt("_TimeSpan", (int)(pattern.TimeSpan / pattern.TimeStep));
//
//		_fillMat.SetVector("_Position", new Vector4(transform.position.x, transform.position.y, transform.position.z, 0));
//		_fillMat.SetVector("_Rotation", new Vector4(transform.eulerAngles.x * Mathf.Deg2Rad, transform.eulerAngles.y * Mathf.Deg2Rad, transform.eulerAngles.z * Mathf.Deg2Rad, 0));
//		_fillMat.SetVector("_Scale", new Vector4(transform.localScale.x, transform.localScale.y, transform.localScale.z, 0));
//
//		_fillMat.SetInt("_SrcMode", (int)pattern.SrcMode);
//		_fillMat.SetInt("_DstMode", (int)pattern.DstMode);
		}

		private void UpdateMeshes(int lineCount)
		{
			if (_setLineCount == lineCount) return;

			//Clear old meshes
			Destroy(_fillMesh);
			Destroy(_lineMesh);

			//Fill mesh has six verts per line (2 tris)
			_fillMesh = new Mesh
			{
				vertices = new Vector3 [6 * lineCount]
			};
			//Line mesh is just two verts per line
			_lineMesh = new Mesh
			{
				vertices = new Vector3 [6 * lineCount]
			};

			//Create index arrays and populate
			var fillIndices = new int[6 * lineCount];
			var lineIndices = new int[2 * lineCount];
			for (var i = 0; i < lineCount; i++) {
				for (var j = 0; j < 6; j++) {
					fillIndices[i * 6 + j] = i * 6 + j;
					if (j > 1) continue;
					lineIndices[i * 2 + j] = i * 6 + j;
				}
			}

			_fillMesh.SetIndices(fillIndices, MeshTopology.Triangles, 0);
			_lineMesh.SetIndices(lineIndices, MeshTopology.Lines, 0);

			//Assign meshes
			_fillMesh.UploadMeshData(true);
			_lineMesh.UploadMeshData(true);

			//Update args buffer with new stride size (i.e. vertex count per mesh instance)
			_fillArgsBuffer.SetData(new uint[5] {6 * (uint) lineCount, (uint) Pattern.Count, 0, 0, 0});
			_lineArgsBuffer.SetData(new uint[5] {2 * (uint) lineCount, (uint) Pattern.Count, 0, 0, 0});

			_setLineCount = lineCount;
		}

		public void SetPattern(LineCirclePattern pattern)
		{
			Pattern = pattern;
			ResetBounds();
			_maxPossibleBounds = Pattern.GetMaxPossibleBounds();
			
			OnPatternChanged?.Invoke(this, new EventArgs());
			if (pattern.Count != _setCount) {
				SetCount(pattern.Count);
			}
		}
	}
}