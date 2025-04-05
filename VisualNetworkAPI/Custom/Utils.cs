using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using VisualNetworkAPI.Models;

namespace VisualNetworkAPI.Custom
{
  public class Utils
  {
    private readonly IConfiguration _configuration;
    public Utils(IConfiguration configuration)
    {
      _configuration = configuration;
    }

    public string EncriptarSHA256(string texto)
    {
      using (SHA256 sha256Hash = SHA256.Create())
      {
        //Computar hash
        byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(texto));

        //Convertir el array de bytes a string
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < bytes.Length; i++)
        {
          builder.Append(bytes[i].ToString("x2"));
        }

        return builder.ToString();
      }
    }

    public string GenerarJWT(User modelo)
    {
      //Crear información del usuario para el token
      var userClaims = new[]
      {
        new Claim(ClaimTypes.NameIdentifier, modelo.Id.ToString()),
        new Claim(ClaimTypes.Email, modelo.Email)
      };

      var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:key"]!));
      var credential = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

      //Crear detalle del token
      var jwtConfig = new JwtSecurityToken(
          claims: userClaims,
          expires: DateTime.UtcNow.AddMinutes(10),
          signingCredentials: credential
        );

      return new JwtSecurityTokenHandler().WriteToken(jwtConfig);

    }

    public string GenerarRefreshToken()
    {
      var randomNumber = new byte[64];
      using var rng = RandomNumberGenerator.Create();
      rng.GetBytes(randomNumber);
      return Convert.ToBase64String(randomNumber);
    }

  }
}
