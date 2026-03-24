using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
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
            _driver.Navigate().GoToUrl("https://localhost:44360");
            Assert.False(string.IsNullOrWhiteSpace(_driver.Title));
        }

        public void Dispose()
        {
            _driver.Quit();
            _driver.Dispose();
        }
    }
}
