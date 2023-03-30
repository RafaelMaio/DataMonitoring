using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class newChartGen : MonoBehaviour
{
    // Start is called before the first frame update
    //string base_url = "http://localhost:5500/station/chartgen.png?chart=";
    string base_url = "http://192.168.1.175:5500/station/chartgen.png?chart=";

    [SerializeField]
    private string Address = "localhost";

    public string Getaddress()
    { return Address; }
    public void Setaddress(string value)
    { Address = value; }

    public PersonalInfoHandler personalInfoHandler;

    void Start()
    {
        // getchartfromurl("");
        // GetKPI("260");
        // GetCycleTime("260");
        //GetSample("260", "oee", 80);
    }

    public void GetKPI(string station, GameObject go)
    {
        getchartfromurl(base_url.Replace("station", station) + "specKPI", go);
    }
    public void GetCycleTime(string station, GameObject go)
    {
        getchartfromurl(base_url.Replace("station", station) + "specCycleTime", go);
    }
    public void GetSample(string station, string attr, int index1, int index2, GameObject go, bool isTall = false)
    {
        getchartfromurl(base_url.Replace("station", station).Replace("address", Getaddress()) + $"specSamples&attr={attr}&index={index1},{index2}" + (isTall ? "&size=tall" : ""), go, isTall);
    }

    public void getchartfromurl(string url, GameObject go, bool isTall = false)
    {
        // TODO: new datatype constraint 
        StartCoroutine(GetRequest(url, go, isTall));
    }

    // generate sprite based on base64 string that came from server (save it too)  TODO: two functions, separate, maybe a good place for buffer
    void generateViz(string base64str, GameObject go, bool isTall)
    {
        byte[] Bytes = Convert.FromBase64String(base64str);

        Texture2D tex = new Texture2D(1200, 785);
        tex.LoadImage(Bytes);
        //StartCoroutine(LoadPic(tex, Bytes));
        //_ = imaginayAsync(tex, Bytes, go, isTall);
        Rect rect = new(0, 0, tex.width, tex.height);

        Sprite sprite;
        if (!isTall) sprite = Sprite.Create(tex, rect, new Vector2(0, 0), 2100f);
        else sprite = Sprite.Create(tex, rect, new Vector2(0, 0), 2500f);


        SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = go.AddComponent<SpriteRenderer>(); // will crash if there's another renderer (like MeshRenderer) as component
        }
        renderer.sprite = sprite;

        PolygonCollider2D collider = go.GetComponent<PolygonCollider2D>();
        if (collider == null)
            go.AddComponent<PolygonCollider2D>();  //collider will be added after visualization render

        if (!isTall)
        {
            if (go.transform.parent.name.Contains("KPI"))
                personalInfoHandler.refreshingKPIs(false);
            else
                personalInfoHandler.refreshingCycleTimes(false);
        }
    }

    private async Task imaginayAsync(Texture2D tex, byte[] Bytes, GameObject go, bool isTall)
    {
        var success = await AsyncImageLoader.LoadImageAsync(tex, Bytes);
        Rect rect = new(0, 0, tex.width, tex.height);

        Sprite sprite;
        if (!isTall) sprite = Sprite.Create(tex, rect, new Vector2(0, 0), 4200f);
        else sprite = Sprite.Create(tex, rect, new Vector2(0, 0), 5000f);


        SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = go.AddComponent<SpriteRenderer>(); // will crash if there's another renderer (like MeshRenderer) as component
        }
        renderer.sprite = sprite;

        PolygonCollider2D collider = go.GetComponent<PolygonCollider2D>();
        if (collider == null)
            go.AddComponent<PolygonCollider2D>();  //collider will be added after visualization render

        if (!isTall) {
            if (go.transform.parent.name.Contains("KPI"))
                personalInfoHandler.refreshingKPIs(false);
            else
                personalInfoHandler.refreshingCycleTimes(false);
        }
    }

    IEnumerator LoadPic(Texture2D tex, byte[] Bytes)
    {
        tex.LoadImage(Bytes);
        yield return null;
    }

    // network connection and image download
    IEnumerator GetRequest(string uri, GameObject go, bool isTall)
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
            else generateViz(webRequest.downloadHandler.text, go, isTall); // plain base64 string that (hopefully) came without any html tag

        }
    }
}
