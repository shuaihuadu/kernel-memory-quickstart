namespace KernelMemoryExamples;

public static class KM0003_SummarizingDocuments
{
    public static async Task RunAsync()
    {
        IKernelMemory memory = new KernelMemoryBuilder()
            .WithAzureOpenAITextGeneration(TestConfiguration.KernelMemoryAzureOpenAIConfig)
            .WithAzureOpenAITextEmbeddingGeneration(TestConfiguration.KernelMemoryAzureOpenAIEmbeddingConfig)
            .Build<MemoryServerless>();

        Console.WriteLine("Kernel Memory ready.");

        await memory.ImportDocumentAsync(Path.Join(TestConfiguration.DocumentRootPath, "NASA-news.pdf"), documentId: "doc001", steps: Constants.PipelineOnlySummary);

        Console.WriteLine("Document imported and summarized.");

        List<Citation> results = await memory.SearchSummariesAsync(filter: MemoryFilters.ByDocument("doc001"));

        foreach (Citation citation in results)
        {
            string summary = citation.Partitions.First().Text;

            Console.WriteLine($"== {citation.SourceName} summary == \n\n {summary}\n");
        }
    }
}
