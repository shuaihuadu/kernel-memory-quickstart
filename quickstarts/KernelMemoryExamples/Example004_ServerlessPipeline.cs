namespace KernelMemoryExamples;

public static class Example004_ServerlessPipeline
{
    public static async Task RunAsync()
    {
        IKernelMemoryBuilder builder = new KernelMemoryBuilder()
            .WithAzureOpenAITextGeneration(TestConfiguration.KernelMemoryAzureOpenAIConfig)
            .WithAzureOpenAITextEmbeddingGeneration(TestConfiguration.KernelMemoryAzureOpenAIEmbeddingConfig);

        IKernelMemory kernelMemory = builder.Build();

        IPipelineOrchestrator orchestrator = builder.GetOrchestrator();

        Console.WriteLine("* Defining pipeline handlers...");

        TextExtractionHandler textExtraction = new("extract", orchestrator);
        await orchestrator.AddHandlerAsync(textExtraction);

        TextPartitioningHandler textPartitioning = new("partition", orchestrator);
        await orchestrator.AddHandlerAsync(textPartitioning);

        SummarizationHandler summarizeEmbedding = new("summarize", orchestrator);
        await orchestrator.AddHandlerAsync(summarizeEmbedding);

        GenerateEmbeddingsHandler textEmbedding = new("gen_embeddings", orchestrator);
        await orchestrator.AddHandlerAsync(textEmbedding);

        SaveRecordsHandler saveRecords = new("save_records", orchestrator);
        await orchestrator.AddHandlerAsync(saveRecords);

        Console.WriteLine("* Defining pipeline with 4 files...");

        DataPipeline? pipeline = orchestrator
            .PrepareNewDocumentUpload(index: "tests", documentId: "inProcessTest", new TagCollection { { "testName", "example3" } })
            .AddUploadFile("file1", "file1-Wikipedia-Carbon.txt", Path.Join(TestConfiguration.DocumentRootPath, "file1-Wikipedia-Carbon.txt"))
            .AddUploadFile("file2", "file2-Wikipedia-Moon.txt", Path.Join(TestConfiguration.DocumentRootPath, "file2-Wikipedia-Moon.txt"))
            .AddUploadFile("file3", "file3-lorem-ipsum.docx", Path.Join(TestConfiguration.DocumentRootPath, "file3-lorem-ipsum.docx"))
            .AddUploadFile("file4", "file4-SK-Readme.pdf", Path.Join(TestConfiguration.DocumentRootPath, "file4-SK-Readme.pdf"))
            .AddUploadFile("file5", "file5-NASA-news.pdf", Path.Join(TestConfiguration.DocumentRootPath, "file5-NASA-news.pdf"))
            .Then("extract")
            .Then("partition")
            .Then("summarize")
            .Then("gen_embeddings")
            .Then("save_records")
            .Build();

        Console.WriteLine("* Executing pipeline...");

        await orchestrator.RunPipelineAsync(pipeline);

        Console.WriteLine("* File import completed.");
        Console.WriteLine("Refactoring in progress");
    }
}
