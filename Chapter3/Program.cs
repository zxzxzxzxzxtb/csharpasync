using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chapter3
{
    class Program
    {
        static void Main(string[] args) {

            Console.Read();
        }

        #region 3.1数据的并行处理
        /// <summary>
        /// 并行遍历循环，并循环角度，可以在外面取消循环
        /// </summary>
        /// <param name="matrices"></param>
        /// <param name="degrees"></param>
        void RotateMatrices(IEnumerable<Matrix> matrices, float degrees, CancellationToken token) {
            Parallel.ForEach(matrices, new ParallelOptions { CancellationToken = token }, m => m.Rotate(degrees));
        }

        /// <summary>
        /// 如果循环过程中发现无效值，则中断循环
        /// </summary>
        /// <param name="matrices"></param>
        void InvertMatrices(IEnumerable<Matrix> matrices) {
            Parallel.ForEach(matrices, (matrix, state) => {
                if (!matrix.IsInvertible) {
                    state.Stop();
                } else {
                    matrix.IsInvert();
                }
            });
        }

        /// <summary>
        /// 保护共享变量的状态，用lock实现
        /// </summary>
        /// <param name="matrices"></param>
        /// <returns></returns>
        int InvertMatrices(IEnumerable<Matrix> matrices) {
            object mutex = new object();
            int nonInvertibleCount = 0;
            Parallel.ForEach(matrices, matrix => {
                if (matrix.IsInvertible) {
                    matrix.IsInvert();
                } else {
                    lock (mutex) {
                        ++nonInvertibleCount;
                    }
                }
            });
            return nonInvertibleCount;
        }

        #endregion

        #region 3.2并行聚合
        /// <summary>
        /// Parallel方式计算sum
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        static int ParallelSum(IEnumerable<int> values) {
            object mutex = new object();
            int result = 0;
            Parallel.ForEach(source: values,
                localInit: () => 0,
                body: (item, state, localValue) => localValue + item,
                localFinally: localValue => {
                    lock (mutex) {
                        result += localValue;
                    }
                });
            return result;
        }

        /// <summary>
        /// PLINQ方式计算sum
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        static int ParallelSum2(IEnumerable<int> values) {
            return values.AsParallel().Sum();
        }

        static int ParallelSum3(IEnumerable<int> values) {
            return values.AsParallel().Aggregate(
                seed: 0,
                func: (sum, item) => sum + item
                );
        }
        #endregion

        #region 3.3并行调用
        static void ProcessArray(double[] array) {
            Parallel.Invoke(
                () => ProcessPartialArray(array, 0, array.Length /2),
                () => ProcessPartialArray(array, array.Length/2, array.Length)
            );
        }

        static void ProcessPartialArray(double[] array, int begin, int end) {

        }

        static void DoAction20Times(Action action) {
            Action[] actions = Enumerable.Repeat(action, 20).ToArray();
            Parallel.Invoke(actions);
        }

        static void DoActions20Times(Action action, CancellationToken token) {
            Action[] actions = Enumerable.Repeat(action, 20).ToArray();
            Parallel.Invoke(new ParallelOptions { CancellationToken = token }, actions);
        }
        #endregion

        #region 3.4动态并行
        public void ProcessTree(Node root) {
            var task = Task.Factory.StartNew(() => Traverse(root),
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskScheduler.Default
            );
            task.Wait();
        }

        private void Traverse(Node current) {
            if (current.Left != null) {
                Task.Factory.StartNew(()=>Traverse(current.Left),
                    CancellationToken.None,
                    TaskCreationOptions.AttachedToParent,
                    TaskScheduler.Default
                );
            }
            if (current.Right != null) {
                Task.Factory.StartNew(() => Traverse(current.Right),
                    CancellationToken.None,
                    TaskCreationOptions.AttachedToParent,
                    TaskScheduler.Default
                );
            }
        }

        void TaskContinuation() {
            Task task = Task.Factory.StartNew(
                    () => Thread.Sleep(TimeSpan.FromSeconds(2)),
                    CancellationToken.None,
                    TaskCreationOptions.None,
                    TaskScheduler.Default
                );
            Task continuation = task.ContinueWith(
                    t => Trace.WriteLine("Task is done"),
                    CancellationToken.None,
                    TaskContinuationOptions.None,
                    TaskScheduler.Default
                );
        }
        #endregion

        #region 3.5并行LINQ
        static IEnumerable<int> MultiyBy2(IEnumerable<int> values) {
            return values.AsParallel().AsOrdered().Select(item => item * 2);
        }

        static int ParallelSum(IEnumerable<int> values) {
            return values.AsParallel().Sum();
        }
        #endregion
    }

    class Node{
        public Node Left{get;set;}

        public Node Right{get;set;}
    }
}
