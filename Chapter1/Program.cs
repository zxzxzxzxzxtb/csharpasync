using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter1
{
    class Program
    {
        static void Main(string[] args) {
            ExceptionTest();
            Console.Read();
        }

        static async Task DosomethingAsync() {
            int val = 13;
            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            val *= 2;
            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            Console.WriteLine(val);
        }

        void RotateMatrices(IEnumerable<Martix> matrices, float degrees) {
            Parallel.ForEach(matrices, m => m.Rotate(degrees));
        }

        IEnumerable<bool> PrimalityTest(IEnumerable<int> values) {
            return values.AsParallel().Select(val => IsPrime(val));
        }

        private bool IsPrime(int val) {
            return true;
        }

        void ProcessArray(double[] array) {
            Parallel.Invoke(
                ()=> ProcessPartialArray(array,0,array.Length /2),
                ()=> ProcessPartialArray(array,array.Length/2,array.Length)
            );
        }

        void ProcessPartialArray(double[] array,int begin,int end) {

        }

       static void ExceptionTest() {
            try {
                Parallel.Invoke(() => { throw new Exception(); }, () => { throw new Exception(); });
            }catch(AggregateException ex) {
                ex.Handle(e => {
                    Console.WriteLine(e.Message);
                    return true;
                });
            }
        }
    }

    internal class Martix
    {
        internal void Rotate(float degrees) {
            // do something
        }
    }
}
