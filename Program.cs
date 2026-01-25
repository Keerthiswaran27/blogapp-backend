using BlogApp1.Server.Services;
using Supabase;
using System.Data;
using Qdrant.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("https://localhost:7028") // Your WASM base URL
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ? Register HttpClient for DI before building the app
builder.Services.AddHttpClient();

builder.Services.AddHttpClient("Ollama", client =>
{
    client.BaseAddress = new Uri("http://localhost:11434"); // Ollama default
});


// Add controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ? Supabase setup
var url = builder.Configuration["Supabase:Url"];       // from appsettings.json
var key = builder.Configuration["Supabase:Key"];       // service role or anon key

var supabaseOptions = new SupabaseOptions
{
    AutoRefreshToken = true,
    AutoConnectRealtime = false
};

var supabase = new Supabase.Client(url, key, supabaseOptions);

// Register Supabase in DI
builder.Services.AddSingleton(supabase);
builder.Services.AddSingleton(new QdrantClient(
    host: "769593f4-dabf-4352-822f-7b913459b584.europe-west3-0.gcp.cloud.qdrant.io",
    port: 6334,
    https: true,
    apiKey: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhY2Nlc3MiOiJtIn0.Clj2wOfjzjRBQARnYl6TX355LErC4N0q4C-OuWTxrc8"
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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
