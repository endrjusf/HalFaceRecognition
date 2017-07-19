using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;

namespace FaceAdder
{
    class Program
    {
        static void Main(string[] args)
        {
           
            FaceServiceClient faceServiceClient = new FaceServiceClient("01e1692026654958a58bf31618195b51", "https://westcentralus.api.cognitive.microsoft.com/face/v1.0");


            Task.Run(async () =>
            {
                GetValue(faceServiceClient);
            }).GetAwaiter().GetResult();
            Console.ReadKey();
        }

        private static async Task WaitCallLimitPerSecondAsync(Queue<DateTime> _timeStampQueue, int CallLimitPerMinute)
        {
            async Task WaitCallLimitPerSecondAsync()
            {
                Monitor.Enter(_timeStampQueue);
                try
                {
                    if (_timeStampQueue.Count >= CallLimitPerMinute)
                    {
                        TimeSpan timeInterval = DateTime.UtcNow - _timeStampQueue.Peek();
                        if (timeInterval < TimeSpan.FromMinutes(1))
                        {
                            await Task.Delay(TimeSpan.FromMinutes(1) - timeInterval);
                        }
                        _timeStampQueue.Dequeue();
                    }
                    _timeStampQueue.Enqueue(DateTime.UtcNow);
                }
                finally
                {
                    Monitor.Exit(_timeStampQueue);
                }
            }
        }

        private static async Task GetValue(FaceServiceClient faceServiceClient)
        {
            try
            {
                const int PersonCount = 10000;
                const int CallLimitPerSecond = 20;
                Queue<DateTime> _timeStampQueue = new Queue<DateTime>(CallLimitPerSecond);
                _timeStampQueue.Enqueue(DateTime.UtcNow);

                const string personGroupId = "hal_group1";
                const string personGroupName = "HalGroup1";

                await WaitCallLimitPerSecondAsync(_timeStampQueue, CallLimitPerSecond);

                //var group = await faceServiceClient.GetPersonGroupAsync(personGroupId);
                //if (group != null)
                //{
                //    await faceServiceClient.DeletePersonGroupAsync(personGroupId);
                //}
                //await faceServiceClient.CreatePersonGroupAsync(personGroupId, personGroupName);

                string personImageDir = @"C:\HalPhotos";
                var items = Directory.GetFiles(personImageDir);

                CreatePersonResult[] persons = new CreatePersonResult[items.Length];
                var i = 0;
                foreach (string path in items)
                {
                    Thread.Sleep(10000);
                    string personName = path.Split(new[] {'\\'}).Last();
                    persons[i] = await faceServiceClient.CreatePersonAsync(personGroupId, personName);
                    Guid personId = persons[i].PersonId;
                    //foreach (string imagePath in Directory.GetFiles(path, "*.jpg"))
                    {
                        await WaitCallLimitPerSecondAsync(_timeStampQueue, CallLimitPerSecond);
                        Console.WriteLine(personName + " " + personName);
                        using (Stream stream = File.OpenRead(path))
                        {
                            await faceServiceClient.AddPersonFaceAsync(personGroupId, personId, stream);
                        }
                    }

                    i++;
                }

                await faceServiceClient.TrainPersonGroupAsync("hal_group1");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        
    }
}
