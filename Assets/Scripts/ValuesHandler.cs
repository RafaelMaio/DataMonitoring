// ===============================
// AUTHOR     : Rafael Maio (rafael.maio@ua.pt)
// PURPOSE     : Handles the access to the API and all the indicators needed.
// SPECIAL NOTES: It also has the logic handling the indicators values.
// ===============================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using UnityEngine;
using UnityEngine.Networking;
using Random = System.Random;

/// <summary>
/// Contains the targets for the cycle times.
/// </summary>
[Serializable]
public struct TargetCycleTime
{
    public float totalCycleTime;
    public float exitTime;
    public float processTime;
    public float changeTime;

    public TargetCycleTime(float ct, float et, float pt, float wt)
    {
        totalCycleTime = ct;
        exitTime = et;
        processTime = pt;
        changeTime = wt;
    }
}

/// <summary>
/// Contains the targets for the kpis.
/// </summary>
[Serializable]
public struct TargetKPIs
{
    public float oee;
    public float fpy;
    public float partCount;
    public float countNIO;
    public float productivity;

    public TargetKPIs(float oe, float fp, float pc, float pcNIO, float prod)
    {
        oee = oe;
        fpy = fp;
        partCount = pc;
        countNIO = pcNIO;
        productivity = prod;
    }
}

/// <summary>
/// Contains the structure to collect the cycle times information from the API.
/// </summary>
[Serializable]
public struct CycleTimes
{
    public float id;
    public string line;
    public string station;
    public string serialNumber;
    public string reference;
    public string timeStamp;
    
    public float totalCycleTime;
    public float exitTime;
    public float processTime;
    public float changeTime;

    public int index;

    public CycleTimes(float id, string line, string station, string serialNumber, string reference, string timeStamp,
        float ct, float et, float pt, float wt, int i)
    {
        this.id = id;
        this.line = line;
        this.station = station;
        this.serialNumber = serialNumber;
        this.reference = reference;
        this.timeStamp = timeStamp;

        totalCycleTime = ct;
        exitTime = et;
        processTime = pt;
        changeTime = wt;

        index = i;
    }
}

/// <summary>
/// Contains the structure to collect the kpis information from the API.
/// </summary>
[Serializable]
public struct KPIs
{
    public float shiftIdentifier;
    public string locationId;
    public string validFrom;
    public string validTo;
    public float oee;
    public float oeeSetpoint;
    public float availability;
    public float efficiency;
    public float quality;
    public float fpy;
    public float fpySetPoint;
    public float partCount;
    public float partCountSetPoint;
    public float countNIO;
    public float productivity;

    public string station;

    public int index;

    public KPIs(float shiftIdentifier, string locationId, string validFrom, string validTo, float oee, float oeeSetpoint, float availability, 
                float efficiency, float quality, float fpy, float fpySetPoint, float partCount, float partCountSetPoint, float countNIO, float productivity, string station, int i)
    {
        this.shiftIdentifier = shiftIdentifier;
        this.locationId = locationId;
        this.validFrom = validFrom;
        this.validTo = validTo;
        this.oee = oee;
        this.oeeSetpoint = oeeSetpoint;
        this.availability = availability;
        this.efficiency = efficiency;
        this.quality = quality;
        this.fpy = fpy;
        this.fpySetPoint = fpySetPoint;
        this.partCount = partCount;
        this.partCountSetPoint = partCountSetPoint;
        this.countNIO = countNIO;
        this.productivity = productivity;

        this.station = station;
        this.index = i;
    }
}

/// <summary>
/// Contains the structure to collect the cycle times information from the API of every station.
/// </summary>
[Serializable]
public struct AllStations
{
    public Dictionary<int, CycleTimes> cycles;
    public Dictionary<int, KPIs> kpis;

    public Dictionary<int, TargetCycleTime> targetCycles;
    public Dictionary<int, TargetKPIs> targetKPIs;

    public AllStations(Dictionary<int, CycleTimes> c, Dictionary<int, KPIs> k, Dictionary<int, TargetCycleTime> tc, Dictionary<int, TargetKPIs> tk)
    {
        cycles = c;
        kpis = k;
        targetCycles = tc;
        targetKPIs = tk;
    }

