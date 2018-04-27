using UnityEngine;
using System.Collections;

public class CameraBehavior : MonoBehaviour
{
	[SerializeField]
	private Transform m_povMain;
	[SerializeField]
	private Transform m_povScreen;
	[SerializeField]
	private LayerMask m_layerMask;
	[SerializeField]
	private float m_cameraTransition = 5f;
	[SerializeField]
	private bool m_isOnScreen;
	[SerializeField]
	private GameObject m_screen;

	private IEnumerator m_coroutine;

	private void Start ()
	{
		transform.position = m_povMain.position;
		transform.rotation = m_povMain.rotation;
		m_isOnScreen = false;

		StartCoroutine (SetCameraActive (false));
	}

	private IEnumerator MoveCamera (Transform target, float duration)
	{
		float t = 0f;

		while (t < 1f)
		{
			t += Time.deltaTime / duration;
			transform.position = Vector3.Lerp (transform.position, target.position, t);
			transform.rotation = Quaternion.Lerp (transform.rotation, target.rotation, t);

			yield return null;
		}
	}

	private IEnumerator SetCameraActive (bool state)
	{
		yield return new WaitForEndOfFrame ();

		m_screen.SetActive (state);

		yield return null;
	}

	private void Update ()
	{
		if (!m_isOnScreen && Input.GetMouseButtonDown (0))
		{
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast (ray, out hit, m_layerMask))
			{
				if (m_coroutine != null)
				{
					StopCoroutine (m_coroutine);
				}

				m_coroutine = MoveCamera (m_povScreen, m_cameraTransition);
				StartCoroutine (m_coroutine);

				m_isOnScreen = true;
				StartCoroutine (SetCameraActive (true));
			}
		}

		if (Input.GetAxis ("Mouse ScrollWheel") < -0.05f)
		{
			if (m_coroutine != null)
			{
				StopCoroutine (m_coroutine);
			}

			m_coroutine = MoveCamera (m_povMain, m_cameraTransition);
			StartCoroutine (m_coroutine);

			m_isOnScreen = false;
			StartCoroutine (SetCameraActive (false));
		}
	}
}
