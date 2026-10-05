using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using AppInsegura.Datos;
using AppInsegura.Modelos;

namespace AppInsegura.Servicios
{
    public class AuthService
    {
        private readonly BaseDatosUsuarios baseDatos;

        public AuthService(BaseDatosUsuarios baseDatos)
        {
            this.baseDatos = baseDatos;
        }

        public Usuario Registrar(string nombre, string contrasena, string rol = "jugador")
        {
            var nuevo = new Usuario
            {
                Nombre = nombre,
                ContrasenaHash = CalcularHash(contrasena),
                Rol = rol,
                TokenSesion = ""
            };

            baseDatos.Agregar(nuevo);
            return nuevo;
        }

        public Usuario? IniciarSesion(string nombre, string contrasena)
        {
            Usuario? usuario = baseDatos.BuscarExacto(nombre);
            if (usuario == null)
            {
                return null;
            }

            string hashIntento = CalcularHash(contrasena);
            if (usuario.ContrasenaHash != hashIntento)
            {
                return null;
            }

            usuario.TokenSesion = GenerarTokenSesion(); 
            
            //no mostramos token

            return usuario;
        }

        private string CalcularHash(string contrasena)
        {
            // Convertimos la contraseña a bytes para poder procesarla.
            byte[] bytesContrasena = Encoding.UTF8.GetBytes(contrasena);

            // Creamos SHA256 para generar un hash de la contraseña.
            using (SHA256 algoritmoHash = SHA256.Create())
            {
                // Calculamos el hash usando los bytes de la contraseña.
                byte[] bytesHash = algoritmoHash.ComputeHash(bytesContrasena);

                // Convertimos el resultado a texto hexadecimal.
                string hashEnTexto = Convert.ToHexString(bytesHash);

                return hashEnTexto;
            }
        }

        private string GenerarTokenSesion()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}
