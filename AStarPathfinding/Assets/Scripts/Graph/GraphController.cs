using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class GraphController : MonoBehaviour
{
    [SerializeField]
    private Graph _graph;

    [SerializeField]
    private SpriteRenderer _graphOverlayRenderer;

    [SerializeField]
    private int[,] grid;

    [SerializeField]
    private string imageUrl;
    [SerializeField]
    private Texture2D texture;

    private void Awake()
    {
        StartCoroutine(LoadImage());
    }

    private IEnumerator LoadImage()
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else {
            Debug.Log("DONE");
            /*Texture2D tex = ((DownloadHandlerTexture)www.downloadHandler).texture;*/
            Texture2D tex = new Texture2D(texture.height, texture.width);
            tex.filterMode = FilterMode.Point;
            tex.Apply();

            grid = new int[tex.height, tex.width];

            Color[] pixels = texture.GetPixels();

            for (int i = 0; i < tex.height; i++)
            {
                for (int j = 0; j < tex.width; j++)
                {
                    if (pixels[i * tex.height + j].grayscale < 0.2f)
                    {
                        grid[i, j] = 1;
                    } else
                    {
                        grid[i, j] = 0;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            /*float pixelsPerUnit = tex.height / 20;*/
            float pixelsPerUnit = 1;

            _graph = new Graph(grid, tex.height, tex.width, pixelsPerUnit);
            GetComponent<SpriteRenderer>().sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), transform.position, pixelsPerUnit);            
            _graphOverlayRenderer.sprite = Sprite.Create(_graph._graphTexture, new Rect(0, 0, tex.width, tex.height), transform.position, pixelsPerUnit);
        }
    }

    private void PrintGraph()
    {
        foreach (Vertex vertex in _graph.Vertices)
        {
            if (vertex == null)
            {
                continue;
            }

            string s = $"Vertex: {vertex.Identifier}\nConnected Vertices: ";

            foreach (Vertex connectedVertex in vertex.GetConnectedVertices())
            {
                s += $"{connectedVertex.Identifier} ";
            }

            Debug.Log(s);
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            DrawGraph();
        }
    }

    private void DrawGraph()
    {
        if (_graph == null)
        {
            return;
        }

        foreach (Vertex vertex in _graph.Vertices)
        {
            if (vertex == null)
            {
                continue;
            }

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(vertex.Position, new Vector2(1, 1));

            int rowIndex;
            int columnIndex;
            Graph.ReverseCantorPairing(vertex.Identifier, out rowIndex, out columnIndex);

            Handles.color = Color.yellow;
            Handles.Label(vertex.Position, vertex.Identifier.ToString());
            Vector2 columnRowHandlePosition = vertex.Position;
            columnRowHandlePosition.y -= vertex.Size / 6;
            Handles.Label(columnRowHandlePosition, $"{rowIndex},{columnIndex}");

            foreach (Vertex connectedVertex in vertex.GetConnectedVertices())
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(vertex.Position, connectedVertex.Position);
            }
        }
    }
}