    /// <summary>
    /// Updates the cycle times dictionary.
    /// </summary>
    /// <param name="station">Station number.</param>
    /// <param name="newC">New cycle time structure.</param>
    public void UpdateStationCycles(int station, CycleTimes newC)
    {
        /*
        Random random = new Random();
        float tct = newC.totalCycleTime * (float)(0.8 + random.NextDouble() * 0.4);
        float pt = newC.processTime * (float)(0.8 + random.NextDouble() * 0.4);
        float ct = newC.changeTime * (float)(0.8 + random.NextDouble() * 0.4);
        float et = newC.exitTime * (float)(0.8 + random.NextDouble() * 0.4);
        */

        float tct = 40;
        float pt = 30;
        float ct = 10;
        float et = 5;

        if (!cycles.ContainsKey(station))
        {
            cycles.Add(station, newC);
            TargetCycleTime tCT = new TargetCycleTime(tct, et, pt, ct);
            targetCycles.Add(station, tCT);
        }
        else
        {
            cycles[station] = newC;
            TargetCycleTime tCT = new TargetCycleTime(tct, et, pt, ct);
            targetCycles[station] = tCT;
        }
    }

    /// <summary>
    /// Updates the kpis dictionary.
    /// </summary>
    /// <param name="station">Station number.</param>
    /// <param name="newK">New KPIs structure.</param>
    public void UpdateStationKPIs(int station, KPIs newK)
    {
        if (!kpis.ContainsKey(station))
        {
            kpis.Add(station, newK);
            //TargetKPIs tKPI = new TargetKPIs(newK.oeeSetpoint, newK.fpySetPoint, newK.partCountSetPoint, newK.countNIO, newK.productivity);
            TargetKPIs tKPI = new TargetKPIs(newK.oeeSetpoint, newK.fpySetPoint, newK.partCountSetPoint, 0.02f, 97);
            targetKPIs.Add(station, tKPI);
        }
        else
        {
            kpis[station] = newK;
            //TargetKPIs tKPI = new TargetKPIs(newK.oeeSetpoint, newK.fpySetPoint, newK.partCountSetPoint, newK.countNIO, newK.productivity);
            TargetKPIs tKPI = new TargetKPIs(newK.oeeSetpoint, newK.fpySetPoint, newK.partCountSetPoint, 0.02f, 97);
            targetKPIs[station] = tKPI;
        }
    }
}

/// <summary>
/// Contains the structure to collect the bottleneck information from the API.
/// </summary>
[Serializable]
public struct Bottleneck
{
    public int line;
    public int station;
    public string timeStamp;

    public Bottleneck(int l, int s, string t)
    {
        line = l;
        station = s;
        timeStamp = t;
    }
}

public class ValuesHandler : MonoBehaviour
{
    /// <summary>
    /// Cycle times and KPIs information of every station.
    /// </summary>
    public AllStations allStations;

    /// <summary>
    /// Bottleneck information.
    /// </summary>
    public Bottleneck bottleneck;

    /// <summary>
    /// Is the fetch done?
    /// </summary>
    private int allSet = 0;

    /// <summary>
    /// Communication with the Line Monitoring Handler script.
    /// </summary>
    public LineMonitoringHandler lineMonitoringHandler;

    /// <summary>
    /// Communication with the Configuration Handler script.
    /// </summary>
    public ConfigurationHandller configurationHandller;

    /// <summary>
    /// Communication with Usability Test script.
    /// </summary>
    public UsabilityTest usabilityTest;

    /// <summary>
    /// Unity Start function.
    /// </summary>
    void Start()
    {
        allStations = new AllStations(new Dictionary<int, CycleTimes>(), new Dictionary<int, KPIs>(), new Dictionary<int, TargetCycleTime>(), new Dictionary<int, TargetKPIs>());
    }

