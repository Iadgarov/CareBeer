using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Table; // Namespace for Table storage types
using System.Reflection;

namespace Knurd
{
    public class CloudServices
    {
        public static CloudStorageAccount storageAccount;
        public static CloudTable table;
        public static User retrievedResult;

        public static async void createTableStorage()
        {
            // Parse the connection string and return a reference to the storage account.
            var credentials = new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
                "carebeer332211", "iSY3v5xN9Q+h2JH8h2l+syOzjtEQ51L3IVlXIRJm1wX+HCwf+OXqMc38fj7fhJJ8h1IEqKPrItattrxXQSDJhg==");
            CloudServices.storageAccount = new CloudStorageAccount(credentials, true);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Retrieve a reference to the table.
            CloudServices.table = tableClient.GetTableReference("Users"); //TODO: change table's name

            // Create the table if it doesn't exist.
            await CloudServices.table.CreateIfNotExistsAsync();

            Debug.WriteLine("Table storage created.");
        }

        public static void insertEntity(string userName, string password) //TODO: get more vars if needed
        {
            // Create a new customer entity.
            User user = new User(userName, password);

            insertEntity(user);

        }

        public static async void insertEntity(User u) //TODO: get more vars if needed
        {
            // Create a new customer entity.
            User user = u;

            // Create the TableOperation object that inserts the customer entity.
            TableOperation insertOperation = TableOperation.Insert(user);

            // Execute the insert operation.
            try
            {
                await CloudServices.table.ExecuteAsync(insertOperation);
            }
            catch (Exception e)
            {
                Debug.WriteLine("user creation error: " + e.ToString() );
                if (u == null)
                {
                    Debug.WriteLine("user is null");
                }
            }
            Debug.WriteLine("Entity inserted table storage");

        }

        public static async void retrieveEntity(string userName, string password) //TODO: get more vars if needed
        {
            // Create a retrieve operation that takes a customer entity.
            TableOperation retrieveOperation = TableOperation.Retrieve<User>(password, userName);

            // Execute the retrieve operation.
            TableResult _retrievedResult = await CloudServices.table.ExecuteAsync(retrieveOperation);

            CloudServices.retrievedResult = (User)_retrievedResult.Result;

            Debug.WriteLine("Entity retrieved from table storage and in static field: retrievedResult");
        }

        public static async void replaceIneEntity(User u) //TODO: get more vars if needed
        {
            // Assign the result to a CustomerEntity object.
            retrieveEntity(u.userName, u.password);
            User updateEntity = CloudServices.retrievedResult;

            if (updateEntity != null)
            {
         
                // Create the Replace TableOperation.
                TableOperation updateOperation = TableOperation.Replace(updateEntity);

                // Execute the operation.
                await CloudServices.table.ExecuteAsync(updateOperation);

                Debug.WriteLine("Entity updated.");
            }
            else
                Debug.WriteLine("Entity could not be retrieved.");
        } 

    }

 
}
