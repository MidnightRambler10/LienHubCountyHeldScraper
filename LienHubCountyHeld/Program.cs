using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Data;
using Microsoft.Data.SqlClient;

namespace LienHubScraper
{
    public class Program
    {

        static List<Record> FinalRecordsList =  new List<Record>();
        static void Main(string[] args)
        {
            ChromeOptions options = new ChromeOptions();

            // Optional
            options.AddArgument("--start-maximized");
            options.AddArgument("--disable-blink-features=AutomationControlled");

            IWebDriver driver = new ChromeDriver(options);

            driver.Navigate().GoToUrl("https://lienhub.com/user/login");
            
            Thread.Sleep(5000);

            driver = Login(driver, "username", "passwword");

            Thread.Sleep(5000);

            List<string> countyUrls = new List<string>
            {
                "https://lienhub.com/county/alachua/countyheld/certificates",
                "https://lienhub.com/county/bay/countyheld/certificates",
                "https://lienhub.com/county/brevard/countyheld/certificates",
                "https://lienhub.com/county/broward/countyheld/certificates",
                "https://lienhub.com/county/charlotte/countyheld/certificates",
                "https://lienhub.com/county/citrus/countyheld/certificates",
                "https://lienhub.com/county/clay/countyheld/certificates",
               "https://lienhub.com/county/collier/countyheld/certificates",
                "https://lienhub.com/county/duval/countyheld/certificates",
                "https://lienhub.com/county/escambia/countyheld/certificates",
                "https://lienhub.com/county/flagler/countyheld/certificates",
                "https://lienhub.com/county/hernando/countyheld/certificates",
                "https://lienhub.com/county/hillsborough/countyheld/certificates",
                "https://lienhub.com/county/indianriver/countyheld/certificates",
                "https://lienhub.com/county/lake/countyheld/certificates",
                "https://lienhub.com/county/lee/countyheld/certificates",
                "https://lienhub.com/county/martin/countyheld/certificates",
                "https://lienhub.com/county/miamidade/countyheld/certificates",
                "https://lienhub.com/county/monroe/countyheld/certificates",
                "https://lienhub.com/county/nassau/countyheld/certificates",
                "https://lienhub.com/county/okaloosa/countyheld/certificates",
                "https://lienhub.com/county/orange/countyheld/certificates",
                "https://lienhub.com/county/osceola/countyheld/certificates",
                "https://lienhub.com/county/pasco/countyheld/certificates",
                "https://lienhub.com/county/pinellas/countyheld/certificates",
                "https://lienhub.com/county/santarosa/countyheld/certificates",
                "https://lienhub.com/county/seminole/countyheld/certificates",
                "https://lienhub.com/county/stlucie/countyheld/certificates",
                "https://lienhub.com/county/sumter/countyheld/certificates",
                "https://lienhub.com/county/volusia/countyheld/certificates",
                "https://lienhub.com/county/walton/countyheld/certificates",
               "https://lienhub.com/county/sarasota/countyheld/certificates"
            };

            foreach (var url in countyUrls)
            {
                driver = VisitUrl(driver, url);

                var pageSizeSelects = driver.FindElements(By.Id("dt-length-0"));

                if (pageSizeSelects.Count > 0)
                {
                    driver = Pagination(driver);
                }
                else
                {
                    continue;
                }

                
            }

            Save(FinalRecordsList);

            driver = Logout(driver);

            driver.Quit();
        }

        static IWebDriver Login(IWebDriver driver, string username, string password)
        {
            var form = driver.FindElement(By.CssSelector("form[name='login']"));

            var txtUser = form.FindElement(By.CssSelector("input[type='text']"));
            txtUser.Click();
            txtUser.Clear();
            txtUser.SendKeys(username);

            var txtPass = form.FindElement(By.CssSelector("input[type='password']"));
            txtPass.Click();
            txtPass.Clear();
            txtPass.SendKeys(password);

            var btnLogin = form.FindElement(By.CssSelector("input[type='submit']"));
            btnLogin.Click();

            return driver;
        }


        static IWebDriver Logout(IWebDriver driver)
        {
            var logoutBtn = driver.FindElement(By.Id("logout_button"));

            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].click();", logoutBtn);

            return driver;
        }

        static IWebDriver VisitUrl(IWebDriver driver, string url)
        {
            driver.Navigate().GoToUrl(url);

            Console.WriteLine("Visiting: " + url);

            Thread.Sleep(3000);

            return driver;
        }

