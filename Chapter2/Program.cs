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
            //Task<string> slt = DownloadStringWithRetries("http://www.ofmonkey2.com");
            Task<string> slt = DownloadStringWithTimeout("http://www.baidu.com");
            Console.WriteLine(slt);
            Console.Read();
        }

        #region 2.1暂停一段时间
        /// <summary>
        /// 异步方式等待一段时间
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        static async Task<T> DelayResult<T>(T result, TimeSpan delay) {
            await Task.Delay(delay);
            return result;
        }

        /// <summary>
        /// 指数退避，第一次重试前等一秒，第二次等2秒，第三次等4秒
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 超时三秒就返回null
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        static async Task<string> DownloadStringWithTimeout(string uri) {
            using (var client = new HttpClient()) {
                var downloadTask = client.GetStringAsync(uri);
                var timeoutTask = Task.Delay(3000);
                var completedTask = await Task.WhenAny(downloadTask, timeoutTask);
                if (completedTask == timeoutTask) {
                    return null;
                }
                return await downloadTask;
            }
        } 
        #endregion

        #region 报告进度
        static async Task MyMethodAsync(IProgress<double> progress = null) {
            double percentComplete = 0;
            bool done = false;
            while (!done) {
                if (progress != null) {
                    progress.Report(percentComplete);
                }
            }
        }

        static async Task CallMyMethodAsync() {
            var progress = new Progress<double>();
            progress.ProgressChanged += (sender, args) => { 
            
            };
            await MyMethodAsync(progress);
        }

        #endregion

    }
}
