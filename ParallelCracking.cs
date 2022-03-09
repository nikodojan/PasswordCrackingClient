using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PasswordClient.Models;

namespace PasswordClient
{
    public class ParallelCracking
    {
        CancellationTokenSource _source;
        CancellationToken _token;

        public ParallelCracking()
        {
            _source = new CancellationTokenSource();
            _token = _source.Token;
        }

        public UserInfoClearText Result { get; set; }

        /// <summary>
        /// Cracks one password with multiple parallel operations.
        /// Threads are managed by the application. Dictionary entries are taken one by one from the list.
        /// </summary>
        /// <param name="userInfo"></param>
        /// <param name="words"></param>
        /// <returns></returns>
        public UserInfoClearText StartParallelCracking(UserInfo userInfo, List<string> words)
        {
            Console.WriteLine($"New user info opened {userInfo.Username}:{userInfo.EntryptedPasswordBase64}");
            
            ConcurrentBag<UserInfoClearText> bag = new ConcurrentBag<UserInfoClearText>();

            Parallel.ForEach(words, (e, state) =>
            {
                Cracking cracking = new Cracking();
                UserInfoClearText partialResult = cracking.CheckWordWithVariations(e, userInfo);
                
                if (partialResult != null && !state.IsStopped && !state.ShouldExitCurrentIteration)
                {
                    state.Stop();
                    bag.Add(partialResult);
                }
            });
            return bag.Any() ? bag.ToArray()[0] : null;
        }

        /// <summary>
        /// Cracks the password with user managed threads.
        /// Each thread works on a chunk of the dictionary.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="words"></param>
        /// <returns></returns>
        public UserInfoClearText StartCrackingWithManagedThreads(UserInfo user, List<string> words)
        {
            //create chunks
            List<List<string>> dictChunks = new List<List<string>>();
            int size = words.Count / 4;
            int count = 0;
            for (int i = 0; i < 4; i++)
            {
                List<string> list = new List<string>();
                for (int k = 0; k < size; k++)
                {
                    list.Add(words[count]);
                    count++;
                }
                dictChunks.Add(list);
            }

            var rest = words.Count % 4;
            for (int j = words.Count - rest; j <= words.Count - 1; j++)
            {
                dictChunks[3].Add(words[j]);
            }

            Console.WriteLine(dictChunks.Count);
            Console.WriteLine(dictChunks[0].Count);
            Console.WriteLine(dictChunks[1].Count);
            Console.WriteLine(dictChunks[2].Count);
            Console.WriteLine(dictChunks[3].Count);

            Task[] tasks = new Task[4];

            for (int j = 0; j < 4; ++j)
            {
                int chunk = j;
                var task = Task.Run(() => CrackingTask(user, dictChunks[chunk], _token, PwCallback), _token);
                tasks[j] = task;
            }
            Stopwatch watch = new Stopwatch();
            watch.Start();
            try
            {
                Task.WaitAll(tasks, _token);
                Console.WriteLine(Result?.Password);
            }
            catch (Exception e)
            {
                //Console.WriteLine(e);
            }
            watch.Stop();
            Console.WriteLine(watch.Elapsed.TotalSeconds);
            return Result;
        }

        public void CrackingTask(UserInfo user, List<string> words, CancellationToken token, Action<UserInfoClearText> callback)
        {
            Cracking cracking = new Cracking();
            var result = cracking.RunCracking(user, words);
            if (result is not null)
            {
                //Console.WriteLine("Found match: " + result.Password);
                callback(result);
                _source.Cancel();
            }

        }

        public void PwCallback(UserInfoClearText clearText)
        {
            Result = clearText;
        }
    }
}
