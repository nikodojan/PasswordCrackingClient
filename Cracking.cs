using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PasswordClient.Models;
using PasswordClient.util;

namespace PasswordClient
{
    public class Cracking
    {
        /// <summary>
        /// The algorithm used for encryption.
        /// Must be exactly the same algorithm that was used to encrypt the passwords in the password file
        /// </summary>
        private readonly HashAlgorithm _messageDigest;

        private string FilePath = "dictionary.txt";


        public Cracking()
        {
            _messageDigest = new SHA1CryptoServiceProvider();
            //_messageDigest = new MD5CryptoServiceProvider();
            // seems to be same speed

        }
        public UserInfoClearText RunCracking(UserInfo userInfo, List<string> words)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Console.WriteLine("User Info opened");

            foreach (var word in words)
            {
                UserInfoClearText partialResult = CheckWordWithVariations(word, userInfo);
                if (partialResult is not null) return partialResult;
            }
            
            stopwatch.Stop();
            return null;
        }

        /// <summary>
        /// Runs the password cracking algorithm
        /// </summary>
        public UserInfoClearText RunCracking(UserInfo userInfo)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Console.WriteLine("User Info opened");

            using (FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read))

            using (StreamReader dictionary = new StreamReader(fs))
            {
                while (!dictionary.EndOfStream)
                {
                    String dictionaryEntry = dictionary.ReadLine();
                    UserInfoClearText partialResult = CheckWordWithVariations(dictionaryEntry, userInfo);
                    if (partialResult is not null)
                    {
                        stopwatch.Stop();
                        Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);
                        return partialResult;
                    };
                }
            }

            return null;
            //Console.WriteLine(string.Join(", ", result));
            //Console.WriteLine("Out of {0} password {1} was found ", userInfos.Count, result.Count);
            //Console.WriteLine();
            
        }

        /// <summary>
        /// Generates a lot of variations, encrypts each of the and compares it to all entries in the password file
        /// </summary>
        /// <param name="dictionaryEntry">A single word from the dictionary</param>
        /// <param name="userInfo">List of (username, encrypted password) pairs from the password file</param>
        /// <returns>A list of (username, readable password) pairs. The list might be empty</returns>
        public UserInfoClearText CheckWordWithVariations(String dictionaryEntry, UserInfo userInfo)
        {
            List<UserInfoClearText> result = new List<UserInfoClearText>(); //might be empty
            
            String possiblePassword = dictionaryEntry;
            UserInfoClearText partialResult = CheckSingleWord(userInfo, possiblePassword);
            if (partialResult is not null) return partialResult;

            String possiblePasswordUpperCase = dictionaryEntry.ToUpper();
            UserInfoClearText partialResultUpperCase = CheckSingleWord(userInfo, possiblePasswordUpperCase);
            if (partialResultUpperCase is not null) return partialResultUpperCase;

            String possiblePasswordCapitalized = StringUtilities.Capitalize(dictionaryEntry);
            UserInfoClearText partialResultCapitalized = CheckSingleWord(userInfo, possiblePasswordCapitalized);
            if (partialResultCapitalized is not null) return partialResultCapitalized;

            String possiblePasswordReverse = StringUtilities.Reverse(dictionaryEntry);
            UserInfoClearText partialResultReverse = CheckSingleWord(userInfo, possiblePasswordReverse);
            if (partialResultReverse is not null) return partialResultReverse;

            for (int i = 0; i < 100; i++)
            {
                String possiblePasswordEndDigit = dictionaryEntry + i;
                UserInfoClearText partialResultEndDigit = CheckSingleWord(userInfo, possiblePasswordEndDigit);
                if (partialResultEndDigit is not null) return partialResultEndDigit;
            }

            for (int i = 0; i < 100; i++)
            {
                String possiblePasswordStartDigit = i + dictionaryEntry;
                UserInfoClearText partialResultStartDigit = CheckSingleWord(userInfo, possiblePasswordStartDigit);
                if (partialResultStartDigit is not null) return partialResultStartDigit;
            }

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    String possiblePasswordStartEndDigit = i + dictionaryEntry + j;
                    UserInfoClearText partialResultStartEndDigit = CheckSingleWord(userInfo, possiblePasswordStartEndDigit);
                    if (partialResultStartEndDigit is not null) return partialResultStartEndDigit;
                }
            }
            
            return null;
        }

        /// <summary>
        /// Checks a single word (or rather a variation of a word): Encrypts and compares to all entries in the password file
        /// </summary>
        /// <param name="userInfos"></param>
        /// <param name="possiblePassword">List of (username, encrypted password) pairs from the password file</param>
        /// <returns>A list of (username, readable password) pairs. The list might be empty</returns>
        private UserInfoClearText CheckSingleWord(UserInfo userInfo, String possiblePassword)
        {
            char[] charArray = possiblePassword.ToCharArray();
            byte[] passwordAsBytes = Array.ConvertAll(charArray, PasswordFileHandler.GetConverter());

            byte[] encryptedPassword = _messageDigest.ComputeHash(passwordAsBytes);
            //string encryptedPasswordBase64 = System.Convert.ToBase64String(encryptedPassword);

            
            if (CompareBytes(userInfo.EntryptedPassword, encryptedPassword))  //compares byte arrays
            {
                Console.WriteLine(userInfo.Username + " " + possiblePassword);
                return new UserInfoClearText(userInfo.Username, possiblePassword);
            }
            
            return null;
        }

        /// <summary>
        /// Compares to byte arrays. Encrypted words are byte arrays
        /// </summary>
        /// <param name="firstArray"></param>
        /// <param name="secondArray"></param>
        /// <returns></returns>
        private static bool CompareBytes(IList<byte> firstArray, IList<byte> secondArray)
        {
            //if (secondArray == null)
            //{
            //    throw new ArgumentNullException("firstArray");
            //}
            //if (secondArray == null)
            //{
            //    throw new ArgumentNullException("secondArray");
            //}
            if (firstArray.Count != secondArray.Count)
            {
                return false;
            }
            for (int i = 0; i < firstArray.Count; i++)
            {
                if (firstArray[i] != secondArray[i])
                    return false;
            }
            return true;
        }

    }
}
