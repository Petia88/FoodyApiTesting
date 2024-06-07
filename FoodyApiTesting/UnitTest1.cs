using FoodyApiTesting.Models;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;

namespace FoodyApiTesting
{
    public class FoodyTests
    {
        private RestClient client;
        private static string foodId;

        [OneTimeSetUp]
        public void Setup()
        {
            string accessToken = GetAccessToken("petia", "123456");

            var restOptions = new RestClientOptions("http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:86")
            {
                Authenticator = new JwtAuthenticator(accessToken),
            };
            this.client = new RestClient(restOptions);
        }
        
        private string GetAccessToken(string username, string password)
        {
            var authClient = new RestClient("http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:86");

            var authRequest = new RestRequest("/api/User/Authentication", Method.Post);
            authRequest.AddJsonBody(
            new
            {
                userName = username,
                password = password
            });
            var response = authClient.Execute(authRequest);

            if(response.IsSuccessStatusCode)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var accessToken = content.GetProperty("accessToken").GetString();
                return accessToken;
            }
            else
            {
                throw new InvalidOperationException("Authentication failed");
            }
        }

        [Order (1)]
        [Test]
        public void CreateFood_WithCorrectData_ShouldSucceed()
        {
            //Arrange
            var newFood = new FoodDTO
            {
                Name = "New Test Food",
                Description = "Some Description",
                Url = ""
            };
            var request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(newFood);

            //Act
            var response = this.client.Execute(request);
            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            var data = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(data.FoodId, Is.Not.Null);

            foodId = data.FoodId;
        }

        [Order(2)]
        [Test]
        public void EditFood_WithCorrectData_ShouldSucceed()
        {
            //Arrange
            var request = new RestRequest($"/api/Food/Edit/{foodId}", Method.Patch);
            request.AddJsonBody(new[]
            {
                new
                {
                    path = "/name",
                    op = "replace",
                    value = "New Food Title",
                },
            });

            //Act
            var response = this.client.Execute(request);
            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var data = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(data.Message, Is.EqualTo("Successfully edited"));

        }

        [Order(3)]
        [Test]
        public void GetAllFoods_WithCorrectData_ShouldSucceed()
        {
            //Arrange
            var request = new RestRequest("/api/Food/All", Method.Get);

            //Act
            var response = this.client.Execute(request);
            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var data = JsonSerializer.Deserialize<ApiResponseDTO[]>(response.Content);
            Assert.That(data.Length, Is.GreaterThan(0));
        }

        [Order(4)]
        [Test]
        public void DeleteFood_WithCorrectData_ShouldSucceed()
        {
            //Arrange
            var request = new RestRequest($"/api/Food/Delete/{foodId}", Method.Delete);

            //Act
            var response = this.client.Execute(request);
            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var data = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(data.Message, Is.EqualTo("Deleted successfully!"));
        }

        [Order(5)]
        [Test]
        public void CreateFood_WithUncorrectData_ShouldFail()
        {
            //Arrange
            var newFood = new FoodDTO
            {
                Description = "Some Description",
                Url = ""
            };
            var request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(newFood);

            //Act
            var response = this.client.Execute(request);
            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Order(6)]
        [Test]
        public void EditFood_WithUncorrectData_ShouldFail()
        {
            //Arrange
            var request = new RestRequest($"/api/Food/Edit/XXXXXXX", Method.Patch);
            request.AddJsonBody(new[]
            {
                new
                {
                    path = "/name",
                    op = "replace",
                    value = "New Food Title",
                },
            });

            //Act
            var response = this.client.Execute(request);
            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            var data = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(data.Message, Is.EqualTo("No food revues..."));

        }

        [Order(7)]
        [Test]
        public void DeleteFood_WithUncorrectData_Shouldfail()
        {
            //Arrange
            var request = new RestRequest($"/api/Food/Delete/XXXXXXX", Method.Delete);

            //Act
            var response = this.client.Execute(request);
            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var data = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(data.Message, Is.EqualTo("Unable to delete this food revue!"));
        }
    }
}