        static IWebDriver Pagination(IWebDriver driver)
        {
            // set page size to 100
            var pageSizeSelect = driver.FindElement(By.Id("dt-length-0"));

            var selectElement = new OpenQA.Selenium.Support.UI.SelectElement(pageSizeSelect);
            selectElement.SelectByValue("100");

            Thread.Sleep(2000);

            List<Record> records = new List<Record>();

            while (true)
            {

                records = Scrape(driver);
                var nextButton = driver.FindElement(By.CssSelector("button[aria-label='Next']"));

                string ariaDisabled = nextButton.GetAttribute("aria-disabled");

                

                FinalRecordsList.AddRange(records); 

                // stop when Next is disabled
                if (ariaDisabled == "true")
                {
                    Console.WriteLine("Last page reached. Stopping pagination.");
                    break;
                }

                nextButton.Click();

                Thread.Sleep(3000);

                Console.WriteLine("Clicked Next page...");
            }

            return driver;
        }

        static List<Record> Scrape(IWebDriver driver)
        {
            // Scraping logic goes here

            List<Record> records = new List<Record>();

            string countyName = driver.Url.Split('/')[4];
            countyName = char.ToUpper(countyName[0]) + countyName.Substring(1);

            var rows = driver.FindElements(By.CssSelector("#certs tbody tr"));

            // No records found for this county
            if (rows.Count <= 1)
            {
                var accountCell = rows[0].FindElements(By.CssSelector("td.account_number"));

                if (accountCell.Count == 0)
                {
                    Console.WriteLine($"{countyName}: No records found.");
                    return records;
                }
               
            }

            foreach (var row in rows)
            {

                try
                {
                    Record record = new Record();

                    record.CountyName = countyName;
                    record.DateCaptured = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    record.Account = row.FindElement(By.CssSelector("td.account_number")).Text.Trim();

                    record.TaxYear = row.FindElement(By.CssSelector("td.tax_year")).Text.Trim();

                    var certificate = row.FindElement(By.CssSelector("td.certificate_number a"));
                    record.Certificate = certificate.Text.Trim();
                    record.TaxSaleUrl = certificate.GetAttribute("href");

                    record.IssuedDate = row.FindElement(By.CssSelector("td.issued_date")).Text.Trim();

                    record.ExpirationDate = row.FindElement(By.CssSelector("td.expiration_date")).Text.Trim();

                    record.PurchaseAmount = row.FindElement(By.CssSelector("td.purchase_amt")).Text.Trim();

                    records.Add(record);
                }
                catch (Exception)
                {

                    continue;
                }
                
                
            }

            return records;
        }

        static void Save(List<Record> records)
        {
            string connectionString =
                "connectionstring";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Truncate the table
                using (SqlCommand truncateCmd = new SqlCommand(
                    "TRUNCATE TABLE dbo.LienHub_CountyHeld", conn))
                {
                    truncateCmd.ExecuteNonQuery();
                    Console.WriteLine("Table LienHub_CountyHeld truncated successfully.");
                }

                // Insert records
                foreach (var rec in records)
                {
                    using (SqlCommand cmd = new SqlCommand(@"
INSERT INTO dbo.LienHub_CountyHeld
(
    TaxSaleUrl,
    CountyName,
    DateCaptured,
    NumberOfRecords,
    Pages,
    Account,
    [Tax Year],
    Certificate,
    [Issued Date],
    [Expiration Date],
    [Purchase Amount]
)
VALUES
(
    @TaxSaleUrl,
    @CountyName,
    @DateCaptured,
    @NumberOfRecords,
    @Pages,
    @Account,
    @TaxYear,
    @Certificate,
    @IssuedDate,
    @ExpirationDate,
    @PurchaseAmount
)", conn))
                    {
                        cmd.Parameters.AddWithValue("@TaxSaleUrl", rec.TaxSaleUrl ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@CountyName", rec.CountyName ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@DateCaptured", rec.DateCaptured ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@NumberOfRecords", rec.NumberOfRecords ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Pages", rec.Pages ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Account", rec.Account ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@TaxYear", rec.TaxYear ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Certificate", rec.Certificate ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@IssuedDate", rec.IssuedDate ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ExpirationDate", rec.ExpirationDate ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@PurchaseAmount", rec.PurchaseAmount ?? (object)DBNull.Value);

                        cmd.ExecuteNonQuery();
                    }
                }

                Console.WriteLine($"Saved {records.Count} records to LienHub_CountyHeld successfully.");
            }
        }
    }
}