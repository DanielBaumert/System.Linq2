namespace System.Linq2 {
    public unsafe delegate void UnsafeAction<T1>(T1* arg1) where T1 : unmanaged;

    public unsafe delegate void UnsafeAction<T1, T2>(T1* arg1, T2 arg2) where T1 : unmanaged;
    public unsafe delegate void UnsafeAction2<T1, T2>(T1* arg1, T2* arg2) where T1 : unmanaged where T2 : unmanaged;

    public unsafe delegate void UnsafeAction<T1, T2, T3>(T1* arg1, T2* arg2, T3* arg3) where T1 : unmanaged where T2 : unmanaged where T3 : unmanaged;

    public unsafe delegate TOut UnsafeFunc<T1, TOut>(T1* arg1) where T1 : unmanaged;
    public unsafe delegate TOut* UnsafeFunc2<T1, TOut>(T1* arg1) where T1 : unmanaged where TOut : unmanaged;

    public unsafe delegate TOut UnsafeFunc<T1, T2, TOut>(T1* arg1, T2* arg2) where T1 : unmanaged where T2 : unmanaged;
    public unsafe delegate TOut* UnsafeFunc2<T1, T2, TOut>(T1* arg1, T2* arg2) where T1 : unmanaged where T2 : unmanaged where TOut : unmanaged;

    public static class Linq {
        public static IEnumerable<TOut> Select<T, TOut>(this T[] source, UnsafeFunc<T, TOut> func) where T : unmanaged {
            unsafe TOut innerLoopCall(int i) {
                fixed (T* itemPtr = source) {
                    return func(itemPtr + i);
                }
            }

            for (int i = 0, n = source.Length; i < n; i++) {
                yield return innerLoopCall(i);
            }
        }

        public static IEnumerable<TOut> Select<T, TOut>(this T[] source, UnsafeFunc<T, int, TOut> func) where T : unmanaged where TOut : unmanaged {
            unsafe TOut innerLoopCall(int i) {
                fixed (T* itemPtr = source) {
                    return func(itemPtr + i, &i);
                }
            }

            for (int i = 0, n = source.Length; i < n; i++) {
                yield return innerLoopCall(i);
            }
        }


        public unsafe static void ForEach<T>(this T[] source, UnsafeAction<T> action) where T : unmanaged {
            fixed (T* itemPtr = source) {
                for (int i = 0, n = source.Length; i < n; i++) {
                    action(itemPtr + i);
                }
            }
        }

        public unsafe static TSource[] ToArray<TSource>(IEnumerable<TSource> source) => source == null ? throw new ArgumentNullException("source") : new Buffer<TSource>(source).ToArray();
    }

    internal struct Buffer<TElement> {
        private static readonly TElement[] empty = new TElement[4];
        internal TElement[] items;
        internal int count;

        internal Buffer(IEnumerable<TElement> source) {
            TElement[] array = null;
            int num = 0;
            ICollection<TElement> collection = source as ICollection<TElement>;
            if (collection != null) {
                num = collection.Count;
                if (num > 0) {
                    array = new TElement[num];
                    collection.CopyTo(array, 0);
                }
            } else {
                foreach (TElement telement in source) {
                    if (array == null) {
                        array = empty;
                    } else if (array.Length == num) {
                        TElement[] array2 = new TElement[checked(num * 2)];
                        Array.Copy(array, 0, array2, 0, num);
                        array = array2;
                    }
                    array[num] = telement;
                    num++;
                }
            }
            items = array;
            count = num;
        }

        internal TElement[] ToArray() => items;
    }
}
