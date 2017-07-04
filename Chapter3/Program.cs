using System;
using System.Collections.Generic;
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
    }
}
