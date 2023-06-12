using FacciensApi.DTO;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace FacciensApi.Servicios
{
    public class HashService
    {
        public ResultadoHashDTO Hash(string texto)
        {
            byte[] salt = new byte[16];
            using(RandomNumberGenerator random = RandomNumberGenerator.Create())
            {
                random.GetBytes(salt);
            }
            return Hash(texto, salt);
        }

        public ResultadoHashDTO Hash(string texto, byte[] salt)
        {
            byte[] llave = KeyDerivation.Pbkdf2(password: texto, salt: salt,prf:KeyDerivationPrf.HMACSHA1 ,iterationCount: 10000, numBytesRequested: 32);
            string hash = Convert.ToBase64String(llave);
            return new ResultadoHashDTO
            {
                Hash=hash,
                Sal=salt
            };
        }
    }
}
