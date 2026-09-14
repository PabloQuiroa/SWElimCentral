// Pablo José Quiroa Martínez - 0901-22-2929
using System.Security.Cryptography;
using System.Text;

namespace SistemaWebElimCentral.Controllers.Seguridad
{
    public static class Cls_Hash_Controlador
    {
        public static string HashearSHA256(string sContrasena)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(sContrasena));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        public static bool VerificarContrasena(string sContrasena, string sHash)
        {
            return HashearSHA256(sContrasena) == sHash;
        }
    }
}