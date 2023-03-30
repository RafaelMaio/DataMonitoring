// ===============================
// AUTHOR     : Rafael Maio (rafael.maio@ua.pt)
// PURPOSE     : Draws the charts for specific indicators.
// SPECIAL NOTES: X
// ===============================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;
using System;
using Random = System.Random;
using UnityEngine.Networking;

/// <summary>
/// All values from cycle times.
/// </summary>
[Serializable]
public struct AllValuesCycles
{
    /// <summary>
    /// List with cycle times structure.
    /// </summary>
    public List<CycleTimes> allCycleTimes;
}

/// <summary>
/// All values from KPIs.
/// </summary>
[Serializable]
public struct AllValuesKPIs
{
    /// <summary>
    /// List with KPIs structure.
    /// </summary>
    public List<KPIs> allKPIs;
}

public class maintainLineRenderer : MonoBehaviour
{
    /// <summary>
    /// Line renderer to draw the target.
    /// </summary>
    public LineRenderer lineRendererTarget;

    /// <summary>
    /// Line renderer to draw the plot.
    /// </summary>
    public LineRenderer lineRendererPlot;

    /// <summary>
    /// X label.
    /// </summary>
    public TMP_Text xLabel;

    /// <summary>
    /// Slider to change the number of last samples/shifts.
    /// </summary>
    public CustomPinchSlider sampleSlider;

    /// <summary>
    /// Size of the X axis.
    /// </summary>
    private const float xPlotSize = 0.6f;

    /// <summary>
    /// Size of the Y axis.
    /// </summary>
    private const float yPlotSize = 0.3f;

    /// <summary>
    /// Communication with values handler script.
    /// </summary>
    public ValuesHandler valuesHandler;

    /// <summary>
    /// The number of the line.
    /// </summary>
    private int line_number;

    /// <summary>
    /// The corresponding station number.
    /// </summary>
    private int station_number;

    /// <summary>
    /// Specific cycle time to be ploted.
    /// </summary>
    private string cycleTime;

    /// <summary>
    /// Specific kpi to be ploted.
    /// </summary>
    private string kpi;

    /// <summary>
    /// List of values of the cycle times.
    /// </summary>
    private AllValuesCycles allValuesCycles = new AllValuesCycles();

    /// <summary>
    /// List of values of the kpis.
    /// </summary>
    private AllValuesKPIs allValuesKPIs = new AllValuesKPIs();

    /// <summary>
    /// Communication with the Personal Info Handler Script.
    /// </summary>
    public PersonalInfoHandler personalInfoHandler;

    /// <summary>
    /// The max value from the plot.
    /// </summary>
    public TMP_Text maxValueText;

    /// <summary>
    /// Communication with the Usability Test Script.
    /// </summary>
    public UsabilityTest usabilityTest;

    /// <summary>
    /// Communication with the script for accessing vegalite
    /// </summary>
    public newChartGen newChartG;

    /// <summary>
    /// Unity Start function.
    /// Change the sorting order from the line renderers to improve visability.
    /// </summary>
    private void Start()
    {
    }

    /// <summary>
    /// Change the number of last samples in the text.
    /// Called when the slider changes.
    /// </summary>
    public void sampleNumberChange()
    {
        /*
        if (sampleSlider.SliderValueMax != 0)
        {
            xLabel.text = "Last " + ((int)(Math.Round(sampleSlider.SliderValueMin * 100))).ToString() + " - " + ((int)(Math.Round(sampleSlider.SliderValueMax * 100))).ToString() + " Samples (5sec)";
        }
        else
        {
            xLabel.text = "Last 5 Samples (5sec)";
        }*/
        if (sampleSlider.SliderValueMin != 0)
        {
            xLabel.text = "Last " + ((int)(Math.Round(sampleSlider.SliderValueMin * 100))).ToString() + " to " + ((int)(Math.Round(sampleSlider.SliderValueMax * 100))).ToString() + " Samples (5sec)";
        }
        else
        {
            xLabel.text = "Last 1 to " + ((int)(Math.Round(sampleSlider.SliderValueMax * 100))).ToString() + " Samples (5sec)";
        }
    }

    /// <summary>
    /// Change the number of last shift in the text.
    /// Called when the slider changes.
    /// </summary>
    public void shiftNumberChange()
    {
        /*
        if (sampleSlider.SliderValue != 0)
        {
            xLabel.text = "Last " + ((int)(Math.Round(sampleSlider.SliderValue * 60))).ToString() + " shifts";
        }
        else
        {
            xLabel.text = "Last 3 shifts";
        }
        */
        if (sampleSlider.SliderValueMin != 0)
        {
            xLabel.text = "Last " + ((int)(Math.Round(sampleSlider.SliderValueMin * 60))).ToString() + " to " + ((int)(Math.Round(sampleSlider.SliderValueMax * 60))).ToString() + " Shifts";
        }
        else
        {
            xLabel.text = "Last 1 to " + ((int)(Math.Round(sampleSlider.SliderValueMax * 60))).ToString() + " Shifts";
        }
    }

