using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Internal;
using GenHTTP.Modules.Controllers;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Practices;
using GenHTTP.Playground;
using Microsoft.EntityFrameworkCore;

var app = Layout.Create()
                .AddController<TestController>("test");

await Host.Create()
          .Handler(app)
          .Development()
          .Defaults()
          .Console()
          .RunAsync();

public class TestController
{

    [ControllerAction(RequestMethod.Get)]
    public async Task<LoginResponse> Login()
    {
        try
        {
            await using var context = new TestDbContext();

            /*await context.Database.EnsureCreatedAsync();
            
            context.Items.Add(new Item()
            {
                Text = "Dummy"
            });

            await context.SaveChangesAsync();*/

            var item = await context.Items.FirstOrDefaultAsync();

            return new LoginResponse(item?.Text ?? string.Empty);
        }
        catch (Exception e)
        {
            var em = e.Message;
            throw;
        }
    } 
    
}

public record LoginResponse(string Message);