
using Microsoft.Extensions.Configuration;

const string APP_PATH = "/home/keilen/Desktop/ConsoleApp1/ConsoleApp1";

async Task<int> main()
{
    //NOTE: this is a sample script to demonstrate the process and will not successfully connect to internet, since there is no API to talk to.
    //As such, it is expected to error out if run as-is.
    //The client is mocked for the unit tests, but left blank here.
    var httpClient = new HttpClient();

    //Import settings
    var builder = new ConfigurationBuilder()
        .SetBasePath(APP_PATH)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    IConfiguration configuration = builder.Build();

    //Spin up database and handlers
    using var db = new TransactionContext(configuration);
    var databaseHandler = new DatabaseHandler(configuration, db);
    var snapshotHandler = new SnapshotHandler(configuration, httpClient);

    //Execute hourly processing
    Console.WriteLine("Retrieving snapshot...");
    var snapshot = await snapshotHandler.GetSnapshot();
    Console.WriteLine($"Retrieved {snapshot.Count} transactions.");

    Console.WriteLine("Processing Transactions...");
    databaseHandler.ProcessTransactions(snapshot, DateTime.Now);
    Console.WriteLine("Done processing transactions.");

    Console.WriteLine("Process complete.");
    return 0;
}

await main();