    /// <summary>
    /// Fill the station structures with the first API fetch.
    /// </summary>
    /// <param name="line_number">Line number.</param>
    /// <param name="station_number">Station number.</param>
    public void fillDynamicDataAsset(int line_number, int station_number)
    {
        string header = "https://ews-emea.api.bosch.com";
        if (usabilityTest.getOnFlag())
        {
            header = "http://192.168.1.175:5500";
            //header = "http://localhost:5500";
        }
        string urlCycleTime = header + "/it/application/api/augmanity-pps4-dummy/d/v1/api/kpi/cycle-times";
        string paramsCycleTime = "?line=" + line_number.ToString() + "&station=" + station_number.ToString() ;
        StartCoroutine(GetRequest(urlCycleTime + paramsCycleTime, station_number, 0));

        string urlKPIs = header + "/it/application/api/augmanity-pps4-dummy/d/v1/api/kpi";
        urlKPIs = urlKPIs + "/current-shift";
        string paramsKPIs = "?line=" + line_number.ToString() + "&station=" + station_number.ToString();
        StartCoroutine(GetRequest(urlKPIs + paramsKPIs, station_number, 1));

        string urlBottleneck = header + "/it/application/api/augmanity-pps4-dummy/d/v1/api/bottlenecks/actual";
        string paramsBottleneck = "?line=" + line_number.ToString();
        StartCoroutine(GetRequest(urlBottleneck + paramsBottleneck, station_number, 2));
    }

    /// <summary>
    /// Verify if the station has data in the API.
    /// </summary>
    /// <param name="line_number">Line number.</param>
    /// <param name="station_number">Station number.</param>
    public void verifyDataInStation(int line_number, int station_number)
    {
        string header = "https://ews-emea.api.bosch.com";
        if (usabilityTest.getOnFlag())
        {
            header = "http://192.168.1.175:5500";
            //header = "http://localhost:5500";
        }
        string urlCycleTime = header + "/it/application/api/augmanity-pps4-dummy/d/v1/api/kpi/cycle-times";
        string paramsCycleTime = "?line=" + line_number.ToString() + "&station=" + station_number.ToString();
        StartCoroutine(VerifyIfDataExists(urlCycleTime + paramsCycleTime, station_number, line_number, 0));

        string urlKPIs = header + "/it/application/api/augmanity-pps4-dummy/d/v1/api/kpi";
        urlKPIs = urlKPIs + "/current-shift";
        string paramsKPIs = "?line=" + line_number.ToString() + "&station=" + station_number.ToString();
        StartCoroutine(VerifyIfDataExists(urlKPIs + paramsKPIs, station_number, line_number, 1));
    }

