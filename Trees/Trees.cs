namespace Trees
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    public enum NodePos { Left, Right }

    public class MerkleNode
    {
        public string Hash { get; set; }
        public string? Data { get; set; }
        public MerkleNode? Left { get; set; }
        public MerkleNode? Right { get; set; }
        public MerkleNode? Parent { get; set; }
        public NodePos? Position { get; set; }

        public bool IsLeaf => Left == null && Right == null;

        // Leaf node constructor
        public MerkleNode(string data, string strTagLeafValue)
        {
            Data = data;
            Hash = TaggedHash(strTagLeafValue, Encoding.UTF8.GetBytes(data));
        }

        // Internal node constructor
        public MerkleNode(MerkleNode left, MerkleNode right, string strTagBranchValue)
        {
            Left = left;
            Right = right;

            if (left != null)
            {
                left.Parent = this;
                left.Position = NodePos.Left;
            }

            if (right != null)
            {
                right.Parent = this;
                right.Position = NodePos.Right;
            }

            // Concatenate left and right hashes as bytes
            byte[] leftHashBytes = hsba(left?.Hash ?? "");
            byte[] rightHashBytes = hsba(right?.Hash ?? left?.Hash ?? "");

            byte[] combined = new byte[leftHashBytes.Length + rightHashBytes.Length];
            Buffer.BlockCopy(leftHashBytes, 0, combined, 0, leftHashBytes.Length);
            Buffer.BlockCopy(rightHashBytes, 0, combined, leftHashBytes.Length, rightHashBytes.Length);

            Hash = TaggedHash(strTagBranchValue, combined);
        }

        private static string TaggedHash(string tag, byte[] message)
        {
            using var sha256 = SHA256.Create();

            // Compute tag hash once
            byte[] tagHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(tag));

            // Prepare input = tagHash || tagHash || message
            byte[] toHash = new byte[tagHash.Length * 2 + message.Length];
            Buffer.BlockCopy(tagHash, 0, toHash, 0, tagHash.Length);
            Buffer.BlockCopy(tagHash, 0, toHash, tagHash.Length, tagHash.Length);
            Buffer.BlockCopy(message, 0, toHash, tagHash.Length * 2, message.Length);

            byte[] resultHash = sha256.ComputeHash(toHash);

            // Convert to hex string
            return BitConverter.ToString(resultHash).Replace("-", "").ToLowerInvariant();
        }

        // Converts to byte array.
        private static byte[] hsba(string hex)
        {
            if (hex.Length % 2 != 0)
                throw new ArgumentException("Invalid hexadecimal string length");

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            return bytes;
        }
    }

    public class MerkleTree
    {
        public MerkleNode? Root { get; private set; }
        public List<MerkleNode> Leaves { get; private set; } = new();

        private string tagLeafValue = "";
        private string tagBranchValue = "";

        public MerkleTree(List<string> dataBlocks, string tagLeafValue, string tagBranchValue)
        {
            this.tagLeafValue = tagLeafValue;
            this.tagBranchValue = tagBranchValue;
            BuildTree(dataBlocks);
        }

        private void BuildTree(List<string> dataBlocks)
        {
            if (dataBlocks == null || dataBlocks.Count == 0)
                throw new ArgumentException("Data block list cannot be empty.");

            // Step 1: Create leaf nodes
            var currentLevel = new List<MerkleNode>();
            foreach (var data in dataBlocks)
            {
                var leaf = new MerkleNode(data, tagLeafValue);
                currentLevel.Add(leaf);
                Leaves.Add(leaf);
            }

            if (currentLevel != null)
            {
                // Step 2: Build tree upwards
                while (currentLevel.Count > 1)
                {
                    var nextLevel = new List<MerkleNode>();

                    for (int i = 0; i < currentLevel.Count; i += 2)
                    {
                        var left = currentLevel[i];
                        MerkleNode? right = (i + 1 < currentLevel.Count) ? currentLevel[i + 1] : null;

                        // Duplicate last node if odd number of nodes
                        if (right == null)
                        {
                            right = left;
                        }

                        var parent = new MerkleNode(left, right, tagBranchValue);
                        nextLevel.Add(parent);
                    }

                    currentLevel = nextLevel;
                }

                Root = currentLevel[0];
            }
        }

        public string? GetMerkleRoot()
        {
            return Root?.Hash;
        }
    }
}
