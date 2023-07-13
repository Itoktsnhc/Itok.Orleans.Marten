using Itok.Orleans.Demo;
using Orleans.Hosting;
using Itok.Orleans.Persistence.Marten;
using Marten;
using Microsoft.OpenApi.Models;
using Orleans;

var builder = Host.CreateDefaultBuilder(args);

builder.UseOrleans(siloBuilder =>
{
    siloBuilder.UseLocalhostClustering();
    siloBuilder.AddMartenGrainStorage("marten", options =>
    {
        options.StoreOptions = new StoreOptions();
        options.StoreOptions.Connection(
            "Host=localhost;Port=5432;Database=marten_db;Username=iom_user;Password=111111");
    });
    siloBuilder.ConfigureApplicationParts(config =>
    {
        config.AddApplicationPart(typeof(StateGrain).Assembly).WithReferences();
    });
});

builder.ConfigureWebHostDefaults(webBuilder =>
{
    webBuilder.ConfigureServices(services =>
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "Itok.DEMO", Version = "v1"}); });
    });
    webBuilder.Configure((ctx, app) =>
    {
        if (ctx.HostingEnvironment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseSwagger();
        app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Itok.DEMO"); });
        ;
        app.UseRouting();
        app.UseAuthorization();
        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    });
});


var app = builder.Build();


await app.RunAsync();