using AtlasSasS.Application.Usecases.Login;
using AtlasSasS.Domain.Abstractions.Login;
using AtlasSasS.Domain.Abstractions.Repository;
using AtlasSasS.Infrastructure.Login;
using AtlasSasS.Infrastructure.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddSingleton<IJwtService, JwtService>();

builder.Services.AddSingleton<RegisterWithPassword>();
builder.Services.AddSingleton<LoginWithPassword>();

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>();

builder.Services
	.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidIssuer = jwt!.Issuer,
			ValidateAudience = true,
			ValidAudience = jwt.Audience,
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.HmacSecret)),
			ValidateLifetime = true,
			ClockSkew = TimeSpan.FromSeconds(30)
		};

		// 선택: Refresh 토큰이 API로 들어오는 걸 막기
		options.Events = new JwtBearerEvents
		{
			OnTokenValidated = ctx => {
				var typ = ctx.Principal?.FindFirst("typ")?.Value;
				if (typ == "refresh")
					ctx.Fail("Refresh token not allowed for API access");
				return Task.CompletedTask;
			}
		};
	});

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
