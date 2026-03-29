using System;
using System.Collections.Generic;
using System.Text;

namespace servicetest.E2ETests;

public class UserFlowTests : ITestFlow
{
    public async Task RunAsync(TestRunner test)
    {
        Console.WriteLine("RUNNING USER E2E FLOW");

        string testEmail = $"user_{Guid.NewGuid()}@test.com";
        string testPassword = "Test123!";

        await test.Step("Register new user", async () =>
        {
            var res = await test.PostAsync("api/users/register", new
            {
                Username = "TestUser",
                Email = testEmail,
                Password = testPassword,
                IsModerator = false
            });

            test.Assert(res.Code == 200, "Expected HTTP 200 OK for register.", res.Json.ToString());
        });

        await test.Step("Fail to register duplicate email", async () =>
        {
            var res = await test.PostAsync("api/users/register", new
            {
                Username = "CopyTestUser",
                Email = testEmail,
                Password = "Password1!",
                IsModerator = false
            });

            test.Assert(res.Code == 400, "Expected HTTP 400 for duplicate email.", res.Json.ToString());
        });

        await test.Step("Fail to login with wrong password", async () =>
        {
            var res = await test.PostAsync("api/users/auth", new
            {
                Email = testEmail,
                Password = "WrongPassword1!"
            });

            test.Assert(res.Code == 400, "Expected HTTP 400 for wrong password.", res.Json.ToString());
        });

        await test.Step("Login user", async () =>
        {
            var res = await test.PostAsync("api/users/auth", new
            {
                Email = testEmail,
                Password = testPassword
            });

            test.Assert(res.Code == 200, "Expected HTTP 200 for login user.", res.Json.ToString());

            string token = res.Json.GetProperty("JwtToken").GetString()!;
            test.SetBearer(token);
        });
    }
}
