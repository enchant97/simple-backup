using NUnit.Framework;
using SimpleBackup.Core.Paths;

namespace SimpleBackup.UnitTests
{
    public class PathCheckerTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TestCase("/home/user", "/home", ExpectedResult = true)]
        [TestCase(@"C:\Users\user\Documents", @"C:\Users\user", ExpectedResult = true)]
        [TestCase("/home/user", "/root", ExpectedResult = false)]
        [TestCase(@"C:\Users\user\Documents", @"C:\Users\another-user", ExpectedResult = false)]
        public bool TestIsPartOf(string possibleChild, string possibleParent)
        {
            return Checkers.IsPathPartOf(possibleChild, possibleParent);
        }
        [TestCase("/home/user", "/home", "/home/hello", "/root", ExpectedResult = true)]
        [TestCase("/etc/", "/home", "/home/hello", "/root", ExpectedResult = false)]
        public bool TestIsPartOfMultiple(string possibleChild, params string[] possibleParents)
        {
            return Checkers.IsPathPartOf(possibleChild, possibleParents);
        }
    }
}