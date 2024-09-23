// Services/BlsApiService.cs
using System.Net.Http;
using System.Text.Json;
using CpiWebService.Models;

namespace CpiWebService.Services
{
    public class BlsApiService
    {
        private readonly HttpClient _httpClient;
        private const string ApiUrl = "https://api.bls.gov/publicAPI/v1/timeseries/data/";
        //private const string SeriesId = "LAUCN040010000000005";

        public BlsApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public virtual async Task<List<CpiData>> GetAllCpiData(string seriesId)
        {
            var url = $"{ApiUrl}{seriesId}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonData = JsonDocument.Parse(responseContent);

            // Safely required properties
            if (!jsonData.RootElement.TryGetProperty("Results", out var resultsElement))
            {
                throw new InvalidOperationException("API response does not contain 'Results' property.");
            }

            if (!resultsElement.TryGetProperty("series", out var seriesElement) || seriesElement.GetArrayLength() == 0)
            {
                throw new InvalidOperationException("API response does not contain 'series' property or it's empty.");
            }

            var seriesArray = seriesElement.EnumerateArray();
            var firstSeries = seriesArray.First();

            if (!firstSeries.TryGetProperty("data", out var dataArray))
            {
                throw new InvalidOperationException("API response does not contain 'data' property.");
            }

            var dataPoints = dataArray.EnumerateArray();
            var cpiDataList = new List<CpiData>();

            foreach (var dp in dataPoints)
            {
                if (!dp.TryGetProperty("year", out var yearProperty) || !int.TryParse(yearProperty.GetString(), out var year))
                {
                    continue; // Skip this data point if the year is missing or invalid
                }

                if (!dp.TryGetProperty("periodName", out var periodNameProperty) || !Enum.TryParse<Month>(periodNameProperty.GetString(), out var month))
                {
                    continue; // Skip this data point if the month is missing or invalid
                }

                if (!dp.TryGetProperty("value", out var valueProperty) || !int.TryParse(valueProperty.GetString(), out var cpiValue))
                {
                    continue; // Skip this data point if the CPI value is missing or invalid
                }

                // Safely check for footnotes
                string notes = string.Empty;
                if (dp.TryGetProperty("footnotes", out var footnotesArray) && footnotesArray.GetArrayLength() > 0)
                {
                    var firstFootnote = footnotesArray.EnumerateArray().First();
                    if (firstFootnote.TryGetProperty("text", out var footnoteTextProperty))
                    {
                        notes = footnoteTextProperty.GetString() ?? string.Empty;
                    }
                }

                // Add the valid CPI data to the list
                cpiDataList.Add(new CpiData
                {
                    Year = year,
                    Month = (int)month,
                    CPI = cpiValue,
                    Notes = notes
                });
            }

            return cpiDataList;
        }
    }
}
