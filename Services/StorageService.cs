using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using ProiectMTP.Models;

namespace ProiectMTP.Services
{
    public class AppData
    {
        public List<Transaction> Transactions { get; set; } = new();
        public List<StockPosition> Stocks { get; set; } = new();
        
        public decimal BudgetFood { get; set; }
        public decimal BudgetBills { get; set; }
        public decimal BudgetInvestments { get; set; }
    }

    public class StorageService
    {
        private readonly string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.json");

        public async Task SaveDataAsync(AppData data)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(data, options);
            await File.WriteAllTextAsync(_filePath, json);
        }

        public async Task<AppData> LoadDataAsync()
        {
            if (!File.Exists(_filePath))
            {
                return new AppData();
            }

            string json = await File.ReadAllTextAsync(_filePath);
            try
            {
                return JsonSerializer.Deserialize<AppData>(json) ?? new AppData();
            }
            catch
            {
                return new AppData();
            }
        }
    }
}