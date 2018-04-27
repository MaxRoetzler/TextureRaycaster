/// Date	: 27/04/2018
/// Company	: -
/// Author	: Maximilian Rötzler
/// License	: This code is licensed under MIT license

using UnityEngine.EventSystems;
using UnityEngine;

public class ClickableObj : MonoBehaviour, IPointerClickHandler
{
	public void OnPointerClick (PointerEventData eventData)
	{
		GetComponent<Renderer> ().material.color = Random.ColorHSV (1f, 1f, 0.5f, 1f);
	}
}