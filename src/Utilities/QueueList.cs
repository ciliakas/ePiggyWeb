namespace ePiggyWeb.Utilities
{
    public class QueueList<T>
    {
        private int _nextIndex = 0;
        private T[] _arr;
        public T this[int i]
        {
            get => _arr[i];
            set => _arr[i] = value;
        }
        public QueueList(int size = 5)//default size for queue 5
        {
            _arr = new T[size];
        }

        public void Add(T value)
        {
            if (_nextIndex >= _arr.Length)
            {
                //throw new IndexOutOfRangeException($"The collection can hold only {_arr.Length} elements.");
                return;
            }
            _arr[_nextIndex++] = value;
        }

        public void SwitchItems(int indexOne, int indexTwo)
        {
            var size = _nextIndex;
            if (indexOne >= size || indexOne >= size)
            {
                //throw new IndexOutOfRangeException($"The indexes are out of boundaries.");
                return;
            }

            var temp = _arr[indexOne];
            _arr[indexOne] = _arr[indexTwo];
            _arr[indexTwo] = temp;
        }

        public void Remove(int index)
        {
            var size = _nextIndex;
            if (index >= size)
            {
                //throw new IndexOutOfRangeException($"Index out of boundaries");
                return;
            }

            _arr = _arr.RemoveAt(index);
            _nextIndex--;
        }
    }
}
