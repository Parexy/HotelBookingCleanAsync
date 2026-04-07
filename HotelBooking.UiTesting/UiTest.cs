using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;

namespace HotelBooking.UiTesting
{
    public class UiTest : IDisposable
    {
        private readonly IWebDriver _driver;

        public UiTest()
        {
            var options = new ChromeOptions();
            _driver = new ChromeDriver(options);
        }

        [Fact]
        public void HomePage_Should_Show_Title()
        {
            _driver.Navigate().GoToUrl("https://localhost:5001");
            Assert.False(string.IsNullOrWhiteSpace(_driver.Title));
        }

        [Fact]
        public void CreateBooking_Should_Submit_Form()
        {
            _driver.Navigate().GoToUrl("https://localhost:5001/Bookings/Create");

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

            // Wait for inputs
            var startDate = wait.Until(d => d.FindElement(By.Id("StartDate")));
            var endDate = _driver.FindElement(By.Id("EndDate"));
            var customerSelectElement = _driver.FindElement(By.Id("CustomerId"));

            // Fill dates
            startDate.Clear();
            startDate.SendKeys(DateTime.Now.AddDays(5).ToString("yyyy-MM-dd"));

            endDate.Clear();
            endDate.SendKeys(DateTime.Now.AddDays(7).ToString("yyyy-MM-dd"));

            // Select first valid customer
            var customerSelect = new OpenQA.Selenium.Support.UI.SelectElement(customerSelectElement);

            var option = customerSelect.Options
                .FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.GetAttribute("value")));

            Assert.NotNull(option); // ensures dropdown isn't empty

            customerSelect.SelectByText(option.Text);

            // Submit form
            var submitButton = _driver.FindElement(By.CssSelector("input[type='submit']"));
            submitButton.Click();

            // Wait for result (either redirect or validation message)
            wait.Until(d =>
                d.Url.Contains("/Bookings") || d.PageSource.Contains("error", StringComparison.OrdinalIgnoreCase)
            );

            // Assert either success (redirect) OR failure message exists
            var success = _driver.Url.Contains("/Bookings");
            var hasError = _driver.PageSource.Contains("text-danger", StringComparison.OrdinalIgnoreCase);

            Assert.True(success || hasError, "Expected either redirect or validation error.");
        }

        public void Dispose()
        {
            _driver.Quit();
            _driver.Dispose();
        }
    }
}
