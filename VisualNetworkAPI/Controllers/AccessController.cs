using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisualNetworkAPI.Custom;
using VisualNetworkAPI.Models;
using VisualNetworkAPI.Models.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace VisualNetworkAPI.Controllers
{
  [Route("api/[controller]")]
  [AllowAnonymous]
  [ApiController]
  public class AccessController : ControllerBase
  {
    private readonly VisualNetworkContext _context;
    private readonly Utils _utils;
    public AccessController(VisualNetworkContext context, Utils utils)
    {
      _context = context;
      _utils = utils;
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register(UserDTO objeto)
    {
      // Verificar si ya existe un usuario con el mismo correo electrónico
      if (await _context.Users.AnyAsync(u => u.Email == objeto.Email))
      {
        return StatusCode(StatusCodes.Status409Conflict, new { message = "Ya existe un usuario con este correo electrónico." });
      }

      // Verificar si ya existe un usuario con el mismo nombre de usuario
      if (await _context.Users.AnyAsync(u => u.User1 == objeto.User))
      {
        return StatusCode(StatusCodes.Status409Conflict, new { message = "Ya existe un usuario con este nombre de usuario." });
      }

      var modelUser = new User
      {
        User1 = objeto.User,
        Password = _utils.EncriptarSHA256(objeto.Password),
        Email = objeto.Email,
        FirstName = objeto.FirstName,
        LastName = objeto.LastName,
        Phone = objeto.Phone,
        Address = objeto.Address,
        Genre = objeto.Genre,
        Avatar = "/avatars/blank.svg",
        Active = true,
        CreatedBy = objeto.FirstName + " " + objeto.LastName,
        CreatedDate = DateTime.UtcNow,
        LastUpdate = DateTime.UtcNow,
      };

      await _context.Users.AddAsync(modelUser);
      await _context.SaveChangesAsync();

      if (modelUser.Id != 0)
      {
        return StatusCode(StatusCodes.Status200OK, new { success = true });
      }
      else
      {
        return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Error al guardar el usuario." });
      }
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login(LoginDTO loginObject)
    {
      // Validar si el objeto de login es nulo
      if (loginObject == null)
      {
        return StatusCode(StatusCodes.Status400BadRequest, new { message = "Los datos de inicio de sesión no pueden estar vacíos." });
      }

      // Validar si el campo de email está presente
      if (string.IsNullOrWhiteSpace(loginObject.Email))
      {
        return StatusCode(StatusCodes.Status400BadRequest, new { message = "Se requiere el nombre de usuario o el correo electrónico." });
      }

      // Validar si la contraseña está presente
      if (string.IsNullOrWhiteSpace(loginObject.Password))
      {
        return StatusCode(StatusCodes.Status400BadRequest, new { message = "La contraseña es requerida." });
      }

      var loginIdentifier = loginObject.Email.Trim(); 

      var user = await _context.Users
          .Where(u =>
              (u.Email == loginObject.Email || u.User1 == loginIdentifier) &&
              u.Password == _utils.EncriptarSHA256(loginObject.Password)
          )
          .FirstOrDefaultAsync();

      if (user == null)
      {
        return StatusCode(StatusCodes.Status401Unauthorized, new { isSuccess = false, message = "Credenciales inválidas." });
      }
      else if (!(bool)user.Active)
      {
        return StatusCode(StatusCodes.Status403Forbidden, new { isSuccess = false, message = "Esta cuenta está inactiva." });
      }
      else
      {
        var accessToken = _utils.GenerarJWT(user);
        var refreshToken = _utils.GenerarRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7); 

        var refreshTokenEntity = new RefreshToken
        {
          UserId = user.Id,
          Token = refreshToken,
          ExpiryDate = refreshTokenExpiry,
          CreatedAt = DateTime.UtcNow,
          RevokedAt = null
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, accessToken = accessToken, refreshToken = refreshToken });
      }
    }

    [HttpPost]
    [Route("refresh-token")]
    public async Task<IActionResult> RefreshToken(string refreshToken)
    {
      if (string.IsNullOrEmpty(refreshToken))
      {
        return BadRequest("Refresh Token is required.");
      }

      var refreshTokenEntity = await _context.RefreshTokens
          .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

      if (refreshTokenEntity == null || refreshTokenEntity.ExpiryDate <= DateTime.UtcNow || refreshTokenEntity.RevokedAt != null)
      {
        return Unauthorized("Invalid or expired Refresh Token.");
      }

      var user = await _context.Users.FindAsync(refreshTokenEntity.UserId);
      if (user == null || !(bool)user.Active)
      {
        return Unauthorized("Invalid user associated with Refresh Token.");
      }

      // Revocar el Refresh Token antiguo (opcional pero recomendado)
      refreshTokenEntity.RevokedAt = DateTime.UtcNow;
      _context.RefreshTokens.Update(refreshTokenEntity);

      // Generar un nuevo Access Token y Refresh Token
      var newAccessToken = _utils.GenerarJWT(user);
      var newRefreshToken = _utils.GenerarRefreshToken();
      var newRefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

      var newRefreshTokenEntity = new RefreshToken
      {
        UserId = user.Id,
        Token = newRefreshToken,
        ExpiryDate = newRefreshTokenExpiry,
        CreatedAt = DateTime.UtcNow,
        RevokedAt = null
      };

      _context.RefreshTokens.Add(newRefreshTokenEntity);
      await _context.SaveChangesAsync();

      return Ok(new { accessToken = newAccessToken, refreshToken = newRefreshToken });
    }

    [HttpPost]
    [Route("logout")]
    public async Task<IActionResult> Logout(string refreshToken)
    {
      if (string.IsNullOrEmpty(refreshToken))
      {
        return BadRequest("Refresh Token is required.");
      }

      var refreshTokenEntity = await _context.RefreshTokens
          .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.RevokedAt == null);

      if (refreshTokenEntity != null)
      {
        refreshTokenEntity.RevokedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Logged out successfully." });
      }

      return BadRequest("Invalid or already logged out Refresh Token.");
    }

  }
}
