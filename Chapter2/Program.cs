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
            //Task<string> slt = DownloadStringWithTimeout("http://www.baidu.com");
            Task<string> slt = DownloadAllAsync(new List<string>() { "http://www.baidu.com", "http://www.qq.com" });
            Console.WriteLine(slt.Result);
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

        #region 2.3报告进度
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

        #region 2.4等待一组任务完成
        static async Task<string> DownloadAllAsync(IEnumerable<string> urls) {
            var httpClient = new HttpClient();
            var downloads = urls.Select(url => httpClient.GetStringAsync(url));
            Task<string>[] downloadTasks = downloads.ToArray();
            string[] htmlPages = await Task.WhenAll(downloadTasks);
            return string.Concat(htmlPages);
        }

        static async Task ThrowNotImplementedExceptionAsync() {
            throw new NotImplementedException();
        }

        static async Task ThrowInvalidOperationExceptionAsync() {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// 随机捕获一个异常
        /// </summary>
        /// <returns></returns>
        static async Task ObserveOneExceptionAsync() {
            var task1 = ThrowNotImplementedExceptionAsync();
            var task2 = ThrowInvalidOperationExceptionAsync();
            try {
                await Task.WhenAll(task1, task2);
            } catch (Exception ex) {

            }
        }

        /// <summary>
        /// 捕获所有异常
        /// </summary>
        /// <returns></returns>
        static async Task ObserveAllExceptionAsync() {
            var task1 = ThrowNotImplementedExceptionAsync();
            var task2 = ThrowInvalidOperationExceptionAsync();
            Task alltasks = Task.WhenAll(task1, task2);
            try {
                await alltasks;
            } catch {
                AggregateException allExceptions = alltasks.Exception;
            }
        }
        #endregion

        #region 2.5等待何意一个任务完成
        static async Task<int> FirstRespondingUrlAsync(string urlA, string urlB) {
            var httpClient = new HttpClient();
            Task<byte[]> downloadTaskA = httpClient.GetByteArrayAsync(urlA);
            Task<byte[]> downloadTaskB = httpClient.GetByteArrayAsync(urlB);
            Task<byte[]> completedTask = await Task.WhenAny(downloadTaskA, downloadTaskB);
            byte[] data = await completedTask;
            return data.Length;
        }
        #endregion

        #region 任务完成时的处理
        static async Task<int> DelayAndReturnAsync(int val) {
            await Task.Delay(TimeSpan.FromSeconds(val));
            return val;
        }
        
        static async Task ProcessTasksAsync() {
            Task<int> TaskA = DelayAndReturnAsync(2);
            Task<int> TaskB = DelayAndReturnAsync(3);
            Task<int> TaskC = DelayAndReturnAsync(1);
            var tasks = new[] { TaskA, TaskB, TaskC };
            var processingTasks = tasks.Select(async t => {
                var result = await t;
                Console.WriteLine(result);
            });
            await Task.WhenAll(processingTasks);
        }
        #endregion
    }
}
