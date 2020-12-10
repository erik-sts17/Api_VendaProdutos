using System.Security.Cryptography;
using System.Text;

namespace Desafio_Api.DTO
{
    public class LoginDTO
    {
        public string Email { get; set; }
        public string Senha { get; set; }

        public string CriarHash(string senha)
        {
            var md5 = MD5.Create();
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(senha); 
            byte[] hash = md5.ComputeHash(bytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}