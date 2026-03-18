# TransactionsIngest_Exercise
This is a demo project to demonstrate a small console app which accesses a database and performs CRUD operations.

LOCAL SETUP:
These setup instructions assume that you have a Visual Studio environment capable of running .NET 10 with Entity Framework Core and SQLite. 

Two files (Program.cs and UnitTest.cs) make reference to a path on the local machine, in the variable named 'APP_PATH'. Be sure to update this to point to the matching directory on your local machine, ie C:\your\path\to\ConsoleApp1\ConsoleApp1.

Further settings are available in appsettings.json. Modify the "DefaultConnection" value here to refer to the local database file. By default, this will also be located in C:\your\path\to\ConsoleApp1\ConsoleApp1.

In a VS environment, the app can easily be run using standard .NET Runtime. Note, however, that the current version of the main script (Program.cs) is written AS IF it could reach an API serving the required json files. Thus, it will error out on a typical local machine. If desiring to use an actual API, modify appsettings.json and, if necessary, modify the main script to create a custom HttpClient with the desired parameters.

To see the program in action, run the unit tests contained in UnitTest.cs. These are Xunit based and demonstrate the capabilities of the classes and should cover all standard outcomes outlined in the requirements.

If desiring to use a custom SQLite database, ensure that it is present in APP_PATH and named "transactions.db"


DESIGN NOTES:
For the sake of simplicity, the class making the API calls (SnapshotHandler) uses a straightforward instance of the HttpClient class and makes a call without any further parameters such as time or duration, with the assumption that the api will handle such things on its end automatically.

The unit tests automatically blank the database file in order to create a clean test environment. Ensure that the database file used doesn't contain anything you are not willing to lose.

APPROACH:
This work was tackled in a test-first approach, crafting unit tests first and designing the code to pass them. The code places on emphasis on being concise and complete, but not robust. Error reporting is left out for conciseness and the convenience of anyone reviewing the code by hand. Although I strove to wrote self-documenting code in variable, function, and test names, comments were used to better describe and break up larger sections of code to enhance readability.

When designing the structure of the code, emphasis was given to many small classes over one or two large ones, to make review and updating as easy as possible. Even so, the two handlers (snapshot and database) handle all logically important aspects of the code, and all other classes serve a supplementary purpose as storage, scripting, or database context. I am overall pleased with the result, although I regret not having sufficient time to implement the optional Finalization functionality.
