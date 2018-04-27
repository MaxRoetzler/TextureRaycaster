/// Date	: 27/04/2018
/// Company	: -
/// Author	: Maximilian Rötzler
/// License	: This code is licensed under MIT license

using UnityEngine.EventSystems;
using UnityEngine;

public class DragableUI : MonoBehaviour, IDragHandler, IBeginDragHandler
{
	private RectTransform m_transform;
	private Rect m_dragArea;
	private Vector2 m_offset;

	private void Start ()
	{
		m_transform = GetComponent <RectTransform> ();
	}

	public void OnBeginDrag (PointerEventData eventData)
	{
		Canvas canvas = GetComponentInParent <Canvas> ();
		RectTransform canvasRect = canvas.GetComponent<RectTransform> ();

		m_dragArea = new Rect
		{
			min = canvasRect.rect.min + m_transform.rect.size / 2,
			max = canvasRect.rect.max - m_transform.rect.size / 2,
		};

		m_offset = eventData.position - m_transform.anchoredPosition;
	}

	public void OnDrag (PointerEventData eventData)
	{
		Vector2 rectClamped = eventData.position - m_offset;

		rectClamped.x = Mathf.Clamp (rectClamped.x, m_dragArea.min.x, m_dragArea.max.x);
		rectClamped.y = Mathf.Clamp (rectClamped.y, m_dragArea.min.y, m_dragArea.max.y);

		m_transform.anchoredPosition = rectClamped;
	}
}