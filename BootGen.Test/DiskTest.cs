using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BootGen.Core;

namespace BootGenTest;

[TestClass]
public class DiskTest
{
    [TestMethod]
    public void TestDisk()
    {
        var disk = new Disk("tmp");
        if (Directory.Exists("tmp/tmp2"))
            Directory.Delete("tmp/tmp2", true);
        disk.WriteText("tmp2", "test.txt", "hello");
        Assert.IsTrue(File.Exists("tmp/tmp2/test.txt"));
        disk.Delete("tmp2", "test.txt");
        Assert.IsFalse(File.Exists("tmp/tmp2/test.txt"));
    }
}