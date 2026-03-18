using Microsoft.Extensions.Configuration;
using System.Reflection;

class DatabaseHandler
{
    private readonly IConfiguration _config;
    private readonly TransactionContext _dbContext;

    public DatabaseHandler(IConfiguration config, TransactionContext db)
    {
        _config = config;
        _dbContext = db;
    }

    public void ProcessTransactions(List<Transaction> snapshot, DateTime timestamp)
    {
        //Retrieve all db entries within 24 hours of the timestamp
        var startTime = timestamp.AddHours(-24);
        var recentTransactions = _dbContext.Transactions
            .Where(t => t.TransactionTime >= startTime && t.TransactionTime <= timestamp)
            .ToList();

        //Compare each database entry to see if it has been updated
        foreach (var transaction in recentTransactions)
            ProcessTransaction(transaction, snapshot);
        //Insert all new entries in the snapshot into the database
        foreach (var transaction in snapshot)
        {
            if(!_dbContext.Transactions.Any(t => t.TransactionId == transaction.TransactionId))
            {
                _dbContext.Transactions.Add(transaction);
                _dbContext.SaveChanges();
            }
        }

        _dbContext.SaveChanges();
    }

    private void ProcessTransaction(Transaction transaction, List<Transaction> snapshot)
    {
        //If the transaction is present in the snapshot, check if any properties have been updated
        if(snapshot.Any(t => t.TransactionId == transaction.TransactionId))
        {
            var oldTransaction = transaction;
            var newTransaction = snapshot.FirstOrDefault(t => t.TransactionId == transaction.TransactionId);
            
            //Iterate dynamically through all properties and compare
            PropertyInfo[] properties = typeof(Transaction).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if(!Equals(property.GetValue(oldTransaction), property.GetValue(newTransaction)))
                {
                    _dbContext.TransactionUpdates.Add(new TransactionUpdate(0, transaction.TransactionId, DateTime.Now, $"Updated {property.Name} from {property.GetValue(oldTransaction)} to {property.GetValue(newTransaction)}."));
                    _dbContext.SaveChanges();
                    property.SetValue(oldTransaction, property.GetValue(newTransaction)); 
                }       
            }
            _dbContext.Transactions.Update(oldTransaction);
            _dbContext.SaveChanges();
        }
        
        //If the transaction is not present in the snapshot, revoke
        else
        {
            transaction.Revoked = true;
            _dbContext.TransactionUpdates.Add(new TransactionUpdate(0, transaction.TransactionId, DateTime.Now, $"Revoked transaction due to missing from snapshot."));
            _dbContext.Transactions.Update(transaction);
            _dbContext.SaveChanges();
        }
    }
}