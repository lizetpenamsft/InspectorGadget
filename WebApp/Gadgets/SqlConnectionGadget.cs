using System.Net.Http;
using System.Threading.Tasks;
using InspectorGadget.WebApp.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;

namespace InspectorGadget.WebApp.Gadgets
{
    public class SqlConnectionGadget : GadgetBase<SqlConnectionGadget.Request, SqlConnectionGadget.Result>
    {
        public class Request : GadgetRequest
        {
            public string SqlConnectionString { get; set; }
            public string SqlQuery { get; set; }
            public bool UseAzureManagedIdentity { get; set; }
        }

        public class Result
        {
            public string Output { get; set; }
        }

        public SqlConnectionGadget(IHttpClientFactory httpClientFactory, IUrlHelper url)
            : base(httpClientFactory, url, nameof(ApiController.SqlConnection))
        {
        }

        protected override async Task<Result> ExecuteCoreAsync(Request request)
        {
            using (var connection = new SqlConnection(request.SqlConnectionString))
            using (var command = connection.CreateCommand())
            {
                if (request.UseAzureManagedIdentity)
                {
                    // Request an access token for Azure SQL Database using the current Azure Managed Identity.
                    connection.AccessToken = await new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/");
                }
                connection.Open();
                command.CommandText = request.SqlQuery;
                var result = await command.ExecuteScalarAsync();
                return new Result { Output = result?.ToString() };
            }
        }
    }
}