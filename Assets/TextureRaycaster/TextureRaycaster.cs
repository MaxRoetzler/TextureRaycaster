/// Date	: 27/04/2018
/// Company	: -
/// Author	: Maximilian Rötzler
/// License	: This code is licensed under MIT license

using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
	[RequireComponent (typeof (Canvas))]
	public class TextureRaycaster : BaseRaycaster
	{
		#region Fields
		private const int kNoEventMaskSet = -1;
		private static readonly List<Graphic> s_SortedGraphics = new List<Graphic> ();

		[SerializeField]
		private LayerMask m_textureLayer = -1;

		[SerializeField]
		private LayerMask m_physicsLayer = -1;

		[SerializeField]
		private bool m_IgnoreReversedGraphics = true;

		private Canvas m_Canvas;
		private Camera m_mainCamera;
		private Vector2 m_lastPosition;
		private BlockingObjects m_BlockingObjects;
		private List<Graphic> m_RaycastResults = new List<Graphic> ();

		#endregion

		#region Properties
		public bool ignoreReversedGraphics
		{
			get
			{
				return m_IgnoreReversedGraphics;
			}
			set
			{
				m_IgnoreReversedGraphics = value;
			}
		}

		public BlockingObjects blockingObjects
		{
			get
			{
				return m_BlockingObjects;
			}
			set
			{
				m_BlockingObjects = value;
			}
		}

		private Canvas canvas
		{
			get
			{
				if (m_Canvas != null)
				{
					return m_Canvas;
				}

				m_Canvas = GetComponent<Canvas> ();
				return m_Canvas;
			}
		}

		public override Camera eventCamera
		{
			get
			{
				return canvas.worldCamera;
			}
		}

		public Camera mainCamera
		{
			get
			{
				if (m_mainCamera != null)
				{
					return m_mainCamera;
				}

				m_mainCamera = Camera.main;
				return m_mainCamera;
			}
		}
		#endregion

		public enum BlockingObjects
		{
			None,
			TwoD,
			ThreeD,
			All,
		}

		#region Methods
		public override void Raycast (PointerEventData eventData, List<RaycastResult> resultAppendList)
		{
			// Physics raycast through main camera against objects on projector layer
			RaycastHit hit;
			Ray ray = mainCamera.ScreenPointToRay (eventData.position);

			if (!Physics.Raycast (ray, out hit, m_textureLayer))
			{
				m_lastPosition = Vector2.zero;
				eventData.delta = Vector2.zero;
				eventData.position = Vector2.zero;

				return;
			}

			// Create new ray from objects UV coordinates
			ray = eventCamera.ViewportPointToRay (new Vector3 (hit.textureCoord.x, hit.textureCoord.y));

			#if UNITY_EDITOR
			Debug.DrawRay (ray.origin, ray.direction, Color.red);
			#endif

			// Override the eventData mouse position with projected coordinates
			eventData.position = new Vector2 (hit.textureCoord.x * eventCamera.pixelWidth, hit.textureCoord.y * eventCamera.pixelHeight);

			// Override eventData delta
			Vector2 canvasPos = eventCamera.ViewportToScreenPoint (hit.textureCoord - (Vector2.one * 0.5f));
			eventData.delta = canvasPos - m_lastPosition;
			m_lastPosition = canvasPos;

			m_RaycastResults.Clear ();

			// Raycast against UI elements, collect raycastResults
			Raycast (canvas, eventCamera, eventData.position, m_RaycastResults);

			for (int index = 0; index < m_RaycastResults.Count; ++index)
			{
				GameObject gameObject = m_RaycastResults [index].gameObject;
	
				RaycastResult raycastResult = new RaycastResult ()
				{
					module = this,
					gameObject = gameObject,
					distance = hit.distance,
					index = resultAppendList.Count,
					sortingOrder = canvas.sortingOrder,
					screenPosition = eventData.position,
					depth = m_RaycastResults [index].depth,
					sortingLayer = canvas.sortingLayerID,
				};

				resultAppendList.Add (raycastResult);
			}

			// If no UI was hit, raycast against physics objects and collect raycastResults
			if (resultAppendList.Count == 0)
			{
				if (Physics.Raycast (ray, out hit, eventCamera.farClipPlane - eventCamera.nearClipPlane, m_physicsLayer))
				{
					RaycastResult raycastResult = new RaycastResult ()
					{
						module = this,
						sortingOrder = 0,
						sortingLayer = 0,
						distance = hit.distance,
						worldNormal = hit.normal,
						worldPosition = hit.point,
						index = resultAppendList.Count,
						gameObject = hit.collider.gameObject,
						screenPosition = eventData.position,
					};

					resultAppendList.Add (raycastResult);
				}
			}
		}

		private static void Raycast (Canvas canvas, Camera eventCamera, Vector2 pointerPosition, List <Graphic> results)
		{
			IList <Graphic> graphicsForCanvas = GraphicRegistry.GetGraphicsForCanvas (canvas);

			for (int index = 0; index < graphicsForCanvas.Count; ++index)
			{
				Graphic graphic = graphicsForCanvas [index];

				if (graphic.depth != -1 && graphic.raycastTarget && (RectTransformUtility.RectangleContainsScreenPoint (graphic.rectTransform, pointerPosition, eventCamera) && graphic.Raycast (pointerPosition, eventCamera)))
				{
					s_SortedGraphics.Add (graphic);
				}
			}

			s_SortedGraphics.Sort (((g1, g2) => g2.depth.CompareTo (g1.depth)));

			for (int index = 0; index < s_SortedGraphics.Count; ++index)
			{
				results.Add (s_SortedGraphics [index]);
			}

			s_SortedGraphics.Clear ();
		}
		#endregion
	}
}