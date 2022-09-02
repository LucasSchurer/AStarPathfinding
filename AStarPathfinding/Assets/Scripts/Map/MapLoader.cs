using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MapLoader : MonoBehaviour
{
    [SerializeField]
    private string _imageUrl;
    public string ImageURL => _imageUrl;

    [SerializeField]
    private float _pixelsPerUnit = 1;

    [SerializeField]
    private SpriteRenderer _spriteRenderer;

    private Texture2D _mapTexture;

    public delegate void OnMapSpriteCreated(Sprite sprite);
    public OnMapSpriteCreated onMapSpriteCreated;

    private void Start()
    {
        StartCoroutine(CreateMapSprite());
    }

    private IEnumerator CreateMapSprite()
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(_imageUrl);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            _mapTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            _mapTexture.filterMode = FilterMode.Point;
            _mapTexture.Apply();
            _spriteRenderer.sprite = Sprite.Create(_mapTexture, new Rect(0, 0, _mapTexture.width, _mapTexture.height), transform.position, _pixelsPerUnit);
            onMapSpriteCreated?.Invoke(_spriteRenderer.sprite);
        }
    }
}
