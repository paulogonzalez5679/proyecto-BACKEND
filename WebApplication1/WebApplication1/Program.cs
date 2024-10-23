using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configura la cadena de conexi�n
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Registra ApplicationDbContext con SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add services to the container.
builder.Services.AddRazorPages(); // Aseg�rate de que esta l�nea est� presente.

// Registrar los controladores para la API
builder.Services.AddControllers();

// Agregar Swagger como servicio
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    // Configurar Swagger solo para el entorno de desarrollo
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = string.Empty; 
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Mapea los controladores y las p�ginas Razor
app.MapControllers();
app.MapRazorPages(); // Aseg�rate de que esta l�nea est� presente.

app.Run();
