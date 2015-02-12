using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Minimap : MonoBehaviour {
	public static Minimap API;				// не ставить 2 миникарты сразу! если надо, то убрать, и иметь ссылки на каждый

	public GameObject elementPrefab; 		//префаб иконки на миникарте. Должен содержать Image.
	Transform myTransform;
	public Transform FollowGO;				//за чем следует миникарта

	float screenRatio;						//соотношение сторон:  высота/ширина
	public float elementScale= 0.02f;		//0 - 0.1 размер иконки в процентах от размера карты  
	public float maxDistance = 10f;			//радиус карты в игровых единицах
	public float updatesPerSecond = 20f;

	List<completeElement> completeElements = new List<completeElement>(); 	//список, содержащий все неообходимое для работы и иконкой
	//---------------------------------------------------------------------------

	 
	public struct completeElement
	{
		public MinimapElement MinimapLink;	//ссылка на класс оригинала
		public RectTransform RTransform;	//ссылка на RectTransform
		public Transform _ETransform;		//ссылка на Transform
		public Image _Image;				//ссылка на Image компонент иконки

		public completeElement(MinimapElement _MinimapLink, RectTransform _RTransform )
		{
			MinimapLink = _MinimapLink;
			RTransform = _RTransform;
			_ETransform = MinimapLink.myTransform;
			_Image = _RTransform.GetComponent<Image>();
		}


	}



	// Use this for initialization

	void Awake()
	{
		if (API == null)
		{
			API = this;
			myTransform = GetComponent<Transform>();
			screenRatio = Screen.height/ Screen.width;
		}


	}

	void Start () 
	{

		StartCoroutine(UpdateMap()); // запуск слежения, можно вызывать откуда хотите, и остонавливать тоже
	}


	//добавляет элемент в список, создает его образ на миникарте, масштабирует.
	public void AddElement(MinimapElement me)
	{

		GameObject temp = Instantiate(elementPrefab) as GameObject;
		RectTransform tempRect = temp.GetComponent<RectTransform>();
		completeElements.Add (new completeElement(me, tempRect));

		completeElement ce = completeElements[completeElements.Count - 1];

		ce.RTransform.SetParent(myTransform);
		ce.RTransform.localScale = Vector3.one;
		ce.RTransform.anchorMin = Vector2.zero;
		ce.RTransform.anchorMax = new Vector2(elementScale, elementScale * screenRatio);
		ce.RTransform.position = Vector3.zero;
		ce.RTransform.localPosition = Vector3.zero;

		if (ce._Image != null)
		{
			ce._Image.sprite = ce.MinimapLink.Icon;
		}

	}

	//удаляет элемент с карты
	public void DeleteElement(MinimapElement me)
	{

		foreach (completeElement ce in completeElements)
		{
			if (ce.MinimapLink == me )
			{
				Destroy(ce.RTransform.gameObject);
				completeElements.Remove(ce);
				break;
			}

		}

	}

	//скрывает элемент
	public void HideElement(MinimapElement me)
	{
		
		foreach (completeElement ce in completeElements)
		{
			if (ce.MinimapLink == me )
			{
				ce._Image.enabled = false;
		
				break;
			}
			
		}
		
	}

	//показывает элемент
	public void ShowElement(MinimapElement me)
	{

		foreach (completeElement ce in completeElements)
		{
			if (ce.MinimapLink == me )
			{
				ce._Image.enabled = true;
				
				break;
			}
			
		}
		
	}

	//Преобразует мировые координаты относительно FollowGO, поворачивает относительно FollowGO, 
	//переводится в координаты для RectTransform, применяются изменения
	void CorrectPositionElements()
	{
		Vector2 FollowV2 = new Vector2(FollowGO.position.x, FollowGO.position.y);

		foreach (completeElement me in completeElements)
		{
			Vector2 MapE =  new Vector2(me._ETransform.position.x, me._ETransform.position.y); 
		
			Vector2 V2Dist =  MapE - FollowV2;
			V2Dist = NormalizedV2(V2Dist);
			float dist = Vector2.Distance(MapE, FollowV2);


			if (dist > maxDistance)
			{
				V2Dist *= maxDistance;
			}
			else
			{
				V2Dist *= dist;
			}

						
			float angleRotationAim = 6.2831f -  myTransform.rotation.eulerAngles.z * Mathf.Deg2Rad  ;
		
			V2Dist = new Vector2(
				V2Dist.x * Mathf.Cos(angleRotationAim) - V2Dist.y * Mathf.Sin(angleRotationAim), 
				V2Dist.x * Mathf.Sin(angleRotationAim) + V2Dist.y * Mathf.Cos(angleRotationAim)
				);

			Vector2 AnchPos = new Vector2(V2Dist.x / maxDistance , V2Dist.y / maxDistance ); // теперь вектор от 0 до 1

			// 0.5 чтобы из [-1;1] сделать [-0.5; 0.5];  (1f - 2f * elementScale) чтобы обеспечить отступ
			AnchPos *= 0.5f * (1f - 2f* elementScale) ; 
			AnchPos = new Vector2(AnchPos.x + 0.5f, AnchPos.y + 0.5f); // смещение в центр карты

			me.RTransform.localScale = Vector3.one;
			me.RTransform.anchorMin =  AnchPos - (Vector2.one * elementScale) ;
			me.RTransform.anchorMax = AnchPos + (Vector2.one * elementScale);
		

			me.RTransform.offsetMax = Vector2.zero;
			me.RTransform.offsetMin = Vector2.zero;
		

		}

	}

	//Возвращает нормализованный вектор
	Vector2 NormalizedV2(Vector2 V2)
	{
		float lenght = Mathf.Sqrt(V2.x * V2.x + V2.y * V2.y);
		return new Vector2(V2.x / lenght, V2.y / lenght);

	}

	//Вызывает обновления позиций элементов
	IEnumerator UpdateMap()
	{
		for(;;)
		{
			CorrectPositionElements();
			yield return new WaitForSeconds (1f / updatesPerSecond);
		}
	}

	
	// Update is called once per frame
	void Update () {
	
	}
}
