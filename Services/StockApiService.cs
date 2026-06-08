using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProiectMTP.Services
{
    public class StockApiService
    {
        private readonly HttpClient _httpClient = new();

        public async Task<decimal> GetLatestPriceAsync(string ticker)
        {
            if (string.IsNullOrWhiteSpace(ticker)) return 0.0m;

            string symbol = ticker.ToUpper();
            string url = $"https://query1.finance.yahoo.com/v8/finance/chart/{symbol}";

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                
                var responseMessage = await _httpClient.SendAsync(request);
                if (responseMessage.IsSuccessStatusCode)
                {
                    string response = await responseMessage.Content.ReadAsStringAsync();
                    using JsonDocument doc = JsonDocument.Parse(response);
                    JsonElement root = doc.RootElement;

                    if (root.TryGetProperty("chart", out JsonElement chartElement) &&
                        chartElement.TryGetProperty("result", out JsonElement resultElement) &&
                        resultElement.GetArrayLength() > 0)
                    {
                        JsonElement firstResult = resultElement[0];
                        if (firstResult.TryGetProperty("meta", out JsonElement metaElement) &&
                            metaElement.TryGetProperty("regularMarketPrice", out JsonElement priceElement))
                        {
                            decimal price = priceElement.GetDecimal();
                            if (price > 0) return price;
                        }
                    }
                }
            }
            catch
            {
            }

            int seed = 0;
            foreach (char c in symbol) seed += c;
            var random = new Random(seed);
            return (decimal)(random.NextDouble() * 130 + 45);
        }

        public async Task<List<decimal>> GetHistoricalPricesAsync(string ticker)
        {
            var prices = new List<decimal>();
            if (string.IsNullOrWhiteSpace(ticker)) return prices;

            string symbol = ticker.ToUpper();
            string url = $"https://query1.finance.yahoo.com/v8/finance/chart/{symbol}?range=7d&interval=1d";

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                
                var responseMessage = await _httpClient.SendAsync(request);
                if (responseMessage.IsSuccessStatusCode)
                {
                    string response = await responseMessage.Content.ReadAsStringAsync();
                    using JsonDocument doc = JsonDocument.Parse(response);
                    JsonElement root = doc.RootElement;

                    if (root.TryGetProperty("chart", out JsonElement chartElement) &&
                        chartElement.TryGetProperty("result", out JsonElement resultElement) &&
                        resultElement.GetArrayLength() > 0)
                    {
                        JsonElement firstResult = resultElement[0];
                        if (firstResult.TryGetProperty("indicators", out JsonElement indicatorsElement) &&
                            indicatorsElement.TryGetProperty("quote", out JsonElement quoteElement) &&
                            quoteElement.GetArrayLength() > 0)
                        {
                            JsonElement firstQuote = quoteElement[0];
                            if (firstQuote.TryGetProperty("close", out JsonElement closeElement) && 
                                closeElement.ValueKind == JsonValueKind.Array)
                            {
                                foreach (JsonElement priceEl in closeElement.EnumerateArray())
                                {
                                    if (priceEl.ValueKind == JsonValueKind.Number)
                                    {
                                        prices.Add(priceEl.GetDecimal());
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
            }

            return prices;
        }
    }
}