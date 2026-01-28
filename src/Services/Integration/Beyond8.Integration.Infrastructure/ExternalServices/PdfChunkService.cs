using System.Text;
using Beyond8.Integration.Application.Dtos.AiIntegration.Embedding;
using Beyond8.Integration.Application.Services.Interfaces;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace Beyond8.Integration.Infrastructure.ExternalServices
{
    public class PdfChunkService(ILogger<PdfChunkService> logger) : IPdfChunkService
    {
        public List<DocumentChunk> ChunkPdf(
            Stream pdfStream,
            Guid courseId,
            Guid documentId,
            int chunkSize = 500,
            int chunkOverlap = 15)
        {
            try
            {
                var chunks = new List<DocumentChunk>();

                using var document = PdfDocument.Open(pdfStream);
                var pageNumber = 1;

                foreach (var page in document.GetPages())
                {
                    var pageText = ExtractTextFromPage(page);

                    if (string.IsNullOrWhiteSpace(pageText))
                    {
                        pageNumber++;
                        continue;
                    }

                    // Chunk text theo chunkSize và chunkOverlap
                    var pageChunks = ChunkText(pageText, chunkSize, chunkOverlap);

                    for (int i = 0; i < pageChunks.Count; i++)
                    {
                        chunks.Add(new DocumentChunk
                        {
                            Text = pageChunks[i],
                            CourseId = courseId,
                            DocumentId = documentId,
                            PageNumber = pageNumber,
                            ChunkIndex = i,
                            SectionTitle = null // Có thể parse từ PDF structure sau
                        });
                    }

                    pageNumber++;
                }

                if (chunks.Count == 0)
                {
                    logger.LogWarning("Không thể extract text từ PDF.");
                    return [];
                }

                logger.LogInformation(
                    "Chunked PDF into {ChunkCount} chunks across {PageCount} pages",
                    chunks.Count,
                    pageNumber - 1);

                return chunks;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error chunking PDF");
                return [];
            }
        }

        private static string ExtractTextFromPage(Page page)
        {
            var words = page.GetWords();
            if (!words.Any())
                return string.Empty;

            var textBuilder = new StringBuilder();
            Word? previousWord = null;

            foreach (var word in words)
            {
                var currentWord = word.Text;

                if (previousWord != null)
                {
                    textBuilder.Append(' ');
                }

                textBuilder.Append(currentWord);
                previousWord = word;
            }

            return textBuilder.ToString();
        }


        private static List<string> ChunkText(string text, int chunkSize, int chunkOverlap)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            var chunks = new List<string>();
            var words = text.Split([' ', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries);

            if (words.Length == 0)
                return new List<string> { text };

            // Giới hạn overlap tối đa ~25% số từ trong chunk (ước lượng chunkSize/5 ký tự ≈ chunkSize/20 từ)
            var maxOverlapWords = Math.Max(1, chunkSize / 20);
            var overlapWords = Math.Min(chunkOverlap, maxOverlapWords);

            var currentChunk = new StringBuilder();
            var currentLength = 0;
            var startIndex = 0;

            for (var i = 0; i < words.Length; i++)
            {
                var word = words[i];
                var wordLength = word.Length + 1; // +1 for space

                if (currentLength + wordLength > chunkSize && currentChunk.Length > 0)
                {
                    chunks.Add(currentChunk.ToString().Trim());

                    // Bước nhảy: chunk mới bắt đầu từ (startIndex - overlapWords) từ, tránh overlap quá lớn
                    var overlapStart = Math.Max(0, startIndex - overlapWords);
                    currentChunk.Clear();
                    currentLength = 0;

                    for (var j = overlapStart; j < i; j++)
                    {
                        currentChunk.Append(words[j]);
                        currentChunk.Append(' ');
                        currentLength += words[j].Length + 1;
                    }

                    startIndex = overlapStart;
                }

                currentChunk.Append(word);
                currentChunk.Append(' ');
                currentLength += wordLength;
            }

            if (currentChunk.Length > 0)
                chunks.Add(currentChunk.ToString().Trim());

            return chunks;
        }
    }
}
