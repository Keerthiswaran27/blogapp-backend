using BlogApp1.Server.Services;
using Supabase;
using System.Data;
using Qdrant.Client;

var builder = WebApplication.CreateBuilder(args);


var frontend_url = builder.Configuration["Frontend:Url"] ?? "http://localhost:7028";

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins(frontend_url) // Your WASM base URL
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ? Register HttpClient for DI before building the app
builder.Services.AddHttpClient();

var ollama_url = builder.Configuration["Ollama:BaseUrl"];
builder.Services.AddHttpClient("Ollama", client =>
{
    client.BaseAddress = new Uri(ollama_url); // Ollama default
});


// Add controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ? Supabase setup
var url = builder.Configuration["Supabase:Url"];       // from appsettings.json
var key = builder.Configuration["Supabase:Key"];     
var qdrant_url = builder.Configuration["Qdrant:url"];     
var qdrant_apikey = builder.Configuration["Qdrant:Apikey"];     

var supabaseOptions = new SupabaseOptions
{
    AutoRefreshToken = true,
    AutoConnectRealtime = false
};

var supabase = new Supabase.Client(url, key, supabaseOptions);

// Register Supabase in DI
builder.Services.AddSingleton(supabase);
builder.Services.AddSingleton(new QdrantClient(
    host: qdrant_url,
    port: 6334,
    https: true,
    apiKey: qdrant_apikey
));
builder.Services.AddHttpClient<GeminiService>();
builder.Services.AddSingleton<RAGIngestionService>();
builder.Services.AddScoped<RAGQueryService>();
builder.Services.AddScoped<IngestionService>();
builder.Services.AddScoped<RetrievalService>();
builder.Services.AddScoped<LlmService>();


var app = builder.Build();

app.UseCors("AllowBlazorClient");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.Run();
