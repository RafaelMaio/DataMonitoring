using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using System.Linq;


public class ChartGen : MonoBehaviour
{
    // Start is called before the first frame update
    string base_url = "http://localhost:5500/station/chartgen.png?chart=";

    void Start()
    {
        // getchartfromurl("");
        // GetKPI("260");
        // GetCycleTime("260");
        // GetSample("260", "oee", 80);
    }

    public void GetKPI(string station)
    {
        getchartfromurl(base_url.Replace("station", station) + "specKPI");
    }
    public void GetCycleTime(string station)
    {
        getchartfromurl(base_url.Replace("station", station) + "specCycleTime");
    }
    public void GetSample(string station, string attr, int index)
    {
        getchartfromurl(base_url.Replace("station", station) + $"specSamples&attr={attr}&index={index}");
    }

    public void getchartfromurl(String url)
    {
        // TODO: new datatype constraint 
        StartCoroutine(GetRequest(url));
    }

    // generate sprite based on base64 string that came from server (save it too)  TODO: two functions, separate, maybe a good place for buffer
    void generateViz(string base64str)
    {
        byte[] Bytes = Convert.FromBase64String(base64str);
        
        Texture2D tex = new Texture2D(1200, 785); // ratio for 2144/1400 (26:17) with 1200 term
        tex.LoadImage(Bytes);
        Rect rect = new(0, 0, tex.width, tex.height);
        Sprite sprite = Sprite.Create(tex, rect, new Vector2(0, 0), 100f);
        SpriteRenderer renderer = this.gameObject.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = this.gameObject.AddComponent<SpriteRenderer>(); // will crash if there's another renderer (like MeshRenderer) as component
        }
        renderer.sprite = sprite;

        PolygonCollider2D collider = gameObject.GetComponent<PolygonCollider2D>();
        if (collider == null)
            gameObject.AddComponent<PolygonCollider2D>();  //collider will be added after visualization render
    }

    // network connection and image download
    IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;
            
            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
            }
            else generateViz(webRequest.downloadHandler.text); // plain base64 string that (hopefully) came without any html tag

        }
    }
}
