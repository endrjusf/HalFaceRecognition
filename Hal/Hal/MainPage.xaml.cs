using System;
using System.IO;
using System.Linq;
using System.Text;
using Windows.Foundation;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using Microsoft.ProjectOxford.Face;

//using Microsoft.ProjectOxford.Face;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Hal
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //MainPage rootPage = MainPage.Current;
        private Windows.Foundation.Collections.IPropertySet appSettings;
        private const String photoKey = "capturedPhoto";
        private FaceServiceClient faceServiceClient;

        public MainPage()
        {
            this.InitializeComponent();
            appSettings = ApplicationData.Current.LocalSettings.Values;
            this.faceServiceClient = new FaceServiceClient("01e1692026654958a58bf31618195b51", "https://westcentralus.api.cognitive.microsoft.com/face/v1.0");

            //ResetButton.Visibility = Visibility.Collapsed;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //var captureManager = new MediaCapture();
                //await captureManager.InitializeAsync();
                //var imgFormat = ImageEncodingProperties.CreatePng();

                //// create storage file in local app storage
                //var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                //    "face.png",
                //    CreationCollisionOption.ReplaceExisting);
                //// take photo
                //await captureManager.CapturePhotoToStorageFileAsync(imgFormat, file);
                //rootPage.NotifyUser("", NotifyType.StatusMessage);

                //Using Windows.Media.Capture.CameraCaptureUI API to capture a photo
                CameraCaptureUI dialog = new CameraCaptureUI();
                Size aspectRatio = new Size(16, 9);
                dialog.PhotoSettings.CroppedAspectRatio = aspectRatio;

                var uploadedFile = this.File.Text;

                //if (string.IsNullOrWhiteSpace(uploadedFile))
                {

                    var filePicker = new FileOpenPicker();
                    filePicker.FileTypeFilter.Add("*");
                    //StorageFile file = await filePicker.PickSingleFileAsync();

                    //if (file != null)
                    //{
                        // BitmapImage bitmapImage = new BitmapImage();
                       

                    StorageFile file = await dialog.CaptureFileAsync(CameraCaptureUIMode.Photo);
                    if (file != null)
                    {
                        using (var fileStream = await file.OpenAsync(FileAccessMode.Read))
                        {
                            //var people = await this.faceServiceClient.ListPersonsAsync("hal_group1");
                           
                            var image = fileStream.AsStream();

                            var id = await this.faceServiceClient.DetectAsync(image);

                            if (id.Length == 0)
                            {
                                this.Result.Text = "USER UNKNOWN";
                                return;
                            }

                            //await this.faceServiceClient.CreateFaceListAsync("test", "test");
                            //var face = await this.faceServiceClient.AddFaceToFaceListAsync("test", image);
                            //this.Result.Text = face.PersistedFaceId.ToString();
                            var result = await this.faceServiceClient.IdentifyAsync("hal_group1", new [] { id[0].FaceId }, 0.0001f, 2);


                            if (result.Length == 0)
                            {
                                this.Result.Text = "USER UNKNOWN";
                            }
                            else
                            {
                                var sb = new StringBuilder();

                                foreach (var r in result)
                                {
                                    sb.Append("Face ID: ");
                                    sb.Append(r.FaceId);
                                    sb.Append(" candidates: ");

                                    foreach (var c in r.Candidates)
                                    {
                                        var person = await this.faceServiceClient.GetPersonAsync("hal_group1", c.PersonId);
                                        sb.Append($"{person.Name} {c.Confidence}, ");
                                    }
                                }

                                //var id = result.First().FaceId;
                                //var person = await this.faceServiceClient.GetPersonAsync("hal_group1", id);
                                this.Result.Text = sb.ToString();
                            }
                        }

                    }
                }
                //else
                //{
                //    var filePicker = new FileOpenPicker();
                //    filePicker.FileTypeFilter.Add("*");
                //    StorageFile file = await filePicker.PickSingleFileAsync();

                //    if (file != null)
                //    {
                //        // BitmapImage bitmapImage = new BitmapImage();
                //        using (var fileStream = await file.OpenAsync(FileAccessMode.Read))
                //        {
                //            var image = fileStream.AsStream();
                //            var result = await this.faceServiceClient.DetectAsync(image, true, false, null);
                //            this.Result.Text = (result.Length == 0) ? "USER UNKNOWN" : result[0].FaceId.ToString();
                //        }

                //    }

                 

                //}
            }
            catch (Exception ex)
            {
                this.Result.Text = ex.ToString();
            }


        }
    }
}
