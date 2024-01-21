namespace KernelMemoryExamples;

public static class KM0001_UploadAndAsk
{
    public static async Task RunAsync()
    {
        IKernelMemory memory = new KernelMemoryBuilder()
            .WithAzureOpenAITextGeneration(TestConfiguration.KernelMemoryAzureOpenAIConfig)
            .WithAzureOpenAITextEmbeddingGeneration(TestConfiguration.KernelMemoryAzureOpenAIEmbeddingConfig)
            .Build<MemoryServerless>();

        string documentFilePath = Path.Join(TestConfiguration.DocumentRootPath, "sample-SK-Readme.pdf");

        await memory.ImportDocumentAsync(documentFilePath, documentId: "example001");

        string question = "What's Semantic Kernel?";

        MemoryAnswer answer = await memory.AskAsync(question);

        Console.WriteLine($"Question: {question}");
        Console.WriteLine($"Answer:{answer.Result}");

        Console.WriteLine("Sources:\n");

        foreach (var item in answer.RelevantSources)
        {
            Console.WriteLine($"    - {item.SourceName}  - {item.Link} [{item.Partitions.First().LastUpdate:D}]");
        }
    }
}
