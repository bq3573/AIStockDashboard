using HtmlAgilityPack;
using System.Text;

namespace StockInfoApp.Utilities
{
    public class ArticleExtractor
    {

        public Dictionary<string, string> ArticleToBodyMapping = new Dictionary<string, string>
        {
            {"benzinga", "//div[@id='article-body']" },
            {"fool", "//div[@class='article-body']" },
        };


        public string GetArticleTarget(string url)
        {
            var target = "";
            if (url.Contains("benzinga"))
            {
                target = ArticleToBodyMapping["benzinga"];
            } else if (url.Contains("fool"))
            {
                target = ArticleToBodyMapping["fool"];
            }

            return target;  
        }

        public List<string> SplitTextIntoChunks(string articleText, int maxTokens = 4000)
        {
            List<string> chunks = new List<string>();
            string[] sentences = articleText.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            StringBuilder chunk = new StringBuilder();
            int currentTokenCount = 0;

            foreach (var sentence in sentences)
            {
                int sentenceTokens = sentence.Split(' ').Length;
                if (currentTokenCount + sentenceTokens > maxTokens)
                {
                    chunks.Add(chunk.ToString());
                    chunk.Clear();
                    currentTokenCount = 0;
                }

                chunk.Append(sentence + ". ");
                currentTokenCount += sentenceTokens;
            }

            // Add remaining chunk
            if (chunk.Length > 0)
            {
                chunks.Add(chunk.ToString());
            }

            return chunks;
        }

        public string ExtractArticleText(string url)
        {

            var target = GetArticleTarget(url);

            var web = new HtmlWeb();
            var doc = web.Load(url);

            // works with Benzinga
            var contentNode = doc.DocumentNode.SelectSingleNode(target);
            // class: 'article-body' the motley fool
            // Example: Assuming article text is within <div class="article-content"> tags


            return contentNode?.InnerText.Trim();
        }

    }

}