    /// <summary>
    /// Change the number of points to draw the plot - Cycle time.
    /// Gets the values from the API.
    /// </summary>
    public void changeNumberOfPointsTime()
    {
        personalInfoHandler.refreshingCycleTimes(true);
        int numberOfPoints = (int)(Math.Round(sampleSlider.SliderValueMax * 100));
        if (numberOfPoints < 5)
        {
            numberOfPoints = 5;
        }
        lineRendererPlot.positionCount = numberOfPoints;

        string header = "https://ews-emea.api.bosch.com";
        if (usabilityTest.getOnFlag())
        {
            //header = "http://192.168.1.159:5500";
            header = "http://localhost:5500";
        }
        string urlCycleTime = header + "/it/application/api/augmanity-pps4-dummy/d/v1/api/kpi/cycle-times";
        string paramsCycleTime = "?line=" + line_number.ToString() + "&station=" + station_number.ToString();
        StartCoroutine(GetRequest(urlCycleTime + paramsCycleTime, 0, numberOfPoints));
    }

    public void changeNumberOfPointsTime_v2()
    {
        int numberOfPointsMin = (int)(Math.Round(sampleSlider.SliderValueMin * 100));
        if (numberOfPointsMin == 0) numberOfPointsMin = 1;
        int numberOfPointsMax = (int)(Math.Round(sampleSlider.SliderValueMax * 100));
        /*
        if (numberOfPoints < 5)
        {
            numberOfPoints = 5;
        }*/

        string station = staticFunctions.FindChildByRecursion(this.transform.parent, "Title").GetComponent<TMP_Text>().text.Split("Station: ")[1];
        string specificTime = staticFunctions.FindChildByRecursion(this.transform.parent, "CycleTimesTitle").GetComponent<TMP_Text>().text;
        personalInfoHandler.refreshingCycleTimes(true);

        Debug.Log(specificTime);

        string specificTimeFixed = "";
        if (specificTime.Contains("Cycle Time"))
        {
            specificTimeFixed = "totalCycleTime";
        }
        else if(specificTime.Contains("Waiting Time"))
        {
            specificTimeFixed = "changeTime";
        }
        else if (specificTime.Contains("Process Time"))
        {
            specificTimeFixed = "processTime";
        }
        else if (specificTime.Contains("Exit Time"))
        {
            specificTimeFixed = "exitTime";
        }

        newChartG.GetSample(station, specificTimeFixed, numberOfPointsMin, numberOfPointsMax, staticFunctions.FindChildByRecursion(this.transform.parent, "VegaLiteIntegration").gameObject);

        //personalInfoHandler.refreshingCycleTimes(false);
    }

    /// <summary>
    /// Change the number of points to draw the plot - KPIs.
    /// Gets the values from the API.
    /// </summary>
    public void changeNumberOfPointsKPI()
    {
        personalInfoHandler.refreshingKPIs(true);
        int numberOfPoints = (int)(Math.Round(sampleSlider.SliderValueMin * 60));
        if (numberOfPoints < 3)
        {
            numberOfPoints = 3;
        }

        string header = "https://ews-emea.api.bosch.com";
        if (usabilityTest.getOnFlag())
        {
            //header = "http://192.168.1.159:5500";
            header = "http://localhost:5500";
        }
        string urlKPIs = header + "/it/application/api/augmanity-pps4-dummy/d/v1/api/kpi";
        DateTime now = DateTime.Now;
        int startYear = now.Year;
        int startMonth = now.Month - 2;
        if(now.Month == 1 || now.Month == 2)
        {
            startMonth = 12 + startMonth;
            startYear = now.Year - 1;
        }
        string paramsKPIs = "?line=" + line_number.ToString() + "&station=" + station_number.ToString() +
            "&startDate=" + startYear + "-" + startMonth + "-" + now.Day + "&endDate=" + now.Year+ "-" + now.Month + "-" + now.Day + "";
        StartCoroutine(GetRequest(urlKPIs + paramsKPIs, 1, numberOfPoints));
    }

    public void changeNumberOfPointsKPI_v2()
    {
        int numberOfPointsMin = (int)(Math.Round(sampleSlider.SliderValueMin * 60));
        if (numberOfPointsMin == 0) numberOfPointsMin = 1;
        int numberOfPointsMax = (int)(Math.Round(sampleSlider.SliderValueMax * 60));
        /*
        if (numberOfPoints < 3)
        {
            numberOfPoints = 3;
        }*/

        string station = staticFunctions.FindChildByRecursion(this.transform.parent, "Title").GetComponent<TMP_Text>().text.Split("Station: ")[1];
        string specificKPI = staticFunctions.FindChildByRecursion(this.transform.parent, "KPITitle").GetComponent<TMP_Text>().text;
        personalInfoHandler.refreshingKPIs(true);
        string specificKPIFixed = "";
        if (specificKPI.Contains("OEE"))
        {
            specificKPIFixed = "oee";
        }
        else if (specificKPI.Contains("FPY"))
        {
            specificKPIFixed = "fpy";
        }
        else if (specificKPI.Contains("PartCount"))
        {
            specificKPIFixed = "partCount";
        }
        else if (specificKPI.Contains("PartCountNIO"))
        {
            specificKPIFixed = "countNIO";
        }
        else if (specificKPI.Contains("Productivity"))
        {
            specificKPIFixed = "productivity";
        }

        newChartG.GetSample(station, specificKPIFixed, numberOfPointsMin, numberOfPointsMax, staticFunctions.FindChildByRecursion(this.transform.parent, "VegaLiteIntegration").gameObject);

        //personalInfoHandler.refreshingKPIs(false);
    }

