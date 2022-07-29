using System.Threading.Tasks;
using System.Collections.Generic; 
using System.Text.Json;
using System.Text.Json.Serialization;
using System;
using System.Net.Http;
using System.Net;
using System.Net.Mail;
using System.Timers;
using System.Collections;
using Microsoft.Win32;
using DotNetEnv;

namespace HelloWorld
{  
        public class Quote
    {
        public string symbol { get; set; }
        public string shortName { get; set; }
        public string longName { get; set; }
        public string currency { get; set; }
        public double regularMarketPrice { get; set; }
        public int regularMarketDayHigh { get; set; }
        public int regularMarketDayLow { get; set; }
        public string regularMarketDayRange { get; set; }
        public double regularMarketChange { get; set; }
        public double regularMarketChangePercent { get; set; }
        public DateTime regularMarketTime { get; set; }
        public long marketCap { get; set; }
        public int regularMarketVolume { get; set; }
        public double regularMarketPreviousClose { get; set; }
        public int regularMarketOpen { get; set; }
        public int averageDailyVolume10Day { get; set; }
        public int averageDailyVolume3Month { get; set; }
        public double fiftyTwoWeekLowChange { get; set; }
        public string fiftyTwoWeekRange { get; set; }
        public double fiftyTwoWeekHighChange { get; set; }
        public double fiftyTwoWeekHighChangePercent { get; set; }
        public int fiftyTwoWeekLow { get; set; }
        public double fiftyTwoWeekHigh { get; set; }
        public double twoHundredDayAverage { get; set; }
        public double twoHundredDayAverageChange { get; set; }
        public double twoHundredDayAverageChangePercent { get; set; }
    }

    public class Root
    {
        public List<Quote> results { get; set; }
        public DateTime requestedAt { get; set; }
    }
    class Program
    {
       
       static readonly HttpClient client = new HttpClient();
       static async Task<string> GetQuote(string quote){
        try	
            {
              HttpResponseMessage response = await client.GetAsync("https://brapi.dev/api/quote/"+quote);
              response.EnsureSuccessStatusCode();
              string responseBody = await response.Content.ReadAsStringAsync();
              return responseBody;
            }
            catch(HttpRequestException e)
            {
              Console.WriteLine("\nException Caught!");	
              Console.WriteLine("Message :{0} ",e.Message);
              return "";
            }
            
       }
      
       public static void CreateTestMessage2(string tipo, string quote)
        {
            
            string from = System.Environment.GetEnvironmentVariable("EMAIL_FROM");
            string to = System.Environment.GetEnvironmentVariable("EMAIL_TO");
            string emailUser = System.Environment.GetEnvironmentVariable("EMAIL_USER");
            string emailPassword = System.Environment.GetEnvironmentVariable("EMAIL_PASSWORD");
            string smtpPort = int.Parse(System.Environment.GetEnvironmentVariable("SMTP_PORT"));
            string smtpCliete = System.Environment.GetEnvironmentVariable("SMTP_CLIENTE");
            

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(from); 
            mail.To.Add(to);
            if(tipo == "venda"){
              mail.Subject = "Sua ação "+ quote + " bateu o valor de venda!"; 
              mail.Body = "Sua ação "+ quote + " bateu o valor de venda! É aconselhavel que venda sua ação nesse momento!!!";
            }
            else{
              mail.Subject = "Sua ação "+ quote + " bateu o valor de compra!";
              mail.Body = "Sua ação "+ quote + " bateu o valor de compra! É aconselhavel que compre sua ação nesse momento!!!";
            }
          
            var smtp = new SmtpClient(smtpCliete, int.Parse(smtpPort));
            smtp.UseDefaultCredentials = false;
            smtp.EnableSsl = true;
            smtp.Credentials = new NetworkCredential(emailUser, emailPassword);
            smtp.Send(mail);
        }
        static void seeQuotes(string quoteUrl, Double salePrice, Double buyPrice){    
              string jsonResponse = GetQuote(quoteUrl).Result;
              Root? root = JsonSerializer.Deserialize<Root>(jsonResponse);
              Quote quote = root.results[0];
              
              if(quote.regularMarketPrice >= salePrice){
                 CreateTestMessage2("venda", quoteUrl);
              }
              else if(quote.regularMarketPrice <= buyPrice){
                CreateTestMessage2("compra",quoteUrl);
              }
        }
      
       static void Main(string[] args)
        {
          DotNetEnv.Env.Load();

          if(args.Length < 2){
                Console.WriteLine("Existem parâmetros faltando! Leia a documentação novamente e tente outra vez!");
                Console.Write($"{Environment.NewLine}Press any key to exit...");
                Console.ReadKey(true);
              }
          string quoteUrl = args[0];
          Double salePrice = Double.Parse(args[1]);
          Double buyPrice = Double.Parse(args[2]);
          if(buyPrice > salePrice){
            Console.WriteLine("o preço de venda deve serr maior que o preço de compra! Tente novamente!");
            Console.Write($"{Environment.NewLine}Press any key to exit...");
            Console.ReadKey(true);
          } 
          void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
            {
                seeQuotes(quoteUrl, salePrice, buyPrice);
            }
          System.Timers.Timer myTimer = new System.Timers.Timer();
          myTimer.Elapsed += OnTimedEvent;
          myTimer.Interval = 10000;
          myTimer.Enabled = true;
          Console.WriteLine("Press \'e\' to escape the sample.");
          while (Console.Read() != 'e') ;
        }
    }
}