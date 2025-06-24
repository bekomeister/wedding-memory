using Google.Apis.Auth.OAuth2;
using FirebaseAdmin;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession();

// Firebase Admin SDK başlat
var firebaseConfig = builder.Configuration.GetSection("Firebase");
var credentialPath = firebaseConfig["CredentialPath"];
var firebaseKeyJson = Environment.GetEnvironmentVariable("FIREBASE_KEY_JSON");

if (!string.IsNullOrEmpty(firebaseKeyJson))
{
    // Environment variable'dan Firebase key'i oku
    var tempPath = Path.GetTempFileName();
    await System.IO.File.WriteAllTextAsync(tempPath, firebaseKeyJson);
    credentialPath = tempPath;
}

if (!string.IsNullOrEmpty(credentialPath) && FirebaseApp.DefaultInstance == null)
{
    FirebaseApp.Create(new AppOptions()
    {
        Credential = GoogleCredential.FromFile(credentialPath)
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
