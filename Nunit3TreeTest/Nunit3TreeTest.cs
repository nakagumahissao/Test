using NUnit.Framework;
using System.Collections.Generic;
using Trees;

namespace Nunit3TreeTest
{
    public class Tests
    {
        string[] _args = Array.Empty<string>();

        [SetUp]
        public void Setup()
        {
            _args = new[] { "aaa", "bbb", "ccc", "ddd", "eee" };
        }

        [Test]
        public void MerkleTree_ShouldBuildCorrectRootHash()
        {
            // Arrange
            var dataBlocks = new List<string>(_args);

            // Act
            var tree = new MerkleTree(dataBlocks, "Bitcoin_Transaction", "Bitcoin_Transaction");
            var rootHash = tree.GetMerkleRoot();

            // Assert
            Assert.IsNotNull(rootHash, "Merkle root should not be null");
            Assert.That("4aa906745f72053498ecc74f79813370a4fe04f85e09421df2d5ef760dfa94b5" == rootHash);
        }
    }
}