using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Chapter2
{
    class Program
    {
        static void Main(string[] args) {
            //Task<string> slt = DelayResult<string>("hello", TimeSpan.FromSeconds(2));
            //Console.WriteLine(slt.Result);
            Task<string> slt = DownloadStringWithRetries("http://www.ofmonkey2.com");
            Console.WriteLine(slt);
            Console.Read();
        }

        static async Task<T> DelayResult<T>(T result, TimeSpan delay) {
            await Task.Delay(delay);
            return result;
        }

        static async Task<string> DownloadStringWithRetries(string uri) {
            using (var client = new HttpClient()) {
                var nextDelay = TimeSpan.FromSeconds(1);
                for (int i = 0; i != 3; ++i) {
                    try {
                        return await client.GetStringAsync(uri);
                    } catch {

                    }
                    await Task.Delay(nextDelay);
                    nextDelay = nextDelay + nextDelay;  
                }
                return await client.GetStringAsync(uri);
            }
        }
    }
}
