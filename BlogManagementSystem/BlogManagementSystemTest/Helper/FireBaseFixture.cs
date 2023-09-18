using FireBase;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;


namespace BlogManagementSystemTest.Helper
{
    public class FireBaseFixture : IDisposable
    {
        public ServiceProvider ServiceProvider { get; }

        private static readonly object _lockObject = new object();

        private static FirebaseApp? FirebaseApp { get; set; }

        public FireBaseFixture()
        {
            var services = new ServiceCollection();

            lock (_lockObject)
            {
                if (FirebaseApp == null)
                {
                    var path = GetFirebaseKeyPath();

                    FirebaseApp = FirebaseApp.Create(new AppOptions
                    {
                        Credential = GoogleCredential.FromFile(path)
                    });
                }

                services.AddSingleton<FireBaseNotification>();
            }

            ServiceProvider = services.BuildServiceProvider();
        }

        public void Dispose()
        {
            ServiceProvider.Dispose();
        }

        private string GetFirebaseKeyPath()
        {
            var builder = WebApplication.Create();
            var rootPath = builder.Environment.ContentRootPath;
            string[] pathParts = rootPath.Split(new string[] { "\\" }, StringSplitOptions.None);
            string combinedString = string.Join("\\", pathParts.Take(pathParts.Length - 4));

            var path = Path.Combine(combinedString, "FireBase\\private_key.json");

            return path;
        }
    }
}
