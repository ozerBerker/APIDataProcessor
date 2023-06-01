using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace APIDataProcessor
{
    internal class Program
    {
        public static List<IntraDayTradeHistory> intraDayTradeHistoryList;
        static void Main(string[] args)
        {
            //string endDate = DateTime.Now.ToString("yyyy-MM-dd"), startDate = DateTime.Now.ToString("yyyy-MM-dd");
            string endDate = "2022-01-26", startDate = "2022-01-26";
            string url = "https://seffaflik.epias.com.tr/transparency/service/market/";

            ConvertJsonToList(getRequest(url, endDate, startDate));
            //printListItems();
            TurnListToValid();
            FactTable();
            Console.ReadLine();
        }
        public static void FactTable()
        {
            var sumOfPricesByUniqueId = intraDayTradeHistoryList
                .GroupBy(x => x.conract)
                .Select(group => new { conract = group.Key, TotalPrice = group.Sum(x => x.price), TotalQuantity = group.Sum(x => x.quantity) });

            Console.WriteLine("|Tarih\t\t|Toplam İşlem Miktarı (MWh)\t|Toplam İşlem Tutarı (TL)\t|Ağırlıklı Ortalama Fiyat (TL/MWh)");
            foreach (var item in sumOfPricesByUniqueId)
            {
                Console.Write(ConvertConractToDate(item.conract) + "\t");
                Console.Write(CalcTotalTransactionAmount(item.TotalQuantity) + "\t\t\t\tTl\t");
                Console.Write(CalcTotalTransactionSum(item.TotalPrice, item.TotalQuantity) + "\t\t\t");
                Console.Write(CalcWeightedAveragePrice(item.TotalPrice, item.TotalQuantity));
                Console.WriteLine();
            }
        }



        public static void TurnListToValid()
        {
            intraDayTradeHistoryList = intraDayTradeHistoryList
                .Where(x => (x.conract.StartsWith("PH")) & (x.conract.Substring(6, 2) == "26" /*DateTime.Now.ToString("06")*/ ))
                .OrderBy(x => x.conract)
                .ToList();
        }

        public static void ConvertJsonToList(string _request)
        {
            string response = _request;
            Root rootObject = JsonConvert.DeserializeObject<Root>(response);
            intraDayTradeHistoryList = rootObject.body.intraDayTradeHistoryList;

        }

        public static string getRequest(string _url, string _endDate, string _startDate)
        {
            string rawResponse = "";
            var client = new RestClient(_url);

            var request = new RestRequest("intra-day-trade-history?");
            request.AddParameter("endDate", _endDate);
            request.AddParameter("startDate", _startDate);

            var response = client.Get(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK) rawResponse = response.Content.ToString();

            return rawResponse;
        }

        /*
         * Fact table Values
         */

        public static string ConvertConractToDate(string _conract)
        {
            string year, month, day, hour;
            year = _conract.Substring(2, 2);
            month = _conract.Substring(4, 2);
            day = _conract.Substring(6, 2);
            hour = _conract.Substring(8, 2);

            return day + "/" + month + "/" + year + " " + hour + ":00";
        }

        public static double CalcTotalTransactionSum(double _price, double _quantity)
        {
            return ((_price * _quantity) / 10);
        }

        public static double CalcTotalTransactionAmount(double _quantity)
        {
            return (_quantity / 10);
        }

        public static double CalcWeightedAveragePrice(double _price, double _quantity)
        {
            return (CalcTotalTransactionSum(_price, _quantity) / CalcTotalTransactionAmount(_quantity));
        }


        /*
         * Print the list
         */

        public static void printListItems()
        {
            foreach (IntraDayTradeHistory trade in intraDayTradeHistoryList)
            {
                Console.WriteLine("id: " + trade.id);
                Console.WriteLine("date: " + trade.date);
                Console.WriteLine("conract: " + trade.conract);
                Console.WriteLine("price: " + trade.price);
                Console.WriteLine("quantity: " + trade.quantity);
                Console.WriteLine();
            }
        }

        public static void printListItems(int j)
        {
            for (int i = 0; i < j; i++)
            {
                Console.WriteLine("id: " + intraDayTradeHistoryList[i].id);
                Console.WriteLine("date: " + intraDayTradeHistoryList[i].date);
                Console.WriteLine("conract: " + intraDayTradeHistoryList[i].conract);
                Console.WriteLine("price: " + intraDayTradeHistoryList[i].price);
                Console.WriteLine("quantity: " + intraDayTradeHistoryList[i].quantity);
                Console.WriteLine();
            }
        }
    }

    /*
     * Define a class that matches the structure of your JSON data
     */
    public class Root
    {
        public string resultCode { get; set; }
        public string resultDescription { get; set; }
        public Body body { get; set; }
    }
    public class Body
    {
        public List<IntraDayTradeHistory> intraDayTradeHistoryList { get; set; }
        public List<Statistic> statistics { get; set; }
    }
    public class IntraDayTradeHistory
    {
        public string id { get; set; }
        public DateTime date { get; set; }
        public string conract { get; set; }
        public double price { get; set; }
        public int quantity { get; set; }
    }
    public class Statistic
    {
        public DateTime date { get; set; }
        public double priceWeightedAverage { get; set; }
        public double priceMax { get; set; }
        public double priceMin { get; set; }
        public int quantityMax { get; set; }
        public int quantityMin { get; set; }
        public int quantitySum { get; set; }
    }
}
