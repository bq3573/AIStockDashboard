using StockInfoApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers(); // Registers controllers
builder.Services.AddHttpClient<StockService>(); // Registers the StockService
builder.Services.AddHttpClient<FinnHubService>(); // Registers the StockService
builder.Services.AddRazorPages(); // Registers Razor Pages
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policyBuilder =>
    {
        policyBuilder.AllowAnyOrigin()
                     .AllowAnyMethod()
                     .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts(); // For production, adds HTTP Strict Transport Security
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseCors(); // Enable CORS before mapping controllers
app.UseAuthorization();

app.MapControllers(); // Ensure controllers are mapped
app.MapRazorPages(); // Ensure Razor Pages are mapped

app.Run();
