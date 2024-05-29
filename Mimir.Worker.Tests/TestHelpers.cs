namespace Mimir.Worker.Tests;

public static class TestHelpers
{
    public static string ReadTestData(string fileName)
    {
        var projectDirectory = Path.GetFullPath(
            Path.Combine(Directory.GetCurrentDirectory(), @"../../../")
        );
        var path = Path.Combine(projectDirectory, "TestData", fileName);
        return File.ReadAllText(path);
    }
}
