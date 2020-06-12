using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pacco.Services.Availability.Api;
using Pacco.Services.Availability.Application.DTO;
using Pacco.Services.Availability.Infrastructure.Mongo.Documents;
using Pacco.Services.Availability.Tests.Shared.Factories;
using Pacco.Services.Availability.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;

namespace Pacco.Services.Availability.Tests.EndToEnd.Sync
{
    public class GetResourceTests : IClassFixture<PaccoApplicationFactory<Program>>, IDisposable
    {
        Task<HttpResponseMessage> Act()
            => _httpClient.GetAsync($"resources/{_resourceId}");

        [Fact]
        public async Task given_resourceId_that_not_exists_endpoint_should_return_http_status_code_bad_request()
        {
            var response = await Act();
            
            response.ShouldNotBeNull();
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task given_resourceId_that_exists_endpoint_should_return_dto_with_valid_data()
        {
            await InsertResourceAsync();

            var response = await Act();
            
            response.ShouldNotBeNull();
            var json = await response.Content.ReadAsStringAsync();
            var dto = JsonConvert.DeserializeObject<ResourceDto>(json);
            
            dto.ShouldNotBeNull();
            dto.Id.ShouldBe(_resourceId);
        }
        
        #region ARRANGE

        private readonly Guid _resourceId;
        private readonly HttpClient _httpClient;
        private readonly MongoDbFixture<ResourceDocument, Guid> _mongoDbFixture;
        
        public GetResourceTests(PaccoApplicationFactory<Program> factory)
        {
            _resourceId = Guid.NewGuid();
            _mongoDbFixture = new MongoDbFixture<ResourceDocument, Guid>("resources");
            factory.Server.AllowSynchronousIO = true;
            _httpClient = factory.CreateClient();
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
            _mongoDbFixture?.Dispose();
        }

        private Task InsertResourceAsync()
            => _mongoDbFixture.InsertAsync(new ResourceDocument
            {
                Id = _resourceId,
                Tags = new[] {"tag"},
                Reservations = new[]
                {
                    new ReservationDocument
                    {
                        TimeStamp = DateTime.UtcNow.AsDaysSinceEpoch(),
                        Priority = 0
                    }
                }
            });

        #endregion
    }
}