using Moq;
using Moq.Protected;
using System.Text.Json;
using System.Net;
using Microsoft.Extensions.Configuration;
using Xunit;


namespace ConsoleApp1.Tests
    {
    public class TransactionTests
    {
        const string APP_PATH = "/home/keilen/Desktop/ConsoleApp1/ConsoleApp1";
        private List<Transaction> originalList;
        private IConfiguration _config;
        private HttpClient _httpClient;
        private DateTime _currentTime;
        private TransactionContext _dbContext;
        private void RepeatingTestSetup()
        {
            originalList = new List<Transaction>
            {
                new Transaction(
                    1001,
                    DateTime.Parse("2026-02-27T08:50:10Z"),
                    "4111111111111111",
                    "STO-01",
                    "Wireless Mouse",
                    19.99m,
                    false
                ),
                new Transaction(
                    1002,
                    DateTime.Parse("2026-02-27T06:15:30Z"),
                    "4000000000000002",
                    "STO-02",
                    "USB-C Cable",
                    25.0m,
                    false
                ),
                new Transaction(
                    1005,
                    DateTime.Parse("2026-02-27T11:50:10Z"),
                    "4111111111111111",
                    "STO-03",
                    "Graphics Card",
                    19999.99m,
                    false
                )
            };

            var jsonResponse = JsonSerializer.Serialize(originalList);
            _currentTime = new DateTime(2026, 2, 27, 15, 0, 0, DateTimeKind.Utc);

            //Setup config
            var builder = new ConfigurationBuilder()
            .SetBasePath(APP_PATH)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            _config = builder.Build();

            //Setup HttpClient with mocked response
            var mockMessageHandler = new Mock<HttpMessageHandler>();
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse)
            };
            mockMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponseMessage);
            _httpClient = new HttpClient(mockMessageHandler.Object);

            //Declare a database and delete any existing entries (if present)
            _dbContext = new TransactionContext(_config);
            _dbContext.Transactions.RemoveRange(_dbContext.Transactions);
            _dbContext.TransactionUpdates.RemoveRange(_dbContext.TransactionUpdates);
            _dbContext.SaveChanges();
        }
        
        [Fact]
        public async Task GetSnapshot_ReturnsList()
        {
            //Arrange
            RepeatingTestSetup();
            var testSnapshotHandler = new SnapshotHandler(_config, _httpClient);

            //Act
            var responseList = await testSnapshotHandler.GetSnapshot();

            //Assert
            Assert.IsType<List<Transaction>>(responseList);
        }

        [Fact]
        public async Task GetSnapshot_ContainsAllTransactions()
        {
            //Arrange
            RepeatingTestSetup();
            var testSnapshotHandler = new SnapshotHandler(_config, _httpClient);

            //Act
            var responseList = await testSnapshotHandler.GetSnapshot();

            //Assert
            for (int i = 0; i < originalList.Count; i++)
                Assert.Equal(responseList[i].TransactionId, originalList[i].TransactionId);
        }

        [Fact]
        public async Task DatabaseInitializesToEmpty()
        {
            //Arrange
            RepeatingTestSetup();

            //Assert
            Assert.Empty(_dbContext.Transactions);
        }

        [Fact]
        public async Task DatabaseHandler_InsertsNewTransactions()
        {
            //Arrange
            RepeatingTestSetup();
            var testHandler = new DatabaseHandler(_config, _dbContext);

            //Act
            testHandler.ProcessTransactions(originalList, _currentTime);

            //Assert
            for (int i = 0; i < originalList.Count; i++)
                Assert.True(_dbContext.Transactions.Any(t => t.TransactionId == originalList[i].TransactionId));
        }

        [Fact]
        public async Task DatabaseHandler_RevokesMissingTransaction()
        {
            //Arrange
            RepeatingTestSetup();
            _dbContext.Transactions.Add(new Transaction(999, _currentTime, revoked: false));
            _dbContext.SaveChanges();

            var testHandler = new DatabaseHandler(_config, _dbContext);

            //Act
            testHandler.ProcessTransactions(originalList, _currentTime);

            //Assert
            Assert.True(_dbContext.Transactions.FirstOrDefault(t => t.TransactionId == 999).Revoked);
        }

        [Fact]
        public async Task DatabaseHandler_UpdatesChangedTransaction()
        {
            //Arrange
            RepeatingTestSetup();
            var tempAmount = 100m;
            var oldTransaction = new Transaction(originalList[0].TransactionId, originalList[0].TransactionTime, originalList[0].CardNumber, originalList[0].LocationCode, originalList[0].ProductName, originalList[0].Amount, originalList[0].Finalized, originalList[0].Revoked);
            oldTransaction.Amount = tempAmount;
            _dbContext.Transactions.Add(oldTransaction);
            _dbContext.SaveChanges();

            var testHandler = new DatabaseHandler(_config, _dbContext);

            //Act
            testHandler.ProcessTransactions(originalList, _currentTime);

            //Assert
            Assert.NotEqual(tempAmount,_dbContext.Transactions.FirstOrDefault(t => t.TransactionId == oldTransaction.TransactionId).Amount);
            Assert.True(_dbContext.TransactionUpdates.Any(t => t.TransactionId == oldTransaction.TransactionId && t.Message.Contains("Amount")));
        }

        [Fact]
        public async Task DatabaseHandler_IgnoresUnchangedTransaction()
        {
            //Arrange
            RepeatingTestSetup();
            _dbContext.Transactions.Add(originalList[0]);
            _dbContext.SaveChanges();

            var testHandler = new DatabaseHandler(_config, _dbContext);

            //Act
            testHandler.ProcessTransactions(originalList, _currentTime);

            //Assert
            Assert.False(_dbContext.TransactionUpdates.Any(t => t.TransactionId == originalList[0].TransactionId));
        }

        [Fact]
        public async Task DatabaseHandler_RepeatRunsDoNotRepeatActions()
        {
            //Arrange
            RepeatingTestSetup();
            var tempAmount = 100m;
            var oldTransaction = new Transaction(originalList[0].TransactionId, originalList[0].TransactionTime, originalList[0].CardNumber, originalList[0].LocationCode, originalList[0].ProductName, originalList[0].Amount, originalList[0].Finalized, originalList[0].Revoked);
            oldTransaction.Amount = tempAmount;
            _dbContext.Transactions.Add(oldTransaction);
            _dbContext.SaveChanges();

            var testHandler = new DatabaseHandler(_config, _dbContext);

            //Act
            testHandler.ProcessTransactions(originalList, _currentTime);
            testHandler.ProcessTransactions(originalList, _currentTime);
            testHandler.ProcessTransactions(originalList, _currentTime);

            //Assert
            Assert.True(_dbContext.TransactionUpdates.Where(t => t.TransactionId == oldTransaction.TransactionId && t.Message.Contains("Amount")).Count() == 1);
        }

        [Fact]
        public async Task DatabaseHandler_IgnoresOldTransaction()
        {
            //Arrange
            RepeatingTestSetup();
            var oldTransaction = new Transaction(101, DateTime.Parse("2025-02-26T15:00:00Z"), revoked: false);
            _dbContext.Transactions.Add(oldTransaction);
            _dbContext.SaveChanges();
            var testHandler = new DatabaseHandler(_config, _dbContext);

            //Act
            testHandler.ProcessTransactions(originalList, _currentTime);

            //Assert
            Assert.False(_dbContext.TransactionUpdates.Any(t => t.TransactionId == oldTransaction.TransactionId));
        }
    }
}