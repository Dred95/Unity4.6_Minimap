using UnityEngine;
using System.Collections;

public class MinimapElement : MonoBehaviour {
	public Transform myTransform;
	public Sprite Icon;
	// Use this for initialization
	void Awake()
	{ 

	}

	void Start () {
		
		myTransform = GetComponent<Transform>();
		
		AddToMinimap();
	}

	//добавляет оригинал на карту
	public void AddToMinimap()
	{
		Minimap.API.AddElement(this);
	}

	//удаляет оригинал с карты
	public void DeleteFromMinimap()
	{
		Minimap.API.DeleteElement(this);
	}

	// Update is called once per frame
	void Update () {
	
	}
}
