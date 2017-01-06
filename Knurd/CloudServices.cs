
﻿using System;
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

        public static async Task<int> insertEntity(User user) //TODO: get more vars if needed
        {
            // Create the TableOperation object that inserts the customer entity.
            TableOperation insertOperation = TableOperation.Insert(user);

            // Execute the insert operation.
            try
            {
                await CloudServices.table.ExecuteAsync(insertOperation);
            }
            catch (Exception e)
            {
                Debug.WriteLine("user creation error: " + e.ToString());
                if (user == null)
                {
                    Debug.WriteLine("user is null");
                }
                
            }
            Debug.WriteLine("Entity inserted table storage");
            return 1;

        }

        public static async Task<TableResult> retrieveEntity(User user) //TODO: get more vars if needed
        {
            // Create a retrieve operation that takes a customer entity.
            TableOperation retrieveOperation = TableOperation.Retrieve<User>(user.PartitionKey, user.RowKey);

            // Execute the retrieve operation.
            return await CloudServices.table.ExecuteAsync(retrieveOperation);
        }


        public static async Task<int> replaceIneEntity(User user) //TODO: get more vars if needed
        {
            // Assign the result to a CustomerEntity object. 
            TableResult result = await CloudServices.retrieveEntity(user);
           

            if (result.Result != null)
            {
                User updateEntity = (User)(result.Result);
                // Create the Replace TableOperation.
                updateUserFileds(updateEntity, user);
                TableOperation updateOperation = TableOperation.Replace(updateEntity);

                // Execute the operation.
                await CloudServices.table.ExecuteAsync(updateOperation);

                Debug.WriteLine("Entity updated.");
            }
            else
                Debug.WriteLine("Entity could not be retrieved.");

            return 1; //random number- had to rerutn type Task<xxx> so I chose int.    
        }



        private static void updateUserFileds(User userBefore, User userAfter) //TODO: not sure what fields need to be changed- gave them all!
        {
            userBefore.B_energyList = userAfter.B_energyList;
            userBefore.B_maxPoints = userAfter.B_maxPoints;
            userBefore.B_minPoints = userAfter.B_minPoints;
            userBefore.B_reaction_mean = userAfter.B_reaction_mean;
            userBefore.B_reaction_mistakes = userAfter.B_reaction_mistakes;
            userBefore.B_reaction_variance = userAfter.B_reaction_variance;
            userBefore.B_stepAmplitude = userAfter.B_stepAmplitude;
            userBefore.B_strideLength = userAfter.B_strideLength;
            userBefore.energyList = userAfter.energyList;
            userBefore.maxPoints = userAfter.maxPoints;
            userBefore.minPoints = userAfter.minPoints;
            userBefore.reaction_baslineExists = userAfter.reaction_baslineExists;
            userBefore.reaction_mean = userAfter.reaction_mean;
            userBefore.reaction_mistakes = userAfter.reaction_mistakes;
            userBefore.reaction_variance = userAfter.reaction_variance;
            userBefore.speech_baslineExists = userAfter.speech_baslineExists;
            userBefore.stepAmplitude = userAfter.stepAmplitude;
            userBefore.step_baslineExists = userAfter.step_baslineExists;
            userBefore.strideLength = userAfter.strideLength;
        }


    }


}