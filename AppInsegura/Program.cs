using System;
using System.IO;
using AppInsegura.Datos;
using AppInsegura.Modelos;
using AppInsegura.Servicios;

namespace AppInsegura
{
    public class Program
    {
        private static readonly BaseDatosUsuarios baseDatos = new BaseDatosUsuarios();
        private static readonly AuthService auth = new AuthService(baseDatos);
        private static Usuario? usuarioActual = null;

        public static void Main(string[] args)
        {

            CargarUsuariosDePrueba();
            

            Console.WriteLine("=== Gestor de Usuarios y Partidas ===");
            Console.WriteLine();

            bool salir = false;
            while (!salir)
            {
                MostrarMenu();
                string opcion = Console.ReadLine() ?? "";

                try
                {
                    switch (opcion)
                    {
                        case "1":
                            Registrar();
                            break;
                        case "2":
                            IniciarSesion();
                            break;
                        case "3":
                            BuscarUsuario();
                            break;
                        case "4":
                            VerPerfil();
                            break;
                        case "5":
                            PanelAdministracion();
                            break;
                        case "6":
                            SincronizarConServidor();
                            break;
                        case "0":
                            salir = true;
                            break;
                        default:
                            Console.WriteLine("Opción no válida.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ha ocurrido un error inesperado:");
                    Console.WriteLine(ex.ToString());
                }

                Console.WriteLine();
            }

            Console.WriteLine("Hasta luego.");
        }

        private static void CargarUsuariosDePrueba()
        {
            string carpeta = AppContext.BaseDirectory; //Obtenim la direccio de la carpeta on se executa el programa

            string ruta = Path.Combine(carpeta, "README-alumnado.md"); //busquem el readme dins de la carpeta

            while (!File.Exists(ruta)) //Si no trobem el readme, anem pujant de carpetes i tornem a buscar en aquella
            {
                DirectoryInfo? padre = Directory.GetParent(carpeta);

                if (padre == null) //Si no existeix una carpeta pare deixem de buscar
                {
                    break;
                }

                carpeta = padre.FullName; //pujem a la carpeta pare

                ruta = Path.Combine(carpeta, "README-alumnado.md"); //tornem a crear la ruta del readme
            }

            if (!File.Exists(ruta)) //Si no trobem el readme enviem un missatge i sortim
            {
                Console.WriteLine("No se ha encontrado el README.");
                return;
            }

            string[] lineas = File.ReadAllLines(ruta); //llegim totes les linees del readme

            foreach (string linea in lineas) //recorrem cada linea del readme
            {
                string[] datos = linea.Split('|'); //separem les linees utilitzan |

                if (datos.Length >= 4) //comprobem que la linea tingui suficients datos per a poder llegir: el usuari, rol i contrasenya
                {
                    string nombre = datos[1].Trim(); //obtenim nom de usuari

                    string contrasena = datos[2].Trim();

                    string rol = datos[3].Trim();

                    if (nombre != "Usuario" && !nombre.Contains("---")) //ignorem la capçalera
                    {
                        auth.Registrar(nombre, contrasena, rol); //Registrem
                    }
                }
            }
        }

        private static void MostrarMenu()
        {
            Console.WriteLine("------------------------------------");
            Console.WriteLine($"Usuario actual: {(usuarioActual != null ? usuarioActual.Nombre : "ninguno")}");
            Console.WriteLine("1. Registrar usuario");
            Console.WriteLine("2. Iniciar sesión");
            Console.WriteLine("3. Buscar usuario por nombre");
            Console.WriteLine("4. Ver mi perfil");
            if (usuarioActual != null && usuarioActual.Rol == "admin")
            {
                Console.WriteLine("5. Panel de administración");
            }
            Console.WriteLine("6. Sincronizar partida con el servidor");
            Console.WriteLine("0. Salir");
            Console.Write("Elige una opción: ");
        }

        private static void Registrar()
        {
            Console.Write("Nombre de usuario: ");
            string nombre = Console.ReadLine() ?? "";
            Console.Write("Contraseña: ");
            string contrasena = "";
            ConsoleKeyInfo tecla;

            do // Ocultamos la contraseña mientras escribimos con *
            {
                tecla = Console.ReadKey(true);

                if (tecla.Key != ConsoleKey.Enter)
                {
                    contrasena = contrasena + tecla.KeyChar; 
                    Console.Write("*");
                }
            } while (tecla.Key != ConsoleKey.Enter);

            Console.WriteLine();

            Usuario nuevo = auth.Registrar(nombre, contrasena);
            Console.WriteLine($"Usuario '{nuevo.Nombre}' registrado con rol '{nuevo.Rol}'.");
        }

        private static void IniciarSesion()
        {
            Console.Write("Nombre de usuario: ");
            string nombre = Console.ReadLine() ?? "";
            Console.Write("Contraseña: ");
            string contrasena = "";
            ConsoleKeyInfo tecla;

            do
            {
                tecla = Console.ReadKey(true);

                if (tecla.Key != ConsoleKey.Enter)
                {
                    contrasena = contrasena + tecla.KeyChar;
                    Console.Write("*");
                }
            } while (tecla.Key != ConsoleKey.Enter);

            Console.WriteLine();

            Usuario? usuario = auth.IniciarSesion(nombre, contrasena);
            if (usuario == null)
            {
                Console.WriteLine("Usuario o contraseña incorrectos.");
                return;
            }

            usuarioActual = usuario;
            Console.WriteLine($"Bienvenido, {usuario.Nombre}.");
        }

        private static void BuscarUsuario()
        {
            Console.Write("Nombre a buscar: ");
            string nombre = Console.ReadLine() ?? "";

            Usuario? encontrado = baseDatos.BuscarPorNombre(nombre);
            Console.WriteLine(encontrado != null
                ? $"Encontrado: {encontrado.Nombre} (rol: {encontrado.Rol})"
                : "No se ha encontrado ningún usuario con ese nombre.");
        }

        private static void VerPerfil()
        {
            if (usuarioActual == null)
            {
                Console.WriteLine("Primero debes iniciar sesión.");
                return;
            }

            // Hemos eliminado la opción que mostrara el token
            Console.WriteLine($"Nombre: {usuarioActual.Nombre}");
            Console.WriteLine($"Rol: {usuarioActual.Rol}");
        }

        private static void PanelAdministracion()
        {
            if(usuarioActual == null || usuarioActual.Rol != "admin") //Evitamos que cualquiera que no sea admin pueda ver la lista de usuarios
            {
                Console.WriteLine("No tienes permisos para acceder a esta opcion");
                return;

            }

            Console.WriteLine("=== PANEL DE ADMINISTRACIÓN ===");
            Console.WriteLine("Lista de usuarios registrados:");
            foreach (Usuario u in baseDatos.ListarTodos())
            {
                Console.WriteLine($" - {u.Nombre} ({u.Rol})");
            }
        }

        private static void SincronizarConServidor()
        {
            if (usuarioActual == null)
            {
                Console.WriteLine("Primero debes iniciar sesión.");
                return;
            }

            var red = new RedService();
            red.EnviarPuntuacion(usuarioActual.Nombre, 1000);
        }
    }
}
