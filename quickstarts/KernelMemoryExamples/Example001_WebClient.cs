namespace KernelMemoryExamples;

public static class Example001_WebClient
{
    public static async Task RunAsync()
    {
        MemoryWebClient memoryWebClient = new("http://127.0.0.1:9001/");

        bool ingestion = true;
        bool useImages = true;
        bool retrieval = true;
        bool purge = true;

        List<string> toDelete = new();

        if (ingestion)
        {
            Console.WriteLine("\n====================================\n");

            Console.WriteLine("Uploading text about E=mc^2");

            string docId = await memoryWebClient.ImportTextAsync("In physics, mass–energy equivalence is the relationship between mass and energy " +
                                             "in a system's rest frame, where the two quantities differ only by a multiplicative " +
                                             "constant and the units of measurement. The principle is described by the physicist " +
                                             "Albert Einstein's formula: E = m*c^2");

            toDelete.Add(docId);

            toDelete.Add("doc001");

            Console.WriteLine("Uploading article file about Carbon");

            await memoryWebClient.ImportDocumentAsync(Path.Join(TestConfiguration.DocumentRootPath, "file1-Wikipedia-Carbon.txt"), documentId: "doc001");

            if (useImages)
            {
                toDelete.Add("img001");

                Console.WriteLine("Uploading Image file with a news about a conference sponsored by Microsoft");

                await memoryWebClient.ImportDocumentAsync(new Document("img001").AddFiles([Path.Join(TestConfiguration.DocumentRootPath, "file6-ANWC-image.jpg")]));
            }

            toDelete.Add("doc002");

            if (!await memoryWebClient.IsDocumentReadyAsync(documentId: "doc002"))
            {
                Console.WriteLine("Uploading a text file, a Word doc, and a PDF about Semantic Kernel");

                await memoryWebClient.ImportDocumentAsync(
                    new Document("doc002")
                    .AddFiles(
                        [Path.Join(TestConfiguration.DocumentRootPath, "file2-Wikipedia-Moon.txt"),
                            Path.Join(TestConfiguration.DocumentRootPath, "file3-lorem-ipsum.docx"),
                            Path.Join(TestConfiguration.DocumentRootPath, "file4-SK-Readme.pdf")]
                            )
                    .AddTag("user", "Blake"));
            }
            else
            {
                Console.WriteLine("doc002 already uploaded.");
            }

            toDelete.Add("doc003");

            if (!await memoryWebClient.IsDocumentReadyAsync(documentId: "doc003"))
            {
                Console.WriteLine("Uploading a PDF with a news about NASA and Orion");

                await memoryWebClient.ImportDocumentAsync(new Document("doc003")
                    .AddFile(Path.Join(TestConfiguration.DocumentRootPath, "file5-NASA-news.pdf"))
                    .AddTag("user", "Taylor")
                    .AddTag("collection", "meetings")
                    .AddTag("collection", "NASA")
                    .AddTag("collection", "space")
                    .AddTag("type", "news"));
            }
            else
            {
                Console.WriteLine("doc003 already uploaded.");
            }

            toDelete.Add("webPage1");

            if (!await memoryWebClient.IsDocumentReadyAsync("webPage1"))
            {
                Console.WriteLine("Uploading https://raw.githubusercontent.com/microsoft/kernel-memory/main/README.md");

                await memoryWebClient.ImportWebPageAsync("https://raw.githubusercontent.com/microsoft/kernel-memory/main/README.md", documentId: "webPage1");
            }
            else
            {
                Console.WriteLine("webPage1 already uploaded.");
            }

            toDelete.Add("webPage2");

            if (!await memoryWebClient.IsDocumentReadyAsync("webPage2"))
            {
                Console.WriteLine("Uploading https://raw.githubusercontent.com/microsoft/kernel-memory/main/docs/SECURITY_FILTERS.md");

                await memoryWebClient.ImportWebPageAsync(
                    "https://raw.githubusercontent.com/microsoft/kernel-memory/main/docs/SECURITY_FILTERS.md",
                    documentId: "webPage2",
                    steps: Constants.PipelineWithoutSummary);
            }
            else
            {
                Console.WriteLine("webPage2 already uploaded.");
            }
        }

        Console.WriteLine("\n====================================\n");

        foreach (string docId in toDelete)
        {
            while (!await memoryWebClient.IsDocumentReadyAsync(documentId: docId))
            {
                Console.WriteLine("Waiting for memory ingestion to complete...");

                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }

        if (retrieval)
        {
            Console.WriteLine("\n====================================\n");

            string question = "What's E = m*c^2?";
            Console.WriteLine($"Question: {question}");

            MemoryAnswer answer = await memoryWebClient.AskAsync(question);
            Console.WriteLine($"\nAnswer: {answer.Result}");

            Console.WriteLine("\n====================================\n");

            question = "What's Semantic Kernel?";
            Console.WriteLine($"Question: {question}");

            answer = await memoryWebClient.AskAsync(question);
            Console.WriteLine($"\nAnswer: {answer.Result}\n\n  Sources:\n");

            foreach (Citation citation in answer.RelevantSources)
            {
                Console.WriteLine(citation.SourceUrl != null
                    ? $"  - {citation.SourceUrl} [{citation.Partitions.First().LastUpdate:D}]"
                    : $"  - {citation.SourceName} - {citation.Link} [{citation.Partitions.First().LastUpdate:D}]");
            }

            if (useImages)
            {
                Console.WriteLine("\n====================================\n");

                question = "Which conference is Microsoft sponsoring?";
                Console.WriteLine($"Question: {question}");

                answer = await memoryWebClient.AskAsync(question);
                Console.WriteLine($"\nAnswer: {answer.Result}\n\n  Sources:\n");

                foreach (Citation citation in answer.RelevantSources)
                {
                    Console.WriteLine(citation.SourceUrl != null
                        ? $"  - {citation.SourceUrl} [{citation.Partitions.First().LastUpdate:D}]"
                        : $"  - {citation.SourceName}  -  {citation.Link} [{citation.Partitions.First().LastUpdate:D}]");
                }

            }

            Console.WriteLine("\n====================================\n");

            question = "Any news fromo NASA about Orion?";
            Console.WriteLine($"Question: {question}");

            answer = await memoryWebClient.AskAsync(question, filter: MemoryFilters.ByTag("user", "Blake"));
            Console.WriteLine($"\nBlake Answer (none expected): {answer.Result}");

            answer = await memoryWebClient.AskAsync(question, filter: MemoryFilters.ByTag("user", "Taylor"));
            Console.WriteLine($"\nTaylor Answer: {answer.Result}\n  Source:\n");

            foreach (Citation citation in answer.RelevantSources)
            {
                Console.WriteLine(citation.SourceUrl != null
                    ? $"  - {citation.SourceUrl} [{citation.Partitions.First().LastUpdate:D}]"
                    : $"  - {citation.SourceName}  -  {citation.Link} [{citation.Partitions.First().LastUpdate:D}]");
            }

            Console.WriteLine("\n====================================\n");

            question = "What is Orion?";
            Console.WriteLine($"Question: {question}");

            answer = await memoryWebClient.AskAsync(question, filter: MemoryFilters.ByTag("type", "article"));
            Console.WriteLine($"\nArticles (none expected): {answer.Result}");

            answer = await memoryWebClient.AskAsync(question, filter: MemoryFilters.ByTag("type", "news"));
            Console.WriteLine($"\nNews: {answer.Result}");
        }

        if (purge)
        {
            Console.WriteLine("====================================");

            foreach (string docId in toDelete)
            {
                Console.WriteLine($"Deleting memories derived from {docId}");

                await memoryWebClient.DeleteDocumentAsync(docId);
            }
        }
    }
}