    /// <summary>
    /// Authenticate in the API
    /// </summary>
    /// <param name="username">Username.</param>
    /// <param name="password">Password.</param>
    /// <returns>Authentication string.</returns>
    public string authenticate(string username, string password)
    {
        string auth = username + ":" + password;
        auth = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(auth));
        auth = "Basic " + auth;
        return auth;
    }

    /// <summary>
    /// Access to the API.
    /// </summary>
    /// <param name="uri">URL to access.</param>
    /// <param name="station_number">Station number.</param>
    /// <param name="which">Which structure are we getting from the API.</param>
    /// <returns></returns>
    IEnumerator GetRequest(string uri, int station_number, int which)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            if (!usabilityTest.getOnFlag())
            {
                string authorization = authenticate("8b9f2e5b-8524-4de5-8472-7e7de6b37864", "4817a476-987f-4925-bcf0-6ec0e0334a29");
                webRequest.SetRequestHeader("AUTHORIZATION", authorization);
            }
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    break;
                case UnityWebRequest.Result.Success:

                    switch (which)
                    {
                        case 0:
                            string currentCycleTime = (webRequest.downloadHandler.text.Split("}")[0] + "}").Replace("[", "");
                            CycleTimes cycleTimesValues = JsonUtility.FromJson<CycleTimes>(currentCycleTime);
                            allStations.UpdateStationCycles(station_number, cycleTimesValues);
                            allSet += 1;
                            break;
                        case 1:
                            string kpisData = (webRequest.downloadHandler.text);
                            KPIs kpisValues = JsonUtility.FromJson<KPIs>(kpisData);
                            allStations.UpdateStationKPIs(station_number, kpisValues);
                            allSet += 1;
                            break;
                        case 2:
                            string bottleneckText = (webRequest.downloadHandler.text);
                            bottleneck = JsonUtility.FromJson<Bottleneck>(bottleneckText);
                            allSet += 1;
                            break;
                    }
                    break;
            }
        }
    }
    
    /// <summary>
    /// Verify which color should the circle be painted.
    /// </summary>
    /// <param name="station">Station number.</param>
    /// <param name="varName">Indicator to check.</param>
    /// <param name="cycle_or_kpi">Is the indicator a cycle time or a kpi?</param>
    /// <param name="positive">Is the indicator better with a higher or lower value than its target?</param>
    /// <returns></returns>
    public float checkThereshold(int station, string varName, bool cycle_or_kpi, bool positive)
    {
        float target, value;
        if (cycle_or_kpi)
        {
            target = (float)allStations.targetCycles[station].GetType().GetField(varName).GetValue(allStations.targetCycles[station]);
            value = (float)allStations.cycles[station].GetType().GetField(varName).GetValue(allStations.cycles[station]);
        }
        else
        {
            target = (float)allStations.targetKPIs[station].GetType().GetField(varName).GetValue(allStations.targetKPIs[station]);
            value = (float)allStations.kpis[station].GetType().GetField(varName).GetValue(allStations.kpis[station]);
        }
        if (positive)
        {
            float red = target * 1.1f;
            if (value >= red) return 101;
            else if(value < red && value > target) return (value - target) * 100 / (red - target);
            else return 1;
        }
        else
        {
            float red = target * 0.9f;
            if (value <= red) return 101;
            else if (value > red && value < target) return (target - value) * 100 / (target - red);
            else return 1;
        }
    }

    /// <summary>
    /// Get allSet variable.
    /// </summary>
    /// <returns></returns>
    public int getAllSet()
    {
        return allSet;
    }

    /// <summary>
    /// Set allSet variable.
    /// </summary>
    public void setAllSet()
    {
        allSet = 0;
    }

    /// <summary>
    /// Get the bottleneck station.
    /// </summary>
    /// <returns>Bottleneck station.</returns>
    public int getBottleneckStation()
    {
        return bottleneck.station;
    }

    /// <summary>
    /// Access the API to verify if the station has data in the API.
    /// </summary>
    /// <param name="uri">URL to fetch.</param>
    /// <param name="station_number">Station number.</param>
    /// <param name="line_number">Line number.</param>
    /// <param name="cycle_or_kpi">Is the fetch to cycle times or to kpis?</param>
    /// <returns></returns>
    IEnumerator VerifyIfDataExists(string uri, int station_number, int line_number, int cycle_or_kpi)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            if (!usabilityTest.getOnFlag())
            {
                string authorization = authenticate("8b9f2e5b-8524-4de5-8472-7e7de6b37864", "4817a476-987f-4925-bcf0-6ec0e0334a29");
                webRequest.SetRequestHeader("AUTHORIZATION", authorization);
            }

            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    break;
                case UnityWebRequest.Result.Success:

                    switch (cycle_or_kpi)
                    {
                        case 0:
                            try
                            {
                                string currentCycleTime = (webRequest.downloadHandler.text.Split("}")[0] + "}").Replace("[", "");
                                CycleTimes cycleTimesValues = JsonUtility.FromJson<CycleTimes>(currentCycleTime);
                                configurationHandller.CloseDialogWarning();
                                lineMonitoringHandler.CloseDialogWarning();
                            }
                            catch
                            {
                                Debug.Log("1");
                                configurationHandller.noDataInStation(line_number, station_number);
                                lineMonitoringHandler.noDataInStation(line_number, station_number);
                            }
                            break;
                        case 1:
                            try
                            {
                                string kpisData = (webRequest.downloadHandler.text);
                                KPIs kpisValues = JsonUtility.FromJson<KPIs>(kpisData);
                                configurationHandller.CloseDialogWarning();
                                lineMonitoringHandler.CloseDialogWarning();
                            }
                            catch
                            {
                                Debug.Log("2");
                                configurationHandller.noDataInStation(line_number, station_number);
                                lineMonitoringHandler.noDataInStation(line_number, station_number);
                            }
                            break;
                    }
                    break;
            }
        }
    }
}