    /// <summary>
    /// Set the info for the specific cycle time being visualized.
    /// </summary>
    /// <param name="line_number">Line number.</param>
    /// <param name="station_number">Station number.</param>
    /// <param name="cycletime">Specific cycle time.</param>
    public void setPersonalInfoCycleTime(int line_number, int station_number, string cycletime)
    {
        this.line_number = line_number;
        this.station_number = station_number;
        this.cycleTime = cycletime;
    }

    /// <summary>
    /// Set the info for the specific kpi being visualized
    /// </summary>
    /// <param name="line_number">Line number.</param>
    /// <param name="station_number">Station number.</param>
    /// <param name="kpi">Specific kpi.</param>
    public void setPersonalInfoKPI(int line_number, int station_number, string kpi)
    {
        this.line_number = line_number;
        this.station_number = station_number;
        this.kpi = kpi;
    }

    /// <summary>
    /// Access the API to collect the data history.
    /// </summary>
    /// <param name="uri">URL to access.</param>
    /// <param name="which">Cycle times or kpis.</param>
    /// <param name="numberOfPoints">Number of points to collect.</param>
    /// <returns></returns>
    IEnumerator GetRequest(string uri, int which, int numberOfPoints)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            if (!usabilityTest.getOnFlag())
            {
                string authorization = valuesHandler.authenticate("8b9f2e5b-8524-4de5-8472-7e7de6b37864", "4817a476-987f-4925-bcf0-6ec0e0334a29");
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
                    Vector3[] newPositions = new Vector3[numberOfPoints];
                    switch (which)
                    {
                        case 0:
                            string allCycleTimeText = "{\"allCycleTimes\":" + webRequest.downloadHandler.text + "}";
                            allValuesCycles = JsonUtility.FromJson<AllValuesCycles>(allCycleTimeText);

                            List<float> allYCValues = new List<float>(); 

                            for (int i = 0; i < numberOfPoints; i++)
                            {
                                float x = (i * xPlotSize) / (numberOfPoints - 1);
                                float yValue = (float)allValuesCycles.allCycleTimes[i].GetType().GetField(cycleTime).GetValue(allValuesCycles.allCycleTimes[i]);
                                allYCValues.Add(yValue);
                                float y = (yValue * yPlotSize);
                                float z = 0;
                                Vector3 newPos = new Vector3(x, y, z);
                                newPositions[i] = newPos;
                            }
                            allYCValues.Sort();
                            for (int i = 0; i < numberOfPoints; i++)
                            {
                                Vector3 newPos = newPositions[i];
                                newPos.y = newPos.y / allYCValues[allYCValues.Count - 1];
                                newPositions[i] = newPos;
                            }
                            maxValueText.text = allYCValues[allYCValues.Count - 1].ToString();
                            lineRendererPlot.SetPositions(newPositions);
                            personalInfoHandler.refreshingCycleTimes(false);
                            break;
                        case 1:
                            string allKPIsText = "{\"allKPIs\":" + webRequest.downloadHandler.text + "}";
                            allValuesKPIs = JsonUtility.FromJson<AllValuesKPIs>(allKPIsText);

                            List<float> allYKValues = new List<float>();

                            for (int i = 0; i < numberOfPoints; i++)
                            {
                                float x = (i * xPlotSize) / (numberOfPoints - 1);
                                float yValue = (float)allValuesKPIs.allKPIs[i].GetType().GetField(kpi).GetValue(allValuesKPIs.allKPIs[i]);
                                allYKValues.Add(yValue);
                                float y = (yValue * yPlotSize);
                                float z = 0;
                                Vector3 newPos = new Vector3(x, y, z);
                                newPositions[i] = newPos;
                            }
                            allYKValues.Sort();
                            for (int i = 0; i < numberOfPoints; i++)
                            {
                                Vector3 newPos = newPositions[i];
                                newPos.y = newPos.y / allYKValues[allYKValues.Count - 1];
                                newPositions[i] = newPos;
                            }
                            maxValueText.text = allYKValues[allYKValues.Count - 1].ToString();
                            lineRendererPlot.SetPositions(newPositions);
                            personalInfoHandler.refreshingKPIs(false);
                            break;
                    }
                    break;
            }
        }
    }

    public void pasteSprite(Sprite sprite)
    {
        staticFunctions.FindChildByRecursion(transform, "VegaLiteIntegration").GetComponent<SpriteRenderer>().sprite = sprite;
    }
}