
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Table; // Namespace for Table storage types
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Knurd
{
    public class CloudServices
    {
        public static CloudStorageAccount storageAccount;
        public static CloudTable table;

        public static async Task<bool> createTableStorage()
        {
            try
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
            catch(Exception e)
            {
                return false;
            }
            return true;
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
                try
                {
                    await CloudServices.table.ExecuteAsync(updateOperation);
                    Debug.WriteLine("Entity updated.");
                }
                catch(Exception e)
                {
                    Debug.WriteLine(e.StackTrace);
                    Debug.WriteLine("------------");
                    Debug.WriteLine(e.Message);
                    Debug.WriteLine("------------");
                    Debug.WriteLine(e.Data);
                }

                
            }
            else
                Debug.WriteLine("Entity could not be retrieved.");

            return 1; //random number- had to rerutn type Task<xxx> so I chose int.    
        }



        private static void updateUserFileds(User userBefore, User userAfter) //TODO: not sure what fields need to be changed- gave them all!
        {
            // walking:
            userBefore.B_acc_energyList = userAfter.B_acc_energyList;
            userBefore.B_gyr_energyList = userAfter.B_gyr_energyList;
            userBefore.B_maxPoints = userAfter.B_maxPoints;
            userBefore.B_minPoints = userAfter.B_minPoints;
            userBefore.B_stepAmplitude = userAfter.B_stepAmplitude;
            userBefore.B_strideLength = userAfter.B_strideLength;
            userBefore.acc_energyList = userAfter.acc_energyList;
            userBefore.gyr_energyList = userAfter.gyr_energyList;
            userBefore.maxPoints = userAfter.maxPoints;
            userBefore.minPoints = userAfter.minPoints;
            userBefore.stepAmplitude = userAfter.stepAmplitude;
            userBefore.step_baslineExists = userAfter.step_baslineExists;
            userBefore.strideLength = userAfter.strideLength;

            // bubble:
            userBefore.B_acc_bubble_energy = userAfter.B_acc_bubble_energy;
            userBefore.B_gyr_bubble_energy = userAfter.B_gyr_bubble_energy;     
            userBefore.acc_bubble_energy = userAfter.acc_bubble_energy;
            userBefore.gyr_bubble_energy = userAfter.gyr_bubble_energy;
            userBefore.bubble_baslineExists = userAfter.bubble_baslineExists;

            // reaction:
            userBefore.B_reaction_mean = userAfter.B_reaction_mean;
            userBefore.B_reaction_mistakes = userAfter.B_reaction_mistakes;
            userBefore.B_reaction_variance = userAfter.B_reaction_variance;
            userBefore.reaction_baslineExists = userAfter.reaction_baslineExists;
            userBefore.reaction_mean = userAfter.reaction_mean;
            userBefore.reaction_mistakes = userAfter.reaction_mistakes;
            userBefore.reaction_variance = userAfter.reaction_variance;

            userBefore.B_reactionSingle_mean = userAfter.B_reactionSingle_mean;
            userBefore.B_reactionSingle_variance = userAfter.B_reactionSingle_variance;
            userBefore.reactionSingle_baslineExists = userAfter.reactionSingle_baslineExists;
            userBefore.reactionSingle_mean = userAfter.reactionSingle_mean;
            userBefore.reactionSingle_variance = userAfter.reactionSingle_variance;

            // speech:
            userBefore.speech_baslineExists = userAfter.speech_baslineExists;

        }

        //test == "reaction" | "accel" | "speech" | "bubble" only!        
        public static async Task<int> userParametersIntoFile(User user, string test)
        {
            // Assign the result to a CustomerEntity object. 
            TableResult result = await CloudServices.retrieveEntity(user);

            if (result.Result != null)
            {
                Debug.WriteLine("Entity retrieved successfully. Writing it into a file...");

                User u = (User)(result.Result);
                string data, fileName;

                if (test.Equals("reaction"))
                {
                    data = u.reaction_baslineExists.ToString()
                        + "#" + u.reaction_mean.ToString() + "#" + u.reaction_variance.ToString()
                        + "#" + u.reaction_mistakes.ToString() + "#" + u.B_reaction_mean.ToString()
                        + "#" + u.B_reaction_variance.ToString() + "#" + u.B_reaction_mistakes.ToString()

                        + "#" + u.reactionSingle_mean.ToString() + "#" + u.reactionSingle_variance.ToString()
                        + u.B_reactionSingle_mean.ToString()+ "#" + u.B_reactionSingle_variance.ToString();

                    fileName = u.userName + "_" + u.password + "_reaction";
                }
                else if (test.Equals("accel"))
                {
                    data = u.step_baslineExists.ToString()
                        + "#" + u.acc_energyList + "#" + u.gyr_energyList + "#" + u.maxPoints + "#" + u.minPoints + "#" + u.strideLength
                        + "#" + u.stepAmplitude + "#" + u.B_acc_energyList + "#" + u.B_gyr_energyList + "#" + u.B_maxPoints + "#" + u.B_minPoints
                        + "#" + u.B_strideLength + "#" + u.B_stepAmplitude;
                    fileName = u.userName + "_" + u.password + "_accel";
                }
                else if (test.Equals("bubble"))
                {
                    data = u.bubble_baslineExists.ToString()
                        + "#" + u.acc_bubble_energy + "#" + u.gyr_bubble_energy + "#"
                        + u.B_acc_bubble_energy + "#" + u.B_gyr_bubble_energy;
                    fileName = u.userName + "_" + u.password + "_bubble";
                }
                else if (test.Equals("speech"))
                {
                    //TODO: add here speech parameters
                    data = u.speech_baslineExists.ToString();
                    fileName = u.userName + "_" + u.password + "_speech";
                }
                else
                {
                    Debug.WriteLine("Invalid test name!");
                    return 1;
                }

                var savePicker = new Windows.Storage.Pickers.FileSavePicker();
                savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;

                savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });
                savePicker.SuggestedFileName = fileName;

                Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();

                if (file != null)
                {
                    // Prevent updates to the remote version of the file until
                    // we finish making changes and call CompleteUpdatesAsync.
                    Windows.Storage.CachedFileManager.DeferUpdates(file);
                    // write to file
                    await Windows.Storage.FileIO.WriteTextAsync(file, data);
                    // Let Windows know that we're finished changing the file so
                    // the other app can update the remote version of the file.
                    // Completing updates may require Windows to ask for user input.
                    Windows.Storage.Provider.FileUpdateStatus status =
                        await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);

                }
                else
                {
                    Debug.WriteLine("file is null");
                }

            }
            else
                Debug.WriteLine("Entity could not be retrieved.");

            return 1; //random number- had to rerutn type Task<xxx> so I chose int. 
        }


    }


}
