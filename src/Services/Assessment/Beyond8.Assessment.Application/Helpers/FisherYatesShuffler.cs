namespace Beyond8.Assessment.Application.Helpers;


public static class FisherYatesShuffler
{
    public static void Shuffle<T>(IList<T> list, int seed)
    {
        var random = new Random(seed); // 1. Khởi tạo bộ sinh số ngẫu nhiên
        var n = list.Count;

        // 2. Vòng lặp từ phần tử cuối cùng về phần tử thứ 2
        for (var i = n - 1; i > 0; i--)
        {
            // 3. Chọn ngẫu nhiên một vị trí j từ 0 đến i (bao gồm cả i)
            var j = random.Next(i + 1);

            // 4. Hoán đổi vị trí của phần tử tại i và phần tử tại j
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    public static List<T> ShuffleCopy<T>(IEnumerable<T> source, int seed)
    {
        var list = source.ToList(); // Tạo bản sao
        Shuffle(list, seed);        // Xáo trộn bản sao
        return list;                // Trả về danh sách mới
    }

    public static int GenerateSeed()
    {
        return Random.Shared.Next();
    }
}
